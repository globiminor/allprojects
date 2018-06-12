namespace LeastCostPathUI
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
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.optHeightVelo = new System.Windows.Forms.RadioButton();
      this.optMultiLayer = new System.Windows.Forms.RadioButton();
      this.pnlTerrainVelo = new System.Windows.Forms.Panel();
      this.grdLevels = new System.Windows.Forms.DataGridView();
      this.pnlMulti = new System.Windows.Forms.Panel();
      this.cntConfigSimple = new LeastCostPathUI.CntConfig();
      this.btnEdit = new System.Windows.Forms.Button();
      this.btnAddLevel = new System.Windows.Forms.Button();
      this.pnlTerrainVelo.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdLevels)).BeginInit();
      this.pnlMulti.SuspendLayout();
      this.SuspendLayout();
      // 
      // optHeightVelo
      // 
      this.optHeightVelo.AutoSize = true;
      this.optHeightVelo.Location = new System.Drawing.Point(2, 3);
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
      this.optMultiLayer.Location = new System.Drawing.Point(148, 3);
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
      this.pnlTerrainVelo.Controls.Add(this.cntConfigSimple);
      this.pnlTerrainVelo.Location = new System.Drawing.Point(0, 26);
      this.pnlTerrainVelo.Name = "pnlTerrainVelo";
      this.pnlTerrainVelo.Size = new System.Drawing.Size(419, 96);
      this.pnlTerrainVelo.TabIndex = 28;
      // 
      // grdLevels
      // 
      this.grdLevels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grdLevels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grdLevels.Location = new System.Drawing.Point(33, 0);
      this.grdLevels.Name = "grdLevels";
      this.grdLevels.Size = new System.Drawing.Size(386, 96);
      this.grdLevels.TabIndex = 29;
      // 
      // pnlMulti
      // 
      this.pnlMulti.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlMulti.Controls.Add(this.btnEdit);
      this.pnlMulti.Controls.Add(this.btnAddLevel);
      this.pnlMulti.Controls.Add(this.grdLevels);
      this.pnlMulti.Location = new System.Drawing.Point(0, 26);
      this.pnlMulti.Name = "pnlMulti";
      this.pnlMulti.Size = new System.Drawing.Size(419, 96);
      this.pnlMulti.TabIndex = 30;
      // 
      // cntConfigSimple
      // 
      this.cntConfigSimple.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cntConfigSimple.ConfigVm = null;
      this.cntConfigSimple.Location = new System.Drawing.Point(0, 0);
      this.cntConfigSimple.Name = "cntConfigSimple";
      this.cntConfigSimple.Size = new System.Drawing.Size(422, 95);
      this.cntConfigSimple.TabIndex = 0;
      // 
      // btnEdit
      // 
      this.btnEdit.Image = global::LeastCostPathUI.Images.Edit;
      this.btnEdit.Location = new System.Drawing.Point(2, 64);
      this.btnEdit.Name = "btnEdit";
      this.btnEdit.Size = new System.Drawing.Size(25, 25);
      this.btnEdit.TabIndex = 31;
      this.btnEdit.UseVisualStyleBackColor = true;
      // 
      // btnAddLevel
      // 
      this.btnAddLevel.Image = global::LeastCostPathUI.Images.Add;
      this.btnAddLevel.Location = new System.Drawing.Point(2, 41);
      this.btnAddLevel.Name = "btnAddLevel";
      this.btnAddLevel.Size = new System.Drawing.Size(25, 25);
      this.btnAddLevel.TabIndex = 30;
      this.btnAddLevel.UseVisualStyleBackColor = true;
      this.btnAddLevel.Click += new System.EventHandler(this.BtnAddLevel_Click);
      // 
      // CntConfigView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.optMultiLayer);
      this.Controls.Add(this.optHeightVelo);
      this.Controls.Add(this.pnlMulti);
      this.Controls.Add(this.pnlTerrainVelo);
      this.Name = "CntConfigView";
      this.Size = new System.Drawing.Size(422, 122);
      this.pnlTerrainVelo.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grdLevels)).EndInit();
      this.pnlMulti.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.RadioButton optHeightVelo;
    private System.Windows.Forms.RadioButton optMultiLayer;
    private System.Windows.Forms.Panel pnlTerrainVelo;
    private System.Windows.Forms.DataGridView grdLevels;
    private System.Windows.Forms.Panel pnlMulti;
    private System.Windows.Forms.Button btnAddLevel;
    private CntConfig cntConfigSimple;
    private System.Windows.Forms.Button btnEdit;
  }
}
