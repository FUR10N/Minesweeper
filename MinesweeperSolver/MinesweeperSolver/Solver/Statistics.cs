using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver
{
	public class Statistics
	{
		public Dictionary<string, int> movesByAlgorithm = new Dictionary<string, int>();
		public int Passes;
		public long TotalMilliseconds;
		public int AveragePassTime
		{
			get { return (int)(TotalMilliseconds / (double)Passes); }
		}

		public void AddMove(string alg)
		{
			int count;
			movesByAlgorithm.TryGetValue(alg, out count);
			
			movesByAlgorithm[alg] = count + 1;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("-----------\n");
			foreach (var alg in movesByAlgorithm)
				sb.Append(alg.Key + ": \t\t" + alg.Value + "\n");
			sb.Append("-----------\n");

			sb.Append("Passes: \t\t\t" + Passes + "\n");
			sb.Append("Total Pass Time: \t\t" + TotalMilliseconds + "\n");
			sb.Append("Ave Pass Time: \t\t" +  AveragePassTime);
			return sb.ToString();
		}
	}
}
