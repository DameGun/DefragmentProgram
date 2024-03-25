using DefragCourseProject.Libs.FragmentationModels;
using DefragCourseProject.Libs.Win32ApiCalls;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace DefragCourseProject.Libs
{
	public delegate void Logging(string propertyName, string value);

    public class DriveAnalyzer : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		public Logging Logger { get; set; }

		protected virtual void OnPropertyChanged(string propertyName, string value)
		{
			Logger.Invoke(propertyName, value);
		}

		private DriveInfo _selectedDrive;
		public DriveInfo SelectedDrive
		{
			get => _selectedDrive;
			set {
				_selectedDrive = value;
				DriveClusterSize = DriveHelper.GetClusterSize(SelectedDrive.Name);
			}
		}
		public uint DriveClusterSize { get; set; }
		public string ErrorString { get; set; }

		private string _statusString;
		public string StatusString
		{
			get => _statusString;
			set
			{
				_statusString = value;
				OnPropertyChanged(nameof(StatusString), _statusString);
			}
		}

		private double _progressBarValue;
		public double ProgressBarValue
		{
			get => _progressBarValue;
			set
			{
				_progressBarValue = value;
				OnPropertyChanged(nameof(ProgressBarValue), _progressBarValue.ToString());
			}
		}

		private bool _progressBarType;
		public bool ProgressBarType
		{
			get => _progressBarType;
			set
			{
				_progressBarType = value;
				OnPropertyChanged(nameof(ProgressBarType), _progressBarType.ToString());
			}
		}


		private string[] GetAllFiles(string directoryPath)
		{
			List<string> allFiles = new List<string>();
			TraverseDirectory(directoryPath, allFiles);
			return allFiles.ToArray();
		}

		private void TraverseDirectory(string directoryPath, List<string> allFiles)
		{
			try
			{
				foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly))
				{
					allFiles.Add(filePath);
				}

				foreach (string subdirectory in Directory.EnumerateDirectories(directoryPath))
				{
					TraverseDirectory(subdirectory, allFiles);
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				ErrorString = $"Ошибка доступа при получении файлов: {ex.Message}";
			}
			catch (Exception ex)
			{
				ErrorString = $"Ошибка при получении файлов: {ex.Message}";
			}
		}

		public async Task<List<Color>> AnalyzeDrive()
		{
			StatusString = "Получение файлов с диска";
			ProgressBarType = true;
			var allFiles = GetAllFiles(SelectedDrive.RootDirectory.FullName);

			int count = allFiles.Length;
			ProgressBarValue = 0;

			ConcurrentBag<FileFragmentationInfo> filesWithFragmentation = new ConcurrentBag<FileFragmentationInfo>();

			var progress = new Progress<double>(value =>
			{
				ProgressBarValue = ((double)(value + 1) / count) * 100;
				StatusString = $"Получение информации о фрагментации файлов: {value} / {count}";
			});

			ProgressBarType = false;

			await Task.Run(() =>
			{
				int j = 0;
				Parallel.For(0, count, i =>
				{
					j++;
					FileFragmentationInfo fileInfo = new FileFragmentationInfo(allFiles[i]);
					if (fileInfo.FragmentationIndex >= 0 && fileInfo.FragmentationIndex <= 1)
					{
						filesWithFragmentation.Add(fileInfo);
					}

					((IProgress<double>)progress).Report(j);
				});
			});

			allFiles = new string[0];

			StatusString = "Группировка файлов по кластерам";
			ProgressBarType = true;
			List<Cluster> clustersData = GroupFragmentationDataByClusters(filesWithFragmentation.ToList());

			filesWithFragmentation.Clear();

			StatusString = "Группировка кластеров по диапозонам";
			ProgressBarType = true;
			List<ClusterGroup> clustersGroups = GroupClustersBySequentialRanges(clustersData);

			return clustersGroups.Select(c => c.FragmentationColor).ToList();
		}

		private List<Cluster> GroupFragmentationDataByClusters(List<FileFragmentationInfo> fragmentationData)
		{
			var clusterGroups = fragmentationData
				.GroupBy(file => (int)(new FileInfo(file.FilePath).Length / DriveClusterSize))
				.Select(group => new Cluster(group.Key, group.Average(f => f.FragmentationIndex), group.ToList()))
				.OrderBy(g => g.Number)
				.ToList();

			return clusterGroups;
		}

		private List<ClusterGroup> GroupClustersBySequentialRanges(List<Cluster> clusters)
		{
			int rangeSize = (int)Math.Ceiling((double)clusters.Count / 200);

			var result = clusters
				.GroupBy(cluster => $"{rangeSize * (cluster.Number / rangeSize)}-{rangeSize * (cluster.Number / rangeSize) + rangeSize}")
				.Select(group => 
					new ClusterGroup(
						group.Key, 
						group.Select(c => c.AverageFragmentationIndex).Average(), 
						CalculateColor(group.Average(c => c.FileFragments.Average(f => f.FragmentationIndex))), 
						group.ToList()))
				.ToList();

			return result;
		}

		private static Color CalculateColor(double fragmentationIndex)
		{
			byte red, green;
			if (fragmentationIndex < 0.25)
			{
				red = 255;
				green = (byte)(255 * fragmentationIndex * 4);
			}
			else if (fragmentationIndex < 0.5)
			{
				red = (byte)(255 * (0.5 - fragmentationIndex) * 4);
				green = 255;
			}
			else if (fragmentationIndex < 0.75)
			{
				red = 0;
				green = 255;
			}
			else
			{
				red = 0;
				green = (byte)(255 * (1 - fragmentationIndex) * 4);
			}

			return Color.FromRgb(red, green, 0);
		}
	}
}
