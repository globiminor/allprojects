namespace TMapWin
{
  partial class CntSelection
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.treSelection = new System.Windows.Forms.TreeView();
      this.grdRecord = new System.Windows.Forms.DataGridView();
      this.splData = new System.Windows.Forms.Splitter();
      ((System.ComponentModel.ISupportInitialize)(this.grdRecord)).BeginInit();
      this.SuspendLayout();
      // 
      // treSelection
      // 
      this.treSelection.Dock = System.Windows.Forms.DockStyle.Top;
      this.treSelection.HideSelection = false;
      this.treSelection.Location = new System.Drawing.Point(0, 0);
      this.treSelection.Name = "treSelection";
      this.treSelection.Size = new System.Drawing.Size(150, 160);
      this.treSelection.TabIndex = 0;
      this.treSelection.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treSelection_AfterSelect);
      // 
      // grdRecord
      // 
      this.grdRecord.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grdRecord.Location = new System.Drawing.Point(0, 160);
      this.grdRecord.Name = "grdRecord";
      this.grdRecord.ReadOnly = true;
      this.grdRecord.RowHeadersVisible = false;
      this.grdRecord.Size = new System.Drawing.Size(150, 88);
      this.grdRecord.TabIndex = 1;
      // 
      // splData
      // 
      this.splData.Dock = System.Windows.Forms.DockStyle.Top;
      this.splData.Location = new System.Drawing.Point(0, 160);
      this.splData.Name = "splData";
      this.splData.Size = new System.Drawing.Size(150, 4);
      this.splData.TabIndex = 2;
      this.splData.TabStop = false;
      // 
      // CntSelection
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.splData);
      this.Controls.Add(this.grdRecord);
      this.Controls.Add(this.treSelection);
      this.Name = "CntSelection";
      this.Size = new System.Drawing.Size(150, 248);
      ((System.ComponentModel.ISupportInitialize)(this.grdRecord)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private System.Windows.Forms.TreeView treSelection;
    private System.Windows.Forms.DataGridView grdRecord;
    private System.Windows.Forms.Splitter splData;

  }
}
