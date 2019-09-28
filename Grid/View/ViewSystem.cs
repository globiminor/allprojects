using Basics.Geom;
using System;

namespace Grid.View
{
  public class ViewSystem
  {
    public const double PIX_M = 0.0254 / 72.0;

    public IPoint P { get; private set; }
    public IPoint U { get; private set; }
    public IPoint V { get; private set; }

    public static ViewSystem GetViewSystem(double azimuthDeg, double slopeDeg)
    {
      // see also getViewDir(azimuth,slope,dx,dy,dz);
      double a = (90.0 - azimuthDeg) * Math.PI / 180.0;
      double dx = Math.Cos(a);
      double dy = Math.Sin(a);

      Point3D u = new Point3D(dy, -dx, 0.0);

      double dz = Math.Sin(slopeDeg * Math.PI / 180.0);
      double l = Math.Sqrt(1.0 - dz * dz);
      dx *= l;
      dy *= l;

      Point3D v = new Point3D(dy * u.Z - dz * u.Y, dz * u.X - dx * u.Z, dx * u.Y - dy * u.X);

      Point3D p = new Point3D(dx, dy, dz);

      ViewSystem vs = new ViewSystem { P = p, U = u, V = v };
      return vs;
    }

  }
}
