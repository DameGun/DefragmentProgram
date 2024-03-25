using System.Windows.Media;

namespace DefragCourseProject.Libs.FragmentationModels
{
	public class ClusterGroup
	{
		public string Range { get; set; }
		public double AverageFragmentationIndex { get; set; }
		public Color FragmentationColor;
		public List<Cluster> Clusters { get; set; }

		public ClusterGroup(string range, double averageFragmentationIndex, Color fragmentationColor, List<Cluster> clusters)
		{
			Range = range;
			AverageFragmentationIndex = averageFragmentationIndex;
			FragmentationColor = fragmentationColor;
			Clusters = clusters;
		}
	}
}
