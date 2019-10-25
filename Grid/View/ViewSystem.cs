using Basics.Geom;
using System;

namespace Grid.View
{
  public class ViewSystem
  {
    public const double PIX_M = 0.0254 / 72.0;

    public static ViewSystem GetViewSystem(double azimuthDeg, double slopeDeg, 
      double focus, int footX, int footY)
    {
      // see also getViewDir(azimuth,slope,dx,dy,dz);
      double a = (90.0 - azimuthDeg) * Math.PI / 180.0;
      double dx = Math.Cos(a);
      double dy = Math.Sin(a);

      Point3D u = new Point3D(dy, -dx, 0.0);

      double dz = Math.Sin(slopeDeg * Math.PI / 180.0);
      double f = Math.Sqrt(1.0 - dz * dz);
      dx *= f;
      dy *= f;

      Point3D v = new Point3D(dy * u.Z - dz * u.Y, dz * u.X - dx * u.Z, dx * u.Y - dy * u.X);

      Point3D p = new Point3D(dx, dy, dz);

      double l = PIX_M / focus;
      Point lp = PointOp.Scale(1 / l, p);

      Point plane = PointOp.Scale(footX, u) + PointOp.Scale(footY, v) - lp;
      ViewSystem vs = new ViewSystem(plane, u, v);

      return vs;
    }

    private const double _epsi = 1.0e-8;

    private readonly IPoint _plane;
    private readonly IPoint _uDir;
    private readonly IPoint _vDir;

    public double Det0 { get; private set; }
    public double Det1 { get; private set; }
    public double Det2 { get; private set; }
    public double DetT { get; private set; }
    public double DetU0 { get; private set; }
    public double DetU1 { get; private set; }
    public double DetU2 { get; private set; }
    public double DetV0 { get; private set; }
    public double DetV1 { get; private set; }
    public double DetV2 { get; private set; }

    public ViewSystem(IPoint plane, IPoint uDir, IPoint vDir)
    {
      _plane = plane;
      _uDir = uDir;
      _vDir = vDir;

      CalcParams();
    }

    private void CalcParams()
    {
      /* calculating parameters */
      IPoint b = _plane;

      Det0 = _uDir.Y * _vDir.Z - _uDir.Z * _vDir.Y;
      Det1 = _uDir.Z * _vDir.X - _uDir.X * _vDir.Z;
      Det2 = _uDir.X * _vDir.Y - _uDir.Y * _vDir.X;

      DetT = b.X * Det0 + b.Y * Det1 + b.Z * Det2;

      DetU0 = b.Y * _vDir.Z - b.Z * _vDir.Y;
      DetU1 = b.Z * _vDir.X - b.X * _vDir.Z;
      DetU2 = b.X * _vDir.Y - b.Y * _vDir.X;

      DetV0 = _uDir.Y * b.Z - _uDir.Z * b.Y;
      DetV1 = _uDir.Z * b.X - _uDir.X * b.Z;
      DetV2 = _uDir.X * b.Y - _uDir.Y * b.X;
    }
    public Point3D ProjectLocalPoint(IPoint p0)
    {
      //Point p0 = new Point3D(p.X - _center.X, -(p.Y - _center.Y), p.Z - _center.Z);
      double det = p0.X * Det0 + p0.Y * Det1 + p0.Z * Det2;
      if (Math.Abs(det) < _epsi)
      {
        return null;
      }

      Point3D p1 = new Point3D
      {
        Z = -DetT / det,
        X = (p0.X * DetU0 + p0.Y * DetU1 + p0.Z * DetU2) / det,
        Y = (p0.X * DetV0 + p0.Y * DetV1 + p0.Z * DetV2) / det
      };
      return p1;
    }
  }
}
