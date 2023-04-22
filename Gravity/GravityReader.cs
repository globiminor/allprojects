
using Basics.Geom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gravity
{

  public class GravityReader
  {
    public const int CORNERS_KNOWN = 1 << 0;
    public const int PLANES_KNOWN = 1 << 1;
    public const int VOLUMES_KNOWN = 1 << 2;
    public const int DENSITIES_KNOWN = 1 << 3;

    public static void GpInput(TextReader fi, ref string line, Dictionary<int, IReadOnlyList<int>> gp)
    {
      while (line != null)
      {
        IList<string> parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Count <= 0)
        { return; }
        if (!int.TryParse(parts[0], out int i))
        { return; }

        List<int> indices = new List<int>();
        for (int iPart = 1; iPart < parts.Count; iPart++)
        {
          if (parts[iPart] == "end")
          {
            gp[i] = indices;
            line = fi.ReadLine();
          }
          if (!int.TryParse(parts[iPart], out int index))
          { return; }
          indices.Add(index);
        }
      }
    }

    private int ReadGeometry(TextReader fi, ref string comm, Dictionary<int, IPoint> co,
      Dictionary<int, IReadOnlyList<int>> pl, Dictionary<int, IReadOnlyList<int>> vm)
    {
      if ("points3D" == comm)
      {
        while ((comm = fi.ReadLine()) != null)
        {
          IList<string> parts = comm.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Count >= 4 && int.TryParse(parts[0], out int i) && double.TryParse(parts[1], out double x)
            && double.TryParse(parts[2], out double y) && double.TryParse(parts[3], out double z))
          {
            co[i] = new Point3D(x, y, z);
          }
          else break;
        }
        return CORNERS_KNOWN;
      }

      if ("planes" == comm)
      {
        GpInput(fi, ref comm, pl);
        return PLANES_KNOWN;
      }

      if ("volumes" == comm)
      {
        GpInput(fi, ref comm, vm);
        return VOLUMES_KNOWN;
      }

      return 0;
    }

    private int PotenzInput0(TextReader fi, ref string comm,
      Dictionary<int, IPoint> co,
      Dictionary<int, IReadOnlyList<int>> pl,
      Dictionary<int, IReadOnlyList<int>> vm, Dictionary<int, double> rhos)
    {
      int i = ReadGeometry(fi, ref comm, co, pl, vm);
      if (i > 0)
      {
        return i;
      }

      if ("densities" == comm)
      {
        while ((comm = fi.ReadLine()) != null)
        {
          IList<string> parts = comm.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Count >= 2 && int.TryParse(parts[0], out i) && double.TryParse(parts[1], out double rho))
          {
            rhos[i] = rho;
          }
          else break;
        }
        return DENSITIES_KNOWN;
      }

      return 0;
    }

    private int ReadData(TextReader fi, Dictionary<int, IPoint> co, Dictionary<int, IReadOnlyList<int>> pl, Dictionary<int, IReadOnlyList<int>> vm,
      Dictionary<int, double> rho)
    {
      string line;
      int status;

      status = 0;
      line = fi.ReadLine();
      while (line != null)
      {
        status = status | PotenzInput0(fi, ref line, co, pl, vm, rho);
      }
      return status;
    }

    public Dictionary<int, Body> GetVolumns(TextReader reader, out Dictionary<int, double> densities)
    {
      Dictionary<int, IPoint> co = new Dictionary<int, IPoint>();
      Dictionary<int, IReadOnlyList<int>> pl = new Dictionary<int, IReadOnlyList<int>>();
      Dictionary<int, IReadOnlyList<int>> vm = new Dictionary<int, IReadOnlyList<int>>();
      Dictionary<int, double> rho = new Dictionary<int, double>();
      ReadData(reader, co, pl, vm, rho);

      Dictionary<int, Surface> areas = new Dictionary<int, Surface>();
      foreach (var pair in pl)
      {
        Surface area = new Surface(CreateLine(pair.Value, co));
        areas.Add(pair.Key, area);
      }

      Dictionary<int, Body> volumes = new Dictionary<int, Body>();
      foreach (var pair in vm)
      {
        volumes.Add(pair.Key, CreateBody(pair.Value, areas));
      }
      densities = rho;
      return volumes;
    }

    private static Body CreateBody(IReadOnlyList<int> planeIndices, Dictionary<int, Surface> areas)
    {
      Body v = new Body();
      foreach (var planeIndex in planeIndices)
      {
        SignedSurface signedArea = new SignedSurface();
        signedArea.Sign = (planeIndex >= 0) ? 1 : -1;
        signedArea.Area = areas[Math.Abs(planeIndex)];
        v.Planes.Add(signedArea);
      }
      return v;
    }

    private static Polyline CreateLine(IReadOnlyList<int> cornerIndices, Dictionary<int, IPoint> co)
    {
      Polyline line = new Polyline();
      foreach (var cornerIndex in cornerIndices)
      {
        line.Add(co[cornerIndex]);
      }
      if (line.Points[0] != line.Points.Last())
      {
        line.Add(line.Points[0]);
      }
      return line;
    }

  }

  public class Body : IBody
  {
    private readonly List<SignedSurface> _planes = new List<SignedSurface>();

    IReadOnlyList<ISignedSurface> IBody.Planes => Planes;
    public List<SignedSurface> Planes { get { return _planes; } }
  }

  public class SignedSurface: ISignedSurface
  {
    public int Sign { get; set; }

    ISimpleSurface ISignedSurface.Area => Area;
    public Surface Area { get; set; }
  }
}
