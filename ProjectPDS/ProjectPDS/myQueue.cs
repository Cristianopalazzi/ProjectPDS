using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace ProjectPDS
{
    class MyQueue
    {

        public MyQueue()
        {
            filesToSend = new BlockingCollection<Work>();
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
            form = new MainForm();
            form.Show();
            Application.Run();
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
                Work w = null;
                NeighborSelection formUsers = new NeighborSelection
                {
                    Text = "Condividi " + file + " con "
                };
                formUsers.Focus();
                formUsers.ShowDialog();

                ArrayList array = formUsers.getSelectedNames();
                if (array.Count != 0)
                {
                    w = new Work(file, array);
                    filesToSend.Add(w);
                    form.BeginInvoke((MethodInvoker)delegate
                    {
                        form.apriFileToSend(w);
                    });
                }
                pipeServer.Disconnect();
            }
        }

        ~MyQueue() { threadPipe.Join(); waitOnTake.Join(); }

        public void listenOnQueue()
        {
            while (true)
            {
                Console.WriteLine("Aspetto il prossimo work");
                Work w = filesToSend.Take();
                int receivers = w.SendingFiles.Count;
                Sender s = new Sender();
                List<Thread> threads = new List<Thread>();
                foreach (SendingFile sf in w.SendingFiles)
                {
                    Thread t = new Thread(() =>
                    {
                        s.sendFile(sf.IpAddr, w.FileName, sf.Sock);
                    })
                    {
                        Name = "thread che manda " + w.FileName + " a  " + sf.Name
                    };
                    t.Start();
                    threads.Add(t);
                    
                }
                foreach (var t in threads)
                    t.Join();
                Console.WriteLine("Finiti tutti");
            }
        }

     


        private BlockingCollection<Work> filesToSend;
        private Thread threadPipe, waitOnTake;
        private MainForm form;

        
    }
}