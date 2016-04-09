using System;
using System.Collections.Generic;

namespace Basics.Geom.Network
{
  public class TopologicalLine
  {
    private Polyline _line;

    private bool _fromAngleKnown;
    private double _fromAngle;
    private bool _toAngleKnown;
    private double _toAngle;

    private LineListPolygon _leftPoly;
    private LineListPolygon _rightPoly;

    private IPoint _leftCentroid;
    private IPoint _rightCentroid;

    private MaxCode _maxCode = 0;
    private double _yMax;

    internal enum MaxCode { 
      StartMax = -2, 
      ClockWise = -1, 
      Unknown = 0, 
      CounterClockWise = 1,
      EndMax = 2 
    }

    public TopologicalLine(Polyline line)
    {
      _line = line;
    }

    public Polyline Line
    {
      get { return _line; }
    }

    public IPoint FromPoint
    {
      get { return _line.Points.First.Value; }
    }

    public IPoint ToPoint
    {
      get { return _line.Points.Last.Value; }
    }

    public double FromAngle
    {
      get
      {
        if (_fromAngleKnown == false)
        {
          IPoint t0 = _line.Segments.First.TangentAt(0);

          _fromAngle = Math.Atan2(t0.Y, t0.X);
          _fromAngleKnown = true;
        }

        return _fromAngle;
      }
    }
    public double ToAngle
    {
      get
      {
        if (_toAngleKnown == false)
        {
          IPoint t0 = _line.Segments.Last.TangentAt(1);

          _toAngle = Math.Atan2(-t0.Y, -t0.X);
          _toAngleKnown = true;
        }

        return _toAngle;
      }
    }

    public LineListPolygon LeftPoly
    {
      get { return _leftPoly; }
      internal set { _leftPoly = value; }
    }

    public IPoint LeftCentroid
    {
      get { return _leftCentroid; }
      internal set { _leftCentroid = value; }
    }

    public LineListPolygon RightPoly
    {
      get { return _rightPoly; }
      internal set { _rightPoly = value; }
    }

    public IPoint RightCentroid
    {
      get { return _rightCentroid; }
      internal set { _rightCentroid = value; }
    }

    /// <summary>
    /// return the y-coordinat of a point p where p.X == this.Extent.XMax
    /// </summary>
    /// <returns></returns>
    internal double YMax()
    {
      if (_maxCode == MaxCode.Unknown)
      { Orientation(); }
      return _yMax;
    }
    internal MaxCode Orientation()
    {
      if (_maxCode != MaxCode.Unknown)
      {
        return _maxCode;
      }
      IBox box = _line.Extent.Clone();
      double xMax = box.Max.X;
      box.Max.X = xMax + 1;
      box.Min.X = xMax;

      LinkedListNode<IPoint> n = _line.Points.First;
      while (n.Value.X < xMax) // TODO select box
      {
        n = n.Next;
      }

      LinkedListNode<IPoint> n_ = n.Previous;
      if (n_ == null)
      {
        _maxCode = MaxCode.StartMax;
        return _maxCode;
      }
      LinkedListNode<IPoint> n1 = n.Next;
      if (n1 == null)
      {
        _maxCode = MaxCode.EndMax;
        return _maxCode;
      }

      IPoint p;

      p = n.Value;
      xMax = p.X;
      _yMax = p.Y;


      p = n_.Value;
      double dx0 = p.X - xMax;
      double dy0 = p.Y - _yMax;

      p = n1.Value;
      double dx1 = p.X - xMax;
      double dy1 = p.Y - _yMax;

      int sign = Math.Sign(dx0 * dy1 - dy0 * dx1);
      if (sign < 0)
      {
        _maxCode = MaxCode.ClockWise;
      }
      else
      { _maxCode = MaxCode.CounterClockWise; }

      return _maxCode;
    }

    public void ClearPoly()
    {
      _rightPoly = null;
      _leftPoly = null;
    }
  }
}
