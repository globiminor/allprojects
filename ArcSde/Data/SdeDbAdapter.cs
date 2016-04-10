using Basics.Data;

namespace ArcSde.Data
{
  public class SdeDbAdapter : DbBaseAdapter
  {
    private SdeDbConnection _connection;

    public SdeDbAdapter(SdeDbConnection connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public SdeDbConnection DbConnection
    {
      get { return _connection; }
    }

    public new SdeDbCommand SelectCommand
    {
      get { return (SdeDbCommand)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    public new SdeDbCommand InsertCommand
    {
      get { return (SdeDbCommand)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }
    public new SdeDbCommand UpdateCommand
    {
      get { return (SdeDbCommand)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }
    public new SdeDbCommand DeleteCommand
    {
      get { return (SdeDbCommand)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }
}
