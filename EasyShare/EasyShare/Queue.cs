using System.Collections.Generic;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System.Collections.Concurrent;
using System.IO.Compression;
using System;
using System.Net.Sockets;

namespace EasyShare
{
    class Queue
    {
        public Queue()
        {
            NeighborSelection.SendSelectedNeighbors += Receive_selected_neighbors;
            filesToSend = new BlockingCollection<List<SendingFile>>();
            cachingFiles = new Dictionary<string, ZipInfo>();
            threadPipe = new Thread(ListenOnPipe)
            {
                Name = "ThreadPipe",
                IsBackground = true
            };
            threadPipe.Start();
            waitOnTake = new Thread(ListenOnQueue)
            {
                Name = "waitOnTake",
                IsBackground = true
            };
            waitOnTake.Start();
        }

        private void ListenOnPipe()
        {
            NamedPipeServerStream pipeServer = null;
            StreamReader sr = null;
            try
            {
                pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.In);
                sr = new StreamReader(pipeServer);
                while (true)
                {
                    pipeServer.WaitForConnection();
                    string file = sr.ReadLine();
                    if (pipeServer.IsConnected)
                        pipeServer.Disconnect();
                    OpenNeighbors?.Invoke(file);
                }
            }
            catch
            {
                if (sr != null)
                    sr.Close();
                ListenOnPipe();
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public void ListenOnQueue()
        {
            while (true)
            {
                List<SendingFile> sf = filesToSend.Take();
                ZipInfo zipInfo;
                string pathFile = sf[0].FileName;

                if (!cachingFiles.ContainsKey(pathFile))
                    zipInfo = CreateZip(pathFile);

                else
                {
                    if (FileChanged(pathFile))
                    {
                        cachingFiles.Remove(pathFile);
                        zipInfo = CreateZip(pathFile);
                    }
                    else zipInfo = cachingFiles[pathFile];
                }

                if(zipInfo == null)
                {
                    QueueUpdateState(sf[0].Sock, Constants.FILE_STATE.ERROR);
                    QueueBalloon(Path.GetFileName(pathFile), null, Constants.NOTIFICATION_STATE.FILE_ERROR_SEND); 
                    continue;
                }

                Sender sender = new Sender();
                foreach (SendingFile s in sf)
                {
                    Thread t = new Thread(() =>
                    {
                        sender.SendFile(s.IpAddr, s.FileName, s.Sock, zipInfo);
                    })
                    {
                        Name = "thread che manda " + s.FileName + " a  " + s.Name,
                        IsBackground = true 
                    };
                    t.Start();
                }
            }
        }

        private void Receive_selected_neighbors(List<SendingFile> sendingFiles)
        {
            if (sendingFiles != null)
                filesToSend.Add(sendingFiles);
        }

        private ZipInfo CreateZip(string pathFile)
        {
            string zipLocation = String.Empty;
            try
            {


                string zipToSend = RandomStr() + Constants.ZIP_EXTENSION;
                FileAttributes attr = File.GetAttributes(pathFile);
                zipLocation = App.defaultFolder + "\\" + zipToSend;
                bool isFile;
                DateTime lastWrite;

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    ZipFile.CreateFromDirectory(pathFile, zipLocation, CompressionLevel.NoCompression, false);
                    isFile = false;
                    DirectoryInfo dInfo = new DirectoryInfo(pathFile);
                    lastWrite = LastestModified(dInfo, dInfo.LastWriteTime);
                }
                else
                {
                    ZipArchive newFile = ZipFile.Open(zipLocation, ZipArchiveMode.Create);
                    newFile.CreateEntryFromFile(pathFile, Path.GetFileName(pathFile), CompressionLevel.NoCompression);
                    newFile.Dispose();
                    FileInfo fInfo = new FileInfo(pathFile);
                    isFile = true;
                    lastWrite = fInfo.LastWriteTime;
                }
                long zipLength = new FileInfo(zipLocation).Length;
                ZipInfo zipInfo = new ZipInfo(zipToSend, zipLocation, zipLength, isFile, lastWrite);
                cachingFiles.Add(pathFile, zipInfo);
                return zipInfo;
            }
            catch 
            {
                if (!String.IsNullOrEmpty(zipLocation))
                    File.Delete(zipLocation);
                return null;
            }
            
        }

        private bool FileChanged(string pathFile)
        {
            FileAttributes attr = File.GetAttributes(pathFile);

            if (!cachingFiles.TryGetValue(pathFile, out ZipInfo zipInfo))
                return false;

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo dInfo = new DirectoryInfo(pathFile);
                if (LastestModified(dInfo, zipInfo.LastWrite) > zipInfo.LastWrite)
                    return true;
            }
            else
            {
                if (new FileInfo(pathFile).LastWriteTime > zipInfo.LastWrite)
                    return true;
            }
            return false;
        }


        private string RandomStr()
        {
            string rStr = Path.GetRandomFileName();
            rStr = rStr.Replace(".", "");
            return rStr;
        }


        private DateTime LastestModified(DirectoryInfo d, DateTime last)
        {
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
                if (fi.LastWriteTime > last)
                    last = fi.LastWriteTime;

            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                if (di.LastWriteTime > last)
                    last = di.LastWriteTime;
                last = LastestModified(di, last);
            }
            return last;
        }

        private BlockingCollection<List<SendingFile>> filesToSend;
        private Dictionary<string, ZipInfo> cachingFiles;
        private Thread threadPipe, waitOnTake;
        public delegate void myDel(string file);
        public static event myDel OpenNeighbors;
        public delegate void myDel2(string fileName, string userName, Constants.NOTIFICATION_STATE state);
        public static event myDel2 QueueBalloon;
        public delegate void myDel3(Socket s, Constants.FILE_STATE state);
        public static event myDel3 QueueUpdateState;
    }
}