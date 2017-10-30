using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace ProjectPDS
{
    public partial class NeighborSelection : Form
    {
        public NeighborSelection()
        {
            InitializeComponent();
            InizializeInterface();
        }


        public ArrayList getSelectedNames()
        {
           
            ArrayList values = new ArrayList();
            foreach (var item in listView1.Items.Cast<ListViewItem>())
                if (item.Selected)
                    values.Add(item.Tag.ToString());
            return values;
        }


        private void InizializeInterface()
        {
            ImageList iml = new ImageList();
            iml.ImageSize = new Size(64, 64);
            
            listView1.LargeImageList = iml;
            foreach (var item in np.getNeighbors())
            {
                
                var listItem = listView1.Items.Add(np.getUserFromIp(item.Key));
                listItem.Tag = item.Key.Substring(item.Key.LastIndexOf("@") + 1);
                var ms = new MemoryStream(item.Value);
                iml.Images.Add(item.Key,Image.FromStream(ms));
                listItem.ImageKey = item.Key;

                var listItem2 = listView1.Items.Add(np.getUserFromIp(item.Key) + " b");
                listItem2.ImageKey = item.Key;
                listItem2.Tag = item.Key.Substring(item.Key.LastIndexOf("@") + 1);

                var listItem3 = listView1.Items.Add(np.getUserFromIp(item.Key) + " C");
                listItem3.ImageKey = item.Key;
                listItem3.Tag = item.Key.Substring(item.Key.LastIndexOf("@") + 1);

            }
        }
        NeighborProtocol np = NeighborProtocol.getInstance;

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
