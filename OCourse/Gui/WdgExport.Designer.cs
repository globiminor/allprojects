namespace OCourse.Gui
{
  partial class WdgExport
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
      this.txtExport = new System.Windows.Forms.TextBox();
      this.lblExport = new System.Windows.Forms.Label();
      this.btnExport = new System.Windows.Forms.Button();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.btnTemplate = new System.Windows.Forms.Button();
      this.lblTemplate = new System.Windows.Forms.Label();
      this.txtTemplate = new System.Windows.Forms.TextBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // txtExport
      // 
      this.txtExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtExport.Location = new System.Drawing.Point(89, 12);
      this.txtExport.Name = "txtExport";
      this.txtExport.Size = new System.Drawing.Size(478, 20);
      this.txtExport.TabIndex = 8;
      // 
      // lblExport
      // 
      this.lblExport.AutoSize = true;
      this.lblExport.Location = new System.Drawing.Point(12, 15);
      this.lblExport.Name = "lblExport";
      this.lblExport.Size = new System.Drawing.Size(56, 13);
      this.lblExport.TabIndex = 9;
      this.lblExport.Text = "Export File";
      // 
      // btnExport
      // 
      this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnExport.Location = new System.Drawing.Point(573, 10);
      this.btnExport.Name = "btnExport";
      this.btnExport.Size = new System.Drawing.Size(26, 23);
      this.btnExport.TabIndex = 10;
      this.btnExport.Text = "...";
      this.btnExport.UseVisualStyleBackColor = true;
      this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
      // 
      // btnTemplate
      // 
      this.btnTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTemplate.Location = new System.Drawing.Point(573, 58);
      this.btnTemplate.Name = "btnTemplate";
      this.btnTemplate.Size = new System.Drawing.Size(26, 23);
      this.btnTemplate.TabIndex = 13;
      this.btnTemplate.Text = "...";
      this.btnTemplate.UseVisualStyleBackColor = true;
      this.btnTemplate.Click += new System.EventHandler(this.BtnTemplate_Click);
      // 
      // lblTemplate
      // 
      this.lblTemplate.AutoSize = true;
      this.lblTemplate.Location = new System.Drawing.Point(12, 63);
      this.lblTemplate.Name = "lblTemplate";
      this.lblTemplate.Size = new System.Drawing.Size(70, 13);
      this.lblTemplate.TabIndex = 12;
      this.lblTemplate.Text = "Template File";
      // 
      // txtTemplate
      // 
      this.txtTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTemplate.Location = new System.Drawing.Point(89, 60);
      this.txtTemplate.Name = "txtTemplate";
      this.txtTemplate.Size = new System.Drawing.Size(478, 20);
      this.txtTemplate.TabIndex = 11;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(452, 121);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(44, 23);
      this.btnOK.TabIndex = 14;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(502, 121);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(65, 23);
      this.btnCancel.TabIndex = 15;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // WdgExport
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(627, 179);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnTemplate);
      this.Controls.Add(this.lblTemplate);
      this.Controls.Add(this.txtTemplate);
      this.Controls.Add(this.btnExport);
      this.Controls.Add(this.lblExport);
      this.Controls.Add(this.txtExport);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgExport";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Export to OCAD";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtExport;
    private System.Windows.Forms.Label lblExport;
    private System.Windows.Forms.Button btnExport;
    private System.Windows.Forms.SaveFileDialog dlgSave;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.Button btnTemplate;
    private System.Windows.Forms.Label lblTemplate;
    private System.Windows.Forms.TextBox txtTemplate;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
  }
}