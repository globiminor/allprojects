﻿namespace LeastCostPathUI
{
  partial class CntConfigView
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CntConfigView));
      this.btnStepCost = new System.Windows.Forms.Button();
      this.txtCost = new System.Windows.Forms.TextBox();
      this.lblCost = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this._lstStep = new System.Windows.Forms.ComboBox();
      this.txtResol = new System.Windows.Forms.TextBox();
      this.lblResol = new System.Windows.Forms.Label();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.txtVelo = new System.Windows.Forms.TextBox();
      this.txtHeight = new System.Windows.Forms.TextBox();
      this.btnVelo = new System.Windows.Forms.Button();
      this.btnHeight = new System.Windows.Forms.Button();
      this.lblVelo = new System.Windows.Forms.Label();
      this.lblHeight = new System.Windows.Forms.Label();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.optHeightVelo = new System.Windows.Forms.RadioButton();
      this.optMultiLayer = new System.Windows.Forms.RadioButton();
      this.pnlTerrainVelo = new System.Windows.Forms.Panel();
      this.label2 = new System.Windows.Forms.Label();
      this.grdLevels = new System.Windows.Forms.DataGridView();
      this.pnlMulti = new System.Windows.Forms.Panel();
      this.btnAddLevel = new System.Windows.Forms.Button();
      this.pnlTerrainVelo.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdLevels)).BeginInit();
      this.pnlMulti.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnStepCost
      // 
      this.btnStepCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStepCost.Image = ((System.Drawing.Image)(resources.GetObject("btnStepCost.Image")));
      this.btnStepCost.Location = new System.Drawing.Point(399, 1);
      this.btnStepCost.Name = "btnStepCost";
      this.btnStepCost.Size = new System.Drawing.Size(20, 20);
      this.btnStepCost.TabIndex = 19;
      this.btnStepCost.Click += new System.EventHandler(this.BtnStepCost_Click);
      // 
      // txtCost
      // 
      this.txtCost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCost.Location = new System.Drawing.Point(276, 1);
      this.txtCost.Name = "txtCost";
      this.txtCost.ReadOnly = true;
      this.txtCost.Size = new System.Drawing.Size(119, 20);
      this.txtCost.TabIndex = 18;
      // 
      // lblCost
      // 
      this.lblCost.AutoSize = true;
      this.lblCost.Location = new System.Drawing.Point(242, 4);
      this.lblCost.Name = "lblCost";
      this.lblCost.Size = new System.Drawing.Size(28, 13);
      this.lblCost.TabIndex = 17;
      this.lblCost.Text = "Cost";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(149, 4);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(34, 13);
      this.label1.TabIndex = 16;
      this.label1.Text = "Steps";
      // 
      // _lstStep
      // 
      this._lstStep.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this._lstStep.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstStep.FormattingEnabled = true;
      this._lstStep.ItemHeight = 18;
      this._lstStep.Location = new System.Drawing.Point(189, 0);
      this._lstStep.Name = "_lstStep";
      this._lstStep.Size = new System.Drawing.Size(47, 24);
      this._lstStep.TabIndex = 15;
      this._lstStep.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LstStep_DrawItem);
      // 
      // txtResol
      // 
      this.txtResol.Location = new System.Drawing.Point(85, 1);
      this.txtResol.Name = "txtResol";
      this.txtResol.Size = new System.Drawing.Size(37, 20);
      this.txtResol.TabIndex = 14;
      this.txtResol.Text = "1.0";
      this.txtResol.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblResol
      // 
      this.lblResol.AutoSize = true;
      this.lblResol.Location = new System.Drawing.Point(5, 4);
      this.lblResol.Name = "lblResol";
      this.lblResol.Size = new System.Drawing.Size(57, 13);
      this.lblResol.TabIndex = 13;
      this.lblResol.Text = "Resolution";
      // 
      // txtVelo
      // 
      this.txtVelo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtVelo.Location = new System.Drawing.Point(85, 31);
      this.txtVelo.Name = "txtVelo";
      this.txtVelo.Size = new System.Drawing.Size(310, 20);
      this.txtVelo.TabIndex = 23;
      this.txtVelo.Text = "*.tif";
      // 
      // txtHeight
      // 
      this.txtHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtHeight.Location = new System.Drawing.Point(85, 6);
      this.txtHeight.Name = "txtHeight";
      this.txtHeight.Size = new System.Drawing.Size(310, 20);
      this.txtHeight.TabIndex = 20;
      this.txtHeight.Text = "*.asc";
      // 
      // btnVelo
      // 
      this.btnVelo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnVelo.Image = ((System.Drawing.Image)(resources.GetObject("btnVelo.Image")));
      this.btnVelo.Location = new System.Drawing.Point(399, 31);
      this.btnVelo.Name = "btnVelo";
      this.btnVelo.Size = new System.Drawing.Size(20, 20);
      this.btnVelo.TabIndex = 25;
      this.btnVelo.Click += new System.EventHandler(this.BtnVelo_Click);
      // 
      // btnHeight
      // 
      this.btnHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnHeight.Image = ((System.Drawing.Image)(resources.GetObject("btnHeight.Image")));
      this.btnHeight.Location = new System.Drawing.Point(399, 5);
      this.btnHeight.Name = "btnHeight";
      this.btnHeight.Size = new System.Drawing.Size(20, 20);
      this.btnHeight.TabIndex = 24;
      this.btnHeight.Click += new System.EventHandler(this.BtnHeight_Click);
      // 
      // lblVelo
      // 
      this.lblVelo.AutoSize = true;
      this.lblVelo.Location = new System.Drawing.Point(3, 34);
      this.lblVelo.Name = "lblVelo";
      this.lblVelo.Size = new System.Drawing.Size(66, 13);
      this.lblVelo.TabIndex = 22;
      this.lblVelo.Text = "Velocity Grid";
      // 
      // lblHeight
      // 
      this.lblHeight.AutoSize = true;
      this.lblHeight.Location = new System.Drawing.Point(5, 9);
      this.lblHeight.Name = "lblHeight";
      this.lblHeight.Size = new System.Drawing.Size(62, 13);
      this.lblHeight.TabIndex = 21;
      this.lblHeight.Text = "Terrain Grid";
      // 
      // optHeightVelo
      // 
      this.optHeightVelo.AutoSize = true;
      this.optHeightVelo.Location = new System.Drawing.Point(6, 25);
      this.optHeightVelo.Name = "optHeightVelo";
      this.optHeightVelo.Size = new System.Drawing.Size(127, 17);
      this.optHeightVelo.TabIndex = 26;
      this.optHeightVelo.TabStop = true;
      this.optHeightVelo.Text = "TerrainVelocity Model";
      this.optHeightVelo.UseVisualStyleBackColor = true;
      // 
      // optMultiLayer
      // 
      this.optMultiLayer.AutoSize = true;
      this.optMultiLayer.Location = new System.Drawing.Point(152, 25);
      this.optMultiLayer.Name = "optMultiLayer";
      this.optMultiLayer.Size = new System.Drawing.Size(108, 17);
      this.optMultiLayer.TabIndex = 27;
      this.optMultiLayer.TabStop = true;
      this.optMultiLayer.Text = "Multi Level Model";
      this.optMultiLayer.UseVisualStyleBackColor = true;
      // 
      // pnlTerrainVelo
      // 
      this.pnlTerrainVelo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlTerrainVelo.Controls.Add(this.label2);
      this.pnlTerrainVelo.Controls.Add(this.lblHeight);
      this.pnlTerrainVelo.Controls.Add(this.lblVelo);
      this.pnlTerrainVelo.Controls.Add(this.txtVelo);
      this.pnlTerrainVelo.Controls.Add(this.btnHeight);
      this.pnlTerrainVelo.Controls.Add(this.txtHeight);
      this.pnlTerrainVelo.Controls.Add(this.btnVelo);
      this.pnlTerrainVelo.Location = new System.Drawing.Point(0, 43);
      this.pnlTerrainVelo.Name = "pnlTerrainVelo";
      this.pnlTerrainVelo.Size = new System.Drawing.Size(419, 71);
      this.pnlTerrainVelo.TabIndex = 28;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(82, 54);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(67, 13);
      this.label2.TabIndex = 29;
      this.label2.Text = "Velocity type";
      // 
      // grdLevels
      // 
      this.grdLevels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grdLevels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grdLevels.Location = new System.Drawing.Point(25, 0);
      this.grdLevels.Name = "grdLevels";
      this.grdLevels.Size = new System.Drawing.Size(394, 70);
      this.grdLevels.TabIndex = 29;
      // 
      // pnlMulti
      // 
      this.pnlMulti.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlMulti.Controls.Add(this.btnAddLevel);
      this.pnlMulti.Controls.Add(this.grdLevels);
      this.pnlMulti.Location = new System.Drawing.Point(0, 44);
      this.pnlMulti.Name = "pnlMulti";
      this.pnlMulti.Size = new System.Drawing.Size(419, 70);
      this.pnlMulti.TabIndex = 30;
      // 
      // btnAddLevel
      // 
      this.btnAddLevel.Image = global::LeastCostPathUI.Images.Add;
      this.btnAddLevel.Location = new System.Drawing.Point(2, 42);
      this.btnAddLevel.Name = "btnAddLevel";
      this.btnAddLevel.Size = new System.Drawing.Size(23, 23);
      this.btnAddLevel.TabIndex = 30;
      this.btnAddLevel.UseVisualStyleBackColor = true;
      this.btnAddLevel.Click += new System.EventHandler(this.BtnAddLevel_Click);
      // 
      // CntConfigView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.pnlMulti);
      this.Controls.Add(this.optMultiLayer);
      this.Controls.Add(this.optHeightVelo);
      this.Controls.Add(this.btnStepCost);
      this.Controls.Add(this.txtCost);
      this.Controls.Add(this.lblResol);
      this.Controls.Add(this.lblCost);
      this.Controls.Add(this.txtResol);
      this.Controls.Add(this.label1);
      this.Controls.Add(this._lstStep);
      this.Controls.Add(this.pnlTerrainVelo);
      this.Name = "CntConfigView";
      this.Size = new System.Drawing.Size(422, 114);
      this.pnlTerrainVelo.ResumeLayout(false);
      this.pnlTerrainVelo.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdLevels)).EndInit();
      this.pnlMulti.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtResol;
    private System.Windows.Forms.Label lblResol;
    private System.Windows.Forms.ComboBox _lstStep;
    private System.Windows.Forms.TextBox txtCost;
    private System.Windows.Forms.Label lblCost;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnStepCost;
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.TextBox txtVelo;
    private System.Windows.Forms.TextBox txtHeight;
    private System.Windows.Forms.Button btnVelo;
    private System.Windows.Forms.Button btnHeight;
    private System.Windows.Forms.Label lblVelo;
    private System.Windows.Forms.Label lblHeight;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.RadioButton optHeightVelo;
    private System.Windows.Forms.RadioButton optMultiLayer;
    private System.Windows.Forms.Panel pnlTerrainVelo;
    private System.Windows.Forms.DataGridView grdLevels;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Panel pnlMulti;
    private System.Windows.Forms.Button btnAddLevel;
  }
}
