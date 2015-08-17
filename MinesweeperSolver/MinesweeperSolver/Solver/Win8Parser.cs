using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MinesweeperSolver.Solver
{
	public class Win8Parser : IScreenParser
	{
		private int xoffset = 155;//155
		private int yoffset = 168;//168
		private const int screenWidth = 1920;
		private const int screenHeight = 1200;
		public const int blockWidth = 54;
		public const int blockHeight = 54;

		private BlockParser blockParser;

		private DxScreenCapture dx;
		public Win8Parser()
		{
			this.blockParser = new BlockParser();
			dx = new DxScreenCapture();
		}

		public bool ParseScreen(Board board, out List<Block> nonZeroValues)
		{
			Surface surface = dx.CaptureScreen();
			DataRectangle dr = surface.LockRectangle(LockFlags.None);
			DataStream gs = dr.Data;

			nonZeroValues = new List<Block>();
			bool initialBoard = true;

			if (xoffset == 0 || yoffset == 0)
				setOffset(gs);

			byte[] buffer = new byte[blockWidth * 4 * blockHeight];
			PixelColor[,] pixels = new PixelColor[blockWidth, blockHeight];
			for (int i = 0; i < board.Width; i++)
			{
				for (int j = 0; j < board.Height; j++)
				{
					for (int k = 0; k < blockHeight; k++)
					{
						gs.Position = (k + GetYCoord(j)) * screenWidth * 4 + GetXCoord(i) * 4;
						gs.Read(buffer, blockWidth * 4 * k, blockWidth * 4);
					}
					for (int a = 0; a < blockWidth; a++)
					{
						for (int b = 0; b < blockHeight; b++)
						{
							int offset = b * blockWidth * 4 + (a * 4);
							pixels[b, a].Red = buffer[offset + 2];
							pixels[b, a].Green = buffer[offset + 1];
							pixels[b, a].Blue = buffer[offset + 0];
						}
					}
					board.Grid[i, j] = blockParser.Parse(i, j, pixels);

					if (board.Grid[i, j].State == BlockState.ParseFailed)
					{
						throw new ParserException("Point [" + i + ", " + j + "] failed! Pixel [" + GetXCoord(i) + "," + GetYCoord(j) + "]");
					}
					if (board.Grid[i, j].Value > 0)
						nonZeroValues.Add(board.Grid[i, j]);

					if (board.Grid[i, j].State != BlockState.Unknown)
						initialBoard = false;
				}
			}

			gs.Close();
			surface.UnlockRectangle();
			surface.Dispose();

			return initialBoard;
		}

		private void setOffset(DataStream gs)
		{
			PixelColor[,] test = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/test.png")));

			int testWidth = 5;
			int testHeight = 5;
			byte[] buffer = new byte[testWidth * 4 * testHeight];
			PixelColor[,] pixels = new PixelColor[testWidth, testHeight];
			for (int x = 0; x < screenWidth-5; x+=5)
			{
				for (int y = 0; y < screenHeight-5; y+=5)
				{
					for (int line = 0; line < testHeight; line++)
					{
						gs.Position = (line + y) * screenWidth * 4 + x * 4;
						gs.Read(buffer, testWidth * 4 * line, testWidth * 4);
					}

					for (int a = 0; a < testWidth; a++)
					{
						for (int b = 0; b < testHeight; b++)
						{
							int offset = b * testWidth * 4 + (a * 4);
							pixels[b, a].Red = buffer[offset + 2];
							pixels[b, a].Green = buffer[offset + 1];
							pixels[b, a].Blue = buffer[offset + 0];
						}
					}

					if (BlockParser.CompareImages(pixels, test))
					{
						int finalX;
						int finalY;
						byte[] pixel = new byte[4];
						for (finalX = x; finalX > 0; finalX--)
						{
							gs.Position = y * screenWidth * 4 + finalX * 4;
							gs.Read(pixel, 0, 4);
							if (compareInt(pixel[0], 58, 9) &&
								compareInt(pixel[1], 52, 9) &&
								compareInt(pixel[2], 47, 9))
								break;
						}
						for (finalY = y; finalY > 0; finalY--)
						{
							gs.Position = finalY * screenWidth * 4 + x * 4;
							gs.Read(pixel, 0, 4);
							if (compareInt(pixel[0], 58, 9) &&
								compareInt(pixel[1], 52, 9) &&
								compareInt(pixel[2], 47, 9))
								break;
						}
						xoffset = finalX-3;
						yoffset = finalY-1;
						return;
					}
				}
			}
		}

		public int GetXCoord(int xIndex)
		{
			int pos = xoffset + xIndex * blockWidth;
			return (int)(pos - ((float)xIndex / 29f) * 7f);
		}

		public int GetYCoord(int yIndex)
		{
			int pos = yoffset + yIndex * blockHeight;
			return (int)(pos - ((float)yIndex / 29f) * 7f);
		}

		public static BitmapSource copyScreen()
		{
			var left = 0;
			var top = 0;
			var right = 1920;
			var bottom = 1080;
			var width = right - left;
			var height = bottom - top;

			using (var screenBmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			{
				using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp))
				{
					bmpGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
					return Imaging.CreateBitmapSourceFromHBitmap(
							screenBmp.GetHbitmap(),
							IntPtr.Zero,
							Int32Rect.Empty,
							BitmapSizeOptions.FromEmptyOptions());
				}
			}
		}

		private static bool compareInt(int i1, int i2, int delta)
		{
			return Math.Abs(i1 - i2) < delta;
		}
	}
}
