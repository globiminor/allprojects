using System;

namespace ArcSde
{
  internal class SePolygon : SeGeometry
  {
    internal SePolygon(IntPtr linePtr, bool allowFree)
      : base(linePtr, allowFree)
    {
      CApi.SE_shape_get_num_points(_shpPtr, 0, 0, out int nPoints);
    }

    public SePolygon(Se_Point[][] pointList)
    {
      int nRings = pointList.Length;
      int nPoints = 0;
      int[] iOffsets = new int[nRings];
      for (int iRing = 0; iRing < nRings; iRing++)
      {
        iOffsets[iRing] = nPoints;
        nPoints += pointList[iRing].Length;
      }

      Se_Point[] pAllPoints = new Se_Point[nPoints];
      int iPoint = 0;
      for (int iRing = 0; iRing < nRings; iRing++)
      {
        Se_Point[] pRing = pointList[iRing];
        foreach (var pPoint in pRing)
        {
          pAllPoints[iPoint] = pPoint;
          iPoint++;
        }
      }

      CApi.SE_shape_generate_polygon(nPoints, nRings, iOffsets, pAllPoints, null, null, _shpPtr);
    }
  }
}
