
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Basics.Data;

namespace Shape.Data
{
  [System.ComponentModel.ToolboxItem(false)]
  public class ShapeConnection : DbBaseConnection
  {
    public event CancelEventHandler StartingOperation;
    public event EventHandler CommittingOperation;
    public event EventHandler AbortingOperation;

    private ShapeTransaction _trans;
    private string _connection;

    public static readonly string FieldShape = "Shape";
    public static readonly string FieldSymbol = "Symbol";
    public static readonly string FieldAngle = "Angle";

    public ShapeConnection(string connectionString)
    {
      _connection = connectionString;
    }

    public override void Close()
    { }
    public override void Open()
    { }

    public override Basics.Geom.IBox GetExtent(string tableName)
    {
      string fullPath = GetFullPath(tableName);
      using (ShpReader reader = new ShpReader(fullPath))
      {
        Basics.Geom.Box extent = reader.Extent;
        return extent;
      }
    }

    public override string ConnectionString
    {
      get { return _connection; }
      set { _connection = value; }
    }

    public override void ChangeDatabase(string databaseName)
    { throw new Exception("The method or operation is not implemented."); }
    public override string Database
    {
      get { return _connection; }
    }
    public override string DataSource
    {
      get { return _connection; }
    }
    public override string ServerVersion
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override ConnectionState State
    {
      get { return ConnectionState.Open; }
    }

    private string GetFullPath(string name)
    {
      if (!System.IO.Path.HasExtension(name) || System.IO.Path.GetExtension(name) != ".shp")
      { name = name + ".shp"; }
      string fullPath = System.IO.Path.Combine(_connection, name);
      return fullPath;
    }
    public override SchemaColumnsTable GetTableSchema(string name)
    {
      string fullPath = GetFullPath(name);
      if (!System.IO.File.Exists(fullPath))
      { return null; }
      DBase.DBaseReader reader = new DBase.DBaseReader(fullPath);
      SchemaColumnsTable schema = new SchemaColumnsTable();
      schema.AddSchemaColumn("Shape", typeof(Basics.Geom.IGeometry));
      foreach (var column in reader.Schema.Columns)
      {
        schema.AddSchemaColumn(column.ColumnName, column.DataType);
      }
      reader.Close();
      return schema;
    }

    public new ShapeTransaction BeginTransaction()
    {
      return (ShapeTransaction)base.BeginTransaction();
    }
    public new ShapeTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
      return (ShapeTransaction)base.BeginTransaction(isolationLevel);
    }
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      _trans = ShapeTransaction.Start(this, isolationLevel);
      return _trans;
    }

    public new ShapeCommand CreateCommand()
    {
      return (ShapeCommand)CreateDbCommand();
    }
    protected override DbCommand CreateDbCommand()
    {
      return new ShapeCommand(this);
    }

    internal void OnStartingOperation(CancelEventArgs args)
    {
      StartingOperation?.Invoke(this, args);
    }

    internal void OnCommittingOperation(EventArgs args)
    {
      CommittingOperation?.Invoke(this, args);
    }

    internal void OnAbortingOperation(EventArgs args)
    {
      AbortingOperation?.Invoke(this, args);
    }

    public new ShapeAdapter CreateAdapter()
    {
      return (ShapeAdapter)base.CreateAdapter();
    }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      ShapeAdapter apt = new ShapeAdapter(this);
      return apt;
    }
  }
}
