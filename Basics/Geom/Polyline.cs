using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Line2D.
  /// </summary>
  public class Polyline : Geometry, IMultipartGeometry
  {
    private readonly LinkedList<IPoint> _pointList;
    private readonly SegmentList _segmentList;
    private LinkedList<InnerCurve> _innerCurveList;
    private Box _extent;

    private BoxTree<Curve> _spatialIndex;

    public Polyline()
    {
      _pointList = new LinkedList<IPoint>();
      _segmentList = new SegmentList(this);
    }

    public static Polyline Create<T>(IEnumerable<T> pointList) where T : IPoint
    {
      Polyline p = new Polyline();
      p.AddRange(pointList);
      return p;
    }

    private void AddRange<T>(IEnumerable<T> pointList) where T : IPoint
    {
      foreach (T point in pointList)
      {
        _pointList.AddLast(point);
      }
    }
    internal LinkedList<InnerCurve> SegmentParts
    { // used from SegmentList
      get
      { return _innerCurveList; }
    }

    public SegmentList Segments
    {
      get
      { return _segmentList; }
    }

    public void Add(IPoint point)
    {
      _spatialIndex = null;

      _pointList.AddLast(point);
      if (_innerCurveList != null)
      { _innerCurveList.AddLast((InnerCurve)null); }
    }
    public void AddFirst(IPoint point)
    {
      _spatialIndex = null;

      _pointList.AddFirst(point);
      if (_innerCurveList != null)
      { _innerCurveList.AddFirst((InnerCurve)null); }
    }

    public int Add(Curve segment)
    {
      _spatialIndex = null;

      int i = _pointList.Count - 1;
      if (i < 0)
      { _pointList.AddFirst(segment.Start); }
      else
      {
        IPoint last = _pointList.Last.Value;
        IPoint start = segment.Start;
        if (last != start && Point.CastOrCreate(last).Dist2(start) > 0.0001 * segment.Length())
        { throw new InvalidOperationException("segment does not fit to the end of polyline"); }
      }
      _pointList.AddLast(segment.End);

      if (_innerCurveList == null && segment.InnerCurve() != null)
      {
        _innerCurveList = new LinkedList<InnerCurve>();
        for (int iSeg = 0; iSeg < i; iSeg++)
        { _innerCurveList.AddLast((InnerCurve)null); }
      }
      if (_innerCurveList != null)
      { _innerCurveList.AddLast(segment.InnerCurve()); }

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
      foreach (Curve pLine in Segments)
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
      foreach (Curve curve in Segments)
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
        Curve c = Segments[iStart].Subpart(p0, p1);
        subpart.Add(c);
      }
      else
      {
        Curve c = Segments[iStart].Subpart(p0, 1);
        subpart.Add(c);
        for (int i = iStart + 1; i < iSeg; i++)
        {
          subpart.Add(Segments[i]);
        }
        if (iEnd >= 0)
        {
          Curve endPart = Segments[iEnd].Subpart(0, p1);
          subpart.Add(endPart);
        }
      }
      return subpart;
    }

    public LinkedList<IPoint> Points
    {
      get { return _pointList; }
    }
    internal LinkedList<InnerCurve> InnerCurves
    {
      get { return _innerCurveList; }
    }

    public double Length()
    {
      double dDist = 0;
      foreach (Curve pLine in Segments)
      {
        dDist += pLine.Length();
      }
      return dDist;
    }

    public double[] ParamAt(double distance)
    {
      double dRest = distance;
      int iSeg = 0;
      foreach (Curve curve in Segments)
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

    public IList<Polyline> Split(IList<ParamGeometryRelation> splits)
    {
      Dictionary<Curve, List<ParamGeometryRelation>> curveSplits =
        new Dictionary<Curve, List<ParamGeometryRelation>>(new Curve.GeometryEquality());

      foreach (ParamGeometryRelation rel in splits)
      {
        ParamGeometryRelation r = rel.GetChildRelation(this);
        Curve curve = (Curve)r.CurrentX;
        List<ParamGeometryRelation> list;
        if (curveSplits.TryGetValue(curve, out list) == false)
        {
          list = new List<ParamGeometryRelation>();
          curveSplits.Add(curve, list);
        }
        list.Add(r);
      }

      List<Polyline> parts = new List<Polyline>();
      Polyline part = null;

      foreach (Curve curve in Segments)
      {
        List<ParamGeometryRelation> list;
        if (curveSplits.TryGetValue(curve, out list) == false)
        {
          if (part == null)
          { part = new Polyline(); }
          part.Add(curve.Clone());
          continue;
        }

        IList<Curve> curveParts = curve.Split(list);
        Curve curvePart;
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

    private Curve Segment(LinkedListNode<IPoint> pointNode,
      LinkedListNode<InnerCurve> innerCurveNode)
    {
      InnerCurve innerCurve = null;
      if (innerCurveNode != null)
      { innerCurve = innerCurveNode.Value; }

      return Curve.Create(pointNode.Value, pointNode.Next.Value, innerCurve);
    }

    internal void InitNode(out LinkedListNode<IPoint> pointNode,
      out LinkedListNode<InnerCurve> innerCurveNode)
    {
      pointNode = _pointList.First;
      if (_innerCurveList != null)
      { innerCurveNode = _innerCurveList.First; }
      else
      { innerCurveNode = null; }
    }

    internal void NextNodes(ref LinkedListNode<IPoint> pointNode,
      ref LinkedListNode<InnerCurve> innerCurveNode)
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

    public override IBox Extent
    {
      get
      {
        if (_extent == null)
        {
          Box extent = null;
          foreach (Curve seg in Segments)
          {
            if (extent == null)
            {
              IBox box = seg.Extent;
              Point min = Point.Create(box.Min);
              Point max = Point.Create(box.Max);
              extent = new Box(min, max);
            }
            else
            { extent.Include(seg.Extent); }
          }
          if (extent == null && _pointList.Count > 0)
          {
            IBox box = _pointList.First.Value.Extent;
            Point min = Point.Create(box.Min);
            Point max = Point.Create(box.Max);
            extent = new Box(min, max);
          }
          _extent = extent;
        }
        return _extent;
      }
    }
    IBox IGeometry.Extent
    { get { return Extent; } }

    public BoxTree<Curve> SpatialIndex
    {
      get
      {
        if (_spatialIndex == null)
        {
          _spatialIndex = new BoxTree<Curve>(2);
          _spatialIndex.InitSize(new IGeometry[] { Extent });
          foreach (Curve curve in Segments)
          { _spatialIndex.Add(curve.Extent, curve); }
        }
        return _spatialIndex;
      }
    }
    public PointCollection Border
    {
      get
      {
        int iNPoints = _pointList.Count;
        if (iNPoints == 0)
        { return null; }
        IPoint start = _pointList.First.Value;
        IPoint end = _pointList.Last.Value;
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

    //  foreach (Curve seg in Segments)
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
    //						foreach (IGeometry pGeom in pTouch)
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

    private Polyline Project__(IProjection projection)
    {
      Polyline pPoly = new Polyline();
      foreach (Curve line in Segments)
      { pPoly.Add(line.Project(projection)); }
      return pPoly;
    }
    protected override Geometry Project_(IProjection projection)
    { return Project__(projection); }
    public new Polyline Project(IProjection projection)
    { return (Polyline)Project_(projection); }
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

    public IEnumerable<Curve> Subparts()
    {
      return _segmentList;
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }

    #endregion

    /// <summary>
    /// Douglas Peuker
    /// </summary>
    /// <param name="d">maximum offset</param>
    /// <returns></returns>
    public Polyline Generalize(double d)
    {
      List<IPoint> points;
      if (_segmentList == null)
      {
        points = new List<IPoint>(Points);
      }
      else
      {
        points = new List<IPoint>(2 * Points.Count);
        points.Add(Points.First.Value);
        foreach (Curve curve in Segments)
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
      foreach (Curve curve in Segments)
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

    public Polyline Invert()
    {
      Polyline invert = new Polyline();
      if (SegmentParts == null)
      {
        LinkedListNode<IPoint> node = Points.Last;
        while (node != null)
        {
          invert.Add(node.Value);
          node = node.Previous;
        }
      }
      else
      {
        LinkedListNode<IPoint> startNode = Points.Last;
        LinkedListNode<IPoint> endNode = startNode.Previous;
        LinkedListNode<InnerCurve> curveNode = SegmentParts.Last;
        while (endNode != null)
        {
          Curve curve = Curve.Create(endNode.Value, startNode.Value, curveNode.Value);
          invert.Add(curve.Invert());

          startNode = endNode;
          endNode = endNode.Previous;
          curveNode = curveNode.Previous;
        }
      }

      return invert;
    }
  }
}
