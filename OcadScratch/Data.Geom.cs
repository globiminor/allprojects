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
      return new Basics.Geom.Arc(Center.GetGeometry(), Radius, 0, 2 * Math.PI);
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
