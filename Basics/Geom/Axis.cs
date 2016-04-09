using System.Collections.Generic;

namespace Basics.Geom
{
  public class OrthogonalSystem
  {
    private List<Axis> _axes;

    public OrthogonalSystem()
    {
      _axes = new List<Axis>();
    }
    public OrthogonalSystem(int count)
    {
      _axes = new List<Axis>(count);
    }

    public static OrthogonalSystem Create(IList<IPoint> points)
    {
      OrthogonalSystem system = new OrthogonalSystem(points.Count);

      foreach (IPoint point in points)
      {
        Axis newAxe = system.GetOrthogonal(point);

        system.Axes.Add(newAxe);
      }

      return system;
    }

    public IList<Axis> Axes
    {
      get { return _axes; }
    }

    public Point OrigVector(int index)
    {
      Point p = Point.Create(_axes[index].Point);
      IList<double> factors = _axes[index].Factors;

      for (int i = 0; i < index; i++)
      {
        Axis a = _axes[i];
        p = p + factors[i] * a.Point;
      }
      return p;
    }

    public Axis GetOrthogonal(IPoint vector)
    {
      int n = _axes.Count;

      Point offset = Point.Create(vector);
      double[] factors = new double[n];

      for (int i = 0; i < n; i++)
      {
        Axis axis = _axes[i];
        double length2 = axis.Length2Epsi;
        double f;
        if (length2 != 0)
        { f = axis.Point.SkalarProduct(vector) / axis.Length2Epsi; }
        else
        { f = axis.Point.SkalarProduct(vector) / axis.Point.OrigDist2(); } 
        Point axePoint = f * axis.Point;
        offset = offset - axePoint;
        factors[i] = f;
      }

      Axis newAxis = new Axis(offset, factors);
      return newAxis;
    }

    public IList<double> VectorFactors(IList<double> orthoCoordinates)
    {
      return VectorFactors(orthoCoordinates, 0, true);
    }

    /// <summary>
    /// Calculates the factors of the OrigVectors
    /// Remark: The factors are calculated in reverse direction
    /// </summary>
    /// <param name="orthoCoordinates"></param>
    /// <param name="minIndex">indicates the minimum index of the OrigVector, where a factor will be calculated</param>
    /// <param name="fullList">indicates the order of the return values:
    /// true: factor[i] is the factor of OrigVector[i]
    /// false: the list contains only n - minIndex values, factor[i] is the factor of OrigVector[n - i - 1]
    /// </param>
    /// <returns></returns>
    public IList<double> VectorFactors(IList<double> orthoCoordinates, int minIndex, bool fullList)
    {
      int n = _axes.Count;
      IList<double> factors;

      if (fullList)
      { factors = new double[n]; }
      else
      { factors = new List<double>(n - minIndex); }

      for (int i = n - 1; i >= minIndex; i--)
      {
        double factor = orthoCoordinates[i];
        for (int j = n - 1; j > i; j--)
        {
          factor -= factors[j] * _axes[j].Factors[i];
        }

        if (fullList)
        { factors[i] = factor; }
        else
        { factors.Add(factor); }
      }
      return factors;
    }

    public OrthogonalSystem Clone()
    {
      OrthogonalSystem clone = new OrthogonalSystem();
      foreach (Axis axis in _axes)
      { clone.Axes.Add(axis); }
      return clone;
    }
  }

  public class Axis
  {
    private readonly Point _axis;
    private double _length2 = -1;

    private readonly double[] _factors;

    public Axis(Point point, double[] factors)
    {
      _axis = point;
      _factors = factors;
    }

    public Point Point
    {
      get { return _axis; }
    }

    public double[] Factors
    {
      get { return _factors; }
    }

    public double Length2
    {
      get
      {
        if (_length2 < 0)
        {
          _length2 = _axis.OrigDist2();
        }
        return _length2;        
      }
    }

    public double Length2Epsi
    {
      get
      {
        double l2 = Length2;
        if (l2 < 1.0e-12)
        {
          return 0;
        }
        return l2;
      }
    }
  }
}
