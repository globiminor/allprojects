namespace Dhm
{
  partial class WdgAdapt
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
      this.btnOK = new System.Windows.Forms.Button();
      this.grdContours = new System.Windows.Forms.DataGridView();
      this.btnZoom = new System.Windows.Forms.Button();
      this.btnRedraw = new System.Windows.Forms.Button();
      this.btnJoin = new System.Windows.Forms.Button();
      this.btnSave = new System.Windows.Forms.Button();
      this.grdJoinOptions = new System.Windows.Forms.DataGridView();
      ((System.ComponentModel.ISupportInitialize)(this.grdContours)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.grdJoinOptions)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(221, 236);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(61, 23);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "Continue";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // grdContours
      // 
      this.grdContours.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grdContours.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grdContours.Location = new System.Drawing.Point(12, 5);
      this.grdContours.Name = "grdContours";
      this.grdContours.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.grdContours.Size = new System.Drawing.Size(180, 105);
      this.grdContours.TabIndex = 1;
      // 
      // btnZoom
      // 
      this.btnZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnZoom.Location = new System.Drawing.Point(221, 5);
      this.btnZoom.Name = "btnZoom";
      this.btnZoom.Size = new System.Drawing.Size(61, 23);
      this.btnZoom.TabIndex = 2;
      this.btnZoom.Text = "Zoom";
      this.btnZoom.UseVisualStyleBackColor = true;
      this.btnZoom.Click += new System.EventHandler(this.BtnZoom_Click);
      // 
      // btnRedraw
      // 
      this.btnRedraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRedraw.Location = new System.Drawing.Point(221, 34);
      this.btnRedraw.Name = "btnRedraw";
      this.btnRedraw.Size = new System.Drawing.Size(61, 23);
      this.btnRedraw.TabIndex = 3;
      this.btnRedraw.Text = "Redraw";
      this.btnRedraw.UseVisualStyleBackColor = true;
      this.btnRedraw.Click += new System.EventHandler(this.BtnRedraw_Click);
      // 
      // btnJoin
      // 
      this.btnJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnJoin.Location = new System.Drawing.Point(221, 145);
      this.btnJoin.Name = "btnJoin";
      this.btnJoin.Size = new System.Drawing.Size(61, 23);
      this.btnJoin.TabIndex = 4;
      this.btnJoin.Text = "Join";
      this.btnJoin.UseVisualStyleBackColor = true;
      this.btnJoin.Click += new System.EventHandler(this.BtnJoin_Click);
      // 
      // btnSave
      // 
      this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.Location = new System.Drawing.Point(221, 207);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(61, 23);
      this.btnSave.TabIndex = 5;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
      // 
      // grdJoinOptions
      // 
      this.grdJoinOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grdJoinOptions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grdJoinOptions.Location = new System.Drawing.Point(12, 145);
      this.grdJoinOptions.Name = "grdJoinOptions";
      this.grdJoinOptions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.grdJoinOptions.Size = new System.Drawing.Size(180, 105);
      this.grdJoinOptions.TabIndex = 6;
      // 
      // WdgAdapt
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.grdJoinOptions);
      this.Controls.Add(this.btnSave);
      this.Controls.Add(this.btnJoin);
      this.Controls.Add(this.btnRedraw);
      this.Controls.Add(this.btnZoom);
      this.Controls.Add(this.grdContours);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgAdapt";
      this.ShowInTaskbar = false;
      this.Text = "WdgAdapt";
      ((System.ComponentModel.ISupportInitialize)(this.grdContours)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.grdJoinOptions)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.DataGridView grdContours;
    private System.Windows.Forms.Button btnZoom;
    private System.Windows.Forms.Button btnRedraw;
    private System.Windows.Forms.Button btnJoin;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.DataGridView grdJoinOptions;
  }
}