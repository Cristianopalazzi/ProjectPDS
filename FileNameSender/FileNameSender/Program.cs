using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;

namespace FileNameSender
{
    class Program
    {
        static void Main(string[] args)
        {
            NamedPipeClientStream pipeClient =
                new NamedPipeClientStream(".", "testpipe", PipeDirection.Out);
            pipeClient.Connect();
            StreamWriter sw = new StreamWriter(pipeClient);
            sw.WriteLine(args[0]);
            sw.Flush();
            pipeClient.Close();
        }
    }
}
