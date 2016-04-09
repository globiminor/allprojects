using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom.Network
{
  public class LineList
  {
    private List<LineList> _innerRings;
    private LinkedList<DirectedRow> _directedRows;

    private bool _hasEquals; // has equal 

    private LineList()
    {
      _innerRings = new List<LineList>();
      _directedRows = new LinkedList<DirectedRow>();
    }
    public LineList(DirectedRow row0)
      : this()
    {
      _directedRows.AddFirst(row0);
    }

    public LineList(DirectedRow row0, DirectedRow row1)
      : this()
    {
      _directedRows.AddFirst(row0);
      _directedRows.AddLast(row1);

      if (row0.TopoLine == row1.TopoLine)
      { _hasEquals = true; }
    }

    public IPoint FromPoint
    {
      get { return _directedRows.First.Value.FromPoint; }
    }

    public IPoint ToPoint
    {
      get { return _directedRows.Last.Value.ToPoint; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns>clockwise = 1, counterclockwise = -1, not closed or empty = 0</returns>
    public int Orientation()
    {
      if (_directedRows.Count == 0)
      { return 0; }
      if (PointOperator.Dist2(FromPoint, ToPoint) != 0)
      { return 0; }
      if (_hasEquals)
      {
        LineList list = RemoveEquals();

        if (list == null)
        { return 0; }

        return list.Orientation();
      }

      double xMax = 0;
      LinkedListNode<DirectedRow> nodeMax = null;
      LinkedListNode<DirectedRow> node = _directedRows.First;

      while (node != null)
      {
        IBox _queryEnvelope = node.Value.TopoLine.Line.Extent;
        if (nodeMax == null || _queryEnvelope.Max.X > xMax)
        {
          xMax = _queryEnvelope.Max.X;
          nodeMax = node;
        }
        node = node.Next;
      }

      Debug.Assert(nodeMax != null, "Error in software design assumption");
      int orientation = (int)nodeMax.Value.TopoLine.Orientation();
      if (orientation == 0)
      { return 0; }
      if (nodeMax.Value.IsBackward)
      { orientation = -orientation; }

      if (Math.Abs(orientation) > 1)
      {
        double angle0;
        double angle1;
        if (orientation < 0)
        {
          angle1 = nodeMax.Value.FromAngle;
          node = PreviousDirNode(nodeMax);
          angle0 = node.Value.ToAngle;
        }
        else
        {
          angle0 = nodeMax.Value.ToAngle;
          node = NextDirNode(nodeMax);
          angle1 = node.Value.FromAngle;
        }
        Debug.Assert(Math.Abs(angle0) >= Math.PI / 2, "Error in software design assumption");
        Debug.Assert(Math.Abs(angle1) >= Math.PI / 2, "Error in software design assumption");

        double dAngle = angle1 - angle0;
        if (dAngle == 0)
        { return 0; }
        if (dAngle < 0)
        {
          dAngle += Math.PI * 2;
        }
        // avoid numerical problems
        if (angle0 > 0)
        {
          if (dAngle < 4) // exactly 3 * Math.PI / 2
          {
            Debug.Assert(dAngle < 2 * Math.PI + 0.1, "Error in software design assumption");
            orientation = 1;
          }
          else
          {
            Debug.Assert(dAngle > 3 * Math.PI / 2 - 0.1, "Error in software design assumption");
            orientation = -1;
          }
        }
        else
        {
          if (dAngle < 2) // exactly Math.PI / 2
          {
            Debug.Assert(dAngle < Math.PI / 2 + 0.1, "Error in software design assumption");
            orientation = 1;
          }
          else
          {
            Debug.Assert(dAngle > Math.PI - 0.1, "Error in software design assumption");
            orientation = -1;
          }
        }
      }

      return orientation;
    }

    private LinkedListNode<DirectedRow> NextDirNode(LinkedListNode<DirectedRow> node)
    {
      LinkedListNode<DirectedRow> nodeNext = node.Next;
      if (nodeNext == null)
      { nodeNext = _directedRows.First; }
      return nodeNext;
    }

    private LinkedListNode<DirectedRow> PreviousDirNode(LinkedListNode<DirectedRow> node)
    {
      LinkedListNode<DirectedRow> nodePrevious = node.Previous;
      if (nodePrevious == null)
      { nodePrevious = _directedRows.Last; }
      return nodePrevious;
    }

    public LineList RemoveEquals()
    {
      Debug.Assert(PointOperator.Dist2(FromPoint, ToPoint) == 0,
        "invalid context for calling of method");

      LineList removedList = null;

      if (_directedRows.Count == 1)
      {
        removedList = new LineList(_directedRows.First.Value);
        return removedList;
      }
      LinkedListNode<DirectedRow> currNodeAll = _directedRows.First;
      DirectedRow dir0 = currNodeAll.Value;

      while (currNodeAll.Next != null)
      {
        currNodeAll = currNodeAll.Next;
        DirectedRow dir1 = currNodeAll.Value;

        if (dir0 == null)
        { dir0 = dir1; }
        else if (dir0.TopoLine != dir1.TopoLine)
        {
          if (removedList == null)
          { removedList = new LineList(dir0, dir1); }
          else
          { removedList.AddRow(dir1, false); }
          dir0 = dir1;
        }
        else if (removedList != null)
        {
          removedList._directedRows.RemoveLast();

          if (removedList._directedRows.Count == 0)
          {
            removedList = null;
            dir0 = null;
          }
          else
          { dir0 = removedList._directedRows.Last.Value; }
        }
        else
        { dir0 = null; }
      }

      while (removedList != null && removedList._directedRows.First.Value.TopoLine ==
        removedList._directedRows.Last.Value.TopoLine)
      {
        removedList._directedRows.RemoveFirst();
        removedList._directedRows.RemoveLast();
        if (removedList._directedRows.Count == 0)
        { removedList = null; }
      }
      return removedList;
    }

    public LinkedList<DirectedRow> DirectedRows
    {
      get { return _directedRows; }
    }

    public bool IsClosed
    {
      get { return PointOperator.Dist2(FromPoint, ToPoint) == 0; }
    }

    public void AddInnerRing(LineList innerRing)
    {
      _innerRings.Add(innerRing);
    }

    public override bool Equals(Object obj)
    {
      LineList lc = obj as LineList;
      if (lc == null)
      {
        return false;
      }
      return _directedRows.Equals(lc._directedRows);
    }

    // to make the compiler happy
    public override int GetHashCode()
    { return _innerRings.GetHashCode() + 29 * _directedRows.GetHashCode(); }


    /// <summary>
    /// Gets a new LineList with reversed direction
    /// Remark: inner lists are not transfered to new LineList
    /// </summary>
    public LineList Reverse()
    {
      LineList reverse = new LineList();

      foreach (DirectedRow dRow in _directedRows)
      {
        reverse._directedRows.AddFirst(dRow.Reverse());
      }
      return reverse;
    }

    /// <summary>
    /// Get Border of polygon (only outer ring!)
    /// </summary>
    /// <returns></returns>
    public Polyline GetBorder()
    {
      Polyline border = new Polyline();
      foreach (DirectedRow pRow in _directedRows)
      {
        Polyline nextPart = pRow.Line();
        foreach (Curve c in nextPart.Segments)
        {
          border.Add(c);
        }
      }
      //object missing = Type.Missing;
      //polyCollection.AddGeometry(ring, ref missing, ref missing);

      //foreach (LineList lc in _innerRings)
      //{
      //  polyCollection.AddGeometry(((IGeometryCollection)lc.GetPolygon()).get_Geometry(0), ref missing, ref missing);
      //}
      return border;
    }

    public Area GetPolygon()
    {
      Area poly = CombineRings();
      return poly;
    }

    private Area CombineRings()
    {
      Area polygon = new Area();
      Polyline ring = new Polyline();
      foreach (DirectedRow pRow in _directedRows)
      {
        Polyline nextPart = pRow.Line();
        foreach (Curve c in nextPart.Segments)
        {
          ring.Add(c);
        }
      }
      polygon.Border.Add(ring);

      foreach (LineList lc in _innerRings)
      {
        polygon.Border.Add(lc.CombineRings().Border[0]);
      }
      return polygon;
    }

    public void AddRow(DirectedRow dRow, bool inFront)
    {
      if (inFront)
      {
        if (_hasEquals == false && _directedRows.First.Value.TopoLine == dRow.TopoLine)
        { _hasEquals = true; }
        _directedRows.AddFirst(dRow);
      }
      else
      {
        if (_hasEquals == false && _directedRows.Last.Value.TopoLine == dRow.TopoLine)
        { _hasEquals = true; }
        _directedRows.AddLast(dRow);
      }
    }

    public void AddCollection(LineList list)
    {
      if (_hasEquals == false && (list._hasEquals ||
        _directedRows.Last.Value.TopoLine ==
        list._directedRows.First.Value.TopoLine))
      { _hasEquals = true; }
      foreach (DirectedRow row in list._directedRows)
      {
        _directedRows.AddLast(row);
      }
    }

    public IBox Envelope()
    {
      IBox boxTotal = null;
      foreach (DirectedRow pRow in _directedRows)
      {
        IBox box = pRow.TopoLine.Line.Extent;
        if (boxTotal == null)
        { boxTotal = box.Clone(); }
        else
        {
          boxTotal.Include(box);
        }
      }

      return boxTotal;
    }

    private List<DirectedRow> _fromNode = new List<DirectedRow>();
    private List<DirectedRow> _toNode = new List<DirectedRow>();

    public IList<DirectedRow> FromNode
    {
      get { return _fromNode; }
    }
    public IList<DirectedRow> ToNode
    {
      get { return _toNode; }
    }

    public void Append(DirectedRow rowEnd, DirectedRow rowAppend, IComparer<DirectedRow> comparer)
    {
      DirectedRow row = _directedRows.First.Value;
      if (comparer.Compare(rowEnd, row) == 0)
      {
        Debug.Assert(FromNode == null, "error in assumption");
        if (rowEnd.IsBackward != row.IsBackward)
        { rowAppend = rowAppend.Reverse(); }

        _directedRows.AddFirst(rowAppend);
      }
      else
      {
        row = _directedRows.Last.Value;
        Debug.Assert(comparer.Compare(rowEnd, row) == 0,
          "error in assumption");

        if (rowEnd.IsBackward != row.IsBackward)
        { rowAppend = rowAppend.Reverse(); }

        _directedRows.AddLast(rowAppend);
      }
    }

    internal void Completed()
    {
      if (_hasEquals == false && _directedRows.Count > 1 &&
        _directedRows.First.Value.TopoLine == _directedRows.Last.Value.TopoLine)
      {
        _hasEquals = true;
      }
    }
  }
}
