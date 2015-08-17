using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver
{
	public class NeighborList : List<Block>
	{
		public string PrettyPrint;

		private NeighborList() : base() { }

		public NeighborList(Block[,] Grid, Block target, int order = 1)
		{
			for (int i = target.Y - order; i <= target.Y + order; i++)
			{
				if (i < 0 || i > Grid.GetLength(1) - 1)
					continue;
				for (int j = target.X - order; j <= target.X + order; j++)
				{
					if (j < 0 || j > Grid.GetLength(0) - 1)
						continue;
					//this.PrettyPrint2 += Grid[j,i] + " ";
					if (j != target.X || i != target.Y)
						this.Add(Grid[j, i]);
				}
				//this.PrettyPrint2 += "\n";
			}
		}

		public NeighborList(Block[,] grid, IEnumerable<Block> connectedArea)
		{
			Dictionary<MultiKey, Block> unique = new Dictionary<MultiKey, Block>();
			foreach (Block b in connectedArea)
			{
				var neighbors = new NeighborList(grid, b);
				foreach (Block b2 in neighbors)
					unique[new MultiKey(b2.X, b2.Y)] = b2;
			}
			foreach (Block b in connectedArea)
				unique.Remove(new MultiKey(b.X, b.Y));

			foreach (Block b in unique.Values)
				this.Add(b);
		}

		public int GetFlagCount(bool useGuesses = false)
		{
			return this.Count(i => i.State == BlockState.Flag || (useGuesses && i.Guess == GuessState.Flag));
		}

		public IEnumerable<Block> GetUnknownBlocks(bool useGuesses = false)
		{
			if (useGuesses)
				return this.Where(n => n.State == BlockState.Unknown && n.Guess == GuessState.None);
			return this.Where(n => n.State == BlockState.Unknown);
		}
	}

	public struct MultiKey
	{
		readonly int X;
		readonly int Y;

		public MultiKey(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}
}
