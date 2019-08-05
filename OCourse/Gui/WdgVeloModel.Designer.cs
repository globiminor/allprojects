namespace OCourse.Gui
{
  partial class WdgVeloModel
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgVeloModel));
      this.txtMap = new System.Windows.Forms.TextBox();
      this.btnCourse = new System.Windows.Forms.Button();
      this.lblCourse = new System.Windows.Forms.Label();
      this.mnuAll = new System.Windows.Forms.MenuStrip();
      this.mniSettings = new System.Windows.Forms.ToolStripMenuItem();
      this.mniOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSaveAs = new System.Windows.Forms.ToolStripMenuItem();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.mnuAll.SuspendLayout();
      this.SuspendLayout();
      // 
      // txtMap
      // 
      this.txtMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtMap.Location = new System.Drawing.Point(98, 37);
      this.txtMap.Name = "txtMap";
      this.txtMap.Size = new System.Drawing.Size(347, 20);
      this.txtMap.TabIndex = 0;
      // 
      // btnCourse
      // 
      this.btnCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCourse.Location = new System.Drawing.Point(451, 35);
      this.btnCourse.Name = "btnCourse";
      this.btnCourse.Size = new System.Drawing.Size(23, 23);
      this.btnCourse.TabIndex = 1;
      this.btnCourse.Text = "...";
      this.btnCourse.UseVisualStyleBackColor = true;
      // 
      // lblCourse
      // 
      this.lblCourse.AutoSize = true;
      this.lblCourse.Location = new System.Drawing.Point(10, 40);
      this.lblCourse.Name = "lblCourse";
      this.lblCourse.Size = new System.Drawing.Size(40, 13);
      this.lblCourse.TabIndex = 2;
      this.lblCourse.Text = "Course";
      // 
      // mnuAll
      // 
      this.mnuAll.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSettings});
      this.mnuAll.Location = new System.Drawing.Point(0, 0);
      this.mnuAll.Name = "mnuAll";
      this.mnuAll.Size = new System.Drawing.Size(800, 24);
      this.mnuAll.TabIndex = 3;
      this.mnuAll.Text = "menuStrip1";
      // 
      // mniSettings
      // 
      this.mniSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniOpen,
            this.mniSave,
            this.mniSaveAs});
      this.mniSettings.Name = "mniSettings";
      this.mniSettings.Size = new System.Drawing.Size(61, 20);
      this.mniSettings.Text = "Settings";
      // 
      // mniOpen
      // 
      this.mniOpen.Name = "mniOpen";
      this.mniOpen.Size = new System.Drawing.Size(180, 22);
      this.mniOpen.Text = "Open ..";
      this.mniOpen.Click += new System.EventHandler(this.MniOpen_Click);
      // 
      // mniSave
      // 
      this.mniSave.Name = "mniSave";
      this.mniSave.Size = new System.Drawing.Size(180, 22);
      this.mniSave.Text = "Save";
      // 
      // mniSaveAs
      // 
      this.mniSaveAs.Name = "mniSaveAs";
      this.mniSaveAs.Size = new System.Drawing.Size(180, 22);
      this.mniSaveAs.Text = "Save as ...";
      // 
      // WdgVeloModel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.lblCourse);
      this.Controls.Add(this.btnCourse);
      this.Controls.Add(this.txtMap);
      this.Controls.Add(this.mnuAll);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.mnuAll;
      this.Name = "WdgVeloModel";
      this.Text = "WdgVeloModel";
      this.mnuAll.ResumeLayout(false);
      this.mnuAll.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtMap;
    private System.Windows.Forms.Button btnCourse;
    private System.Windows.Forms.Label lblCourse;
    private System.Windows.Forms.MenuStrip mnuAll;
    private System.Windows.Forms.ToolStripMenuItem mniSettings;
    private System.Windows.Forms.ToolStripMenuItem mniOpen;
    private System.Windows.Forms.ToolStripMenuItem mniSave;
    private System.Windows.Forms.ToolStripMenuItem mniSaveAs;
    private System.Windows.Forms.ToolTip ttp;
  }
}