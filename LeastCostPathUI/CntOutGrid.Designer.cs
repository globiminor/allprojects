namespace LeastCostPathUI
{
  partial class CntOutGrid
  {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CntOutGrid));
      this.chkSynchron = new System.Windows.Forms.CheckBox();
      this.chkCost = new System.Windows.Forms.CheckBox();
      this.chkCostImg = new System.Windows.Forms.CheckBox();
      this.chkDir = new System.Windows.Forms.CheckBox();
      this.chkDirImg = new System.Windows.Forms.CheckBox();
      this.txtCost = new System.Windows.Forms.TextBox();
      this.txtCostImg = new System.Windows.Forms.TextBox();
      this.txtDir = new System.Windows.Forms.TextBox();
      this.txtDirImg = new System.Windows.Forms.TextBox();
      this.btnCost = new System.Windows.Forms.Button();
      this.btnCostImg = new System.Windows.Forms.Button();
      this.btnDir = new System.Windows.Forms.Button();
      this.btnDirImg = new System.Windows.Forms.Button();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.txtX = new System.Windows.Forms.TextBox();
      this.txtY = new System.Windows.Forms.TextBox();
      this.lblX = new System.Windows.Forms.Label();
      this.lblY = new System.Windows.Forms.Label();
      this.grpCoord = new System.Windows.Forms.Panel();
      this.grpCoord.SuspendLayout();
      this.SuspendLayout();
      // 
      // chkSynchron
      // 
      this.chkSynchron.Checked = true;
      this.chkSynchron.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkSynchron.Location = new System.Drawing.Point(0, 0);
      this.chkSynchron.Name = "chkSynchron";
      this.chkSynchron.Size = new System.Drawing.Size(128, 16);
      this.chkSynchron.TabIndex = 0;
      this.chkSynchron.Text = "Synchronize names";
      // 
      // chkCost
      // 
      this.chkCost.Location = new System.Drawing.Point(16, 24);
      this.chkCost.Name = "chkCost";
      this.chkCost.Size = new System.Drawing.Size(104, 16);
      this.chkCost.TabIndex = 1;
      this.chkCost.Text = "Cost";
      // 
      // chkCostImg
      // 
      this.chkCostImg.Location = new System.Drawing.Point(16, 48);
      this.chkCostImg.Name = "chkCostImg";
      this.chkCostImg.Size = new System.Drawing.Size(104, 16);
      this.chkCostImg.TabIndex = 2;
      this.chkCostImg.Text = "Cost Image";
      // 
      // chkDir
      // 
      this.chkDir.Location = new System.Drawing.Point(16, 72);
      this.chkDir.Name = "chkDir";
      this.chkDir.Size = new System.Drawing.Size(104, 16);
      this.chkDir.TabIndex = 3;
      this.chkDir.Text = "Direction";
      // 
      // chkDirImg
      // 
      this.chkDirImg.Location = new System.Drawing.Point(16, 96);
      this.chkDirImg.Name = "chkDirImg";
      this.chkDirImg.Size = new System.Drawing.Size(104, 16);
      this.chkDirImg.TabIndex = 4;
      this.chkDirImg.Text = "Direction Image";
      // 
      // txtCost
      // 
      this.txtCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCost.Location = new System.Drawing.Point(136, 24);
      this.txtCost.Name = "txtCost";
      this.txtCost.Size = new System.Drawing.Size(256, 20);
      this.txtCost.TabIndex = 5;
      this.txtCost.Text = "*c.grd";
      this.txtCost.TextChanged += new System.EventHandler(this.txtCost_TextChanged);
      // 
      // txtCostImg
      // 
      this.txtCostImg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCostImg.Location = new System.Drawing.Point(136, 48);
      this.txtCostImg.Name = "txtCostImg";
      this.txtCostImg.Size = new System.Drawing.Size(256, 20);
      this.txtCostImg.TabIndex = 6;
      this.txtCostImg.Text = "*c.tif";
      this.txtCostImg.TextChanged += new System.EventHandler(this.txtCostImg_TextChanged);
      // 
      // txtDir
      // 
      this.txtDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtDir.Location = new System.Drawing.Point(136, 72);
      this.txtDir.Name = "txtDir";
      this.txtDir.Size = new System.Drawing.Size(256, 20);
      this.txtDir.TabIndex = 7;
      this.txtDir.Text = "*d.grd";
      this.txtDir.TextChanged += new System.EventHandler(this.txtDir_TextChanged);
      // 
      // txtDirImg
      // 
      this.txtDirImg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtDirImg.Location = new System.Drawing.Point(136, 96);
      this.txtDirImg.Name = "txtDirImg";
      this.txtDirImg.Size = new System.Drawing.Size(256, 20);
      this.txtDirImg.TabIndex = 8;
      this.txtDirImg.Text = "*d.tif";
      this.txtDirImg.TextChanged += new System.EventHandler(this.txtDirImg_TextChanged);
      // 
      // btnCost
      // 
      this.btnCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCost.Image = ((System.Drawing.Image)(resources.GetObject("btnCost.Image")));
      this.btnCost.Location = new System.Drawing.Point(392, 24);
      this.btnCost.Name = "btnCost";
      this.btnCost.Size = new System.Drawing.Size(20, 20);
      this.btnCost.TabIndex = 9;
      this.btnCost.Click += new System.EventHandler(this.btnCost_Click);
      // 
      // btnCostImg
      // 
      this.btnCostImg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCostImg.Image = ((System.Drawing.Image)(resources.GetObject("btnCostImg.Image")));
      this.btnCostImg.Location = new System.Drawing.Point(392, 48);
      this.btnCostImg.Name = "btnCostImg";
      this.btnCostImg.Size = new System.Drawing.Size(20, 20);
      this.btnCostImg.TabIndex = 10;
      this.btnCostImg.Click += new System.EventHandler(this.btnCostImg_Click);
      // 
      // btnDir
      // 
      this.btnDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDir.Image = ((System.Drawing.Image)(resources.GetObject("btnDir.Image")));
      this.btnDir.Location = new System.Drawing.Point(392, 72);
      this.btnDir.Name = "btnDir";
      this.btnDir.Size = new System.Drawing.Size(20, 20);
      this.btnDir.TabIndex = 11;
      this.btnDir.Click += new System.EventHandler(this.btnDir_Click);
      // 
      // btnDirImg
      // 
      this.btnDirImg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDirImg.Image = ((System.Drawing.Image)(resources.GetObject("btnDirImg.Image")));
      this.btnDirImg.Location = new System.Drawing.Point(392, 96);
      this.btnDirImg.Name = "btnDirImg";
      this.btnDirImg.Size = new System.Drawing.Size(20, 20);
      this.btnDirImg.TabIndex = 12;
      this.btnDirImg.Click += new System.EventHandler(this.btnDirImg_Click);
      // 
      // txtX
      // 
      this.txtX.Location = new System.Drawing.Point(24, 0);
      this.txtX.Name = "txtX";
      this.txtX.Size = new System.Drawing.Size(80, 20);
      this.txtX.TabIndex = 13;
      this.txtX.Text = "0";
      this.txtX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // txtY
      // 
      this.txtY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtY.Location = new System.Drawing.Point(176, 0);
      this.txtY.Name = "txtY";
      this.txtY.Size = new System.Drawing.Size(80, 20);
      this.txtY.TabIndex = 14;
      this.txtY.Text = "0";
      this.txtY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblX
      // 
      this.lblX.Location = new System.Drawing.Point(0, 0);
      this.lblX.Name = "lblX";
      this.lblX.Size = new System.Drawing.Size(24, 16);
      this.lblX.TabIndex = 15;
      this.lblX.Text = "X";
      // 
      // lblY
      // 
      this.lblY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lblY.Location = new System.Drawing.Point(152, 0);
      this.lblY.Name = "lblY";
      this.lblY.Size = new System.Drawing.Size(24, 16);
      this.lblY.TabIndex = 16;
      this.lblY.Text = "Y";
      // 
      // grpCoord
      // 
      this.grpCoord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpCoord.Controls.Add(this.txtX);
      this.grpCoord.Controls.Add(this.lblX);
      this.grpCoord.Controls.Add(this.txtY);
      this.grpCoord.Controls.Add(this.lblY);
      this.grpCoord.Location = new System.Drawing.Point(136, 0);
      this.grpCoord.Name = "grpCoord";
      this.grpCoord.Size = new System.Drawing.Size(256, 24);
      this.grpCoord.TabIndex = 17;
      // 
      // CntOutGrid
      // 
      this.Controls.Add(this.grpCoord);
      this.Controls.Add(this.btnDirImg);
      this.Controls.Add(this.btnDir);
      this.Controls.Add(this.btnCostImg);
      this.Controls.Add(this.btnCost);
      this.Controls.Add(this.txtDirImg);
      this.Controls.Add(this.txtDir);
      this.Controls.Add(this.txtCostImg);
      this.Controls.Add(this.txtCost);
      this.Controls.Add(this.chkDirImg);
      this.Controls.Add(this.chkDir);
      this.Controls.Add(this.chkCostImg);
      this.Controls.Add(this.chkCost);
      this.Controls.Add(this.chkSynchron);
      this.Name = "CntOutGrid";
      this.Size = new System.Drawing.Size(416, 120);
      this.grpCoord.ResumeLayout(false);
      this.grpCoord.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    /// <summary>
    /// designer variables
    /// </summary>
    private System.Windows.Forms.CheckBox chkSynchron;
    private System.Windows.Forms.CheckBox chkCost;
    private System.Windows.Forms.CheckBox chkCostImg;
    private System.Windows.Forms.CheckBox chkDir;
    private System.Windows.Forms.CheckBox chkDirImg;
    private System.Windows.Forms.TextBox txtCost;
    private System.Windows.Forms.TextBox txtCostImg;
    private System.Windows.Forms.TextBox txtDir;
    private System.Windows.Forms.TextBox txtDirImg;
    private System.Windows.Forms.Button btnCost;
    private System.Windows.Forms.Button btnCostImg;
    private System.Windows.Forms.Button btnDir;
    private System.Windows.Forms.Button btnDirImg;
    public System.Windows.Forms.SaveFileDialog dlgSave;
    private System.Windows.Forms.TextBox txtX;
    private System.Windows.Forms.TextBox txtY;
    private System.Windows.Forms.Label lblX;
    private System.Windows.Forms.Label lblY;
    private System.Windows.Forms.Panel grpCoord;

  }
}
