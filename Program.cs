using System;
using System.IO;
using WinSCP;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

public class Program
{
    static int Main(string[] args)
    {
        try
        {
            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = "10.33.49.220",
                UserName = "smartlidar",
                Password = "molas",
                SshHostKeyFingerprint = "ssh-ed25519 255 5DqCBnNs/7ywwkEK/UG0zKMtlxzQ+NhviwzZNZtC39Y"
            };


            // Obtain source and destination information from config file. Write on the manual the correct formating of this file.
            // First line contains temp folder, second line contains backup folder
            string[] Config = System.IO.File.ReadAllLines(@"C:\Users\guili\Desktop\config.txt"); // This needs to be adjusted on the other laptop
            //config[0] is the local temp path
            //config[1] is the local backup path


            using (Session session = new Session())
            {
                // Connect
                session.Open(sessionOptions);

                const string remotePath = "/media/molasext/OpenFast";  // This will remain the same, no need to adjust in the other laptop

                // Get list of files in the directory
                RemoteDirectoryInfo directoryInfo = session.ListDirectory(remotePath); // here we have a list of all files in the remote directory
                //int NumFilesRemoteDir = directoryInfo.Files.Count; // Number of files inside the remote dir

                // Select the most recent file
                RemoteFileInfo latest =
                    directoryInfo.Files
                        .Where(file => !file.IsDirectory)
                        .OrderByDescending(file => file.LastWriteTime)
                        .FirstOrDefault();

                // Any file at all?
                if (latest == null)
                {
                    throw new Exception("No file found");
                }
                // Download the selected file
                session.GetFileToDirectory(latest.FullName, Config[0]);



                // Move file locally to backup folder after checking if it already exists. 



                // Obtain file path and name in temp directory:
                string[] FilePathTemp = Directory.GetFiles(Config[0]);
                // Add condition if the folder is empty
                if (string.IsNullOrEmpty(FilePathTemp[0]))
                {
                    throw new Exception("File could not be downloaded");
                }
                string FileNameTemp = Path.GetFileName(FilePathTemp[0]); // Extract file name

                //Obtain files list in backup directory:
                string[] FilePathBackup = Directory.GetFiles(Config[1]);

                // Check if file already exists in backup directory by comparing the strings
                int NumFilesBackup = FilePathBackup.Length;
                int cont = 0;
                for (int iBackup = 0; iBackup < NumFilesBackup; iBackup++)
                {
                    string FileNameBackup = Path.GetFileName(FilePathBackup[iBackup]);
                    if (FileNameBackup == FileNameTemp)
                    {
                        cont++;
                    }
                }

                // Create full destination path of the file
                string FullDestination = Config[1] + "\\" + FileNameTemp;
                if (cont == 0) // Means that the file is not there
                {
                    File.Copy(FilePathTemp[0], FullDestination, true);
                }
                // Delete file from temp file after copying it, or if it already exists in the backup folder
                File.Delete(FilePathTemp[0]);
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
            return 1;
        }
    }
}

