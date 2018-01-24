namespace MacroUi
{
  partial class MacroWdg
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MacroWdg));
      this.btnRun = new System.Windows.Forms.Button();
      this.txtMacro = new System.Windows.Forms.TextBox();
      this.btnOpen = new System.Windows.Forms.Button();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.chkActive = new System.Windows.Forms.CheckBox();
      this.dgvData = new System.Windows.Forms.DataGridView();
      this.btnSelectFiles = new System.Windows.Forms.Button();
      this.dlgOpenData = new System.Windows.Forms.OpenFileDialog();
      this.optFiles = new System.Windows.Forms.RadioButton();
      this.optOnce = new System.Windows.Forms.RadioButton();
      this.txtProgress = new System.Windows.Forms.ListBox();
      this.btn5Staffel = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
      this.SuspendLayout();
      // 
      // btnRun
      // 
      this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnRun.Enabled = false;
      this.btnRun.Location = new System.Drawing.Point(310, 333);
      this.btnRun.Name = "btnRun";
      this.btnRun.Size = new System.Drawing.Size(75, 23);
      this.btnRun.TabIndex = 0;
      this.btnRun.Text = "Run";
      this.btnRun.UseVisualStyleBackColor = true;
      this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
      // 
      // txtMacro
      // 
      this.txtMacro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtMacro.Location = new System.Drawing.Point(12, 386);
      this.txtMacro.Name = "txtMacro";
      this.txtMacro.Size = new System.Drawing.Size(343, 20);
      this.txtMacro.TabIndex = 2;
      this.txtMacro.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMacro_KeyDown);
      this.txtMacro.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtMacro_KeyUp);
      this.txtMacro.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtMacro_MouseDown);
      this.txtMacro.Validating += new System.ComponentModel.CancelEventHandler(this.txtMacro_Validating);
      // 
      // btnOpen
      // 
      this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOpen.Location = new System.Drawing.Point(361, 386);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(23, 23);
      this.btnOpen.TabIndex = 3;
      this.btnOpen.Text = "...";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
      // 
      // dlgOpen
      // 
      this.dlgOpen.Filter = "*.mcr | *.mcr";
      // 
      // chkActive
      // 
      this.chkActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.chkActive.AutoSize = true;
      this.chkActive.Checked = true;
      this.chkActive.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkActive.Location = new System.Drawing.Point(12, 368);
      this.chkActive.Name = "chkActive";
      this.chkActive.Size = new System.Drawing.Size(151, 17);
      this.chkActive.TabIndex = 5;
      this.chkActive.Text = "Use last active Application";
      this.chkActive.UseVisualStyleBackColor = true;
      // 
      // dgvData
      // 
      this.dgvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvData.Location = new System.Drawing.Point(12, 12);
      this.dgvData.Name = "dgvData";
      this.dgvData.Size = new System.Drawing.Size(291, 344);
      this.dgvData.TabIndex = 6;
      // 
      // btnSelectFiles
      // 
      this.btnSelectFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSelectFiles.Location = new System.Drawing.Point(309, 12);
      this.btnSelectFiles.Name = "btnSelectFiles";
      this.btnSelectFiles.Size = new System.Drawing.Size(24, 23);
      this.btnSelectFiles.TabIndex = 7;
      this.btnSelectFiles.Text = "...";
      this.btnSelectFiles.UseVisualStyleBackColor = true;
      this.btnSelectFiles.Click += new System.EventHandler(this.btnSelectFiles_Click);
      // 
      // dlgOpenData
      // 
      this.dlgOpenData.Filter = "OCAD files|*.ocd| All files |*.*";
      this.dlgOpenData.Multiselect = true;
      // 
      // optFiles
      // 
      this.optFiles.AutoSize = true;
      this.optFiles.Location = new System.Drawing.Point(310, 70);
      this.optFiles.Name = "optFiles";
      this.optFiles.Size = new System.Drawing.Size(82, 17);
      this.optFiles.TabIndex = 8;
      this.optFiles.Text = "Run all Files";
      this.optFiles.UseVisualStyleBackColor = true;
      this.optFiles.CheckedChanged += new System.EventHandler(this.optRun_CheckChanged);
      // 
      // optOnce
      // 
      this.optOnce.AutoSize = true;
      this.optOnce.Checked = true;
      this.optOnce.Location = new System.Drawing.Point(310, 93);
      this.optOnce.Name = "optOnce";
      this.optOnce.Size = new System.Drawing.Size(72, 17);
      this.optOnce.TabIndex = 9;
      this.optOnce.TabStop = true;
      this.optOnce.Text = "Run once";
      this.optOnce.UseVisualStyleBackColor = true;
      this.optOnce.CheckedChanged += new System.EventHandler(this.optRun_CheckChanged);
      // 
      // txtProgress
      // 
      this.txtProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtProgress.Location = new System.Drawing.Point(12, 413);
      this.txtProgress.Name = "txtProgress";
      this.txtProgress.Size = new System.Drawing.Size(372, 108);
      this.txtProgress.TabIndex = 10;
      // 
      // btn5Staffel
      // 
      this.btn5Staffel.Location = new System.Drawing.Point(307, 304);
      this.btn5Staffel.Name = "btn5Staffel";
      this.btn5Staffel.Size = new System.Drawing.Size(75, 23);
      this.btn5Staffel.TabIndex = 11;
      this.btn5Staffel.Text = "5erStaffel";
      this.btn5Staffel.UseVisualStyleBackColor = true;
      this.btn5Staffel.Click += new System.EventHandler(this.btn5Staffel_Click);
      // 
      // MacroWdg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(396, 528);
      this.Controls.Add(this.btn5Staffel);
      this.Controls.Add(this.txtProgress);
      this.Controls.Add(this.optOnce);
      this.Controls.Add(this.optFiles);
      this.Controls.Add(this.btnSelectFiles);
      this.Controls.Add(this.dgvData);
      this.Controls.Add(this.chkActive);
      this.Controls.Add(this.btnOpen);
      this.Controls.Add(this.txtMacro);
      this.Controls.Add(this.btnRun);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MacroWdg";
      this.Text = "Macro";
      this.TopMost = true;
      this.TransparencyKey = System.Drawing.Color.Silver;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
      this.Load += new System.EventHandler(this.WdgMacro_Load);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MacroWdg_KeyDown);
      ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnRun;
    private System.Windows.Forms.TextBox txtMacro;
    private System.Windows.Forms.Button btnOpen;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.CheckBox chkActive;
    private System.Windows.Forms.DataGridView dgvData;
    private System.Windows.Forms.Button btnSelectFiles;
    private System.Windows.Forms.OpenFileDialog dlgOpenData;
    private System.Windows.Forms.RadioButton optFiles;
    private System.Windows.Forms.RadioButton optOnce;
    private System.Windows.Forms.ListBox txtProgress;
    private System.Windows.Forms.Button btn5Staffel;
  }
}

