using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom.Network
{
  public delegate void GeometryCompletedEventHandler<T>(T sender, LineList closedPolygon);

  public class RingGrower
  {
    private IComparer<ICurve> _comparer;

    private class DirectedRowComparer : IComparer<DirectedRow>
    {
      IComparer<ICurve> _subComparer;
      public DirectedRowComparer(IComparer<ICurve> subComparer)
      {
        _subComparer = subComparer;
      }
      public int Compare(DirectedRow x, DirectedRow y)
      {
        if (x.IsBackward != y.IsBackward)
        {
          if (x.IsBackward)
          { return -1; }
          else
          { return 1; }
        }
        return _subComparer.Compare(x.TopoLine.Line, y.TopoLine.Line);
      }
    }

    private static readonly double _tolerance = 1e-5;
    private SortedDictionary<DirectedRow, LineList> _startRows;
    private SortedDictionary<DirectedRow, LineList> _endRows;

    public event GeometryCompletedEventHandler<RingGrower> GeometryCompleted;

    public RingGrower(IComparer<ICurve> comparer)
    {
      _comparer = comparer;
      DirectedRowComparer cmp = new DirectedRowComparer(_comparer);
      _startRows = new SortedDictionary<DirectedRow, LineList>(cmp);
      _endRows = new SortedDictionary<DirectedRow, LineList>(cmp);
    }

    public LineList Add(DirectedRow row0, DirectedRow row1)
    {
      if (_endRows.TryGetValue(row0, out LineList pre))
      {
        _endRows.Remove(row0);
      }
      if (_startRows.TryGetValue(row1, out LineList post))
      {
        _startRows.Remove(row1);
      }

      if (pre == null && post == null)
      {
        if (row0.TopoLine == row1.TopoLine && row0.IsBackward == row1.IsBackward)
        {
          pre = new LineList(row0);
          pre.Completed();
          OnClosing(pre);
          return pre;
        }

        pre = new LineList(row0, row1);
        _startRows.Add(row0, pre);
        _endRows.Add(row1, pre);
        return pre;
      }
      else if (pre == null)
      {
        post.AddRow(row0, true);
        _startRows.Add(row0, post);
        return post;
      }
      else if (post == null)
      {
        pre.AddRow(row1, false);
        _endRows.Add(row1, pre);
        return pre;
      }
      else if (pre == post) // Polygon completed
      {
        pre.Completed();
        OnClosing(pre);
        return pre;
      }
      else // merge pre and post
      {
        pre.AddCollection(post);
        DirectedRow z = pre.DirectedRows.Last.Value;
        // updating _endRows, 
        Debug.Assert(z == post.DirectedRows.Last.Value, "error in assumptions");
        Debug.Assert(_endRows[z] == post, "error in assumptions");
        _endRows[z] = pre;

        return pre;
      }
    }

    private void OnClosing(LineList closedPolygon)
    {
      GeometryCompleted?.Invoke(this, closedPolygon);
    }

    public void ResolvePoint(IPoint point, List<DirectedRow> intersectList, DirectedRow row)
    {
      DirectedRow.RowByLineAngleComparer comparer = new DirectedRow.RowByLineAngleComparer();
      if (intersectList.Count > 1)
      {
        intersectList.Sort(comparer);
        int index = intersectList.BinarySearch(row);
        ResolvePoint(point, intersectList, index);
      }
      else if (intersectList.Count != 0)
      { ResolvePoint(point, intersectList, 0); }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"> the point to be resolved</param>
    /// <param name="intersectList"> an array listing the lines to the point in counterclockwise order. first line appears twice</param>
    /// <param name="index"> the index of the current row in the array</param>
    private void ResolvePoint(IPoint point, List<DirectedRow> intersectList, int index)
    {
      if (intersectList.Count < 2)
      {
        Add(intersectList[0].Reverse(), intersectList[0]);
        return;
      }

      DirectedRow leftRow, middleRow, rightRow;
      middleRow = intersectList[index];
      if (index < intersectList.Count - 1)
      { rightRow = intersectList[index + 1]; }
      else
      { rightRow = intersectList[0]; }

      if (index == 0)
      { leftRow = intersectList[intersectList.Count - 1]; }
      else
      { leftRow = intersectList[index - 1]; }

      if (_comparer.Compare(middleRow.TopoLine.Line, rightRow.TopoLine.Line) < 0)
      {
        Add(middleRow.Reverse(), rightRow);
      }
      if (_comparer.Compare(middleRow.TopoLine.Line, leftRow.TopoLine.Line) < 0)
      {
        Add(leftRow.Reverse(), middleRow);
      }
    }

    public IList<LineList> GetAndRemoveCollectionsInside(IBox box)
    {
      List<LineList> insideList = new List<LineList>();
      foreach (var pair in _startRows)
      {
        IBox polyEnvelope = pair.Value.Envelope();
        if (box == null ||
          (polyEnvelope.Min.X + _tolerance >= box.Min.X &&
          polyEnvelope.Min.Y + _tolerance >= box.Min.Y &&
          polyEnvelope.Max.X - _tolerance <= box.Max.X &&
          polyEnvelope.Max.Y - _tolerance <= box.Max.Y))
        {
          insideList.Add(pair.Value);
        }
      }

      foreach (var poly in insideList)
      {
        _startRows.Remove(poly.DirectedRows.First.Value);
        _endRows.Remove(poly.DirectedRows.Last.Value);
      }

      return insideList;
    }

    private List<DirectedRow> DirectedRows<TCurve>(IPoint point,
      IEnumerable<TCurve> intersects, ICurve mainRow)
      where TCurve : ICurve
    {
      List<TopologicalLine> tList = new List<TopologicalLine>();
      foreach (var row in intersects)
      {
        if (mainRow != null && _comparer.Compare(mainRow, row) > 0)
        { return null; }

        TopologicalLine tLine = new TopologicalLine(row);
        tList.Add(tLine);
      }

      List<DirectedRow> dirList = new List<DirectedRow>();
      foreach (var tLine in tList)
      {
        if (PointOp.Dist2(point, tLine.FromPoint) == 0)
        { dirList.Add(new DirectedRow(tLine, false)); }
        if (PointOp.Dist2(point, tLine.ToPoint) == 0)
        { dirList.Add(new DirectedRow(tLine, true)); }
      }

      return dirList;
    }

    public static void Sort(List<DirectedRow> dirList)
    {
      if (dirList != null)
      { dirList.Sort(new DirectedRow.RowByLineAngleComparer()); }
    }

    public IList<DirectedRow> SortedDirectedRows<TCurve>(IPoint point,
      IEnumerable<TCurve> pIntersects, TCurve mainRow)
      where TCurve: ICurve
    {
      List<DirectedRow> dirList = DirectedRows(point, pIntersects, mainRow);

      if (dirList != null)
      { dirList.Sort(new DirectedRow.RowByLineAngleComparer()); }

      return dirList;
    }
  }

}
