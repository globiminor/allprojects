using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using Basics.Data;
using Basics.Geom;

namespace TData
{
  class TTableConnection : DbBaseConnection
  {
    private readonly TTable _table;
    private DbBaseConnection _conn;

    public TTableConnection(TTable table, DbBaseConnection connection)
    {
      _table = table;
      _conn = connection;
    }

    public TTable Table
    {
      get { return _table; }
    }

    public DbBaseConnection BaseConnection
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
      return _conn.GetSchema(collectionName);
    }
    public override IBox GetExtent(string tableName)
    {
      return _conn.GetExtent(tableName);
    }

    public override string ConnectionString
    {
      get { return _conn.ConnectionString; }
      set { _conn.ConnectionString = value; }
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

    public override SchemaColumnsTable GetTableSchema(string name)
    {
      return _conn.GetTableSchema(name);
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      DbTransaction trans = _conn.BeginTransaction();
      return trans;
    }

    protected override DbCommand CreateDbCommand()
    {
      return new TTableCommand(this);
    }

    public new TTableAdapter CreateAdapter()
    {
      return (TTableAdapter)base.CreateAdapter();
    }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      TTableAdapter apt = new TTableAdapter(this);
      return apt;
    }
  }

  internal sealed class TTableCommand : DbBaseCommand
  {
    private readonly DbBaseCommand _baseCmd;

    public TTableCommand(TTableConnection connection)
      : base(connection)
    {
      _baseCmd = Connection.BaseConnection.CreateCommand();
    }

    public override string CommandText
    {
      get { return _baseCmd.CommandText; }
      set { _baseCmd.CommandText = value; }
    }
    public new TTableConnection Connection
    {
      get { return (TTableConnection)DbConnection; }
      set { DbConnection = value; }
    }

    protected override DbConnection DbConnection
    {
      get { return base.DbConnection; }
      set
      {
        TTableConnection connection = (TTableConnection)value;
        base.DbConnection = connection;
      }
    }

    protected override DbParameter CreateDbParameter()
    {
      return _baseCmd.CreateParameter();
    }
    protected override DbParameterCollection DbParameterCollection
    {
      get { return _baseCmd.Parameters; }
    }

    public override int ExecuteNonQuery()
    {
      int n = _baseCmd.ExecuteNonQuery();
      return n;
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      TTableDbReader reader = new TTableDbReader(this, _baseCmd, behavior);
      return reader;
    }

    protected override DbTransaction DbTransaction
    {
      get { return _baseCmd.Transaction; }
      set { _baseCmd.Transaction = value; }
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    { throw new NotImplementedException(); }
    protected override int ExecuteInsert(InsertCommand insert)
    { throw new NotImplementedException(); }
    protected override int ExecuteDelete(DeleteCommand delete)
    { throw new NotImplementedException(); }
  }

  internal class TTableDbReader : DbBaseReader
  {
    #region nested classes

    private class ElemsEnumerator : IEnumerator, IDisposable
    {
      private readonly TTableDbReader _reader;
      private DbDataReader _baseReader;
      private DataView _vwEdits;
      private int _keyOrdinal;
      private IEnumerator _editsEnum;

      public ElemsEnumerator(TTableDbReader reader)
      {
        _reader = reader;
        _baseReader = _reader._baseReader;

        DataView vwEdits = _reader._command.Connection.Table.Edits;
        if (vwEdits != null)
        {
          DataTable tblEdits = vwEdits.Table;
          _vwEdits = new DataView(tblEdits);
          _vwEdits.Sort = tblEdits.PrimaryKey[0].ColumnName;
          _keyOrdinal = -1;

          _editsEnum = _vwEdits.GetEnumerator();
        }
      }

      private int KeyOrdinal
      {
        get
        {
          if (_keyOrdinal < 0)
          { _keyOrdinal = _reader.GetOrdinal(_vwEdits.Table.PrimaryKey[0].ColumnName); }
          return _keyOrdinal;
        }
      }
      public void Dispose()
      {
        if (_baseReader != null)
        {
          _baseReader.Dispose();
          _baseReader = null;
        }
        IDisposable transDisp = _editsEnum as IDisposable;
        if (transDisp != null)
        { transDisp.Dispose(); }

        if (_vwEdits != null)
        { _vwEdits.Dispose(); }
      }

      public void Reset()
      { }
      public object Current
      {
        get { return _reader; }
      }
      public bool MoveNext()
      {
        while (_baseReader != null)
        {
          bool success = _baseReader.Read();
          if (!success)
          {
            _baseReader.Dispose();
            _baseReader = null;
            continue;
          }
          if (_vwEdits != null)
          {
            object key = GetValue(KeyOrdinal);
            if (_vwEdits.Find(key) >= 0)
            { continue; }
          }

          bool conditions = _reader._command.Validate(_reader);
          if (conditions)
          { return true; }
        }

        while (_editsEnum != null)
        {
          bool success = _editsEnum.MoveNext();
          if (!success)
          {
            _editsEnum = null;
            continue;
          }

          DataRowView vRow = (DataRowView)_editsEnum.Current;
          if (vRow.Row.RowState == DataRowState.Unchanged) // implies Deleted
          { continue; }

          bool conditions = _reader._command.Validate(_reader);
          if (conditions)
          { return true; }
        }

        return false;
      }
      public object GetValue(int ordinal)
      {
        object value;
        if (_baseReader != null)
        {
          value = _baseReader.GetValue(ordinal);
        }
        else if (_editsEnum != null)
        {
          DataRowView vRow = (DataRowView)_editsEnum.Current;
          string name = _reader.GetName(ordinal);
          value = vRow[name];
        }
        else
        { value = null; }
        return value;
      }
    }

    #endregion

    private TTableCommand _command;

    private DbBaseCommand _baseCmd;
    private DbDataReader _baseReader;

    private ElemsEnumerator _enum;

    public TTableDbReader(TTableCommand command, DbBaseCommand baseCmd, CommandBehavior behavior)
      : base(command, behavior)
    {
      _command = command;
      _baseCmd = baseCmd;
      _baseReader = _baseCmd.ExecuteReader(behavior);
    }

    protected override void Dispose(bool disposing)
    {
      if (_baseReader != null)
      {
        _baseReader.Dispose();
        _baseReader = null;
      }
      if (_baseCmd != null)
      {
        _baseCmd.Dispose();
        _baseCmd = null;
      }
      base.Dispose(disposing);
    }

    public override IEnumerator GetEnumerator()
    {
      IEnumerator e = GetEnumeratorCore();
      return e;
    }
    private ElemsEnumerator GetEnumeratorCore()
    {
      ElemsEnumerator e = new ElemsEnumerator(this);
      return e;
    }



    public override object GetValue(int ordinal)
    {
      return _enum.GetValue(ordinal);
    }

    public override bool Read()
    {
      if (_enum == null)
      { _enum = GetEnumeratorCore(); }

      bool next = _enum.MoveNext();
      return next;
    }

    public override bool NextResult()
    {
      return true;
    }

    public override int RecordsAffected
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    protected override SchemaColumnsTable GetSchemaTableCore()
    {
      return _command.GetSchemaTable();
    }
  }
  internal class TTableAdapter : DbBaseAdapter
  {
    private TTableConnection _connection;

    public TTableAdapter(TTableConnection connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public TTableConnection DbConnection
    {
      get { return _connection; }
    }

    public new TTableCommand SelectCommand
    {
      get { return (TTableCommand)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    public new TTableCommand InsertCommand
    {
      get { return (TTableCommand)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }
    public new TTableCommand UpdateCommand
    {
      get { return (TTableCommand)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }
    public new TTableCommand DeleteCommand
    {
      get { return (TTableCommand)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }

}
