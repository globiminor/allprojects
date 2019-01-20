
using Basics.Geom;
using System;
using System.Collections.Generic;
using System.IO;

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

    public Dictionary<int, Volume> GetVolumns(TextReader reader, out Dictionary<int, double> densities)
    {
      Dictionary<int, IPoint> co = new Dictionary<int, IPoint>();
      Dictionary<int, IReadOnlyList<int>> pl = new Dictionary<int, IReadOnlyList<int>>();
      Dictionary<int, IReadOnlyList<int>> vm = new Dictionary<int, IReadOnlyList<int>>();
      Dictionary<int, double> rho = new Dictionary<int, double>();
      ReadData(reader, co, pl, vm, rho);

      Dictionary<int, Volume> volumes = new Dictionary<int, Volume>();
      foreach (var pair in vm)
      {
        volumes.Add(pair.Key, CreateVolume(pair.Value, pl, co));
      }
      densities = rho;
      return volumes;
    }

    private static Volume CreateVolume(IReadOnlyList<int> planeIndices, Dictionary<int, IReadOnlyList<int>> pl, Dictionary<int, IPoint> co)
    {
      Volume v = new Volume();
      foreach (var planeIndex in planeIndices)
      {
        SignedArea area = new SignedArea();
        area.Border.Add(CreateLine(pl[Math.Abs(planeIndex)], co));
        area.Sign = (planeIndex >= 0) ? 1 : -1;
        v.Planes.Add(area);
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
      if (line.Points.First.Value != line.Points.Last.Value)
      {
        line.Add(line.Points.First.Value);
      }
      return line;
    }

  }

  public class Volume : IVolume
  {
    private List<SignedArea> _planes = new List<SignedArea>();

    IReadOnlyList<ISignedArea> IVolume.Planes => Planes;
    public List<SignedArea> Planes { get { return _planes; } }
  }

  public class SignedArea : Area, ISignedArea
  {
    public int Sign { get; set; }
  }
}
