
using Basics.Data;

namespace Ocad.Data
{
  public class OcadAdapter : DbBaseAdapter
  {
    private OcadConnection _connection;

    public OcadAdapter(OcadConnection connection)
    {
      _connection = connection;
      base.SelectCommand = _connection.CreateCommand();

      //MissingSchemaAction = MissingSchemaAction.Ignore;
      //AdaptColumnStringLength = true;
    }

    public OcadConnection DbConnection
    {
      get { return _connection; }
    }

    public new OcadCommand SelectCommand
    {
      get { return (OcadCommand)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    public new OcadCommand InsertCommand
    {
      get { return (OcadCommand)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }
    public new OcadCommand UpdateCommand
    {
      get { return (OcadCommand)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }
    public new OcadCommand DeleteCommand
    {
      get { return (OcadCommand)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }
}
