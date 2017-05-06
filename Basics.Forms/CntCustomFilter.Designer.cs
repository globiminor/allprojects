namespace Basics.Forms
{
  partial class CntCustomFilter
  {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.lblFilter = new System.Windows.Forms.Label();
      this.btnApply = new System.Windows.Forms.Button();
      this._treValues = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // lblFilter
      // 
      this.lblFilter.AutoSize = true;
      this.lblFilter.Location = new System.Drawing.Point(4, 4);
      this.lblFilter.Name = "lblFilter";
      this.lblFilter.Size = new System.Drawing.Size(29, 13);
      this.lblFilter.TabIndex = 0;
      this.lblFilter.Text = "Filter";
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnApply.Location = new System.Drawing.Point(99, 124);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(45, 23);
      this.btnApply.TabIndex = 1;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // _treValues
      // 
      this._treValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
      | System.Windows.Forms.AnchorStyles.Left)
      | System.Windows.Forms.AnchorStyles.Right)));
      this._treValues.CheckBoxes = true;
      this._treValues.Location = new System.Drawing.Point(4, 21);
      this._treValues.Name = "_treValues";
      this._treValues.Size = new System.Drawing.Size(140, 97);
      this._treValues.TabIndex = 2;
      // 
      // CntCustomFilter
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this._treValues);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.lblFilter);
      this.Name = "CntCustomFilter";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblFilter;
    private System.Windows.Forms.Button btnApply;
    private System.Windows.Forms.TreeView _treValues;
  }
}
