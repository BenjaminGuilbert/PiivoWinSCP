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
    /// Class to communicate to a SFTP server
    /// </summary>
    class SFTP
    {
        private static Logger logger = new Logger("main.PiivoWinSCP.SFTP");

        /// <summary>
        /// Download Files from the remote server to the local server
        /// </summary>
        /// <param name="RemotePath">Path in the distant server</param>
        /// <param name="LocalPath">Path in the local server</param>
        /// <param name="HostName">DNS of the server</param>
        /// <param name="PortNumber">the port number</param>
        /// <param name="Login">User name</param>
        /// <param name="Password">User's Password</param>
        /// <param name="SshHostKeyFingerprint">ssh-rsa 2048 xx:xx:xx:xx:xx:xx:xx:xx...</param>
        /// <param name="SshPrivateKeyPath">Path of the private SSH Key</param>
        /// <param name="MoveFiles">if true, the file in the remote server will be deleted</param>
        /// <param name="TimeStamped">if true, the datetime will be add in the end of the file name</param>
        static public void Get(String RemotePath, String LocalPath, String HostName, String PortNumber, String Login, String Password, String SshHostKeyFingerprint, String sshPrivateKeyPath, String MoveFiles, String TimeStamped)
        {
            logger.info("Starting get file from {0} ...", String.Concat(HostName, "/", RemotePath));
            int iPortNumber = 22;
            if(PortNumber != null && !Int32.TryParse(PortNumber, out iPortNumber))
            {
                logger.error("Get : PortNumber {0} is not a correct number.", PortNumber);
                throw new Exception("ERROR - Get : PortNumber " + PortNumber + " is not a correct number.");
            }

            if (!Directory.Exists(LocalPath))
            {
                Directory.CreateDirectory(LocalPath);
            }

            // Create session
            SessionOptions sessionOptions = createSession(HostName, iPortNumber, Login, Password, SshHostKeyFingerprint, sshPrivateKeyPath);

            //Download File from remote path
            Boolean RemoveRemoteFiles = (MoveFiles == "True") ? true : false;
            List<String> filesTransfered = GetFiles(sessionOptions, RemotePath, LocalPath, RemoveRemoteFiles);

            //Add time stamp in the name of each file downloaded
            Boolean addTimeStamp = (TimeStamped == "True") ? true : false;
            if (addTimeStamp)
            {
                Utils.RenameFilesWithTimeStamp(LocalPath, filesTransfered);
            }



             
        }

        /// <summary>
        /// Upload Files from the local server to the remote server
        /// </summary>
        /// <param name="LocalPath">Path in the local server</param>
        /// <param name="RemotePath">Path in the distant server</param>
        /// <param name="HostName">DNS of the server</param>
        /// <param name="PortNumber">the port number</param>
        /// <param name="Login">User name</param>
        /// <param name="Password">User's Password</param>
        /// <param name="SshHostKeyFingerprint">ssh-rsa 2048 xx:xx:xx:xx:xx:xx:xx:xx...</param>
        /// <param name="SshPrivateKeyPath">Path of the private SSH Key</param>
        /// <param name="MoveFiles">if true, the file in the remote server will be deleted</param>
        /// <param name="TimeStamped">if true, the datetime will be add in the end of the file name</param>
        static public void Put(String LocalPath, String RemotePath, String HostName, String PortNumber, String Login, String Password, String SshHostKeyFingerprint, String SshPrivateKeyPath, String MoveFiles, String TimeStamped)
        {
            logger.info("Starting put file from {0} ...", LocalPath);
            //Check if the local directory exist
            Utils.CheckIfDirectoryExist(LocalPath);            

            //Check if the PortNumber is in a good format
            int iPortNumber = 22;
            if (PortNumber != null && !Int32.TryParse(PortNumber, out iPortNumber))
            {
                logger.error("Put : PortNumber {0} is not a correct number.", PortNumber);
                throw new Exception("ERROR - Put : PortNumber " + PortNumber + " is not a correct number.");
            }
                
            //Create session
            SessionOptions sessionOptions = createSession(HostName, iPortNumber, Login, Password, SshHostKeyFingerprint, SshPrivateKeyPath);

            //Add time stamp in the name of each file before upload
            Boolean addTimeStamp = (TimeStamped == "True") ? true : false;
            if (addTimeStamp)
            {
                String pattern = Path.GetFileName(LocalPath);
                String directoryPath = Path.GetDirectoryName(LocalPath);
                List<String> filesToTransfer = Directory.GetFiles(directoryPath, pattern).ToList<String>();
                Utils.RenameFilesWithTimeStamp(directoryPath, filesToTransfer);
            }

            //Upload Files to remote path
            Boolean RemoveLocalFiles = (MoveFiles == "True") ? true : false;
            List<String> filesTransfered = PutFiles(sessionOptions, RemotePath, LocalPath, RemoveLocalFiles);

        }




        static private SessionOptions createSession(String hostName, int portNumber, String userName, String pasword, String sshHostKeyFingerprint, String sshPrivateKeyPath)
        {
            try
            {
                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.Protocol = Protocol.Sftp;
                sessionOptions.HostName = hostName;
                sessionOptions.PortNumber = portNumber;
                sessionOptions.UserName = userName;
                sessionOptions.Password = pasword;
                sessionOptions.SshHostKeyFingerprint = sshHostKeyFingerprint;
                sessionOptions.SshPrivateKeyPath = sshPrivateKeyPath;

                logger.info("Create connexion session to {0}: Success", hostName);

                return sessionOptions;
            }
            catch (Exception e)
            {
                logger.error("Cannot create connexion session to {0}.", hostName);
                throw new Exception("ERROR - {0}", e);
            }

        }


        static private List<String> GetFiles(SessionOptions sessionOptions, String RemotePath, String LocalPath, Boolean RemoveRemoteFiles)
        {
            try
            {
                //Session
                using (Session session = new Session())
                {
                    session.Open(sessionOptions);
                    logger.info("Session is opened. Starting copy to {0} ", LocalPath);

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult = session.GetFiles(RemotePath, LocalPath, RemoveRemoteFiles, transferOptions);

                    transferResult.Check();

                    List<String> allFilesTransfered = new List<string>();

                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        logger.info("Download of {0} succeeded", transfer.FileName);
                        allFilesTransfered.Add(transfer.FileName);
                    }

                    return allFilesTransfered;

                }
            }
            catch (Exception e)
            {
                logger.error("ERROR -  PutFiles : " + e.Message);
                throw new Exception("ERROR - GetFiles : "+e.Message, e);
            }
        }


        static private List<String> PutFiles(SessionOptions sessionOptions, String RemotePath, String LocalPath, Boolean RemoveRemoteFiles)
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
                    // transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                    TransferOperationResult transferResult = session.PutFiles(LocalPath, RemotePath, RemoveRemoteFiles, transferOptions);

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
                logger.error("ERROR -  PutFiles : " + e.Message);
                throw new Exception("ERROR -  PutFiles : "+e.Message, e);
            }
        }


        
    }
}
