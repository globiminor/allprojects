using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Basics.Forms
{
  public partial class CntCustomFilter : UserControl, FilterHeaderCell.IFilterControl
  {
    private bool _focused;
    private bool _applyFilter;
    private string _lastFilterStatement;

    public CntCustomFilter()
    {
      InitializeComponent();
      _focused = true;
    }

    string FilterHeaderCell.IFilterControl.FilterText
    {
      get { return _lastFilterStatement; }
    }

    public bool ApplyFilter { get { return _applyFilter; } }

    public void InitColumn(DataGridViewColumn column, IList<object> values)
    {
      _treValues.Nodes.Clear();
      _applyFilter = false;
      foreach (object value in values)
      {
        _treValues.Nodes.Add(new TreeNode($"{value}"));
      }
      int nodeHeight = _treValues.Nodes.Count > 0 ? _treValues.Nodes[0].Bounds.Height : 16;
      int fullHeight = (_treValues.Nodes.Count + 2) * nodeHeight + Height - _treValues.Height;
      Height = Math.Max(Math.Min(column.DataGridView.Height - 40, fullHeight), 100);
    }

    private bool TryAppendInStatement(StringBuilder sb, DataGridViewColumn col)
    {
      List<string> values = new List<string>();
      foreach (TreeNode node in _treValues.Nodes)
      {
        if (node.Checked)
        {
          values.Add(node.Text);
        }
      }
      if (values.Count == 0)
      { return false; }

      sb.Append(" IN (");
      bool first = true;
      foreach (string value in values)
      {
        if (first)
        { first = false; }
        else
        { sb.Append(", "); }
        if (col?.ValueType == typeof(string))
        { sb.Append($"'{value}'"); }
        else
        { sb.Append($"{value}"); }
      }
      sb.Append(")");
      return true;
    }
    public string GetFilterStatement(DataGridViewColumn column)
    {
      _lastFilterStatement = null;
      StringBuilder sb = new StringBuilder();
      sb.Append($"{column.DataPropertyName}");

      if (!TryAppendInStatement(sb, column))
      { return null; }

      _lastFilterStatement = sb.ToString();
      return _lastFilterStatement;
    }

    bool FilterHeaderCell.IFilterControl.Focused
    {
      get { return _focused; }
    }

    private void btnApply_Click(object sender, System.EventArgs e)
    {
      _applyFilter = true;
      Hide();
    }
  }
}
