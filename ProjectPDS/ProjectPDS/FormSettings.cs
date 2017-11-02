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
using System.Runtime.Serialization.Formatters.Binary;

namespace ProjectPDS
{
    public partial class FormSettings : Form
    {
         public FormSettings()
        {
            InitializeComponent();
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - 369, workingArea.Bottom - 340);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            this.checkBox1.Checked = props.AutoAccept;
            this.checkBox2.Checked = props.DefaultDir;
            this.textBox1.Text = props.DefaultDirPath;
            if (!checkBox2.Checked)
                textBox1.Enabled = false;
            if (!props.Online)
            {
                button1.Text = "Stato: Offline";
                button1.BackColor = Color.Gray;
            }
            else
            {
                button1.Text = "Stato: Online";
                button1.BackColor = System.Drawing.SystemColors.HotTrack;
            }

            panel2.Hide();
            //if (this.WindowState == FormWindowState.Minimized)
            //    this.Hide();

        }

        private void FormSettings_closed(object sender, FormClosedEventArgs e)
        {
            Settings.writeSettings(props);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (String.Compare("Stato: Online", button1.Text) == 0)
            {
                button1.Text = "Stato: Offline";
                button1.BackColor = Color.Gray;
                props.Online = false;
            }
            else
            {
                button1.Text = "Stato: Online";
                button1.BackColor = System.Drawing.SystemColors.HotTrack;
                props.Online = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // When closing the form, save the data from the TextBoxes to a file 
            props.DefaultDirPath = textBox1.Text;
            props.DefaultDir = checkBox2.Checked;
            props.AutoAccept = checkBox1.Checked;
            panel2.Hide();
        }



        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel2.Hide();
            checkBox2.Checked = props.DefaultDir;
            checkBox1.Checked = props.AutoAccept;
            textBox1.Text = props.DefaultDirPath;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panel2.Show();
        }


        private void FormSettings_Deactivate(object sender, EventArgs e)
        {
            //   Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Seleziona la cartella di default.";
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private Settings props = Settings.getInstance;
    }
}
