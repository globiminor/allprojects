using System.Collections.Generic;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for SegmentList.
  /// </summary>
  public class SegmentList : IEnumerable<Curve>
  {
    private Polyline _polyline;

    public SegmentList(Polyline polyline)
    {
      _polyline = polyline;
    }

    #region ICollection Members

    public int Count
    {
      get { return _polyline.Points.Count - 1; }
    }

    #endregion

    public Curve this[int index]
    {
      get
      {
        _polyline.InitNode(out LinkedListNode<IPoint> p0, out LinkedListNode<InnerCurve> c);

        for (int i = 0; i < index; i++)
        { _polyline.NextNodes(ref p0, ref c); }
        return Curve.Create(p0.Value, p0.Next.Value, c?.Value);
      }
    }
    public Curve First
    {
      get
      {
        _polyline.InitNode(out LinkedListNode<IPoint> p0, out LinkedListNode<InnerCurve> c);

        InnerCurve ic = null;
        if (c != null)
        { ic = c.Value; }
        return Curve.Create(p0.Value, p0.Next.Value, ic);
      }
    }
    public Curve Last
    {
      get
      {
        LinkedListNode<IPoint> p0 = _polyline.Points.Last;
        LinkedList<InnerCurve> c = _polyline.SegmentParts;

        return Curve.Create(p0.Previous.Value, p0.Value, c?.Last.Value);
      }
    }


    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    { return GetEnumerator(); }
    public IEnumerator<Curve> GetEnumerator()
    {
      if (_polyline.Points.Count < 2)
      { yield break; }

      LinkedListNode<InnerCurve> innerNode = null;
      InnerCurve innerCurve = null;
      if (_polyline.InnerCurves != null)
      {
        innerNode = _polyline.InnerCurves.First;
        innerCurve = innerNode.Value;
      }

      for (LinkedListNode<IPoint> pointNode = _polyline.Points.First; pointNode.Next != null; pointNode = pointNode.Next)
      {
        if (innerNode != null)
        { innerCurve = innerNode.Value; }
        yield return Curve.Create(pointNode.Value, pointNode.Next.Value, innerCurve);

        if (innerNode != null)
        { innerNode = innerNode.Next; }
      }
    }
  }
}
