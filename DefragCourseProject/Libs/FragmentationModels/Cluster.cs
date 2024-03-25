using System.Windows.Media;

namespace DefragCourseProject.Libs.FragmentationModels
{
	public class Cluster
	{
		public int Number { get; set; }
		public double AverageFragmentationIndex { get; set; }
		public List<FileFragmentationInfo> FileFragments { get; set; }

		public Cluster(int number, double averageFragmentationIndex, List<FileFragmentationInfo> fileFragments)
		{
			Number = number;
			AverageFragmentationIndex = averageFragmentationIndex;
			FileFragments = fileFragments;
		}
	}
}
