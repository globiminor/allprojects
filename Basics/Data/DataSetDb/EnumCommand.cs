namespace Basics.Data.DataSetDb
{
  public sealed class EnumCommand : DbBaseCommand
  {
    public EnumCommand(EnumConnection connection)
      : base(connection)
    { }

    public new EnumConnection Connection
    {
      get { return (EnumConnection)DbConnection; }
      set { DbConnection = value; }
    }

    protected override System.Data.Common.DbConnection DbConnection
    {
      get { return base.Connection; }
      set
      {
        EnumConnection connection = (EnumConnection)value;
        base.DbConnection = connection;
      }
    }

    public new EnumTransaction Transaction
    {
      get { return (EnumTransaction)DbTransaction; }
      set { DbTransaction = value; }
    }
    protected override System.Data.Common.DbTransaction DbTransaction
    {
      get { return base.DbTransaction; }
      set
      {
        EnumTransaction trans = (EnumTransaction)value;
        base.DbTransaction = trans;
      }
    }

    public new EnumReader ExecuteReader()
    { return ExecuteReader(System.Data.CommandBehavior.Default); }
    public new EnumReader ExecuteReader(System.Data.CommandBehavior behavior)
    { return (EnumReader)ExecuteDbDataReader(behavior); }

    protected override System.Data.Common.DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
    {
      EnumReader reader = new EnumReader(this, behavior);
      return reader;
    }

    protected override int ExecuteUpdate(UpdateCommand update)
    { throw new System.NotImplementedException(); }
    protected override int ExecuteInsert(InsertCommand insert)
    { throw new System.NotImplementedException(); }
    protected override int ExecuteDelete(DeleteCommand delete)
    { throw new System.NotImplementedException(); }
  }
}
