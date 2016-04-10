using System;

namespace ArcSde
{
  public class SeVersionInfo : IDisposable
  {
    IntPtr _versionInfo;
    SeConnection _conn;

    public SeVersionInfo(SeConnection conn)
    {
      _conn = conn;
      Api.SE_versioninfo_create(ref _versionInfo);
    }

    public void LoadInfo(string version)
    {
      Api.SE_version_get_info(_conn.Conn, version, _versionInfo);
    }

    ~SeVersionInfo()
    {
      Dispose();
    }
    public void Dispose()
    {
      if (_versionInfo != IntPtr.Zero)
      {
        CApi.SE_versioninfo_free(_versionInfo);
      }
      _versionInfo = IntPtr.Zero;
    }

    public int GetStateId()
    {
      int stateId = 0;
      Api.SE_versioninfo_get_state_id(_versionInfo, ref stateId);

      return stateId;
    }

    public void ChangeState(int stateId)
    {
      Api.SE_version_change_state(_conn.Conn, _versionInfo, stateId);
    }
  }
}