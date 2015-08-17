using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver.Algos
{
	public class SimpleSolver : IAlgorithm
	{
		public string Name
		{
			get { return "SimpleSolver"; }
		}

		/// <summary>
		/// Guesses that are made in a particular round. When round guesses are set, it essentially overrides the actual value of the block
		/// (and doesn't appear as a guess to the different solver algorithms).
		/// </summary>
		private List<Block> roundGuesses = new List<Block>();

		/// <summary>
		/// This is the fastest step and is done at the start of ever pass.
		/// <para></para>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Movement> FindMoves(Board board)
		{
			var movesLock0 = getMovesLock0(board);
			if (movesLock0.Any())
			{
				return movesLock0.Distinct();
			}

			Movement? move = getMoveLock1(board);
			if (move == null)
				return Enumerable.Empty<Movement>();
			return new List<Movement>() {move.GetValueOrDefault() };
		}

		/// <summary>
		/// This is the fastest step and is done at the start of ever pass.
		/// <para></para>
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Movement> getMovesLock0(Board board)
		{
			foreach (Block current in board.nonZeroValues)
			{
				var neighbors = new NeighborList(board.Grid, current);

				var unknown = neighbors.GetUnknownBlocks().ToList();
				if (unknown.Count == 0) // Already solved
					continue;

				int flagCount = neighbors.GetFlagCount();

				// Check if all neighboring mines have been found
				if (flagCount == current.Value && unknown.Count != 0)
				{
					yield return new Movement(current, MoveTypes.DoubleClick);
				}
				// Check if rest of unknown neighbors should be flags
				else if (current.Value - flagCount == unknown.Count)
				{
					foreach (var n in neighbors)
					{
						if (n.State == BlockState.Unknown)
						{
							yield return new Movement(n, MoveTypes.SetFlag);
						}
					}
				}
				else if (unknown.Count != 0 && current.Value - flagCount > 0)
				{
					var combinationMoves = board.getCombinationMoves(unknown, current.Value - flagCount);
					foreach (var c in combinationMoves)
						yield return c;
				}
			}
		}


		/// <summary>
		/// This algorithm will loop through each unsolved value on the board.
		/// It will then lock one flag on the board, and check if the board is in a valid state.
		/// If the board is invalid, then that block should be cleared, and that move will be returned.
		/// </summary>
		/// <returns></returns>
		private Movement? getMoveLock1(Board board)
		{
			for (int i = 0; i < board.Width; i++)
			{
				for (int j = 0; j < board.Height; j++)
				{
					Block current = board.Grid[i, j];
					if (current.State != BlockState.Value || current.Value == 0)
						continue;

					var neighbors = new NeighborList(board.Grid, current);
					var unknown = neighbors.Where(n => n.State == BlockState.Unknown).ToList();

					var valid = board.getValidCombinations(unknown, 2);
					//clearAllGuesses();

					foreach (var comb in valid)
					{
						foreach (var flag in comb)
						{
							// Loop through each potential flag in a combiantion.
							// If setting that block as a flag breaks the board, then it must not be a flag
							flag.SetRoundGuess(BlockState.Flag, roundGuesses);

							bool guessFailed = false;
							try
							{
								getMovesLock0(board).ToList();
							}
							catch (InvalidBoardException ex)
							{
								guessFailed = true;
							}

							clearAllRoundGuesses();

							if (guessFailed)
							{
								return new Movement(flag, MoveTypes.SetClear);
							}
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Clears any (round) guess that have been made on the board since the last call to this method.
		/// </summary>
		private void clearAllRoundGuesses(BlockState targetState = BlockState.Flag | BlockState.Value)
		{
			for (int i = 0; i < roundGuesses.Count; i++)
			{
				if ((targetState & roundGuesses[i].RoundGuess) == roundGuesses[i].RoundGuess)
				{
					roundGuesses[i].SetRoundGuess(BlockState.Unknown, roundGuesses);
					roundGuesses.RemoveAt(i--);
				}
			}
		}
	}
}
