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
using System.Net.Sockets;
using System.Collections.Concurrent;

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
            Socket s = (Socket)((Button)sender).Tag;
            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException exc)
            {
                MessageBox.Show(exc.ToString());
            }
            finally
            {
                sockProg.TryRemove(s, out ProgressBar p);
            }
        }

        public void AddFile(Work w)
        {
            files.Add(w);
            
            TableLayoutPanel esterno = new TableLayoutPanel();
            esterno.AutoSize = true;
            esterno.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            esterno.Dock = DockStyle.Top;
            esterno.RowCount = 2;
            esterno.ColumnCount = 1;

            //Pannello con il nome del file
            Panel p = new Panel
            {
                BackColor = Color.Turquoise,
                AutoSize = true,
                Padding = new Padding(2),
                Dock = DockStyle.Top,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            //Label per il nome del file
            Label l = new Label
            {
                ForeColor = Color.White,
                AutoSize = true,
                Padding = new Padding(3),
                Text = "Invio in corso di  " + w.FileName
            };

            //table singolo file
            TableLayoutPanel tp = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Padding = new Padding(3),
                Location = new Point(0, 0),
                ColumnCount = 3
            };


            //for per riempire il table 
            int i = 0;
            foreach (SendingFile singleFile in w.SendingFiles)
            {
                sockProg.TryAdd(singleFile.Sock, singleFile.Progress);
                string nameFoto = singleFile.Name + "@" + singleFile.IpAddr;

                byte[] foto;
                np.getNeighbors().TryGetValue(nameFoto, out foto);
                //dichiaro pannello label + foto
                Panel pInternal = new Panel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Anchor = AnchorStyles.Top,
                    Padding = new Padding(2)
                };
                PictureBox pic = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Size = new Size(64, 64),
                    Location = new Point(2, 0)
                };

                //potrebbe diventare un metodo
                var ms = new MemoryStream(foto);
                Image im = Image.FromStream(ms);
                pic.Image = im;
                ms.Close();

                pInternal.Controls.Add(pic);
                pInternal.Controls.Add(new Label() { AutoEllipsis = true, AutoSize = true, Anchor = AnchorStyles.Left, Text = singleFile.Name, Location = new Point(2, 72) });

                //dichiaro bottone
                Button b = new Button
                {
                    AutoSize = true,
                    Tag = singleFile.Sock,
                    Anchor = AnchorStyles.Left,
                    Text = "Annulla"
                };
                b.Click += button_click;

                //aggiungo pannello, progressbar, bottone
                tp.Controls.Add(pInternal, 0, i);


                tp.Controls.Add(singleFile.Progress, 1, i);
                tp.Controls.Add(b, 2, i);
                tp.RowCount++;
                i++;
            }

            //aggiungo la label al pannello
            p.Controls.Add(l);
            //aggiungo il pannello e la tabella interna alla tabella esterna
            esterno.Controls.Add(p, 0, 0);
            esterno.Controls.Add(tp, 0, 1);
            //aggiungo la tabella esterna al form originale
            tableLayoutPanel1.Controls.Add(esterno);
            tableLayoutPanel1.RowCount++;
        }

        public void updateProgressBar(string filename, Socket sock, int percentage)
        {

            //foreach (Work file in files)
            //{
            //    string originale = Path.GetFileName(file.FileName);
            //    if (String.Compare(originale, filename) == 0)
            //        foreach (SendingFile sf in file.SendingFiles)
            //            if (sock.Equals(sf.Sock))
            //                sf.Progress.Value = percentage;
            //}
            ProgressBar pb;
            sockProg.TryGetValue(sock, out pb);
            pb.Value = percentage;
        }



        private void remove_row(TableLayoutPanel panel, int row_index_to_remove)
        {
            tableLayoutPanel1.SuspendLayout();
            if (row_index_to_remove >= panel.RowCount)
            {
                return;
            }

            // delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, row_index_to_remove);
                panel.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
            for (int i = row_index_to_remove + 1; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - 1);
                    }
                }
            }

            // remove last row
            panel.RowCount--;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();

        }



        private NeighborProtocol np = NeighborProtocol.getInstance;
        private ArrayList files = new ArrayList();
        private ConcurrentDictionary<Socket, ProgressBar> sockProg = new ConcurrentDictionary<Socket, ProgressBar>();
    }
}
