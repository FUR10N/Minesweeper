using MinesweeperSolver.Combinatorics;
using MinesweeperSolver.Solver.Algos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MinesweeperSolver.Solver
{
	public delegate void OnProgressUpdate(Board sender, ProgressArgs args);
	public delegate void OnFinished(Board sender);

	public class Board
	{
		public event OnProgressUpdate ProgressUpdate;
		public event OnFinished Finished;
		public bool AutoGuess { get; set; }

		public List<Block> nonZeroValues;

		public Block[,] Grid;

		public Statistics Stats = new Statistics();

		private IScreenParser parser;
		private List<Block> tempGuesses = new List<Block>();

		private List<IAlgorithm> algorithms = new List<IAlgorithm>();
		private Guesser guesser = new Guesser();
		private BackgroundWorker worker;

		public Board(IScreenParser parser, int width = 30, int height = 16)
		{
			this.parser = parser;
			algorithms.Add(new SimpleSolver());
			algorithms.Add(new MacroSolver());

			Grid = new Block[width, height];
			
		}

		public int Width { get { return Grid.GetLength(0); } }

		public int Height { get { return Grid.GetLength(1); } }

		public void StartSolver()
		{
			worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.ProgressChanged += (s, e) =>
			{
				if (ProgressUpdate != null)
					ProgressUpdate(this, new ProgressArgs((string)e.UserState));
			};
			worker.RunWorkerCompleted += (s, e) =>
			{
				if (Finished != null)
					Finished(this);
			};
			worker.DoWork += makeMoves;


			// Is Initial
			try
			{
				if (parser.ParseScreen(this, out this.nonZeroValues))
				{
					// Start in middle of screen
					new Movement(new Block(Width / 2, Height / 2, BlockState.Unknown), MoveTypes.DoubleClick).Execute(parser);
					Thread.Sleep(500);
				}
			}
			catch (ParserException ex2)
			{
				if (ProgressUpdate != null)
					ProgressUpdate(this, new ProgressArgs(ex2.Message));
				return;
			}

			worker.RunWorkerAsync();
		}

		private void makeMoves(object sender, DoWorkEventArgs args)
		{
			bool hasMoves = true;
			bool parseFailed = false;
			Stopwatch stopwatch = new Stopwatch();
			while (hasMoves)
			{
				stopwatch.Start();

				clearAllGuesses();

				parseFailed = !parseScreenAfterAnimation();
				if (parseFailed)
				{
					passComplete((BackgroundWorker)sender, stopwatch);
					break;
				}

				hasMoves = false;
				foreach (IAlgorithm alg in algorithms)
				{
					var moves = alg.FindMoves(this);
					if (moves.Any())
					{
						foreach (var move in moves)
						{
							hasMoves = true;
							move.Execute(parser);
							Stats.AddMove(alg.Name);
						}
						// After a move was found, restart the entire process.
						// Re-parsing the screen and then doing the faster algorithms is the best step
						break;
					}
				}

				// Must make a guess at this point
				if (!hasMoves)
				{
					var guesses = guesser.FindMoves(this);
					if (guesses.Any())
					{
						if (AutoGuess)
						{
							hasMoves = true;
							guesses.First().Execute(parser);
						}
						else
						{
							guesses.First().MoveMouseToBlock(parser);
						}

						Stats.AddMove(guesser.Name);
						worker.ReportProgress(0, "Make Guess: " + guesses.First().ToString());
					}
				}

				passComplete((BackgroundWorker)sender, stopwatch);
			}
		}

		private void passComplete(BackgroundWorker worker, Stopwatch stopwatch)
		{
			Stats.Passes++;
			worker.ReportProgress(0, String.Format("{0}: {1} ms", Stats.Passes.ToString("D2"), stopwatch.ElapsedMilliseconds));
			Stats.TotalMilliseconds += stopwatch.ElapsedMilliseconds;

			stopwatch.Stop();
			stopwatch.Reset();
		}

		/// <summary>
		/// Runs the screen parser. If it fails, then assume the animation didn't finish yet, so try again.
		/// </summary>
		private bool parseScreenAfterAnimation()
		{
			try
			{
				Thread.Sleep(200);
				parser.ParseScreen(this, out this.nonZeroValues);
			}
			catch (ParserException ex)
			{
				try
				{
					Thread.Sleep(150);
					parser.ParseScreen(this, out this.nonZeroValues);
				}
				catch (ParserException ex2)
				{
					if (worker != null)
						worker.ReportProgress(0, ex2.Message);
					//MessageBox.Show(ex2.Message);
					return false;
				}
			}
			return true;
		}

		internal List<Block> getUnknownBlocks(out int flagsLeft)
		{
			int flags = 99;
			List<Block> unknown = new List<Block>();
			for (int i = 0; i < 30; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if (Grid[i, j].State == BlockState.Flag)
						flags--;
					else if (Grid[i, j].State == BlockState.Unknown)
						unknown.Add(Grid[i, j]);
				}
			}
			flagsLeft = flags;
			return unknown;
		}

		private bool isBoardSolved()
		{
			for (int i = 0; i < Grid.GetLength(0); i++)
			{
				for (int j = 0; j < Grid.GetLength(1); j++)
				{
					if (Grid[i, j].State != BlockState.Value)
						continue;
					var neighbors = new NeighborList(Grid, Grid[i, j]);
					int flagCount = neighbors.GetFlagCount();
					var unknown = neighbors.GetUnknownBlocks().ToList();
					if (Grid[i,j].Value - flagCount < unknown.Count)
						return false;
				}
			}
			return true;
		}

		internal bool isBlockSolved(Block block, NeighborList neighbors, bool useGuesses = false)
		{
			if (block.State != BlockState.Value)
				return false;

			if (neighbors == null)
				new NeighborList(Grid, block);

			return neighbors.GetFlagCount(useGuesses) == block.Value;
		}

		internal IEnumerable<IList<Block>> getValidCombinations(IList<Block> set, int chooseSize)
		{
			// Get all flag combinations around current block (n choose k)
			// Where n is the number of unknown blocks, and k is the number of flags left
			var flagCombinations = new Combinations<Block>(set, chooseSize);
			
			// Remove any combinations that break the game board
			return flagCombinations.Where(comb =>
			{
				for (int i = 0; i < comb.Count; i++) comb[i].SetGuess(GuessState.Flag, tempGuesses);
				bool valid = comb.All(c => isValidBlock(c));
				clearAllGuesses();
				return valid;
			});
		}

		internal IEnumerable<Movement> getCombinationMoves(IList<Block> set, int chooseSize)
		{
			var validFlagCombinations = getValidCombinations(set, chooseSize);

			if (!validFlagCombinations.Any())
				throw new InvalidBoardException("No valid flag combinations found. The board is invalid.");
			// Get any flags that present in all of the guess. These are guaranteed to be flags
			var validFlags = validFlagCombinations.Aggregate((l1, l2) => l1.Intersect(l2).ToList()).ToList();
			foreach (var flag in validFlags)
				yield return new Movement(flag, MoveTypes.SetFlag);

			clearAllGuesses();

			// Now get all values that are set to be cleared in each valid combination
			// These are the values should be cleared regardless of which combination is the correct one
			List<IEnumerable<Block>> clearCombinations = new List<IEnumerable<Block>>();
			foreach (var flagComb in validFlagCombinations)
			{
				foreach (var flag in flagComb)
				{
					flag.SetGuess(GuessState.Flag, tempGuesses);
				}
				List<Block> allClearBlocks = new List<Block>();
				foreach (var flag in flagComb)
				{
					var temp = new List<Block>();
					isValidBlock(flag, allClearBlocks);

				}
				clearCombinations.Add(allClearBlocks);
				clearAllGuesses();
			}

			// Get the clear blocks that exist in each flag combination
			var validClearBlocks = clearCombinations.Aggregate((l1, l2) => l1.Intersect(l2).ToList()).ToList();
			foreach (var clear in validClearBlocks)
				yield return new Movement(clear, MoveTypes.SetClear);

			clearAllGuesses();
		}

		/// <summary>
		/// Gets a list of blocks that can be cleared if the given guess is performed.
		/// </summary>
		/// <param name="guess"></param>
		/// <returns></returns>
		private IEnumerable<Block> clearBlocksAroundGuess(Block guess)
		{
			List<Block> ret = new List<Block>();
			var adjacentToGuess = new NeighborList(Grid, guess);
			var allBlocksToCheck = new List<Block>();

			// Loop through each block around the guess
			foreach (var adj in adjacentToGuess)
			{
				allBlocksToCheck.AddRange(new NeighborList(Grid, adj));
			}

			// Loop through each neighbor of an adjacent block to the guess
			// Basically, search each block around the guess, and get blocks that can cleared around the blocks next to guess
			foreach (var t in allBlocksToCheck.Distinct())
			{
				if (t.State != BlockState.Value)
					continue;

				var neighbors = new NeighborList(Grid, t);
				int flags = neighbors.Count(n => n.State == BlockState.Flag || n.Guess == GuessState.Flag);
				int unknown = neighbors.Count(n => n.State == BlockState.Unknown);

				if (flags == t.Value && unknown != 0)
				{
					foreach (var clear in neighbors)
						if (clear.State == BlockState.Unknown && clear.Guess == GuessState.None)
							ret.Add(clear);
				}
			}

			return ret.Distinct();
		}

		/// <summary>
		/// Clears any guess that have been made on the board since the last call to this method.
		/// </summary>
		private void clearAllGuesses(GuessState targetState = GuessState.Flag | GuessState.Value)
		{
			List<Block> temp = new List<Block>();
			for (int i = 0; i < tempGuesses.Count; i++)
			{
				if ((targetState & tempGuesses[i].Guess) == tempGuesses[i].Guess)
				{
					tempGuesses[i].SetGuess(GuessState.None, tempGuesses);
					//tempGuesses.RemoveAt(i--);
				}
				else
					temp.Add(tempGuesses[i]);
			}
			tempGuesses = temp;
		}

		/// <summary>
		/// Checks if the block and all neighboring blocks are in a valid state given
		/// the current board state plus any guesses that have been made this round.
		/// <para>The idea is the come up with some guess (of where a flag or value is). Whoever calls this will make the guess (using SetGuess).</para>
		/// <para>After making the guess, this function will check the game board around that block to see if it is in a valid state.</para>
		/// <para>If it is not in a valid state, then we can assume that guess is incorrect and the caller can gain some information about a potential move.</para>
		/// </summary>
		/// <param name="block">The block to check. All 2-order neighbors around the block will be also checked.</param>
		/// <param name="potentialvalues">When checking valid blocks, this algorithm marks necessary neighbors as values 
		/// (if a block was solved by the guess)
		/// <para>Each block that was marked as a value will be added to this parameter.</para>
		/// </param>
		/// <returns>True if the block is valid.</returns>
		internal bool isValidBlock(Block block, List<Block> potentialvalues = null)
		{
			var n = new NeighborList(Grid, block);

			// If the given block is solved, then mark all unknown neighbors as values (using SetGuess)
			if (isBlockSolved(block, n, true))
			{
				for (int i = 0; i < n.Count; i++)
				{
					if (n[i].State == BlockState.Unknown && n[i].Guess != GuessState.Flag)
					{
						if (potentialvalues != null)
							potentialvalues.Add(n[i]);
						n[i].SetGuess(GuessState.Value, tempGuesses);
					}
				}
			}
			// Now loop through each neighbor of the block, and perform what was done above.
			for (int i = 0; i < n.Count; i++)
			{
				var test = new NeighborList(Grid, n[i]);
				if (isBlockSolved(n[i], test, true))
				{
					for (int j = 0; j < test.Count; j++)
					{
						if (test[j].State == BlockState.Unknown && test[j].Guess != GuessState.Flag)
						{
							if (potentialvalues != null)
								potentialvalues.Add(test[j]);
							test[j].SetGuess(GuessState.Value, tempGuesses);
						}
					}
					if (!isValidGuess(n[i], test))
						return false;
				}
			}

			return isValidGuess(block, n) && new NeighborList(Grid,block,2).All(b=>isValidGuess(b));
		}

		/// <summary>
		/// If the given block has a value, then this checks if the neighboring flags/potential flags 
		/// represents a valid block (ie if block is a 2 and there are 3 flags around it, then it is invalid).
		/// <para>If the given block does not have value, then assume it is a valid block.</para>
		/// </summary>
		/// <param name="block"></param>
		/// <param name="neighbors"></param>
		/// <returns></returns>
		private bool isValidGuess(Block block, NeighborList neighbors = null)
		{
			if (block.State == BlockState.Unknown || block.State == BlockState.Flag)
				return true;

			if (neighbors == null)
				neighbors = new NeighborList(Grid, block);

			int flags = neighbors.GetFlagCount(true);
			int unknown = neighbors.Count(n => n.State == BlockState.Unknown && n.Guess == GuessState.None);
			if (block.Value < flags || block.Value - flags > unknown)
				return false;
			return true;
		}

		public string PrettyPrint()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 30; j++)
				{
					if (Grid[j,i] == null)
						sb.Append("  ");
					else
						sb.Append(Grid[j,i].ToString() + " ");
				}
				sb.Append("\n");
			}
			
			return sb.ToString();
		}
	}
}
