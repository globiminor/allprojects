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
    private System.Windows.Forms.TreeView _treData;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container _components = null;

    public CntToc()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      _treeData = _treData.Nodes.Add("Data");
      _mapData = new TMap.GroupMapData("Data");
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_components != null)
        {
          _components.Dispose();
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
      this._treData = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // treData
      // 
      this._treData.Dock = System.Windows.Forms.DockStyle.Fill;
      this._treData.HideSelection = false;
      this._treData.ImageIndex = -1;
      this._treData.Location = new System.Drawing.Point(0, 0);
      this._treData.Name = "treData";
      this._treData.SelectedImageIndex = -1;
      this._treData.Size = new System.Drawing.Size(150, 150);
      this._treData.TabIndex = 0;
      this._treData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreData_MouseDown);
      this._treData.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreData_MouseUp);
      this._treData.DoubleClick += new System.EventHandler(this.TreData_DoubleClick);
      this._treData.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TreData_MouseMove);
      // 
      // WdgToc
      // 
      this.AllowDrop = true;
      this.Controls.Add(this._treData);
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

    private void TreData_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      try
      {
        System.Windows.Forms.TreeNode pNode = _treData.GetNodeAt(e.X, e.Y);
        if (pNode == _treData.SelectedNode)
        { _dragNode = _treData.SelectedNode; }
        else
        { _dragNode = null; }
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void TreData_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
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

        pNode = _treData.GetNodeAt(e.X, e.Y);
        if (pNode == _dragNode)
        { return; }
        if (pNode == null)
        { return; }

        if (_dragBefore == pNode)
        { return; }

        if (_dragBefore != null)
        {
          System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, _dragBefore.Bounds.Top,
            _treData.Width, _dragBefore.Bounds.Height);
          _treData.Invalidate(rect);
        }

        _dragBefore = pNode;
        System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(_treData.Handle);
        System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
        g.DrawLine(p, 0, pNode.Bounds.Top, _treData.Width, pNode.Bounds.Top);
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void TreData_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
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

    private WdgProperties _wdgProps;
    private void TreData_DoubleClick(object sender, System.EventArgs e)
    {
      try
      {
        if (_treData.SelectedNode == null)
        { return; }
        if (_treData.SelectedNode.Tag as TMap.MapData == null)
        { return; }
        if (_wdgProps == null)
        { _wdgProps = new WdgProperties(); }
        _wdgProps.SetData((TMap.MapData)_treData.SelectedNode.Tag);
        _wdgProps.ShowDialog();
      }
      catch (System.Exception exp)
      { System.Windows.Forms.MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    #endregion
  }
}
