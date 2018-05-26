namespace Dhm
{
  partial class WdgSetGridHeight
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
      this.lstGrids = new System.Windows.Forms.ComboBox();
      this.txtGrid = new System.Windows.Forms.TextBox();
      this.lblBreite = new System.Windows.Forms.Label();
      this.lblGrid = new System.Windows.Forms.Label();
      this.btnExport = new System.Windows.Forms.Button();
      this.lblPos = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lstGrids
      // 
      this.lstGrids.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.lstGrids.FormattingEnabled = true;
      this.lstGrids.Location = new System.Drawing.Point(106, 44);
      this.lstGrids.Name = "lstGrids";
      this.lstGrids.Size = new System.Drawing.Size(100, 21);
      this.lstGrids.TabIndex = 0;
      // 
      // txtGrid
      // 
      this.txtGrid.Location = new System.Drawing.Point(106, 101);
      this.txtGrid.Name = "txtGrid";
      this.txtGrid.Size = new System.Drawing.Size(100, 20);
      this.txtGrid.TabIndex = 1;
      // 
      // lblBreite
      // 
      this.lblBreite.AutoSize = true;
      this.lblBreite.Location = new System.Drawing.Point(29, 104);
      this.lblBreite.Name = "lblBreite";
      this.lblBreite.Size = new System.Drawing.Size(34, 13);
      this.lblBreite.TabIndex = 2;
      this.lblBreite.Text = "Breite";
      // 
      // lblGrid
      // 
      this.lblGrid.AutoSize = true;
      this.lblGrid.Location = new System.Drawing.Point(29, 47);
      this.lblGrid.Name = "lblGrid";
      this.lblGrid.Size = new System.Drawing.Size(26, 13);
      this.lblGrid.TabIndex = 3;
      this.lblGrid.Text = "Grid";
      // 
      // btnExport
      // 
      this.btnExport.Location = new System.Drawing.Point(106, 142);
      this.btnExport.Name = "btnExport";
      this.btnExport.Size = new System.Drawing.Size(100, 23);
      this.btnExport.TabIndex = 4;
      this.btnExport.Text = "Save as...";
      this.btnExport.UseVisualStyleBackColor = true;
      this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
      // 
      // lblPos
      // 
      this.lblPos.AutoSize = true;
      this.lblPos.Location = new System.Drawing.Point(32, 216);
      this.lblPos.Name = "lblPos";
      this.lblPos.Size = new System.Drawing.Size(0, 13);
      this.lblPos.TabIndex = 5;
      // 
      // WdgSetGridHeight
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.lblPos);
      this.Controls.Add(this.btnExport);
      this.Controls.Add(this.lblGrid);
      this.Controls.Add(this.lblBreite);
      this.Controls.Add(this.txtGrid);
      this.Controls.Add(this.lstGrids);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "WdgSetGridHeight";
      this.ShowInTaskbar = false;
      this.Text = "Set Grid Height";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox lstGrids;
    private System.Windows.Forms.TextBox txtGrid;
    private System.Windows.Forms.Label lblBreite;
    private System.Windows.Forms.Label lblGrid;
    private System.Windows.Forms.Button btnExport;
    private System.Windows.Forms.Label lblPos;
  }
}