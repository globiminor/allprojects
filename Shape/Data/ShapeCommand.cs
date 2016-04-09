
using System;
using System.Data;
using System.Data.Common;
using Basics.Data;

namespace Shape.Data
{
  public sealed class ShapeCommand : DbBaseCommand
  {
    public ShapeCommand(ShapeConnection connection)
      : base(connection)
    { }

    public new ShapeConnection Connection
    {
      get { return (ShapeConnection)DbConnection; }
      set { DbConnection = value; }
    }

    protected override DbConnection DbConnection
    {
      get { return base.Connection; }
      set
      {
        ShapeConnection connection = (ShapeConnection)value;
        base.DbConnection = connection;
      }
    }

    public new ShapeTransaction Transaction
    {
      get { return (ShapeTransaction)DbTransaction; }
      set { DbTransaction = value; }
    }
    protected override DbTransaction DbTransaction
    {
      get { return base.DbTransaction; }
      set
      {
        ShapeTransaction trans = (ShapeTransaction)value;
        base.DbTransaction = trans;
      }
    }

    public new ShapeDbReader ExecuteReader()
    { return ExecuteReader(CommandBehavior.Default); }
    public new ShapeDbReader ExecuteReader(CommandBehavior behavior)
    { return (ShapeDbReader)ExecuteDbDataReader(behavior); }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      ShapeDbReader reader = new ShapeDbReader(this, behavior);
      return reader;
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    { throw new NotImplementedException(); }
    protected override int ExecuteInsert(InsertCommand insert)
    { throw new NotImplementedException(); }
    protected override int ExecuteDelete(DeleteCommand delete)
    { throw new NotImplementedException(); }

    public string GetFullName()
    {
      string fullName = System.IO.Path.Combine(Connection.ConnectionString, TableName);
      return fullName;
    }
  }
}
