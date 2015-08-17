using MinesweeperSolver.Solver;
using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MinesweeperSolver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
			Left = 700+1920;
			Top = 300;

			InitializeComponent();
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			//BitmapSource currentScreen = Win8Parser.copyScreen();
			//imgPart.Source = currentScreen;

			//PixelColor[,] testPixels = BitmapSourceHelper.GetPixels(testBMP);

			//new BlockParser().Parse(0, 0, testPixels);
			//PixelColor testColor = testPixels[Int32.Parse(txtX.Text), Int32.Parse(txtY.Text)];
			//txtPixel.Text = testColor.ToString();
			//btnPart.Background = new SolidColorBrush(Color.FromArgb(testColor.Alpha, (byte)testColor.Red, (byte)testColor.Green, (byte)testColor.Blue));


			int x = Int32.Parse(txtCoordX.Text);
			int y = Int32.Parse(txtCoordY.Text);
			int captureWidth = 1920;
			int captureHeight = 1200;
			Surface surface = new DxScreenCapture().CaptureScreen();
			DataRectangle dr = surface.LockRectangle(new System.Drawing.Rectangle(0, 0, captureWidth, captureHeight), LockFlags.None);
			DataStream gs = dr.Data;

			WriteableBitmap wb = new WriteableBitmap(54, 54, 96, 96, PixelFormats.Bgra32, null);
			int bytesPerPixel = (wb.Format.BitsPerPixel + 7) / 8;
			int stride = wb.PixelWidth * bytesPerPixel;

			byte[] buffer = new byte[54 * 4 * 54];
			for (int i = 0; i < 54; i++)
			{
				gs.Position = (i + y) * captureWidth * 4 + x * 4;
				gs.Read(buffer, 54 * 4 * i, 54 * 4);
			}
			wb.WritePixels(new Int32Rect(0, 0, 54, 54), buffer, 54 * 4, 0);

			imgPart.Source = wb;
		}

		//private void writeImage()
		//{
		//	BitmapSource currentScreen = Win8Parser.copyScreen();

		//	CroppedBitmap testBMP = new CroppedBitmap(currentScreen, new Int32Rect(new Win8Parser().GetXCoord(10),
		//		new Win8Parser().GetYCoord(13), 54, 54));
		//	var encoder = new PngBitmapEncoder();
		//	encoder.Frames.Add(BitmapFrame.Create(testBMP));
		//	using (var stream = File.OpenWrite("test.png"))
		//		encoder.Save(stream);
		//}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			//writeImage();
			txtProgress.Text = "";
			Board board;
			try
			{
				board = new Board(new Win8Parser(), 30, 16);
			}
			catch (ParserException ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}

			board.AutoGuess = chkAutoGuess.IsChecked.GetValueOrDefault();
			board.ProgressUpdate += board_ProgressUpdate;
			board.Finished += board_Finished;
			board.StartSolver();
		}

		private void board_Finished(Board sender)
		{
			txtProgress.Text += "\n\n" + sender.Stats;
		}

		private void board_ProgressUpdate(Board sender, ProgressArgs args)
		{
			txtProgress.Text += args.Message + "\n";
		}
	}
}
