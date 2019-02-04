
using System;
using System.Collections.Generic;
using Basics.Geom;
using Basics.Geom.Projection;
using Ocad;

namespace OCourse.Tracking
{
  class Track
  {
    private readonly Course _course;
    private IList<TrackPoint> _track;
    public Track(Course course, IList<TrackPoint> track)
    {
      _course = course;
      _track = track;
    }

    public void Fit()
    {
      double lon0 = 0;
      if (_track.Count > 0)
      {
        lon0 = (int)(_track[0].Long / 6) + 3;
      }
      Projection prj = new Utm();
      List<IPoint> utm = new List<IPoint>();
      foreach (var tp in _track)
      {
        Point gg = new Point2D((tp.Long - lon0) * Math.PI / 180.0,
          tp.Lat * Math.PI / 180.0);

        IPoint p = prj.Gg2Prj(gg);
        utm.Add(p);
      }

      IPoint center = GetCenter(utm);
    }

    private IPoint GetCenter(IList<IPoint> line)
    {
      Point center = new Point2D();
      IPoint p1 = null;
      double sumW = 0;
      foreach (var point in line)
      {
        IPoint p0 = p1;
        p1 = point;

        if (p0 == null)
        { continue; }
        double w = Math.Sqrt(PointOp.Dist2(p0, p1, GeometryOperator.DimensionXY));

        center = center + w / 2.0 * (Point.CastOrCreate(p0) + Point.CastOrCreate(p1));
        sumW += w;
      }

      center = (1 / sumW) * center;
      return center;
    }
  }
}
