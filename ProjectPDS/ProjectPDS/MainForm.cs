using System;
using System.Windows.Forms;
using System.Collections;

namespace ProjectPDS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            fils = new FilesToSend();
            formSettings = new FormSettings();
        }

        public void apriFileToSend(Work w)
        {
            fils.AddFile(w);
            if (!fils.Visible)
                fils.ShowDialog();
            else
            {
                if (fils.WindowState == FormWindowState.Minimized)
                    fils.WindowState = FormWindowState.Normal;
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        

        private void fileInInvioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!fils.Visible)
                fils.ShowDialog();
            else
            {
                if (fils.WindowState == FormWindowState.Minimized)
                    fils.WindowState = FormWindowState.Normal;
            }
        }

        private void fileInRicezioneToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void contattiOnlineToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void impostazioniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (formSettings.Visible)
                formSettings.Close();
            else
                formSettings.Show();
        }

        private void esciToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO per ora va bene cosi, poi cambiare in application.exit
            System.Environment.Exit(0);
        }



        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.Hide();
                    this.WindowState = FormWindowState.Minimized;
                }
                else
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                }
            }


        }

        private FilesToSend fils;
        private FormSettings formSettings;

      
    }
}
