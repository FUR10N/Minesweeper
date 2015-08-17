using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver.Algos
{
	public interface IAlgorithm
	{
		string Name { get; }

		IEnumerable<Movement> FindMoves(Board board);
	}
}
