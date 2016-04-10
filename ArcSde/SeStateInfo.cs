using System;

namespace ArcSde
{
  public class SeStateInfo : IDisposable
  {
    SeConnection _conn;
    IntPtr _stateInfo;

    public SeStateInfo(SeConnection conn)
    {
      _conn = conn;
      Api.SE_stateinfo_create(ref _stateInfo);
    }

    ~SeStateInfo()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (_stateInfo != IntPtr.Zero)
      {
        CApi.SE_stateinfo_free(_stateInfo);
      }
      _stateInfo = IntPtr.Zero;
    }

    public void LoadInfo(int stateId)
    {
      Api.SE_state_get_info(_conn.Conn, stateId, _stateInfo);
    }

    public DateTime? GetClosingTime()
    {
      DateTime closing = new DateTime();
      bool isNull;
      Api.SE_stateinfo_get_closing_time(_stateInfo, ref closing, out isNull);
      if (isNull)
      {
        return null;
      }
      else
      {
        return closing;
      }

    }

    public SeStateInfo Create(int baseVersion)
    {
      SeStateInfo created = new SeStateInfo(_conn);
      Api.SE_state_create(_conn.Conn, _stateInfo, baseVersion, created._stateInfo);
      return created;
    }

    public int GetId()
    {
      int stateId = 0;
      Api.SE_stateinfo_get_id(_stateInfo, ref stateId);
      return stateId;
    }
  }
}