namespace Dhm
{
  partial class WdgMain
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnDlgOpen = new System.Windows.Forms.Button();
      this.txtOcad = new System.Windows.Forms.TextBox();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.btnOK = new System.Windows.Forms.Button();
      this.txtTolerance = new System.Windows.Forms.TextBox();
      this.lblTolerance = new System.Windows.Forms.Label();
      this.chkShow = new System.Windows.Forms.CheckBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnPause = new System.Windows.Forms.Button();
      this.chkAddToMap = new System.Windows.Forms.CheckBox();
      this.btnSettings = new System.Windows.Forms.Button();
      this.grpMap = new System.Windows.Forms.GroupBox();
      this.chkIgnoreProgress = new System.Windows.Forms.CheckBox();
      this.grpMesh = new System.Windows.Forms.GroupBox();
      this.chkTris = new System.Windows.Forms.CheckBox();
      this.chkLines = new System.Windows.Forms.CheckBox();
      this.btnDrawMesh = new System.Windows.Forms.Button();
      this.chkToplevel = new System.Windows.Forms.CheckBox();
      this.chkOCAD = new System.Windows.Forms.CheckBox();
      this.grpMap.SuspendLayout();
      this.grpMesh.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnDlgOpen
      // 
      this.btnDlgOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDlgOpen.Location = new System.Drawing.Point(254, 9);
      this.btnDlgOpen.Name = "btnDlgOpen";
      this.btnDlgOpen.Size = new System.Drawing.Size(26, 23);
      this.btnDlgOpen.TabIndex = 0;
      this.btnDlgOpen.Text = "...";
      this.btnDlgOpen.UseVisualStyleBackColor = true;
      this.btnDlgOpen.Click += new System.EventHandler(this.btnDlgOpen_Click);
      // 
      // txtOcad
      // 
      this.txtOcad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtOcad.Location = new System.Drawing.Point(12, 12);
      this.txtOcad.Name = "txtOcad";
      this.txtOcad.Size = new System.Drawing.Size(236, 20);
      this.txtOcad.TabIndex = 1;
      // 
      // dlgOpen
      // 
      this.dlgOpen.FileName = "openFileDialog1";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(194, 201);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(86, 23);
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "Create DHM";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // txtTolerance
      // 
      this.txtTolerance.Location = new System.Drawing.Point(73, 56);
      this.txtTolerance.Name = "txtTolerance";
      this.txtTolerance.Size = new System.Drawing.Size(34, 20);
      this.txtTolerance.TabIndex = 3;
      this.txtTolerance.Text = "1.0";
      this.txtTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblTolerance
      // 
      this.lblTolerance.AutoSize = true;
      this.lblTolerance.Location = new System.Drawing.Point(12, 59);
      this.lblTolerance.Name = "lblTolerance";
      this.lblTolerance.Size = new System.Drawing.Size(55, 13);
      this.lblTolerance.TabIndex = 4;
      this.lblTolerance.Text = "Tolerance";
      // 
      // chkShow
      // 
      this.chkShow.AutoSize = true;
      this.chkShow.Checked = true;
      this.chkShow.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkShow.Location = new System.Drawing.Point(6, 19);
      this.chkShow.Name = "chkShow";
      this.chkShow.Size = new System.Drawing.Size(114, 17);
      this.chkShow.TabIndex = 5;
      this.chkShow.Text = "Show in TMapWin";
      this.chkShow.UseVisualStyleBackColor = true;
      this.chkShow.Visible = false;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(194, 259);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(86, 23);
      this.btnCancel.TabIndex = 6;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnPause
      // 
      this.btnPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPause.Location = new System.Drawing.Point(194, 228);
      this.btnPause.Name = "btnPause";
      this.btnPause.Size = new System.Drawing.Size(86, 23);
      this.btnPause.TabIndex = 7;
      this.btnPause.Text = "Pause";
      this.btnPause.UseVisualStyleBackColor = true;
      this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
      // 
      // chkAddToMap
      // 
      this.chkAddToMap.AutoSize = true;
      this.chkAddToMap.Location = new System.Drawing.Point(28, 42);
      this.chkAddToMap.Name = "chkAddToMap";
      this.chkAddToMap.Size = new System.Drawing.Size(81, 17);
      this.chkAddToMap.TabIndex = 8;
      this.chkAddToMap.Text = "Add to Map";
      this.chkAddToMap.UseVisualStyleBackColor = true;
      this.chkAddToMap.Visible = false;
      // 
      // btnSettings
      // 
      this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSettings.Location = new System.Drawing.Point(194, 117);
      this.btnSettings.Name = "btnSettings";
      this.btnSettings.Size = new System.Drawing.Size(86, 22);
      this.btnSettings.TabIndex = 9;
      this.btnSettings.Text = "Load Settings";
      this.btnSettings.UseVisualStyleBackColor = true;
      this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
      // 
      // grpMap
      // 
      this.grpMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.grpMap.Controls.Add(this.chkIgnoreProgress);
      this.grpMap.Controls.Add(this.grpMesh);
      this.grpMap.Controls.Add(this.chkShow);
      this.grpMap.Controls.Add(this.chkAddToMap);
      this.grpMap.Location = new System.Drawing.Point(12, 82);
      this.grpMap.Name = "grpMap";
      this.grpMap.Size = new System.Drawing.Size(153, 203);
      this.grpMap.TabIndex = 10;
      this.grpMap.TabStop = false;
      this.grpMap.Text = "Map";
      // 
      // chkIgnoreProgress
      // 
      this.chkIgnoreProgress.AutoSize = true;
      this.chkIgnoreProgress.Location = new System.Drawing.Point(28, 65);
      this.chkIgnoreProgress.Name = "chkIgnoreProgress";
      this.chkIgnoreProgress.Size = new System.Drawing.Size(97, 17);
      this.chkIgnoreProgress.TabIndex = 15;
      this.chkIgnoreProgress.Text = "Ignore Progess";
      this.chkIgnoreProgress.UseVisualStyleBackColor = true;
      // 
      // grpMesh
      // 
      this.grpMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpMesh.Controls.Add(this.chkTris);
      this.grpMesh.Controls.Add(this.chkLines);
      this.grpMesh.Controls.Add(this.btnDrawMesh);
      this.grpMesh.Location = new System.Drawing.Point(6, 95);
      this.grpMesh.Name = "grpMesh";
      this.grpMesh.Size = new System.Drawing.Size(141, 102);
      this.grpMesh.TabIndex = 11;
      this.grpMesh.TabStop = false;
      this.grpMesh.Text = "Draw Mesh";
      // 
      // chkTris
      // 
      this.chkTris.AutoSize = true;
      this.chkTris.Location = new System.Drawing.Point(22, 71);
      this.chkTris.Name = "chkTris";
      this.chkTris.Size = new System.Drawing.Size(43, 17);
      this.chkTris.TabIndex = 13;
      this.chkTris.Text = "Tris";
      this.chkTris.UseVisualStyleBackColor = true;
      // 
      // chkLines
      // 
      this.chkLines.AutoSize = true;
      this.chkLines.Checked = true;
      this.chkLines.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkLines.Location = new System.Drawing.Point(22, 48);
      this.chkLines.Name = "chkLines";
      this.chkLines.Size = new System.Drawing.Size(51, 17);
      this.chkLines.TabIndex = 12;
      this.chkLines.Text = "Lines";
      this.chkLines.UseVisualStyleBackColor = true;
      // 
      // btnDrawMesh
      // 
      this.btnDrawMesh.Location = new System.Drawing.Point(10, 19);
      this.btnDrawMesh.Name = "btnDrawMesh";
      this.btnDrawMesh.Size = new System.Drawing.Size(75, 23);
      this.btnDrawMesh.TabIndex = 11;
      this.btnDrawMesh.Text = "Draw Mesh";
      this.btnDrawMesh.UseVisualStyleBackColor = true;
      this.btnDrawMesh.Click += new System.EventHandler(this.btnDrawMesh_Click);
      // 
      // chkToplevel
      // 
      this.chkToplevel.AutoSize = true;
      this.chkToplevel.Checked = true;
      this.chkToplevel.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkToplevel.Location = new System.Drawing.Point(12, 300);
      this.chkToplevel.Name = "chkToplevel";
      this.chkToplevel.Size = new System.Drawing.Size(94, 17);
      this.chkToplevel.TabIndex = 14;
      this.chkToplevel.Text = "Allways on top";
      this.chkToplevel.UseVisualStyleBackColor = true;
      this.chkToplevel.CheckedChanged += new System.EventHandler(this.chkToplevel_CheckedChanged);
      // 
      // chkOCAD
      // 
      this.chkOCAD.AutoSize = true;
      this.chkOCAD.Location = new System.Drawing.Point(198, 288);
      this.chkOCAD.Name = "chkOCAD";
      this.chkOCAD.Size = new System.Drawing.Size(82, 30);
      this.chkOCAD.TabIndex = 15;
      this.chkOCAD.Text = "Show errors\r\nin OCAD";
      this.chkOCAD.UseVisualStyleBackColor = true;
      // 
      // WdgMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 329);
      this.Controls.Add(this.chkOCAD);
      this.Controls.Add(this.chkToplevel);
      this.Controls.Add(this.grpMap);
      this.Controls.Add(this.btnSettings);
      this.Controls.Add(this.btnPause);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.lblTolerance);
      this.Controls.Add(this.txtTolerance);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.txtOcad);
      this.Controls.Add(this.btnDlgOpen);
      this.Name = "WdgMain";
      this.Text = "Dhm";
      this.Load += new System.EventHandler(this.WdgMain_Load);
      this.Closing += new System.ComponentModel.CancelEventHandler(this.WdgMain_Closing);
      this.Closed += new System.EventHandler(this.WdgMain_Closed);
      this.grpMap.ResumeLayout(false);
      this.grpMap.PerformLayout();
      this.grpMesh.ResumeLayout(false);
      this.grpMesh.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnDlgOpen;
    private System.Windows.Forms.TextBox txtOcad;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TextBox txtTolerance;
    private System.Windows.Forms.Label lblTolerance;
    private System.Windows.Forms.CheckBox chkShow;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnPause;
    private System.Windows.Forms.CheckBox chkAddToMap;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.GroupBox grpMap;
    private System.Windows.Forms.Button btnDrawMesh;
    private System.Windows.Forms.GroupBox grpMesh;
    private System.Windows.Forms.CheckBox chkTris;
    private System.Windows.Forms.CheckBox chkLines;
    private System.Windows.Forms.CheckBox chkToplevel;
    private System.Windows.Forms.CheckBox chkIgnoreProgress;
    private System.Windows.Forms.CheckBox chkOCAD;
  }
}

