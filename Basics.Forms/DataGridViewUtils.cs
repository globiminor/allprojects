using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Basics.Forms
{
  public static class DataGridViewUtils
  {
    public static IEnumerable<ToolStripItem> Enum(this ToolStripItemCollection items)
    {
      foreach (var o in items)
      {
        yield return (ToolStripItem)o;
      }
    }

    public static IEnumerable<DataGridViewRow> Enum(this DataGridViewSelectedRowCollection rows)
    {
      foreach (var o in rows)
      {
        yield return (DataGridViewRow)o;
      }
    }
    public static IEnumerable<DataGridViewRow> Enum(this DataGridViewRowCollection rows)
    {
      foreach (var o in rows)
      {
        yield return (DataGridViewRow)o;
      }
    }
    public static IEnumerable<DataGridViewColumn> Enum(this DataGridViewColumnCollection columns)
    {
      foreach (var o in columns)
      {
        yield return (DataGridViewColumn)o;
      }
    }
    public static IEnumerable<ListViewItem> Enum(this ListView.SelectedListViewItemCollection items)
    {
      foreach (var o in items)
      {
        yield return (ListViewItem)o;
      }
    }

    public static IEnumerable<TreeNode> Enum(this TreeNodeCollection nodes)
    {
      foreach (var o in nodes)
      {
        yield return (TreeNode)o;
      }
    }

    public static IEnumerable GetSelectedItems(this DataGridView grd, bool returnAllIfEmptySelection = true)
    {
      IEnumerable rows;
      if (grd.SelectedRows.Count == 0 && returnAllIfEmptySelection)
        rows = grd.Rows;
      else
        rows = grd.SelectedRows;

      foreach (var o in rows)
      {
        DataGridViewRow row = (DataGridViewRow)o;
        yield return row.DataBoundItem;
      }
    }
  }
}
