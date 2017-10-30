using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using System.IO;

namespace ProjectPDS
{
    public partial class FilesToSend : Form
    {
        public FilesToSend()
        {
            InitializeComponent();
            Sender.updateProgress += updateProgressBar;
        }

    
        private void button_click(object sender, EventArgs e)
        {
            MessageBox.Show("caramelle");
        }

        public void AddFile(Work w)
        {
            FileProgress fp = new FileProgress(w.FileName);
            TableLayoutPanel esterno = new TableLayoutPanel();
            esterno.AutoSize = true;
            esterno.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            esterno.Dock = DockStyle.Top;
            esterno.RowCount = 2;
            esterno.ColumnCount = 1;

            //Pannello con il nome del file
            Panel p = new Panel();
            p.BackColor = Color.Turquoise;
            p.AutoSize = true;
            p.Padding = new Padding(2);
            p.Dock = DockStyle.Top;
            p.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            //Label per il nome del file
            Label l = new Label();
            l.ForeColor = Color.White;
            l.AutoSize = true;
            l.Padding = new Padding(3);
            l.Text = "Invio in corso di  " + w.FileName;

            //table singolo file
            TableLayoutPanel tp = new TableLayoutPanel();
            tp.AutoSize = true;
            tp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tp.Dock = DockStyle.Top;
            tp.Padding = new Padding(3);
            tp.Location = new Point(0, 0);


            //for per riempire il table 
            int i = 0;
            foreach (var u in w.Receivers)
            {
                string name = np.getUserFromIp(u.ToString());
                string nameFoto = name + "@" + u.ToString();

                byte[] foto;
                np.getNeighbors().TryGetValue(nameFoto, out foto);
                //dichiaro pannello label + foto
                Panel pInternal = new Panel();
                pInternal.AutoSize = true;
                pInternal.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                pInternal.Anchor = AnchorStyles.Top;
                pInternal.Padding = new Padding(2);
                PictureBox pic = new PictureBox();
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Size = new Size(64, 64);
                pic.Location = new Point(2, 0);

                //potrebbe diventare un metodo
                var ms = new MemoryStream(foto);
                Image im = Image.FromStream(ms);
                pic.Image = im;
                ms.Close();
                

                pInternal.Controls.Add(pic);
                pInternal.Controls.Add(new Label() { AutoEllipsis = true, AutoSize = true, Anchor = AnchorStyles.Left, Text = name, Location = new Point(2, 72) });

                //dichiaro bottone
                Button b = new Button();
                b.AutoSize = true;
                b.Anchor = AnchorStyles.Left;
                b.Click += this.button_click;
                b.Text = "Annulla";

                //aggiungo pannello, progressbar, bottone
                tp.Controls.Add(pInternal, 0, i);
                ProgressBar barra = new ProgressBar();
                barra.Size = new Size(300, 20);
                barra.Anchor = AnchorStyles.Left;

                tp.Controls.Add(barra, 1, i);
                tp.Controls.Add(b, 2, i);
                fp.Elements.Add(name, barra);
                i++;
            }

            //aggiungo la label al pannello
            p.Controls.Add(l);
            //aggiungo il pannello e la tabella interna alla tabella esterna
            esterno.Controls.Add(p, 0, 0);
            esterno.Controls.Add(tp, 0, 1);
            //aggiungo la tabella esterna al form originale
            tableLayoutPanel1.Controls.Add(esterno);
            files.Add(fp);
        }

        public void updateProgressBar(string filename,string receiver,int percentage)
        {
            string receiverName = np.getUserFromIp(receiver);
            foreach(FileProgress file in files)
            {
                string originale = Path.GetFileName(file.FileName);
                if (String.Compare(originale,filename)== 0 )
                {
                    ProgressBar p;

                    // p.Invoke((MethodInvoker)delegate
                    //{
                    //    file.Elements.TryGetValue(receiverName, out p);
                    //    if (p != null)
                    //        p.Value = percentage;
                    //});
                    file.Elements.TryGetValue(receiverName, out p);
                    p.Value = percentage;
                }
            }
        }


  
        Dictionary<String, Thread> dic = new Dictionary<string, Thread>();
        NeighborProtocol np = NeighborProtocol.getInstance;
        ArrayList files = new ArrayList();

    }
}
