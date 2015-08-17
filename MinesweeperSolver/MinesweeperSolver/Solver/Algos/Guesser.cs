using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver.Algos
{
	public class Guesser : IAlgorithm
	{
		public Guesser()
		{
		}

		public IEnumerable<Movement> FindMoves(Board board)
		{
			int flagsLeft = 0;
			List<Block> allUnknown = board.getUnknownBlocks(out flagsLeft);
			float overallFlagProb = (float)flagsLeft / allUnknown.Count;

			Block bestClearChance = null;
			float bestClearProbability = 0f;
			Block bestFlag = null;
			float bestFlagProbability = 0f;
			//Dictionary<Block, float> probs = new Dictionary<Block, float>();

			for (int i = 0; i < board.Width; i++)
			{
				for (int j = 0; j < board.Height; j++)
				{
					Block current = board.Grid[i, j];
					if (current.State != BlockState.Value || current.Value == 0)
						continue;

					var neighbors = new NeighborList(board.Grid, current);

					int flagCount = neighbors.GetFlagCount();
					var unknown = neighbors.GetUnknownBlocks().ToList();

					var combinations = board.getValidCombinations(unknown, current.Value - flagCount).ToList();
					foreach (var un in unknown)
					{
						int count = combinations.Count(c => c.Any(b => b == un));
						float flagProb = (float)count / combinations.Count;
						float clearProb = 1 - flagProb;

						//float existing;
						//if (probs.TryGetValue(un, out existing))
						//{
						//	if (flagProb < existing)
						//		probs[un] = flagProb;
						//}
						//else
						//{
						//	probs.Add(un, flagProb);
						//}

						if (clearProb > bestClearProbability)
						{
							bestClearChance = un;
							bestClearProbability = clearProb;
						}
						if (flagProb > bestFlagProbability)
						{
							bestFlag = un;
							bestFlagProbability = flagProb;
						}
					}
				}
			}

			float overallClearProb = 1 - overallFlagProb;
			float max = getHeighestProb(overallClearProb, overallFlagProb, bestFlagProbability, bestClearProbability);

			if (overallClearProb == max || overallFlagProb == max)
			{
				MoveTypes moveType = overallClearProb == max ? MoveTypes.SetClear : MoveTypes.SetFlag;
				Block random = getBestRandom(board, allUnknown);
				if (random != null)
					yield return new Movement(random, moveType, overallClearProb == max ? overallClearProb : overallFlagProb);
			}
			else if (bestClearProbability == max)
			{
				yield return new Movement(bestClearChance, MoveTypes.SetClear, bestClearProbability);
			}
			else
			{
				yield return new Movement(bestFlag, MoveTypes.SetFlag, bestFlagProbability);
			}
			yield break;
		}

		private float getHeighestProb(params float[] probs)
		{
			return probs.Max();
		}

		private Block getBestRandom(Board board, List<Block> allUnknown)
		{
			Block onlyUknownNeighbors = null;

			foreach (var un in allUnknown)
			{
				var neighbors = new NeighborList(board.Grid, un);
				if (neighbors.All(i => i.State == BlockState.Unknown))
				{
					if (onlyUknownNeighbors == null)
						onlyUknownNeighbors = un;
				}
				// Sugest it if no neighbars are values and it is not surrounded by flags
				else if (!neighbors.Any(i => i.State == BlockState.Value) && !neighbors.All(i=>i.State == BlockState.Flag))
				{
					// best choice because if not a flag, then it can open up non-guesses
					return un;
				}
			}

			return onlyUknownNeighbors;
		}

		public string Name
		{
			get { return "Guesser"; }
		}
	}
}
