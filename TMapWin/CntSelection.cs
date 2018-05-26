using System;
using System.Data;
using System.Windows.Forms;
using TMap;

namespace TMapWin
{
  public partial class CntSelection : UserControl
  {
    [Obsolete("Use Map/RowTreeNode")]
    private class TreeNode { }

    private class MapTreeNode : System.Windows.Forms.TreeNode
    {
      private readonly MapData _map;

      public MapTreeNode(MapData map)
        : base(map.Name)
      {
        _map = map;
      }
      public MapData Map
      { get { return _map; } }
    }

    private class RowTreeNode : System.Windows.Forms.TreeNode
    {
      private readonly DataRow _row;

      public RowTreeNode(DataRow row)
        : base(row[0].ToString())
      {
        _row = row;
      }
      public DataRow Row
      { get { return _row; } }
    }

    private MapTreeNode _currentMapNode;
    private Attribute.TblDataDataTable _attrs;
    private ContextMenuStrip _nodeMenu;

    public CntSelection()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      _attrs = new Attribute.TblDataDataTable();
      DataView vAttr = new DataView(_attrs);
      vAttr.AllowDelete = false;
      vAttr.AllowNew = false;

      DataGridViewTextBoxColumn col;

      grdRecord.AutoGenerateColumns = false;

      col = new DataGridViewTextBoxColumn();
      col.DataPropertyName = _attrs.AttributeColumn.ColumnName;
      col.ReadOnly = true;
      col.HeaderText = "Attribute";
      grdRecord.Columns.Add(col);

      col = new DataGridViewTextBoxColumn();
      col.DataPropertyName = _attrs.ValueColumn.ColumnName;
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.HeaderText = "Value";
      grdRecord.Columns.Add(col);

      grdRecord.DataSource = vAttr;

      treSelection.NodeMouseClick += TreSelection_NodeMouseClick;
      _nodeMenu = new ContextMenuStrip();
      _nodeMenu.Items.Add("ZoomTo", null, NodeMenu_ZoomTo);
      _nodeMenu.Items.Add("Flash", null, NodeMenu_Flash);
    }

    public IContext Map
    {
      get { return _map; }
      set { _map = value; }
    }

    public void Clear()
    {
      treSelection.Nodes.Clear();
      _attrs.Clear();
    }
    public int Add(MapData map)
    {
      _currentMapNode = new MapTreeNode(map);

      return treSelection.Nodes.Add(_currentMapNode);
    }

    public int Add(DataRow row)
    {
      RowTreeNode node = new RowTreeNode(row);
      node.Tag = row;
      node.ContextMenuStrip = _nodeMenu;
      return _currentMapNode.Nodes.Add(node);
    }

    public void InitSetting()
    {
      treSelection.ExpandAll();
      if (treSelection.Nodes.Count > 0)
      {
        treSelection.SelectedNode = treSelection.Nodes[0].Nodes[0];
      }
    }

    private System.Windows.Forms.TreeNode _lastClickedNode;
    private IContext _map;

    void TreSelection_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
      try
      {
        _lastClickedNode = e.Node;
      }
      catch (Exception exp)
      { MessageBox.Show(Basics.Utils.GetMsg(exp)); }
    }

    private void NodeMenu_ZoomTo(object sender, EventArgs args)
    {
      try
      {
        RowTreeNode node = _lastClickedNode as RowTreeNode;
        if (node == null)
        { return; }
        if (_map == null)
        { return; }

        Basics.Geom.IBox box = null;
        foreach (object item in node.Row.ItemArray)
        {
          Basics.Geom.IGeometry geom = item as Basics.Geom.IGeometry;
          if (geom == null)
          { continue; }

          if (box == null)
          { box = geom.Extent.Clone(); }
          else
          { box.Include(geom.Extent); }
        }
        if (box != null)
        {
          Basics.Geom.Point p0 = Basics.Geom.Point.CastOrCreate(box.Min);
          Basics.Geom.Point p1 = Basics.Geom.Point.CastOrCreate(box.Max);
          Basics.Geom.Point pm = 0.5 * (p0 + p1);
          p0 = pm + 1.1 * (p0 - pm);
          p1 = pm + 1.1 * (p1 - pm);
          box = new Basics.Geom.Box(p0, p1);
        }
        _map.SetExtent(box);
        _map.Draw();
      }
      catch (Exception exp)
      { MessageBox.Show(Basics.Utils.GetMsg(exp)); }
    }
    private void NodeMenu_Flash(object sender, EventArgs args)
    {
    }

    private void TreSelection_AfterSelect(object sender, TreeViewEventArgs e)
    {
      try
      {
        _attrs.Clear();
        if (treSelection.SelectedNode is RowTreeNode rowNode)
        {
          DataRow row = rowNode.Row;
          foreach (DataColumn col in row.Table.Columns)
          { _attrs.AddTblDataRow(col.ColumnName, row[col].ToString()); }
        }
      }
      catch (Exception exp)
      { MessageBox.Show(Basics.Utils.GetMsg(exp)); }
    }
  }
}
