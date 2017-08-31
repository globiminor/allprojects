using System;
using System.Data;

namespace Basics.Data
{
  public abstract class TypedColumn
  {
    private readonly string _name;
    public string Name { get { return _name; } }

    protected TypedColumn(string name)
    {
      _name = name;
    }

    public override string ToString()
    {
      return _name;
    }

    public abstract DataColumn CreatePrimaryKey(DataTable table);
    public abstract DataColumn CreateColumn();

    public bool IsNull(DataRow row)
    {
      return row.IsNull(_name);
    }

    public void SetNull(DataRow row)
    {
      row[_name] = DBNull.Value;
    }
  }

  public class TypedColumn<T> : TypedColumn
  {

    public TypedColumn(string name)
      : base(name)
    { }

    public override DataColumn CreatePrimaryKey(DataTable table)
    {
      DataColumn column = CreateColumn();
      column.AutoIncrement = true;
      table.Columns.Add(column);
      table.PrimaryKey = new DataColumn[] { column };
      return column;
    }

    public Type DataType { get { return typeof(T); } }

    public override DataColumn CreateColumn()
    {
      DataColumn column = new DataColumn(Name, typeof(T));
      return column;
    }

    public T GetValue(DataRowView row)
    {
      try
      {
        return (T)row[Name];
      }
      catch (Exception e)
      { throw new InvalidOperationException("Error getting value for " + GetRowInfo(row), e); }
    }
    public T GetValue(DataRow row)
    {
      try
      {
        return (T)row[Name];
      }
      catch (Exception e)
      { throw new InvalidOperationException("Error getting value for " + GetRowInfo(row), e); }
    }

    private string GetRowInfo(DataRowView row)
    {
      if (row == null)
      { return string.Format("row == NULL; (Column = '{0}')", Name); }
      return GetRowInfo(row.Row);
    }

    private string GetRowInfo(DataRow row)
    {
      if (row == null)
      { return string.Format("row == NULL; (Column = '{0}')", Name); }

      DataTable tbl = row.Table;
      if (tbl == null)
      { return string.Format("Column '{0}'", Name); }

      return string.Format("Column '{0}'; (Table = '{1}')", Name, tbl.TableName);
    }

    public T GetValue(DataRowView row, T nullValue)
    {
      return GetValue(row.Row, nullValue);
    }

    public T GetValue(DataRow row, T nullValue)
    {
      object value = row[Name];
      if (value == DBNull.Value)
      {
        value = nullValue;
      }
      return (T)value;
    }

    public void SetValue(DataRow row, T value)
    {
      row[Name] = value;
    }
  }
}
