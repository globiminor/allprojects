using System;
using System.Data;
using System.Data.Common;
using Basics.Data;

namespace Shape.Data
{
  [System.ComponentModel.ToolboxItem(false)]
  public sealed class AccessCommand : DbBaseCommand
  {
    public AccessCommand(AccessConnection connection)
      : base(connection)
    {
    }

    public new AccessConnection Connection
    {
      get { return (AccessConnection)DbConnection; }
      set { DbConnection = value; }
    }

    protected override DbConnection DbConnection
    {
      get { return base.Connection; }
      set
      {
        AccessConnection connection = (AccessConnection)value;
        base.DbConnection = connection;
      }
    }

    public new AccessDbReader ExecuteReader()
    { return ExecuteReader(CommandBehavior.Default); }
    public new AccessDbReader ExecuteReader(CommandBehavior behavior)
    { return (AccessDbReader)ExecuteDbDataReader(behavior); }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      AccessDbReader reader = new AccessDbReader(this, behavior);
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
