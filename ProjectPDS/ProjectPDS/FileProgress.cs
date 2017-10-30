using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ProjectPDS
{
    class FileProgress
    {
        
        
        private string fileName;
        public string FileName { get => fileName; set => fileName = value; }
        private Dictionary<string, ProgressBar> elements;
        public Dictionary<string, ProgressBar> Elements { get => elements; set => elements = value; }

        public FileProgress(Work w )
        {
            FileName = w.FileName;
            NeighborProtocol np = NeighborProtocol.getInstance;
            foreach(var v in w.Receivers)
            {
                ProgressBar p = new ProgressBar();
                p.Size = new Size(300, 20);
                p.Anchor = AnchorStyles.Left;
                elements.Add(np.getUserFromIp(v.ToString()), p);
            }
        }

        public FileProgress(string name)
        {
            fileName = name;
            Elements = new Dictionary<string, ProgressBar>();
        }
    }
}
