using MinesweeperSolver.Combinatorics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver.Algos
{
	public class MacroSolver : IAlgorithm
	{
		private List<Block> tempGuesses = new List<Block>();

		public string Name
		{
			get { return "MacroSolver"; }
		}

		public IEnumerable<Movement> FindMoves(Board board)
		{
			clearAllGuesses();

			int flagsLeft = 99;
			List<Block> unknown = board.getUnknownBlocks(out flagsLeft);

			if (flagsLeft > 8)
				yield break;

			List<Block> unsolvedBlocks = new List<Block>();
			foreach (var un in unknown)
				unsolvedBlocks.AddRange(new NeighborList(board.Grid, un).Where(i => i.State == BlockState.Value));
			unsolvedBlocks = unsolvedBlocks.Distinct().ToList();

			var flagCombinations = new Combinations<Block>(unknown, flagsLeft);

			// Remove any combinations that locally break the game board
			var validFlagCombinations = flagCombinations.Where(comb =>
			{
				foreach (var item in comb) item.SetGuess(GuessState.Flag, tempGuesses);
				bool valid = comb.All(c => board.isValidBlock(c));
				clearAllGuesses();
				return valid;
			}).ToList();

			// Remove any combinations taht globally break the game board
			for (int i = 0; i < validFlagCombinations.Count; i++)
			{
				clearAllGuesses();
				var comb = validFlagCombinations[i];
				foreach (var guess in comb)
					guess.SetGuess(GuessState.Flag, tempGuesses);

				if (!isValidMacroGuess(board, unsolvedBlocks, comb))
					validFlagCombinations.RemoveAt(i--);
			}

			clearAllGuesses();

			// Get any flags that present in all of the guess. These are guaranteed to be flags
			var validFlags = validFlagCombinations.Aggregate((l1, l2) => l1.Intersect(l2).ToList()).ToList();

			foreach (var flag in validFlags)
				yield return new Movement(flag, MoveTypes.SetFlag);

			foreach (var un in unknown)
			{
				if (validFlagCombinations.All(c => !c.Contains(un)))
					yield return new Movement(un, MoveTypes.SetClear);
			}

			clearAllGuesses();
		}

		private bool isValidMacroGuess(Board board, IEnumerable<Block> unsolvedBlocks, IList<Block> comb)
		{
			foreach (Block b in unsolvedBlocks)
			{
				var neighbors = new NeighborList(board.Grid, b);
				int flags = neighbors.Count(i => i.State == BlockState.Flag || i.Guess == GuessState.Flag);
				if (flags != b.Value)
					return false;
			}
			return true;
		}


		private void clearAllGuesses(GuessState targetState = GuessState.Flag | GuessState.Value)
		{
			for (int i = 0; i < tempGuesses.Count; i++)
			{
				if ((targetState & tempGuesses[i].Guess) == tempGuesses[i].Guess)
				{
					tempGuesses[i].SetGuess(GuessState.None, tempGuesses);
					tempGuesses.RemoveAt(i--);
				}
			}
		}
	}
}
