
using System.Data;
using System;
using Basics.Geom;
using System.Data.Common;
using System.Collections;

namespace Basics.Data.DataSetDb
{
  [System.ComponentModel.ToolboxItem(false)]
  public class EnumConnection : DbBaseConnection
  {
    private EnumTransaction _trans;
    private readonly IEnumerator _enum;
    private DataTable _schema;

    public EnumConnection(IEnumerator enumerator, DataTable schema)
    {
      _enum = enumerator;
      _schema = schema;
    }

    public IEnumerator Enumerator
    {
      get { return _enum; }
    }
    public override void Close()
    { }
    public override void Open()
    { }

    public override string ConnectionString
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public override void ChangeDatabase(string databaseName)
    { throw new Exception("The method or operation is not implemented."); }
    public override string Database
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override string DataSource
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override string ServerVersion
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override ConnectionState State
    {
      get { return ConnectionState.Open; }
    }

    public override DataTable GetSchema(string collectionName)
    {
      if (collectionName.Equals("Tables", StringComparison.InvariantCultureIgnoreCase))
      {
        SchemaTablesTable tbl = new SchemaTablesTable();

        SchemaTablesTable.Row row = tbl.NewRow();
        row.TableName = _schema.TableName;
        tbl.AddRow(row);
        row.AcceptChanges();

        return tbl;
      }
      throw new NotImplementedException();
    }

    public override SchemaColumnsTable GetTableSchema(string name)
    {
      SchemaColumnsTable schema;

      schema = new SchemaColumnsTable();
      DataTable tbl = _schema;
      foreach (var o in tbl.Columns)
      {
        DataColumn column = (DataColumn)o;
        Type type = column.DataType;
        schema.AddSchemaColumn(column.ColumnName, type);
      }
      return schema;
    }

    public override IBox GetExtent(string name)
    {
      throw new NotImplementedException();
    }
    public new EnumTransaction BeginTransaction()
    {
      return (EnumTransaction)base.BeginTransaction();
    }
    public new EnumTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
      return (EnumTransaction)base.BeginTransaction(isolationLevel);
    }
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      _trans = EnumTransaction.Start(this, isolationLevel);
      return _trans;
    }

    public new EnumCommand CreateCommand()
    {
      return (EnumCommand)CreateDbCommand();
    }
    protected override DbCommand CreateDbCommand()
    {
      return new EnumCommand(this);
    }

    public new EnumAdapter CreateAdapter()
    {
      return (EnumAdapter)base.CreateAdapter();
    }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      EnumAdapter apt = new EnumAdapter(this);
      return apt;
    }

  }

}
