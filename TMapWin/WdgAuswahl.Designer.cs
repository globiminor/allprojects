namespace TMapWin
{
  partial class WdgAuswahl
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
      this.lstAuswahl = new System.Windows.Forms.ComboBox();
      this.lblAuswahl = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lstAuswahl
      // 
      this.lstAuswahl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.lstAuswahl.FormattingEnabled = true;
      this.lstAuswahl.Location = new System.Drawing.Point(22, 43);
      this.lstAuswahl.Name = "lstAuswahl";
      this.lstAuswahl.Size = new System.Drawing.Size(228, 21);
      this.lstAuswahl.TabIndex = 0;
      // 
      // lblAuswahl
      // 
      this.lblAuswahl.AutoSize = true;
      this.lblAuswahl.Location = new System.Drawing.Point(19, 18);
      this.lblAuswahl.Name = "lblAuswahl";
      this.lblAuswahl.Size = new System.Drawing.Size(35, 13);
      this.lblAuswahl.TabIndex = 1;
      this.lblAuswahl.Text = "label1";
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(127, 92);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(51, 23);
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(199, 92);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(51, 23);
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // WdgAuswahl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(261, 135);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.lblAuswahl);
      this.Controls.Add(this.lstAuswahl);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "WdgAuswahl";
      this.Text = "Auswahl";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox lstAuswahl;
    private System.Windows.Forms.Label lblAuswahl;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
  }
}