using System;

namespace ArcSde
{
  internal class SeLineString : SeGeometry
  {
    internal SeLineString(IntPtr linePtr, bool allowFree)
      : base(linePtr, allowFree)
    { }

    public SeLineString(SePoint[] pointList)
    {
      int nPoints = pointList.Length;
      Se_Point[] pList = new Se_Point[nPoints];
      bool b3D = pointList[0].Is3D();
      bool bMeasured = pointList[0].IsMeasured();

      double[] dZ = null;
      double[] dM = null;
      if (b3D)
      { dZ = new double[nPoints]; }
      if (bMeasured)
      { dM = new double[nPoints]; }
      for (int iPoint = 0; iPoint < nPoints; iPoint++)
      {
        SePoint current = pointList[iPoint];

        Se_Point p = new Se_Point(current.X, current.Y);
        pList[iPoint] = p;

        if (dZ != null && current.Is3D())
        { dZ[iPoint] = current.Z; }
        else
        { dZ = null; }

        if (dM != null && current.IsMeasured())
        { dM[iPoint] = current.M; }
        else
        { dM = null; }
      }

      CApi.SE_shape_generate_line(nPoints, 1, new Int32[] { 0 }, pList, dZ, dM, _shpPtr);
    }

    public SeLineString(Se_Point[] pointList)
    {
      CApi.SE_shape_generate_line(pointList.Length, 1, new Int32[] { 0 },
        pointList, null, null, _shpPtr);
    }

    public bool IsSimple()
    {
      IntPtr pSeGeom1;
      CApi.SE_shape_create(IntPtr.Zero, out pSeGeom1);
      bool bReturn;

      try
      {
        int rc = CApi.SE_shape_as_simple_line(_shpPtr, pSeGeom1);
        if (rc != 0)
        {
          bReturn = false;
          if (rc != SdeErrNo.SE_SELF_INTERSECTING)
          {
            //ErrorHandling.checkRC(IntPtr.Zero, IntPtr.Zero, rc);
          }
        }
        else
        {
          int nParts, nSubParts;
          CApi.SE_shape_get_num_parts(pSeGeom1, out nParts, out nSubParts);

          bReturn = (nParts == 1);
        }
      }
      finally
      {
        if (pSeGeom1 != IntPtr.Zero)
        { CApi.SE_shape_free(pSeGeom1); }
      }

      return bReturn;
    }
  }
}
