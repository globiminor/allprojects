namespace OCourse
{
  partial class WdgRelay
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
      this.btnCourseXml = new System.Windows.Forms.Button();
      this.txtCourseOcd = new System.Windows.Forms.TextBox();
      this.lblOcad = new System.Windows.Forms.Label();
      this.txtCourseXml = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.btnVerify = new System.Windows.Forms.Button();
      this.btnEstimate = new System.Windows.Forms.Button();
      this.grpExport = new System.Windows.Forms.GroupBox();
      this.btnExportTxtV8 = new System.Windows.Forms.Button();
      this.btnCreateCsv = new System.Windows.Forms.Button();
      this.btnCombinations = new System.Windows.Forms.Button();
      this.btnAdaptMaps = new System.Windows.Forms.Button();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.txtGrafics = new System.Windows.Forms.TextBox();
      this.btnGrafics = new System.Windows.Forms.Button();
      this.lblGrafics = new System.Windows.Forms.Label();
      this.txtMapFolder = new System.Windows.Forms.TextBox();
      this.bntMapFolder = new System.Windows.Forms.Button();
      this.lblMapFolder = new System.Windows.Forms.Label();
      this.chkDummyPrefix = new System.Windows.Forms.CheckBox();
      this.lblDummyPrefix = new System.Windows.Forms.Label();
      this.txtDummyPrefix = new System.Windows.Forms.TextBox();
      this.grpExport.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCourseXml
      // 
      this.btnCourseXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCourseXml.Location = new System.Drawing.Point(498, 30);
      this.btnCourseXml.Name = "btnCourseXml";
      this.btnCourseXml.Size = new System.Drawing.Size(26, 23);
      this.btnCourseXml.TabIndex = 11;
      this.btnCourseXml.Text = "...";
      this.btnCourseXml.UseVisualStyleBackColor = true;
      this.btnCourseXml.Click += new System.EventHandler(this.BtnCourseXml_Click);
      // 
      // txtCourseOcd
      // 
      this.txtCourseOcd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCourseOcd.Location = new System.Drawing.Point(111, 6);
      this.txtCourseOcd.Name = "txtCourseOcd";
      this.txtCourseOcd.ReadOnly = true;
      this.txtCourseOcd.Size = new System.Drawing.Size(380, 20);
      this.txtCourseOcd.TabIndex = 10;
      // 
      // lblOcad
      // 
      this.lblOcad.AutoSize = true;
      this.lblOcad.Location = new System.Drawing.Point(12, 9);
      this.lblOcad.Name = "lblOcad";
      this.lblOcad.Size = new System.Drawing.Size(70, 13);
      this.lblOcad.TabIndex = 9;
      this.lblOcad.Text = "Course (.ocd)";
      // 
      // txtCourseXml
      // 
      this.txtCourseXml.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCourseXml.Location = new System.Drawing.Point(111, 32);
      this.txtCourseXml.Name = "txtCourseXml";
      this.txtCourseXml.Size = new System.Drawing.Size(381, 20);
      this.txtCourseXml.TabIndex = 12;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 35);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(67, 13);
      this.label1.TabIndex = 13;
      this.label1.Text = "Course (.xml)";
      // 
      // btnVerify
      // 
      this.btnVerify.Location = new System.Drawing.Point(111, 87);
      this.btnVerify.Name = "btnVerify";
      this.btnVerify.Size = new System.Drawing.Size(110, 23);
      this.btnVerify.TabIndex = 14;
      this.btnVerify.Text = "Verify From/To";
      this.btnVerify.UseVisualStyleBackColor = true;
      this.btnVerify.Click += new System.EventHandler(this.BtnVerify_Click);
      // 
      // btnEstimate
      // 
      this.btnEstimate.Location = new System.Drawing.Point(111, 58);
      this.btnEstimate.Name = "btnEstimate";
      this.btnEstimate.Size = new System.Drawing.Size(110, 23);
      this.btnEstimate.TabIndex = 15;
      this.btnEstimate.Text = "Estimate From/To";
      this.btnEstimate.UseVisualStyleBackColor = true;
      this.btnEstimate.Click += new System.EventHandler(this.BtnEstimate_Click);
      // 
      // grpExport
      // 
      this.grpExport.Controls.Add(this.txtDummyPrefix);
      this.grpExport.Controls.Add(this.lblDummyPrefix);
      this.grpExport.Controls.Add(this.chkDummyPrefix);
      this.grpExport.Controls.Add(this.btnExportTxtV8);
      this.grpExport.Controls.Add(this.btnCreateCsv);
      this.grpExport.Controls.Add(this.btnCombinations);
      this.grpExport.Location = new System.Drawing.Point(244, 58);
      this.grpExport.Name = "grpExport";
      this.grpExport.Size = new System.Drawing.Size(279, 118);
      this.grpExport.TabIndex = 16;
      this.grpExport.TabStop = false;
      this.grpExport.Text = "Export";
      // 
      // btnExportTxtV8
      // 
      this.btnExportTxtV8.Location = new System.Drawing.Point(22, 87);
      this.btnExportTxtV8.Name = "btnExportTxtV8";
      this.btnExportTxtV8.Size = new System.Drawing.Size(154, 23);
      this.btnExportTxtV8.TabIndex = 19;
      this.btnExportTxtV8.Text = "Create Txt V8 File";
      this.btnExportTxtV8.UseVisualStyleBackColor = true;
      this.btnExportTxtV8.Click += new System.EventHandler(this.BtnExportTxtV8_Click);
      // 
      // btnCreateCsv
      // 
      this.btnCreateCsv.Location = new System.Drawing.Point(22, 58);
      this.btnCreateCsv.Name = "btnCreateCsv";
      this.btnCreateCsv.Size = new System.Drawing.Size(154, 23);
      this.btnCreateCsv.TabIndex = 18;
      this.btnCreateCsv.Text = "Create Csv File";
      this.btnCreateCsv.UseVisualStyleBackColor = true;
      this.btnCreateCsv.Click += new System.EventHandler(this.BtnCreateCsv_Click);
      // 
      // btnCombinations
      // 
      this.btnCombinations.Location = new System.Drawing.Point(22, 29);
      this.btnCombinations.Name = "btnCombinations";
      this.btnCombinations.Size = new System.Drawing.Size(154, 23);
      this.btnCombinations.TabIndex = 17;
      this.btnCombinations.Text = "Combinations ( txt)";
      this.btnCombinations.UseVisualStyleBackColor = true;
      this.btnCombinations.Click += new System.EventHandler(this.BtnCombinations_Click);
      // 
      // btnAdaptMaps
      // 
      this.btnAdaptMaps.Location = new System.Drawing.Point(111, 180);
      this.btnAdaptMaps.Name = "btnAdaptMaps";
      this.btnAdaptMaps.Size = new System.Drawing.Size(110, 23);
      this.btnAdaptMaps.TabIndex = 20;
      this.btnAdaptMaps.Text = "Adapt Map Files";
      this.btnAdaptMaps.UseVisualStyleBackColor = true;
      this.btnAdaptMaps.Click += new System.EventHandler(this.BtnAdaptMaps_Click);
      // 
      // dlgFolder
      // 
      this.dlgFolder.Description = "Directory with Course Maps";
      // 
      // txtGrafics
      // 
      this.txtGrafics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtGrafics.Location = new System.Drawing.Point(111, 240);
      this.txtGrafics.Name = "txtGrafics";
      this.txtGrafics.Size = new System.Drawing.Size(380, 20);
      this.txtGrafics.TabIndex = 21;
      // 
      // btnGrafics
      // 
      this.btnGrafics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGrafics.Location = new System.Drawing.Point(497, 238);
      this.btnGrafics.Name = "btnGrafics";
      this.btnGrafics.Size = new System.Drawing.Size(26, 23);
      this.btnGrafics.TabIndex = 22;
      this.btnGrafics.Text = "...";
      this.btnGrafics.UseVisualStyleBackColor = true;
      this.btnGrafics.Click += new System.EventHandler(this.BtnGrafics_Click);
      // 
      // lblGrafics
      // 
      this.lblGrafics.AutoSize = true;
      this.lblGrafics.Location = new System.Drawing.Point(12, 243);
      this.lblGrafics.Name = "lblGrafics";
      this.lblGrafics.Size = new System.Drawing.Size(93, 13);
      this.lblGrafics.TabIndex = 23;
      this.lblGrafics.Text = "Grafics File (*.ocd)";
      // 
      // txtMapFolder
      // 
      this.txtMapFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtMapFolder.Location = new System.Drawing.Point(111, 209);
      this.txtMapFolder.Name = "txtMapFolder";
      this.txtMapFolder.Size = new System.Drawing.Size(380, 20);
      this.txtMapFolder.TabIndex = 24;
      // 
      // bntMapFolder
      // 
      this.bntMapFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bntMapFolder.Location = new System.Drawing.Point(497, 207);
      this.bntMapFolder.Name = "bntMapFolder";
      this.bntMapFolder.Size = new System.Drawing.Size(26, 23);
      this.bntMapFolder.TabIndex = 25;
      this.bntMapFolder.Text = "...";
      this.bntMapFolder.UseVisualStyleBackColor = true;
      this.bntMapFolder.Click += new System.EventHandler(this.BntMapFolder_Click);
      // 
      // lblMapFolder
      // 
      this.lblMapFolder.AutoSize = true;
      this.lblMapFolder.Location = new System.Drawing.Point(12, 212);
      this.lblMapFolder.Name = "lblMapFolder";
      this.lblMapFolder.Size = new System.Drawing.Size(60, 13);
      this.lblMapFolder.TabIndex = 26;
      this.lblMapFolder.Text = "Map Folder";
      // 
      // chkDummyPrefix
      // 
      this.chkDummyPrefix.AutoSize = true;
      this.chkDummyPrefix.Location = new System.Drawing.Point(186, 38);
      this.chkDummyPrefix.Name = "chkDummyPrefix";
      this.chkDummyPrefix.Size = new System.Drawing.Size(87, 30);
      this.chkDummyPrefix.TabIndex = 20;
      this.chkDummyPrefix.Text = "insert dummy\r\ncontrols";
      this.chkDummyPrefix.UseVisualStyleBackColor = true;
      // 
      // lblDummyPrefix
      // 
      this.lblDummyPrefix.AutoSize = true;
      this.lblDummyPrefix.Location = new System.Drawing.Point(186, 75);
      this.lblDummyPrefix.Name = "lblDummyPrefix";
      this.lblDummyPrefix.Size = new System.Drawing.Size(71, 13);
      this.lblDummyPrefix.TabIndex = 21;
      this.lblDummyPrefix.Text = "Dummy Prefix";
      // 
      // txtDummyPrefix
      // 
      this.txtDummyPrefix.Location = new System.Drawing.Point(186, 92);
      this.txtDummyPrefix.Name = "txtDummyPrefix";
      this.txtDummyPrefix.Size = new System.Drawing.Size(71, 20);
      this.txtDummyPrefix.TabIndex = 22;
      this.txtDummyPrefix.Text = "15";
      // 
      // WdgRelay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(584, 287);
      this.Controls.Add(this.lblMapFolder);
      this.Controls.Add(this.bntMapFolder);
      this.Controls.Add(this.txtMapFolder);
      this.Controls.Add(this.lblGrafics);
      this.Controls.Add(this.btnGrafics);
      this.Controls.Add(this.txtGrafics);
      this.Controls.Add(this.btnAdaptMaps);
      this.Controls.Add(this.grpExport);
      this.Controls.Add(this.btnEstimate);
      this.Controls.Add(this.btnVerify);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtCourseXml);
      this.Controls.Add(this.btnCourseXml);
      this.Controls.Add(this.txtCourseOcd);
      this.Controls.Add(this.lblOcad);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgRelay";
      this.ShowInTaskbar = false;
      this.Text = "Relay";
      this.grpExport.ResumeLayout(false);
      this.grpExport.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnCourseXml;
    private System.Windows.Forms.TextBox txtCourseOcd;
    private System.Windows.Forms.Label lblOcad;
    private System.Windows.Forms.TextBox txtCourseXml;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.Button btnVerify;
    private System.Windows.Forms.Button btnEstimate;
    private System.Windows.Forms.GroupBox grpExport;
    private System.Windows.Forms.Button btnCombinations;
    private System.Windows.Forms.SaveFileDialog dlgSave;
    private System.Windows.Forms.Button btnCreateCsv;
    private System.Windows.Forms.Button btnExportTxtV8;
    private System.Windows.Forms.Button btnAdaptMaps;
    private System.Windows.Forms.FolderBrowserDialog dlgFolder;
    private System.Windows.Forms.TextBox txtGrafics;
    private System.Windows.Forms.Button btnGrafics;
    private System.Windows.Forms.Label lblGrafics;
    private System.Windows.Forms.TextBox txtMapFolder;
    private System.Windows.Forms.Button bntMapFolder;
    private System.Windows.Forms.Label lblMapFolder;
    private System.Windows.Forms.TextBox txtDummyPrefix;
    private System.Windows.Forms.Label lblDummyPrefix;
    private System.Windows.Forms.CheckBox chkDummyPrefix;

  }
}