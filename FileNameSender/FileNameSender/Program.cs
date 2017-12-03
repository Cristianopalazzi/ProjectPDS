
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System;

namespace FileNameSender
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            NamedPipeClientStream pipeClient = null;
            StreamWriter sw = null;
            try
            {
                //TODO aggiungere apertura processo
                Process[] p = Process.GetProcessesByName("ProjectPDSWPF");
                if (p.Length == 0)
                {
                    Process.Start(Environment.CurrentDirectory + "\\ProjectPDSWPF.exe");
                    Thread.Sleep(250);
                }
                pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.Out);
                pipeClient.Connect(3000);
                sw = new StreamWriter(pipeClient);
                sw.WriteLine(args[0]);
                sw.Flush();
            }
            catch
            {
                //Notifica o qualcosa di visuale per dire all'utente che la nostra app è chiusa.
            }
            finally
            {
               
                if (sw != null)
                    sw.Close();
                if (pipeClient != null)
                    pipeClient.Close();
            }
        }
    }
}