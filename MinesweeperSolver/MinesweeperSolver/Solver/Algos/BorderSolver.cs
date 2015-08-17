using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlagCombination = System.Collections.Generic.IEnumerable<System.Collections.Generic.IList<MinesweeperSolver.Solver.Block>>;

namespace MinesweeperSolver.Solver.Algos
{
	public class BorderSolver : IAlgorithm
	{
		public string Name
		{
			get { return "BorderSolver"; }
		}

		public IEnumerable<Movement> FindMoves(Board board)
		{
			List<Tuple<Block, FlagCombination>> allValidCombinations = new List<Tuple<Block, FlagCombination>>();
			for (int i = 0; i < board.Width; i++)
			{
				for (int j = 0; j < board.Height; j++)
				{
					Block current = board.Grid[i, j];
					if (current.State != BlockState.Value || current.Value == 0)
						continue;
					
					var neighbors = new NeighborList(board.Grid, current);
					if (board.isBlockSolved(current, neighbors))
						continue;

					int flagCount = neighbors.GetFlagCount();
					var unknown = neighbors.GetUnknownBlocks().ToList();

					allValidCombinations.Add(new Tuple<Block, FlagCombination>(current, 
						board.getValidCombinations(unknown, current.Value - flagCount)));
				}
			}

			yield break;
		}
	}
}
