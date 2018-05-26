using System;
using System.Collections.Generic;
using Basics.Geom;

namespace ArcSde.Data
{
  static class Convert
  {
    public static SeShape ToShape(IGeometry geom, SeLayer lyr)
    {
      SeShape shape = new SeShape(lyr.LayerInfo.CoordRef);
      if (geom is Area area)
      {
        List<Se_Point> points = new List<Se_Point>();
        List<int> parts = new List<int>();
        foreach (Polyline polyline in area.Border)
        {
          parts.Add(points.Count);
          foreach (IPoint point in polyline.Points)
          {
            Se_Point p = new Se_Point(point.X, point.Y);
            points.Add(p);
          }
        }
        CApi.SE_shape_generate_polygon(points.Count, parts.Count, parts.ToArray(), points.ToArray(),
          null, null, shape.Ptr);
      }
      else if (geom is IBox box)
      {
        Se_Point[] points = new Se_Point[]
          {
            new Se_Point(box.Min.X, box.Min.Y),
            new Se_Point(box.Min.X, box.Max.Y),
            new Se_Point(box.Max.X, box.Max.Y),
            new Se_Point(box.Max.X, box.Min.Y),
            new Se_Point(box.Min.X, box.Min.Y)
          };
        List<int> parts = new List<int>();
        parts.Add(0);
        CApi.SE_shape_generate_polygon(points.Length, parts.Count, parts.ToArray(), points,
          null, null, shape.Ptr);
      }
      else
      {
        throw new NotImplementedException("Unhandled Geometry type " + geom.GetType());
      }
      return shape;
    }

    private static void Check(int rc)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero, rc);
    }

    public static IGeometry ToGeom(SeShape shape)
    {
      if (CApi.SE_shape_is_point(shape.Ptr))
      {
        Se_Point[] points = new Se_Point[1];
        double[] zs = new double[1];
        double[] ms = new double[1];
        CApi.SE_shape_get_points(shape.Ptr, 1, 0, null, points, zs, ms);

        return new Point3D(points[0].X, points[0].Y, zs[0]);
      }

      Check(CApi.SE_shape_get_num_parts(shape.Ptr, out int nrParts, out int nrSubparts));

      PolylineCollection lines = new PolylineCollection();
      for (int iPart = 1; iPart <= nrParts; iPart++)
      {
        int[] offsets = new int[nrSubparts];
        ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero, CApi.SE_shape_get_num_points(shape.Ptr, iPart, 0, out int nrPoints));
        Se_Point[] points = new Se_Point[nrPoints];
        double[] z = new double[nrPoints];
        double[] m = new double[nrPoints];
        ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero, CApi.SE_shape_get_points(shape.Ptr, iPart, 0, offsets, points, z, m));

        for (int iSub = 0; iSub < nrSubparts; iSub++)
        {
          if (iSub > 0 && offsets[iSub] == 0)
          { continue; }

          int iEnd = nrPoints;
          if (iSub + 1 < nrSubparts && offsets[iSub + 1] > 0)
          {
            iEnd = offsets[iSub + 1];
          }
          Polyline line = new Polyline();
          for (int iPoint = offsets[iSub]; iPoint < iEnd; iPoint++)
          {
            Point3D p = new Point3D(points[iPoint].X, points[iPoint].Y, z[iPoint]);
            line.Add(p);
          }
          lines.Add(line);
        }
      }

      if (CApi.SE_shape_is_line(shape.Ptr))
      {
        if (lines.Count == 1) return lines[0];
        return lines;
      }
      else if (CApi.SE_shape_is_simple_line(shape.Ptr))
      {
        if (lines.Count == 1) return lines[0];
        return lines;
      }
      else if (CApi.SE_shape_is_polygon(shape.Ptr))
      {
        Area area = new Area(lines);
        return area;
      }

      else
      { throw new NotImplementedException("Unhandled geometry type"); }
    }
  }
}
