using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace AbstractionConfig
{
    public static class ContentSupport
    {
        public static string ContentRootPath;

        public static string GetContentRoot()
        {
            return ContentRootPath;
        }

        public static string GetAbstractionContentRoot(string abstractionName)
        {
            string polishedName = abstractionName.Replace("TRANS", "").Replace("ABS", "");
            return GetContentRoot() + polishedName + "\\";
        }

        private const string TransInputFolder = "In";
        private const string TransOutputFolder = "Out";
        private const string ContentFolderPrefixFilter = "Content_v";

        public static void CopyFromSourceToTrans(string sourceAbstractionName, string transName)
        {
            string sourceContentInputRoot = GetAbstractionContentRoot(sourceAbstractionName);
            string transContentInputRoot = GetTransContentInputRoot(transName);
            //string[] directoryNames = Directory.GetDirectories(sourceContentInputRoot, ContentFolderPrefixFilter + "*");
            //DirectoryInfo[] directories = directoryNames.Select(dirName => new DirectoryInfo(dirName)).ToArray();
            DirectoryInfo sourceDir = new DirectoryInfo(sourceContentInputRoot);
            DirectoryInfo targetDir = new DirectoryInfo(transContentInputRoot);
            CopyDirectoryTree(sourceDir, targetDir);
            //foreach(var sourceDir in directories)
            //{
            //    CopyDirectoryTree(sourceDir, targetDir);
            //}
        }

        private static string GetTransContentInputRoot(string transName)
        {
            return GetAbstractionContentRoot(transName) + TransInputFolder + "\\";
        }

        public static void CleanupTransformationDirectories(string transName)
        {
            Contract.Requires(transName.EndsWith("TRANS"));
            string transContentInputRoot = GetTransContentInputRoot(transName);
            CleanupDirectory(transContentInputRoot);
            string transContentOutputRoot = GetTransContentOutputRoot(transName);
            CleanupDirectory(transContentOutputRoot);
        }

        public static void CleanupDirectory(string directoryName)
        {
            Contract.Requires(string.IsNullOrEmpty(ContentRootPath) == false);
            Contract.Requires(directoryName.StartsWith(ContentRootPath));
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
            if(directoryInfo.Exists)
                directoryInfo.Delete(true);
            directoryInfo.Create();
        }

        public static void CopyDirectoryTree(DirectoryInfo source, DirectoryInfo target, string contentTargetName = null, bool deleteIfContentExists = false)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                bool isContentDir = dir.Name.StartsWith(ContentFolderPrefixFilter);
                string targetDirName = isContentDir && contentTargetName != null
                                           ? Path.Combine(dir.Name, contentTargetName)
                                           : dir.Name;
                if(isContentDir && deleteIfContentExists)
                {
                    string targetDirFullName = Path.Combine(target.FullName, targetDirName);
                    if(Directory.Exists(targetDirFullName))
                    {
                        Directory.Delete(targetDirFullName, true);
                    }
                }
                CopyDirectoryTree(dir, target.CreateSubdirectory(targetDirName));
            }
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        } 


        public static void CopyFromTransToTarget(string transName, string targetAbstractionName)
        {
            string transContentOutputRoot = GetTransContentOutputRoot(transName);
            string targetContentRootInputRoot = GetAbstractionContentRoot(targetAbstractionName);
            CopyDirectoryTree(new DirectoryInfo(transContentOutputRoot),
                              new DirectoryInfo(targetContentRootInputRoot), transName, true);
        }

        private static string GetTransContentOutputRoot(string transName)
        {
            return GetAbstractionContentRoot(transName) + TransOutputFolder + "\\";
        }
    }
}