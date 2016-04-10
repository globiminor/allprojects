namespace LeastCostPathUI
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
      this.components = new System.ComponentModel.Container();
      this.dgCustom = new System.Windows.Forms.DataGridView();
      this.txtAssembly = new System.Windows.Forms.TextBox();
      this.btnOpen = new System.Windows.Forms.Button();
      this.dlgOpenAssembly = new System.Windows.Forms.OpenFileDialog();
      this.btnOk = new System.Windows.Forms.Button();
      this.lblAssembly = new System.Windows.Forms.Label();
      this.lblPlugins = new System.Windows.Forms.Label();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpAssembly = new System.Windows.Forms.GroupBox();
      this.optExisting = new System.Windows.Forms.RadioButton();
      this.optCustom = new System.Windows.Forms.RadioButton();
      this.grpCustom = new System.Windows.Forms.GroupBox();
      this.txtCode = new System.Windows.Forms.TextBox();
      this.lblDeclare = new System.Windows.Forms.Label();
      this.lblEnd = new System.Windows.Forms.Label();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.dgCustom)).BeginInit();
      this.grpAssembly.SuspendLayout();
      this.grpCustom.SuspendLayout();
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
      this.dgCustom.Location = new System.Drawing.Point(10, 72);
      this.dgCustom.MultiSelect = false;
      this.dgCustom.Name = "dgCustom";
      this.dgCustom.ReadOnly = true;
      this.dgCustom.RowHeadersWidth = 18;
      this.dgCustom.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgCustom.Size = new System.Drawing.Size(370, 91);
      this.dgCustom.TabIndex = 0;
      this.dgCustom.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgCustom_MouseDown);
      this.dgCustom.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgCustom_MouseMove);
      this.dgCustom.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgCustom_CellContentDoubleClick);
      this.dgCustom.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgCustom_MouseUp);
      this.dgCustom.SelectionChanged += new System.EventHandler(this.dgCustom_SelectionChanged);
      // 
      // txtAssembly
      // 
      this.txtAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtAssembly.Location = new System.Drawing.Point(8, 33);
      this.txtAssembly.Name = "txtAssembly";
      this.txtAssembly.Size = new System.Drawing.Size(339, 20);
      this.txtAssembly.TabIndex = 1;
      // 
      // btnOpen
      // 
      this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOpen.Location = new System.Drawing.Point(353, 33);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(24, 23);
      this.btnOpen.TabIndex = 2;
      this.btnOpen.Text = "...";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.Location = new System.Drawing.Point(289, 342);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(54, 23);
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // lblAssembly
      // 
      this.lblAssembly.AutoSize = true;
      this.lblAssembly.Location = new System.Drawing.Point(7, 17);
      this.lblAssembly.Name = "lblAssembly";
      this.lblAssembly.Size = new System.Drawing.Size(51, 13);
      this.lblAssembly.TabIndex = 4;
      this.lblAssembly.Text = "Assembly";
      // 
      // lblPlugins
      // 
      this.lblPlugins.AutoSize = true;
      this.lblPlugins.Location = new System.Drawing.Point(9, 56);
      this.lblPlugins.Name = "lblPlugins";
      this.lblPlugins.Size = new System.Drawing.Size(87, 13);
      this.lblPlugins.TabIndex = 5;
      this.lblPlugins.Text = "Available Plugins";
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(349, 342);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(52, 23);
      this.btnCancel.TabIndex = 8;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // grpAssembly
      // 
      this.grpAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpAssembly.Controls.Add(this.lblAssembly);
      this.grpAssembly.Controls.Add(this.txtAssembly);
      this.grpAssembly.Controls.Add(this.btnOpen);
      this.grpAssembly.Controls.Add(this.dgCustom);
      this.grpAssembly.Controls.Add(this.lblPlugins);
      this.grpAssembly.Location = new System.Drawing.Point(23, 12);
      this.grpAssembly.Name = "grpAssembly";
      this.grpAssembly.Size = new System.Drawing.Size(386, 169);
      this.grpAssembly.TabIndex = 9;
      this.grpAssembly.TabStop = false;
      this.grpAssembly.Text = "From existing Code";
      // 
      // optExisting
      // 
      this.optExisting.AutoSize = true;
      this.optExisting.Checked = true;
      this.optExisting.Location = new System.Drawing.Point(3, 12);
      this.optExisting.Name = "optExisting";
      this.optExisting.Size = new System.Drawing.Size(14, 13);
      this.optExisting.TabIndex = 10;
      this.optExisting.TabStop = true;
      this.optExisting.UseVisualStyleBackColor = true;
      this.optExisting.CheckedChanged += new System.EventHandler(this.opt_CheckedChanged);
      // 
      // optCustom
      // 
      this.optCustom.AutoSize = true;
      this.optCustom.Location = new System.Drawing.Point(3, 187);
      this.optCustom.Name = "optCustom";
      this.optCustom.Size = new System.Drawing.Size(14, 13);
      this.optCustom.TabIndex = 11;
      this.optCustom.UseVisualStyleBackColor = true;
      this.optCustom.CheckedChanged += new System.EventHandler(this.opt_CheckedChanged);
      // 
      // grpCustom
      // 
      this.grpCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpCustom.Controls.Add(this.lblEnd);
      this.grpCustom.Controls.Add(this.lblDeclare);
      this.grpCustom.Controls.Add(this.txtCode);
      this.grpCustom.Location = new System.Drawing.Point(23, 187);
      this.grpCustom.Name = "grpCustom";
      this.grpCustom.Size = new System.Drawing.Size(386, 149);
      this.grpCustom.TabIndex = 12;
      this.grpCustom.TabStop = false;
      this.grpCustom.Text = "Custom";
      // 
      // txtCode
      // 
      this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCode.Location = new System.Drawing.Point(8, 30);
      this.txtCode.Multiline = true;
      this.txtCode.Name = "txtCode";
      this.txtCode.Size = new System.Drawing.Size(369, 97);
      this.txtCode.TabIndex = 0;
      // 
      // lblDeclare
      // 
      this.lblDeclare.AutoSize = true;
      this.lblDeclare.Location = new System.Drawing.Point(9, 14);
      this.lblDeclare.Name = "lblDeclare";
      this.lblDeclare.Size = new System.Drawing.Size(126, 13);
      this.lblDeclare.TabIndex = 1;
      this.lblDeclare.Text = "static double CostStep() {\r\n";
      // 
      // lblEnd
      // 
      this.lblEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblEnd.AutoSize = true;
      this.lblEnd.Location = new System.Drawing.Point(9, 130);
      this.lblEnd.Name = "lblEnd";
      this.lblEnd.Size = new System.Drawing.Size(11, 13);
      this.lblEnd.TabIndex = 2;
      this.lblEnd.Text = "}";
      // 
      // WdgCustom
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(413, 377);
      this.Controls.Add(this.grpCustom);
      this.Controls.Add(this.optCustom);
      this.Controls.Add(this.optExisting);
      this.Controls.Add(this.grpAssembly);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgCustom";
      this.ShowInTaskbar = false;
      this.Text = "Plugin";
      ((System.ComponentModel.ISupportInitialize)(this.dgCustom)).EndInit();
      this.grpAssembly.ResumeLayout(false);
      this.grpAssembly.PerformLayout();
      this.grpCustom.ResumeLayout(false);
      this.grpCustom.PerformLayout();
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
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.GroupBox grpAssembly;
    private System.Windows.Forms.RadioButton optExisting;
    private System.Windows.Forms.RadioButton optCustom;
    private System.Windows.Forms.GroupBox grpCustom;
    private System.Windows.Forms.TextBox txtCode;
    private System.Windows.Forms.Label lblDeclare;
    private System.Windows.Forms.Label lblEnd;
    private System.Windows.Forms.ToolTip ttp;
  }
}