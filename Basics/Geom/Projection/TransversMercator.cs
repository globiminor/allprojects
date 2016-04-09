using System;

namespace Basics.Geom.Projection
{
  public class TransversMercator : Projection
  {
    private int _nE;
    private double[] _eFe;
    private double[] _eFn;
    private double _e2;

    private double _scale0 = 1.0;

    public void SetParameter(double scale0)
    {
      _scale0 = scale0;
    }
     ///this routine computes the values of the factors for approximation
     ///of meridian length (the largest neglected factor is of the size of
     ///2.e-11 (for n_fe = 4) )
    public override void SetEllipsoid(Ellipsoid ellipsoid)
    {
      base.SetEllipsoid(ellipsoid);

      _e2 = ellipsoid.E2;

      double a1, a2, a3;
      double[/*10*/] c;

      _nE = 5;
      _eFe = new double[_nE];
      _eFn = new double[_nE];
      c = new double[_nE];

      for (int i = 0; i < _nE; i++)
      {
        _eFe[i] = 0.0;
        c[i] = 1.0;
      }
      for (int i = 1; i < _nE; i++)
      {
        c[0] = c[0] * (2 * i - 1) / (2 * i) * _e2 * (2 * i + 1) / (2 * i);
        for (int j = 1; j < _nE; j++)
        {
          c[j] = c[j] * _e2 * (2 * i + 1) / (2 * i);
        }
        for (int j = 1; j < i; j++)
        {
          c[j] = c[j] * (2 * i - 1) / (2 * i);
        }
        c[i] = c[i] / (2 * i);

        _eFe[0] = _eFe[0] + c[0];
        for (int j = 1; j <= i; j++)
        {
          _eFe[j] = _eFe[j] - c[j];
        }
      }

      _eFn[0] = 1 / _eFe[0];
      a1 = _eFn[0] * _eFe[1];
      a2 = _eFn[0] * _eFe[2];
      a3 = _eFn[0] * _eFe[3];
      _eFn[1] = -((a1 - 1) * a1 + 1) * a1;
      _eFn[2] = -(((-6 * a1 + 2) * a1 - 4 * a2) * a1 + a2);
      _eFn[3] = -((6 * a1 * a1 + 6 * a2) * a1 + a3);
    }

    /// <summary>
    ///          this routine computes the length of the meriadian from 0 latitude
    ///          to `phi' latitude
    ///          to compute the factores `Mefe', call the routine init_merid !
    /// </summary>
    double LatitudeMeridian(double phi)
    {
      double sp, sp2, cp, m;
      int i;

      sp = Math.Sin(phi);
      sp2 = sp * sp;
      cp = Math.Cos(phi);
      m = 0.0;
      for (i = _nE - 1; i > 0; i--)
      {
        m = sp2 * m + _eFe[i];
      }

      return Ellipsoid.A * (1 - _e2) * (_eFe[0] * phi + sp * cp * m);
    }


    /// <summary>
    ///  this routine computes the latitude of the meriadian from its length
    ///  to compute the factores `MeFn', call the routine init_merid !
    /// </summary>
    /// <param name="no"></param>
    /// <returns></returns>
    double MeridianLatitude(double no)
    {
      double phi, sp, sp2, cp, m;
      int i;

      phi = _eFn[0] * no / (Ellipsoid.A * (1 - _e2));
      sp = Math.Sin(phi);
      sp2 = sp * sp;
      cp = Math.Cos(phi);
      m = 0.0;
      for (i = 1; i <= 3; i++)
      {
        m = sp2 * m + _eFn[4 - i];
      }

      return phi + sp * cp * m;
    }



    public override IPoint Gg2Prj(IPoint ell)
    {
      if (_e2 != Ellipsoid.E2)
      { SetEllipsoid(Ellipsoid); }

      double cp, cp2, sp, t2, eta2, n, l2;
      double[] fa = new double[9];
      int i;

      cp = Math.Cos(ell.Y);
      cp2 = cp * cp;
      sp = Math.Sin(ell.Y);
      t2 = sp * sp / cp2;

      eta2 = _e2 / (1 - _e2) * cp2;
      n = Ellipsoid.A / Math.Sqrt(1 - _e2 * sp * sp);
      l2 = ell.X * ell.X * cp2;

      fa[1] = 1.0;
      fa[3] = (-t2 + eta2 + 1) / 6.0;
      fa[5] = (((-64 * t2 + 13) * eta2 + (-58 * t2 + 14)) * eta2
             + (t2 - 18) * t2 + 5) / 120.0;
      fa[7] = (((-t2 + 179) * t2 - 479) * t2 + 61) / 5040.0;

      double x;
      x = 0.0;
      for (i = 7; i > 0; i -= 2)
      {
        x = x * l2 + fa[i];
      }
      x = x * n * cp * ell.X;

      fa[2] = 1 / 2.0;
      fa[4] = ((4 * eta2 + 9) * eta2 - t2 + 5) / 24.0;
      fa[6] = ((-330 * t2 + 270) * eta2 + (t2 - 58) * t2 + 61) / 720.0;
      fa[8] = (((-t2 + 543) * t2 - 3111) * t2 + 1385) / 40320.0;

      double y;
      y = 0.0;
      for (i = 8; i > 0; i -= 2)
      {
        y = y * l2 + fa[i];
      }
      y = y * n * l2 * sp / cp + LatitudeMeridian(ell.Y);

      return new Point2D(_scale0 * x, _scale0 * y);
    }

    public override IPoint Prj2Gg(IPoint prj)
    {
      if (_e2 != Ellipsoid.E2)
      { SetEllipsoid(Ellipsoid); }

      double cp, cp2, sp, t2, eta2, n, q, q2, ps;
      double[] fa = new double[8];
      int i;
      double lam, phi;

      double x = prj.X / _scale0;
      double y = prj.Y / _scale0;

      ps = MeridianLatitude(y);
      cp = Math.Cos(ps);
      cp2 = cp * cp;
      sp = Math.Sin(ps);
      t2 = sp * sp / cp2;

      eta2 = _e2 / (1 - _e2) * cp2;
      n = Ellipsoid.A / Math.Sqrt(1 - _e2 * sp * sp);
      q = x / n;
      q2 = q * q;

      fa[1] = 1.0;
      fa[3] = -(1 + 2 * t2 + eta2) / 6.0;
      fa[5] = (5 + t2 * (28 + 24 * t2) + eta2 * (6 + 8 * t2)) / 120.0;
      fa[7] = -(61 + t2 * (662 + t2 * (1320 + t2 * 720))) / 5040.0;

      lam = 0.0;
      for (i = 7; i > 0; i -= 2)
      {
        lam = lam * q2 + fa[i];
      }
      lam = q * lam / cp;

      fa[2] = -1 / 2.0;
      fa[4] = (5 + 3 * t2 + eta2 * (1 - 9 * t2 - 4 * eta2)) / 24.0;
      fa[6] = -(61 + t2 * (90 + 45 * t2) +
              eta2 * (46 + t2 * (-252 - 90 * t2))) / 720.0;

      phi = 0.0;
      for (i = 6; i > 0; i -= 2)
      {
        phi = phi * q2 + fa[i];
      }
      phi = phi * (1 + eta2) * q2 * sp / cp + ps;

      return new Point2D(lam, phi);
    }
  }

  public class Utm : TransversMercator
  {
    public Utm()
    {
      base.SetEllipsoid(new Ellipsoid.Hayford());
      SetParameter(0.9996);
    }
  }
}
