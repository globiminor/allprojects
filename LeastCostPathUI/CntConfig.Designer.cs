namespace LeastCostPathUI
{
  partial class CntConfig
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CntConfig));
      this.txtCost = new System.Windows.Forms.TextBox();
      this.lblCost = new System.Windows.Forms.Label();
      this.lblSteps = new System.Windows.Forms.Label();
      this._lstStep = new System.Windows.Forms.ComboBox();
      this.txtResol = new System.Windows.Forms.TextBox();
      this.lblResol = new System.Windows.Forms.Label();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.txtVelo = new System.Windows.Forms.TextBox();
      this.txtHeight = new System.Windows.Forms.TextBox();
      this.lblVelo = new System.Windows.Forms.Label();
      this.lblHeight = new System.Windows.Forms.Label();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.lblVelotyp = new System.Windows.Forms.Label();
      this.btnStepCost = new System.Windows.Forms.Button();
      this.btnHeight = new System.Windows.Forms.Button();
      this.btnVelo = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // txtCost
      // 
      this.txtCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCost.Location = new System.Drawing.Point(318, 67);
      this.txtCost.Name = "txtCost";
      this.txtCost.ReadOnly = true;
      this.txtCost.Size = new System.Drawing.Size(76, 20);
      this.txtCost.TabIndex = 18;
      // 
      // lblCost
      // 
      this.lblCost.AutoSize = true;
      this.lblCost.Location = new System.Drawing.Point(241, 70);
      this.lblCost.Name = "lblCost";
      this.lblCost.Size = new System.Drawing.Size(56, 13);
      this.lblCost.TabIndex = 17;
      this.lblCost.Text = "Costmodel";
      // 
      // lblSteps
      // 
      this.lblSteps.AutoSize = true;
      this.lblSteps.Location = new System.Drawing.Point(138, 71);
      this.lblSteps.Name = "lblSteps";
      this.lblSteps.Size = new System.Drawing.Size(34, 13);
      this.lblSteps.TabIndex = 16;
      this.lblSteps.Text = "Steps";
      this.lblSteps.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // _lstStep
      // 
      this._lstStep.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this._lstStep.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstStep.FormattingEnabled = true;
      this._lstStep.ItemHeight = 18;
      this._lstStep.Location = new System.Drawing.Point(188, 66);
      this._lstStep.Name = "_lstStep";
      this._lstStep.Size = new System.Drawing.Size(47, 24);
      this._lstStep.TabIndex = 15;
      this._lstStep.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LstStep_DrawItem);
      // 
      // txtResol
      // 
      this.txtResol.Location = new System.Drawing.Point(84, 67);
      this.txtResol.Name = "txtResol";
      this.txtResol.Size = new System.Drawing.Size(37, 20);
      this.txtResol.TabIndex = 14;
      this.txtResol.Text = "1.0";
      this.txtResol.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblResol
      // 
      this.lblResol.AutoSize = true;
      this.lblResol.Location = new System.Drawing.Point(4, 70);
      this.lblResol.Name = "lblResol";
      this.lblResol.Size = new System.Drawing.Size(57, 13);
      this.lblResol.TabIndex = 13;
      this.lblResol.Text = "Resolution";
      // 
      // txtVelo
      // 
      this.txtVelo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtVelo.Location = new System.Drawing.Point(84, 28);
      this.txtVelo.Name = "txtVelo";
      this.txtVelo.Size = new System.Drawing.Size(310, 20);
      this.txtVelo.TabIndex = 23;
      this.txtVelo.Text = "*.tif";
      // 
      // txtHeight
      // 
      this.txtHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtHeight.Location = new System.Drawing.Point(84, 3);
      this.txtHeight.Name = "txtHeight";
      this.txtHeight.Size = new System.Drawing.Size(310, 20);
      this.txtHeight.TabIndex = 20;
      this.txtHeight.Text = "*.asc";
      // 
      // lblVelo
      // 
      this.lblVelo.AutoSize = true;
      this.lblVelo.Location = new System.Drawing.Point(2, 31);
      this.lblVelo.Name = "lblVelo";
      this.lblVelo.Size = new System.Drawing.Size(66, 13);
      this.lblVelo.TabIndex = 22;
      this.lblVelo.Text = "Velocity Grid";
      // 
      // lblHeight
      // 
      this.lblHeight.AutoSize = true;
      this.lblHeight.Location = new System.Drawing.Point(4, 6);
      this.lblHeight.Name = "lblHeight";
      this.lblHeight.Size = new System.Drawing.Size(62, 13);
      this.lblHeight.TabIndex = 21;
      this.lblHeight.Text = "Terrain Grid";
      // 
      // lblVelotyp
      // 
      this.lblVelotyp.AutoSize = true;
      this.lblVelotyp.Location = new System.Drawing.Point(81, 51);
      this.lblVelotyp.Name = "lblVelotyp";
      this.lblVelotyp.Size = new System.Drawing.Size(67, 13);
      this.lblVelotyp.TabIndex = 29;
      this.lblVelotyp.Text = "Velocity type";
      // 
      // btnStepCost
      // 
      this.btnStepCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStepCost.Image = ((System.Drawing.Image)(resources.GetObject("btnStepCost.Image")));
      this.btnStepCost.Location = new System.Drawing.Point(398, 67);
      this.btnStepCost.Name = "btnStepCost";
      this.btnStepCost.Size = new System.Drawing.Size(20, 20);
      this.btnStepCost.TabIndex = 19;
      this.btnStepCost.Click += new System.EventHandler(this.BtnStepCost_Click);
      // 
      // btnHeight
      // 
      this.btnHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnHeight.Image = ((System.Drawing.Image)(resources.GetObject("btnHeight.Image")));
      this.btnHeight.Location = new System.Drawing.Point(398, 2);
      this.btnHeight.Name = "btnHeight";
      this.btnHeight.Size = new System.Drawing.Size(20, 20);
      this.btnHeight.TabIndex = 24;
      this.btnHeight.Click += new System.EventHandler(this.BtnHeight_Click);
      // 
      // btnVelo
      // 
      this.btnVelo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnVelo.Image = ((System.Drawing.Image)(resources.GetObject("btnVelo.Image")));
      this.btnVelo.Location = new System.Drawing.Point(398, 28);
      this.btnVelo.Name = "btnVelo";
      this.btnVelo.Size = new System.Drawing.Size(20, 20);
      this.btnVelo.TabIndex = 25;
      this.btnVelo.Click += new System.EventHandler(this.BtnVelo_Click);
      // 
      // CntConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.lblVelotyp);
      this.Controls.Add(this.btnStepCost);
      this.Controls.Add(this.lblHeight);
      this.Controls.Add(this.txtCost);
      this.Controls.Add(this.lblVelo);
      this.Controls.Add(this.lblResol);
      this.Controls.Add(this.txtVelo);
      this.Controls.Add(this.lblCost);
      this.Controls.Add(this.btnHeight);
      this.Controls.Add(this.txtResol);
      this.Controls.Add(this.txtHeight);
      this.Controls.Add(this.btnVelo);
      this.Controls.Add(this.lblSteps);
      this.Controls.Add(this._lstStep);
      this.Name = "CntConfig";
      this.Size = new System.Drawing.Size(422, 96);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtResol;
    private System.Windows.Forms.Label lblResol;
    private System.Windows.Forms.ComboBox _lstStep;
    private System.Windows.Forms.TextBox txtCost;
    private System.Windows.Forms.Label lblCost;
    private System.Windows.Forms.Label lblSteps;
    private System.Windows.Forms.Button btnStepCost;
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.TextBox txtVelo;
    private System.Windows.Forms.TextBox txtHeight;
    private System.Windows.Forms.Button btnVelo;
    private System.Windows.Forms.Button btnHeight;
    private System.Windows.Forms.Label lblVelo;
    private System.Windows.Forms.Label lblHeight;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.Label lblVelotyp;
  }
}
