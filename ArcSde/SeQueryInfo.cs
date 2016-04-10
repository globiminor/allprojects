using System;
using System.Collections.Generic;

namespace ArcSde
{
  public class SeQueryInfo : IDisposable
  {
    IntPtr _ptr;

    public SeQueryInfo(IList<string> columns, IList<string> tables, string where)
    {
      Check(CApi.SE_queryinfo_create(ref _ptr));

      Check(CApi.SE_queryinfo_set_tables(_ptr, tables.Count, new List<string>(tables).ToArray(), null));
      Check(CApi.SE_queryinfo_set_columns(_ptr, columns.Count, new List<string>(columns).ToArray()));
      Check(CApi.SE_queryinfo_set_where_clause(_ptr, where));
    }

    ~SeQueryInfo()
    { Dispose(); }
    public void Dispose()
    {
      if (_ptr != IntPtr.Zero)
      { CApi.SE_queryinfo_free(_ptr); }
      _ptr = IntPtr.Zero;
    }

    public IntPtr Ptr
    { get { return _ptr; } }

    private void Check(int rc)
    {
      ErrorHandling.checkRC(IntPtr.Zero, IntPtr.Zero, rc);
    }

  }
}