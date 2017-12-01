using System.Collections.Generic;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System.Collections.Concurrent;

namespace ProjectPDSWPF
{
    class MyQueue
    {
        public MyQueue()
        {
            NeighborSelection.sendSelectedNeighbors += receive_selected_neighbors;
            filesToSend = new BlockingCollection<List<SendingFile>>();
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

                while (true)
                {
                    pipeServer.WaitForConnection();
                    sr = new StreamReader(pipeServer);
                    string file = sr.ReadLine();
                    sr.Close();
                    openNeighbors(file);
                    pipeServer.Disconnect();
                }
            }
            catch
            {
                if (sr != null)
                    sr.Close();
                if (pipeServer != null)
                    pipeServer.Close();
                listenOnPipe();
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (pipeServer != null)
                    pipeServer.Close();
            }
        }

        ~MyQueue() { threadPipe.Join(); waitOnTake.Join(); }

        public void listenOnQueue()
        {
            while (true)
            {
                List<SendingFile> sf = filesToSend.Take();
                Sender sender = new Sender();
                List<Thread> threads = new List<Thread>();
                foreach (SendingFile s in sf)
                {
                    Thread t = new Thread(() =>
                    {
                        sender.sendFile(s.IpAddr, s.FileName, s.Sock);
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

        private BlockingCollection<List<SendingFile>> filesToSend;
        private Thread threadPipe, waitOnTake;
        public delegate void myDel(string file);
        public static event myDel openNeighbors;
    }
}