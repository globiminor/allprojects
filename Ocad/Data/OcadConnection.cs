using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Basics.Data;
using Basics.Geom;

namespace Ocad.Data
{
  public class OcadConnection : DbBaseConnection
  {
    public event CancelEventHandler StartingOperation;
    public event EventHandler CommittingOperation;
    public event EventHandler AbortingOperation;

    private OcadTransaction _trans;
    private string _connection;
    private Pool _pool;

    public static readonly string TableElements = "Elements";

    public static readonly string FieldId = "Id";
    public static readonly TypedColumn<IGeometry> FieldShape = new TypedColumn<IGeometry>("Shape");
    public static readonly string FieldSymbol = "Symbol";
    public static readonly string FieldAngle = "Angle";
    public static readonly string FieldHeight = "Height";

    private static Dictionary<string, Pool> _poolDict;

    internal class Pool
    {
      private string _connectionString;
      public Pool(string connectionString)
      {
        _connectionString = connectionString;
      }
      public List<OcadConnection> Connections = new List<OcadConnection>();
      public Dictionary<string, OcadElemsInfo> Infos = new Dictionary<string, OcadElemsInfo>();

      public OcadElemsInfo GetInfo(string name)
      {
        OcadElemsInfo info;
        if (Infos.TryGetValue(name, out info) == false)
        {
          string fullName = _connectionString;
          info = new OcadElemsInfo(fullName);
          Infos.Add(name, info);
        }
        return info;
      }
    }

    public OcadConnection(string connectionString)
    {
      _connection = connectionString;
      if (_poolDict == null)
      { _poolDict = new Dictionary<string, Pool>(); }

      Pool pool;
      if (_poolDict.TryGetValue(_connection, out pool) == false)
      {
        pool = new Pool(_connection);
        _poolDict.Add(_connection, pool);
      }
      pool.Connections.Add(this);
      _pool = pool;
    }

    public override void Close()
    { }
    public override void Open()
    { }

    protected override void Dispose(bool disposing)
    {
      _pool.Connections.Remove(this);
      base.Dispose(disposing);
    }

    public override string ConnectionString
    {
      get { return _connection; }
      set { _connection = value; }
    }

    internal OcadElemsInfo GetReader(string name)
    {
      OcadElemsInfo ocad = _pool.GetInfo(name);
      return ocad;
    }
    public override IBox GetExtent(string tableName)
    {
      OcadElemsInfo info = _pool.GetInfo(tableName);
      IBox extent = info.GetExtent();
      return extent;
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
      SchemaColumnsTable elements = GetTableSchema(collectionName);
      foreach (SchemaColumnsTable.Row row in elements.Rows)
      { row.TableName = "Elements"; }

      elements.AcceptChanges();

      // TODO: weitere Typen

      return elements;
    }

    public override SchemaColumnsTable GetTableSchema(string tableName)
    {
      // TODO: korrekt implementieren
      SchemaColumnsTable schema = new SchemaColumnsTable();

      SchemaColumnsTable.Row idRow = schema.AddSchemaColumn(FieldId, typeof(int));
      schema.AddSchemaColumn(FieldShape.Name, FieldShape.DataType);
      schema.AddSchemaColumn(FieldSymbol, typeof(int));
      schema.AddSchemaColumn(FieldAngle, typeof(double));
      schema.AddSchemaColumn(FieldHeight, typeof(double));

      idRow.IsKey = true;

      return schema;
    }

    public new OcadTransaction BeginTransaction()
    {
      return (OcadTransaction)base.BeginTransaction();
    }
    public new OcadTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
      return (OcadTransaction)base.BeginTransaction(isolationLevel);
    }
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      _trans = OcadTransaction.Start(this, isolationLevel);
      return _trans;
    }

    public new OcadCommand CreateCommand()
    {
      return (OcadCommand)CreateDbCommand();
    }
    protected override DbCommand CreateDbCommand()
    {
      return new OcadCommand(this);
    }

    internal void OnStartingOperation(CancelEventArgs args)
    {
      if (StartingOperation != null)
      { StartingOperation(this, args); }
    }

    internal void OnCommittingOperation(EventArgs args)
    {
      if (CommittingOperation != null)
      { CommittingOperation(this, args); }
    }

    internal void OnAbortingOperation(EventArgs args)
    {
      if (AbortingOperation != null)
      { AbortingOperation(this, args); }
    }

    public new OcadAdapter CreateAdapter()
    {
      return (OcadAdapter)base.CreateAdapter();
    }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      OcadAdapter apt = new OcadAdapter(this);
      return apt;
    }

  }
}
