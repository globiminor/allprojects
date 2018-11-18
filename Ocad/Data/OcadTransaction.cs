using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace Ocad.Data
{
  public class OcadTransaction : DbTransaction
  {

    private OcadConnection _connection;
    private readonly IsolationLevel _level;
    private string _transFile;
    private Ocad9Writer _writer;
    private OcadCommand.DeleteIndices _deletes;

    public static OcadTransaction Start(OcadConnection connection,
      IsolationLevel isolationLevel)
    {
      OcadTransaction trans = new OcadTransaction(connection, isolationLevel);
      trans._transFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ocd");

      return trans;
    }

    private OcadTransaction(OcadConnection connection,
       IsolationLevel isolationLevel)
    {
      _connection = connection;
      _level = isolationLevel;
    }

    internal Ocad9Writer Writer
    {
      get
      {
        if (_writer == null)
        {
          File.Copy(Connection.ConnectionString, _transFile);
          _writer = Ocad9Writer.AppendTo(_transFile);
        }
        return _writer;
      }
    }

    internal OcadCommand.DeleteIndices Deletes
    {
      get
      {
        if (_deletes == null)
        { _deletes = new OcadCommand.DeleteIndices(); }
        return _deletes;
      }
    }

    public override void Commit()
    {
      if (_deletes != null)
      { Writer.DeleteElements(_deletes.Delete); }

      if (_writer != null)
      {
        _writer.Close();
        File.Copy(_transFile, _connection.ConnectionString, true);
        File.Delete(_transFile);
      }
    }

    public override void Rollback()
    {
      if (_writer != null)
      {
        _writer.Close();
        File.Delete(_transFile);
      }
    }

    public new OcadConnection Connection
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
