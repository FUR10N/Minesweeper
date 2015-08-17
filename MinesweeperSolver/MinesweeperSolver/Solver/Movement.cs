using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver
{
	public enum MoveTypes
	{
		SetFlag, DoubleClick, SetClear, Guess
	}

	public struct Movement
	{
		public Block Target;
		public MoveTypes Move;
		public float Probability;
		private bool isGuess;

		public Movement(Block target, MoveTypes move, float probability)
		{
			this.Target = target;
			this.Move = move;
			this.Probability = probability;
			this.isGuess = true;
		}

		public Movement(Block target, MoveTypes move)
		{
			this.Target = target;
			this.Move = move;
			this.isGuess = false;
			this.Probability = 1.0f;
		}

		public void MoveMouseToBlock(IScreenParser parser)
		{
			POINT targetPoint = new POINT(parser.GetXCoord(Target.X) + 25, parser.GetYCoord(Target.Y) + 25);
			User32Api.SetCursorPos(targetPoint.X, targetPoint.Y);
		}

		public void Execute(IScreenParser parser)
		{
			POINT targetPoint = new POINT(parser.GetXCoord(Target.X) + 25, parser.GetYCoord(Target.Y) + 25);
			User32Api.SetCursorPos(targetPoint.X, targetPoint.Y);

			switch (Move)
			{
				case MoveTypes.DoubleClick:
					User32Api.MouseDoubleClick(targetPoint);
					break;
				case MoveTypes.SetFlag:
					if (Target.State == BlockState.Flag)
						return;
					else if (Target.UserGuess)
						User32Api.MouseRightClick(targetPoint);
					User32Api.MouseRightClick(targetPoint);
					break;
				case MoveTypes.SetClear:
					User32Api.MouseClick(targetPoint);
					break;
			}
		}

		public override string ToString()
		{
			if (isGuess)
				return String.Format("{0}: {1} [{2}, {3}]: %{4} Chance", Move.ToString(), Target.ToString(), Target.X, Target.Y, (int)(Probability*100f));
			return String.Format("{0}: {1} [{2}, {3}])", Move.ToString(), Target.ToString(), Target.X, Target.Y);
		}
	}
}
