namespace Basics.Data.DataSetDb
{
  public class EnumTransaction : System.Data.Common.DbTransaction
  {
    private EnumConnection _conn;
    private System.Data.IsolationLevel _level;

    public static EnumTransaction Start(EnumConnection conn,
      System.Data.IsolationLevel isolationLevel)
    {
      EnumTransaction trans = new EnumTransaction();
      trans._conn = conn;
      trans._level = isolationLevel;
      return trans;
    }
    public override void Commit()
    { }

    public override void Rollback()
    { }

    protected override System.Data.Common.DbConnection DbConnection
    {
      get { return _conn; }
    }

    public override System.Data.IsolationLevel IsolationLevel
    {
      get { return _level; }
    }
  }
}
