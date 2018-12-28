using System;
using System.Collections.Generic;

namespace Basics.Geom.Network
{
  public class TopologicalLine
  {
    private ICurve _line;

    private double? _fromAngle;
    private double? _toAngle;

    private LineListPolygon _leftPoly;
    private LineListPolygon _rightPoly;

    private IPoint _leftCentroid;
    private IPoint _rightCentroid;

    private MaxCode _maxCode = 0;
    private readonly double _yMax;

    internal enum MaxCode
    {
      StartMax = -2,
      ClockWise = -1,
      Unknown = 0,
      CounterClockWise = 1,
      EndMax = 2
    }

    public TopologicalLine(ICurve line)
    {
      _line = line;
    }

    public ICurve Line
    {
      get { return _line; }
    }

    public IPoint FromPoint
    {
      get { return _line.PointAt(_line.ParameterRange.Min); }
    }

    public IPoint ToPoint
    {
      get { return _line.PointAt(_line.ParameterRange.Max); }
    }

    public double FromAngle
    {
      get
      {
        if (!_fromAngle.HasValue)
        {
          IPoint t0 = _line.TangentAt(_line.ParameterRange.Min)[0];
          _fromAngle = Math.Atan2(t0.Y, t0.X);
        }
        return _fromAngle.Value;
      }
    }
    public double ToAngle
    {
      get
      {
        if (!_toAngle.HasValue)
        {
          IPoint t0 = _line.TangentAt(_line.ParameterRange.Max)[0];
          _toAngle = Math.Atan2(-t0.Y, -t0.X);
        }
        return _toAngle.Value;
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
      Box box = new Box(_line.Extent);
      double xMax = box.Max.X;
      box.Max.X = xMax + 1;
      box.Min.X = xMax;

      Curve pre = null;
      Curve at = null;
      Curve next = null;
      foreach (var seg in _line.Segments)
      {
        if (at != null)
        {
          next = seg;
          break;
        }
        if (seg.Extent.Max.X == xMax)
        {
          at = seg;
          continue;
        }
        pre = seg;
      }
      if (pre == null && at.PointAt(0).X == xMax)
      {
        _maxCode = MaxCode.StartMax;
        return _maxCode;
      }
      if (next == null && at.PointAt(1).X == xMax)
      {
        _maxCode = MaxCode.EndMax;
        return _maxCode;
      }

      if (at.PointAt(1).X == xMax)   // if all curves are linear, and in several other cases
      {  
        Point atTan = at.TangentAt(1);
        Point nextTan = next.TangentAt(0);
        double p = PointOperator.VectorProduct(atTan, nextTan);
        int sign = Math.Sign(p);
        if (sign < 0)
        { _maxCode = MaxCode.ClockWise; }
        else if (sign > 0)
        { _maxCode = MaxCode.CounterClockWise; }
        else if (atTan.Y > 0)
        { _maxCode = MaxCode.CounterClockWise; }
        else
        { _maxCode = MaxCode.ClockWise; }
      }
      else
      {
        IRelParamGeometry g = at;
        IBox range = g.ParameterRange;
        IPoint s = g.PointAt(range.Min);
        IPoint e = g.PointAt(range.Max);

        throw new NotImplementedException();
        //g.NormedMaxOffset;
      }

      return _maxCode;
    }

    public void ClearPoly()
    {
      _rightPoly = null;
      _leftPoly = null;
    }
  }
}
