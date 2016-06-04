using System;
using System.Collections.Generic;
using Basics.Geom;

namespace Grid
{
  public interface ILockable
  {
    void LockBits();
    void UnlockBits();
  }

  public class Dir : IComparable<Dir>
  {
    public class Comparer : IComparer<Dir>, IEqualityComparer<Dir>
    {
      public int Compare(Dir x, Dir y)
      {
        return x.CompareTo(y);
      }

      public bool Equals(Dir x, Dir y)
      {
        return Compare(x, y) == 0;
      }

      public int GetHashCode(Dir obj)
      {
        return obj.Dx.GetHashCode() ^ (7 * (obj.Dy.GetHashCode()));
      }
    }

    public int Dx { get; private set; }
    public int Dy { get; private set; }
    public Dir(int dx, int dy)
    {
      Dx = dx;
      Dy = dy;
    }

    public int CompareTo(Dir other)
    {
      int d = Dx.CompareTo(other.Dx);
      if (d != 0) { return d; }
      return Dy.CompareTo(other.Dy);
    }

    public static readonly Dir N = new Dir(0, 1);
    public static readonly Dir W = new Dir(-1, 0);
    public static readonly Dir Sw = new Dir(-1, -1);
    public static readonly Dir S = new Dir(0, -1);
    public static readonly Dir E = new Dir(1, 0);
    public static readonly Dir Ne = new Dir(1, 1);

    private static LinkedList<Dir> _meshDirs;
    private static LinkedList<Dir> MeshDirs
    { get { return _meshDirs ?? (_meshDirs = new LinkedList<Dir>(new[] { N, W, Sw, S, E, Ne })); } }
    private static Dictionary<Dir, LinkedListNode<Dir>> _meshDirDict;
    private static Dictionary<Dir, LinkedListNode<Dir>> MeshDirDict
    {
      get
      {
        if (_meshDirDict == null)
        {
          Dictionary<Dir, LinkedListNode<Dir>> dict = new Dictionary<Dir, LinkedListNode<Dir>>(new Comparer());
          LinkedListNode<Dir> n = MeshDirs.First;
          while (n != null)
          {
            dict.Add(n.Value, n);
            n = n.Next;
          }
          _meshDirDict = dict;
        }
        return _meshDirDict;
      }
    }

    public static Dir GetNextTriDir(Dir dir)
    {
      LinkedListNode<Dir> node;
      if (!MeshDirDict.TryGetValue(dir, out node))
      {
        return null;
      }
      LinkedListNode<Dir> next = node.Next ?? node.List.First;
      return new Dir(-next.Value.Dx, -next.Value.Dy);
    }
    public static Dir GetPreTriDir(Dir dir)
    {
      LinkedListNode<Dir> node;
      if (!MeshDirDict.TryGetValue(dir, out node))
      {
        return null;
      }
      LinkedListNode<Dir> pre = node.Previous ?? node.List.Last;
      return new Dir(-pre.Value.Dx, -pre.Value.Dy);
    }
  }

  public interface IGrid
  {
    GridExtent Extent { get; }
    Type Type { get; }
    object this[int ix, int iy] { get; set; }
  }

  public interface IGrid<T> : IGrid
  {
    T Value(double x, double y);
    new T this[int ix, int iy] { get; set; }
  }

  public abstract class BaseGrid : IGrid
  {
    private GridExtent _extent;
    private Type _type;

    public BaseGrid(GridExtent extent, Type type)
    {
      _extent = extent;
      _type = type;
    }
    public object Value(double x, double y)
    {
      int ix, iy;
      _extent.GetNearest(x, y, out ix, out iy);
      return this[ix, iy]; // this[ix, iy];
    }

    protected class GridPoint : Point3D
    {
      private readonly IGrid _grid;
      private readonly int _x;
      private readonly int _y;
      public GridPoint(IGrid grid, int x, int y)
        : base(grid.Extent.X0 + x * grid.Extent.Dx, grid.Extent.Y0 + y * grid.Extent.Dx, 0)
      {
        _grid = grid;
        _x = x;
        _y = y;
      }
    }

    public T Offset<T>(int ix, int iy) where T : BaseGrid
    { return null; }
    public object this[int ix, int iy]
    {
      get { return GetThis(ix, iy); }
      set { SetThis(ix, iy, value); }
    }
    protected abstract object GetThis(int ix, int iy);
    protected abstract void SetThis(int ix, int iy, object value);

    public Type Type
    { get { return _type; } }

    public GridExtent Extent
    {
      get { return _extent; }
    }

    public int Topology
    {
      get
      { return 2; }
    }

    private class TopComparer : IComparer<Line>
    {
      public int Compare(Line x, Line y)
      {
        int i = -x.Start.Y.CompareTo(y.Start.Y);
        return i;
      }
    }
    private class InversDoubleComparer : IComparer<double>
    {
      public int Compare(double x, double y)
      {
        int i = -x.CompareTo(y);
        return i;
      }
    }

    private class BottomComparer : IComparer<Line>
    {
      public int Compare(Line x, Line y)
      {
        int i = -x.End.Y.CompareTo(y.End.Y);
        return i;
      }
    }

    public IEnumerable<int[]> EnumerateCells(Polyline line, double width, bool connected)
    {
      Polyline gen = line.Linearize(_extent.Dx);
      IPoint p0 = null;
      foreach (IPoint p1 in gen.Points)
      {
        if (p0 != null)
        {
          Point d = Point.Sub(p1, p0);
          double l = Math.Sqrt(d.OrigDist2());
          double f = width / (2 * l);
          Point v = new Point2D(f * d.Y, -f * d.X);
          Point q00 = p0 + v;
          Point q01 = p0 - v;
          Point q10 = p1 + v;
          Point q11 = p1 - v;
          List<Line> lines = new List<Line>(4);
          lines.Add(GetLine(q00, q01));
          lines.Add(GetLine(q01, q11));
          lines.Add(GetLine(q11, q10));
          lines.Add(GetLine(q10, q00));

          foreach (int[] cell in EnumerateCells(lines))
          {
            yield return cell;
          }
        }

        p0 = p1;
      }
    }
    private Line GetLine(IPoint x, IPoint y)
    {
      Line line;
      if (x.Y >= y.Y)
      { line = new Line(x, y); }
      else
      { line = new Line(y, x); }
      return line;
    }

    public IEnumerable<int[]> EnumerateCells(Area area)
    {
      IList<Polyline> borders = area.Border;
      int alloc = 0;
      foreach (Polyline border in borders)
      { alloc += border.Points.Count; }
      List<Line> lines = new List<Line>(alloc);

      foreach (Polyline border in borders)
      {
        Polyline linears = border.Generalize(_extent.Dx);
        foreach (Line line in linears.Segments)
        {
          lines.Add(GetLine(line.Start, line.End));
        }
      }
      if (lines.Count == 0)
      {
        yield break;
      }
      foreach (int[] cell in EnumerateCells(lines))
      {
        yield return cell;
      }
    }

    private IEnumerable<int[]> EnumerateCells(List<Line> lines)
    {
      TopComparer cmp = new TopComparer();
      lines.Sort(cmp);

      double iy0 = (lines[0].Start.Y - _extent.Y0) / _extent.Dy;
      iy0 = Math.Max(0, iy0);
      iy0 = Math.Min(_extent.Ny, iy0);

      int iLine = 0;
      int nLines = lines.Count;
      int iy1 = _extent.Ny;
      SortedList<double, List<Line>> current = new SortedList<double, List<Line>>(new InversDoubleComparer());
      List<double> xCuts = new List<double>();

      for (int iy = (int)(Math.Ceiling(iy0));
           iy < iy1 && (iLine < nLines || current.Count > 0);
           iy++)
      {
        double y0 = _extent.Y0 + iy * _extent.Dy;
        // Remove handled lines
        while (current.Count > 0 && current.Keys[0] >= y0)
        { current.RemoveAt(0); }

        // add new lines
        while (iLine < nLines && lines[iLine].Start.Y >= y0)
        {
          Line line = lines[iLine];
          double yEnd = line.End.Y;
          if (yEnd < y0)
          {
            List<Line> yLines;
            if (!current.TryGetValue(yEnd, out yLines))
            {
              yLines = new List<Line>();
              current.Add(yEnd, yLines);
            }
            yLines.Add(line);
          }
          iLine++;
        }

        xCuts.Clear();
        foreach (List<Line> cutLines in current.Values)
        {
          foreach (Line cutLine in cutLines)
          {
            double f = (y0 - cutLine.Start.Y) / (cutLine.End.Y - cutLine.Start.Y);
            double xCut = cutLine.Start.X + f * (cutLine.End.X - cutLine.Start.X);
            xCuts.Add(xCut);
          }
        }
        xCuts.Sort();
        for (int i = 0; i < xCuts.Count; i += 2)
        {
          double x0 = xCuts[i];
          double x1 = xCuts[i + 1];
          double ix0 = (x0 - _extent.X0) / _extent.Dx;
          double ix1 = (x1 - _extent.X0) / _extent.Dx;
          ix0 = Math.Max(ix0, 0);
          ix0 = Math.Min(ix0, _extent.Nx);
          ix1 = Math.Max(ix1, 0);
          ix1 = Math.Min(ix1, _extent.Nx);
          for (int ix = (int)Math.Ceiling(ix0); ix <= (int)Math.Floor(ix1); ix++)
          {
            yield return new int[] { ix, iy };
          }
        }
      }
    }
  }

  public abstract class BaseGrid<T> : BaseGrid, IGrid<T>
  {
    public BaseGrid(GridExtent extent)
      : base(extent, typeof(T))
    { }
    public new virtual T Value(double x, double y)
    {
      return (T)base.Value(x, y);
    }
    public new abstract T this[int ix, int iy] { get; set; }
    protected sealed override object GetThis(int ix, int iy)
    { return this[ix, iy]; }
    protected sealed override void SetThis(int ix, int iy, object value)
    { this[ix, iy] = (T)value; }
  }
}
