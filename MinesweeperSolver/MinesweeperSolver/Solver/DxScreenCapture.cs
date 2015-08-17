using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.Direct3D9;

namespace MinesweeperSolver.Solver
{
	public class DxScreenCapture
	{
		Device d;

		public DxScreenCapture()
		{
			PresentParameters present_params = new PresentParameters();
			present_params.Windowed = true;
			present_params.SwapEffect = SwapEffect.Discard;
			d = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, present_params);
		}

		public Surface CaptureScreen()
		{
			Surface s = Surface.CreateOffscreenPlain(d, 1920,1200, Format.A8R8G8B8, Pool.Scratch);
			d.GetFrontBufferData(0, s);
			return s;
		}
	}
}
