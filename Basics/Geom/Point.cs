using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom
{
  public class PointComparer : IComparer<IPoint>, IEqualityComparer<IPoint>
  {
    public bool Equals(IPoint x, IPoint y)
    {
      return Compare(x, y) == 0;
    }
    public int GetHashCode(IPoint o)
    {
      return o.X.GetHashCode() ^ o.Y.GetHashCode();
    }
    public int Compare(IPoint x, IPoint y)
    {
      int nDim = x.Dimension;
      for (int iDim = 0; iDim < nDim; iDim++)
      {
        int i = x[iDim].CompareTo(y[iDim]);
        if (i != 0)
        { return i; }
      }

      return 0;
    }
  }

  /// <summary>
  /// Summary description for Point.
  /// </summary>
  public abstract class Point : Geometry, IPoint, IBox
  {
    private double[] _vector;
    protected Point(int dimension)
    {
      _vector = new double[dimension];
    }
    protected double[] Vector
    {
      get { return _vector; }
      set { _vector = value; }
    }
    /// <summary>
    /// returns coordinate of dimension index
    /// </summary>
    public double this[int index]
    {
      get { return _vector[index]; }
      set { _vector[index] = value; }
    }
    public double[] Coordinates
    {
      get { return _vector; }
    }

    public double X
    {
      get { return _vector[0]; }
      set { _vector[0] = value; }
    }
    public double Y
    {
      get { return _vector[1]; }
      set { _vector[1] = value; }
    }

    public double Z
    {
      get { return _vector[2]; }
      set { _vector[2] = value; }
    }

    IPoint IBox.Min
    {
      get { return this; }
    }
    IPoint IBox.Max
    {
      get { return this; }
    }

    bool IBox.Contains(IBox box)
    {
      return Box.ExceedDimension(this, box, DimensionList(Dimension)) == 0;
    }
    bool IBox.Contains(IBox box, int[] dimensionList)
    {
      return Box.ExceedDimension(this, box, dimensionList) == 0;
    }

    public IGeometry Border
    { get { return null; } }
    protected override IGeometry BorderGeom
    { get { return null; } }

    /// <summary>
    /// Vector product
    /// </summary>
    public double VectorProduct(IPoint p1)
    {
      double v = X * p1.Y - Y * p1.X;
      return v;
    }

    public Point3D VectorProduct3D(IPoint p1)
    {
      if (p1 == null)
      { return null; }

      Point3D p = new Point3D
      {
        X = Y * p1.Z - Z * p1.Y,
        Y = Z * p1.X - X * p1.Z,
        Z = X * p1.Y - Y * p1.X
      };
      return p;
    }

    public override IBox Extent
    {
      get { return this; }
    }

    private Point Project__(IProjection projection)
    {
      IPoint p = projection.Project(this);

      return CastOrCreate(p);
    }
    IGeometry IGeometry.Project(IProjection projection)
    { return Project(projection); }
    IPoint IPoint.Project(IProjection projection)
    { return Project(projection); }
    protected override Geometry Project_(IProjection projection)
    { return Project__(projection); }
    public new Point Project(IProjection projection)
    { return (Point)Project_(projection); }

    public Point Clone()
    {
      Point p = new Vector(Dimension);
      for (int i = 0; i < Dimension; i++)
      { p[i] = this[i]; }
      return p;
    }
    public override int Topology
    { get { return 0; } }

    public static Point Create(double[] coords)
    {
      return new Vector(coords);
    }

    public static Point Create(int dim)
    {
      if (dim == 2)
      { return new Point2D(); }
      else if (dim == 3)
      { return new Point3D(); }
      else
      { return new Vector(dim); }
    }

    public static Point CastOrCreate(IPoint point)
    {
      Point p = point as Point ?? Create(point);
      return p;
    }
    public static Point Create(IPoint point)
    {
      int iDim = point.Dimension;
      Point p = Create(iDim);
      for (int i = 0; i < iDim; i++)
      { p[i] = point[i]; }
      return p;
    }
    public double Dist2(IPoint point)
    {
      double dDist2 = 0;
      for (int i = 0; i < Dimension; i++)
      {
        double d = this[i] - point[i];
        dDist2 += d * d;
      }
      return dDist2;

    }

    public double OrigDist2()
    {
      double dist2 = 0;
      for (int i = 0; i < Dimension; i++)
      {
        double d = this[i];
        dist2 += d * d;
      }
      return dist2;
    }

    public Relation RelationTo(IBox box)
    {
      if (box.IsWithin(this))
      { return Relation.Within; }
      else
      { return Relation.Disjoint; }
    }
    public override bool IsWithin(IPoint point)
    {
      return EqualGeometry(point);
    }
    public override bool EqualGeometry(IGeometry other)
    {
      if (this == other)
      { return true; }

      if (!(other is IPoint o))
      { return false; }

      if (Dimension != o.Dimension)
      { return false; }

      for (int dim = 0; dim < Dimension; dim++)
      {
        if (this[dim] != o[dim])
        { return false; }
      }
      return true;
    }

    double IBox.GetMaxExtent()
    {
      return 0;
    }
    void IBox.Include(IBox box)
    {
      throw new InvalidOperationException("Cannot include Box into Point");
    }
    IBox IBox.Clone()
    {
      return new Box(Clone(), Clone());
    }

    public static Point operator +(IPoint p0, Point p1)
    {
      int iDim = Math.Min(p0.Dimension, p1.Dimension);
      Point sum = Create(iDim);
      for (int i = 0; i < iDim; i++)
      { sum[i] = p0[i] + p1[i]; }

      return sum;
    }

    public static Point operator -(Point p0)
    {
      int dim = p0.Dimension;
      Point diff = Create(p0.Dimension);
      for (int i = 0; i < dim; i++)
      { diff[i] = -p0[i]; }

      return diff;
    }

    public static Point operator -(Point p0, IPoint p1)
    { return Sub(p0, p1); }
    public static Point operator -(IPoint p0, Point p1)
    { return Sub(p0, p1); }
    public static Point operator -(Point p0, Point p1)
    { return Sub(p0, p1); }
    public static Point Sub(IPoint p0, IPoint p1)
    {
      int iDim = Math.Min(p0.Dimension, p1.Dimension);
      Point diff = Create(iDim);
      for (int i = 0; i < iDim; i++)
      { diff[i] = p0[i] - p1[i]; }

      return diff;
    }

    /// <summary>
    /// scalar product
    /// </summary>
    public static Point operator *(double f, Point p)
    {
      int iDim = p.Dimension;
      Point skalar = Create(iDim);
      for (int i = 0; i < iDim; i++)
      { skalar[i] = f * p[i]; }

      return skalar;
    }

    public double SkalarProduct(IPoint p1)
    {
      double prod = 0;
      int maxDim = Math.Min(Dimension, p1.Dimension);
      for (int iDim = 0; iDim < maxDim; iDim++)
      { prod += this[iDim] * p1[iDim]; }

      return prod;
    }

    /// <summary>
    /// get factor f of skalar product p0 = f * p1
    /// </summary>
    /// <param name="p0">result of scalar product</param>
    /// <returns>factor</returns>
    public double GetFactor(IPoint p0)
    {
      int iDim = Dimension;
      double dMin = 0;
      double f = 0;
      for (int i = 0; i < iDim; i++)
      {
        if (Math.Abs(this[i]) > dMin)
        {
          f = p0[i] / this[i];
          dMin = Math.Abs(this[i]);
        }
      }

      return f;
    }
  }

  public class Point2D : Point
  {
    [DebuggerStepThrough]
    public Point2D()
      : base(2)
    { }
    public Point2D(double x, double y)
      : base(2)
    {
      Vector[0] = x;
      Vector[1] = y;
    }
    public Point2D(IPoint p)
      : this(p.X, p.Y)
    { }
    protected Point2D(int dimension) : base(dimension) { }

    public override bool Equals(object obj)
    {
      if (!(obj is IPoint cmpr))
      { return false; }
      if (cmpr.Dimension != Dimension)
      { return false; }
      return Vector[0] == cmpr[0] && Vector[1] == cmpr[1];
    }
    public override int GetHashCode()
    {
      return Vector[0].GetHashCode() ^ Vector[1].GetHashCode();
    }

    #region IGeometry Members

    public override int Dimension
    {
      get { return 2; }
    }

    public new Point2D Clone()
    {
      return new Point2D(Vector[0], Vector[1]);
    }

    public override string ToString()
    { return string.Format("{0};{1}", X, Y); }
    #endregion

    public static double Dist2(IPoint p0, IPoint p1)
    {
      double d2 = 0;
      double d = p0.X - p1.X;
      d2 += d * d;
      d = p0.Y - p1.Y;
      d2 += d * d;
      return d2;
    }
  }

  public class Point3D : Point
  {
    public Point3D()
      : base(3)
    { }
    public Point3D(double x, double y, double z)
      : base(3)
    {
      Vector[0] = x;
      Vector[1] = y;
      Vector[2] = z;
    }
    protected Point3D(int dimension)
      : base(dimension)
    { }

    public Point3D Transform(Point3D trans, double[][] rotMatrix)
    {

      Point3D p = new Point3D();

      double[][] r = rotMatrix;

      p.X = r[0][0] * X + r[0][1] * Y + r[0][2] * Z;
      p.Y = r[1][0] * X + r[1][1] * Y + r[1][2] * Z;
      p.Z = r[2][0] * X + r[2][1] * Y + r[2][2] * Z;

      if (trans != null)
      {
        p.X += trans.X;
        p.Y += trans.Y;
        p.Z += trans.Z;
      }

      return p;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Point3D cmpr))
      { return false; }
      if (cmpr.Dimension != Dimension)
      { return false; }
      return Vector[0] == cmpr[0] && Vector[1] == cmpr[1] && Vector[2] == cmpr[2];
    }
    public override int GetHashCode()
    {
      return Vector[0].GetHashCode() ^ Vector[1].GetHashCode();
    }

    public override string ToString()
    {
      return string.Format("{0};{1};{2}", X, Y, Z);
    }

    #region IGeometry Members

    public override int Dimension
    {
      get { return 3; }
    }

    public new Point3D Clone()
    {
      return new Point3D(Vector[0], Vector[1], Vector[2]);
    }

    #endregion
  }

  /// <summary>
  /// Summary description for Point.
  /// </summary>
  public class Vector : Point
  {
    private readonly int _dimension;

    public new Point Clone()
    {
      return new Vector((double[])Vector.Clone());
    }

    public Vector(int dimension)
      : base(dimension)
    {
      Vector = new double[dimension];
      _dimension = dimension;
    }
    public Vector(double[] coordinates)
      : base(0)
    {
      Vector = coordinates;
      _dimension = coordinates.Length;
    }
    public override bool Equals(object obj)
    {
      if (!(obj is IPoint cmpr))
      { return false; }
      if (cmpr.Dimension != _dimension)
      { return false; }
      for (int i = 0; i < _dimension; i++)
      {
        if (cmpr[i] != Vector[i])
        { return false; }
      }
      return true;
    }

    public override int GetHashCode()
    {
      if (_dimension == 1)
      { return Vector[0].GetHashCode(); }
      else
      {
        return (Vector[0].GetHashCode() ^ Vector[1].GetHashCode());
      }
    }

    public static Vector operator +(Vector v0, Vector v1)
    { return v0.Add(v1); }
    public Vector Add(Vector v1)
    {
      Debug.Assert(Dimension == v1.Dimension);
      int iDim = Dimension;
      Vector v = new Vector(iDim);
      for (int i = 0; i < iDim; i++)
      { v[i] = this[i] + v1[i]; }
      return v;
    }

    public static Vector operator -(Vector v0, Vector v1)
    { return v0.Sub(v1); }
    public Vector Sub(Vector v1)
    {
      Debug.Assert(Dimension == v1.Dimension);
      int iDim = Dimension;
      Vector v = new Vector(iDim);
      for (int i = 0; i < iDim; i++)
      { v[i] = this[i] - v1[i]; }
      return v;
    }

    public double Dist2(Vector v)
    {
      Debug.Assert(Dimension == v.Dimension);
      double d2 = 0;
      for (int i = 0; i < Dimension; i++)
      {
        double d = this[i] - v[i];
        d2 += d * d;
      }
      return d2;
    }

    public override string ToString()
    {
      if (Dimension > 2)
      { return string.Format("{0};{1};{2}", X, Y, Z); }
      else if (Dimension > 1)
      { return string.Format("{0};{1}", X, Y); }
      else if (Dimension > 0)
      { return X.ToString(); }
      else
      { return "null"; }
    }

    #region IGeometry Members

    public override int Dimension
    {
      get { return _dimension; }
    }

    #endregion

  }
}
