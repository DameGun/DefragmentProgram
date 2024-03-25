using System.Globalization;
using System.Windows.Data;

namespace DefragCourseProject.Libs.Misc
{
	public class BytesToMegabytesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is long bytes)
			{
				return $"{bytes / (1024 * 1024)} Mb";
			}
			return $"{value}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
