using Eto.Forms;
using Eto.Drawing;
using System.IO;

public static class Program
{
	[STAThread]
	public static void Main()
	{
		CTR.Initialization.InitManager.Init();
		CTR.Window.WManager.Start();
	}
}