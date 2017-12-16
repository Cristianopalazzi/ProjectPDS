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
            try
            {
                String appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                Process[] p = Process.GetProcessesByName("EasyShare");
                if (p.Length == 0)
                    Process.Start(appPath + "\\EasyShare.exe");
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
                if (sw != null)
                    sw.Close();
            }

        }
    }
}