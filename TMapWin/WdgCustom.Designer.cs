namespace TMapWin
{
  partial class WdgCustom
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
      this.dgCustom = new System.Windows.Forms.DataGridView();
      this.txtAssembly = new System.Windows.Forms.TextBox();
      this.btnOpen = new System.Windows.Forms.Button();
      this.dlgOpenAssembly = new System.Windows.Forms.OpenFileDialog();
      this.btnOk = new System.Windows.Forms.Button();
      this.lblAssembly = new System.Windows.Forms.Label();
      this.lblPlugins = new System.Windows.Forms.Label();
      this.chkAdd = new System.Windows.Forms.CheckBox();
      this.chkRun = new System.Windows.Forms.CheckBox();
      this.btnCancel = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.dgCustom)).BeginInit();
      this.SuspendLayout();
      // 
      // dgCustom
      // 
      this.dgCustom.AllowUserToAddRows = false;
      this.dgCustom.AllowUserToDeleteRows = false;
      this.dgCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dgCustom.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgCustom.Location = new System.Drawing.Point(13, 74);
      this.dgCustom.Name = "dgCustom";
      this.dgCustom.ReadOnly = true;
      this.dgCustom.RowHeadersWidth = 18;
      this.dgCustom.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgCustom.Size = new System.Drawing.Size(281, 154);
      this.dgCustom.TabIndex = 0;
      this.dgCustom.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DgCustom_MouseDown);
      this.dgCustom.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DgCustom_MouseMove);
      this.dgCustom.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DgCustom_MouseUp);
      this.dgCustom.SelectionChanged += new System.EventHandler(this.DgCustom_SelectionChanged);
      // 
      // txtAssembly
      // 
      this.txtAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtAssembly.Location = new System.Drawing.Point(13, 23);
      this.txtAssembly.Name = "txtAssembly";
      this.txtAssembly.Size = new System.Drawing.Size(250, 20);
      this.txtAssembly.TabIndex = 1;
      // 
      // btnOpen
      // 
      this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOpen.Location = new System.Drawing.Point(269, 23);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(24, 23);
      this.btnOpen.TabIndex = 2;
      this.btnOpen.Text = "...";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.Location = new System.Drawing.Point(182, 292);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(54, 23);
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
      // 
      // lblAssembly
      // 
      this.lblAssembly.AutoSize = true;
      this.lblAssembly.Location = new System.Drawing.Point(12, 7);
      this.lblAssembly.Name = "lblAssembly";
      this.lblAssembly.Size = new System.Drawing.Size(119, 13);
      this.lblAssembly.TabIndex = 4;
      this.lblAssembly.Text = "Executable or Assembly";
      // 
      // lblPlugins
      // 
      this.lblPlugins.AutoSize = true;
      this.lblPlugins.Location = new System.Drawing.Point(12, 58);
      this.lblPlugins.Name = "lblPlugins";
      this.lblPlugins.Size = new System.Drawing.Size(87, 13);
      this.lblPlugins.TabIndex = 5;
      this.lblPlugins.Text = "Available Plugins";
      // 
      // chkAdd
      // 
      this.chkAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.chkAdd.AutoSize = true;
      this.chkAdd.Enabled = false;
      this.chkAdd.Location = new System.Drawing.Point(12, 266);
      this.chkAdd.Name = "chkAdd";
      this.chkAdd.Size = new System.Drawing.Size(134, 17);
      this.chkAdd.TabIndex = 6;
      this.chkAdd.Text = "Add Selection to Menu";
      this.chkAdd.UseVisualStyleBackColor = true;
      // 
      // chkRun
      // 
      this.chkRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.chkRun.AutoSize = true;
      this.chkRun.Checked = true;
      this.chkRun.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkRun.Enabled = false;
      this.chkRun.Location = new System.Drawing.Point(12, 243);
      this.chkRun.Name = "chkRun";
      this.chkRun.Size = new System.Drawing.Size(140, 17);
      this.chkRun.TabIndex = 7;
      this.chkRun.Text = "Execute selected Plugin";
      this.chkRun.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(242, 292);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(52, 23);
      this.btnCancel.TabIndex = 8;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // WdgCustom
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(306, 327);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.chkRun);
      this.Controls.Add(this.chkAdd);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnOpen);
      this.Controls.Add(this.txtAssembly);
      this.Controls.Add(this.dgCustom);
      this.Controls.Add(this.lblPlugins);
      this.Controls.Add(this.lblAssembly);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "WdgCustom";
      this.ShowInTaskbar = false;
      this.Text = "Plugin";
      ((System.ComponentModel.ISupportInitialize)(this.dgCustom)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGridView dgCustom;
    private System.Windows.Forms.TextBox txtAssembly;
    private System.Windows.Forms.Button btnOpen;
    private System.Windows.Forms.OpenFileDialog dlgOpenAssembly;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Label lblAssembly;
    private System.Windows.Forms.Label lblPlugins;
    private System.Windows.Forms.CheckBox chkAdd;
    private System.Windows.Forms.CheckBox chkRun;
    private System.Windows.Forms.Button btnCancel;
  }
}