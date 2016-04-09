using System;

namespace Basics.Num
{
  public delegate double Function(double x);
  /// <summary>
  /// Summary description for Math.
  /// </summary>
  public class Calc
  {

    private double _epsi;

    /// <summary>
    /// Loest die quadratische Gleichung a x^2 + b x + c = 0;
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="x0">falls reell : kleinere Loesung, sonst : Real-Teil der Loesung</param>
    /// <param name="x1">falls reell : groesser Loesung, sonst : Imaginaer-Teil der Loesung</param>
    /// <returns>true, falls die Loesungen reell,sonst false</returns>
    public static bool SolveSqr(double a, double b, double c, out double x0, out double x1)
    {
      if (a == 0)
      {
        x0 = -c / b;
        x1 = double.NaN;
        return true;
      }
      double dDet = b * b - 4 * a * c;
      double dA2 = 2 * a;
      if (dDet >= 0)
      {
        dDet = Math.Abs(Math.Sqrt(dDet) / dA2);
        x0 = -b / dA2 - dDet;
        x1 = -b / dA2 + dDet;
        return true;
      }
      else
      {
        // return real und imaginaer Teil
        dDet = Math.Sqrt(-dDet);
        x0 = -b / dA2;
        x1 = dDet / dA2;
        return false;
      }
    }

    public Calc(double epsilon)
    {
      _epsi = epsilon;
    }

    public double Epsilon
    {
      get
      { return _epsi; }
      set
      { _epsi = value; }
    }

    /// <summary>
    /// Calculate Integral of function fct from a to b
    /// </summary>
    /// <param name="fct">Function to integrate</param>
    /// <param name="a">start value</param>
    /// <param name="b">end value</param>
    /// <returns></returns>
    public double Integral(Function fct, double a, double b)
    {
      double dh;
      double[] dNew = new double[64];
      double[] dOld = new double[64];

      int nSeg_2, iIteration;

      dh = b - a;
      dNew[0] = dh * (fct(a) + fct(b)) / 2.0;
      dOld[0] = dNew[0];
      nSeg_2 = 1;
      iIteration = 0;

      do
      {
        iIteration++;
        dh = dh / 2.0;

        dNew[0] = dOld[0] / 2.0;

        for (int j = 0; j < nSeg_2; j++)
        {
          dNew[0] += dh * fct(a + (2 * j + 1) * dh);
        }

        double ff = 4;
        for (int j = 0; j < iIteration; j++)
        {
          dNew[j + 1] = dNew[j] + (dNew[j] - dOld[j]) / (ff - 1);
          ff = 4 * ff;
        }

        // check abbruch
        if (iIteration > 1 && Math.Abs(dNew[iIteration - 1] - dNew[iIteration]) <=
          _epsi * dNew[iIteration])
        { return dNew[iIteration]; }

        // naechste Iteration vorbereiten
        double[] dTemp = dNew;
        dNew = dOld;
        dOld = dTemp;

        nSeg_2 = 2 * nSeg_2;

      } while (true);
    }

    /// <summary>
    /// Loest die Gleichung fct(x) = 0 nach dem Newton-Verfahren
    /// </summary>
    /// <param name="fct">Gleichung mit gesuchter Null-Stelle</param>
    /// <param name="dFctDx">Ableitung von fct nach x</param>
    /// <param name="x1">Startwert</param>
    /// <returns></returns>
    public double SolveNewton(Function fct, Function dFctDx, double x1)
    {
      double y0 = fct(x1);

      while (Math.Abs(y0) > _epsi)
      {
        double x0 = x1;

        x1 = x0 - y0 / dFctDx(x0);
        y0 = fct(x1);
      }
      return x1;
    }

    public double SolveSekante(Function fct, double x0, double x1)
    {
      double y0 = fct(x0);
      double y1 = fct(x1);

      while (Math.Abs(y1) > _epsi && Math.Abs(y0) > _epsi)
      {
        double x2 = x0 - y0 / (y1 - y0) * (x1 - x0);
        double y2 = fct(x2);

        if (y2 < 0 == y0 < 0)
        {
          double x3 = x0 - y0 / (y2 - y0) * (x2 - x0);
          if ((x3 < x0 || x3 > x1) && (y0 > 0) != (y1 > 0))
          {
            x3 = (x0 + x1) / 2;
            double y3 = fct(x3);
            if ((y3 > 0) == (y1 > 0))
            {
              x1 = x3;
              y1 = y3;
            }
            else
            {
              x2 = x3;
              y2 = y3;
            }
          }
          x0 = x2;
          y0 = y2;
        }
        else
        {
          x1 = x2;
          y1 = y2;
        }
      }

      if (Math.Abs(y1) < Math.Abs(y0))
      { return x1; }
      else
      { return x0; }
    }
  }
}
