using System;
using System.Data;

namespace TData
{
  public class OpenConnection : IDisposable
  {
    private readonly IDbConnection _conn;
    private readonly bool _close;

    public OpenConnection(IDbConnection conn)
    {
      _conn = conn;
      if (_conn.State == ConnectionState.Closed)
      {
        _conn.Open();
        _close = true;
      }
    }
    public void Dispose()
    {
      if (_close)
      { _conn.Close(); }
    }
  }
}