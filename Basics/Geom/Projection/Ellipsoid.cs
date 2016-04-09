using System;

namespace Basics.Geom.Projection
{
  public class Ellipsoid
  {
    public Point3D Rotate(Point3D p, int axis, double phi)
    {
      double cp, sp;

      cp = Math.Cos(phi);
      sp = Math.Sin(phi);

      Point3D r = new Point3D();
      switch (axis)
      {
        case 2:
          r.X = p.X * cp - p.Y * sp;
          r.Y = p.X * sp + p.Y * cp;
          r.Z = p.Z;
          break;
        case 1:
          r.Z = p.Z * cp - p.X * sp;
          r.X = p.Z * sp + p.X * cp;
          r.Y = p.Y;
          break;
        case 0:
          r.Y = p.Y * cp - p.Z * sp;
          r.Z = p.Y * sp + p.Z * cp;
          r.X = p.X;
          break;
      }
      return r;
    }

    private Datum _datum;

    private double _a;
    private double _e2;

    private double _g0;
    private double _g_f1;
    private double _g_f2;

    private double _gm;
    private double _om;
    private double _w0;

    public Ellipsoid(double a, double e2) :
      this(a, e2, null)
    { }
    public Ellipsoid(double a, double e2, Datum datum)
    {
      _a = a;
      _e2 = e2;

      _datum = datum;
    }

    public Datum Datum
    {
      get { return _datum; }
      set { _datum = value; }
    }
    public double A
    {
      get { return _a; }
    }
    public double B
    {
      // e2 = (a * a - b * b) / (a * a);
      get { return _a * Math.Sqrt(1 - _e2); }
    }
    public double E2
    {
      get { return _e2; }
    }
    public double Gm
    {
      get { return _gm; }
    }
    public double Om
    {
      get { return _om; }
    }
    private double W0
    {
      get { return _w0; }
    }
    private Ellipsoid()
    { }

    public Point3D Kartesic2Elliptic(Point3D p)
    {
      double phi;
      double lam;
      double h;

      double re, r, phi0, phi1;

      lam = Math.Atan2(p.Y, p.X);
      r = Math.Sqrt(p.X * p.X + p.Y * p.Y);
      phi1 = 0.0;
      phi0 = 1.0;
      h = 0.0;
      while (Math.Abs(phi1 - phi0) > Geometry.epsi)
      {
        phi0 = phi1;
        re = _a / Math.Sqrt(1 - _e2 * Math.Sin(phi0) * Math.Sin(phi0));
        phi1 = Math.Atan(p.Z * (re + (h)) / (r * (re * (1 - _e2) + h)));
        h = r / Math.Cos(phi1) - re;
      }
      phi = phi1;
      re = _a / Math.Sqrt(1 - _e2 * Math.Sin(phi1) * Math.Sin(phi1));
      h = r / Math.Cos(phi1) - re;

      Point3D ret = new Point3D(lam, phi, h);
      return ret;
    }


    public Point3D Elliptic2Kartesic(Point3D e)
    {
      double lam = e.X;
      double phi = e.Y;
      double h = e.Z;

      double sp = Math.Sin(phi);
      double cp = Math.Cos(phi);

      double rn = _a / Math.Sqrt(1 - _e2 * sp * sp);
      Point3D r = new Point3D();
      r.X = (rn + h) * cp * Math.Cos(lam);
      r.Y = (rn + h) * cp * Math.Sin(lam);
      r.Z = (rn * (1 - _e2) + h) * sp;

      return r;
    }


    public double NormG(double lat)
    {
      double si1, si2;

      si1 = Math.Sin(lat);
      si2 = Math.Sin(2.0 * lat);

      return _g0 * (1.0 + _g_f1 * si1 * si1 + _g_f2 * si2 * si2);
    }

    public static double E2fromAB(double a, double b)
    {
      return (a * a - b * b) / (a * a);
    }

    public static double BfromAF(double a, double f)
    {
      return a - a / f;
    }


    /*    geodetic reference systems  */
  public class Everest : Ellipsoid
  {
    public Everest()
    {
      _a = 6377276.345;
      double f = 300.8017;
      double b = BfromAF(_a, f);
      _e2 = E2fromAB(_a, b);
    }
  }

    public class Bessel : Ellipsoid
    {
      public Bessel()
      {
        _a = 6377397.155;
        _e2 = 0.006674372230614;
      }
    }

    public class Clarke66 : Ellipsoid
    {
      public Clarke66()
      {
        _a = 6378206.4;
        double f = 294.9787;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);
      }
    }

    public class Clarke80 : Ellipsoid
    {
      public Clarke80()
      {
        _a = 6378249.145;
        double f = 293.465;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);
      }
    }

    public class Hayford : Ellipsoid
    {
      public Hayford()
      /*     international ellipsoid 1930, stockholm  */
      {
        _a = 6378388.0;
        double f = 297.0;

        double b = BfromAF(_a,f);
        _e2 = E2fromAB(_a, b);

        _g0 = 9.780490;
        _g_f1 = 5.2884e-3;
        _g_f2 = -5.87e-6;
      }
    }

    public class Krassowsky : Ellipsoid
    {
      public Krassowsky()
      {
        _a = 6378245.0;
        double f = 298.3;
        _g0 = 9.780490;

        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);
      }
    }

    public class Australia : Ellipsoid
    {
      public Australia()
      {
        _a = 6378160.0;
        double f = 298.25;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);
      }
    }

    public class Grs67 : Ellipsoid
    {
      public Grs67()
      {
        _a = 6378160.0;
        double f = 298.247;

        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);

        _g0 = 9.780318;
        _g_f1 = 5.3024e-3;
        _g_f2 = -5.87e-6;
      }
    }

    public class Wgs72 : Ellipsoid
    {
      public Wgs72()
      {
        _a = 6378135.0;
        double f = 298.26;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);
      }
    }

    public class Grs75 : Ellipsoid
    {
      public Grs75()
      {
        _a = 6378140.0;
        double f = 298.257;

        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);

        _g0 = 9.780320;
        _g_f1 = 5.30233e-3;
        _g_f2 = -5.89e-6;

        _gm = 3.986005e14;
        _om = 7.292115e-5;
        _w0 = 6.263683e07;
      }
    }

    public class Grs80 : Ellipsoid
    {
      public Grs80()
      {
        _a = 6378137.0;
        double f = 298.257222101;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);

        _g0 = 9.780327;
        _g_f1 = 5.3024e-3;
        _g_f2 = -5.87e-6;

        _gm = 3.986005e14;
        _om = 7.292115e-5;
        _w0 = 6.283686e07;
      }
    }

    public class Grs83 : Ellipsoid
    {
      public Grs83()
      {
        _a = 6378136.0;
        double f = 298.257;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);

        _g0 = 9.78032;
        _gm = 3.9860044e14;
        _om = 7.292115e-5;
      }
    }

    public class Wgs84 : Ellipsoid
    {
      public Wgs84()
      {
        _a = 6378137.0;
        double f = 298.257223563;
        double b = BfromAF(_a, f);
        _e2 = E2fromAB(_a, b);

        _g0 = 9.780326772;
        _gm = 3.986008e14;
        _om = 7.292115e-5;
      }
    }
  }
}
