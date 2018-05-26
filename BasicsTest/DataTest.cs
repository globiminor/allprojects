using Basics.Data;
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace BasicsTest
{
  [TestClass]
  public class DataTest
  {
    [TestMethod]
    public void CanSelect()
    {
      DataSet ds = new DataSet();
      DataTable parent = new DataTable("ParentTbl");
      ds.Tables.Add(parent);
    }
  }

  public class DataSetConnection : DbBaseConnection
  {
    private readonly DataSet _ds;
    public DataSetConnection(DataSet ds)
    {
      _ds = ds;
    }

    internal DataSet DataSet => _ds;
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      return new DataSetTransaction(this, isolationLevel);
    }

    public override string ConnectionString { get; set; }
    public override string Database => "Dataset";
    public override string DataSource => _ds.DataSetName;
    public override string ServerVersion => throw new System.NotImplementedException();
    public override ConnectionState State => ConnectionState.Open;

    public override SchemaColumnsTable GetTableSchema(string name)
    {
      throw new System.NotImplementedException();
    }

    public override IBox GetExtent(string tableName)
    {
      DataTable tbl = _ds.Tables[0];
      DataColumn geomCol = null;
      foreach (DataColumn col in tbl.Columns)
      {
        if (typeof(IGeometry).IsAssignableFrom(col.DataType))
        {
          geomCol = col;
        }
      }
      if (geomCol == null)
      { return null; }

      IBox extent = null;
      foreach (DataRowView vRow in tbl.DefaultView)
      {
        IGeometry geom = vRow.Row[geomCol] as IGeometry;
        if (geom == null)
        { continue; }

        if (extent == null)
        { extent = geom.Extent.Clone(); }
        else
        { extent?.Include(geom.Extent); }
      }
      return extent;
    }

    public override void Open()
    { }
    public override void Close()
    { }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      return new DataSetAdapter();
    }

    public override void ChangeDatabase(string databaseName)
    { throw new System.NotImplementedException(); }

    protected override DbCommand CreateDbCommand()
    {
      return new DataSetCommand(this);
    }
  }

  public class DataSetCommand : DbBaseCommand
  {
    public DataSetCommand(DbBaseConnection connection) : base(connection)
    { }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      return new DataSetReader(this, behavior);
    }

    protected override int ExecuteInsert(InsertCommand insert)
    {
      throw new System.NotImplementedException();
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    {
      throw new System.NotImplementedException();
    }

    protected override int ExecuteDelete(DeleteCommand delete)
    {
      throw new System.NotImplementedException();
    }
  }

  public class DataSetReader : DbBaseReader
  {
    public DataSetReader(DataSetCommand cmd, CommandBehavior behavior) :
      base(cmd, behavior)
    { }

    protected override SchemaColumnsTable GetSchemaTableCore()
    {
      throw new NotImplementedException();
    }
    public override IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }
    public override bool Read()
    {
      throw new NotImplementedException();
    }
    public override bool NextResult()
    {
      throw new NotImplementedException();
    }
    public override object GetValue(int ordinal)
    {
      throw new NotImplementedException();
    }
  }
  public class DataSetAdapter : DbBaseAdapter
  { }

  public class DataSetTransaction : DbTransaction
  {
    private readonly DataSetConnection _connection;
    private readonly IsolationLevel _isolationLevel;

    public DataSetTransaction(DataSetConnection connection, IsolationLevel isolationLevel)
    {
      _connection = connection;
      _isolationLevel = isolationLevel;
    }

    public new DbConnection Connection => _connection;
    protected override DbConnection DbConnection => _connection;
    public override IsolationLevel IsolationLevel => _isolationLevel;

    public override void Commit()
    {
      _connection.DataSet.AcceptChanges();
    }
    public override void Rollback()
    {
      _connection.DataSet.RejectChanges();
    }
  }
}
