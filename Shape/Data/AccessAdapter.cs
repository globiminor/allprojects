using Basics.Data;

namespace Shape.Data
{
  [System.ComponentModel.ToolboxItem(false)]
  public class AccessAdapter : DbBaseAdapter
  {
    private AccessConnection _connection;

    public AccessAdapter(AccessConnection connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public AccessConnection DbConnection
    {
      get { return _connection; }
    }

    public new ShapeCommand SelectCommand
    {
      get { return (ShapeCommand)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    public new ShapeCommand InsertCommand
    {
      get { return (ShapeCommand)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }
    public new ShapeCommand UpdateCommand
    {
      get { return (ShapeCommand)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }
    public new ShapeCommand DeleteCommand
    {
      get { return (ShapeCommand)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }

}
