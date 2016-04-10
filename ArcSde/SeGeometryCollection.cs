using System;
using System.Collections;

namespace ArcSde
{
  internal class SeGeometryCollection : IEnumerable
  {
    private class MyEnumerator : IEnumerator
    {
      SeGeometryCollection _list;
      int _pos = 0;
      SeGeometry _geom;
      public MyEnumerator(SeGeometryCollection geometryList)
      {
        _list = geometryList;
      }
      #region IEnumerator Members

      public void Reset()
      {
        _pos = 0;
      }

      public object Current
      {
        get
        { return _geom; }
      }

      public bool MoveNext()
      {
        _geom = null;
        if (_list == null || _list._shapeArray == null || _list._shapeArray.Length <= _pos)
        { return false; }

        _geom = SeGeometry.Create(_list._shapeArray[_pos], false);

        _pos++;
        return true;
      }

      #endregion
    }


    private IntPtr[] _shapeArray;
    private bool _free;

    internal SeGeometryCollection(IntPtr[] geometryList)
    {
      _shapeArray = geometryList;
      _free = true;
    }
    public SeGeometryCollection()
    {
    }

    ~SeGeometryCollection()
    {
      if (_shapeArray != null && _free)
      { CApi.SE_shape_free_array(_shapeArray.Length, _shapeArray); }
    }

    public int Count
    {
      get
      {
        if (_shapeArray != null)
        { return _shapeArray.Length; }
        return 0;
      }
    }
    public SeGeometry this[int index]
    {
      get
      { return SeGeometry.Create(_shapeArray[index], false); }
    }
    public void AddRange(SeGeometry[] geometryList)
    {
      _free = false;

      int iStart = 0;
      int iNNew = geometryList.Length;
      int iNOld = 0;
      if (_shapeArray == null)
      { _shapeArray = new IntPtr[iNNew]; }
      else
      {
        iNOld = _shapeArray.Length;
        IntPtr[] pNew = new IntPtr[iNOld + iNNew];
        for (int iGeom = 0; iGeom < iNOld; iStart++)
        { pNew[iStart] = _shapeArray[iStart]; }
        _shapeArray = pNew;
      }

      for (int iGeom = 0; iGeom < iNNew; iGeom++)
      { _shapeArray[iGeom + iNOld] = geometryList[iGeom].Reference; }

    }

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    { return new MyEnumerator(this); }

    #endregion

  }
}
