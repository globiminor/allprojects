namespace TMapWin.Browse
{
  partial class WdgBrowse
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
      this.lstData = new System.Windows.Forms.ListView();
      this.lstParents = new System.Windows.Forms.ComboBox();
      this.txtNew = new System.Windows.Forms.TextBox();
      this.btnOpen = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lstData
      // 
      this.lstData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lstData.LabelEdit = true;
      this.lstData.Location = new System.Drawing.Point(12, 41);
      this.lstData.Name = "lstData";
      this.lstData.Size = new System.Drawing.Size(447, 188);
      this.lstData.TabIndex = 1;
      this.lstData.UseCompatibleStateImageBehavior = false;
      this.lstData.View = System.Windows.Forms.View.List;
      this.lstData.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this._lstData_AfterLabelEdit);
      this.lstData.SelectedIndexChanged += new System.EventHandler(this._lstData_SelectedIndexChanged);
      this.lstData.DoubleClick += new System.EventHandler(this._lstData_DoubleClick);
      this.lstData.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this._lstData_BeforeLabelEdit);
      // 
      // lstParents
      // 
      this.lstParents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.lstParents.FormattingEnabled = true;
      this.lstParents.Location = new System.Drawing.Point(12, 12);
      this.lstParents.Name = "lstParents";
      this.lstParents.Size = new System.Drawing.Size(279, 21);
      this.lstParents.TabIndex = 0;
      this.lstParents.SelectedIndexChanged += new System.EventHandler(this._lstParents_SelectedIndexChanged);
      // 
      // txtNew
      // 
      this.txtNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.txtNew.Location = new System.Drawing.Point(11, 240);
      this.txtNew.Name = "txtNew";
      this.txtNew.Size = new System.Drawing.Size(279, 20);
      this.txtNew.TabIndex = 2;
      // 
      // btnOpen
      // 
      this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOpen.Location = new System.Drawing.Point(384, 238);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(75, 23);
      this.btnOpen.TabIndex = 3;
      this.btnOpen.Text = "Open";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
      // 
      // WdgBrowse
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(471, 273);
      this.Controls.Add(this.txtNew);
      this.Controls.Add(this.btnOpen);
      this.Controls.Add(this.lstParents);
      this.Controls.Add(this.lstData);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgBrowse";
      this.ShowInTaskbar = false;
      this.Text = "WdgOpen";
      this.Load += new System.EventHandler(this.WdgOpen_Load);
      this.VisibleChanged += new System.EventHandler(this.WdgOpen_VisibleChanged);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    protected System.Windows.Forms.Button btnOpen;
    protected System.Windows.Forms.ListView lstData;
    protected System.Windows.Forms.ComboBox lstParents;
    protected System.Windows.Forms.TextBox txtNew;
  }
}