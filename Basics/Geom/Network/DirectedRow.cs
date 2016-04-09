using System.Collections.Generic;


namespace Basics.Geom.Network
{
  public class DirectedRow : NetElement
  {
    #region nested classes
    public class RowByLineAngleComparer : IComparer<DirectedRow>
    {
      public int Compare(DirectedRow x, DirectedRow y)
      {
        return x.FromAngle.CompareTo(y.FromAngle);
      }
    }
    #endregion

    private bool _isBackward;
    private TopologicalLine _topologicalLine;

    public DirectedRow(Polyline line, bool isBackward)
    {
      _isBackward = isBackward;
      _topologicalLine = new TopologicalLine(line);
    }
    public DirectedRow(TopologicalLine line, bool isBackward)
    {
      _isBackward = isBackward;
      _topologicalLine = line;
    }

    public bool IsBackward
    {
      get { return _isBackward; }
    }

    public IPoint FromPoint
    {
      get
      {
        if (_isBackward)
        {
          return _topologicalLine.ToPoint;
        }
        else
        {
          return _topologicalLine.FromPoint;
        }
      }
    }

    protected override NNetPoint NetPoint__
    {
      get { return new NNetPoint(FromPoint); }
    }

    public IPoint ToPoint
    {
      get
      {
        if (_isBackward)
        {
          return _topologicalLine.FromPoint;
        }
        else
        {
          return _topologicalLine.ToPoint;
        }
      }
    }

    public double FromAngle
    {
      get
      {
        if (_isBackward)
        { return _topologicalLine.ToAngle; }
        else
        { return _topologicalLine.FromAngle; }
      }
    }
    public double ToAngle
    {
      get
      {
        if (_isBackward)
        { return _topologicalLine.FromAngle; }
        else
        { return _topologicalLine.ToAngle; }
      }
    }
    public DirectedRow Reverse()
    {
      return new DirectedRow(_topologicalLine, !_isBackward);
    }

    public Polyline Line()
    {
      Polyline line;
      if (_isBackward)
      { line = _topologicalLine.Line.Invert(); }
      else
      { line = _topologicalLine.Line; }

      return line;
    }

    public LineListPolygon RightPoly
    {
      get
      {
        if (_isBackward)
        { return _topologicalLine.LeftPoly; }
        else
        { return _topologicalLine.RightPoly; }
      }
      internal set
      {
        if (_isBackward)
        { _topologicalLine.LeftPoly = value; }
        else
        { _topologicalLine.RightPoly = value; }
      }
    }

    public IPoint RightCentroid
    {
      get
      {
        if (_isBackward)
        { return _topologicalLine.LeftCentroid; }
        else
        { return _topologicalLine.RightCentroid; }
      }
      set
      {
        if (_isBackward)
        { _topologicalLine.LeftCentroid = value; }
        else
        { _topologicalLine.RightCentroid = value; }
      }
    }

    public LineListPolygon LeftPoly
    {
      get
      {
        if (_isBackward == false)
        { return _topologicalLine.LeftPoly; }
        else
        { return _topologicalLine.RightPoly; }
      }
      internal set
      {
        if (_isBackward == false)
        { _topologicalLine.LeftPoly = value; }
        else
        { _topologicalLine.RightPoly = value; }
      }
    }

    public IPoint LeftCentroid
    {
      get
      {
        if (_isBackward == false)
        { return _topologicalLine.LeftCentroid; }
        else
        { return _topologicalLine.RightCentroid; }
      }
      set
      {
        if (_isBackward == false)
        { _topologicalLine.LeftCentroid = value; }
        else
        { _topologicalLine.RightCentroid = value; }
      }
    }

    public TopologicalLine TopoLine
    {
      get { return _topologicalLine; }
    }
  }
}
