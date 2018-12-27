using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Basics.Data
{
  public static class Utils
  {
    public static IEnumerable<DataTable> Enum(this DataTableCollection tables)
    {
      foreach (var o in tables)
      {
        yield return (DataTable)o;
      }
    }
    public static IEnumerable<DataColumn> Enum(this DataColumnCollection columns)
    {
      foreach (var o in columns)
      {
        yield return (DataColumn)o;
      }
    }

    public static IEnumerable<DataRelation> Enum(this DataRelationCollection rels)
    {
      foreach (var o in rels)
      {
        yield return (DataRelation)o;
      }
    }
    public static IEnumerable<DataRow> Enum(this DataRowCollection rows)
    {
      foreach (var o in rows)
      {
        yield return (DataRow)o;
      }
    }
    public static IEnumerable<DataRowView> Enum(this DataView view)
    {
      foreach (var o in view)
      {
        yield return (DataRowView)o;
      }
    }

    public static IEnumerable<XmlNode> Enum(this XmlNodeList nodes)
    {
      foreach (var o in nodes)
      {
        yield return (XmlNode)o;
      }
    }
    
    public static IEnumerable<System.Data.Common.DbParameter> Enum(this System.Data.Common.DbParameterCollection pars)
    {
      foreach (var o in pars)
      {
        yield return (System.Data.Common.DbParameter)o;
      }
    }


    public static IEnumerable<System.ComponentModel.ListSortDescription> Enum(this System.ComponentModel.ListSortDescriptionCollection sorts)
    {
      foreach (var o in sorts)
      {
        yield return (System.ComponentModel.ListSortDescription)o;
      }
    }
    public static IEnumerable<System.ComponentModel.PropertyDescriptor> Enum(this System.ComponentModel.PropertyDescriptorCollection props)
    {
      foreach (var o in props)
      {
        yield return (System.ComponentModel.PropertyDescriptor)o;
      }
    }

    public static DataTable GetTemplateTable(DataTable table)
    {
      DataTable tmpl = table.Clone();
      tmpl.PrimaryKey = null;
      foreach (var col in tmpl.Columns.Enum())
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
      foreach (var row in rows)
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
