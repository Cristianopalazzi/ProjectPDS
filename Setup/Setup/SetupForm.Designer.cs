namespace Setup
{
    partial class SetupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.sfoglia = new System.Windows.Forms.Button();
            this.conferma = new System.Windows.Forms.Button();
            this.annulla = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.Error = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.fine = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Georgia", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(42, 81);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(417, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Benvenuto al setup di EasyShare!";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 232);
            this.textBox1.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(312, 29);
            this.textBox1.TabIndex = 1;
            // 
            // sfoglia
            // 
            this.sfoglia.Location = new System.Drawing.Point(349, 227);
            this.sfoglia.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.sfoglia.Name = "sfoglia";
            this.sfoglia.Size = new System.Drawing.Size(137, 39);
            this.sfoglia.TabIndex = 2;
            this.sfoglia.Text = "Sfoglia";
            this.sfoglia.UseVisualStyleBackColor = true;
            this.sfoglia.Click += new System.EventHandler(this.sfoglia_Click);
            // 
            // conferma
            // 
            this.conferma.Location = new System.Drawing.Point(181, 387);
            this.conferma.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.conferma.Name = "conferma";
            this.conferma.Size = new System.Drawing.Size(137, 39);
            this.conferma.TabIndex = 3;
            this.conferma.Text = "Conferma";
            this.conferma.UseVisualStyleBackColor = true;
            this.conferma.Click += new System.EventHandler(this.conferma_Click);
            // 
            // annulla
            // 
            this.annulla.Location = new System.Drawing.Point(349, 387);
            this.annulla.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.annulla.Name = "annulla";
            this.annulla.Size = new System.Drawing.Size(137, 39);
            this.annulla.TabIndex = 4;
            this.annulla.Text = "Annulla";
            this.annulla.UseVisualStyleBackColor = true;
            this.annulla.Click += new System.EventHandler(this.annulla_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 193);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(300, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Scegli una cartella per continuare.";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Scegli la cartella dove installare EasyShare";
            this.folderBrowserDialog1.SelectedPath = "C:\\Program Files (x86)\\Prove";
            // 
            // Error
            // 
            this.Error.AutoSize = true;
            this.Error.Location = new System.Drawing.Point(6, 282);
            this.Error.Name = "Error";
            this.Error.Size = new System.Drawing.Size(0, 23);
            this.Error.TabIndex = 6;
            this.Error.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.fine);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(520, 462);
            this.panel1.TabIndex = 7;
            this.panel1.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(149, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(194, 23);
            this.label3.TabIndex = 0;
            this.label3.Text = "Installazione in corso.";
            // 
            // fine
            // 
            this.fine.Enabled = false;
            this.fine.Location = new System.Drawing.Point(358, 371);
            this.fine.Name = "fine";
            this.fine.Size = new System.Drawing.Size(128, 55);
            this.fine.TabIndex = 1;
            this.fine.Text = "Fine";
            this.fine.UseVisualStyleBackColor = true;
            this.fine.Click += new System.EventHandler(this.fine_Click);
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(520, 462);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Error);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.annulla);
            this.Controls.Add(this.conferma);
            this.Controls.Add(this.sfoglia);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupForm";
            this.ShowIcon = false;
            this.Text = "SetupForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button sfoglia;
        private System.Windows.Forms.Button conferma;
        private System.Windows.Forms.Button annulla;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label Error;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button fine;
    }
}