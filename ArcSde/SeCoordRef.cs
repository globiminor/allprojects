using System;

namespace ArcSde
{
  public class SeCoordRef
  {
    private IntPtr _ptr;

    public SeCoordRef()
    {
      CApi.SE_coordref_create(out _ptr);
    }
    ~SeCoordRef()
    {
      Dispose();
    }
    internal IntPtr Ptr
    {
      get
      { return _ptr; }
    }

    public void Dispose()
    {
      if (_ptr != IntPtr.Zero)
      {
        CApi.SE_coordref_free(_ptr);
        _ptr = IntPtr.Zero;
      }
    }
  }
}
