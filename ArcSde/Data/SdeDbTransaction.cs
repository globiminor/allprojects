using System;
using System.Data;
using System.Data.Common;

namespace ArcSde.Data
{
  public class SdeDbTransaction : DbTransaction
  {

    private SdeDbConnection _connection;
    private IsolationLevel _level;

    private SdeDbTransaction(SdeDbConnection connection,
       IsolationLevel isolationLevel)
    {
      _connection = connection;
      _level = isolationLevel;

    }

    public static SdeDbTransaction Start(SdeDbConnection connection,
      IsolationLevel isolationLevel)
    {
      SdeDbTransaction trans = new SdeDbTransaction(connection, isolationLevel);

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

    public new SdeDbConnection Connection
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
