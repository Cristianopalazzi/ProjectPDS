﻿using System.Collections.Generic;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System.Collections.Concurrent;
using System.IO.Compression;
using System;

namespace EasyShare
{
    class MyQueue
    {
        public MyQueue()
        {
            NeighborSelection.sendSelectedNeighbors += receive_selected_neighbors;
            filesToSend = new BlockingCollection<List<SendingFile>>();
            cachingFiles = new Dictionary<string, ZipInfo>();
            threadPipe = new Thread(listenOnPipe)
            {
                Name = "ThreadPipe",
                IsBackground = true
            };
            threadPipe.Start();
            waitOnTake = new Thread(listenOnQueue)
            {
                Name = "waitOnTake",
                IsBackground = true
            };
            waitOnTake.Start();
        }

        private void listenOnPipe()
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
                    openNeighbors(file);
                }
            }
            catch
            {
                if (sr != null)
                    sr.Close();
                listenOnPipe();
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        ~MyQueue() { threadPipe.Join(); waitOnTake.Join(); }

        public void listenOnQueue()
        {
            while (true)
            {
                List<SendingFile> sf = filesToSend.Take();
                ZipInfo zipInfo;
                string pathFile = sf[0].FileName;

                if (!cachingFiles.ContainsKey(pathFile))
                    zipInfo = createZip(pathFile);

                else
                {
                    if (fileChanged(pathFile))
                    {
                        cachingFiles.Remove(pathFile);
                        zipInfo = createZip(pathFile);
                    }
                    else zipInfo = cachingFiles[pathFile];
                }

                Sender sender = new Sender();
                List<Thread> threads = new List<Thread>();
                foreach (SendingFile s in sf)
                {
                    Thread t = new Thread(() =>
                    {
                        sender.sendFile(s.IpAddr, s.FileName, s.Sock, zipInfo);
                    })
                    {
                        Name = "thread che manda " + s.FileName + " a  " + s.Name,
                        IsBackground = true
                    };
                    t.Start();
                    threads.Add(t);
                }
            }
        }

        private void receive_selected_neighbors(List<SendingFile> sendingFiles)
        {
            if (sendingFiles != null)
                filesToSend.Add(sendingFiles);
        }

        private ZipInfo createZip(string pathFile)
        {
            string zipToSend = RandomStr() + Constants.ZIP_EXTENSION;
            FileAttributes attr = File.GetAttributes(pathFile);
            string zipLocation = App.defaultFolder + "\\" + zipToSend;
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

        //TODO togliere la cancellazione degli zip
        private bool fileChanged(string pathFile)
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
        public static event myDel openNeighbors;
    }
}