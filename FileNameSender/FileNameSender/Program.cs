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
            bool connected = false;
            Mutex m = new Mutex(true, "myMutex", out bool created);
            try
            {
                String appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                Process[] p = Process.GetProcessesByName("EasyShare");

                if (created)
                {
                    if (p.Length == 0)
                        Process.Start(appPath + "\\EasyShare.exe");
                }
                else
                    m.WaitOne();
                while (!connected)
                {
                    try
                    {
                        pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.Out);
                        pipeClient.Connect(500);
                        connected = true;
                        sw = new StreamWriter(pipeClient);
                        sw.WriteLine(args[0]);
                        sw.Flush();
                    }
                    catch (TimeoutException te)
                    {
                        if (sw != null)
                            sw.Close();
                        continue;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                m.ReleaseMutex();
                if (sw != null)
                    sw.Close();
            }

        }
    }
}