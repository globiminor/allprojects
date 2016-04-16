namespace TMapWin
{
  partial class WdgMain
  {
    private System.Windows.Forms.OpenFileDialog dlgDsOpen;
    private System.Windows.Forms.MainMenu mnuMain;
    private System.Windows.Forms.Panel pnlData;
    private CntToc wdgToc;
    internal CntMap wdgMap;
    private OptionButton optZoomIn;
    private OptionButton optZoomOut;
    private OptionButton optSelect;
    private System.Windows.Forms.TextBox txtPosition;
    private System.Windows.Forms.Splitter splToc;
    private System.Windows.Forms.Splitter splitter1;
    private CntSelection wdgSel;
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
      Settings.Default.Save();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgMain));
      this.dlgDsOpen = new System.Windows.Forms.OpenFileDialog();
      this.mnuMain = new System.Windows.Forms.MainMenu(this.components);
      this.pnlData = new System.Windows.Forms.Panel();
      this.wdgMap = new TMapWin.CntMap();
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.wdgSel = new TMapWin.CntSelection();
      this.splToc = new System.Windows.Forms.Splitter();
      this.wdgToc = new TMapWin.CntToc();
      this.txtPosition = new System.Windows.Forms.TextBox();
      this.mainMenu = new System.Windows.Forms.MenuStrip();
      this.mnuLayout = new System.Windows.Forms.ToolStripMenuItem();
      this.mniOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuData = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuLoad = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRefresh_ = new System.Windows.Forms.ToolStripMenuItem();
      this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.mniPlugins = new System.Windows.Forms.ToolStripMenuItem();
      this.mniPluginSepNew = new System.Windows.Forms.ToolStripSeparator();
      this.mniPluginNew = new System.Windows.Forms.ToolStripMenuItem();
      this.optMove = new TMapWin.OptionButton();
      this.optSelect = new TMapWin.OptionButton();
      this.optZoomOut = new TMapWin.OptionButton();
      this.optZoomIn = new TMapWin.OptionButton();
      this.pnlData.SuspendLayout();
      this.mainMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlData
      // 
      this.pnlData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlData.Controls.Add(this.wdgMap);
      this.pnlData.Controls.Add(this.splitter1);
      this.pnlData.Controls.Add(this.wdgSel);
      this.pnlData.Controls.Add(this.splToc);
      this.pnlData.Controls.Add(this.wdgToc);
      this.pnlData.Location = new System.Drawing.Point(8, 32);
      this.pnlData.Name = "pnlData";
      this.pnlData.Size = new System.Drawing.Size(712, 282);
      this.pnlData.TabIndex = 0;
      // 
      // wdgMap
      // 
      this.wdgMap.AllowDrop = true;
      this.wdgMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.wdgMap.Dock = System.Windows.Forms.DockStyle.Fill;
      this.wdgMap.Location = new System.Drawing.Point(154, 0);
      this.wdgMap.Name = "wdgMap";
      this.wdgMap.Size = new System.Drawing.Size(404, 282);
      this.wdgMap.TabIndex = 2;
      this.wdgMap.MouseMoveMap += new System.Windows.Forms.MouseEventHandler(this.wdgMap_MouseMove);
      this.wdgMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.data_DragDrop);
      this.wdgMap.Resize += new System.EventHandler(this.wdgMap_Resize);
      this.wdgMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.data_DragEnter);
      // 
      // splitter1
      // 
      this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
      this.splitter1.Location = new System.Drawing.Point(558, 0);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(4, 282);
      this.splitter1.TabIndex = 4;
      this.splitter1.TabStop = false;
      // 
      // wdgSel
      // 
      this.wdgSel.BackColor = System.Drawing.SystemColors.Control;
      this.wdgSel.Dock = System.Windows.Forms.DockStyle.Right;
      this.wdgSel.Location = new System.Drawing.Point(562, 0);
      this.wdgSel.Name = "wdgSel";
      this.wdgSel.Size = new System.Drawing.Size(150, 282);
      this.wdgSel.TabIndex = 3;
      // 
      // splToc
      // 
      this.splToc.BackColor = System.Drawing.SystemColors.Control;
      this.splToc.Location = new System.Drawing.Point(150, 0);
      this.splToc.Name = "splToc";
      this.splToc.Size = new System.Drawing.Size(4, 282);
      this.splToc.TabIndex = 1;
      this.splToc.TabStop = false;
      // 
      // wdgToc
      // 
      this.wdgToc.AllowDrop = true;
      this.wdgToc.Dock = System.Windows.Forms.DockStyle.Left;
      this.wdgToc.Location = new System.Drawing.Point(0, 0);
      this.wdgToc.Name = "wdgToc";
      this.wdgToc.Size = new System.Drawing.Size(150, 282);
      this.wdgToc.TabIndex = 0;
      this.wdgToc.DragDrop += new System.Windows.Forms.DragEventHandler(this.data_DragDrop);
      this.wdgToc.DragEnter += new System.Windows.Forms.DragEventHandler(this.data_DragEnter);
      // 
      // txtPosition
      // 
      this.txtPosition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.txtPosition.Location = new System.Drawing.Point(584, 320);
      this.txtPosition.Name = "txtPosition";
      this.txtPosition.ReadOnly = true;
      this.txtPosition.Size = new System.Drawing.Size(136, 20);
      this.txtPosition.TabIndex = 14;
      this.txtPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtPosition.Visible = false;
      // 
      // mainMenu
      // 
      this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLayout,
            this.mnuData,
            this.optionsToolStripMenuItem});
      this.mainMenu.Location = new System.Drawing.Point(0, 0);
      this.mainMenu.Name = "mainMenu";
      this.mainMenu.Size = new System.Drawing.Size(728, 24);
      this.mainMenu.TabIndex = 15;
      this.mainMenu.Text = "menuStrip1";
      // 
      // mnuLayout
      // 
      this.mnuLayout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniOpen,
            this.mnuSave});
      this.mnuLayout.Name = "mnuLayout";
      this.mnuLayout.Size = new System.Drawing.Size(52, 20);
      this.mnuLayout.Text = "Layout";
      // 
      // mniOpen
      // 
      this.mniOpen.Name = "mniOpen";
      this.mniOpen.Size = new System.Drawing.Size(100, 22);
      this.mniOpen.Text = "Open";
      // 
      // mnuSave
      // 
      this.mnuSave.Name = "mnuSave";
      this.mnuSave.Size = new System.Drawing.Size(100, 22);
      this.mnuSave.Text = "Save";
      // 
      // mnuData
      // 
      this.mnuData.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoad,
            this.mnuRefresh_});
      this.mnuData.Name = "mnuData";
      this.mnuData.Size = new System.Drawing.Size(42, 20);
      this.mnuData.Text = "Data";
      // 
      // mnuLoad
      // 
      this.mnuLoad.Name = "mnuLoad";
      this.mnuLoad.Size = new System.Drawing.Size(131, 22);
      this.mnuLoad.Text = "Load";
      this.mnuLoad.Click += new System.EventHandler(this.mnuLoad_Click);
      // 
      // mnuRefresh_
      // 
      this.mnuRefresh_.Name = "mnuRefresh_";
      this.mnuRefresh_.ShortcutKeyDisplayString = "";
      this.mnuRefresh_.ShortcutKeys = System.Windows.Forms.Keys.F5;
      this.mnuRefresh_.Size = new System.Drawing.Size(131, 22);
      this.mnuRefresh_.Text = "Refresh";
      this.mnuRefresh_.Click += new System.EventHandler(this.mnuRefresh_Click);
      // 
      // optionsToolStripMenuItem
      // 
      this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniPlugins});
      this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
      this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
      this.optionsToolStripMenuItem.Text = "Options";
      // 
      // mniPlugins
      // 
      this.mniPlugins.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniPluginSepNew,
            this.mniPluginNew});
      this.mniPlugins.Name = "mniPlugins";
      this.mniPlugins.Size = new System.Drawing.Size(107, 22);
      this.mniPlugins.Text = "Plugins";
      // 
      // mniPluginSepNew
      // 
      this.mniPluginSepNew.Name = "mniPluginSepNew";
      this.mniPluginSepNew.Size = new System.Drawing.Size(104, 6);
      // 
      // mniPluginNew
      // 
      this.mniPluginNew.Name = "mniPluginNew";
      this.mniPluginNew.Size = new System.Drawing.Size(107, 22);
      this.mniPluginNew.Text = "New...";
      this.mniPluginNew.Click += new System.EventHandler(this.mnuNew_Click);
      // 
      // optMove
      // 
      this.optMove.BackColor = System.Drawing.SystemColors.Control;
      this.optMove.Checked = false;
      this.optMove.Group = true;
      this.optMove.Image = ((System.Drawing.Image)(resources.GetObject("optMove.Image")));
      this.optMove.Location = new System.Drawing.Point(244, 0);
      this.optMove.Name = "optMove";
      this.optMove.Size = new System.Drawing.Size(24, 24);
      this.optMove.TabIndex = 16;
      this.optMove.UseVisualStyleBackColor = false;
      this.optMove.Click += new System.EventHandler(this.optMove_Click);
      // 
      // optSelect
      // 
      this.optSelect.BackColor = System.Drawing.SystemColors.Control;
      this.optSelect.Checked = false;
      this.optSelect.Group = true;
      this.optSelect.Image = ((System.Drawing.Image)(resources.GetObject("optSelect.Image")));
      this.optSelect.Location = new System.Drawing.Point(268, 0);
      this.optSelect.Name = "optSelect";
      this.optSelect.Size = new System.Drawing.Size(24, 24);
      this.optSelect.TabIndex = 13;
      this.optSelect.UseVisualStyleBackColor = false;
      this.optSelect.Click += new System.EventHandler(this.optSelect_Click);
      // 
      // optZoomOut
      // 
      this.optZoomOut.BackColor = System.Drawing.SystemColors.Control;
      this.optZoomOut.Checked = false;
      this.optZoomOut.Group = true;
      this.optZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("optZoomOut.Image")));
      this.optZoomOut.Location = new System.Drawing.Point(220, 0);
      this.optZoomOut.Name = "optZoomOut";
      this.optZoomOut.Size = new System.Drawing.Size(24, 24);
      this.optZoomOut.TabIndex = 12;
      this.optZoomOut.UseVisualStyleBackColor = false;
      this.optZoomOut.Click += new System.EventHandler(this.optZoomOut_Click);
      // 
      // optZoomIn
      // 
      this.optZoomIn.Checked = false;
      this.optZoomIn.Group = true;
      this.optZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("optZoomIn.Image")));
      this.optZoomIn.Location = new System.Drawing.Point(196, 0);
      this.optZoomIn.Name = "optZoomIn";
      this.optZoomIn.Size = new System.Drawing.Size(24, 24);
      this.optZoomIn.TabIndex = 11;
      this.optZoomIn.Click += new System.EventHandler(this.optZoomIn_Click);
      // 
      // WdgMain
      // 
      this.AllowDrop = true;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(728, 346);
      this.Controls.Add(this.optMove);
      this.Controls.Add(this.txtPosition);
      this.Controls.Add(this.optSelect);
      this.Controls.Add(this.optZoomOut);
      this.Controls.Add(this.optZoomIn);
      this.Controls.Add(this.pnlData);
      this.Controls.Add(this.mainMenu);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.mainMenu;
      this.Menu = this.mnuMain;
      this.Name = "WdgMain";
      this.Text = "TMap";
      this.Load += new System.EventHandler(this.WdgMain_Load);
      this.DragOver += new System.Windows.Forms.DragEventHandler(this.WdgMain_DragOver);
      this.pnlData.ResumeLayout(false);
      this.mainMenu.ResumeLayout(false);
      this.mainMenu.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private System.Windows.Forms.MenuStrip mainMenu;
    private System.Windows.Forms.ToolStripMenuItem mnuLayout;
    private System.Windows.Forms.ToolStripMenuItem mniOpen;
    private System.Windows.Forms.ToolStripMenuItem mnuSave;
    private System.Windows.Forms.ToolStripMenuItem mnuData;
    private System.Windows.Forms.ToolStripMenuItem mnuLoad;
    private System.Windows.Forms.ToolStripMenuItem mnuRefresh_;
    private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
    private OptionButton optMove;
    private System.Windows.Forms.ToolStripMenuItem mniPlugins;
    private System.Windows.Forms.ToolStripSeparator mniPluginSepNew;
    private System.Windows.Forms.ToolStripMenuItem mniPluginNew;
  }
}