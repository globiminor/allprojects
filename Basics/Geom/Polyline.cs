using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Line2D.
  /// </summary>
  public class Polyline : Geometry, IMultipartGeometry, ICurve, ISimplePolyline
  {
    private readonly List<IPoint> _pointList;
    private List<IInnerSegment> _innerCurveList;
    private Box _extent;

    private BoxTree<ISegment> _spatialIndex;

    public Polyline()
    {
      _pointList = new List<IPoint>();
    }

    public static Polyline Create<T>(IEnumerable<T> pointList) where T : IPoint
    {
      Polyline p = new Polyline();
      p.AddRange(pointList);
      return p;
    }

    public Polyline ToBeziers()
    {
      Polyline copy = new Polyline();
      foreach (var seg in EnumSegments())
      {
        if (seg is Line || seg is Bezier)
        {
          copy.Add(seg.Clone());
          continue;
        }
        Arc arc = seg as Arc;
        foreach (var part in arc.EnumBeziers())
        {
          copy.Add(part);
        }
      }
      return copy;
    }

    public bool HasNonBezier()
    {
      foreach (var seg in EnumSegments())
      {
        if (seg is Line || seg is Bezier)
        { continue; }
        return true;
      }
      return false;
    }

    IBox IParamGeometry.ParameterRange
    {
      get
      {
        Point min = Point.Create_0(1);
        Point max = Point.Create_0(1);
        min[0] = 0;
        max[0] = Points.Count - 1;
        return new Box(min, max);
      }
    }
    IPoint IParamGeometry.PointAt(IPoint parameters)
    {
      double at = parameters[0];
      int iSeg = (int)at;
      if (iSeg >= Points.Count - 1)
      { iSeg--; }

      return GetSegment(iSeg).PointAt(at - iSeg);
    }
    IList<IPoint> ITangentGeometry.TangentAt(IPoint parameters)
    {
      double at = parameters[0];
      int iSeg = (int)at;
      if (iSeg >= Points.Count - 1)
      { iSeg--; }

      return new[] { GetSegment(iSeg).TangentAt(at - iSeg) };
    }

    private void AddRange<T>(IEnumerable<T> points) where T : IPoint
    {
      foreach (var point in points)
      { _pointList.Add(point); }
    }
    internal List<IInnerSegment> SegmentParts
    { // used from SegmentList
      get { return _innerCurveList; }
    }

    IEnumerable<ISegment> ICurve.Segments => EnumSegments();
    public IEnumerable<ISegment> EnumSegments()
    {
      if (Points.Count < 2)
      { yield break; }

      IEnumerator<IInnerSegment> innerSegs = _innerCurveList?.GetEnumerator();
      IEnumerator<IPoint> points = Points.GetEnumerator();

      points.MoveNext();
      IPoint p2 = points.Current;
      while (points.MoveNext())
      {
        IPoint p1 = p2;
        p2 = points.Current;
        innerSegs?.MoveNext();
        IInnerSegment innerSeg = innerSegs?.Current;
        yield return Curve.Create(p1, p2, innerSeg);
      }
    }

    public void Insert(int position, IPoint point)
    {
      if (position == _pointList.Count)
      {
        Add(point);
        return;
      }
      _spatialIndex = null;

      _pointList.Insert(position, point);
      _innerCurveList?.Insert(position, null);
    }

    public void Replace(int position, IPoint point)
    {
      _spatialIndex = null;

      if (position < 0)
        position = Points.Count + position;
      _pointList[position] = point;
    }

    public void RemoveAt(int index)
    {
      if (index < 0)
      {
        index = Points.Count - 1;
      }
      _spatialIndex = null;

      _pointList.RemoveAt(index);
      _innerCurveList?.RemoveAt(Math.Max(index, _innerCurveList.Count - 1));
    }

    public void Add(IPoint point)
    {
      _spatialIndex = null;

      _pointList.Add(point);
      if (_innerCurveList != null)
      { _innerCurveList.Add((InnerCurve)null); }
    }

    public int Add(ISegment segment, bool verify = true)
    {
      _spatialIndex = null;

      int i = _pointList.Count - 1;
      if (i < 0)
      { _pointList.Add(segment.Start); }
      else
      {
        if (verify)
        {
          IPoint last = _pointList[_pointList.Count - 1];
          IPoint start = segment.Start;
          if (last != start && PointOp.Dist2(last, start) > 0.0001 * segment.Length())
          { throw new InvalidOperationException("segment does not fit to the end of polyline"); }
        }
      }
      _pointList.Add(segment.End);

      if (_innerCurveList == null && segment.GetInnerSegment() != null)
      {
        _innerCurveList = new List<IInnerSegment>();
        for (int iSeg = 0; iSeg < i; iSeg++)
        { _innerCurveList.Add(null); }
      }
      if (_innerCurveList != null)
      { _innerCurveList.Add(segment.GetInnerSegment()); }

      return i + 1;
    }

    //public void Insert(int position, Point point)
    //{
    //  _pointList.Insert(position, point);
    //  if (_innerCurveList != null)
    //  { _innerCurveList.Insert(position,null); }
    //}


    public Polyline Clone()
    {
      Polyline pClone = new Polyline();
      foreach (var pLine in EnumSegments())
      {
        pClone.Add(pLine.Clone());
      }
      return pClone;
    }

    public Polyline SubPart(double start, double length)
    {
      Polyline subpart = new Polyline();

      double end = start + length;
      double l1 = 0;
      int iStart = -1;
      int iEnd = -1;
      int iSeg = 0;
      double p0 = 0;
      double p1 = 0;
      foreach (var curve in EnumSegments())
      {
        double l0 = l1;
        double d = curve.Length();
        l1 = l1 + d;
        if (l1 > start && iStart < 0)
        {
          p0 = curve.ParamAt(start - l0);
          iStart = iSeg;
        }
        if (l1 >= end)
        {
          p1 = curve.ParamAt(end - l0);
          iEnd = iSeg;
          break;
        }
        iSeg++;
      }
      if (iStart < 0)
      {
        return null;
      }
      if (iStart == iEnd)
      {
        ISegment c = GetSegment(iStart).Subpart(p0, p1);
        subpart.Add(c);
      }
      else
      {
        ISegment c = GetSegment(iStart).Subpart(p0, 1);
        subpart.Add(c);
        for (int i = iStart + 1; i < iSeg; i++)
        {
          subpart.Add(GetSegment(i));
        }
        if (iEnd >= 0)
        {
          ISegment endPart = GetSegment(iEnd).Subpart(0, p1);
          subpart.Add(endPart);
        }
      }
      return subpart;
    }

    public IReadOnlyList<IPoint> Points
    {
      get { return _pointList; }
    }
    public ISegment GetSegment(int iSegment)
    {
      if (iSegment < 0)
      { iSegment = Points.Count + iSegment - 1; }
      return Curve.Create(Points[iSegment], Points[iSegment + 1], InnerCurves?[iSegment]);
    }
    internal IReadOnlyList<IInnerSegment> InnerCurves
    {
      get { return _innerCurveList; }
    }

    public double Length()
    {
      double dDist = 0;
      foreach (var pLine in EnumSegments())
      {
        dDist += pLine.Length();
      }
      return dDist;
    }

    public double[] ParamAt(double distance)
    {
      double dRest = distance;
      int iSeg = 0;
      foreach (var curve in EnumSegments())
      {
        double dLength = curve.Length();
        if (dLength > dRest)
        {
          double t = curve.ParamAt(dRest);
          return new[] { iSeg, t };
        }
        dRest -= dLength;
        iSeg++;
      }
      return null;
    }

    public IList<Polyline> Split(IEnumerable<ParamGeometryRelation> splits)
    {
      Dictionary<ISegment, List<ParamGeometryRelation>> curveSplits =
        new Dictionary<ISegment, List<ParamGeometryRelation>>(new Curve.GeometryEquality());

      foreach (var rel in splits)
      {
        ParamGeometryRelation r = rel.GetChildRelation(this);
        Curve curve = (Curve)r.CurrentX;
        if (curveSplits.TryGetValue(curve, out List<ParamGeometryRelation> list) == false)
        {
          list = new List<ParamGeometryRelation>();
          curveSplits.Add(curve, list);
        }
        list.Add(r);
      }
      if (curveSplits.Count == 0)
      { return new List<Polyline> { this }; }

      List<Polyline> parts = new List<Polyline>();
      Polyline part = null;

      foreach (var curve in EnumSegments())
      {
        if (curveSplits.TryGetValue(curve, out List<ParamGeometryRelation> list) == false)
        {
          if (part == null)
          { part = new Polyline(); }
          part.Add(curve.Clone());
          continue;
        }

        List<double> splitAts = Curve.GetSplitAts(curve, list);

        IList<ISegment> curveParts = Curve.GetSplitSegments(curve, splitAts);
        ISegment curvePart;
        int n = curveParts.Count - 1;
        for (int i = 0; i < n; i++)
        {
          curvePart = curveParts[i];
          if (curvePart != null)
          {
            if (part == null)
            { part = new Polyline(); }
            part.Add(curvePart);
          }
          if (part != null)
          { parts.Add(part); }
          part = null;
        }
        curvePart = curveParts[n];
        if (curvePart != null)
        {
          if (part == null)
          { part = new Polyline(); }
          part.Add(curvePart);
        }
      }

      if (part != null)
      { parts.Add(part); }

      return parts;
    }

    private ISegment Segment(LinkedListNode<IPoint> pointNode,
      LinkedListNode<InnerCurve> innerCurveNode)
    {
      InnerCurve innerCurve = null;
      if (innerCurveNode != null)
      { innerCurve = innerCurveNode.Value; }

      return Curve.Create(pointNode.Value, pointNode.Next.Value, innerCurve);
    }

    internal void NextNodes(ref LinkedListNode<IPoint> pointNode,
      ref LinkedListNode<IInnerSegment> innerCurveNode)
    {
      pointNode = pointNode.Next;
      if (innerCurveNode != null)
      { innerCurveNode = innerCurveNode.Next; }
    }


    #region IGeometry Members

    public override int Dimension
    {
      get { return 2; }
    }

    public override int Topology
    {
      get { return 1; }
    }

    protected override IBox GetExtent() => Extent;
    public new Box Extent
    {
      get
      {
        if (_extent == null)
        {
          Box extent = null;
          foreach (var seg in EnumSegments())
          {
            if (extent == null)
            {
              IBox box = seg.Extent;
              extent = new Box(box);
            }
            else
            { extent.Include(seg.Extent); }
          }
          if (extent == null && _pointList.Count > 0)
          {
            IBox box = Point.CastOrWrap(_pointList[0]);
            extent = new Box(box);
          }
          _extent = extent;
        }
        return _extent;
      }
    }
    IBox IGeometry.Extent
    { get { return Extent; } }

    public BoxTree<ISegment> SpatialIndex
    {
      get
      {
        if (_spatialIndex == null)
        {
          _spatialIndex = new BoxTree<ISegment>(2);
          _spatialIndex.InitSize(new IGeometry[] { Extent });
          foreach (var curve in EnumSegments())
          { _spatialIndex.Add(curve.Extent, curve); }
        }
        return _spatialIndex;
      }
    }
    public PointCollection Border
    {
      get
      {
        int nPoints = _pointList.Count;
        if (nPoints == 0)
        { return null; }
        IPoint start = _pointList[0];
        IPoint end = _pointList[nPoints - 1];
        PointCollection border = new PointCollection();
        if (start.Equals(end))
        { border.Add(start); }
        else
        {
          border.AddRange(new[] { start, end });
          return border;
        }
        return border;
      }
    }
    protected override IGeometry BorderGeom
    { get { return Border; } }
    IGeometry IGeometry.Border
    { get { return Border; } }

    public override bool IsWithin(IPoint point)
    {
      return false;
    }

    //public override bool Intersects(IBox box, IList<int> calcDimensions)
    //{
    //  IBox extent;
    //  Relation rel;

    //  extent = Extent;
    //  rel = box.RelationTo(extent, calcDimensions);
    //  if (rel == Relation.Contains)
    //  { return true; }
    //  if (rel == Relation.Disjoint)
    //  { return false; }

    //  foreach (var seg in Segments)
    //  {
    //    extent = seg.Extent;
    //    rel = box.RelationTo(extent, calcDimensions);
    //    if (rel == Relation.Contains)
    //    { return true; }
    //    if (rel == Relation.Intersect)
    //    {
    //      return true;
    //      //          if (pSeg.Intersects(box))
    //      //          { return true; }
    //    }
    //  }
    //  return false;
    //}

    //    public bool Intersects(IGeometry other)
    //    {
    //      if (other.Topology > this.Topology)
    //      { return other.Intersects(this); }
    //
    //      IBox pOtherBox = other.Extent;
    //      int iNSeg = this.Segments.Count - 1;
    //
    //      for (int iSeg = 0; iSeg < iNSeg; iSeg++)
    //      {
    //        Line pSeg = this.Segments[iSeg];
    //        Box pSegBox = pSeg.Extent;
    //        if (pSegBox.Intersects(pOtherBox) == false)
    //        { continue; }
    //
    //        if (pSeg.Intersects(other))
    //        { return true; }
    //      }
    //      return false;
    //    }
    //		public bool Intersection(IGeometry other,
    //			ref GeometryCollection intersection,ref GeometryCollection touch)
    //		{
    //			if (other.Topology > this.Topology)
    //			{ return other.Intersects(this); }
    //			bool bIntersect = false;
    //
    //			IBox pOtherBox = other.Extent;
    //			int iNSeg = this.Segments.Count - 1;
    //
    //			for (int iSeg = 0; iSeg <= iNSeg; iSeg++)
    //			{
    //				Line pSeg = this.Segments[iSeg];
    //				Box pSegBox = pSeg.Extent;
    //				if (pSegBox.Intersects(pOtherBox) == false)
    //				{ continue; }
    //
    //				GeometryCollection pTouch = null;
    //				if (pSeg.Intersection(other,ref intersection,ref pTouch))
    //				{ 
    //					if (pTouch != null)
    //					{
    //						foreach (var pGeom in pTouch)
    //						{ 
    //							if (pGeom.Topology == 0)
    //							{ 
    //								if ((iSeg == 0 && pSeg.Start.Intersects(pGeom)) ||
    //									(iSeg == iNSeg && pSeg.End.Intersects(pGeom)))
    //								{
    //									if (touch == null)
    //									{ touch = new GeometryCollection(); }
    //									touch.Add(pGeom);
    //								}
    //								else
    //								{
    //									if (intersection == null)
    //									{ intersection = new GeometryCollection(); }
    //									intersection.Add(pGeom); 
    //								}
    //							}
    //							else
    //							{
    //								if (touch == null)
    //								{ touch = new GeometryCollection(); }
    //								touch.Add(pGeom);
    //							}
    //						}
    //					}
    //					bIntersect = true; 
    //				}
    //			}
    //			return bIntersect;
    //		}

    protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
    public Polyline Project(IProjection projection)
    {
      Polyline projected = new Polyline();
      foreach (var line in EnumSegments())
      { projected.Add(line.Project(projection)); }
      return projected;
    }
    IGeometry IGeometry.Project(IProjection projection)
    { return Project(projection); }

    #endregion

    #region IMultipartGeometry Members

    public bool IsContinuous
    {
      get
      { return true; }
    }

    public bool HasSubparts
    {
      get
      { return true; }
    }

    IEnumerable<IGeometry> IMultipartGeometry.Subparts() => EnumSegments();

    #endregion

    /// <summary>
    /// Douglas Peuker
    /// </summary>
    /// <param name="d">maximum offset</param>
    /// <returns></returns>
    public Polyline Generalize(double d)
    {
      List<IPoint> points;
      if (_innerCurveList == null)
      {
        points = new List<IPoint>(Points);
      }
      else
      {
        points = new List<IPoint>(2 * Points.Count);
        points.Add(Points[0]);
        foreach (var curve in EnumSegments())
        {
          points.AddRange(curve.Linearize(d, false));
        }
      }
      double d2 = d * d;
      int n = points.Count;
      double[] d2s = new double[n];

      Generalize(points, 0, n, d2s, d2);

      Polyline gen = new Polyline();

      gen.Add(Point.Create(points[0]));

      for (int i = 1; i < n - 1; i++)
      {
        if (d2s[i] > d2)
        { gen.Add(Point.Create(points[i])); }
      }
      gen.Add(Point.Create(points[n - 1]));

      return gen;
    }

    private void Generalize(List<IPoint> points, int i0, int i1, IList<double> d2, double d20)
    {
      if (i1 - i0 < 2)
      { return; }

      Line line = new Line(points[i0], points[i1 - 1]);

      int iMax = 0;
      double d2Max = d20;

      for (int i = i0 + 1; i < i1 - 1; i++)
      {
        d2[i] = line.Distance2(points[i]);
        if (d2[i] > d2Max)
        {
          d2Max = d2[i];
          iMax = i;
        }
      }

      if (iMax != 0)
      {
        Generalize(points, i0, iMax, d2, d20);
        Generalize(points, iMax, i1, d2, d20);
      }
    }

    public Polyline Linearize(double maxOffset)
    {
      Polyline poly = new Polyline();
      IList<IPoint> list = null;
      int n = 0;
      foreach (var curve in EnumSegments())
      {
        list = curve.Linearize(maxOffset);
        n = list.Count - 1;
        for (int i = 0; i < n; i++)
        { poly.Add(list[i]); }
      }
      if (list != null && n > 0)
      { poly.Add(list[n]); }

      return poly;
    }

    ICurve ICurve.Invert()
    { return Invert(); }
    public Polyline Invert()
    {
      Polyline invert = Polyline.Create(Points);
      invert._pointList.Reverse();
      if (SegmentParts != null)
      {
        invert._innerCurveList = new List<IInnerSegment>();
        for (int iSeg = Points.Count - 2; iSeg >= 0; iSeg--)
        {

          ISegment curve = GetSegment(iSeg);

          IInnerSegment invertedSeg = curve.Invert().GetInnerSegment();
          invert._innerCurveList.Add(invertedSeg);
        }
      }

      return invert;
    }
  }
}
