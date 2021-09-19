namespace Apex.Robot.Vision.Helpers
{
    public class PathHelper
    {
        internal static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
