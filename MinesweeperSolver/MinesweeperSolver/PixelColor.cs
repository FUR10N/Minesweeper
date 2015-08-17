using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
	[StructLayout(LayoutKind.Sequential)]
	public struct PixelColor
	{
		public byte Blue;
		public byte Green;
		public byte Red;
		public byte Alpha;

		public PixelColor(byte red, byte green, byte blue)
		{
			this.Blue = blue;
			this.Green = green;
			this.Red = red;
			this.Alpha = 255;
		}

		public static PixelColor operator -(PixelColor p1, PixelColor p2)
		{
			return new PixelColor((byte)Math.Abs(p1.Red - p2.Red), (byte)Math.Abs(p1.Green - p2.Green), (byte)Math.Abs(p1.Blue - p2.Blue));
		}

		public override string ToString()
		{
			return String.Format("R: {0}, G: {1}, B: {2}, A: {3}", Red, Green, Blue, Alpha);
		}
	}
}
