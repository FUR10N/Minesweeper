using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver
{
	public interface IScreenParser
	{
		bool ParseScreen(Board board, out List<Block> nonZeroValues);

		int GetXCoord(int xIndex);

		int GetYCoord(int yIndex);
	}
}
