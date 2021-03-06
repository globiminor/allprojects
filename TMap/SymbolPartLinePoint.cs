using Basics.Geom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TMap
{
  public class SymbolPartLinePoint : SymbolPart
  {
    [Flags]
    public enum LinePointType
    {
      StartPoint = 1,
      EndPoint = 2,
      DashPoint = 4,
      VertexPoint = 8
    }
    private LinePointType _linePointType;
    SymbolPartPoint _basePoint;
    private readonly List<double> _dash;

    public SymbolPartLinePoint(LinePointType linePointType)
    {
      _linePointType = linePointType;
      _basePoint = new SymbolPartPoint();

      DrawLevel = 0;

      _dash = new List<double>();
    }
    public SymbolPartPoint Point
    {
      get { return _basePoint; }
      set { _basePoint = value; }
    }
    public LinePointType PointType
    {
      get { return _linePointType; }
      set { _linePointType = value; }
    }
    public List<double> Dash
    {
      get { return _dash; }
    }

    public override string GetDrawExpressions() => null;
    public override void Draw(IGeometry geometry, IDrawable drawable)
    {
      Polyline line = (Polyline)geometry;
      _basePoint.Tag = Tag;
      if ((_linePointType & LinePointType.StartPoint) != 0)
      {
        _basePoint.Draw(Basics.Geom.Point.CastOrWrap(line.Points[0]), drawable);
      }
      if ((_linePointType & LinePointType.EndPoint) != 0)
      {
        _basePoint.Draw(Basics.Geom.Point.CastOrWrap(line.Points.Last()), drawable);
      }
      if ((_linePointType & LinePointType.VertexPoint) != 0)
      {
        bool first = false;
        foreach (var curve in line.EnumSegments())
        {
          if (first)
          {
            IPoint p = curve.PointAt(0);
            IPoint t = curve.TangentAt(0);
            double rot = 0;
            if (t.X != 0 || t.Y != 0)
            { rot = Math.Atan2(t.Y, t.X); }
            _basePoint.Draw(p, drawable, 1, rot);
          }
          first = true;
        }

        //LinkedListNode<Geometry.Point> node = line.Points.First.Next;
        //while (node != null && node.Next != null)
        //{
        //  _basePoint.Draw(node.Value, properties, drawable);
        //  node = node.Next;
        //}
      }
      if ((_linePointType & LinePointType.DashPoint) != 0)
      {
        IList<double> dash = _dash;
        if (dash == null || dash.Count == 0)
        { dash = new double[] { 20 }; }
        if (DashAdjust)
        {
          dash = new List<double>(dash);
          double d0 = line.Project(Geometry.ToXY).Length();
        }
        int iDash = 0;
        double offset = DashOffset;
        while (offset > dash[iDash])
        {
          offset -= dash[iDash];
          iDash++;
          if (iDash >= _dash.Count)
          { iDash--; }
        }
        double d = dash[iDash] - offset;

        IEnumerator<ISegment> curves = line.EnumSegments().GetEnumerator();
        curves.Reset();
        curves.MoveNext();
        ISegment c = curves.Current;
        double l = c.Length();
        while (true)
        {
          if (l <= d)
          {
            d = d - l;
            if (curves.MoveNext())
            {
              c = curves.Current;
              l = c.Length();
              continue;
            }
            else
            { break; }
          }
          double par = c.ParamAt(d);
          IPoint p = c.PointAt(par);
          IPoint t = c.TangentAt(par);

          double rot = 0;
          if (t.X != 0 || t.Y != 0)
          { rot = Math.Atan2(t.Y, t.X); }
          _basePoint.Draw(p, drawable, 1, rot);

          iDash++;
          if (iDash >= _dash.Count)
          { iDash = 0; }
          d += dash[iDash];
        }
      }
    }
    public override double Size()
    {
      return _basePoint.Size();
    }
    public override int Topology
    {
      get { return 1; }
    }
  }
}