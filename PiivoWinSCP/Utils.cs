using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiivoWinSCP
{
    class Utils
    {
        /// <summary>
        /// Add a timeStamp in the end of each files
        /// </summary>
        /// <param name="LocalPath"></param>
        /// <param name="files"></param>
        internal static void RenameFilesWithTimeStamp(String LocalPath, List<String> files)
        {
            try
            {
                foreach (String file in files)
                {
                    String fileName = Path.GetFileName(file);
                    RenameFileWithTimeStamp(Path.Combine(LocalPath, fileName));
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Add timeStamp in the end of the file
        /// </summary>
        /// <param name="filePath"></param>
        internal static void RenameFileWithTimeStamp(String filePath)
        {
            try
            {
                String stamp = DateTime.Now.ToString("_yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                FileInfo fInfo = new FileInfo(filePath);
                String fileName = Path.GetFileName(filePath);
                String directoryPath = Path.GetDirectoryName(filePath);
                String newName = Path.GetFileNameWithoutExtension(fileName) + stamp + fInfo.Extension;
                fInfo.MoveTo(Path.Combine(directoryPath, newName));
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        /// <summary>
        /// Create a CSV File containing all files' name
        /// </summary>
        /// <param name="path"></param>
        /// <param name="allFilesTransfered"></param>
        internal static void CreateCSV(string path, List<string> allFilesTransfered)
        {
            StringBuilder csv = new StringBuilder();

            foreach (String sFilePath in allFilesTransfered)
            {
                csv.AppendLine(Path.GetFileName(sFilePath));
            }

            File.WriteAllText(path, csv.ToString());
            Utils.RenameFileWithTimeStamp(path);
        }


        internal static void CheckIfDirectoryExist(string LocalPath)
        {
            String directoryPath = Path.GetDirectoryName(LocalPath);
            if (!Directory.Exists(directoryPath))
            {
                throw new Exception("ERROR - Put : The directory of " + LocalPath + " does not exist.");
            }
        }

        internal static void CheckIfUriWellFormed(string uri)
        {
            try
            {
                Path.GetFullPath(uri);
            }
            catch(Exception)
            {
                throw new Exception("ERROR - Put : The path " + uri + " is not well formed.");
            }
        }
    }
}
