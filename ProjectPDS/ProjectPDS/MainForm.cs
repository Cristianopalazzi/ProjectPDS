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

        private FilesToSend fils;
    }
}
