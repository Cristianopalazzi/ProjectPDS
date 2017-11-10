using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectPDS
{
    public partial class ReceivingFiles : Form
    {
        public ReceivingFiles()
        {
            InitializeComponent();
            Receiver.updateProgress += updateProgressBar;
        }


        public int addFile(ReceivingFile r)
        {
            files.Add(r);
            //Pannello per definire gli spazi di ogni pezzo
            TableLayoutPanel riga = new TableLayoutPanel();
            riga.AutoSize = true;
            riga.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            riga.ColumnCount = 3;
            riga.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            riga.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            riga.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            //pannello che contiene nome e immagine del tizio
            Panel nomeImmagine = new Panel();
            nomeImmagine.Dock = DockStyle.Fill;
            //nomeImmagine.Size = new Size(96,96);
            nomeImmagine.BackColor = Color.Azure;

            PictureBox immagine = new PictureBox();
            immagine.Size = new Size(75,75);
            immagine.Location = new Point(10,2);
            immagine.SizeMode = PictureBoxSizeMode.StretchImage;
            immagine.Image = getFoto(r.Username, r.Ipaddr);

            Label nomeUser = new Label();
            nomeUser.AutoEllipsis = true;
            nomeUser.AutoSize = true;
            nomeUser.Dock = DockStyle.Bottom;
            nomeUser.TextAlign = ContentAlignment.MiddleCenter;
            nomeUser.Text = r.Username;





            nomeImmagine.Controls.Add(nomeUser);
            nomeImmagine.Controls.Add(immagine);

            //pannello che contiene nomefile + progressbar
            Panel fileBarra = new Panel();
            fileBarra.Padding = new Padding(5);
            fileBarra.Anchor = AnchorStyles.Right;
            fileBarra.Size = new Size(350, 96);
            fileBarra.BackColor = Color.Aqua;

            Label nomeFile = new Label();
            nomeFile.AutoSize = true;
            nomeFile.Text = r.Filename;
            nomeFile.Padding = new Padding(3);

            fileBarra.Controls.Add(nomeFile);
            fileBarra.Controls.Add(r.Progress);
            //aggiungo i pannelli e il box finale alla riga
            riga.Controls.Add(nomeImmagine);
            riga.Controls.Add(fileBarra);
            riga.Controls.Add(r.PicProgress);
            //aggiungo la riga alla struttura esterna
            tableLayoutPanel1.Controls.Add(riga);
            //TODO tLP.ROWCOUNT modificare? 
            return files.IndexOf(r);

        }





        private Image getFoto(string username, string ipaddr)
        {
            byte[] foto;
            //TODO aggiungere placeholder se non ritorna niente
            np.getNeighbors().TryGetValue(username + "@" + ipaddr, out foto);
            if (foto != null)
            {
                var ms = new MemoryStream(foto);
                Image im = Image.FromStream(ms);
                ms.Close();
                return im;
            }
            return null;
        }

        public void updateProgressBar(int index, int percentage)
        {
            ((ReceivingFile)files[index]).Progress.Value = percentage;
        }


        private ArrayList files = new ArrayList();
        private NeighborProtocol np = NeighborProtocol.getInstance;
    }
}
