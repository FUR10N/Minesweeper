using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver.Solver
{
	public class Block
	{
		private BlockState _state;
		public BlockState State
		{
			get
			{
				if (RoundGuess != BlockState.Unknown)
					return RoundGuess;
				return _state;
			}
		}

		public BlockState RoundGuess { get; private set; }
		public GuessState Guess { get; private set; }
		public int Value;
		public bool UserGuess { get; private set; }

		public int X;
		public int Y;

		public Block(int x, int y, BlockState state, bool userGuess = false)
		{
			this.X = x;
			this.Y = y;
			this._state = state;
			this.Value = 0;
			this.Guess = GuessState.None;
			this.RoundGuess = BlockState.Unknown;
			this.UserGuess = userGuess;
		}

		public Block(int x, int y, int value)
		{
			this.X = x;
			this.Y = y;
			this._state = BlockState.Value;
			this.Value = value;
			this.Guess = GuessState.None;
			this.RoundGuess = BlockState.Unknown;
		}

		/// <summary>
		/// Keep track of all guesses
		/// </summary>
		/// <param name="guess"></param>
		/// <param name="currentGuesses"></param>
		public void SetGuess(GuessState guess, List<Block> currentGuesses)
		{
			this.Guess = guess;
			if (guess != GuessState.None)
				currentGuesses.Add(this);
		}

		public void SetRoundGuess(BlockState guess, List<Block> roundGuesses)
		{
			this.RoundGuess = guess;
			if (guess != BlockState.Unknown)
				roundGuesses.Add(this);
		}

		public override string ToString()
		{
			switch (Guess)
			{
				case GuessState.Flag:
					return "?";
				case GuessState.Value:
					return "~";
			}
			if (UserGuess)
				return "?";
			switch (State)
			{
				case BlockState.Flag:
					return "F";
				case BlockState.ParseFailed:
					return "X";
				case BlockState.Unknown:
					return "U";
				default:
					return Value.ToString();
			}
		}
	}

	[Flags]
	public enum GuessState
	{
		None = 0x0, 
		Flag = 0x1, 
		Value = 0x2
	}

	public enum BlockState
	{
		Unknown, Flag, Value, ParseFailed
	}
}
