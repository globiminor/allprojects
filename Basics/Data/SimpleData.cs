using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Basics.Data
{
  public class SimpleDbReader<T> : DbBaseReader
  {
    #region nested classes
    private class Enumerator : IEnumerator<T>
    {
      private readonly SimpleDbReader<T> _reader;
      private IEnumerator<T> _enumerator;
      private T _currentElem;
      Geom.IGeometry _intersect = null;

      public Enumerator(SimpleDbReader<T> reader, ISimpleData<T> data)
      {
        _reader = reader;
        Geom.IBox extent = null;

        foreach (var parameter in reader._command.Parameters.Enum())
        {
          if (parameter.Value is Geom.IGeometry)
          {
            _intersect = (Geom.IGeometry)parameter.Value;
            extent = _intersect.Extent;
          }
        }

        _enumerator = data.GetEnumerator(extent);
      }

      #region IEnumerator Members

      public void Dispose()
      {
        _enumerator.Dispose();
      }

      public void Reset()
      {
        _enumerator.Reset();
        _currentElem = default(T);
      }

      object System.Collections.IEnumerator.Current
      { get { return Current; } }
      public T Current
      {
        get { return _currentElem; }
      }

      public bool MoveNext()
      {
        bool value = _enumerator.MoveNext();
        bool conditions = false;
        _currentElem = default(T);
        while (value && conditions == false)
        {
          _currentElem = _enumerator.Current;

          conditions = _reader._command.Validate(_reader);

          if (conditions == false)
          { value = _enumerator.MoveNext(); }
        }
        return value;
      }

      #endregion
    }
    #endregion

    private SimpleCommand<T> _command;


    private Enumerator _enum;
    private int _nRows;

    private ISimpleData<T> _data;

    public SimpleDbReader(SimpleCommand<T> command, CommandBehavior behavior)
      : base(command, behavior)
    {
      _command = command;
    }

    protected override void Dispose(bool disposing)
    {
      if (_enum != null)
      {
        _enum.Dispose();
        _enum = null;
      }
      base.Dispose(disposing);
    }

    public override System.Collections.IEnumerator GetEnumerator()
    {
      if (_enum != null)
      { _enum.Dispose(); }

      _enum = new Enumerator(this, Data);
      _nRows = 0;
      return _enum;
    }

    public override object GetValue(int ordinal)
    {
      string name = GetName(ordinal);
      return Data.GetValue(_enum.Current, name);
    }

    private ISimpleData<T> Data
    {
      get
      {
        if (_data == null)
        {
          _data = _command.Connection.GetReader();
        }
        return _data;
      }
    }

    public override bool Read()
    {
      if (_enum == null)
      { GetEnumerator(); }

      bool next = _enum.MoveNext();
      if (next == false)
      { }
      else
      { _nRows++; }
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
      SchemaColumnsTable schema = _command.GetSchemaTable();
      return schema;
    }
  }

  public sealed class SimpleCommand<T> : DbBaseCommand
  {
    public SimpleCommand(SimpleConnection<T> connection)
      : base(connection)
    { }

    public new SimpleConnection<T> Connection
    {
      get { return (SimpleConnection<T>)DbConnection; }
      set { DbConnection = value; }
    }

    protected override DbConnection DbConnection
    {
      get { return base.Connection; }
      set
      {
        SimpleConnection<T> connection = (SimpleConnection<T>)value;
        base.DbConnection = connection;
      }
    }

    public new SimpleTransaction<T> Transaction
    {
      get { return (SimpleTransaction<T>)DbTransaction; }
      set { DbTransaction = value; }
    }
    protected override DbTransaction DbTransaction
    {
      get { return base.DbTransaction; }
      set
      {
        SimpleTransaction<T> trans = (SimpleTransaction<T>)value;
        base.DbTransaction = trans;
      }
    }

    public new SimpleDbReader<T> ExecuteReader()
    { return ExecuteReader(CommandBehavior.Default); }
    public new SimpleDbReader<T> ExecuteReader(CommandBehavior behavior)
    { return (SimpleDbReader<T>)ExecuteDbDataReader(behavior); }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      SimpleDbReader<T> reader = new SimpleDbReader<T>(this, behavior);
      return reader;
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    { throw new NotImplementedException(); }
    protected override int ExecuteInsert(InsertCommand insert)
    { throw new NotImplementedException(); }
    protected override int ExecuteDelete(DeleteCommand delete)
    { throw new NotImplementedException(); }
  }

  public interface ISimpleData<T>
  {
    Geom.IBox GetExtent();
    IEnumerator<T> GetEnumerator(Geom.IBox geom);
    object GetValue(T element, string fieldName);
    SchemaColumnsTable GetTableSchema();
  }
  public class SimpleConnection<T> : DbBaseConnection
  {
    public event CancelEventHandler StartingOperation;
    public event EventHandler CommittingOperation;
    public event EventHandler AbortingOperation;

    private SimpleTransaction<T> _trans;
    private ISimpleData<T> _data;

    public SimpleConnection(ISimpleData<T> data)
    {
      _data = data;
    }

    public ISimpleData<T> Data
    {
      get { return _data; }
      set { _data = value; }
    }
    public override void Close()
    { }
    public override void Open()
    { }

    public override string ConnectionString
    {
      get { return null; }
      set { }
    }

    internal ISimpleData<T> GetReader()
    {
      return _data;
    }
    public override Geom.IBox GetExtent(string tableName)
    {
      Geom.IBox extent = _data.GetExtent();
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

    public override SchemaColumnsTable GetTableSchema(string tableName)
    {
      return Data.GetTableSchema();
    }

    public new SimpleTransaction<T> BeginTransaction()
    {
      return (SimpleTransaction<T>)base.BeginTransaction();
    }
    public new SimpleTransaction<T> BeginTransaction(IsolationLevel isolationLevel)
    {
      return (SimpleTransaction<T>)base.BeginTransaction(isolationLevel);
    }
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      _trans = SimpleTransaction<T>.Start(this, isolationLevel);
      return _trans;
    }

    public new SimpleCommand<T> CreateCommand()
    {
      return (SimpleCommand<T>)CreateDbCommand();
    }
    protected override DbCommand CreateDbCommand()
    {
      return new SimpleCommand<T>(this);
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

    public new SimpleAdapter<T> CreateAdapter()
    {
      return (SimpleAdapter<T>)base.CreateAdapter();
    }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      SimpleAdapter<T> apt = new SimpleAdapter<T>(this);
      return apt;
    }

  }

  public class SimpleTransaction<T> : DbTransaction
  {

    private readonly SimpleConnection<T> _connection;
    private readonly IsolationLevel _level;

    private SimpleTransaction(SimpleConnection<T> connection,
       IsolationLevel isolationLevel)
    {
      _connection = connection;
      _level = isolationLevel;

    }

    public static SimpleTransaction<T> Start(SimpleConnection<T> connection,
      IsolationLevel isolationLevel)
    {
      SimpleTransaction<T> trans = new SimpleTransaction<T>(connection, isolationLevel);

      return trans;
    }

    public override void Commit()
    {
      throw new NotImplementedException();
    }

    public override void Rollback()
    {
      throw new NotImplementedException();
    }

    public new SimpleConnection<T> Connection
    {
      get { return _connection; }
    }
    protected override DbConnection DbConnection
    { get { return Connection; } }

    public override IsolationLevel IsolationLevel
    {
      get { return _level; }
    }
  }

  public class SimpleAdapter<T> : DbBaseAdapter
  {
    private SimpleConnection<T> _connection;

    public SimpleAdapter(SimpleConnection<T> connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public SimpleConnection<T> DbConnection
    {
      get { return _connection; }
    }

    public new SimpleCommand<T> SelectCommand
    {
      get { return (SimpleCommand<T>)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    public new SimpleCommand<T> InsertCommand
    {
      get { return (SimpleCommand<T>)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }
    public new SimpleCommand<T> UpdateCommand
    {
      get { return (SimpleCommand<T>)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }
    public new SimpleCommand<T> DeleteCommand
    {
      get { return (SimpleCommand<T>)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }
}
