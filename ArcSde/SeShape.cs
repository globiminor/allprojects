using System;

namespace ArcSde
{
  public class SeShape : IDisposable
  {
    private IntPtr _coordPtr;
    IntPtr _ptr;

    public SeShape()
    {
      CApi.SE_coordref_create(out _coordPtr);
      CApi.SE_shape_create(_coordPtr, out _ptr);
    }

    public SeShape(SeCoordRef coord)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero, CApi.SE_coordref_create(out _coordPtr));
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero, CApi.SE_coordref_duplicate(coord.Ptr, _coordPtr));
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero, CApi.SE_shape_create(_coordPtr, out _ptr));
    }


    public IntPtr Ptr
    {
      get { return _ptr; }
    }
    ~SeShape()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (_coordPtr != IntPtr.Zero)
      {
        CApi.SE_coordref_free(_coordPtr);
        _coordPtr = IntPtr.Zero;
      }
      if (_ptr != IntPtr.Zero)
      {
        CApi.SE_shape_free(_ptr);
        _ptr = IntPtr.Zero;
      }
    }
  }
}
