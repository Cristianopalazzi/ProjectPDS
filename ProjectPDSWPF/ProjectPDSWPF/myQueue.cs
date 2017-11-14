using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Linq;
using System.Windows;

namespace ProjectPDSWPF
{
    class MyQueue
    {
        public MyQueue()
        {
            MainWindow.sendSelectedNeighbors += receive_selected_neighbors;
            filesToSend = new BlockingCollection<List<SendingFile>>();
            threadPipe = new Thread(listenOnPipe)
            {
                Name = "ThreadPipe"
            };
            threadPipe.Start();
            waitOnTake = new Thread(listenOnQueue)
            {
                Name = "waitOnTake"
            };
            waitOnTake.Start();
        }

        private void listenOnPipe()
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.In);
            while (true)
            {
                Console.WriteLine("Waiting for client connection...");
                pipeServer.WaitForConnection();
                Console.WriteLine("Client connected.");
                StreamReader sr = new StreamReader(pipeServer);
                string file = sr.ReadLine();
                Console.WriteLine(file);
                openNeighbors(file);
                pipeServer.Disconnect();
            }
        }

        ~MyQueue() { threadPipe.Join(); waitOnTake.Join(); }

        public void listenOnQueue()
        {
            while (true)
            {
                Console.WriteLine("Aspetto il prossimo work");
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
                        Name = "thread che manda " + s.FileName + " a  " + s.Name
                    };
                    t.Start();
                    threads.Add(t);
                }
                foreach (var t in threads)
                    t.Join();
                Console.WriteLine("Finiti tutti");
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