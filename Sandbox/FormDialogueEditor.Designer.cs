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
            this.gbDialogue = new System.Windows.Forms.GroupBox();
            this.lDialogOptions = new BrightIdeasSoftware.FastDataListView();
            this.olvColumnDialog = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.bRemoveDialogOption = new System.Windows.Forms.Button();
            this.bAddDialogOption = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.bFile = new System.Windows.Forms.ToolStripMenuItem();
            this.bOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.bSave = new System.Windows.Forms.ToolStripMenuItem();
            this.bSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.sfd = new System.Windows.Forms.SaveFileDialog();
            this.gbDialogue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lDialogOptions)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDialogue
            // 
            this.gbDialogue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDialogue.Controls.Add(this.lDialogOptions);
            this.gbDialogue.Controls.Add(this.bRemoveDialogOption);
            this.gbDialogue.Controls.Add(this.bAddDialogOption);
            this.gbDialogue.Location = new System.Drawing.Point(14, 27);
            this.gbDialogue.Name = "gbDialogue";
            this.gbDialogue.Size = new System.Drawing.Size(513, 335);
            this.gbDialogue.TabIndex = 3;
            this.gbDialogue.TabStop = false;
            this.gbDialogue.Text = "Dialogue";
            // 
            // lDialogOptions
            // 
            this.lDialogOptions.AllColumns.Add(this.olvColumnDialog);
            this.lDialogOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lDialogOptions.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            this.lDialogOptions.CellEditUseWholeCell = false;
            this.lDialogOptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnDialog});
            this.lDialogOptions.DataSource = null;
            this.lDialogOptions.Location = new System.Drawing.Point(6, 19);
            this.lDialogOptions.MultiSelect = false;
            this.lDialogOptions.Name = "lDialogOptions";
            this.lDialogOptions.ShowGroups = false;
            this.lDialogOptions.Size = new System.Drawing.Size(501, 281);
            this.lDialogOptions.TabIndex = 5;
            this.lDialogOptions.UseCompatibleStateImageBehavior = false;
            this.lDialogOptions.View = System.Windows.Forms.View.List;
            this.lDialogOptions.VirtualMode = true;
            // 
            // olvColumnDialog
            // 
            this.olvColumnDialog.AspectName = "Text";
            this.olvColumnDialog.AutoCompleteEditor = false;
            this.olvColumnDialog.AutoCompleteEditorMode = System.Windows.Forms.AutoCompleteMode.None;
            this.olvColumnDialog.ButtonSizing = BrightIdeasSoftware.OLVColumn.ButtonSizingMode.CellBounds;
            this.olvColumnDialog.CellEditUseWholeCell = true;
            this.olvColumnDialog.FillsFreeSpace = true;
            this.olvColumnDialog.Groupable = false;
            this.olvColumnDialog.Text = "Options";
            // 
            // bRemoveDialogOption
            // 
            this.bRemoveDialogOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bRemoveDialogOption.Location = new System.Drawing.Point(432, 306);
            this.bRemoveDialogOption.Name = "bRemoveDialogOption";
            this.bRemoveDialogOption.Size = new System.Drawing.Size(75, 23);
            this.bRemoveDialogOption.TabIndex = 4;
            this.bRemoveDialogOption.Text = "Remove";
            this.bRemoveDialogOption.UseVisualStyleBackColor = true;
            this.bRemoveDialogOption.Click += new System.EventHandler(this.bRemoveDialogOption_Click);
            // 
            // bAddDialogOption
            // 
            this.bAddDialogOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bAddDialogOption.Location = new System.Drawing.Point(6, 306);
            this.bAddDialogOption.Name = "bAddDialogOption";
            this.bAddDialogOption.Size = new System.Drawing.Size(75, 23);
            this.bAddDialogOption.TabIndex = 3;
            this.bAddDialogOption.Text = "Add";
            this.bAddDialogOption.UseVisualStyleBackColor = true;
            this.bAddDialogOption.Click += new System.EventHandler(this.bAddDialogOption_Click);
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
            this.Controls.Add(this.gbDialogue);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormDialogueEditor";
            this.Text = "FormDialogueEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDialogEditor_FormClosing);
            this.Load += new System.EventHandler(this.FormDialogEditor_Load);
            this.gbDialogue.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lDialogOptions)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button bAddDialogOption;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem bFile;
        private System.Windows.Forms.ToolStripMenuItem bOpen;
        private System.Windows.Forms.ToolStripMenuItem bSave;
        private System.Windows.Forms.ToolStripMenuItem bSaveAs;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.SaveFileDialog sfd;
        private System.Windows.Forms.GroupBox gbDialogue;
        private System.Windows.Forms.Button bRemoveDialogOption;
        private BrightIdeasSoftware.FastDataListView lDialogOptions;
        private BrightIdeasSoftware.OLVColumn olvColumnDialog;
    }
}