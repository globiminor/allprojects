using System;
using System.Data;
using System.Data.Common;

namespace Shape.Data
{
  public class ShapeTransaction : DbTransaction
  {

    private ShapeConnection _connection;
    private IsolationLevel _level;

    private ShapeTransaction(ShapeConnection connection,
       IsolationLevel isolationLevel)
    {
      _connection = connection;
      _level = isolationLevel;

    }

    public static ShapeTransaction Start(ShapeConnection connection,
      IsolationLevel isolationLevel)
    {
      ShapeTransaction trans = new ShapeTransaction(connection, isolationLevel);

      return trans;
    }

    public override void Commit()
    {
      throw new NotImplementedException();
    }

    public override void Rollback()
    {
      throw new NotImplementedException();
    }

    public new ShapeConnection Connection
    {
      get { return _connection; }
    }
    protected override DbConnection DbConnection
    { get { return Connection; } }

    public override IsolationLevel IsolationLevel
    {
      get { return _level; }
    }
  }
}
