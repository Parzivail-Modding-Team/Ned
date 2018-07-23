namespace Sandbox
{
    partial class FormDialogEditor
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
            System.Windows.Forms.GroupBox gbDialogue;
            this.bAddNode = new System.Windows.Forms.Button();
            this.cbActor = new System.Windows.Forms.ComboBox();
            this.lDialogOptions = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnDialogue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.bAddDialogOption = new System.Windows.Forms.Button();
            gbDialogue = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.lDialogOptions)).BeginInit();
            gbDialogue.SuspendLayout();
            this.SuspendLayout();
            // 
            // bAddNode
            // 
            this.bAddNode.Location = new System.Drawing.Point(426, 19);
            this.bAddNode.Name = "bAddNode";
            this.bAddNode.Size = new System.Drawing.Size(75, 23);
            this.bAddNode.TabIndex = 0;
            this.bAddNode.Text = "Add Node";
            this.bAddNode.UseVisualStyleBackColor = true;
            this.bAddNode.Click += new System.EventHandler(this.bAddNode_Click);
            // 
            // cbActor
            // 
            this.cbActor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbActor.FormattingEnabled = true;
            this.cbActor.Location = new System.Drawing.Point(299, 19);
            this.cbActor.Name = "cbActor";
            this.cbActor.Size = new System.Drawing.Size(121, 21);
            this.cbActor.TabIndex = 1;
            // 
            // lDialogOptions
            // 
            this.lDialogOptions.AllColumns.Add(this.olvColumnDialogue);
            this.lDialogOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lDialogOptions.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            this.lDialogOptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnDialogue});
            this.lDialogOptions.Cursor = System.Windows.Forms.Cursors.Default;
            this.lDialogOptions.Location = new System.Drawing.Point(6, 48);
            this.lDialogOptions.Name = "lDialogOptions";
            this.lDialogOptions.ShowGroups = false;
            this.lDialogOptions.Size = new System.Drawing.Size(495, 360);
            this.lDialogOptions.TabIndex = 2;
            this.lDialogOptions.UseCompatibleStateImageBehavior = false;
            this.lDialogOptions.View = System.Windows.Forms.View.Details;
            this.lDialogOptions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lDialogOptions_KeyDown);
            // 
            // olvColumnDialogue
            // 
            this.olvColumnDialogue.AspectName = "Text";
            this.olvColumnDialogue.AspectToStringFormat = "";
            this.olvColumnDialogue.AutoCompleteEditor = false;
            this.olvColumnDialogue.AutoCompleteEditorMode = System.Windows.Forms.AutoCompleteMode.None;
            this.olvColumnDialogue.CellEditUseWholeCell = true;
            this.olvColumnDialogue.Groupable = false;
            this.olvColumnDialogue.Text = "Dialogue";
            this.olvColumnDialogue.Width = 483;
            // 
            // gbDialogue
            // 
            gbDialogue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            gbDialogue.Controls.Add(this.bAddDialogOption);
            gbDialogue.Controls.Add(this.lDialogOptions);
            gbDialogue.Controls.Add(this.bAddNode);
            gbDialogue.Controls.Add(this.cbActor);
            gbDialogue.Location = new System.Drawing.Point(14, 12);
            gbDialogue.Name = "gbDialogue";
            gbDialogue.Size = new System.Drawing.Size(507, 414);
            gbDialogue.TabIndex = 3;
            gbDialogue.TabStop = false;
            gbDialogue.Text = "Dialogue";
            // 
            // bAddDialogOption
            // 
            this.bAddDialogOption.Location = new System.Drawing.Point(6, 19);
            this.bAddDialogOption.Name = "bAddDialogOption";
            this.bAddDialogOption.Size = new System.Drawing.Size(117, 23);
            this.bAddDialogOption.TabIndex = 3;
            this.bAddDialogOption.Text = "Add Dialog Option";
            this.bAddDialogOption.UseVisualStyleBackColor = true;
            this.bAddDialogOption.Click += new System.EventHandler(this.bAddDialogOption_Click);
            // 
            // FormDialogEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 438);
            this.Controls.Add(gbDialogue);
            this.Name = "FormDialogEditor";
            this.Text = "FormDialogEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDialogEditor_FormClosing);
            this.Load += new System.EventHandler(this.FormDialogEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lDialogOptions)).EndInit();
            gbDialogue.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bAddNode;
        private System.Windows.Forms.ComboBox cbActor;
        private BrightIdeasSoftware.ObjectListView lDialogOptions;
        private BrightIdeasSoftware.OLVColumn olvColumnDialogue;
        private System.Windows.Forms.Button bAddDialogOption;
    }
}