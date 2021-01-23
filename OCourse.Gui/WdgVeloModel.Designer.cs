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
      if (disposing)
      {
        _bindingSource?.Dispose();
        components?.Dispose();
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
      this.btnMap = new System.Windows.Forms.Button();
      this.lblMap = new System.Windows.Forms.Label();
      this.mnuAll = new System.Windows.Forms.MenuStrip();
      this.mniSettings = new System.Windows.Forms.ToolStripMenuItem();
      this.mniOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSaveAs = new System.Windows.Forms.ToolStripMenuItem();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.grdSymbols = new System.Windows.Forms.DataGridView();
      this.lblDefaultVelo = new System.Windows.Forms.Label();
      this.txtDefaultVelo = new System.Windows.Forms.TextBox();
      this.txtDefaultPntSize = new System.Windows.Forms.TextBox();
      this.lblDefaultPntSize = new System.Windows.Forms.Label();
      this.mnuAll.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdSymbols)).BeginInit();
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
      // btnMap
      // 
      this.btnMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMap.Location = new System.Drawing.Point(451, 35);
      this.btnMap.Name = "btnMap";
      this.btnMap.Size = new System.Drawing.Size(23, 23);
      this.btnMap.TabIndex = 1;
      this.btnMap.Text = "...";
      this.btnMap.UseVisualStyleBackColor = true;
      this.btnMap.Click += new System.EventHandler(this.Btnmap_Click);
      // 
      // lblMap
      // 
      this.lblMap.AutoSize = true;
      this.lblMap.Location = new System.Drawing.Point(10, 40);
      this.lblMap.Name = "lblMap";
      this.lblMap.Size = new System.Drawing.Size(28, 13);
      this.lblMap.TabIndex = 2;
      this.lblMap.Text = "Map";
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
      this.mniOpen.Size = new System.Drawing.Size(124, 22);
      this.mniOpen.Text = "Open ..";
      this.mniOpen.Click += new System.EventHandler(this.MniOpen_Click);
      // 
      // mniSave
      // 
      this.mniSave.Name = "mniSave";
      this.mniSave.Size = new System.Drawing.Size(124, 22);
      this.mniSave.Text = "Save";
      this.mniSave.Click += new System.EventHandler(this.MniSave_Click);
      // 
      // mniSaveAs
      // 
      this.mniSaveAs.Name = "mniSaveAs";
      this.mniSaveAs.Size = new System.Drawing.Size(124, 22);
      this.mniSaveAs.Text = "Save as ...";
      this.mniSaveAs.Click += new System.EventHandler(this.MniSaveAs_Click);
      // 
      // grdSymbols
      // 
      this.grdSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grdSymbols.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grdSymbols.Location = new System.Drawing.Point(13, 114);
      this.grdSymbols.Name = "grdSymbols";
      this.grdSymbols.Size = new System.Drawing.Size(775, 324);
      this.grdSymbols.TabIndex = 4;
      // 
      // lblDefaultVelo
      // 
      this.lblDefaultVelo.AutoSize = true;
      this.lblDefaultVelo.Location = new System.Drawing.Point(12, 65);
      this.lblDefaultVelo.Name = "lblDefaultVelo";
      this.lblDefaultVelo.Size = new System.Drawing.Size(81, 13);
      this.lblDefaultVelo.TabIndex = 5;
      this.lblDefaultVelo.Text = "Default Velocity";
      // 
      // txtDefaultVelo
      // 
      this.txtDefaultVelo.Location = new System.Drawing.Point(98, 62);
      this.txtDefaultVelo.Name = "txtDefaultVelo";
      this.txtDefaultVelo.Size = new System.Drawing.Size(44, 20);
      this.txtDefaultVelo.TabIndex = 6;
      this.txtDefaultVelo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // txtDefaultPntSize
      // 
      this.txtDefaultPntSize.Location = new System.Drawing.Point(98, 88);
      this.txtDefaultPntSize.Name = "txtDefaultPntSize";
      this.txtDefaultPntSize.Size = new System.Drawing.Size(44, 20);
      this.txtDefaultPntSize.TabIndex = 8;
      this.txtDefaultPntSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblDefaultPntSize
      // 
      this.lblDefaultPntSize.AutoSize = true;
      this.lblDefaultPntSize.Location = new System.Drawing.Point(12, 91);
      this.lblDefaultPntSize.Name = "lblDefaultPntSize";
      this.lblDefaultPntSize.Size = new System.Drawing.Size(83, 13);
      this.lblDefaultPntSize.TabIndex = 7;
      this.lblDefaultPntSize.Text = "Default Pnt.Size";
      // 
      // WdgVeloModel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.txtDefaultPntSize);
      this.Controls.Add(this.lblDefaultPntSize);
      this.Controls.Add(this.txtDefaultVelo);
      this.Controls.Add(this.lblDefaultVelo);
      this.Controls.Add(this.grdSymbols);
      this.Controls.Add(this.lblMap);
      this.Controls.Add(this.btnMap);
      this.Controls.Add(this.txtMap);
      this.Controls.Add(this.mnuAll);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.mnuAll;
      this.Name = "WdgVeloModel";
      this.Text = "WdgVeloModel";
      this.mnuAll.ResumeLayout(false);
      this.mnuAll.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdSymbols)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtMap;
    private System.Windows.Forms.Button btnMap;
    private System.Windows.Forms.Label lblMap;
    private System.Windows.Forms.MenuStrip mnuAll;
    private System.Windows.Forms.ToolStripMenuItem mniSettings;
    private System.Windows.Forms.ToolStripMenuItem mniOpen;
    private System.Windows.Forms.ToolStripMenuItem mniSave;
    private System.Windows.Forms.ToolStripMenuItem mniSaveAs;
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.DataGridView grdSymbols;
    private System.Windows.Forms.Label lblDefaultVelo;
    private System.Windows.Forms.TextBox txtDefaultVelo;
    private System.Windows.Forms.TextBox txtDefaultPntSize;
    private System.Windows.Forms.Label lblDefaultPntSize;
  }
}