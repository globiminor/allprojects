namespace LeastCostPathUI
{
  partial class WdgLeastCostPath
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgLeastCostPath));
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.mnuMain = new System.Windows.Forms.MainMenu(this.components);
      this.mniSettings = new System.Windows.Forms.MenuItem();
      this.mniOpen = new System.Windows.Forms.MenuItem();
      this.mniSave = new System.Windows.Forms.MenuItem();
      this.mniSaveAs = new System.Windows.Forms.MenuItem();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnClose = new System.Windows.Forms.Button();
      this.cntConfig = new LeastCostPathUI.CntConfigView();
      this.cntOutput = new LeastCostPathUI.CntOutput();
      this.SuspendLayout();
      // 
      // mnuMain
      // 
      this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mniSettings});
      // 
      // mniSettings
      // 
      this.mniSettings.Index = 0;
      this.mniSettings.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mniOpen,
            this.mniSave,
            this.mniSaveAs});
      this.mniSettings.Text = "Settings";
      // 
      // mniOpen
      // 
      this.mniOpen.Index = 0;
      this.mniOpen.Text = "Open";
      // 
      // mniSave
      // 
      this.mniSave.Index = 1;
      this.mniSave.Text = "Save";
      // 
      // mniSaveAs
      // 
      this.mniSaveAs.Index = 2;
      this.mniSaveAs.Text = "Save As";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(304, 628);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(56, 23);
      this.btnOK.TabIndex = 15;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.Location = new System.Drawing.Point(376, 628);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(56, 23);
      this.btnClose.TabIndex = 16;
      this.btnClose.Text = "Close";
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // cntConfig
      // 
      this.cntConfig.Location = new System.Drawing.Point(14, -4);
      this.cntConfig.Name = "cntConfig";
      this.cntConfig.Size = new System.Drawing.Size(422, 98);
      this.cntConfig.TabIndex = 27;
      // 
      // cntOutput
      // 
      this.cntOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cntOutput.Location = new System.Drawing.Point(14, 90);
      this.cntOutput.Name = "cntOutput";
      this.cntOutput.Size = new System.Drawing.Size(422, 570);
      this.cntOutput.TabIndex = 26;
      // 
      // WdgLeastCostPath
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(448, 663);
      this.Controls.Add(this.cntConfig);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.cntOutput);
      this.Menu = this.mnuMain;
      this.Name = "WdgLeastCostPath";
      this.Text = "Least Cost Path";
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.MainMenu mnuMain;
    private System.Windows.Forms.MenuItem mniSettings;
    private System.Windows.Forms.MenuItem mniOpen;
    private System.Windows.Forms.MenuItem mniSave;
    private System.Windows.Forms.MenuItem mniSaveAs;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnClose;
    private CntOutput cntOutput;
    private CntConfigView cntConfig;

    #endregion
  }
}