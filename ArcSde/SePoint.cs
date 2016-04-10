using System;

namespace ArcSde
{
  internal class SePoint : SeGeometry
  {
    private Se_Point _p;
    private double _dZ = double.NaN;
    private double _dM = double.NaN;

    internal SePoint(IntPtr pointPtr, bool allowFree)
      : base(pointPtr, allowFree)
    {
      Se_Point[] pPointList = new Se_Point[1];
      double[] dZList = new double[1];
      double[] dMList = new double[1];
      CApi.SE_shape_get_points(pointPtr, 1, 0, null, pPointList, dZList, dMList);

      _p = pPointList[0];
      _dZ = dZList[0];
      _dM = dMList[0];
    }

    public SePoint(double x, double y)
    {
      GeneratePoint(x, y);
    }
    public SePoint(SeCoordRef cr, double x, double y)
      : base(cr)
    {
      GeneratePoint(x, y);
    }

    public override int GetHashCode()
    {
      return _p.GetHashCode();
    }


    private void GeneratePoint(double x, double y)
    {
      _p = new Se_Point(x, y);

      CApi.SE_shape_generate_point(1, new Se_Point[] { _p }, null, null, _shpPtr);
    }

    public double X
    {
      get { return _p.X; }
    }
    public double Y
    {
      get { return _p.Y; }
    }
    public double Z
    {
      get
      { return _dZ; }
    }
    public double M
    {
      get
      { return _dM; }
    }
  }
}
