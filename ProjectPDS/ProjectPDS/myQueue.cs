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
            threadPipe = new Thread(listenOnPipe);
            threadPipe.Start();
            waitOnTake = new Thread(prova);
            waitOnTake.Start();
            filesToSend = new BlockingCollection<Work>();
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
                array.Add("receiver1");
                array.Add("receiver2");
                Work w = new Work(file, array);
                filesToSend.Add(w);
                pipeServer.Disconnect();

            }
        }

        ~MyQueue() { threadPipe.Join(); waitOnTake.Join(); }

        public void prova()
        {
            while (true)
            {
                //TODO farlo con i thread, non con i task
                Console.WriteLine("Aspetto il prossimo work");
                Work w = filesToSend.Take();
                int receiversNumber = w.Receivers.Count;
                Task[] tasks = new Task[receiversNumber];
                for (int i = 0; i < receiversNumber; i++)
                {
                    string receiver = (string)w.Receivers[i];
                    tasks[i] = Task.Run(() =>
                    {
                        //sendFile (w.fileName, receiver)
                        Console.WriteLine("Task numero {0} ha terminato ", Task.CurrentId.Value);
                    });
                }

                //Aspetto tutti i task
                Task.WaitAll(tasks);
                Console.WriteLine("Tutti i task conclusi");

                //List<Thread> threads = new List<Thread>();
                //for (int i = 0; i < 3; i++)
                //{
                //    string receiver = (string)w.Receivers[i];
                //    Thread t = new Thread(() =>
                //    {
                //        //sendFile(w.fileName, receiver)
                //    });
                //    t.Start();
                //    threads.Add(t);
                //}
                //foreach (var t in threads)
                //    t.Join();
                //Console.WriteLine("Finiti tutti");
            }
        }
        BlockingCollection<Work> filesToSend;
        private Thread threadPipe, waitOnTake;

    }
}