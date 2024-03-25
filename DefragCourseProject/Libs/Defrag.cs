using System.Diagnostics;
using System.Text;
using System.Windows.Shapes;

namespace DefragCourseProject.Libs
{
	public static class Defrag
	{
		public static Logging Logger { get; set; }

		public static async void DefragmentDrive(char driveLetter)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "defrag.exe",
				Arguments = $"{driveLetter}: /U",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.GetEncoding("CP866"),
			};

			Process defragProcess = new Process
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};

			defragProcess.OutputDataReceived += (sender, eventArgs) =>
			{
				if (!string.IsNullOrEmpty(eventArgs.Data))
				{
					string test = eventArgs.Data;
					Logger.Invoke("DefragInfo", $"{eventArgs.Data}\n");
				}
			};

			defragProcess.Start();
			defragProcess.BeginOutputReadLine();
			defragProcess.WaitForExit();
		}
	}
}
