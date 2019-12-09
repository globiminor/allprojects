namespace Cards.Gui
{
  partial class FrmCards
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
        components?.Dispose();
        _bindingSource?.Dispose();
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
      this.btnSolve = new System.Windows.Forms.Button();
      this.txtMoves = new System.Windows.Forms.TextBox();
      this.lblMoves = new System.Windows.Forms.Label();
      this.lblDisp = new System.Windows.Forms.Label();
      this.txtDisp = new System.Windows.Forms.TextBox();
      this.txtInfo = new System.Windows.Forms.TextBox();
      this.lblInfo = new System.Windows.Forms.Label();
      this.btnRevert = new System.Windows.Forms.Button();
      this.btnSlow = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.mnuCards = new System.Windows.Forms.MenuStrip();
      this.mniSettings = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSpider = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSpider4 = new System.Windows.Forms.ToolStripMenuItem();
      this.mniTriPeaks = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.mniSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mniLoad = new System.Windows.Forms.ToolStripMenuItem();
      this.mniNew = new System.Windows.Forms.ToolStripMenuItem();
      this.txtPoints = new System.Windows.Forms.TextBox();
      this.lblPoints = new System.Windows.Forms.Label();
      this.cntCards = new Cards.Gui.CntCards();
      this.mniCapture = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuCards.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnSolve
      // 
      this.btnSolve.Location = new System.Drawing.Point(288, 33);
      this.btnSolve.Name = "btnSolve";
      this.btnSolve.Size = new System.Drawing.Size(75, 23);
      this.btnSolve.TabIndex = 1;
      this.btnSolve.Text = "Solve";
      this.btnSolve.UseVisualStyleBackColor = true;
      this.btnSolve.Click += new System.EventHandler(this.BtnSolve_Click);
      // 
      // txtMoves
      // 
      this.txtMoves.Location = new System.Drawing.Point(57, 34);
      this.txtMoves.Name = "txtMoves";
      this.txtMoves.ReadOnly = true;
      this.txtMoves.Size = new System.Drawing.Size(52, 20);
      this.txtMoves.TabIndex = 2;
      // 
      // lblMoves
      // 
      this.lblMoves.AutoSize = true;
      this.lblMoves.Location = new System.Drawing.Point(12, 36);
      this.lblMoves.Name = "lblMoves";
      this.lblMoves.Size = new System.Drawing.Size(39, 13);
      this.lblMoves.TabIndex = 3;
      this.lblMoves.Text = "Moves";
      // 
      // lblDisp
      // 
      this.lblDisp.AutoSize = true;
      this.lblDisp.Location = new System.Drawing.Point(383, 37);
      this.lblDisp.Name = "lblDisp";
      this.lblDisp.Size = new System.Drawing.Size(99, 13);
      this.lblDisp.TabIndex = 4;
      this.lblDisp.Text = "Showinterval [secs]";
      // 
      // txtDisp
      // 
      this.txtDisp.Location = new System.Drawing.Point(518, 34);
      this.txtDisp.Name = "txtDisp";
      this.txtDisp.Size = new System.Drawing.Size(52, 20);
      this.txtDisp.TabIndex = 5;
      this.txtDisp.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // txtInfo
      // 
      this.txtInfo.Location = new System.Drawing.Point(653, 34);
      this.txtInfo.Name = "txtInfo";
      this.txtInfo.ReadOnly = true;
      this.txtInfo.Size = new System.Drawing.Size(155, 20);
      this.txtInfo.TabIndex = 6;
      // 
      // lblInfo
      // 
      this.lblInfo.AutoSize = true;
      this.lblInfo.Location = new System.Drawing.Point(611, 37);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(25, 13);
      this.lblInfo.TabIndex = 7;
      this.lblInfo.Text = "Info";
      // 
      // btnRevert
      // 
      this.btnRevert.Location = new System.Drawing.Point(115, 32);
      this.btnRevert.Name = "btnRevert";
      this.btnRevert.Size = new System.Drawing.Size(26, 23);
      this.btnRevert.TabIndex = 8;
      this.btnRevert.Text = "-";
      this.btnRevert.UseVisualStyleBackColor = true;
      this.btnRevert.Click += new System.EventHandler(this.BtnRevert_Click);
      // 
      // btnSlow
      // 
      this.btnSlow.Location = new System.Drawing.Point(498, 34);
      this.btnSlow.Name = "btnSlow";
      this.btnSlow.Size = new System.Drawing.Size(20, 20);
      this.btnSlow.TabIndex = 9;
      this.btnSlow.Text = "-";
      this.btnSlow.UseVisualStyleBackColor = true;
      this.btnSlow.Click += new System.EventHandler(this.BtnSlow_Click);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(570, 34);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(20, 20);
      this.button1.TabIndex = 10;
      this.button1.Text = "+";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.Button1_Click);
      // 
      // mnuCards
      // 
      this.mnuCards.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSettings});
      this.mnuCards.Location = new System.Drawing.Point(0, 0);
      this.mnuCards.Name = "mnuCards";
      this.mnuCards.Size = new System.Drawing.Size(820, 24);
      this.mnuCards.TabIndex = 11;
      this.mnuCards.Text = "menuStrip1";
      // 
      // mniSettings
      // 
      this.mniSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSpider,
            this.mniTriPeaks,
            this.toolStripSeparator1,
            this.mniSave,
            this.mniLoad,
            this.mniNew,
            this.mniCapture});
      this.mniSettings.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.mniSettings.Name = "mniSettings";
      this.mniSettings.Size = new System.Drawing.Size(61, 20);
      this.mniSettings.Text = "Settings";
      // 
      // mniSpider
      // 
      this.mniSpider.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSpider4});
      this.mniSpider.Name = "mniSpider";
      this.mniSpider.Size = new System.Drawing.Size(152, 22);
      this.mniSpider.Text = "Spider";
      // 
      // mniSpider4
      // 
      this.mniSpider4.Name = "mniSpider4";
      this.mniSpider4.Size = new System.Drawing.Size(116, 22);
      this.mniSpider4.Text = "4 Colors";
      this.mniSpider4.Click += new System.EventHandler(this.MniSpider4_Click);
      // 
      // mniTriPeaks
      // 
      this.mniTriPeaks.Name = "mniTriPeaks";
      this.mniTriPeaks.Size = new System.Drawing.Size(152, 22);
      this.mniTriPeaks.Text = "Tri Peaks";
      this.mniTriPeaks.Click += new System.EventHandler(this.MniTriPeaks_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
      // 
      // mniSave
      // 
      this.mniSave.Name = "mniSave";
      this.mniSave.Size = new System.Drawing.Size(152, 22);
      this.mniSave.Text = "Save";
      this.mniSave.Click += new System.EventHandler(this.MniSave_Click);
      // 
      // mniLoad
      // 
      this.mniLoad.Name = "mniLoad";
      this.mniLoad.Size = new System.Drawing.Size(152, 22);
      this.mniLoad.Text = "Load";
      this.mniLoad.Click += new System.EventHandler(this.MniLoad_Click);
      // 
      // mniNew
      // 
      this.mniNew.Name = "mniNew";
      this.mniNew.Size = new System.Drawing.Size(152, 22);
      this.mniNew.Text = "New";
      this.mniNew.Click += new System.EventHandler(this.MniNew_Click);
      // 
      // txtPoints
      // 
      this.txtPoints.Location = new System.Drawing.Point(213, 33);
      this.txtPoints.Name = "txtPoints";
      this.txtPoints.ReadOnly = true;
      this.txtPoints.Size = new System.Drawing.Size(52, 20);
      this.txtPoints.TabIndex = 12;
      // 
      // lblPoints
      // 
      this.lblPoints.AutoSize = true;
      this.lblPoints.Location = new System.Drawing.Point(168, 38);
      this.lblPoints.Name = "lblPoints";
      this.lblPoints.Size = new System.Drawing.Size(36, 13);
      this.lblPoints.TabIndex = 13;
      this.lblPoints.Text = "Points";
      // 
      // cntCards
      // 
      this.cntCards.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cntCards.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.cntCards.CardHeight = 30;
      this.cntCards.CardsVm = null;
      this.cntCards.CardWidth = 30;
      this.cntCards.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cntCards.Location = new System.Drawing.Point(12, 61);
      this.cntCards.Name = "cntCards";
      this.cntCards.Size = new System.Drawing.Size(796, 334);
      this.cntCards.TabIndex = 0;
      // 
      // mniCapture
      // 
      this.mniCapture.Name = "mniCapture";
      this.mniCapture.Size = new System.Drawing.Size(152, 22);
      this.mniCapture.Text = "Capture";
      this.mniCapture.Click += new System.EventHandler(this.MniCapture_Click);
      // 
      // FrmCards
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(820, 407);
      this.Controls.Add(this.lblPoints);
      this.Controls.Add(this.txtPoints);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.btnSlow);
      this.Controls.Add(this.btnRevert);
      this.Controls.Add(this.lblInfo);
      this.Controls.Add(this.txtInfo);
      this.Controls.Add(this.txtDisp);
      this.Controls.Add(this.lblDisp);
      this.Controls.Add(this.lblMoves);
      this.Controls.Add(this.txtMoves);
      this.Controls.Add(this.btnSolve);
      this.Controls.Add(this.cntCards);
      this.Controls.Add(this.mnuCards);
      this.MainMenuStrip = this.mnuCards;
      this.Name = "FrmCards";
      this.Text = "Cards";
      this.Load += new System.EventHandler(this.FrmCards_Load);
      this.mnuCards.ResumeLayout(false);
      this.mnuCards.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private CntCards cntCards;
    private System.Windows.Forms.Button btnSolve;
    private System.Windows.Forms.TextBox txtMoves;
    private System.Windows.Forms.Label lblMoves;
    private System.Windows.Forms.Label lblDisp;
    private System.Windows.Forms.TextBox txtDisp;
    private System.Windows.Forms.TextBox txtInfo;
    private System.Windows.Forms.Label lblInfo;
    private System.Windows.Forms.Button btnRevert;
    private System.Windows.Forms.Button btnSlow;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.MenuStrip mnuCards;
    private System.Windows.Forms.ToolStripMenuItem mniSettings;
    private System.Windows.Forms.TextBox txtPoints;
    private System.Windows.Forms.Label lblPoints;
    private System.Windows.Forms.ToolStripMenuItem mniSpider;
    private System.Windows.Forms.ToolStripMenuItem mniSpider4;
    private System.Windows.Forms.ToolStripMenuItem mniTriPeaks;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem mniSave;
    private System.Windows.Forms.ToolStripMenuItem mniLoad;
    private System.Windows.Forms.ToolStripMenuItem mniNew;
    private System.Windows.Forms.ToolStripMenuItem mniCapture;
  }
}

