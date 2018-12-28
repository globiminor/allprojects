using System;

namespace Basics.Geom.Projection
{
  public class InclinedMercator : Projection
  {
    private double _lam0;
    private double _phi0;

    private double _phi1;
    private double _alpha;
    private double _r0;
    private double _e;
    private double _k;

    private double _offX;
    private double _offY;

    public void SetOffset(double offX, double offY)
    {
      _offX = offX;
      _offY = offY;
    }
    public override void SetEllipsoid(Ellipsoid ellipsoid)
    {
      SetParams(ellipsoid, _lam0, _phi0);
    }

    public void SetParams(Ellipsoid ellipsoid, double lam0, double phi0)
    {
      base.SetEllipsoid(ellipsoid);
      _lam0 = lam0;
      _phi0 = phi0;

      double a = ellipsoid.A;
      double b = ellipsoid.B;
      double e2 = ellipsoid.E2;

      double cp = Math.Cos(_phi0);
      double sp = Math.Sin(_phi0);

      double es2 = (a * a - b * b) / (b * b);
      double v0 = Math.Sqrt(1 + es2 * cp * cp);
      _phi1 = Math.Atan(Math.Tan(_phi0) / v0);
      _alpha = sp / Math.Sin(_phi1);
      double w_ch = Math.Sqrt(1 - e2 * sp * sp);
      double m0 = b * b / (a * w_ch * w_ch * w_ch);
      double n0 = a / w_ch;
      _r0 = Math.Sqrt(m0 * n0);

      _e = Math.Sqrt(e2);
      double k1 = Math.Tan(Math.PI / 4.0 + _phi1 / 2.0) /
        Math.Exp(Math.Log(Math.Tan(Math.PI / 4.0 + _phi0 / 2.0)) * _alpha);
      double k2 = Math.Exp(Math.Log((1 + _e * sp) / (1 - _e * sp)) * _alpha * _e / 2.0);
      _k = k1 * k2;
    }

    //transforms sphere-coordinates into ellipsoidal coordinates
    //(after Gauss 1843, 2. proj. sphere <-> ellipsoid )
    private double Kugel2Ellipsoid(double k_phi, double a, double k, double e)
    {
      double phi0, phi1, f;

      f = Math.Exp(-1 / a * Math.Log(k) + 1 / a * Math.Log(Math.Tan(Math.PI / 4.0 + k_phi / 2.0)));
      phi0 = k_phi + 1.0;
      phi1 = k_phi;
      while (Math.Abs(phi1 - phi0) > Geometry.epsi)
      {
        phi0 = phi1;
        double sp = Math.Sin(phi0);
        double f0 = Math.Exp(Math.Log((1 + e * sp) / (1 - e * sp)) * e / 2.0);
        phi1 = 2 * Math.Atan(f * f0) - Math.PI / 2.0;
      }
      return phi1;
    }


    /// <summary>
    /// transforms mercator coordinates [meter] into
    /// geographic coordinates [rad]
    /// </summary>
    private Point Prj2Gg(IPoint p, double r0)
    {
      return new Point2D(
        p.X / r0,
        2.0 * (Math.Atan(Math.Exp(p.Y / r0)) - Math.PI / 4.0)
        );
    }


    public override IPoint Prj2Gg(IPoint prj)
    {
      prj = new Point2D(prj.X - _offX, prj.Y - _offY);
      Point kugel = Prj2Gg(prj, _r0);

      Point3D p3 = new Point3D(
        Math.Cos(kugel.X) * Math.Cos(kugel.Y),
        Math.Sin(kugel.X) * Math.Cos(kugel.Y),
        Math.Sin(kugel.Y));

      p3 = Ellipsoid.Rotate(p3, 1, -_phi1);

      double lam = Math.Atan2(p3.Y, p3.X);
      double phi = Math.Asin(p3.Z);

      Point ell = kugel; // kugel wird nicht mehr benoetigt
      ell.X = _lam0 + lam / _alpha;
      ell.Y = Kugel2Ellipsoid(phi, _alpha, _k, _e);

      return ell;
    }

    /// <summary>
    /// transforms sphere-coordinates into ellipsoidal coordinates
    /// (after Gauss 1843, 2. proj. sphere <-> ellipsoid )
    /// </summary>
    private double Ellipsoid2Kugel(double e_phi, double a, double k, double e)
    {
      double f1, f2;

      f1 = k * Math.Exp(a * Math.Log(Math.Tan(Math.PI / 4.0 + e_phi / 2.0)));
      f2 = Math.Exp(Math.Log((1 - e * Math.Sin(e_phi)) / (1 + e * Math.Sin(e_phi))) * e * a / 2.0);
      return 2 * Math.Atan(f1 * f2) - Math.PI / 2.0;
    }

    /// <summary>
    ///     transforms geographic coordinates [rad] into
    ///     mercartor coordinates [m]
    /// </summary>
    private Point Gg2Prj(IPoint gg, double r0)
    {
      Point2D prj = new Point2D(
        r0 * gg.X,
        r0 * Math.Log(Math.Tan(gg.Y / 2.0 + Math.PI / 4.0)));
      return prj;
    }

    public override IPoint Gg2Prj(IPoint ell)
    {
      Point2D kugel = new Point2D(
        _alpha * (ell.X - _lam0),
        Ellipsoid2Kugel(ell.Y, _alpha, _k, _e)
        );

      Point3D p3 = new Point3D(
      Math.Cos(kugel.X) * Math.Cos(kugel.Y),
      Math.Sin(kugel.X) * Math.Cos(kugel.Y),
      Math.Sin(kugel.Y));

      p3 = Ellipsoid.Rotate(p3, 1, _phi1);
      kugel.X = Math.Atan2(p3.Y, p3.X);
      kugel.Y = Math.Asin(p3.Z);
      Point prj = Gg2Prj(kugel, _r0);

      prj.X += _offX;
      prj.Y += _offY;
      return prj;
    }
  }

  public class Ch1903Raw : InclinedMercator
  {
    protected Ch1903Raw(double offsetEast, double offsetNorth)
      : this()
    {
      SetOffset(offsetEast, offsetNorth);
    }
    public Ch1903Raw()
    {
      Ellipsoid ellipsoid = new Ellipsoid.Bessel();
      ellipsoid.Datum = new Datum.Ch1903();

      int lam_g = 7;
      int lam_m = 26;
      double lam_s = 22.5;
      double lam0 = Units.Gms2Rad(lam_g, lam_m, lam_s);

      int phi_g = 46;
      int phi_m = 57;
      double phi_s = 8.66;
      double phi0 = Units.Gms2Rad(phi_g, phi_m, phi_s);

      SetParams(ellipsoid, lam0, phi0);
    }
  }
  public class Ch1903 : Ch1903Raw
  {
    public Ch1903()
      : base(600000.0, 200000.0)
    { }
  }

  public class Ch1903_LV95 : Ch1903Raw
  {
    public Ch1903_LV95()
      : base(2600000.0, 1200000.0)
    { }
  }

}
