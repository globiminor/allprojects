using System;

namespace OMapScratch
{
  partial interface IDrawable
  {
    Basics.Geom.IGeometry GetGeometry();
  }

  partial interface ISegment
  {
    Basics.Geom.Curve GetGeometry();
  }

  partial class Pnt
  {
    Basics.Geom.IGeometry IDrawable.GetGeometry()
    { return GetGeometry(); }
    public Basics.Geom.Point2D GetGeometry()
    {
      return new Basics.Geom.Point2D(X, Y);
    }
  }

  partial class Map
  {
    private class MyPrjGlobal : Basics.Geom.IProjection
    {
      private readonly double _x0;
      private readonly double _y0;

      public MyPrjGlobal(double x0, double y0)
      {
        _x0 = x0;
        _y0 = y0;
      }

      public Basics.Geom.IPoint Project(Basics.Geom.IPoint point)
      {
        return new Basics.Geom.Point2D(point.X + _x0, -point.Y + _y0);
      }
    }

    public Basics.Geom.IProjection GetGlobalPrj()
    {
      double[] offset = GetOffset();
      return new MyPrjGlobal(offset[0], offset[1]);
    }

  }
  partial class Lin
  {
    Basics.Geom.Curve ISegment.GetGeometry()
    {
      return new Basics.Geom.Line(From.GetGeometry(), To.GetGeometry());
    }
  }
  partial class Circle : ISegment
  {
    Basics.Geom.Curve ISegment.GetGeometry()
    {
      return new Basics.Geom.Arc(Center.GetGeometry(), Radius, Azi, Ang);
    }
  }
  partial class Bezier : ISegment
  {
    Basics.Geom.Curve ISegment.GetGeometry()
    {
      return new Basics.Geom.Bezier(From.GetGeometry(), I0.GetGeometry(), I1.GetGeometry(), To.GetGeometry());
    }
  }

  partial class Curve
  {
    Basics.Geom.IGeometry IDrawable.GetGeometry()
    {
      Basics.Geom.Polyline line = new Basics.Geom.Polyline();
      foreach (ISegment seg in this)
      {
        line.Add(seg.GetGeometry());
      }

      return line;
    }
  }
}
