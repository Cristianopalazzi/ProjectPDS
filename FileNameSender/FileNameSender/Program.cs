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
                String appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                Process[] p = Process.GetProcessesByName("EasyShare");
                if (p.Length == 0)
                {
                    Process.Start(appPath + "\\EasyShare.exe");
                    Thread.Sleep(1000);
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
                Console.ReadLine();
                if (sw != null)
                    sw.Close();
            }
        }
    }
}