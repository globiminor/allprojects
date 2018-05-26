using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using Basics.Data;
using Basics.Geom;

namespace Shape.Data
{
  public class AccessConnection : DbBaseConnection
  {
    public event CancelEventHandler StartingOperation;
    public event EventHandler CommittingOperation;
    public event EventHandler AbortingOperation;

    private string _connPath;
    private OleDbConnection _conn;

    public AccessConnection(string connectionString)
    {
      _connPath = connectionString;
      string sConn =
        string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};User Id=admin;Password=;",
        _connPath);
      _conn = new OleDbConnection(sConn);
    }

    internal OleDbConnection OleDbConnection
    {
      get { return _conn; }
    }
    protected override void Dispose(bool disposing)
    {
      if (_conn != null)
      {
        _conn.Dispose();
        _conn = null;
      }
      base.Dispose(disposing);
    }

    public override void Close()
    {
      _conn.Close();
    }
    public override void Open()
    {
      _conn.Open();
    }

    public override DataTable GetSchema(string collectionName)
    {
      if ("columns".Equals(collectionName, StringComparison.InvariantCultureIgnoreCase)
        || "tablesgeom".Equals(collectionName, StringComparison.InvariantCultureIgnoreCase))
      {
        DataTable tblCols = _conn.GetSchema("columns");
        DataTable copy = tblCols.Copy();
        copy.Columns.Remove(SchemaColumnsTable.DataTypeColumn.Name);
        copy.Columns.Add(SchemaColumnsTable.DataTypeColumn.CreateColumn());
        for (int i = 0; i < tblCols.Rows.Count; i++)
        {
          Type t;
          int intType = (int)tblCols.Rows[i][SchemaColumnsTable.DataTypeColumn.Name];
          if (intType == 130)
          { t = typeof(string); }
          else if (intType == 2)
          { t = typeof(bool); }
          else if (intType == 3)
          { t = typeof(int); }
          else if (intType == 5)
          { t = typeof(double); }
          else if (intType == 7)
          { t = typeof(DateTime); }
          else if (intType == 11)
          { t = typeof(bool); }
          else if (intType == 17)
          { t = typeof(object); }
          else if (intType == 72)
          { t = typeof(Guid); }
          else if (intType == 128)
          { t = typeof(IGeometry); }
          else
          { throw new NotImplementedException("unhandled type " + intType); }

          SchemaColumnsTable.DataTypeColumn.SetValue(copy.Rows[i], t);
        }
        copy.AcceptChanges();
        return copy;
      }
      else
      {
        return _conn.GetSchema(collectionName);
      }
    }
    public static readonly string ShapeField = "Shape";
    public override IBox GetExtent(string tableName)
    {
      string sel = string.Format("SELECT {0} FROM {1}", ShapeField, tableName);
      DataTable tbl = new DataTable();
      using (OleDbDataAdapter ada = new OleDbDataAdapter(sel, _conn))
      {
        ada.Fill(tbl);
      }
      Box allBox = null;
      foreach (DataRow row in tbl.Rows)
      {
        byte[] data = (byte[])row[ShapeField];
        Box box = AccessUtils.GetExtent(data);
        if (allBox == null)
        { allBox = box; }
        else
        { allBox.Include(box); }
      }
      return allBox;
    }

    public override string ConnectionString
    {
      get { return _connPath; }
      set { _connPath = value; }
    }

    public override void ChangeDatabase(string databaseName)
    {
      _conn.ChangeDatabase(databaseName);
    }
    public override string Database
    {
      get { return _conn.Database; }
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
      get { return _conn.State; }
    }

    private string GetFullPath(string name)
    {
      string fullPath = System.IO.Path.Combine(_connPath, name);
      return fullPath;
    }
    public override SchemaColumnsTable GetTableSchema(string name)
    {
      bool open = true;
      if (_conn.State != ConnectionState.Open)
      {
        _conn.Open();
        open = false;
      }
      DbCommand cmd = _conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM " + name;
      DbDataReader rawReader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
      DataTable rawSchema = rawReader.GetSchemaTable();
      rawReader.Close();
      if (!open)
      {
        _conn.Close();
      }
      SchemaColumnsTable schema = new SchemaColumnsTable();
      foreach (DataRow row in rawSchema.Rows)
      {
        string columnName = (string)row["ColumnName"];
        Type dataType = (Type)row["DataType"];
        if (dataType == typeof(byte[])) { dataType = typeof(IGeometry); }
        schema.AddSchemaColumn(columnName, dataType);
      }
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
      OleDbTransaction trans = _conn.BeginTransaction();
      return trans;
    }

    protected override DbCommand CreateDbCommand()
    {
      return new AccessCommand(this);
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
      AccessAdapter apt = new AccessAdapter(this);
      return apt;
    }
  }

}
