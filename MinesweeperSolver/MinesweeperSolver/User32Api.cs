using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MinesweeperSolver
{
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public static implicit operator Point(POINT point)
		{
			return new Point(point.X, point.Y);
		}
	}

	public static class User32Api
	{

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
		private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
		private const uint MOUSEEVENTF_LEFTUP = 0x04;
		private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const uint MOUSEEVENTF_RIGHTUP = 0x10;

		[DllImport("User32.Dll")]
		public static extern long SetCursorPos(int x, int y);

		[DllImport("User32.Dll")]
		public static extern bool GetCursorPos(out POINT p);


		public static void MouseClick(POINT point)
		{
			mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)point.X, (uint)point.Y, 0, new UIntPtr());
		}

		public static void MouseRightClick(POINT point)
		{
			mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)point.X, (uint)point.Y, 0, new UIntPtr());
		}

		public static void MouseDoubleClick(POINT point)
		{
			MouseClick(point);
			Thread.Sleep(75);
			MouseClick(point);
		}
	}
}
