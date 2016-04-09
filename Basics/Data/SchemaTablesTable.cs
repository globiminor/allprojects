using System;
using System.Data;

namespace Basics.Data
{
  public class SchemaTablesTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public string TableName
      {
        get { return TableNameColumn.GetValue(this); }
        set { TableNameColumn.SetValue(this, value); }
      }
    }

    public static readonly TypedColumn<string> TableNameColumn = new TypedColumn<string>("TABLE_NAME");
    public SchemaTablesTable()
      : base("SchemaTables")
    {
      Columns.Add(TableNameColumn.CreateColumn());
    }

    public new Row NewRow()
    {
      return (Row)base.NewRow();
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      Row row = new Row(builder);
      return row;
    }

    public void AddRow(Row row)
    {
      Rows.Add(row);
    }
  }

  public class SchemaColumnsTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public string TableName
      {
        get { return TableNameColumn.GetValue(this); }
        set { TableNameColumn.SetValue(this, value); }
      }
      public string ColumnName
      {
        get { return ColumnNameColumn.GetValue(this); }
        set { ColumnNameColumn.SetValue(this, value); }
      }
      public int ColumnOrdinal
      {
        get { return ColumnOrdinalColumn.GetValue(this); }
        set { ColumnOrdinalColumn.SetValue(this, value); }
      }
      public Type DataType
      {
        get { return DataTypeColumn.GetValue(this); }
        set { DataTypeColumn.SetValue(this, value); }
      }
      public bool IsKey
      {
        get { return IsKeyColumn.GetValue(this); }
        set { IsKeyColumn.SetValue(this, value); }
      }

    }

    public static readonly TypedColumn<string> TableNameColumn = new TypedColumn<string>("TABLE_NAME");
    public static readonly TypedColumn<string> ColumnNameColumn = new TypedColumn<string>("COLUMN_NAME");
    public static readonly TypedColumn<int> ColumnOrdinalColumn = new TypedColumn<int>("COLUMN_ORDINAL");
    public static readonly TypedColumn<Type> DataTypeColumn = new TypedColumn<Type>("DATA_TYPE");
    public static readonly TypedColumn<bool> IsKeyColumn = new TypedColumn<bool>("IS_KEY");

    public SchemaColumnsTable()
      : base("SchemaColumns")
    {
      Columns.Add(TableNameColumn.CreateColumn());
      Columns.Add(ColumnNameColumn.CreateColumn());
      Columns.Add(ColumnOrdinalColumn.CreateColumn());
      Columns.Add(DataTypeColumn.CreateColumn());
      Columns.Add(IsKeyColumn.CreateColumn());
    }

    public new Row NewRow()
    {
      return (Row)base.NewRow();
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      Row row = new Row(builder);
      return row;
    }

    public void AddRow(Row row)
    {
      Rows.Add(row);
    }

    public Row AddSchemaColumn(string name, Type type)
    {
      Row row = NewRow();
      row.ColumnName = name;
      row.DataType = type;
      Rows.Add(row);
      return row;
    }
  }

}