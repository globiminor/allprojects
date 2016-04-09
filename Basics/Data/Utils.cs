using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Basics.Data
{
  public static class Utils
  {
    public static DataTable GetTemplateTable(DataTable table)
    {
      DataTable tmpl = table.Clone();
      tmpl.PrimaryKey = null;
      foreach (DataColumn col in tmpl.Columns)
      { col.AllowDBNull = true; }

      return tmpl;
    }

    public static string GetInListStatement<T>(IEnumerable<T> rows,
      DataColumn valueColumn, DataColumn whereColumn) where T : DataRow
    {
      string preString = "";
      string postString = "";

      if (whereColumn.DataType == typeof(string))
      {
        preString = "'";
        postString = "'";
      }

      StringBuilder inList = new StringBuilder();
      foreach (DataRow row in rows)
      {
        if (inList.Length > 0)
        { inList.Append(", "); }

        inList.AppendFormat("{0}{1}{2}", preString, row[valueColumn.ColumnName], postString);
      }
      if (inList.Length == 0)
      {
        return "1 = 0"; // false expression
      }
      string where = string.Format("{0} IN ({1})", whereColumn.ColumnName, inList);

      return where;
    }
  }
}
