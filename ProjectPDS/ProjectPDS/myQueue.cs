using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;

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

                //TODO far aggiungere i tizi a cui mandare il file ( dall'interfaccia)
                ArrayList array = new ArrayList();
                array.Add("192.168.1.133");
                array.Add("192.168.1.133");
                Work w = new Work(file, array);
                filesToSend.Add(w);

                Thread.Sleep(3000);
                filesToSend.Add(w);
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
                int receiversNumber = w.Receivers.Count;
                Sender s = new Sender();
                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < w.Receivers.Count; i++)
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
        BlockingCollection<Work> filesToSend;
        private Thread threadPipe, waitOnTake;
    }
}