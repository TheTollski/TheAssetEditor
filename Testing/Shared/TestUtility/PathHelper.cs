﻿using System.ComponentModel.Design.Serialization;
using System.Text;

namespace Shared.TestUtility
{
    public static class PathHelper
    {
        /// <summary>
        /// Find the "AssetEditor" folder from the test directory and return the path to the file
        /// Probably superior to the hardcoded path in the original code
        /// </summary>        
        public static string FileFromDataFolder(string fileName, string rootDir = "TheAssetEditor")
        {
            var currentDirectory = TestContext.CurrentContext.TestDirectory;
            if (string.IsNullOrEmpty(currentDirectory))
                return "";

            while (true)
            {
                fileName = Path.GetFileName(currentDirectory); // get last foldername
                if (string.IsNullOrEmpty(fileName))
                    return "";

                if (fileName.ToLower() == rootDir.ToLower())
                    break;

                currentDirectory = Path.GetDirectoryName(currentDirectory); // go one folder UP
                if (string.IsNullOrEmpty(currentDirectory))  // reached root, nothing foun              
                    return "";
            }

            var fullPath = currentDirectory += @"\data\" + fileName;

            return fullPath;
        }


        public static string File(string fileName)
        {
            var fullPath = Path.GetFullPath(@"..\..\..\..\..\Data\" + fileName);
            if (System.IO.File.Exists(fullPath) == false)
                throw new Exception($"Unable to find data file {fileName}");
            return fullPath;
        }

        public static string Folder(string fileName)
        {
            var fullPath = Path.GetFullPath(@"..\..\..\..\..\Data\" + fileName);
            if (Directory.Exists(fullPath) == false)
                throw new Exception($"Unable to find data directory {fullPath}");
            return fullPath;
        }

        public static string Folder2(string fileName)
        {
            var fullPath = Path.GetFullPath(@"..\..\..\..\..\..\Data\" + fileName);
            if (Directory.Exists(fullPath) == false)
                throw new Exception($"Unable to find data directory {fullPath}");
            return fullPath;
        }


        public static byte[] GetFileAsBytes(string path)
        {
            var fullPath = File(path);
            var bytes = System.IO.File.ReadAllBytes(fullPath);
            return bytes; ;
        }

        public static string GetFileContentAsString(string path)
        {
            var bytes = GetFileAsBytes(path);
            return Encoding.UTF8.GetString(bytes);
        }

    }
}
