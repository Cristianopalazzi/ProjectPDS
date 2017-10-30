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
            threadPipe = new Thread(listenOnPipe);
            threadPipe.Start();
            waitOnTake = new Thread(listenOnQueue);
            waitOnTake.Start();
            
        }

        private void listenOnPipe()
        {
            FilesToSend fils = new FilesToSend();
            NamedPipeServerStream pipeServer =
                      new NamedPipeServerStream("testpipe", PipeDirection.In);
            
            while (true)
            {
                Console.WriteLine("Waiting for client connection...");
                pipeServer.WaitForConnection();
                Console.WriteLine("Client connected.");
                StreamReader sr = new StreamReader(pipeServer);
                string file = sr.ReadLine();
              
                Console.WriteLine(file);
                Console.WriteLine("aperto form");
                NeighborSelection form = new NeighborSelection();
                form.Text = "Condividi " + file + " con ";
                form.Focus();
                form.ShowDialog();
                ArrayList array = form.getSelectedNames();
                if (array.Count != 0)
                {
                    Work w = new Work(file, array);
                    filesToSend.Add(w);
                    fils.AddFile(w);
                }
                fils.ShowDialog();
                form.Dispose();
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
                int receivers = w.Receivers.Count;
                Sender s = new Sender();
                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < receivers; i++)
                {
                    string receiver = (string)w.Receivers[i];
                    Thread t = new Thread(() =>
                    {
                        s.sendFile(receiver, w.FileName);
                    });
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
        
        
    }
}