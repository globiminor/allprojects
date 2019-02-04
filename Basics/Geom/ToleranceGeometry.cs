
using System.Collections.Generic;

namespace Basics.Geom
{
  public class ToleranceGeometry : IMultipartGeometry
  {
    private IGeometry _base;
    private double _tolerance;
    private IBox _extent;

    public ToleranceGeometry(IGeometry baseGeometry, double tolerance)
    {
      _base = baseGeometry;
      _tolerance = tolerance;

      IBox box = _base.Extent;
      Point min = Point.Create(box.Min);
      Point max = Point.Create(box.Max);
      int dim = _base.Dimension;
      for (int i = 0; i < dim; i++)
      {
        min[dim] -= tolerance;
        max[dim] += tolerance;
      }

      _extent = new Box(min, max);
    }

    public int Dimension
    { get { return _base.Dimension; } }
    public int Topology
    { get { return _base.Topology; } }
    public IGeometry Border
    {
      get
      {
        ToleranceGeometry border = new ToleranceGeometry(_base.Border, _tolerance);
        return border;
      }
    }

    public IBox Extent
    {
      get { return _extent; }
    }

    public bool IsWithin(IPoint point)
    {
      if (_base.IsWithin(point))
      { return true; }

      if (_base is IPoint pnt)
      {
        double dist2 = PointOp.Dist2(point, pnt);
        double tol2 = _tolerance * _tolerance;
        bool within = dist2 < tol2;
        return within;
      }

      ToleranceGeometry pntTolerance = new ToleranceGeometry(Point.CastOrWrap(point), _tolerance);
      bool intersects = GeometryOperator.Intersects(_base.Border, pntTolerance);
      return intersects;
    }
    public IGeometry Project(IProjection prj)
    {
      IGeometry basePrj = _base.Project(prj);
      IGeometry extPrj = BoxOp.ProjectRaw(_extent, prj);
      double maxExt = BoxOp.GetMaxExtent(extPrj.Extent);
      double maxBase = BoxOp.GetMaxExtent(basePrj.Extent);
      double approxTol = (maxExt - maxBase) / 2.0;

      return new ToleranceGeometry(basePrj, approxTol);
    }
    public bool EqualGeometry(IGeometry other)
    {
      if (!(other is ToleranceGeometry o))
      { return false; }

      if (o._tolerance != _tolerance)
      { return false; }

      if (o._base.EqualGeometry(_base) == false)
      { return false; }

      return true;
    }
    public bool IsContinuous
    {
      get
      {
        if (!(_base is IMultipartGeometry m))
        { return true; }

        return m.IsContinuous;
      }
    }

    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }
    public IEnumerable<IMultipartGeometry> Subparts()
    {
      if (!(_base is IMultipartGeometry m))
      {
        yield return this;
        yield break;
      }

      foreach (var subpart in m.Subparts())
      {
        ToleranceGeometry part = new ToleranceGeometry(subpart, _tolerance);
        yield return part;
      }
    }
    public bool HasSubparts
    {
      get
      {
        if (!(_base is IMultipartGeometry m))
        { return false; }
        return m.HasSubparts;
      }
    }
  }
}
