using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public class Box : BoxCore
  {
    private Point _min;
    private Point _max;
    private readonly int _dimension;
    private int _topology = -1;

    public static BoxCore CastOrWrap(IBox box)
    {
      return box as BoxCore ?? new BoxWrap(box);
    }
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

    public new Point Min => _min;
    public new Point Max => _max;

    public new int Topology
    {
      get
      {
        if (_topology >= 0)
        { return _topology; }
        else
        { return _dimension; }
      }
      internal set { _topology = value; }
    }

    protected override IPoint GetMin() => _min;
    protected override IPoint GetMax() => _max;
    protected override int GetDimension() => _dimension;
    protected override int GetTopology() => Topology;

    /// <summary>
    /// set this box to the bounding box of this and box
    /// </summary>
    /// <param name="box"></param>
    public Box Include(IBox box, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(this, box);
      foreach (int i in dimensions)
      {
        if (box.Min[i] < _min[i])
        { _min[i] = box.Min[i]; }
        if (box.Max[i] > _max[i])
        { _max[i] = box.Max[i]; }
      }
      return this;
    }

    /// <summary>
    /// set this box to the bounding box of this and point
    /// </summary>
    public void Include(IPoint point, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(this, point);
      if (_min == _max)
      {
        _min = Point.Create(_min);
        _max = Point.Create(_max);
      }
      foreach (int i in dimensions)
      {
        if (point[i] < _min[i])
        { _min[i] = point[i]; }
        else if (point[i] > _max[i])
        { _max[i] = point[i]; }
      }
    }

  }
  public class BoxWrap : BoxCore
  {
    private readonly IBox _box;
    public BoxWrap(IBox box)
    {
      _box = box;
    }
    protected override IPoint GetMin() => _box.Min;
    protected override IPoint GetMax() => _box.Max;
    protected override int GetDimension() => _box.Dimension;
    protected override int GetTopology() => _box.Topology;
  }

  public abstract class BoxCore : IBox, IGeometry
  {
    public bool EqualGeometry(IGeometry other)
    { return this == other; }

    protected abstract IPoint GetMin();
    protected abstract IPoint GetMax();
    protected abstract int GetDimension();
    protected abstract int GetTopology();
    // properties
    public IPoint Min => GetMin();
    public IPoint Max => GetMax();

    public double GetMaxExtent(IEnumerable<int> dimensions = null) => BoxOp.GetMaxExtent(this, dimensions);

    public double Dist2(IBox box, IEnumerable<int> dimensions = null) => BoxOp.Dist2(this, box, dimensions);

    public bool IsWithin(IPoint p, IEnumerable<int> dimensions = null) => BoxOp.IsWithin(this, p, dimensions);

    /// <summary>
    /// Indicates if box is within this
    /// </summary>
    /// <returns></returns>
    public bool Contains(IBox box, IEnumerable<int> dimensionList = null) => BoxOp.ExceedDimension(this, box, dimensionList) == 0;
    public int ExceedDimension(IBox box, IEnumerable<int> dimensionList = null) => BoxOp.ExceedDimension(this, box, dimensionList);

    public Relation RelationTo(IBox box, IEnumerable<int> dimensions = null) => BoxOp.GetRelation(this, box, dimensions);

    public GeometryCollection Intersection(IGeometry other)
    {
      GeometryCollection result;

      Relation relation = RelationTo(other.Extent);
      if (relation == Relation.Contains)
      {
        result = new GeometryCollection(new[] { other });
        return result;
      }
      else if (other is IBox)
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
    
    [Obsolete("use BoxOp.EqualGeometry")]
    public override bool Equals(object obj)
    {
      return BoxOp.EqualGeometry(this, obj as IBox);

      if (!(obj is Box cmpr))
      { return false; }
      return PointOp.EqualGeometry(Min, cmpr.Min) && PointOp.EqualGeometry(Max, cmpr.Max);
    }
    public override int GetHashCode()
    {
      return Min.GetHashCode();
    }

    #region IGeometry Members

    public int Dimension => GetDimension();

    public int Topology => GetTopology();

    public IBox Extent
    { get { return this; } }

    IGeometry IGeometry.Border => Border;
    public BoxCollection Border => BoxOp.GetBorder(this);

    bool IGeometry.IsWithin(IPoint point) => BoxOp.IsWithin(this, point);
    IGeometry IGeometry.Project(IProjection projection) => BoxOp.ProjectRaw(this, projection);
    #endregion

    public Box Clone() => BoxOp.Clone(this);
  }

  public static class BoxOp
  {
    public static Box Clone(IBox box)
    {
      return new Box(Point.Create(box.Min), Point.Create(box.Max));
    }

    public static double GetMaxExtent(IBox box, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(box);
      double dMax = 0;
      foreach (int i in dimensions)
      {
        double dExtent = box.Max[i] - box.Min[i];
        if (dExtent > dMax)
        { dMax = dExtent; }
      }
      return dMax;
    }

    public static double Dist2(IBox x, IBox box, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(x, box);

      double d2 = 0;
      foreach (var i in dimensions)
      {
        double d;
        if (x.Max[i] < box.Min[i])
        { d = box.Min[i] - x.Max[i]; }
        else if (x.Min[i] > box.Max[i])
        { d = x.Min[i] - box.Max[i]; }
        else
        { continue; }

        d2 += d * d;
      }
      return d2;
    }

    public static bool Contains(IBox x, IBox y, IEnumerable<int> dimensionList = null) => BoxOp.ExceedDimension(x, y, dimensionList) == 0;

    public static int ExceedDimension(IBox x, IBox y, IEnumerable<int> dimensionList = null)
    {
      dimensionList = dimensionList ?? GeometryOperator.GetDimensions(x, y);
      foreach (var i in dimensionList)
      {
        if (y.Min[i] < x.Min[i])
        { return -i - 1; }
        if (y.Max[i] > x.Max[i])
        { return i + 1; }
      }
      return 0;
    }

    public static Relation GetRelation(IBox x, IBox y, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(x, y);
      bool xIny = true;
      bool yInx = true;


      foreach (var dim in dimensions)
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

    public static bool EqualGeometry(IBox x, IBox y, ICollection<int> dimensions = null)
    {
      if (x == y)
      { return true; }
      if (x == null)
      { return false; }

      return PointOp.EqualGeometry(x.Min, y.Min, dimensions) && PointOp.EqualGeometry(x.Max, y.Max, dimensions);
    }

    public static bool Intersects(IBox box, IBox other)
    {
      Relation relation = GetRelation(box, other);
      return relation != Relation.Disjoint;
    }

    public static bool Intersects(IBox box, IGeometry other)
    {
      Relation relation = GetRelation(box, other.Extent);
      if (relation == Relation.Contains)
      { return true; }
      if (other is IBox)
      {
        return relation != Relation.Disjoint;
      }
      if (relation == Relation.Disjoint)
      { return false; }

      bool intersects = GeometryOperator.Intersects(Box.CastOrWrap(box), other);
      return intersects;
    }

    public static bool IsWithin(IBox box, IPoint p, IEnumerable<int> dimensions = null)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensions(box, p);
      foreach (int i in dimensions)
      {
        if (p[i] < box.Min[i])
        { return false; }
        if (p[i] > box.Max[i])
        { return false; }
      }
      return true;
    }

    public static Line ProjectRaw(IBox box, IProjection projection)
    {
      if (box == null) return null;

      return new Line(box.Min, box.Max).Project(projection);
    }

    public static BoxCollection GetBorder(IBox box)
    {
      int nDiff = 0;
      BoxCollection border = new BoxCollection();
      for (int i = 0; i < box.Dimension; i++)
      {
        if (box.Min[i] != box.Max[i])
        { nDiff++; }
      }
      if (nDiff >= box.Topology)
      {
        System.Diagnostics.Debug.Assert(nDiff == box.Topology, "Error in software design assumption");
        for (int i = 0; i < box.Dimension; i++)
        {
          if (box.Min[i] != box.Max[i])
          {
            Point p0 = Point.Create(box.Min);
            Point p1 = Point.Create(box.Max);
            p1[i] = p0[i];
            Point p2 = Point.Create(box.Min);
            Point p3 = Point.Create(box.Max);
            p2[i] = p3[i];
            Box b0 = new Box(p0, p1) { Topology = box.Topology - 1 };
            Box b1 = new Box(p2, p3) { Topology = box.Topology - 1 };

            border.Add(b0);
            border.Add(b1);
          }
        }
      }
      else
      {
        Box b = Clone(box);
        b.Topology = box.Topology - 1;
        border.Add(b);
      }
      return border;
    }

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
        prj.Add(BoxOp.ProjectRaw(box, projection));
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
