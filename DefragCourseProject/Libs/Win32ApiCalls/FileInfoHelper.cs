using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Ioctl;

namespace DefragCourseProject.Libs.Win32ApiCalls
{
	public static class DriveHelper
	{
		public static double GetFileFragmentationIndex(string filePath)
		{
			var handle = PInvoke.CreateFile(
				filePath,
				((uint)GENERIC_ACCESS_RIGHTS.GENERIC_READ),
				FILE_SHARE_MODE.FILE_SHARE_READ,
				lpSecurityAttributes: null,
				FILE_CREATION_DISPOSITION.OPEN_EXISTING,
				FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS,
				hTemplateFile: null);

			if (handle.IsInvalid)
			{
				return -1;
			}

			var input = new STARTING_VCN_INPUT_BUFFER();
			var output = new RETRIEVAL_POINTERS_BUFFER();
			uint bytesReturned;
			unsafe
			{
				if (!PInvoke.DeviceIoControl(
					handle,
					PInvoke.FSCTL_GET_RETRIEVAL_POINTERS,
					&input,
					(uint)Marshal.SizeOf(input),
					&output,
					(uint)Marshal.SizeOf(typeof(RETRIEVAL_POINTERS_BUFFER)),
					&bytesReturned,
					lpOverlapped: null))
				{
					return -1;
				}

				var extnsLength = output.Extents.Length;
				long fileSizeInClusters = output.Extents._0.NextVcn;
				double fragmentationIndex = (double)output.ExtentCount / fileSizeInClusters;
				return fragmentationIndex;
			}
		}

		public static uint GetClusterSize(string driveLetter)
		{
			uint sectorsPerCluster;
			uint bytesPerSector;
			uint numberOfFreeClusters;
			uint totalNumberOfClusters;

			try
			{
				unsafe
				{
					PInvoke.GetDiskFreeSpace(
						driveLetter,
						&sectorsPerCluster,
						&bytesPerSector,
						&numberOfFreeClusters,
						&totalNumberOfClusters);
				}

				uint clusterSize = sectorsPerCluster * bytesPerSector;
				return clusterSize;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"{ex.Message}");
				return 0;
			}
		}
	}
}
