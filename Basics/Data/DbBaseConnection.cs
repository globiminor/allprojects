using System.Data;
using System.Data.Common;
using Basics.Geom;

namespace Basics.Data
{
  public interface ISpatialDbConnection : IDbConnection
  {
    IBox GetExtent(string tableName);
  }
  public abstract class DbBaseConnection : DbConnection, ISpatialDbConnection
  {
    public abstract IBox GetExtent(string tableName);
    public abstract SchemaColumnsTable GetTableSchema(string name);

    public DbBaseAdapter CreateAdapter()
    {
      return CreateDbAdapter();
    }
    protected abstract DbBaseAdapter CreateDbAdapter();

    public new DbBaseCommand CreateCommand()
    {
      return (DbBaseCommand)base.CreateCommand();
    }
  }
}
