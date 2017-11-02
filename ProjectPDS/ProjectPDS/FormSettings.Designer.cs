namespace ProjectPDS
{
    partial class FormSettings
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
            this.panelStato = new System.Windows.Forms.Panel();
            this.impostazioni = new System.Windows.Forms.Panel();
            this.fileSelector = new System.Windows.Forms.Button();
            this.checkBoxDefaultDir = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoAccept = new System.Windows.Forms.CheckBox();
            this.textBoxDefaultPath = new System.Windows.Forms.TextBox();
            this.annulla = new System.Windows.Forms.Button();
            this.conferma = new System.Windows.Forms.Button();
            this.labelDefaultPath = new System.Windows.Forms.Label();
            this.labelDefaultDir = new System.Windows.Forms.Label();
            this.labelAutoAccept = new System.Windows.Forms.Label();
            this.linkLabelSettings = new System.Windows.Forms.LinkLabel();
            this.stato = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.panelStato.SuspendLayout();
            this.impostazioni.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelStato
            // 
            this.panelStato.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panelStato.Controls.Add(this.impostazioni);
            this.panelStato.Controls.Add(this.linkLabelSettings);
            this.panelStato.Controls.Add(this.stato);
            this.panelStato.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStato.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelStato.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.panelStato.Location = new System.Drawing.Point(0, 0);
            this.panelStato.Name = "panelStato";
            this.panelStato.Size = new System.Drawing.Size(353, 324);
            this.panelStato.TabIndex = 0;
            // 
            // impostazioni
            // 
            this.impostazioni.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.impostazioni.Controls.Add(this.fileSelector);
            this.impostazioni.Controls.Add(this.checkBoxDefaultDir);
            this.impostazioni.Controls.Add(this.checkBoxAutoAccept);
            this.impostazioni.Controls.Add(this.textBoxDefaultPath);
            this.impostazioni.Controls.Add(this.annulla);
            this.impostazioni.Controls.Add(this.conferma);
            this.impostazioni.Controls.Add(this.labelDefaultPath);
            this.impostazioni.Controls.Add(this.labelDefaultDir);
            this.impostazioni.Controls.Add(this.labelAutoAccept);
            this.impostazioni.Dock = System.Windows.Forms.DockStyle.Fill;
            this.impostazioni.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.impostazioni.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.impostazioni.Location = new System.Drawing.Point(0, 0);
            this.impostazioni.Name = "impostazioni";
            this.impostazioni.Size = new System.Drawing.Size(353, 324);
            this.impostazioni.TabIndex = 2;
            // 
            // fileSelector
            // 
            this.fileSelector.Location = new System.Drawing.Point(315, 121);
            this.fileSelector.Name = "fileSelector";
            this.fileSelector.Size = new System.Drawing.Size(26, 23);
            this.fileSelector.TabIndex = 8;
            this.fileSelector.Text = "...";
            this.fileSelector.UseVisualStyleBackColor = true;
            this.fileSelector.Click += new System.EventHandler(this.button4_Click);
            // 
            // checkBoxDefaultDir
            // 
            this.checkBoxDefaultDir.AutoSize = true;
            this.checkBoxDefaultDir.Location = new System.Drawing.Point(289, 84);
            this.checkBoxDefaultDir.Name = "checkBoxDefaultDir";
            this.checkBoxDefaultDir.Size = new System.Drawing.Size(15, 14);
            this.checkBoxDefaultDir.TabIndex = 7;
            this.checkBoxDefaultDir.UseVisualStyleBackColor = true;
            this.checkBoxDefaultDir.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBoxAutoAccept
            // 
            this.checkBoxAutoAccept.AutoSize = true;
            this.checkBoxAutoAccept.Location = new System.Drawing.Point(289, 41);
            this.checkBoxAutoAccept.Name = "checkBoxAutoAccept";
            this.checkBoxAutoAccept.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoAccept.TabIndex = 6;
            this.checkBoxAutoAccept.UseVisualStyleBackColor = true;
            // 
            // textBoxDefaultPath
            // 
            this.textBoxDefaultPath.Location = new System.Drawing.Point(204, 120);
            this.textBoxDefaultPath.Name = "textBoxDefaultPath";
            this.textBoxDefaultPath.Size = new System.Drawing.Size(100, 30);
            this.textBoxDefaultPath.TabIndex = 5;
            // 
            // annulla
            // 
            this.annulla.BackColor = System.Drawing.SystemColors.HotTrack;
            this.annulla.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.annulla.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.annulla.Location = new System.Drawing.Point(16, 225);
            this.annulla.Name = "annulla";
            this.annulla.Size = new System.Drawing.Size(128, 77);
            this.annulla.TabIndex = 4;
            this.annulla.Text = "annulla";
            this.annulla.UseVisualStyleBackColor = false;
            this.annulla.Click += new System.EventHandler(this.button3_Click);
            // 
            // conferma
            // 
            this.conferma.BackColor = System.Drawing.SystemColors.HotTrack;
            this.conferma.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.conferma.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.conferma.Location = new System.Drawing.Point(204, 225);
            this.conferma.Name = "conferma";
            this.conferma.Size = new System.Drawing.Size(129, 77);
            this.conferma.TabIndex = 3;
            this.conferma.Text = "conferma";
            this.conferma.UseVisualStyleBackColor = false;
            this.conferma.Click += new System.EventHandler(this.button2_Click);
            // 
            // labelDefaultPath
            // 
            this.labelDefaultPath.AutoSize = true;
            this.labelDefaultPath.Location = new System.Drawing.Point(12, 123);
            this.labelDefaultPath.Name = "labelDefaultPath";
            this.labelDefaultPath.Size = new System.Drawing.Size(143, 23);
            this.labelDefaultPath.TabIndex = 2;
            this.labelDefaultPath.Text = "Default Dir Path";
            this.labelDefaultPath.Visible = false;
            // 
            // labelDefaultDir
            // 
            this.labelDefaultDir.AutoSize = true;
            this.labelDefaultDir.Location = new System.Drawing.Point(12, 79);
            this.labelDefaultDir.Name = "labelDefaultDir";
            this.labelDefaultDir.Size = new System.Drawing.Size(151, 23);
            this.labelDefaultDir.TabIndex = 1;
            this.labelDefaultDir.Text = "Default Directory";
            // 
            // labelAutoAccept
            // 
            this.labelAutoAccept.AutoSize = true;
            this.labelAutoAccept.Location = new System.Drawing.Point(12, 38);
            this.labelAutoAccept.Name = "labelAutoAccept";
            this.labelAutoAccept.Size = new System.Drawing.Size(104, 23);
            this.labelAutoAccept.TabIndex = 0;
            this.labelAutoAccept.Text = "AutoAccept";
            // 
            // linkLabelSettings
            // 
            this.linkLabelSettings.AutoSize = true;
            this.linkLabelSettings.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelSettings.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.linkLabelSettings.LinkColor = System.Drawing.Color.Honeydew;
            this.linkLabelSettings.Location = new System.Drawing.Point(20, 55);
            this.linkLabelSettings.Name = "linkLabelSettings";
            this.linkLabelSettings.Size = new System.Drawing.Size(143, 29);
            this.linkLabelSettings.TabIndex = 3;
            this.linkLabelSettings.TabStop = true;
            this.linkLabelSettings.Text = "impostazioni";
            this.linkLabelSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // stato
            // 
            this.stato.BackColor = System.Drawing.SystemColors.HotTrack;
            this.stato.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stato.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stato.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.stato.Location = new System.Drawing.Point(24, 187);
            this.stato.Name = "stato";
            this.stato.Size = new System.Drawing.Size(170, 102);
            this.stato.TabIndex = 1;
            this.stato.Text = "Stato: Online";
            this.stato.UseVisualStyleBackColor = true;
            this.stato.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(353, 324);
            this.ControlBox = false;
            this.Controls.Add(this.panelStato);
            this.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.Opacity = 0.95D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Deactivate += new System.EventHandler(this.FormSettings_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormSettings_closed);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.panelStato.ResumeLayout(false);
            this.panelStato.PerformLayout();
            this.impostazioni.ResumeLayout(false);
            this.impostazioni.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelStato;
        private System.Windows.Forms.Button stato;
        private System.Windows.Forms.Panel impostazioni;
        private System.Windows.Forms.Button annulla;
        private System.Windows.Forms.Button conferma;
        private System.Windows.Forms.Label labelDefaultPath;
        private System.Windows.Forms.Label labelDefaultDir;
        private System.Windows.Forms.Label labelAutoAccept;
        private System.Windows.Forms.CheckBox checkBoxDefaultDir;
        private System.Windows.Forms.CheckBox checkBoxAutoAccept;
        private System.Windows.Forms.TextBox textBoxDefaultPath;
        private System.Windows.Forms.LinkLabel linkLabelSettings;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button fileSelector;
      
    }
}