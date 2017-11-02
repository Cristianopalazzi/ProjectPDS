using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace ProjectPDS
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
            Rectangle workingArea = Screen.GetWorkingArea(this);
            Location = new Point(workingArea.Right - 369, workingArea.Bottom - 340);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            checkBoxAutoAccept.Checked = props.AutoAccept;
            checkBoxDefaultDir.Checked = props.DefaultDir;
            textBoxDefaultPath.Text = props.DefaultDirPath;
            if (checkBoxDefaultDir.Checked)
            {
                textBoxDefaultPath.Visible = true;
                labelDefaultPath.Visible = true;
                fileSelector.Visible = true;
            }
            else
            {
                textBoxDefaultPath.Visible = false;
                labelDefaultPath.Visible = false;
                fileSelector.Visible = false;
            }
            if (!props.Online)
            {
                stato.Text = "Stato: Offline";
                stato.BackColor = Color.Gray;
            }
            else
            {
                stato.Text = "Stato: Online";
                stato.BackColor = SystemColors.HotTrack;
            }

            impostazioni.Hide();
            //if (this.WindowState == FormWindowState.Minimized)
            //    this.Hide();

        }

        private void FormSettings_closed(object sender, FormClosedEventArgs e)
        {
            Settings.writeSettings(props);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (String.Compare("Stato: Online", stato.Text) == 0)
            {
                stato.Text = "Stato: Offline";
                stato.BackColor = Color.Gray;
                props.Online = false;
                NeighborProtocol.senderEvent.Reset();
            }
            else
            {
                stato.Text = "Stato: Online";
                stato.BackColor = SystemColors.HotTrack;
                props.Online = true;
                NeighborProtocol.senderEvent.Set();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBoxDefaultDir.Checked)
            {
                if (Directory.Exists(textBoxDefaultPath.Text))
                {
                    props.DefaultDir = checkBoxDefaultDir.Checked;
                    props.AutoAccept = checkBoxAutoAccept.Checked;
                    props.DefaultDirPath = textBoxDefaultPath.Text;
                    impostazioni.Hide();
                    Settings.writeSettings(props);
                }
                else
                    MessageBox.Show("Il percorso selezionato non è valido");
            }
            else
            {
                props.DefaultDir = checkBoxDefaultDir.Checked;
                props.AutoAccept = checkBoxAutoAccept.Checked;
                textBoxDefaultPath.Text = props.DefaultDirPath;
                impostazioni.Hide();
                Settings.writeSettings(props);
            }
        }



        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDefaultDir.Checked)
            {
                textBoxDefaultPath.Visible = true;
                labelDefaultPath.Visible = true;
                fileSelector.Visible = true;
            }
            else
            {
                if (!Directory.Exists(textBoxDefaultPath.Text)) textBoxDefaultPath.Text = "";
                textBoxDefaultPath.Visible = false;
                labelDefaultPath.Visible = false;
                fileSelector.Visible = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            impostazioni.Hide();
            textBoxDefaultPath.Text = props.DefaultDirPath;
            checkBoxDefaultDir.Checked = props.DefaultDir;
            checkBoxAutoAccept.Checked = props.AutoAccept;

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            impostazioni.Show();
        }


        private void FormSettings_Deactivate(object sender, EventArgs e)
        {
            //   Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Seleziona la cartella di default.";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDefaultPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private Settings props = Settings.getInstance;
    }
}