namespace Basics.Data.DataSetDb
{
  [System.ComponentModel.ToolboxItem(false)]
  public class EnumAdapter : DbBaseAdapter
  {
    private EnumConnection _connection;

    public EnumAdapter(EnumConnection connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public EnumConnection DbConnection
    {
      get { return _connection; }
    }

    public new EnumCommand SelectCommand
    {
      get { return (EnumCommand)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    public new EnumCommand InsertCommand
    {
      get { return (EnumCommand)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }
    public new EnumCommand UpdateCommand
    {
      get { return (EnumCommand)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }
    public new EnumCommand DeleteCommand
    {
      get { return (EnumCommand)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }

}
