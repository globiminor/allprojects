namespace TMapWin
{
  /// <summary>
  /// Summary description for WdgToc.
  /// </summary>
  public class CntToc : System.Windows.Forms.UserControl
  {
    // member variables
    private TMap.GroupMapData _mapData;

    private System.Windows.Forms.TreeNode _treeData;
    private System.Windows.Forms.TreeNode _dragNode;
    private System.Windows.Forms.TreeNode _dragBefore;
    // designer variables
    private System.Windows.Forms.TreeView treData;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public CntToc()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      _treeData = treData.Nodes.Add("Data");
      _mapData = new TMap.GroupMapData("Data");
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      this.treData = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // treData
      // 
      this.treData.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treData.HideSelection = false;
      this.treData.ImageIndex = -1;
      this.treData.Location = new System.Drawing.Point(0, 0);
      this.treData.Name = "treData";
      this.treData.SelectedImageIndex = -1;
      this.treData.Size = new System.Drawing.Size(150, 150);
      this.treData.TabIndex = 0;
      this.treData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treData_MouseDown);
      this.treData.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treData_MouseUp);
      this.treData.DoubleClick += new System.EventHandler(this.treData_DoubleClick);
      this.treData.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treData_MouseMove);
      // 
      // WdgToc
      // 
      this.AllowDrop = true;
      this.Controls.Add(this.treData);
      this.Name = "WdgToc";
      this.ResumeLayout(false);

    }
    #endregion

    public TMap.GroupMapData MapData
    {
      get { return _mapData; }
    }

    public TMap.GroupMapData GetSortedMapData()
    {
      TMap.GroupMapData mapData = GetMapData(_treeData.Nodes);
      if (mapData != null) return mapData;
      return new TMap.GroupMapData(null);
    }
    private TMap.GroupMapData GetMapData(System.Windows.Forms.TreeNodeCollection nodes)
    {
      int nNodes = nodes.Count;
      if (nNodes == 0)
      { return null; }

      TMap.GroupMapData group = new TMap.GroupMapData(null);
      for (int i = nodes.Count - 1; i >= 0; i--)
      {
        System.Windows.Forms.TreeNode node = nodes[i];
        TMap.MapData mapData = GetMapData(node.Nodes);
        if (mapData == null)
        {
          mapData = (TMap.MapData)node.Tag;
        }
        group.Subparts.Add(mapData);
      }
      return group;
    }

    public void BuildTree()
    {
      BuildTree(_treeData, _mapData);
    }
    private void BuildTree(System.Windows.Forms.TreeNode node, TMap.GroupMapData data)
    {
      node.Nodes.Clear();
      node.Text = data.Name;
      node.Tag = data;

      foreach (TMap.MapData subpart in data.Subparts)
      {
        System.Windows.Forms.TreeNode subnode = new System.Windows.Forms.TreeNode(subpart.Name);
        subnode.Tag = subpart;
        node.Nodes.Add(subnode);
        if (subpart is TMap.GroupMapData)
        {
          BuildTree(subnode, (TMap.GroupMapData)subpart);
        }
      }
    }

    #region events

    private void treData_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      try
      {
        System.Windows.Forms.TreeNode pNode = treData.GetNodeAt(e.X, e.Y);
        if (pNode == treData.SelectedNode)
        { _dragNode = treData.SelectedNode; }
        else
        { _dragNode = null; }
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void treData_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      try
      {
        System.Windows.Forms.TreeNode pNode;
        if (e.Button != System.Windows.Forms.MouseButtons.Left)
        {
          _dragNode = null;
          return;
        }
        if (_dragNode == null)
        { return; }

        pNode = treData.GetNodeAt(e.X, e.Y);
        if (pNode == _dragNode)
        { return; }
        if (pNode == null)
        { return; }

        if (_dragBefore == pNode)
        { return; }

        if (_dragBefore != null)
        {
          System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, _dragBefore.Bounds.Top,
            treData.Width, _dragBefore.Bounds.Height);
          treData.Invalidate(rect);
        }

        _dragBefore = pNode;
        System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(treData.Handle);
        System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
        g.DrawLine(p, 0, pNode.Bounds.Top, treData.Width, pNode.Bounds.Top);
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void treData_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      try
      {
        if (_dragNode != null && _dragBefore != null)
        {
          _dragNode.Remove();
          _dragBefore.Parent.Nodes.Insert(_dragBefore.Index, _dragNode);
        }
        _dragNode = null;
        _dragBefore = null;
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private WdgProperties wdgProps;
    private void treData_DoubleClick(object sender, System.EventArgs e)
    {
      try
      {
        if (treData.SelectedNode == null)
        { return; }
        if (treData.SelectedNode.Tag as TMap.MapData == null)
        { return; }
        if (wdgProps == null)
        { wdgProps = new WdgProperties(); }
        wdgProps.SetData((TMap.MapData)treData.SelectedNode.Tag);
        wdgProps.ShowDialog();
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    #endregion
  }
}
