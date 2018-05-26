namespace LeastCostPathUI
{
  partial class WdgAnalyzeImg
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
      this.txtFile = new System.Windows.Forms.TextBox();
      this.btnAnalyze = new System.Windows.Forms.Button();
      this.btnOpen = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // txtFile
      // 
      this.txtFile.Location = new System.Drawing.Point(12, 12);
      this.txtFile.Name = "txtFile";
      this.txtFile.Size = new System.Drawing.Size(227, 20);
      this.txtFile.TabIndex = 0;
      // 
      // btnAnalyze
      // 
      this.btnAnalyze.Location = new System.Drawing.Point(12, 92);
      this.btnAnalyze.Name = "btnAnalyze";
      this.btnAnalyze.Size = new System.Drawing.Size(75, 23);
      this.btnAnalyze.TabIndex = 1;
      this.btnAnalyze.Text = "Analyze";
      this.btnAnalyze.UseVisualStyleBackColor = true;
      this.btnAnalyze.Click += new System.EventHandler(this.BtnAnalyze_Click);
      // 
      // btnOpen
      // 
      this.btnOpen.Location = new System.Drawing.Point(245, 12);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(26, 23);
      this.btnOpen.TabIndex = 2;
      this.btnOpen.Text = "...";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
      // 
      // WdgAnalyzeImg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.btnOpen);
      this.Controls.Add(this.btnAnalyze);
      this.Controls.Add(this.txtFile);
      this.Name = "WdgAnalyzeImg";
      this.Text = "WdgAnalyeImg";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtFile;
    private System.Windows.Forms.Button btnAnalyze;
    private System.Windows.Forms.Button btnOpen;
  }
}