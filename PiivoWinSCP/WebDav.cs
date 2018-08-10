using Evolution.Logging.Clr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace PiivoWinSCP
{
    /// <summary>
    /// Class to communicate to a WebDav server
    /// </summary>
    public class WebDav
    {
        private static Logger logger = new Logger("main.PiivoWinSCP.WebDav");

        /// <summary>
        /// Move one or more files from local directory to remote directory.
        /// </summary>
        /// <param name="LocalPath">Full local path</param>
        /// <param name="HostName">DNS of the remote server</param>
        /// <param name="WebDavRoot">path to the root of the webdav</param>
        /// <param name="RemotePath">path from root to the folder targeted in the remote server</param>
        /// <param name="Login">User name</param>
        /// <param name="Password">User's password</param>
        /// <param name="MoveFiles">if true, the file in the local server will be deleted</param>
        /// <param name="CsvResultFilePath"></param>
        static public void Put(String LocalPath, String HostName, String WebDavRoot, String RemotePath, String Login, String Password, String MoveFiles, String CsvResultFilePath)
        {
            //Check if the local directory exist            
            Utils.CheckIfDirectoryExist(LocalPath);
            Utils.CheckIfUriWellFormed(CsvResultFilePath);

            logger.info("Starting put file from {0} ...", LocalPath);

            //Create a session connexion
            SessionOptions sessionOptions = createSession(HostName, Login, Password, WebDavRoot);

            //Upload Files to remote path
            Boolean RemoveLocalFiles = (MoveFiles == "True") ? true : false;
            List<String> allFilesTransfered = PutFiles(sessionOptions, LocalPath, RemotePath, RemoveLocalFiles);

            //Save all files' name in a csv file.
            if (CsvResultFilePath != null && CsvResultFilePath != String.Empty)
            {
                Utils.CreateCSV(CsvResultFilePath, allFilesTransfered);
            }

        }



        static private List<String> PutFiles(SessionOptions sessionOptions, String LocalPath, String RemotePath, Boolean RemoveLocalFiles)
        {
            try
            {
                //Session
                using (Session session = new Session())
                {
                    session.Open(sessionOptions);
                    logger.info("Session is opened. Starting copy to {0} ", RemotePath);

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.FileMask = "* | *.db"; //exclude all thumbs.db

                    TransferOperationResult transferResult = session.PutFiles(LocalPath, RemotePath, RemoveLocalFiles, transferOptions);

                    //Throw an error if upload fails
                    transferResult.Check();

                    List<String> allFilesTransfered = new List<string>();
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        logger.info("Upload of {0} succeeded", transfer.FileName);
                        allFilesTransfered.Add(transfer.FileName);
                    }
                    return allFilesTransfered;

                }
            }
            catch (Exception e)
            {
                throw new Exception("ERROR -  {0}", e);
            }
        }


        static private SessionOptions createSession(String hostName, String userName, String pasword, String root)
        {
            try
            {
                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.Protocol = Protocol.Webdav;
                sessionOptions.HostName = hostName;
                sessionOptions.PortNumber = 443;
                sessionOptions.UserName = userName;
                sessionOptions.Password = pasword;
                sessionOptions.WebdavSecure = true;
                sessionOptions.WebdavRoot = root;

                logger.info("Create connexion session to {0}: Success", hostName);

                return sessionOptions;
            }
            catch (Exception e)
            {
                logger.error("Cannot create connexion session to {0}: Success", hostName);
                throw new Exception("ERROR - {0}", e);
            }

        }
    }
}
