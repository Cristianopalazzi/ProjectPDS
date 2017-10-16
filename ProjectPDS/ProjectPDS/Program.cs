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
            //Receiver r = new Receiver();
            MyQueue queue = new MyQueue();
            //NeighborProtocol n = new NeighborProtocol();
        }
    }
}
