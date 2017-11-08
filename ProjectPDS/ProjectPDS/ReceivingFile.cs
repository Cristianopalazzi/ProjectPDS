using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectPDS
{
    public class ReceivingFile
    {
        private string username,filename,ipaddr;
        private ProgressBar progress;
        private PictureBox picProgress;
        private int index;

        public ReceivingFile(string username,string filename,String ipaddr)
        {
            this.username = username;
            this.filename = filename;
            this.ipaddr = ipaddr;
            progress = new ProgressBar();
            Progress.Size = new System.Drawing.Size(300, 20); //TODO attenzione alle dimensioni fisse della progbar
            Progress.Anchor = AnchorStyles.Left;
            Progress.Padding = new Padding(3);
            picProgress = new PictureBox();
            PicProgress.Size = new System.Drawing.Size(32, 32);
            PicProgress.Anchor = AnchorStyles.Left;
        }

     
        public string Username { get => username; }
        public string Filename { get => filename; }
        public ProgressBar Progress { get => progress; }
        public PictureBox PicProgress { get => picProgress; }
        public string Ipaddr { get => ipaddr; }
    }
}