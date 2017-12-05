using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Setup
{

    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
            Program.FineInstallazione += Program_FineInstallazione;
            textBox1.Text = folderBrowserDialog1.SelectedPath;
            Error.ForeColor = Color.Red;
        }

        private void Program_FineInstallazione(bool value)
        {
            Point p = label3.Location;
            p.X = 10;
            label3.Location = p;
            label3.Text = "Installazione completata, premi fine per uscire";
            fine.Enabled = value;
        }

        private void sfoglia_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = folderBrowserDialog1.SelectedPath;

        }

        private void annulla_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void conferma_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
            {
                Error.Visible = true;
                Error.Text = "Seleziona una cartella per continuare";
                return;
            }
            if (Directory.Exists(textBox1.Text))
            {
                panel1.Visible = true;
                installation(textBox1.Text);

            }
            else
            {
                DialogResult dr = MessageBox.Show("La directory selezionata non esiste, crearla?", "Ops", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    Directory.CreateDirectory(textBox1.Text);
                    panel1.Visible = true;
                    installation(textBox1.Text);

                }
            }
        }

        public delegate void myDelegate(string instPath);
        public static event myDelegate installation;

        private void fine_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
