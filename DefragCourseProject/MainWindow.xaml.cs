using DefragCourseProject.Libs;
using System.Windows.Shapes;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;

namespace DefragCourseProject
{
    public partial class MainWindow : Window
	{
		public DriveAnalyzer Analyzer = new DriveAnalyzer();

		private DriveInfo _selectedDrive;
		public DriveInfo SelectedDrive
		{
			get { return _selectedDrive; }
			set
			{
				_selectedDrive = value;

				Analyzer.SelectedDrive = value;

				analyzeButton.IsEnabled = true;

				optimizeButton.IsEnabled = true;
			}
		}
		
		public List<DriveInfo> Drives { get; }
		public List<Color> ClusterGroupsData { get; set; }

		private const double scale = 3;
		private const double rectSize = 8;

		public MainWindow()
		{
			InitializeComponent();

			DataContext = this;

			Drives = DriveInfo.GetDrives().ToList();
			DriveListView.ItemsSource = Drives;
		}

		public void PropertyChanged(string propertyName, object value) {
			Dispatcher.Invoke(() =>
			{
				if(propertyName == "StatusString")
				{
					progressBarText.Text = value.ToString();
				}
				if(propertyName == "ProgressBarValue") {
					progressBar.Value = Convert.ToDouble(value);
				}
				if(propertyName == "ProgressBarType")
				{
					progressBar.IsIndeterminate = Convert.ToBoolean(value);
				}
				if(propertyName == "DefragInfo")
				{
					defragLoggingTextBox.Text += value;
					defragLoggingTextBox.UpdateLayout();
				}
			});
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var driveItem = (sender as ListView).SelectedItem as DriveInfo;
			SelectedDrive = driveItem;
		}

		private async void analyzeButton_Click(object sender, RoutedEventArgs e)
		{
			Analyzer.Logger += PropertyChanged;

			tabControl.SelectedIndex = 0;

			analyzeButton.IsEnabled = false;
			optimizeButton.IsEnabled = false;

			progressBar.Visibility = Visibility.Visible;

			List<Color> data = new List<Color>();

			await Task.Run(async () =>
			{
				data = await Analyzer.AnalyzeDrive();
			});

			Dispatcher.Invoke(() =>
			{
				ClusterGroupsData = data;
				VisualizeData();
			});
		}

		private void VisualizeData()
		{
			PropertyChanged("StatusString", string.Empty);
			optimizeButton.IsEnabled = true;
			analyzeButton.IsEnabled = true;

			progressBar.Visibility = Visibility.Collapsed;

			clusterVisualizerCanvas.Children.Clear();

			int columnsCount = (int)Math.Floor(clusterVisualizerCanvas.ActualWidth / (rectSize + scale));

			for (int i = 0; i < ClusterGroupsData.Count; i++)
			{
				Rectangle rect = new Rectangle();
				rect.Width = rectSize;
				rect.Height = rectSize;

				rect.Fill = new SolidColorBrush(ClusterGroupsData[i]);

				int column = i % columnsCount;
				int row = i / columnsCount;

				double x = column * (rectSize + scale);
				double y = row * (rectSize + scale);

				Canvas.SetLeft(rect, x);
				Canvas.SetTop(rect, y);

				clusterVisualizerCanvas.Children.Add(rect);
			}
		}

		private async void optimizeButton_Click(object sender, RoutedEventArgs e)
		{
			defragLoggingTextBox.Text = string.Empty;
			tabControl.SelectedIndex = 1;

			Defrag.Logger += PropertyChanged;

			progressBar.Visibility = Visibility.Visible;
			PropertyChanged("ProgressBarType", true);
			PropertyChanged("StatusString", $"Выполняется дефрагментация диска {SelectedDrive.Name[0]}");

			await Task.Run(() =>
			{
				Defrag.DefragmentDrive(SelectedDrive.Name[0]);
			});

			PropertyChanged("StatusString", string.Empty);
			progressBar.Visibility = Visibility.Hidden;
		}
	}
}