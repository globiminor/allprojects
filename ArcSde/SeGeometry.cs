using System;

namespace ArcSde
{
  /// <summary>
  /// Summary description for SeGeometry.
  /// </summary>
  internal abstract class SeGeometry
  {
    protected IntPtr _shpPtr;
    private bool _bFree = true;
    private static IntPtr _coordPtr = IntPtr.Zero;

    public static void SetCoordRef(double x0, double y0, double resolution)
    {
      if (_coordPtr != IntPtr.Zero)
      { CApi.SE_coordref_free(_coordPtr); }

      CApi.SE_coordref_create(out _coordPtr);
      CApi.SE_coordref_set_xy(_coordPtr, x0, y0, resolution);
    }

    protected SeGeometry()
    {
      if (_coordPtr == IntPtr.Zero)
      {
        CApi.SE_coordref_create(out _coordPtr);
        CApi.SE_coordref_set_xy(_coordPtr, 480000, 70000, 100);
      }
      CApi.SE_shape_create(_coordPtr, out _shpPtr);
    }
    protected SeGeometry(IntPtr shape, bool allowFree)
    {
      _shpPtr = shape;
      _bFree = allowFree;
    }
    protected SeGeometry(SeCoordRef cr)
    {
      CApi.SE_shape_create(cr.Ptr, out _shpPtr);
    }
    ~SeGeometry()
    {
      if (_bFree)
      { CApi.SE_shape_free(_shpPtr); }
    }
    public static SeGeometry Create(IntPtr shape, bool allowFree)
    {
      if (CApi.SE_shape_is_point(shape))
      { return new SePoint(shape, allowFree); }
      else if (CApi.SE_shape_is_line(shape))
      { return new SeLineString(shape, allowFree); }
      else if (CApi.SE_shape_is_polygon(shape))
      { return new SePolygon(shape, allowFree); }
      else
      { throw new NotImplementedException("Unhandled shape type"); }
    }

    public override int GetHashCode()
    {
      return _shpPtr.GetHashCode();
    }

    public override bool Equals(object other)
    {
      if (other is SeGeometry == false)
      { return false; }
      SeGeometry pOther = (SeGeometry)other;
      bool bEqual = CApi.SE_shape_is_equal(_shpPtr, pOther._shpPtr);

      return bEqual;
    }

    public Se_Envelope Envelope()
    {
      Se_Envelope pEnv = new Se_Envelope();
      CApi.SE_shape_get_extent(_shpPtr, 0, ref pEnv);
      return pEnv;
    }

    public SeGeometryCollection Intersection(SeGeometry other)
    {
      int nShapes = 0;
      IntPtr[] pIntersections = null;
      CApi.SE_shape_intersect(_shpPtr, other.Reference, ref nShapes, ref pIntersections);

      return new SeGeometryCollection(pIntersections);
    }

    public bool Crosses(SeGeometry other)
    {
      return CApi.SE_shape_is_crossing(_shpPtr, other._shpPtr);
    }
    public bool Intersect(SeGeometry other)
    {
      return !CApi.SE_shape_is_disjoint(_shpPtr, other._shpPtr);
    }
    public bool Within(SeGeometry other)
    {
      return CApi.SE_shape_is_within(_shpPtr, other._shpPtr);
    }
    public bool Touches(SeGeometry other)
    {
      return CApi.SE_shape_is_touching(_shpPtr, other._shpPtr);
    }
    public bool Overlaps(SeGeometry other)
    {
      return CApi.SE_shape_is_overlapping(_shpPtr, other._shpPtr);
    }

    public SeGeometryCollection Boundary()
    {
      SeGeometryCollection pBoundary = null;
      if (CApi.SE_shape_is_point(_shpPtr))
      { return null; }
      else if (CApi.SE_shape_is_line(_shpPtr))
      {
        pBoundary = new SeGeometryCollection();

        CApi.SE_shape_get_num_parts(_shpPtr, out int nParts, out int nSubParts);
        for (int iPart = 1; iPart <= nParts; iPart++)
        {
          CApi.SE_shape_get_num_points(_shpPtr, iPart, 0, out int nPoints);
          CApi.SE_shape_get_num_subparts(_shpPtr, iPart, out nSubParts);
          if ((nSubParts == 1) == false)
          { throw new NotImplementedException(string.Format("Expected 1 subpart, got {0}", nSubParts)); }

          int[] iOffsets = new int[nSubParts];
          Se_Point[] pPointList = new Se_Point[nPoints];
          CApi.SE_shape_get_points(_shpPtr, iPart, 0, iOffsets, pPointList, null, null);

          Se_Point pStart = pPointList[0];
          Se_Point pEnd = pPointList[nPoints - 1];
          if (pStart.X != pEnd.X || pStart.Y != pEnd.Y)
          {
            pBoundary.AddRange(new SeGeometry[] { new SePoint(pStart.X,pStart.Y),
              new SePoint(pEnd.X,pEnd.Y) });
          }
          else
          {
            pBoundary.AddRange(new SeGeometry[] { new SePoint(pStart.X, pStart.Y) });
          }
        }
      }

      return pBoundary;
    }
    public bool CheckCrosses(SeGeometryCollection intersections)
    {
      SeGeometryCollection pBoundary = Boundary();
      foreach (SeGeometry pInter in intersections)
      {
        bool bEqual = false;
        foreach (SeGeometry pGeom in pBoundary)
        {
          if (pGeom.Equals(pInter))
          {
            bEqual = true;
            break;
          }
        }
        if (bEqual == false)
        { return true; }
      }
      return false;
    }
    internal IntPtr Reference
    {
      get
      { return _shpPtr; }
    }

    public bool Is3D()
    {
      return CApi.SE_shape_is_3D(_shpPtr);
    }

    public bool IsMeasured()
    {
      return CApi.SE_shape_is_measured(_shpPtr);
    }
  }
}
