using System;
using System.Windows.Media.Imaging;

namespace MinesweeperSolver.Solver
{
	public class BlockParser
	{
		private const int colorDelta = 40;

		private PixelColor[,] imgGuess;
		private PixelColor[,] imgOne;
		private PixelColor[,] imgTwo;
		private PixelColor[,] imgThree;
		private PixelColor[,] imgFour;
		private PixelColor[,] imgFive;
		private PixelColor[,] imgSix;
		private PixelColor[,] imgSeven;
		private PixelColor[,] imgEight;

		public BlockParser()
		{
			imgGuess = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/guess.png")));

			imgOne = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/one.png")));
			imgTwo = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/two.png")));
			imgThree = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/three.png")));
			imgFour = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/four.png")));
			imgFive = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/five.png")));
			imgSix = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/six.png")));
			imgSeven = BitmapSourceHelper.GetPixels(new BitmapImage(new Uri(@"pack://application:,,,/MinesweeperSolver;component/Images/seven.png")));
		}

		public Block Parse(int x, int y, PixelColor[,] pixels)
		{
			if (isUnknown(pixels))
				return new Block(x, y, BlockState.Unknown);
			if (isZero(pixels))
				return new Block(x, y, 0);
			if (compareImages(pixels, imgOne))
				return new Block(x, y, 1);
			if (compareImages(pixels, imgTwo))
				return new Block(x, y, 2);
			if (compareImages(pixels, imgThree))
				return new Block(x, y, 3);
			if (compareImages(pixels, imgFour))
				return new Block(x, y, 4);
			if (compareImages(pixels, imgFive))
				return new Block(x, y, 5);
			if (compareImages(pixels, imgSix))
				return new Block(x, y, 6);
			if (compareImages(pixels, imgSeven))
				return new Block(x, y, 7);
			if (isFlag(pixels))
				return new Block(x, y, BlockState.Flag);
			if (compareImages(pixels, imgGuess))
				return new Block(x, y, BlockState.Unknown, true);

			return new Block(x, y, BlockState.ParseFailed);
		}

		private bool isZero(PixelColor[,] pixels)
		{
			for (int i = 15; i < 30; i++)
				for (int j = 15; j < 30; j++)
					if (!compareColor(pixels[i, j], new PixelColor(255, 255, 255), colorDelta))
						return false;

			return true;
		}

		private bool isUnknown(PixelColor[,] pixels)
		{
			if (compareColor(pixels[10, 10], new PixelColor(99, 176, 255), colorDelta))
				return true;

			return false;
		}

		private bool isFlag(PixelColor[,] pixels)
		{
			for (int i = 5; i < 10; i++)
				for (int j = 10; j < 20; j++)
					if (!compareColor(pixels[i, j], new PixelColor(229, 194, 79), colorDelta))
						return false;

			return compareColor(pixels[20, 20], new PixelColor(199, 0, 0), colorDelta);
		}

		private bool compareColor(PixelColor c1, PixelColor c2, int delta)
		{
			return Math.Abs(c1.Blue - c2.Blue) < delta &&
				Math.Abs(c1.Red - c2.Red) < delta &&
				Math.Abs(c1.Green - c2.Green) < delta;
		}

		private bool compareImages(PixelColor[,] p1, PixelColor[,] p2)
		{
			if (p1.GetLength(0) != p2.GetLength(0))
				return false;

			long red = 0, green = 0, blue = 0;
			for (int i = 10; i < p1.GetLength(0) - 10; i++)
			{
				for (int j = 10; j < p1.GetLength(1) - 10; j++)
				{
					PixelColor color = p1[i, j];
					PixelColor average = getAverageColor(p2, i, j);

					PixelColor diff = color - average;
					red += diff.Red;
					green += diff.Green;
					blue += diff.Blue;

				}
			}
			red = (int)((float)red / p1.Length);
			green = (int)((float)green / p1.Length);
			blue = (int)((float)blue / p1.Length);

			float total = (float)(red + green + blue) / 3f;

			return total < 6;
		}

		public static bool CompareImages(PixelColor[,] p1, PixelColor[,] p2)
		{
			if (p1.GetLength(0) != p2.GetLength(0))
				return false;

			long red = 0, green = 0, blue = 0;
			for (int i = 1; i < p1.GetLength(0) - 1; i++)
			{
				for (int j = 1; j < p1.GetLength(1) - 1; j++)
				{
					PixelColor color = p1[i, j];
					PixelColor average = getAverageColor(p2, i, j);

					PixelColor diff = color - average;
					red += diff.Red;
					green += diff.Green;
					blue += diff.Blue;

				}
			}
			red = (int)((float)red / p1.Length);
			green = (int)((float)green / p1.Length);
			blue = (int)((float)blue / p1.Length);

			float total = (float)(red + green + blue) / 3f;

			return total < 6;
		}

		private static PixelColor getAverageColor(PixelColor[,] pixels, int x, int y)
		{
			int Blue = 0;
			Blue += pixels[x - 1, y - 1].Blue;
			Blue += pixels[x - 1, y].Blue;
			Blue += pixels[x - 1, y + 1].Blue;

			Blue += pixels[x, y - 1].Blue;
			Blue += pixels[x, y].Blue;
			Blue += pixels[x, y + 1].Blue;

			Blue += pixels[x + 1, y - 1].Blue;
			Blue += pixels[x + 1, y].Blue;
			Blue += pixels[x + 1, y + 1].Blue;

			int Red = 0;
			Red += pixels[x - 1, y - 1].Red;
			Red += pixels[x - 1, y].Red;
			Red += pixels[x - 1, y + 1].Red;

			Red += pixels[x, y - 1].Red;
			Red += pixels[x, y].Red;
			Red += pixels[x, y + 1].Red;

			Red += pixels[x + 1, y - 1].Red;
			Red += pixels[x + 1, y].Red;
			Red += pixels[x + 1, y + 1].Red;

			int Green = 0;
			Green += pixels[x - 1, y - 1].Green;
			Green += pixels[x - 1, y].Green;
			Green += pixels[x - 1, y + 1].Green;

			Green += pixels[x, y - 1].Green;
			Green += pixels[x, y].Green;
			Green += pixels[x, y + 1].Green;

			Green += pixels[x + 1, y - 1].Green;
			Green += pixels[x + 1, y].Green;
			Green += pixels[x + 1, y + 1].Green;


			Red = (int)((float)Red / 9);
			Green = (int)((float)Green / 9);
			Blue = (int)((float)Blue / 9);
			return new PixelColor((byte)Red, (byte)Green, (byte)Blue);
		}
	}
}
