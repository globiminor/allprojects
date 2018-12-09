using System.Collections;
using System.Windows.Forms;

namespace Basics.Forms
{
  public static class DataGridViewUtils
  {
    public static IEnumerable GetSelectedItems(this DataGridView grd, bool returnAllIfEmptySelection = true)
    {
      IEnumerable rows;
      if (grd.SelectedRows.Count == 0 && returnAllIfEmptySelection)
        rows = grd.Rows;
      else
        rows = grd.SelectedRows;

      foreach (DataGridViewRow row in rows)
      {
        yield return row.DataBoundItem;
      }
    }
  }
}
