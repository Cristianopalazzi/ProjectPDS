
using System.IO.Pipes;
using System.IO;


namespace FileNameSender
{
    class Program
    {
        static void Main(string[] args)
        {
            NamedPipeClientStream pipeClient = null;
            StreamWriter sw = null;
            try
            {
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
                if (pipeClient != null)
                    pipeClient.Close();
                if (sw != null)
                    sw.Close();

            }
        }
    }
}