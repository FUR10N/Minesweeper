using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Combinatorics
{
	/// <summary>
	/// Indicates whether a Permutation, Combination or Variation meta-collections
	/// generate repetition sets.  
	/// </summary>
	public enum GenerateOption
	{
		/// <summary>
		/// Do not generate additional sets, typical implementation.
		/// </summary>
		WithoutRepetition,
		/// <summary>
		/// Generate additional sets even if repetition is required.
		/// </summary>
		WithRepetition
	}
}
