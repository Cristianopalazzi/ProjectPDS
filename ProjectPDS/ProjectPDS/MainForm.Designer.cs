namespace ProjectPDS
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fileInInvioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileInRicezioneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contattiOnlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.impostazioniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.esciToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Applicazione";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileInInvioToolStripMenuItem,
            this.fileInRicezioneToolStripMenuItem,
            this.contattiOnlineToolStripMenuItem,
            this.impostazioniToolStripMenuItem,
            this.esciToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(156, 114);
            // 
            // fileInInvioToolStripMenuItem
            // 
            this.fileInInvioToolStripMenuItem.Name = "fileInInvioToolStripMenuItem";
            this.fileInInvioToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.fileInInvioToolStripMenuItem.Text = "File in invio";
            this.fileInInvioToolStripMenuItem.Click += new System.EventHandler(this.fileInInvioToolStripMenuItem_Click);
            // 
            // fileInRicezioneToolStripMenuItem
            // 
            this.fileInRicezioneToolStripMenuItem.Name = "fileInRicezioneToolStripMenuItem";
            this.fileInRicezioneToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.fileInRicezioneToolStripMenuItem.Text = "File in ricezione";
            this.fileInRicezioneToolStripMenuItem.Click += new System.EventHandler(this.fileInRicezioneToolStripMenuItem_Click);
            // 
            // contattiOnlineToolStripMenuItem
            // 
            this.contattiOnlineToolStripMenuItem.Name = "contattiOnlineToolStripMenuItem";
            this.contattiOnlineToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.contattiOnlineToolStripMenuItem.Text = "Contatti online";
            this.contattiOnlineToolStripMenuItem.Click += new System.EventHandler(this.contattiOnlineToolStripMenuItem_Click);
            // 
            // impostazioniToolStripMenuItem
            // 
            this.impostazioniToolStripMenuItem.Name = "impostazioniToolStripMenuItem";
            this.impostazioniToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.impostazioniToolStripMenuItem.Text = "Impostazioni";
            this.impostazioniToolStripMenuItem.Click += new System.EventHandler(this.impostazioniToolStripMenuItem_Click);
            // 
            // esciToolStripMenuItem
            // 
            this.esciToolStripMenuItem.Name = "esciToolStripMenuItem";
            this.esciToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.esciToolStripMenuItem.Text = "Esci";
            this.esciToolStripMenuItem.Click += new System.EventHandler(this.esciToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileInInvioToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileInRicezioneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contattiOnlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem impostazioniToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem esciToolStripMenuItem;
    }
}