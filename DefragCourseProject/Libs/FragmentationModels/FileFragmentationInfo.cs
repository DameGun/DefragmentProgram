using DefragCourseProject.Libs.Win32ApiCalls;

namespace DefragCourseProject.Libs.FragmentationModels
{
    public class FileFragmentationInfo
    {
        public string FilePath { get; set; }
        public double FragmentationIndex { get; set; }

        public FileFragmentationInfo(string filePath)
        {
            FilePath = filePath;
            double fragmentationIndexBuff = DriveHelper.GetFileFragmentationIndex(filePath);
			FragmentationIndex = 
                fragmentationIndexBuff < 0.01 ? 
                    fragmentationIndexBuff * 100 : 
                    fragmentationIndexBuff;
        }
	}
}
