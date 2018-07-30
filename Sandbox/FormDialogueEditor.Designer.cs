namespace Sandbox
{
    partial class FormDialogueEditor
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.bFile = new System.Windows.Forms.ToolStripMenuItem();
            this.bOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.bSave = new System.Windows.Forms.ToolStripMenuItem();
            this.bSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.sfd = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(539, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // bFile
            // 
            this.bFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bOpen,
            this.bSave,
            this.bSaveAs});
            this.bFile.Name = "bFile";
            this.bFile.Size = new System.Drawing.Size(37, 20);
            this.bFile.Text = "File";
            // 
            // bOpen
            // 
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(114, 22);
            this.bOpen.Text = "Open";
            this.bOpen.Click += new System.EventHandler(this.bOpen_Click);
            // 
            // bSave
            // 
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(114, 22);
            this.bSave.Text = "Save";
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bSaveAs
            // 
            this.bSaveAs.Name = "bSaveAs";
            this.bSaveAs.Size = new System.Drawing.Size(114, 22);
            this.bSaveAs.Text = "Save As";
            this.bSaveAs.Click += new System.EventHandler(this.bSaveAs_Click);
            // 
            // ofd
            // 
            this.ofd.Filter = "Ned Projects|*.ned";
            this.ofd.Title = "Open";
            // 
            // sfd
            // 
            this.sfd.Filter = "Ned Project|*.ned";
            this.sfd.Title = "Save";
            // 
            // FormDialogueEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 374);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormDialogueEditor";
            this.Text = "FormDialogueEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDialogEditor_FormClosing);
            this.Load += new System.EventHandler(this.FormDialogEditor_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem bFile;
        private System.Windows.Forms.ToolStripMenuItem bOpen;
        private System.Windows.Forms.ToolStripMenuItem bSave;
        private System.Windows.Forms.ToolStripMenuItem bSaveAs;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.SaveFileDialog sfd;
    }
}