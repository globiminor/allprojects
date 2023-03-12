using Basics.Geom.Index;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Basics.Geom.Operator
{
  public interface IDistanceCalculator
  {
    int Id { get; }
    double MaxDist2 { get; }
    double MinDist2 { get; }
    IEnumerable<IDistanceCalculator> EnumSubCalcs();
  }
  public interface ILineDistanceCalculator : IDistanceCalculator
  {
    Line Line { get; }
  }

  public class DistanceCalculator : IDistanceCalculator
  {
    public class Distance
    {
      public Distance(IReadOnlyCollection<int> dimensions, double tolerance = 0.001, double max2 = double.MaxValue)
      {
        Dimensions = dimensions;

        if (max2 < double.MaxValue)
        {
          _max2 = max2;
          _max = Math.Sqrt(max2);
        }
        else
        {
          _max2 = double.MaxValue;
          _max = double.MaxValue;
        }

        _tolerance = tolerance;
      }

      private double _tolerance;
      private double _max2;
      private double _max;
      public IReadOnlyCollection<int> Dimensions { get; }

      public double Tolerance => _tolerance;
      public double Max2 => _max2;
      public double Max => _max;
      public void SetMax2(double max2)
      {
        _max2 = max2;
        _max = Math.Sqrt(_max2);
      }
      public void SetMax(double max)
      {
        _max = max;
        _max2 = max * max;
      }
      public int Id { get; set; }
    }

    private class Comparer : IComparer<IDistanceCalculator>
    {
      public int Compare(IDistanceCalculator x, IDistanceCalculator y)
      {
        if (x == y)
        { return 0; }

        int d;

        {
          d = x.MinDist2.CompareTo(y.MinDist2);
          if (d != 0)
          { return d; }

          d = x.MaxDist2.CompareTo(y.MaxDist2);
          if (d != 0)
          { return d; }
        }

        return y.Id.CompareTo(x.Id);
      }
    }

    private class PointCalculator : ILineDistanceCalculator
    {
      private readonly IPoint _p;
      private readonly RelParamGeometry _paramGeometry;
      private readonly Distance _common;

      private readonly double _maxOffset;

      private readonly double _maxDist2;
      private readonly double _minDist2;
      private readonly Line _line;
      public int Id { get; }
      public PointCalculator(IPoint p, RelParamGeometry paramGeometry, Distance common)
      {
        _p = p;
        _paramGeometry = paramGeometry;
        Id = common.Id++;
        _common = common;

        double l2 = PointOp.Dist2(_paramGeometry.Extent.Max, _paramGeometry.Extent.Min);
        double no = _paramGeometry.BaseGeom.NormedMaxOffset;
        _maxOffset = no * no * l2;

        if (_maxOffset < common.Tolerance)
        { _maxOffset = 0; }

        IPoint c = GetClosestLinearPoint(_p, _paramGeometry);
        double d = Math.Sqrt(PointOp.Dist2(_p, c, _common.Dimensions));
        double maxDist = d + _maxOffset;
        _maxDist2 = maxDist * maxDist;
        double min = Math.Max(0, d - _maxOffset);
        _minDist2 = min * min;

        _line = new Line(_p, c);
      }

      public override string ToString()
      {
        return $"{Id}; Pnt {_paramGeometry.BaseGeom.GetType().Name}; {MinDist2}; {MaxDist2}";
      }
      public double MaxDist2 => _maxDist2;
      public double MinDist2 => _minDist2;
      public Line Line => _line;

      IEnumerable<IDistanceCalculator> IDistanceCalculator.EnumSubCalcs() => EnumSubCalcs();

      public IEnumerable<PointCalculator> EnumSubCalcs()
      {
        if (_paramGeometry.BaseGeom.IsLinear)
        {
          yield return this;
          yield break;
        }
        foreach (var part in _paramGeometry.Split())
        {
          PointCalculator subCalc = new PointCalculator(_p, part, _common);
          yield return subCalc;
        }
      }
    }

    private class RelParamCalculator : ILineDistanceCalculator
    {
      private readonly RelParamGeometry _x;
      private readonly RelParamGeometry _y;
      private readonly Distance _common;

      private readonly double _minDist2;
      private readonly double _maxDist2;
      private readonly Line _line;

      private readonly double _maxOffsetX;
      private readonly double _maxOffsetY;
      public int Id { get; }
      public RelParamCalculator(RelParamGeometry x, RelParamGeometry y, Distance common)
      {
        _x = x;
        _y = y;
        Id = common.Id++;
        _common = common;

        _maxOffsetX = GetMaxOffset(x);
        _maxOffsetY = GetMaxOffset(y);

        _line = GetClosestLinearLine(_x, _y, common.Dimensions);
        double d = _line.Length(common.Dimensions);
        double maxOffset = _maxOffsetX + _maxOffsetY;

        if (maxOffset < _common.Tolerance)
        { maxOffset = 0; }

        double maxDist = d + maxOffset;
        _maxDist2 = maxDist * maxDist;
        double min = Math.Max(0, d - maxOffset);
        _minDist2 = min * min;
      }

      public double MaxDist2 => _maxDist2;
      public double MinDist2 => _minDist2;
      public Line Line => _line;

      IEnumerable<IDistanceCalculator> IDistanceCalculator.EnumSubCalcs() => EnumSubCalcs();
      private IEnumerable<RelParamCalculator> EnumSubCalcs()
      {
        if (_maxOffsetX + _maxOffsetY < _common.Tolerance)
        {
          yield return this;
          yield break;
        }
        if (_x.BaseGeom.IsLinear)
        {
          foreach (var part in _y.Split())
          {
            yield return new RelParamCalculator(_x, part, _common);
          }
          yield break;
        }
        if (_y.BaseGeom.IsLinear)
        {
          foreach (var part in _x.Split())
          {
            yield return new RelParamCalculator(_y, part, _common);
          }
          yield break;
        }
        RelParamGeometry x;
        RelParamGeometry y;
        if (BoxOp.GetMaxExtent(_x.Extent) >= BoxOp.GetMaxExtent(_y.Extent))
        {
          x = _x;
          y = _y;
        }
        else
        {
          x = _y;
          y = _x;
        }

        foreach (var part in x.Split())
        {
          yield return new RelParamCalculator(y, part, _common);
        }

      }

      private double GetMaxOffset(RelParamGeometry x)
      {
        if (x.BaseGeom.IsLinear)
        {
          return 0;
        }

        double l2 = PointOp.Dist2(x.Extent.Max, x.Extent.Min);
        double no = x.BaseGeom.NormedMaxOffset;
        return no * no * l2;
      }

      public override string ToString()
      {
        return $"RelParam {Id}; {_x.BaseGeom.GetType().Name}; {_y.BaseGeom.GetType().Name}; {_minDist2}; {_maxDist2}";
      }

    }

    private readonly IGeometry _x;
    private readonly IGeometry _y;
    private readonly Distance _common;

    private readonly double _min2;
    private readonly double _max2;

    private SortedList<IDistanceCalculator, IEnumerator<IDistanceCalculator>> _sortedEnumerators;
    private SortedSet<ILineDistanceCalculator> _sortedReduceds;

    public int Id { get; }


    public DistanceCalculator(IGeometry x, IGeometry y, Distance common)
    {
      _x = x;
      _y = y;
      Id = common.Id++;
      _common = common;

      _max2 = BoxOp.GetMaxDist(_x.Extent, _y.Extent, _common.Dimensions).OrigDist2();
      _min2 = BoxOp.GetMinDist(_x.Extent, _y.Extent, _common.Dimensions).OrigDist2();
    }
    public Line GetClosestLine()
    {
      while (Improve())
      { }

      foreach (var calc in _sortedReduceds)
      {
        Line closest = calc.Line;
        return closest;
      }

      return null;
    }

    private bool Improve()
    {
      if (_sortedEnumerators == null)
      {
        _sortedEnumerators = new SortedList<IDistanceCalculator, IEnumerator<IDistanceCalculator>>(new Comparer());
        _sortedEnumerators.Add(this, EnumSubCalcs().GetEnumerator());

        _sortedReduceds = new SortedSet<ILineDistanceCalculator>(new Comparer());
      }

      foreach (var pair in _sortedEnumerators)
      {
        IDistanceCalculator calc = pair.Key;
        double min2 = calc.MinDist2;
        if (min2 > _common.Max2)
        {
          _sortedEnumerators.Clear();
          return false;
        }

        IEnumerator<IDistanceCalculator> enumSubCalcs = pair.Value;
        while (enumSubCalcs.MoveNext())
        {
          IDistanceCalculator subCalc = enumSubCalcs.Current;
          if (subCalc.MinDist2 > _common.Max2)
          { continue; }

          bool equalCalc = (subCalc == calc);
          if (equalCalc)
          {
            if (!(calc is ILineDistanceCalculator lineCalc))
            { throw new InvalidOperationException($"expected {nameof(ILineDistanceCalculator)}, got {calc.GetType()}"); }
            _sortedReduceds.Add(lineCalc);
            //remark: if equalCalc there is only 1 element
            _sortedEnumerators.Remove(calc);
            return true;
          }

          if (_common.Max2 > subCalc.MaxDist2)
          { _common.SetMax2(subCalc.MaxDist2); }

          _sortedEnumerators.Add(subCalc, subCalc.EnumSubCalcs().GetEnumerator());
          if (subCalc.MinDist2 <= calc.MinDist2)
          {
            return true;
          }
        }
        _sortedEnumerators.Remove(calc);
        return true;
      }
      return false;
    }

    IEnumerable<IDistanceCalculator> IDistanceCalculator.EnumSubCalcs() => EnumSubCalcs();
    private IEnumerable<IDistanceCalculator> EnumSubCalcs()
    {
      IGeometry x;
      IGeometry y;
      if (BoxOp.GetMaxExtent(_y.Extent) > BoxOp.GetMaxExtent(_x.Extent))
      {
        x = _y;
        y = _x;
      }
      else
      {
        x = _x;
        y = _y;
      }

      if (GeometryOperator.IsMultipart(x))
      {
        IMultipartGeometry xParts = (IMultipartGeometry)x;
        foreach (var subCalc in EnumMultipartCalcs(y, xParts))
        { yield return subCalc; }
        yield break;
      }

      if (GeometryOperator.IsMultipart(y))
      {
        IMultipartGeometry yParts = (IMultipartGeometry)y;
        foreach (var subCalc in EnumMultipartCalcs(x, yParts))
        { yield return subCalc; }
        yield break;
      }

      if (y is IPoint py)
      {
        yield return new PointCalculator(py, new RelParamGeometry((IRelParamGeometry)x), _common);
        yield break;
      }
      if (x is IPoint px)
      {
        yield return new PointCalculator(px, new RelParamGeometry((IRelParamGeometry)y), _common);
        yield break;
      }

      IRelationGeometry rx = (IRelationGeometry)x;
      IRelationGeometry ry = (IRelationGeometry)y;
      if (rx.IsLinear && ry.IsLinear)
      {
        if (_x.Topology >= _y.Topology)
        {
          if (_x.IsWithin(GeometryOperator.GetFirstPoint(_y)))
          {
            yield return GetWithinCalc();
            yield break;
          }
          yield return new DistanceCalculator(_y, _x.Border, _common);
        }
        if (_y.Topology > _x.Topology
          || (_y.Topology == _x.Topology && _y.Dimension > _y.Topology))
        {
          if (_y.IsWithin(GeometryOperator.GetFirstPoint(_x)))
          {
            yield return GetWithinCalc();
            yield break;
          }
          yield return new DistanceCalculator(_x, _y.Border, _common);
        }

        yield break;
      }

      if (!(rx is IRelParamGeometry rpx))
      { throw new NotImplementedException(); }
      if (!(ry is IRelParamGeometry rpy))
      { throw new NotImplementedException(); }

      yield return new RelParamCalculator(new RelParamGeometry(rpx), new RelParamGeometry(rpy), _common);
    }

    private DistanceCalculator GetWithinCalc()
    {
      _common.SetMax2(0);
      throw new NotImplementedException();
    }

    private class TileGeoms : IMultipartGeometry, IEnumerable<IGeometry>
    {
      private readonly BoxTile _startTile;
      private readonly Box _startBox;

      private readonly Distance _common;

      private IBox _neighborBox;
      private IBox _searchBox;

      private double _m;
      public TileGeoms(BoxTile startTile, Box startBox, Distance common)
      {
        _startTile = startTile;
        _startBox = startBox;

        _common = common;
        _m = double.MaxValue;
      }

      bool IMultipartGeometry.IsContinuous => false;
      bool IMultipartGeometry.HasSubparts => true;
      IEnumerable<IGeometry> IMultipartGeometry.Subparts() => this;
      IEnumerator<IGeometry> IEnumerable<IGeometry>.GetEnumerator() => EnumSubs(null).GetEnumerator();
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => EnumSubs(null).GetEnumerator();

      bool IGeometry.IsWithin(IPoint p) => false;
      IBox IGeometry.Extent => _startTile.UsedExtent;
      IGeometry IGeometry.Border => throw new NotImplementedException();
      IGeometry IGeometry.Project(IProjection projection) => throw new NotImplementedException();
      bool IGeometry.EqualGeometry(IGeometry other) => this == other;
      int IDimension.Dimension => _startBox.Dimension;
      int ITopology.Topology => throw new NotImplementedException();

      private void SetMinDist(double minDist)
      {
        if (_neighborBox == null)
        {
          _m = -1;
          return;
        }

        Point pd = Point.Create_0(_neighborBox.Dimension);
        for (int i = 0; i < _neighborBox.Dimension; i++)
        { pd[i] = minDist; }
        _m = minDist;
        _searchBox = new Box(_neighborBox.Min - pd, _neighborBox.Max + pd);
      }

      public IEnumerable<IGeometry> EnumSubs(IBox neighbor, double maxExtent = -1)
      {
        if (maxExtent < 0 && neighbor != null)
        {
          double s = BoxOp.GetMaxExtent(_startTile.UsedExtent);
          maxExtent = Math.Max(s * 0.4, BoxOp.GetMaxExtent(neighbor) * (2.0 / 3.0));
          maxExtent = Math.Min(s * 0.9, maxExtent); // avoid that first tile is returned as a result
        }

        _m = double.MaxValue;
        _neighborBox = neighbor;
        return EnumSubs(_startTile, _startBox, maxExtent);
      }
      private IEnumerable<IGeometry> EnumSubs(BoxTile startTile, Box startBox, double maxExtent)
      {
        if (_m > _common.Max)
        { SetMinDist(_common.Max); }

        if (_searchBox != null && !BoxOp.Intersects(startBox, _searchBox))
        {
          yield break;
        }
        if (_searchBox != null && BoxOp.GetMinDist(_neighborBox, startBox).OrigDist2() > _common.Max2)
        {
          yield break;
        }

        {
          IBox e = startTile.UsedExtent;
          if (e != null && BoxOp.GetMaxExtent(e) < maxExtent)
          {
            yield return new TileGeoms(startTile, startBox, _common);
            yield break;
          }
        }

        int splitDim = startTile.SplitDimension;
        var children = (startTile.Child1?.MinInParentSplitDim < _neighborBox.Min[splitDim])
          ? new[] { startTile.Child1, startTile.Child0 }
          : new[] { startTile.Child0, startTile.Child1 };
        foreach (var child in children)
        {
          if (child == null)
          { continue; }

          Box childBox = startBox.Clone();
          childBox.Min[splitDim] = child.MinInParentSplitDim;
          childBox.Max[splitDim] = child.MaxInParentSplitDim;

          foreach (var sub in EnumSubs(child, childBox, maxExtent))
          { yield return sub; }
        }

        foreach (var entry in startTile.EnumElems())
        {
          yield return (IGeometry)entry.BaseValue;
        }

      }
    }

    private IEnumerable<DistanceCalculator> EnumMultipartCalcs(IGeometry g, IMultipartGeometry mp)
    {
      IEnumerable<IGeometry> parts = mp.Subparts();
      if (parts is BoxTree bt)
      {
        TileGeoms tileGeoms = new TileGeoms(bt.MainTile, bt.MainBox, _common);
        parts = tileGeoms.EnumSubs(g.Extent);
      }
      else if (parts is TileGeoms tg)
      {
        parts = tg.EnumSubs(g.Extent);
      }

      return parts.Select(p => new DistanceCalculator(p, g, _common));
    }

    private IEnumerable<DistanceCalculator> EnumMultipartCalcsBoxTree(IGeometry g, IMultipartGeometry mp)
    {
      IEnumerable<IGeometry> parts = mp.Subparts();
      BoxTreeMinDistSearch search = new BoxTreeMinDistSearch(g.Extent);
      if (_common.Max2 < double.MaxValue)
      { search.SetMinDist2(_common.Max2); }
      if (parts is BoxTree bt)
      {
        parts = bt.Search(searcher: search).Select(te => (IGeometry)te.BaseValue);
      }

      double max2 = _common.Max2;
      foreach (var part in parts)
      {
        if (max2 > _common.Max2)
        {
          search.SetMinDist2(_common.Max2);
          max2 = _common.Max2;
        }
        yield return new DistanceCalculator(part, g, _common);
      }
    }

    public override string ToString()
    {
      return $"{Id}; {_x.GetType().Name}; {_y.GetType().Name}; {_min2}; {_max2}";
    }
    public double MaxDist2 => _max2;
    public double MinDist2 => _min2;

    public static IPoint GetClosestPoint(IPoint p, IGeometry g, IReadOnlyList<int> dimensions = null, double minD2 = double.MaxValue)
    {
      dimensions = dimensions ?? GeometryOperator.GetDimensionsArray(p, g);
      DistanceCalculator distCalc = new DistanceCalculator(Point.CastOrWrap(p), g, new Distance(dimensions, max2: minD2));
      Line l = distCalc.GetClosestLine();
      if (l == null)
      { return null; }

      if (PointOp.Dist2(l.Start, p) > PointOp.Dist2(l.End, p))
      { return l.Start; }
      else
      { return l.End; }
    }

    public static IPoint GetClosestPoint_v0(IPoint p, IGeometry g)
    {
      if (GeometryOperator.IsMultipart(g))
      {
        IMultipartGeometry xMulti = (IMultipartGeometry)g;
        return ClosestPoint(p, xMulti);
      }
      if (g is IPoint geomP)
      { return geomP; }
      if (!(g is IRelParamGeometry pg))
      { throw new InvalidOperationException("IGeometry is not of type " + typeof(IRelParamGeometry)); }

      if (pg.IsWithin(p))
      { return null; }

      if (pg.IsLinear)
      {
        RelParamGeometry lg = new RelParamGeometry(pg);
        IPoint pp = PointOp.Sub(p, lg.Origin);
        Axis a = lg.OrthoSys.GetOrthogonal(pp);
        bool maybeWithin = true;
        foreach (var f in a.Factors)
        {
          if (f < 0 || f > 1)
          {
            maybeWithin = false;
            break;
          }
        }
        if (maybeWithin)
        {
          IPoint closest = PointOp.Sub(p, a.Point);
          if (a.Factors.Length <= 1)
          {
            return closest;
          }
          if (g.IsWithin(closest))
          { return closest; }
        }
        if (a.Factors.Length == 0)
        { return Point.Create(lg.Origin); }
        if (a.Factors.Length == 1)
        {
          return a.Factors[0] < 0
            ? Point.Create(lg.Origin)
            : lg.Origin + lg.OrthoSys.Axes[0].Point;
        }
        return ClosestPoint(p, g.Border);
      }

      double minDist = double.MaxValue;
      return GetClosestPoint(p, new RelParamGeometry(pg), ref minDist);
    }

    private static IPoint GetClosestPoint(IPoint p, RelParamGeometry lg, ref double minDist)
    {
      double l2 = PointOp.Dist2(lg.Extent.Max, lg.Extent.Min);
      double no = lg.BaseGeom.NormedMaxOffset;
      double maxOffset = no * no * l2;
      if (maxOffset > 1e-8)
      {
        List<Tuple<double, RelParamGeometry>> candidates = new List<Tuple<double, RelParamGeometry>>();
        foreach (var part in lg.Split())
        {
          IPoint c = GetClosestLinearPoint(p, part);
          double d = Math.Sqrt(PointOp.Dist2(c, p));
          candidates.Add(new Tuple<double, RelParamGeometry>(d, part));
        }
        candidates.Sort((x, y) => x.Item1.CompareTo(y.Item1));

        IPoint closest = null;
        foreach (var candidate in candidates)
        {
          if (candidate.Item1 - maxOffset > minDist)
          { continue; }

          IPoint c = GetClosestPoint(p, candidate.Item2, ref minDist);
          if (c == null)
          { continue; }
          double d = Math.Sqrt(PointOp.Dist2(c, p));
          if (d <= minDist)
          {
            minDist = d;
            closest = c;
          }

        }
        return closest;
      }
      return GetClosestLinearPoint(p, lg);
    }

    private static IPoint GetClosestLinearPoint(IPoint p, RelParamGeometry lg)
    {
      IPoint pp = PointOp.Sub(p, lg.Origin);
      Axis a = lg.OrthoSys.GetOrthogonal(pp);

      int nAx = a.Factors.Length;
      IPoint closest = lg.Origin;
      for (int iAx = 0; iAx < nAx; iAx++)
      {
        double f = a.Factors[iAx];
        if (f < 0) { continue; }
        f = f > 1 ? 1 : f;
        closest = closest + PointOp.Scale(f, lg.OrthoSys.Axes[iAx].Point);
      }
      return closest;
    }

    private static Line GetClosestLinearLine(RelParamGeometry x, RelParamGeometry y, IReadOnlyCollection<int> dimensions)
    {
      IGeometry xg = x.GetLinearGeometry();
      IGeometry yg = y.GetLinearGeometry();

      DistanceCalculator linCalc = new DistanceCalculator(xg, yg, new Distance(dimensions));
      Line l = linCalc.GetClosestLine();

      return l;
    }


    private static IPoint ClosestPoint(IPoint p, IMultipartGeometry xMulti)
    {
      IPoint pMin = null;
      double d2 = 0;
      foreach (var part in xMulti.Subparts())
      {
        if (pMin == null)
        {
          pMin = ClosestPoint(p, part);
          d2 = PointOp.Dist2(p, pMin);
        }
        else
        {
          IPoint extent = ClosestPoint(p, part.Extent);
          if (PointOp.Dist2(extent, p) < d2)
          {
            IPoint pPart = ClosestPoint(p, part);
            double dPart2 = PointOp.Dist2(pPart, p);

            if (dPart2 < d2)
            {
              pMin = pPart;
              d2 = dPart2;
            }
          }
        }
      }
      return pMin;
    }

    public static IPoint ClosestPoint(IPoint p, IGeometry g)
    {
      if (GeometryOperator.IsMultipart(g))
      {
        IMultipartGeometry xMulti = (IMultipartGeometry)g;
        return ClosestPoint(p, xMulti);
      }
      if (!(g is IRelParamGeometry pg))
      { throw new InvalidOperationException("IGeometry is not of type " + typeof(IRelParamGeometry)); }

      if (pg.IsWithin(p))
      { return null; }

      if (pg.IsLinear)
      {
        RelParamGeometry lg = new RelParamGeometry(pg);
        IPoint pp = PointOp.Sub(p, lg.Origin);
        Axis a = lg.OrthoSys.GetOrthogonal(pp);
        bool maybeWithin = true;
        foreach (var f in a.Factors)
        {
          if (f < 0 || f > 1)
          {
            maybeWithin = false;
            break;
          }
        }
        if (maybeWithin)
        {
          IPoint closest = PointOp.Sub(p, a.Point);
          if (a.Factors.Length <= 1)
          {
            return closest;
          }
          if (g.IsWithin(closest))
          { return closest; }
        }
        if (a.Factors.Length == 0)
        { return Point.Create(lg.Origin); }
        if (a.Factors.Length == 1)
        {
          return a.Factors[0] < 0
            ? Point.Create(lg.Origin)
            : lg.Origin + lg.OrthoSys.Axes[0].Point;
        }
        return ClosestPoint(p, g.Border);
      }

      double minDist = double.MaxValue;
      return GetClosestPoint(p, new RelParamGeometry(pg), ref minDist);
    }

    private static Point ClosestPoint(IPoint p, IBox b)
    {
      int dim = Math.Min(p.Dimension, b.Dimension);
      Point closest = Point.Create_0(dim);
      for (int i = 0; i < dim; i++)
      {
        double t;
        if (p[i] < b.Min[i])
        { t = b.Min[i]; }
        else if (p[i] < b.Max[i])
        { t = p[i]; }
        else
        { t = b.Max[i]; }
        closest[i] = t;
      }
      return closest;
    }



    private class DistancePair
    {
      private readonly IGeometry _x;
      private readonly IGeometry _y;
      private double _rawMinDist2;
      private double _rawMaxDist2;

      public DistancePair(IGeometry x, IGeometry y, double rawMinDist2, double rawMaxDist2)
      {
        _x = x;
        _y = y;
        _rawMinDist2 = rawMinDist2;
        _rawMaxDist2 = rawMaxDist2;
      }
      public double RawMinDist2 => _rawMinDist2;
      public double RawMaxDist2 => _rawMaxDist2;
    }

  }
}
