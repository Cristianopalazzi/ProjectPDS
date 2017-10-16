using System;
using System.Collections.Generic;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System.Collections;

namespace ProjectPDS
{
    class MyQueue
    {
        public MyQueue()
        {
            threadPipe = new Thread(listenOnPipe);
            threadPipe.Start();
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
                pipeServer.Disconnect();
            }
        }

        ~MyQueue() { threadPipe.Join(); }
        //TODO blockingCollections
        private Thread threadPipe;
    }
}
