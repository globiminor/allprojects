namespace TMapWin
{
  partial class WdgSymbol
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
      this.pgrSymbol = new System.Windows.Forms.PropertyGrid();
      this.pnlSymbol = new System.Windows.Forms.Panel();
      this.grdSymbolPart = new System.Windows.Forms.DataGridView();
      this.btnApply = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.grdSymbolPart)).BeginInit();
      this.SuspendLayout();
      // 
      // pgrSymbol
      // 
      this.pgrSymbol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pgrSymbol.Location = new System.Drawing.Point(0, 127);
      this.pgrSymbol.Name = "pgrSymbol";
      this.pgrSymbol.Size = new System.Drawing.Size(285, 179);
      this.pgrSymbol.TabIndex = 0;
      this.pgrSymbol.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgrSymbol_PropertyValueChanged);
      // 
      // pnlSymbol
      // 
      this.pnlSymbol.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pnlSymbol.Location = new System.Drawing.Point(0, 1);
      this.pnlSymbol.Name = "pnlSymbol";
      this.pnlSymbol.Size = new System.Drawing.Size(161, 32);
      this.pnlSymbol.TabIndex = 3;
      this.pnlSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSymbol_Paint);
      // 
      // grdSymbolPart
      // 
      this.grdSymbolPart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grdSymbolPart.Location = new System.Drawing.Point(0, 41);
      this.grdSymbolPart.Name = "grdSymbolPart";
      this.grdSymbolPart.Size = new System.Drawing.Size(285, 80);
      this.grdSymbolPart.SelectionChanged += new System.EventHandler(grdSymbolPart_SelectionChanged);
      this.grdSymbolPart.TabIndex = 2;
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnApply.Location = new System.Drawing.Point(197, 312);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(75, 23);
      this.btnApply.TabIndex = 25;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // WdgSymbol_
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 343);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.pnlSymbol);
      this.Controls.Add(this.grdSymbolPart);
      this.Controls.Add(this.pgrSymbol);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgSymbol_";
      this.Text = "Symbol";
      ((System.ComponentModel.ISupportInitialize)(this.grdSymbolPart)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PropertyGrid pgrSymbol;
    private System.Windows.Forms.Panel pnlSymbol;
    private System.Windows.Forms.DataGridView grdSymbolPart;
    private System.Windows.Forms.Button btnApply;
  }
}