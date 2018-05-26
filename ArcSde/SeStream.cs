using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ArcSde
{
  public class SeStream : IDisposable
  {
    private SeConnection _conn;
    private IntPtr _ptr;

    public SeStream(SeConnection conn)
    {
      _conn = conn;
      Api.SE_stream_create(conn.Conn, ref _ptr);
    }

    ~SeStream()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (_ptr != IntPtr.Zero)
      {
        CApi.SE_stream_free(_ptr);
      }
      _ptr = IntPtr.Zero;
    }

    public void SetState(int stateId, int differencesId, int differenceType)
    {
      Api.SE_stream_set_state(_ptr, stateId, differencesId, differenceType);
    }

    public void CopyStateRows(string table, int[] recList)
    {
      Api.SE_stream_copy_state_rows(_ptr, table, recList);
    }

    public void DeleteByIdList(string table, int[] recList)
    {
      Check(CApi.SE_stream_delete_by_id_list(_ptr, table, recList, recList.Length));
    }

    public void Select(SeQueryInfo query)
    {
      Check(CApi.SE_stream_query_with_info(_ptr, query.Ptr));
    }

    public void Filter(short searchOrder, bool calcMasks, IList<Se_Filter> filters)
    {
      Se_Filter[] filterArray = new List<Se_Filter>(filters).ToArray();
      Check(CApi.SE_stream_set_spatial_constraints(_ptr, searchOrder, calcMasks, (short)filters.Count, filterArray));
    }

    public void Execute()
    {
      Check(CApi.SE_stream_execute(_ptr));
    }
    private void Check(int rc)
    {
      ErrorHandling.CheckRC(_conn.Conn, _ptr, rc);
    }

    public class FetchEnum : IEnumerator, IDisposable
    {
      private SeStream _stream;
      private IntPtr _ptrOid;
      private IntPtr _ptrOidNull;
      private readonly bool _bind;

      public FetchEnum(SeConnection conn, SeQueryInfo query)
        : this(conn, query, null, false)
      { }

      public FetchEnum(SeConnection conn, SeQueryInfo query, IList<Se_Filter> filters)
        : this(conn, query, filters, false)
      { }

      public FetchEnum(SeConnection conn, SeQueryInfo query, IList<Se_Filter> filters, bool bind)
      {
        _stream = new SeStream(conn);
        _stream.Select(query);
        if (filters != null && filters.Count > 0)
        {
          _stream.Filter(SdeType.SE_SPATIAL_FIRST, false, filters);
        }

        _bind = bind;

        if (_bind)
        {
          _ptrOid = Marshal.AllocHGlobal(sizeof(int));
          _ptrOidNull = Marshal.AllocHGlobal(sizeof(short));
          _stream.Check(CApi.SE_stream_bind_output_column(_stream._ptr, 1, _ptrOid, _ptrOidNull));
        }

        _stream.Execute();
      }

      public void Dispose()
      {
        _stream.Dispose();
        if (_bind)
        {
          Marshal.FreeHGlobal(_ptrOid);
          Marshal.FreeHGlobal(_ptrOidNull);
        }
      }
      public bool MoveNext()
      {
        int state = CApi.SE_stream_fetch(_stream._ptr);
        if (state == SdeErrNo.SE_FINISHED)
        {
          return false;
        }
        _stream.Check(state);
        return true;
      }

      public void Reset()
      {
        throw new InvalidOperationException();
      }

      private static SeShape _queryShape = new SeShape();
      public object GetValue(int index, Type type)
      {
        if (_bind)
        {
          int t = Marshal.ReadInt32(_ptrOid);
          return t;
        }
        else
        {
          short colIdx = (short)(index + 1);
          if (type == typeof(int))
          {
            int rc = CApi.SE_stream_get_integer(_stream._ptr, colIdx, out int i);
            if (rc == SdeErrNo.SE_NULL_VALUE) return DBNull.Value;
            return i;
          }
          else if (type == typeof(Int16))
          {
            int rc = CApi.SE_stream_get_smallint(_stream._ptr, colIdx, out short i);
            if (rc == SdeErrNo.SE_NULL_VALUE) return DBNull.Value;
            _stream.Check(rc);
            return i;
          }
          else if (type == typeof(double))
          {
            int rc = CApi.SE_stream_get_double(_stream._ptr, colIdx, out double d);
            if (rc == SdeErrNo.SE_NULL_VALUE) return DBNull.Value;
            _stream.Check(rc);
            return d;
          }
          else if (type == typeof(DateTime))
          {
            int rc = CApi.SE_stream_get_date(_stream._ptr, colIdx, out Tm d);
            if (rc == SdeErrNo.SE_NULL_VALUE) return DBNull.Value;
            _stream.Check(rc);
            return d.GetDate();
          }
          else if (type == typeof(Guid))
          {
            IntPtr g = Marshal.AllocHGlobal(100);
            int rc = CApi.SE_stream_get_uuid(_stream._ptr, colIdx, g);
            if (rc == SdeErrNo.SE_NULL_VALUE)
            {
              Marshal.FreeHGlobal(g);
              return DBNull.Value;
            }
            string s = Marshal.PtrToStringAnsi(g); //ANSI
            Marshal.FreeHGlobal(g);
            Guid guid = new Guid(s);
            return guid;
          }
          else if (type == typeof(string))
          {
            bool ansi = false;
            if (ansi)
            {
              IntPtr ps = Marshal.AllocHGlobal(2000);
              int rc = CApi.SE_stream_get_string(_stream._ptr, colIdx, ps);
              if (rc == SdeErrNo.SE_NULL_VALUE)
              {
                Marshal.FreeHGlobal(ps);
                return DBNull.Value;
              }
              _stream.Check(rc);
              string s = Marshal.PtrToStringAnsi(ps); //ANSI
              Marshal.FreeHGlobal(ps);
              return s;
            }
            else
            {
              IntPtr ps = Marshal.AllocHGlobal(2000);
              int rc = CApi.SE_stream_get_nstring(_stream._ptr, colIdx, ps);
              if (rc == SdeErrNo.SE_NULL_VALUE)
              {
                Marshal.FreeHGlobal(ps);
                return DBNull.Value;
              }
              _stream.Check(rc);
              string s = Marshal.PtrToStringUni(ps); // UTF16
              Marshal.FreeHGlobal(ps);
              return s;
            }
          }
          else if (type == typeof(SeShape))
          {
            int rc = CApi.SE_stream_get_shape(_stream._ptr, colIdx, _queryShape.Ptr);
            if (rc == SdeErrNo.SE_NULL_VALUE) return DBNull.Value;
            _stream.Check(rc);
            return _queryShape;
          }
          else
          { throw new NotImplementedException("Unhandled type " + type); }
        }
      }

      public object Current
      {
        get { return this; }
      }
    }

  }
}