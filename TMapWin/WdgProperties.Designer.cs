namespace TMapWin
{
  partial class WdgProperties
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

    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this._tabDisplay = new System.Windows.Forms.TabControl();
      this._tpgDisplay = new System.Windows.Forms.TabPage();
      this._txtWhere = new System.Windows.Forms.TextBox();
      this._lblWhere = new System.Windows.Forms.Label();
      this._chkVisible = new System.Windows.Forms.CheckBox();
      this._tpgSymbol = new System.Windows.Forms.TabPage();
      this._btnDown = new System.Windows.Forms.Button();
      this._btnUp = new System.Windows.Forms.Button();
      this._grdSymbols = new System.Windows.Forms.DataGridView();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.txtTransparency = new System.Windows.Forms.TextBox();
      this.lblTransparency = new System.Windows.Forms.Label();
      this._tabDisplay.SuspendLayout();
      this._tpgDisplay.SuspendLayout();
      this._tpgSymbol.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this._grdSymbols)).BeginInit();
      this.SuspendLayout();
      // 
      // _tabDisplay
      // 
      this._tabDisplay.Controls.Add(this._tpgDisplay);
      this._tabDisplay.Controls.Add(this._tpgSymbol);
      this._tabDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this._tabDisplay.Location = new System.Drawing.Point(0, 0);
      this._tabDisplay.Name = "_tabDisplay";
      this._tabDisplay.SelectedIndex = 0;
      this._tabDisplay.Size = new System.Drawing.Size(336, 273);
      this._tabDisplay.TabIndex = 0;
      // 
      // _tpgDisplay
      // 
      this._tpgDisplay.Controls.Add(this.lblTransparency);
      this._tpgDisplay.Controls.Add(this.txtTransparency);
      this._tpgDisplay.Controls.Add(this._txtWhere);
      this._tpgDisplay.Controls.Add(this._lblWhere);
      this._tpgDisplay.Controls.Add(this._chkVisible);
      this._tpgDisplay.Location = new System.Drawing.Point(4, 22);
      this._tpgDisplay.Name = "_tpgDisplay";
      this._tpgDisplay.Padding = new System.Windows.Forms.Padding(3);
      this._tpgDisplay.Size = new System.Drawing.Size(328, 247);
      this._tpgDisplay.TabIndex = 1;
      this._tpgDisplay.Text = "Display";
      this._tpgDisplay.UseVisualStyleBackColor = true;
      // 
      // _txtWhere
      // 
      this._txtWhere.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtWhere.Location = new System.Drawing.Point(84, 39);
      this._txtWhere.Name = "_txtWhere";
      this._txtWhere.Size = new System.Drawing.Size(238, 20);
      this._txtWhere.TabIndex = 2;
      this._txtWhere.TextChanged += new System.EventHandler(this.TxtWhere_TextChanged);
      // 
      // _lblWhere
      // 
      this._lblWhere.AutoSize = true;
      this._lblWhere.Location = new System.Drawing.Point(6, 42);
      this._lblWhere.Name = "_lblWhere";
      this._lblWhere.Size = new System.Drawing.Size(39, 13);
      this._lblWhere.TabIndex = 1;
      this._lblWhere.Text = "Where";
      // 
      // _chkVisible
      // 
      this._chkVisible.AutoSize = true;
      this._chkVisible.Location = new System.Drawing.Point(6, 6);
      this._chkVisible.Name = "_chkVisible";
      this._chkVisible.Size = new System.Drawing.Size(56, 17);
      this._chkVisible.TabIndex = 0;
      this._chkVisible.Text = "Visible";
      this._chkVisible.UseVisualStyleBackColor = true;
      this._chkVisible.CheckedChanged += new System.EventHandler(this.ChkVisible_CheckedChanged);
      // 
      // _tpgSymbol
      // 
      this._tpgSymbol.Controls.Add(this._btnDown);
      this._tpgSymbol.Controls.Add(this._btnUp);
      this._tpgSymbol.Controls.Add(this._grdSymbols);
      this._tpgSymbol.Location = new System.Drawing.Point(4, 22);
      this._tpgSymbol.Name = "_tpgSymbol";
      this._tpgSymbol.Size = new System.Drawing.Size(328, 247);
      this._tpgSymbol.TabIndex = 0;
      this._tpgSymbol.Text = "Symology";
      this._tpgSymbol.UseVisualStyleBackColor = true;
      // 
      // _btnDown
      // 
      this._btnDown.Location = new System.Drawing.Point(296, 56);
      this._btnDown.Name = "_btnDown";
      this._btnDown.Size = new System.Drawing.Size(24, 24);
      this._btnDown.TabIndex = 4;
      this._btnDown.Click += new System.EventHandler(this.BtnDown_Click);
      // 
      // _btnUp
      // 
      this._btnUp.Location = new System.Drawing.Point(296, 32);
      this._btnUp.Name = "_btnUp";
      this._btnUp.Size = new System.Drawing.Size(24, 23);
      this._btnUp.TabIndex = 3;
      this._btnUp.Click += new System.EventHandler(this.BtnUp_Click);
      // 
      // _grdSymbols
      // 
      this._grdSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._grdSymbols.Location = new System.Drawing.Point(8, 8);
      this._grdSymbols.Name = "_grdSymbols";
      this._grdSymbols.RowHeadersWidth = 16;
      this._grdSymbols.Size = new System.Drawing.Size(280, 232);
      this._grdSymbols.TabIndex = 2;
      this._grdSymbols.DoubleClick += new System.EventHandler(this.GrdSymbols_DoubleClick);
      this._grdSymbols.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GrdSymbols_MouseUp);
      // 
      // txtTransparency
      // 
      this.txtTransparency.Location = new System.Drawing.Point(84, 66);
      this.txtTransparency.Name = "txtTransparency";
      this.txtTransparency.Size = new System.Drawing.Size(83, 20);
      this.txtTransparency.TabIndex = 3;
      // 
      // lblTransparency
      // 
      this.lblTransparency.AutoSize = true;
      this.lblTransparency.Location = new System.Drawing.Point(6, 69);
      this.lblTransparency.Name = "lblTransparency";
      this.lblTransparency.Size = new System.Drawing.Size(72, 13);
      this.lblTransparency.TabIndex = 4;
      this.lblTransparency.Text = "Transparency";
      // 
      // WdgProperties
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(336, 273);
      this.Controls.Add(this._tabDisplay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgProperties";
      this.ShowInTaskbar = false;
      this.Text = "Properties";
      this.VisibleChanged += new System.EventHandler(this.WdgProperties_VisibleChanged);
      this._tabDisplay.ResumeLayout(false);
      this._tpgDisplay.ResumeLayout(false);
      this._tpgDisplay.PerformLayout();
      this._tpgSymbol.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this._grdSymbols)).EndInit();
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.TabControl _tabDisplay;
    private System.Windows.Forms.TabPage _tpgDisplay;
    private System.Windows.Forms.TextBox _txtWhere;
    private System.Windows.Forms.Label _lblWhere;
    private System.Windows.Forms.CheckBox _chkVisible;
    private System.Windows.Forms.TabPage _tpgSymbol;
    private System.Windows.Forms.Button _btnDown;
    private System.Windows.Forms.Button _btnUp;
    private System.Windows.Forms.DataGridView _grdSymbols;
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.Label lblTransparency;
    private System.Windows.Forms.TextBox txtTransparency;
  }
}