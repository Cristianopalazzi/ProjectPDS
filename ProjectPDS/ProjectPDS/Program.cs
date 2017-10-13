using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Compression;
using System.IO;

namespace ProjectPDS
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //NeighborProtocol n = new NeighborProtocol();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //Sender ss = new Sender();
            //ss.sendFile("192.168.1.1", "C:\\Users\\Cristiano\\Desktop\\pass.txt");
            Receiver r = new Receiver();
            //ZipFile.CreateFromDirectory("C:\\Users\\Cristiano\\Desktop\\gennarone",
            //    "C:\\Users\\Cristiano\\Desktop\\gennarone.zip", CompressionLevel.Optimal, true);

            //ZipFile.ExtractToDirectory("C:\\Users\\Cristiano\\Desktop\\gennarone.zip",
            //    "C:\\Users\\Cristiano\\Desktop");
        }
    }
}
