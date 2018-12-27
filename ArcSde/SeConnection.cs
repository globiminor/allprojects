using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ArcSde
{
  public class SeConnection : IDisposable
  {
    IntPtr _conn;
    internal IntPtr Conn { get { return _conn; } }

    public SeConnection(string connectionString)
    {
      string server = null;
      string instance = null;
      string database = null;
      string user = null;
      string password = null;
      string[] parts = connectionString.Split(';');
      foreach (var part in parts)
      {
        int ePos = part.IndexOf('=');
        if (ePos < 0) continue;
        string key = part.Substring(0, ePos).Trim().ToUpper();
        string value = part.Substring(ePos + 1);
        if (key == "SERVER")
        { server = value; }
        else if (key == "INSTANCE")
        { instance = value; }
        else if (key == "DATABASE")
        { database = value; }
        else if (key == "USER")
        { user = value; }
        else if (key == "PASSWORD")
        { password = value; }
      }
      Se_Error error = new Se_Error();
      Api.SE_connection_create(server, instance, database, user,
        password, ref error, ref _conn);
    }

    public SeConnection(string server, string instance, string databaseName,
       string user, string password)
    {
      Se_Error error = new Se_Error();
      Api.SE_connection_create(server, instance, databaseName, user,
        password, ref error, ref _conn);
    }
    ~SeConnection()
    {
      Dispose();
    }
    public void Dispose()
    {
      if (_conn != IntPtr.Zero)
      {
        CApi.SE_connection_free(_conn);
      }
      _conn = IntPtr.Zero;
    }

    public SeVersionInfo GetVersionInfo(string version)
    {
      SeVersionInfo vers = new SeVersionInfo(this);
      vers.LoadInfo(version);

      return vers;
    }

    public SeStateInfo GetStateInfo(int stateId)
    {
      SeStateInfo state = new SeStateInfo(this);
      state.LoadInfo(stateId);

      return state;
    }

    public void CloseState(int stateId)
    {
      Api.SE_state_close(_conn, stateId);
    }

    public void StartTransaction()
    {
      Api.SE_connection_start_transaction(_conn);
    }

    public void OpenState(int stateId)
    {
      Api.SE_state_open(_conn, stateId);
    }

    public void CommitTransaction()
    {
      Api.SE_connection_commit_transaction(_conn);
    }

    public void RollbackTransaction()
    {
      Api.SE_connection_rollback_transaction(_conn);
    }

    public IList<string> GetTables(int privilege)
    {
      string[] tables;

      ErrorHandling.CheckRC(Conn, IntPtr.Zero,
        CApi.SE_table_list(Conn, privilege, out int numTables, out IntPtr names));

      IntPtr[] ptrArray = new IntPtr[numTables];
      Marshal.Copy(names, ptrArray, 0, numTables);
      tables = new string[numTables];
      for (int i = 0; i < numTables; i++)
      {
        tables[i] = Marshal.PtrToStringAnsi(ptrArray[i]);
      }

      CApi.SE_table_free_list(numTables, names);

      return tables;
    }

    public void Check(int rc)
    {
      ErrorHandling.CheckRC(Conn, IntPtr.Zero, rc);
    }
    public SeLayerInfoList GetLayers()
    {
      Check(CApi.SE_layer_get_info_list(Conn, out IntPtr layers, out int numLayers));

      SeLayerInfoList layerList = new SeLayerInfoList(numLayers, layers);
      return layerList;
    }
  }
}
