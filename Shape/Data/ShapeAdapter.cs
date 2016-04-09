using Basics.Data;

namespace Shape.Data
{
  public class ShapeAdapter : DbBaseAdapter
  {
    private ShapeConnection _connection;

    public ShapeAdapter(ShapeConnection connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public ShapeConnection DbConnection
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
