using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public class Box : IBox
  {
    private Point _min;
    private Point _max;
    private readonly int _dimension;
    private int _topology = -1;

    public Box(IBox source) :
      this(Point.Create(source.Min), Point.Create(source.Max))
    { }
    public Box(Point min, Point max)
    {
      _dimension = min.Dimension;
      _min = min;
      _max = max;
    }
    public Box(Point p0, Point p1, bool verify)
    {
      if (p0.Dimension < p1.Dimension)
      { _dimension = p0.Dimension; }
      else
      { _dimension = p1.Dimension; }
      if (verify)
      {
        for (int i = 0; i < _dimension; i++)
        {
          if (p0[i] > p1[i])
          {
            double t = p1[i];
            p1[i] = p0[i];
            p0[i] = t;
          }
        }
      }
      _min = p0;
      _max = p1;
    }
    public bool EqualGeometry(IGeometry other)
    { return this == other; }

    // properties
    IPoint IBox.Min => Min;
    public Point Min => _min;

    IPoint IBox.Max => Max;
    public Point Max => _max;

    public double GetMaxExtent()
    {
      double dMax = 0;
      for (int i = 0; i < Dimension; i++)
      {
        double dExtent = _max[i] - _min[i];
        if (dExtent > dMax)
        { dMax = dExtent; }
      }
      return dMax;
    }

    public double Dist2(IBox box, IList<int> calcDimensions)
    {
      double d2 = 0;
      foreach (var i in calcDimensions)
      {
        double d;
        if (Max[i] < box.Min[i])
        { d = box.Min[i] - Max[i]; }
        else if (Min[i] > box.Max[i])
        { d = Min[i] - box.Max[i]; }
        else
        { continue; }

        d2 += d * d;
      }
      return d2;
    }

    // methodes
    public static bool IsWithin(IBox box, IPoint p)
    {
      int dim = box.Dimension;
      for (int i = 0; i < dim; i++)
      {
        if (p[i] < box.Min[i])
        { return false; }
        if (p[i] > box.Max[i])
        { return false; }
      }
      return true;
    }
    public bool IsWithin(IPoint p)
    {
      return IsWithin(this, p);
    }

    /// <summary>
    /// Indicates if box is within this
    /// </summary>
    /// <returns></returns>
    public bool Contains(IBox box, IEnumerable<int> dimensionList)
    {
      return (ExceedDimension(box, dimensionList) == 0);
    }
    /// <summary>
    /// Indicates if box is within this
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    public bool Contains(IBox box)
    {
      return Contains(box, GeometryOperator.DimensionList(_dimension));
    }

    public int ExceedDimension(IBox box)
    {
      return ExceedDimension(this, box, GeometryOperator.DimensionList(_dimension));
    }
    public int ExceedDimension(IBox box, IEnumerable<int> dimensionList)
    {
      return ExceedDimension(this, box, dimensionList);
    }
    public static int ExceedDimension(IBox x, IBox y, IEnumerable<int> dimensionList)
    {
      foreach (var i in dimensionList)
      {
        if (y.Min[i] < x.Min[i])
        { return -i - 1; }
        if (y.Max[i] > x.Max[i])
        { return i + 1; }
      }
      return 0;
    }

    public static Relation Relate(IBox x, IBox y)
    {
      bool xIny = true;
      bool yInx = true;

      int maxDim = Math.Min(x.Dimension, y.Dimension);

      for (int dim = 0; dim < maxDim; dim++)
      {
        double x0 = x.Min[dim];
        double x1 = x.Max[dim];

        double y0 = y.Min[dim];
        double y1 = y.Max[dim];

        if (x1 < y0)
        { return Relation.Disjoint; }
        else if (x1 < y1)
        { yInx = false; }
        else if (x1 > y1)
        { xIny = false; }

        if (x0 > y1)
        { return Relation.Disjoint; }
        else if (x0 > y0)
        { yInx = false; }
        else if (x0 < y0)
        { xIny = false; }
      }

      if (xIny)
      { return Relation.Within; }
      if (yInx)
      { return Relation.Contains; }

      return Relation.Intersect;
    }

    public Relation RelationTo(IBox box)
    {
      return Relate(this, box);
    }

    //private IList<int> Dimensions()
    //{
    //  if (Topology == Dimension)
    //  { return Geometry.DimensionList(Dimension); }
    //  List<int> dims = new List<int>();
    //  for (int dim = 0; dim < Dimension; dim++)
    //  {
    //    if (Min[dim] != Max[dim])
    //    { dims.Add(dim); }
    //  }
    //  if (dims.Count != Topology)
    //  { throw new NotImplementedException(); }
    //  return dims;
    //}
    public bool Intersects(IGeometry other)
    {
      Relation relation = RelationTo(other.Extent);
      if (relation == Relation.Contains)
      { return true; }
      if (other is Box)
      {
        if (relation == Relation.Disjoint)
        { return false; }
        else
        { return true; }
      }

      bool intersects = GeometryOperator.Intersects(this, other);
      return intersects;
    }

    public GeometryCollection Intersection(IGeometry other)
    {
      GeometryCollection result;

      Relation relation = RelationTo(other.Extent);
      if (relation == Relation.Contains)
      {
        result = new GeometryCollection(new[] { other });
        return result;
      }
      else if (other is Box)
      {
        if (relation == Relation.Within)
        { return new GeometryCollection(new IGeometry[] { this }); }
        else if (relation == Relation.Intersect)
        {
          Box box = (Box)other;
          Point min = Point.Create(Min);
          Point max = Point.Create(Max);
          for (int dim = 0; dim < Dimension; dim++)
          {
            min[dim] = Math.Max(Min[dim], box.Min[dim]);
            max[dim] = Math.Min(Max[dim], box.Max[dim]);
          }
          result = new GeometryCollection(new IGeometry[] { new Box(min, max) });
          return result;
        }
        else if (relation == Relation.Disjoint)
        { return null; }
      }

      result = GeometryOperator.Intersection(this, other);
      return result;
    }

    public IGeometry Split(IGeometry[] border)
    {
      throw new NotImplementedException();
    }
    public override bool Equals(object obj)
    {
      if (!(obj is Box cmpr))
      { return false; }
      return _min.Equals(cmpr._min) && _max.Equals(cmpr.Max);
    }
    public override int GetHashCode()
    {
      return _min.GetHashCode();
    }

    /// <summary>
    /// set this box to the bounding box of this and box
    /// </summary>
    /// <param name="box"></param>
    /// <param name="dimension"></param>
    public void Include(IBox box, int dimension)
    {
      for (int i = 0; i < dimension; i++)
      {
        if (box.Min[i] < _min[i])
        { _min[i] = box.Min[i]; }
        if (box.Max[i] > _max[i])
        { _max[i] = box.Max[i]; }
      }
    }

    /// <summary>
    /// set this box to the bounding box of this and box
    /// </summary>
    /// <param name="box"></param>
    public void Include(IBox box)
    {
      Include(box, Math.Min(_dimension, box.Dimension));
    }

    /// <summary>
    /// set this box to the bounding box of this and point
    /// </summary>
    public void Include(IPoint point, int dimension)
    {
      if (_min == _max)
      {
        _min = Point.Create(_min);
        _max = Point.Create(_max);
      }
      for (int i = 0; i < dimension; i++)
      {
        if (point[i] < _min[i])
        { _min[i] = point[i]; }
        else if (point[i] > _max[i])
        { _max[i] = point[i]; }
      }
    }

    /// <summary>
    /// set this box to the bounding box of this and point
    /// </summary>
    public void Include(IPoint point)
    {
      Include(point, Math.Min(_dimension, point.Dimension));
    }

    #region IGeometry Members

    public int Dimension
    { get { return _dimension; } }

    public int Topology
    {
      get
      {
        if (_topology >= 0)
        { return _topology; }
        else
        { return _dimension; }
      }
    }

    public IBox Extent
    { get { return this; } }

    public BoxCollection Border
    {
      get
      {
        int nDiff = 0;
        BoxCollection border = new BoxCollection();
        for (int i = 0; i < _dimension; i++)
        {
          if (_min[i] != _max[i])
          { nDiff++; }
        }
        if (nDiff >= Topology)
        {
          System.Diagnostics.Debug.Assert(nDiff == Topology, "Error in software design assumption");
          for (int i = 0; i < _dimension; i++)
          {
            if (_min[i] != _max[i])
            {
              Point p0 = Point.Create(_min);
              Point p1 = Point.Create(_max);
              p1[i] = p0[i];
              Point p2 = Point.Create(_min);
              Point p3 = Point.Create(_max);
              p2[i] = p3[i];
              Box b0 = new Box(p0, p1) { _topology = Topology - 1 };
              Box b1 = new Box(p2, p3) { _topology = Topology - 1 };

              border.Add(b0);
              border.Add(b1);
            }
          }
        }
        else
        {
          Box b = Clone();
          b._topology = Topology - 1;
          border.Add(b);
        }
        return border;
      }
    }
    IGeometry IGeometry.Border
    { get { return Border; } }

    IGeometry IGeometry.Project(IProjection projection)
    {
      IPoint p0 = _min.Project(projection);
      IPoint p1 = _max.Project(projection);
      return Polyline.Create(new[] { p0, p1 });
    }

    #endregion

    #region ICloneable Members

    public Box Clone()
    {
      return new Box(Point.Create(_min), Point.Create(_max));
    }
    IBox IBox.Clone()
    { return Clone(); }

    #endregion
  }
  public class BoxCollection : List<Box>, IMultipartGeometry
  {
    #region IGeometry Members

    public int Dimension
    { get { return this[0].Dimension; } }

    public int Topology
    { get { return this[0].Topology; } }

    public bool IsWithin(IPoint point)
    {
      return false;
    }

    public IBox Extent
    {
      get
      {
        Box extent = this[0].Clone();
        foreach (var box in this)
        {
          extent.Include(box);
        }
        return extent;
      }
    }

    public IGeometry Border
    {
      get
      {
        GeometryCollection border = new GeometryCollection();
        foreach (var box in this)
        {
          border.Add(box.Border);
        }
        return border;
      }
    }

    public IGeometry Split(IGeometry[] border)
    {
      throw new InvalidOperationException("Cannot split");
    }

    IGeometry IGeometry.Project(IProjection projection)
    {
      GeometryCollection prj = new GeometryCollection();
      foreach (var b in this)
      {
        IBox box = b;
        prj.Add(box.Project(projection));
      }
      return prj;
    }

    public bool EqualGeometry(IGeometry other)
    { return this == other; }


    #endregion

    #region IMultipartGeometry Members

    public bool IsContinuous
    {
      get { return false; }
    }

    public bool HasSubparts
    {
      get { return true; }
    }

    public IEnumerable<Box> Subparts()
    {
      return this;
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }

    #endregion
  }
}
