using System;
using System.Collections.Generic;
using System.Linq;
using Basics.Geom.Index;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Area.
  /// </summary>
  public class Area : Geometry, IGeometry, ISimpleArea
  {
    private readonly PolylineCollection _border;
    BoxTree<Polyline> _spatialIndex;

    public Area()
      : this(new List<Polyline>())
    { }
    public Area(Polyline border)
      : this(new[] { border })
    { }
    public Area(IEnumerable<Polyline> border)
    {
      _border = new PolylineCollection();
      _border.AddRange(border);
    }
    #region IGeometry Members

    IReadOnlyList<ISimplePolyline> ISimpleArea.Border => Lines;
    protected virtual PolylineCollection Lines
    {
      get { return _border; }
    }
    public override int Dimension
    {
      get { return _border.Dimension; }
    }

    public override int Topology
    {
      get { return 2; }
    }

    private BoxTree<Polyline> SpatialIndex
    {
      get
      {
        if (_spatialIndex == null)
        {
          _spatialIndex = new BoxTree<Polyline>(2);
          _spatialIndex.InitSize(new IGeometry[] { Box.CastOrWrap(Extent) });
          foreach (var line in Lines)
          { _spatialIndex.Add(line.Extent, line); }
        }
        return _spatialIndex;
      }
    }

    public override bool IsWithin(IPoint p)
    {
      Point pDown = Point.Create(p);

      int i = 0;

      foreach (var border in Lines)
      {
        BoxRelation rel = BoxOp.GetRelation(border.Extent, Point.CastOrWrap(p).Extent);
        if (rel == BoxRelation.Disjoint)
        { continue; }

        if (PointOp.EqualGeometry(border.Points[0], border.Points.Last()) == false)
        { border.Add(border.Points[0]); }

        double? xPre = null;
        foreach (var curve in border.EnumSegments())
        {
          IBox extent = curve.Extent;

          if (VerifyExtent(p, extent) == false)
          { continue; }

          int iNew = Intersects(p, pDown, curve, extent, ref xPre);
          if (iNew < 0) // Touch
          { return true; }
          i += iNew;
        }
      }
      if ((i & 1) == 1)
      { return true; }
      else
      { return false; }
    }

    private int Intersects(IPoint isInside, Point isInsideDown, ISegment curve, IBox extent,
      ref double? xPre)
    {
      if (curve is IMultipartGeometry multi && multi.HasSubparts)
      {
        int n = 0;
        foreach (var part in multi.Subparts())
        {
          IBox partExtent = part.Extent;
          ISegment subCurve = (ISegment)part;

          if (VerifyExtent(isInside, partExtent) == false)
          { continue; }

          int nNew = Intersects(isInside, isInsideDown, subCurve, partExtent, ref xPre);
          if (nNew < 0) // Touch
          { return nNew; }
          else
          { n += nNew; }
        }
        return n;
      }

      // Remark: Assert.IsTrue(isInside[dim.Y] >= extent.Min[dim.Y]);

      double x = isInside.X;
      if (extent.Min.X == x)
      {
        if (extent.Max.X == x)
        { // senkrechte Linie
          if (extent.Max.Y >= isInside.Y)
          {
            xPre = null;
            return -1;
          } // Touch
        }
        return GetFromPreviousSide(isInside, curve, ref xPre); // TODO
      }
      else if (x == extent.Max.X)
      {
        return GetFromPreviousSide(isInside, curve, ref xPre);
      }
      else // (extent.Min[dim.X] < x && x < extent.Max[dim.X])
      {
        if (isInside.Y > extent.Max.Y)
        { return 1; }

        isInsideDown.Y = extent.Min.Y;

        GeometryCollection intersects = GeometryOperator.Intersection(curve, new Line(isInside, isInsideDown));
        if (intersects != null)
        {
          if (intersects.Count > 1)
          { throw new InvalidOperationException("Not expecting multiple intersects"); }
          return intersects.Count;
        }
        else
        { return 0; }
      }
    }

    private int GetFromPreviousSide(IPoint isInside, ISegment curve, ref double? xPre)
    {
      IPoint p, q;
      double x = isInside.X;

      if (curve.Start.X == x)
      {
        p = curve.Start;
        q = curve.End;
      }
      else
      { // Assert.IsTrue(curve.End[dim.X] == x)
        p = curve.End;
        q = curve.Start;
      }
      double d = p.Y - isInside.Y;
      if (d > 0)
      { return 0; }
      else if (d == 0)
      { return -1; } // touch

      if (xPre.HasValue)
      {
        int inside = 1;
        if (xPre.Value < x == q.X < x)
        { inside = 0; }
        xPre = null;
        return inside;
      }
      else
      {
        xPre = q.X;
        return 0;
      }
    }
    private bool VerifyExtent(IPoint p, IBox extent)
    {
      if (p.X < extent.Min.X || p.X > extent.Max.X)
      { return false; }

      if (p.Y < extent.Min.Y)
      { return false; }

      return true;
    }

    /// <summary>
    /// only valid if polygon is defined clockwise
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IsWithin_clockwiseArea(IPoint p)
    {
      Point min = new Point2D(p.X, p.Y);
      Point2D max = new Point2D(Extent.Max.X, p.Y);
      IPoint near = null;
      bool onRightSide = false;

      Line searchLine = new Line(min, max);
      Box searchBox = new Box(min, max);

      foreach (var eLine in SpatialIndex.Search(searchBox))
      {
        Polyline line = eLine.Value;
        if (Point.Equals(line.Points[0], line.Points.Last()) == false)
        { line.Add(line.Points[0]); }
        foreach (var eCurve in line.SpatialIndex.Search(searchBox))
        {
          ISegment curve = eCurve.Value;
          bool within = IsWithinCurve(p, curve, max, searchLine, ref near, ref onRightSide);
          if (within)
          { return true; }
        }
      }
      return onRightSide;
    }

    private bool IsWithinCurve(IPoint p, ISegment curve, Point2D max, Line searchLine,
      ref IPoint near, ref bool onRightSide)
    {
      bool within = false;
      if (curve is IMultipartGeometry parts && parts.HasSubparts)
      {
        foreach (var part in parts.Subparts())
        {
          ISegment subCurve = (ISegment)part;
          within = IsWithinCurve(p, subCurve, max, searchLine,
            ref near, ref onRightSide);
          if (within)
          { break; }
        }
      }
      else
      {
        within = IsWithinCurveSimple(p, max, ref near, ref onRightSide,
        searchLine, curve);
      }
      return within;
    }

    private bool IsWithinCurveSimple(IPoint p, Point2D max,
      ref IPoint near, ref bool onRightSide, Line searchLine, ISegment curve)
    {
      if (curve.Start.Y == p.Y)
      {
        if (curve.End.Y == p.Y)
        {
          max.X = Math.Max(curve.Start.X, curve.End.Y);
          if (max.X <= p.X)
          { return true; }
        }
        else
        { OnRightSide(p, max, curve.Start, curve.End, true, ref near, ref onRightSide); }
      }
      else if (curve.End.Y == p.Y)
      { OnRightSide(p, max, curve.End, curve.Start, false, ref near, ref onRightSide); }
      else
      {
        GeometryCollection cutList = GeometryOperator.Intersection(curve, searchLine);

        if (cutList == null)
        { return false; }

        IPoint cut = (IPoint)cutList[0];
        if (!(cut.X >= p.X && cut.X <= max.X)) throw new InvalidOperationException($"unexptected cut.X");

        max.X = cut.X;
        onRightSide = (curve.Start.Y > p.Y);
      }
      if (max.X == p.X)
      { return true; }
      return false;
    }

    private void OnRightSide(IPoint p, Point max, IPoint onLine, IPoint offLine,
      bool startOnLine, ref IPoint near, ref bool onRightSide)
    {
      if (onLine.X < p.X || onLine.X > max.X)
      { return; }
      else if (near == null || onLine.X < max.X)
      {
        near = PointOp.Sub(offLine, onLine);
        max.X = onLine.X;
        onRightSide = (near.Y < 0) == startOnLine;
      }
      else if ((near.Y > 0) == (offLine.Y > p.Y))
      {
        Point n = PointOp.Sub(offLine, onLine);
        if ((n.VectorProduct(near) < 0) == (n.Y > 0))
        {
          near = n;
          onRightSide = (near.Y < 0) == startOnLine;
        }
      }
    }

    public new Box Extent => Lines.Extent;
    protected override IBox GetExtent() => Lines.Extent;
    IBox IGeometry.Extent
    { get { return Extent; } }

    public PolylineCollection Border => Lines;
    protected override IGeometry BorderGeom => Border;
    IGeometry IGeometry.Border => Border;


    IGeometry IGeometry.Project(IProjection projection) => Project(projection);
    protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
    public Area Project(IProjection projection)
    {
      return new Area(Lines.Project(projection));
    }

    #endregion

    public void CloseBorders()
    {
      List<Polyline> invalidList = new List<Polyline>();
      foreach (var border in Border)
      {
        if (border.Points.Count == 0)
        {
          invalidList.Add(border);
          continue;
        }
        Point start = Point.CastOrCreate(border.Points[0]);
        if (start.Dist2(border.Points.Last()) > 1e-12)
        { border.Add(start); }
      }
      foreach (var invalid in invalidList)
      {
        Border.Remove(invalid);
      }
    }
  }
}
