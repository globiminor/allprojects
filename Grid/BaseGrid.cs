using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Grid
{
  public interface ILockable
  {
    void LockBits();
    void UnlockBits();
  }

  public interface ICell
  {
    int Ix { get; }
    int Iy { get; }
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

    public override string ToString()
    {
      return $"Dir Dx:{Dx}, Dy:{Dy}";
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
    public static LinkedList<Dir> MeshDirs
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
      if (!MeshDirDict.TryGetValue(dir, out LinkedListNode<Dir> node))
      {
        return null;
      }
      LinkedListNode<Dir> next = node.Next ?? node.List.First;
      return new Dir(-next.Value.Dx, -next.Value.Dy);
    }
    public static Dir GetPreTriDir(Dir dir)
    {
      if (!MeshDirDict.TryGetValue(dir, out LinkedListNode<Dir> node))
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
  public interface IGridStoreType
  {
    Type StoreType { get; }
  }
  public interface IExtractable
  {
    IGrid Extract(IBox extent);
  }

  public interface IGrid<T> : IGrid
  {
    T Value(double x, double y);
    new T this[int ix, int iy] { get; set; }
  }

  public interface IPartialFilledGrid<T> : IGrid<T>
  {
    IGrid<T> GetGridWithData();
  }


  public class OffsetGrid<T> : OffsetGrid, IGrid<T>
  {
    public OffsetGrid(IGrid<T> master, int ox, int oy, int nx, int ny)
      : base(master, ox, oy, nx, ny)
    { }

    protected new IGrid<T> Master { get { return (IGrid<T>)base.Master; } }

    T IGrid<T>.this[int ix, int iy]
    {
      get
      {
        object b = base[ix, iy];
        if (!(b is T t))
        {
          t = (T)Convert.ChangeType(base[ix, iy], typeof(T));
        }
        return t;
      }
      set { base[ix, iy] = value; }
    }

    public T Value(double x, double y)
    {
      Extent.GetNearest(x, y, out int ix, out int iy);
      IGrid<T> t = this;
      return t[ix, iy];
    }
  }
  public class OffsetGrid : IGrid
  {
    private readonly IGrid _master;
    private readonly int _ox;
    private readonly int _oy;
    private readonly GridExtent _offsetExtent;
    public OffsetGrid(IGrid master, int ox, int oy, int nx, int ny)
    {
      _master = master;
      _ox = ox;
      _oy = oy;

      GridExtent m = master.Extent;
      _offsetExtent = new GridExtent(nx, ny, m.X0 + ox * m.Dx, m.Y0 + oy * m.Dy, m.Dx, m.Dy);
    }

    protected IGrid Master { get { return _master; } }

    public GridExtent Extent { get { return _offsetExtent; } }
    public Type Type { get { return _master.Type; } }

    public object this[int ix, int iy]
    {
      get
      {
        if (ix < 0 || iy < 0 || ix >= Extent.Nx || iy >= Extent.Ny)
        { return Extent.NN; }
        int tx = ix + _ox;
        int ty = iy + _oy;
        if (tx < 0 || ty < 0 || tx >= _master.Extent.Nx || ty >= _master.Extent.Ny)
        { return _master.Extent.NN; }
        return _master[tx, ty];
      }
      set
      {
        int tx = ix + _ox;
        int ty = iy + _oy;
        _master[tx, ty] = value;
      }
    }
  }

  public abstract class BaseGrid : IGrid
  {
    private readonly GridExtent _extent;
    private readonly Type _type;

    public static GridExtent GetOffsetExtent(IGrid master, IBox extent, out int offsetX, out int offsetY)
    {
      double t0 = master.Extent.Dx > 0 ? extent.Min.X : extent.Max.X;
      int ox = (int)Math.Floor((t0 - master.Extent.X0) / master.Extent.Dx);
      t0 = master.Extent.Dy > 0 ? extent.Min.Y : extent.Max.Y;
      int oy = (int)Math.Floor((t0 - master.Extent.Y0) / master.Extent.Dy);

      double x0 = master.Extent.X0 + ox * master.Extent.Dx;
      double y0 = master.Extent.Y0 + oy * master.Extent.Dy;

      int nx = (int)Math.Ceiling((Math.Max(x0, extent.Max.X) - Math.Min(x0, extent.Min.X)) / Math.Abs(master.Extent.Dx));
      int ny = (int)Math.Ceiling((Math.Max(y0, extent.Max.Y) - Math.Min(y0, extent.Min.Y)) / Math.Abs(master.Extent.Dy));
      GridExtent gridExtent = new GridExtent(nx, ny, x0, y0, master.Extent.Dx, master.Extent.Dy);

      offsetX = ox;
      offsetY = oy;
      return gridExtent;
    }
    public BaseGrid(GridExtent extent, Type type)
    {
      _extent = extent;
      _type = type;
    }
    public object Value(double x, double y)
    {
      _extent.GetNearest(x, y, out int ix, out int iy);
      return this[ix, iy]; // this[ix, iy];
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

    public Type Type => _type;

    public GridExtent Extent => _extent;

    public int Topology { get { return 2; } }

    public static IGrid<T> TryReduce<T>(IGrid<T> grid)
    {
      if (grid is IPartialFilledGrid<T> reduceable)
      {
        return reduceable.GetGridWithData();
      }
      return grid;
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
      foreach (var p1 in gen.Points)
      {
        if (p0 != null)
        {
          Point d = PointOp.Sub(p1, p0);
          double l = Math.Sqrt(d.OrigDist2());
          double f = width / (2 * l);
          Point v = new Point2D(f * d.Y, -f * d.X);
          Point q00 = p0 + v;
          Point q01 = p0 - v;
          Point q10 = p1 + v;
          Point q11 = p1 - v;
          List<Line> lines = new List<Line>(4)
          {
            GetLine(q00, q01),
            GetLine(q01, q11),
            GetLine(q11, q10),
            GetLine(q10, q00)
          };

          foreach (var cell in EnumerateCells(lines))
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
      foreach (var border in borders)
      { alloc += border.Points.Count; }
      List<Line> lines = new List<Line>(alloc);

      foreach (var border in borders)
      {
        Polyline linears = border.Generalize(_extent.Dx);
        foreach (var line in linears.EnumSegments())
        {
          lines.Add(GetLine(line.Start, line.End));
        }
      }
      if (lines.Count == 0)
      {
        yield break;
      }
      foreach (var cell in EnumerateCells(lines))
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
            if (!current.TryGetValue(yEnd, out List<Line> yLines))
            {
              yLines = new List<Line>();
              current.Add(yEnd, yLines);
            }
            yLines.Add(line);
          }
          iLine++;
        }

        xCuts.Clear();
        foreach (var cutLines in current.Values)
        {
          foreach (var cutLine in cutLines)
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

  public class SimpleGrid<T> : BaseGrid<T>
  {
    private readonly T[,] _value;
    public SimpleGrid(GridExtent extent)
      : base(extent)
    {
      _value = new T[extent.Nx, extent.Ny];
    }
    public override T this[int ix, int iy]
    { get => _value[ix, iy]; set => _value[ix, iy] = value; }
  }

  public abstract class TiledGrid<T> : BaseGrid<T>, IPartialFilledGrid<T>
  {
    private readonly Dictionary<Lcp.Field, IGrid<T>> _grids;
    private int _tileSize;
    protected TiledGrid(GridExtent extent, int tileSize = 128)
      : base(extent)
    {
      _tileSize = tileSize;
      _grids = new Dictionary<Lcp.Field, IGrid<T>>(new Lcp.FieldComparer());
    }

    public int TileSize
    {
      get { return _tileSize; }
      set
      {
        if (_tileSize == value)
        { return; }
        if (_grids.Count > 0)
        { throw new InvalidOperationException("Tiles already initiated"); }

        _tileSize = value;
      }
    }

    public bool DoInitOnRead { get; set; }
    public override T this[int ix, int iy]
    {
      get
      {
        Lcp.Field key = new Lcp.Field { X = ix / _tileSize, Y = iy / _tileSize };
        if (!_grids.TryGetValue(key, out IGrid<T> tile))
        {
          if (!DoInitOnRead)
            return default;

          GridExtent e = Extent;
          tile = CreateTile(TileSize, TileSize, e.X0 + key.X * TileSize * e.Dx, e.Y0 + key.Y * TileSize * e.Dy);
          _grids.Add(key, tile);
        }

        return tile[ix - key.X * _tileSize, iy - key.Y * _tileSize];
      }
      set
      {
        Lcp.Field key = new Lcp.Field { X = ix / _tileSize, Y = iy / _tileSize };
        if (!_grids.TryGetValue(key, out IGrid<T> tile))
        {
          GridExtent e = Extent;
          tile = CreateTile(TileSize, TileSize, e.X0 + key.X * TileSize * e.Dx, e.Y0 + key.Y * TileSize * e.Dy);
          _grids.Add(key, tile);
        }
        tile[ix - key.X * _tileSize, iy - key.Y * _tileSize] = value;
      }
    }

    IGrid<T> IPartialFilledGrid<T>.GetGridWithData()
    { return GetGridWithData(); }
    public IGrid<T> GetGridWithData()
    {
      int xMin = Extent.Nx;
      int xMax = 0;
      int yMin = Extent.Ny;
      int yMax = 0;

      foreach (var key in _grids.Keys)
      {
        xMin = Math.Min(xMin, key.X);
        xMax = Math.Max(xMax, key.X);

        yMin = Math.Min(yMin, key.Y);
        yMax = Math.Max(yMax, key.Y);
      }
      if (xMin > xMax)
      { return null; }

      return new OffsetGrid<T>(this, xMin * TileSize, yMin * TileSize, (1 + xMax - xMin) * TileSize, (1 + yMax - yMin) * TileSize);
    }

    protected abstract IGrid<T> CreateTile(int nx, int ny, double x0, double y0);
  }
}
