using System;
using System.Collections.Generic;

namespace Basics.Geom.Operator
{
  internal class RelParamGeometry
  {
    private class SplitBuilder
    {
      private readonly int _dim;
      private readonly int[] _pos;
      public Box GeomExtent;
      public Box ParamExtent;
      public SplitBuilder(int[] pos, Box paramExtent)
      {
        _pos = pos;
        ParamExtent = paramExtent;

        _dim = pos.Length;
      }
      public bool TryAdd(int[] p, IPoint pt)
      {
        for (int i = 0; i < _dim; i++)
        {
          int d = p[i] - _pos[i];
          if (d < 0 || d > 1)
          { return false; }
        }
        if (GeomExtent == null)
        { GeomExtent = new Box(Point.Create(pt), Point.Create(pt)); }
        else
        { GeomExtent.Include(pt); }
        return true;
      }
    }

    private readonly IRelParamGeometry _geom;
    private readonly IBox _paramRange;
    private IBox _extent;

    private OrthogonalSystem _orthoSys;
    private IPoint _center;

    private IList<IPoint> _axes;
    private IPoint _origin;

    public RelParamGeometry(IRelParamGeometry geom)
    {
      _geom = geom;
    }

    private RelParamGeometry(IRelParamGeometry geom, IBox paramRange, IBox extent)
    {
      _geom = geom;
      _paramRange = paramRange;
      _extent = extent;
    }
    private IList<IPoint> Axes
    {
      get
      {
        if (_axes == null)
        { InitSystem(); }
        return _axes;
      }
    }

    public IGeometry GetLinearGeometry()
    {
      if (Axes.Count == 0)
      { return Point.Create(Origin); }

      if (Axes.Count == 1)
      {
        Point s = Point.Create(Origin);
        Point e = s + Axes[0];
        return new Line(s, e);
      }
      { throw new NotImplementedException(); }
    }

    public LinearIntersect LinearIntersect(RelParamGeometry other)
    {
      OrthogonalSystem fullSystem = OrthoSys.Clone();
      LinearIntersect intersect =
        Operator.LinearIntersect.Create(Origin, fullSystem, other.Origin, other.Axes);
      return intersect;
    }

    private void InitSystem()
    {
      IBox paramRange;
      if (_paramRange == null)
      { paramRange = _geom.ParameterRange; }
      else
      { paramRange = _paramRange; }

      int n = paramRange.Dimension;
      IPoint paramMin = paramRange.Min;
      IPoint paramMax = paramRange.Max;
      Point param = Point.Create(paramMin);
      List<IPoint> axes = new List<IPoint>(n);
      IPoint origin = _geom.PointAt(param);
      for (int i = 0; i < n; i++)
      {
        param[i] = paramMax[i];

        IPoint corner = _geom.PointAt(param);
        Point axis = PointOp.Sub(corner, origin);
        axes.Add(axis);

        param[i] = paramMin[i];
      }
      _origin = origin;
      _axes = axes;
    }

    public OrthogonalSystem OrthoSys
    {
      get
      {
        if (_orthoSys == null)
        {
          OrthogonalSystem orthoSys = OrthogonalSystem.Create(Axes);
          _orthoSys = orthoSys;
        }
        return _orthoSys;
      }
    }

    public IRelParamGeometry BaseGeom
    {
      get { return _geom; }
    }

    public IBox Extent
    {
      get
      {
        if (_extent == null)
        {
          Box extent;
          int dim = _geom.Dimension;

          if (_paramRange == null)
          {
            IBox geomExtent = _geom.Extent;
            extent = new Box(Point.Create(geomExtent.Min), Point.Create(geomExtent.Max));
          }
          else
          {
            extent = GetExtent(_paramRange);
          }
          double normed = _geom.NormedMaxOffset;
          Point min = extent.Min;
          Point max = extent.Max;
          double maxOffset = normed * normed * PointOp.Dist2(min, max);
          for (int iDim = 0; iDim < dim; iDim++)
          {
            min[iDim] -= maxOffset;
            max[iDim] += maxOffset;
          }

          _extent = extent;
        }
        return _extent;
      }
    }

    public IPoint Origin
    {
      get
      {
        if (_origin == null)
        { InitSystem(); }
        return _origin;
      }
    }
    public IPoint Center
    {
      get
      {
        if (_center == null)
        {
          IBox extent = Extent;
          _center = 0.5 * PointOp.Add(extent.Min, extent.Max);
        }
        return _center;
      }
    }
    public IList<RelParamGeometry> Split(bool includeLinears)
    {
      if (includeLinears && BaseGeom.IsLinear)
      {
        return new RelParamGeometry[] { this };
      }
      return Split();
    }

    public List<RelParamGeometry> Split()
    {
      IBox paramRange;
      if (_paramRange == null)
      {
        paramRange = _geom.ParameterRange;
      }
      else
      {
        paramRange = _paramRange;
      }
      return GetSplits(paramRange);
    }
    private List<RelParamGeometry> GetSplits(IBox paramRange)
    {
      int dim = paramRange.Dimension;
      IPoint min = paramRange.Min;
      IPoint max = paramRange.Max;
      IPoint center = 0.5 * PointOp.Add(min, max);
      IPoint[] cs = new IPoint[] { min, center, max };

      int nCombs = 1;
      int nPts = 1;
      for (int i = 0; i < dim; i++)
      {
        nCombs *= 2;
        nPts *= 3;
      }

      List<SplitBuilder> builders = new List<SplitBuilder>(nCombs);
      for (int iComb = 0; iComb < nCombs; iComb++)
      {
        Point splitMin = Point.Create_0(dim);
        Point splitMax = Point.Create_0(dim);
        int comb = iComb;
        int[] pos = new int[dim];
        for (int iDim = 0; iDim < dim; iDim++)
        {
          if ((comb & 1) == 0)
          {
            splitMin[iDim] = min[iDim];
            splitMax[iDim] = center[iDim];
            pos[iDim] = 0;
          }
          else
          {
            splitMin[iDim] = center[iDim];
            splitMax[iDim] = max[iDim];
            pos[iDim] = 1;
          }
        }
        Box splitParamBox = new Box(splitMin, splitMax);
        builders.Add(new SplitBuilder(pos, splitParamBox));
      }

      for (int i = 0; i < nPts; i++)
      {
        int[] code = new int[dim];
        double[] coords = new double[dim];
        int j = i;
        for (int d = 0; d < dim; d++)
        {
          int r = j % 3;
          code[d] = r;
          coords[d] = cs[r][d];
          j = j / 3;
        }
        IPoint paramPt = Point.Create(coords);
        IPoint geomPt = _geom.PointAt(paramPt);

        foreach (var b in builders)
        { b.TryAdd(code, geomPt); }
      }

      List<RelParamGeometry> splits = new List<RelParamGeometry>(builders.Count);
      double normed = _geom.NormedMaxOffset;
      normed = normed * normed;
      foreach (var builder in builders)
      {
        Box extent = builder.GeomExtent;
        Point sMin = extent.Min;
        Point sMax = extent.Max;
        double off = PointOp.Dist2(sMin, sMax) * normed;
        for (int iDim = 0; iDim < sMin.Dimension; iDim++)
        {
          sMin[iDim] -= off;
          sMax[iDim] += off;
        }
        splits.Add(new RelParamGeometry(_geom, builder.ParamExtent, extent));
      }
      return splits;
    }

    [Obsolete]
    private Box GetExtent(IBox paramRange)
    {
      int dim = paramRange.Dimension;
      int nCombs = 1 >> dim;
      IPoint min = paramRange.Min;
      IPoint max = paramRange.Max;
      Box extent = null;
      for (int iComb = 0; iComb < nCombs; iComb++)
      {
        Point p = Point.Create_0(dim);
        int comb = iComb;
        for (int iDim = 0; iDim < dim; iDim++)
        {
          if ((comb & 1) == 0)
          { p[iDim] = min[iDim]; }
          else
          { p[iDim] = max[iDim]; }
          comb /= 2;
        }
        IPoint pointAt = _geom.PointAt(p);
        if (extent == null)
        { extent = new Box(Point.Create(pointAt), Point.Create(pointAt)); }
        else
        { extent.Include(pointAt); }
      }
      return extent;
    }
  }
}
