namespace LeastCostPathUI
{
  partial class WdgCostDetail
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgCostDetail));
      this.cntConfig = new LeastCostPathUI.CntConfig();
      this.button1 = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.pnlTeleport = new System.Windows.Forms.Panel();
      this.lstTarget = new System.Windows.Forms.ComboBox();
      this.lblTeleTarget = new System.Windows.Forms.Label();
      this.lblTeleport = new System.Windows.Forms.Label();
      this.btnTeleport = new System.Windows.Forms.Button();
      this.txtTeleport = new System.Windows.Forms.TextBox();
      this.pnlTeleport.SuspendLayout();
      this.SuspendLayout();
      // 
      // cntConfig
      // 
      this.cntConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cntConfig.ConfigVm = null;
      this.cntConfig.Location = new System.Drawing.Point(2, 0);
      this.cntConfig.Name = "cntConfig";
      this.cntConfig.Size = new System.Drawing.Size(501, 98);
      this.cntConfig.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(355, 154);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(55, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(416, 155);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(56, 23);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // pnlTeleport
      // 
      this.pnlTeleport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlTeleport.Controls.Add(this.lstTarget);
      this.pnlTeleport.Controls.Add(this.lblTeleTarget);
      this.pnlTeleport.Controls.Add(this.lblTeleport);
      this.pnlTeleport.Controls.Add(this.btnTeleport);
      this.pnlTeleport.Controls.Add(this.txtTeleport);
      this.pnlTeleport.Location = new System.Drawing.Point(2, 93);
      this.pnlTeleport.Name = "pnlTeleport";
      this.pnlTeleport.Size = new System.Drawing.Size(501, 55);
      this.pnlTeleport.TabIndex = 34;
      // 
      // lstTarget
      // 
      this.lstTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lstTarget.FormattingEnabled = true;
      this.lstTarget.Location = new System.Drawing.Point(84, 29);
      this.lstTarget.Name = "lstTarget";
      this.lstTarget.Size = new System.Drawing.Size(389, 21);
      this.lstTarget.TabIndex = 34;
      // 
      // lblTeleTarget
      // 
      this.lblTeleTarget.AutoSize = true;
      this.lblTeleTarget.Location = new System.Drawing.Point(4, 32);
      this.lblTeleTarget.Name = "lblTeleTarget";
      this.lblTeleTarget.Size = new System.Drawing.Size(38, 13);
      this.lblTeleTarget.TabIndex = 33;
      this.lblTeleTarget.Text = "Target";
      // 
      // lblTeleport
      // 
      this.lblTeleport.AutoSize = true;
      this.lblTeleport.Location = new System.Drawing.Point(4, 8);
      this.lblTeleport.Name = "lblTeleport";
      this.lblTeleport.Size = new System.Drawing.Size(46, 13);
      this.lblTeleport.TabIndex = 31;
      this.lblTeleport.Text = "Teleport";
      // 
      // btnTeleport
      // 
      this.btnTeleport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTeleport.Image = ((System.Drawing.Image)(resources.GetObject("btnTeleport.Image")));
      this.btnTeleport.Location = new System.Drawing.Point(477, 4);
      this.btnTeleport.Name = "btnTeleport";
      this.btnTeleport.Size = new System.Drawing.Size(20, 20);
      this.btnTeleport.TabIndex = 32;
      // 
      // txtTeleport
      // 
      this.txtTeleport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTeleport.Location = new System.Drawing.Point(84, 5);
      this.txtTeleport.Name = "txtTeleport";
      this.txtTeleport.Size = new System.Drawing.Size(389, 20);
      this.txtTeleport.TabIndex = 30;
      this.txtTeleport.Text = "*.ocd";
      // 
      // WdgCostDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(503, 185);
      this.Controls.Add(this.pnlTeleport);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.cntConfig);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgCostDetail";
      this.Text = "Terrain Velocity Model";
      this.pnlTeleport.ResumeLayout(false);
      this.pnlTeleport.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private CntConfig cntConfig;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Panel pnlTeleport;
    private System.Windows.Forms.Label lblTeleTarget;
    private System.Windows.Forms.Label lblTeleport;
    private System.Windows.Forms.Button btnTeleport;
    private System.Windows.Forms.TextBox txtTeleport;
    private System.Windows.Forms.ComboBox lstTarget;
  }
}