using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Drawing;

namespace ProjectPDS
{
    public class Work
    {
        public Work(string fileName, ArrayList receivers)
        {
            this.fileName = fileName;
            this.sendingFiles = new ArrayList();
            foreach (string variable in receivers)
            {
                int index = variable.LastIndexOf("@");
                string name = variable.Substring(0, index);
                string ipaddr = variable.Substring(index + 1);
                SendingFile sf = new SendingFile(ipaddr, name);
                sendingFiles.Add(sf);
            }
        }

        public string FileName { get => fileName; set => fileName = value; }
        public ArrayList SendingFiles { get => sendingFiles; set => sendingFiles = value; }

        private string fileName;
        private ArrayList sendingFiles;
    }



    public class SendingFile
    {


        public SendingFile(string ipAddr, string name)
        {
            this.IpAddr = ipAddr;
            this.Name = name;
            Progress = new ProgressBar();
            Progress.Size = new System.Drawing.Size(300, 20); //TODO attenzione alle dimensioni fisse della progbar
            Progress.Anchor = AnchorStyles.Left;
          
            Sock = new Socket(AddressFamily.InterNetwork,
              SocketType.Stream, ProtocolType.Tcp);
        }

        public void progChange(int percent)
        {
           
            using (Graphics gr = progress.CreateGraphics())
            {
                gr.DrawString(percent.ToString() + "%",
                    SystemFonts.DefaultFont,
                    Brushes.Black,
                    new PointF(progress.Width / 2 - (gr.MeasureString(percent.ToString() + "%",
                        SystemFonts.DefaultFont).Width / 2.0F),
                    progress.Height / 2 - (gr.MeasureString(percent.ToString() + "%",
                        SystemFonts.DefaultFont).Height / 2.0F)));
            }
        }
        public string IpAddr { get => ipAddr; set => ipAddr = value; }
        public string Name { get => name; set => name = value; }
        public ProgressBar Progress { get => progress; set => progress = value; }
        public Socket Sock { get => sock; set => sock = value; }
        private string ipAddr, name;
        private ProgressBar progress;
        private Socket sock;
    }
}
