using System.Collections.Generic;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for SegmentList.
  /// </summary>
  public class SegmentList : IEnumerable<Curve>
  {
    #region nested classes
    internal class _SegEnumerator : IEnumerator<Curve>
    {
      private Polyline _polyline;
      private LinkedListNode<IPoint> _point;
      private LinkedListNode<InnerCurve> _innerNode;
      private InnerCurve _innerCurve;

      private Curve _line;

      public _SegEnumerator(Polyline polyline)
      {
        _polyline = polyline;
        Reset();
      }
      #region IEnumerator Members

      public void Dispose()
      {  }

      public void Reset()
      {
        _point = _polyline.Points.First;
        _innerNode = null;

        if (_polyline.InnerCurves != null)
        {
          _innerNode = _polyline.InnerCurves.First;
          _innerCurve = _innerNode.Value; 
        }
      }

      public Curve Current
      {
        get
        { return _line; }
      }
      object System.Collections.IEnumerator.Current
			{ get { return Current; } }

      public bool MoveNext()
      {
        if (_point == null)
        { return false; }
        if (_point.Next == null)
        { return false; }

        _line = Curve.Create(_point.Value, _point.Next.Value, _innerCurve);

        // prepare next value
        _point = _point.Next;
        if (_innerNode != null)
        {
          _innerNode = _innerNode.Next;
          if (_innerNode != null)
          { _innerCurve = _innerNode.Value; }
          else
          { System.Diagnostics.Debug.Assert(_point.Next == null); }
        }

        return true;
      }

      #endregion

    }

    #endregion nested classes

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
        LinkedListNode<IPoint> p0;
        LinkedListNode<InnerCurve> c;
        _polyline.InitNode(out p0, out c);

        for (int i = 0; i < index; i++)
        { _polyline.NextNodes(ref p0, ref c); }
        return Curve.Create(p0.Value, p0.Next.Value, c.Value);
      }
    }
    public Curve First
    {
      get
      {
        LinkedListNode<IPoint> p0;
        LinkedListNode<InnerCurve> c;
        _polyline.InitNode(out p0, out c);

        InnerCurve ic = null;
        if (c != null)
        { ic = c.Value; }
        return Curve.Create(p0.Value,p0.Next.Value,ic);
      }
    }
    public Curve Last
    {
      get
      {
        LinkedListNode<IPoint> p0 = _polyline.Points.Last;
        LinkedList<InnerCurve> c = _polyline.SegmentParts;

        return Curve.Create(p0.Previous.Value, p0.Value, c == null ? null : c.Last.Value);
      }
    }

    #region IEnumerable Members

    public IEnumerator<Curve> GetEnumerator()
    {
      return new _SegEnumerator(_polyline);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{ return GetEnumerator(); }

    #endregion
  }
}
