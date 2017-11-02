using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Compression;
using System.IO;
using System.Collections;

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


            Settings instance = Settings.getInstance;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Receiver r = new Receiver();
            NeighborProtocol n = NeighborProtocol.getInstance;
            MyQueue queue = new MyQueue();



        }
    }
}
