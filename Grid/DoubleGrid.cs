using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Basics.Geom;
using Grid.Lcp;

namespace Grid
{
  public interface IDoubleGrid : IGrid<double>
  {
    double Value(double x, double y, EGridInterpolation interpol);
    double Slope2(double x, double y);
  }

  public abstract class DoubleGrid : BaseGrid<double>, IDoubleGrid
  {
    private class DoubleOpGrid : DoubleGrid
    {
      private readonly EOperator _eOperator;
      private readonly IGrid<double> _dGrd0;
      private readonly IGrid<double> _dGrd1;
      private readonly double _dOperator;
      private readonly IGrid<int> _iGrd;

      private readonly double[] _dArray;
      private readonly Func<int, int, double> _opFunc;

      public DoubleOpGrid(IGrid<double> grid, Func<int, int, double> op)
        : base(grid.Extent)
      {
        _dGrd0 = grid;
        _eOperator = EOperator.none;
        _opFunc = op;
      }
      public DoubleOpGrid(IGrid<double> grd, double val, EOperator op)
      : base(grd.Extent)
      {
        _dGrd0 = grd;
        _dOperator = val;
        _eOperator = op;
      }

      public DoubleOpGrid(IGrid<double> grd0, IGrid<double> grd1, EOperator op)
      : base(grd0.Extent)
      {
        _dGrd0 = grd0;
        _dGrd1 = grd1;
        _eOperator = op;
      }

      public DoubleOpGrid(double[] dArray, IGrid<int> grd)
      : base(grd.Extent)
      {
        _dArray = dArray;
        _iGrd = grd;
        _eOperator = EOperator.bracket;
      }


      public override double this[int ix, int iy]
      {
        get
        {
          // operators
          if (_eOperator == EOperator.addVal) return _dGrd0[ix, iy] + _dOperator;
          else if (_eOperator == EOperator.addGrd)
          {
            double x = _dGrd0.Extent.X0 + ix * _dGrd0.Extent.Dx;
            double y = _dGrd0.Extent.Y0 + iy * _dGrd0.Extent.Dy;
            return _dGrd0[ix, iy] + _dGrd1.Value(x, y);
          }
          else if (_eOperator == EOperator.maxGrd)
          {
            double x = _dGrd0.Extent.X0 + ix * _dGrd0.Extent.Dx;
            double y = _dGrd0.Extent.Y0 + iy * _dGrd0.Extent.Dy;

            double g0 = _dGrd0[ix, iy];
            double g1 = _dGrd1.Value(x, y);

            if (double.IsNaN(g0))
              return g1;
            if (double.IsNaN(g1))
              return g0;

            return Math.Max(g0, g1);
          }
          else if (_eOperator == EOperator.multVal) return _dGrd0[ix, iy] * _dOperator;
          else if (_eOperator == EOperator.bracket) return _dArray[_iGrd[ix, iy]];
          else if (_eOperator == EOperator.none && _opFunc != null) return _opFunc(ix, iy);
          else throw new InvalidOperationException("Unhandled operator " + _eOperator);
        }
        set
        { throw new InvalidOperationException("Cannot set value to a calculated grid"); }
      }
    }

    protected DoubleGrid(GridExtent extent)
      : base(extent)
    { }

    public static DoubleGrid Create(double[] dArray, IGrid<int> grd)
    {
      return new DoubleOpGrid(dArray, grd);
    }

    public static DoubleGrid Filter(DoubleGrid grid, Func<int, int, double> func)
    {
      return new DoubleOpGrid(grid, func);
    }
    public static DoubleGrid Max(DoubleGrid x, DoubleGrid y)
    {
      return new DoubleOpGrid(x, y, EOperator.maxGrd);
    }
    public static DoubleGrid operator +(DoubleGrid grd, double val)
    { return Sum(grd, val); }
    public static DoubleGrid Sum(IGrid<double> grd, double val)
    {
      return new DoubleOpGrid(grd, val, EOperator.addVal);
    }
    public static DoubleGrid operator +(DoubleGrid grd0, IGrid<double> grd1)
    { return Sum(grd0, grd1); }
    public static DoubleGrid Sum(IGrid<double> grd0, IGrid<double> grd1)
    {
      return new DoubleOpGrid(grd0, grd1, EOperator.addGrd);
    }

    public static DoubleGrid operator -(DoubleGrid grd, double val)
    {
      return new DoubleOpGrid(grd, -val, EOperator.addVal);
    }
    public static DoubleGrid operator *(DoubleGrid grd, double val)
    {
      return new DoubleOpGrid(grd, val, EOperator.multVal);
    }
    public static DoubleGrid operator /(DoubleGrid grd, double val)
    {
      return new DoubleOpGrid(grd, 1.0 / val, EOperator.multVal);
    }

    public IntGrid ToIntGrid()
    { return ToIntGrid(this); }
    public static IntGrid ToIntGrid(IGrid<double> grid)
    {
      return new IntGrid(grid);
    }

    public void SaveASCII(string name, string format)
    { SaveASCII(this, name, format); }

    public static void SaveASCII(IDoubleGrid grid, string name, string format)
    {
      using (TextWriter w = new StreamWriter(name, false))
      {
        w.WriteLine("NCOLS {0}", grid.Extent.Nx);
        w.WriteLine("NROWS {0}", grid.Extent.Ny);
        w.WriteLine("XLLCORNER {0}", grid.Extent.X0);
        w.WriteLine("YLLCORNER {0}", grid.Extent.Y0 + (grid.Extent.Ny - 1) * grid.Extent.Dy);
        w.WriteLine("CELLSIZE {0}", grid.Extent.Dx);
        w.WriteLine("NODATA_VALUE {0}", grid.Extent.NN);
        for (int iy = 0; iy < grid.Extent.Ny; iy++)
        {
          for (int ix = 0; ix < grid.Extent.Nx; ix++)
          {
            double val = grid[ix, iy];
            if (double.IsNaN(val)) val = grid.Extent.NN;
            w.Write("{0} ", val.ToString(format));
          }
          w.WriteLine();
        }
      }
    }

    public void Save(string name)
    { Save(this, name, WriteValue); }
    public static void Save(IGrid<double> grid, string name)
    {
      if (grid is DoubleGrid g) { g.Save(name); }
      else
      {
        Save(grid, name, delegate (BinaryWriter writer, int ix, int iy)
        { writer.Write(grid[ix, iy]); });
      }
    }
    private static void Save(IGrid<double> grid, string name, Action<BinaryWriter, int, int> writeAction)
    {
      GetStoreProps(grid, out short iLength, out double z0, out double dz);

      using (BinaryWriter writer = new BinaryWriter(new FileStream(name, FileMode.Create)))
      {
        BinarGrid.PutHeader(writer.BaseStream, grid.Extent.Nx, grid.Extent.Ny,
          BinarGrid.EGridType.eDouble, iLength, grid.Extent.X0, grid.Extent.Y0, grid.Extent.Dx,
          z0, dz);
        writer.Seek(BinarGrid.START_DATA, SeekOrigin.Begin);

        int nx = grid.Extent.Nx;
        int ny = grid.Extent.Ny;
        for (int iy = 0; iy < ny; iy++)
        {
          for (int ix = 0; ix < nx; ix++)
          {
            writeAction(writer, ix, iy);
          }
        }
        writer.Close();
      }
    }

    private static void GetStoreProps(IGrid<double> grid, out short iLength, out double z0, out double dz)
    {
      if (grid is DoubleGrid g)
      { g.GetStoreProps(out iLength, out z0, out dz); }
      else
      { GetDefaultStoreProps(out iLength, out z0, out dz); }
    }

    protected virtual void GetStoreProps(out short iLength, out double z0, out double dz)
    {
      GetDefaultStoreProps(out iLength, out z0, out dz);
    }
    private static void GetDefaultStoreProps(out short iLength, out double z0, out double dz)
    {
      iLength = 8;
      z0 = 0;
      dz = 0;
    }

    protected virtual void WriteValue(BinaryWriter writer, int ix, int iy)
    {
      writer.Write(this[ix, iy]);
    }

    public Polyline GetContour(double x0, double y0)
    {
      Extent.GetBilinear(x0, y0, out int ix, out int iy, out double dx, out double dy);

      double hContour = Value(x0, y0, EGridInterpolation.bilinear);
      if (double.IsNaN(hContour))
      { return null; }

      double h00 = this[ix, iy];
      double h01 = this[ix, iy + 1];

      IMeshLine start;
      double h0;
      double h1;
      if (h00.CompareTo(hContour) != h01.CompareTo(hContour))
      {
        h0 = h00;
        h1 = h01;
        start = new GridMeshLine(this, ix, iy, Dir.N);
      }
      else if (h01.CompareTo(hContour) != (h1 = this[ix + 1, iy + 1]).CompareTo(hContour))
      {
        h0 = h01;
        start = new GridMeshLine(this, ix, iy + 1, Dir.E);
      }
      else if (h00.CompareTo(hContour) != (h1 = this[ix + 1, iy]).CompareTo(hContour))
      {
        h0 = h00;
        start = new GridMeshLine(this, ix + 1, iy, Dir.E);
      }
      else
      { return null; }

      if (h0 > h1)
      { start = start.Invers(); }

      double f = (hContour - h0) / (h1 - h0);
      return MeshUtils.GetContour(start, f, new GridMeshLine.Comparer());
    }

    public double Value(double x, double y, EGridInterpolation eInter)
    {
      return Value(this, x, y, eInter);
    }
    public static double Value(IGrid<double> grid, double x, double y, EGridInterpolation eInter)
    {
      if (eInter == EGridInterpolation.nearest)
      {
        return grid.Value(x, y);
      }
      else if (eInter == EGridInterpolation.bilinear)
      {
        double h00, h01, h10, h11;
        double h0, h1;

        grid.Extent.GetBilinear(x, y, out int ix, out int iy, out double dx, out double dy);

        h00 = grid[ix, iy];
        if (dx == 0)
        {
          if (dy == 0)
          {
            return h00;
          }
          h01 = grid[ix, iy + 1];
          return h00 + (h01 - h00) * dy;
        }
        if (dy == 0)
        {
          h10 = grid[ix + 1, iy];
          return h00 + (h10 - h00) * dx;
        }

        h01 = grid[ix, iy + 1];
        h10 = grid[ix + 1, iy];
        h11 = grid[ix + 1, iy + 1];

        h0 = h00 + (h10 - h00) * dx;
        h1 = h01 + (h11 - h01) * dx;
        return h0 + (h1 - h0) * dy;
      }
      else
      {
        throw new Exception("Unhandled Interpolation method " + eInter);
      }
    }

    public double Slope2(double x, double y)
    {
      return Slope2(this, x, y);
    }
    public static double Slope2(IGrid<double> grid, double x, double y)
    {
      Point3D vertical = Vertical(grid, x, y);
      return (vertical.X * vertical.X + vertical.Y * vertical.Y);
    }

    public Point3D Vertical(double x, double y)
    {
      return Vertical(this, x, y);
    }

    public static Point3D Vertical(IGrid<double> grid, double x, double y)
    {
      grid.Extent.GetBilinear(x, y, out int ix, out int iy, out double dx, out double dy);

      Point3D p = Vertical(grid, ix, iy, dx, dy);
      return p;
    }


    private static Point3D Vertical(IGrid<double> grid, int ix, int iy, double dx, double dy)
    {
      double h00 = grid[ix, iy];
      double h01 = grid[ix, iy + 1];
      double h10 = grid[ix + 1, iy];
      double h11 = grid[ix + 1, iy + 1];

      double h0 = h00 + (h10 - h00) * dx;
      double h1 = h01 + (h11 - h01) * dx;

      double dhy = h1 - h0;
      double dhx = (h10 + (h11 - h10) * dy) - (h00 + (h01 - h00) * dy);

      double h = h0 + dhy * dy;

      dhy /= grid.Extent.Dy;
      dhx /= grid.Extent.Dx;

      Point3D p = new Point3D(dhx, dhy, h);

      return p;
    }

    public delegate double HillShadingFunction(Point3D vertical);

    public DoubleGrid HillShading(HillShadingFunction shadeFunction)
    {
      return new DoubleOpGrid(this, delegate (int ix, int iy)
      {
        Point3D v = Vertical(this, ix, iy, 0, 0);
        double s = shadeFunction(v);
        return s;
      });
      //int nx = Extent.Nx;
      //int ny = Extent.Ny;

      //DoubleGrid shade = new DoubleGrid(nx - 1, ny - 1, typeof(double),
      //  Extent.X0, Extent.Y0, Extent.Dx);

      //for (int ix = 0; ix < nx - 1; ix++)
      //{
      //  for (int iy = 0; iy < ny - 1; iy++)
      //  {
      //    Point3D v = Vertical(ix, iy, 0, 0);

      //    double s = shadeFunction(v);

      //    shade.SetThis(ix, iy, s);
      //  }
      //}

      //return shade;
    }

    public double Min()
    { return Min(this); }
    public static double Min(IGrid<double> grid)
    {
      double m;
      m = double.MaxValue;
      for (int j = 0; j < grid.Extent.Ny; j++)
      {
        for (int i = 0; i < grid.Extent.Nx; i++)
        {
          double t = grid[i, j];
          if (t < m)
          {
            m = t;
          }
        }
      }
      return m;
    }

    public Polyline Profil(Polyline line)
    { return Profil(this, line); }
    public static Polyline Profil(IDoubleGrid grid, Polyline line)
    {
      Polyline profile = new Polyline();
      IPoint pos;

      double sumDist = 0;
      double h;
      pos = line.Points[0];
      h = grid.Value(pos.X, pos.Y, EGridInterpolation.bilinear);
      profile.Add(new Point2D(0, h));
      foreach (var segment in line.EnumSegments())
      {
        double dist = segment.Length();
        sumDist += dist;

        pos = segment.End;
        h = grid.Value(pos.X, pos.Y, EGridInterpolation.bilinear);

        profile.Add(new Point2D(sumDist, h));
      }
      return profile;
    }

    public double CalcDh(int i0, int j0, int n)
    { return CalcDh(this, i0, j0, n); }
    public static double CalcDh(IDoubleGrid grid, int i0, int j0, int n)
    {
      int i1 = i0 + n;
      int j1 = j0 + n;
      double dhMax = 0;

      // first triangle, must correspond to triangles in drawCell
      double h0 = grid[i0, j0];
      double h1 = grid[i1, j0];
      double h2 = grid[i0, j1];
      double a0 = -(h1 - h0); // eliminated common factor n !!!
      double b0 = -(h2 - h0);
      double c0 = n;
      double d0 = -(a0 * i0 + b0 * j0 + c0 * h0);

      // second triangle, must correspond to triangles in drawCell
      h0 = grid[i1, j1];
      double a1 = (h2 - h0); // eliminated common factor n !!!
      double b1 = (h1 - h0);
      double c1 = n;
      double d1 = -(a1 * i1 + b1 * j1 + c1 * h0);

      for (int i = i0; i <= i1; i++)
      {
        for (int j = j0; j <= j1; j++)
        {
          double h = grid[i, j];
          if (double.IsNaN(h) || h < 0)
          {
            return double.NaN;
          }
          double dh = (i - i0 + j - j0 <= n)
            ? Math.Abs(h + (a0 * i + b0 * j + d0) / c0)
            : Math.Abs(h + (a1 * i + b1 * j + d1) / c1);

          if (dh > dhMax)
          {
            dhMax = dh;
          }
        }
      }

      return dhMax;
    }
  }

  public class DataDoubleGrid : DoubleGrid
  {
    public enum FileType
    {
      Ascii, Binary, Unknown
    }

    private double _z0;
    private double _dz;
    private short _iLength;
    private Type _type;
    private byte[,] _bValue;
    private short[,] _sValue;
    private int[,] _iValue;
    private float[,] _fValue;
    private double[,] _dValue;

    private class GetLine
    {
      private readonly string[] _parts;
      private int _index = 0;

      public GetLine(string[] parts)
      { _parts = parts; }

      public string Next()
      {
        if (_index > _parts.Length)
        { return null; }
        _index++;
        return _parts[_index - 1];
      }
    }

    public static FileType GetFileType(string fileName)
    {
      if (!File.Exists(fileName))
      {
        return FileType.Unknown;
      }
      using (FileStream s = new FileStream(fileName, FileMode.Open))
      {
        s.Seek(0, SeekOrigin.Begin);
        int n = 1024;
        byte[] bytes = new byte[n];
        int max = s.Read(bytes, 0, n);
        string asciiStart = Encoding.ASCII.GetString(bytes, 0, max);
        string[] parts = asciiStart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        GridExtent ext = GridExtent.GetAsciiHeader(new GetLine(parts).Next);
        if (ext != null)
        { return FileType.Ascii; }


        BinarGrid.GetHeader(s, out int nx, out int ny, out BinarGrid.EGridType eType, out short iLength,
          out double x0, out double y0, out double dx, out double z0, out double dz);
        if (nx > 0 && ny > 0 && iLength > 0 &&
          (eType == BinarGrid.EGridType.eDouble || eType == BinarGrid.EGridType.eInt))
        { return FileType.Binary; }
      }
      return FileType.Unknown;
    }

    protected DataDoubleGrid(GridExtent extent)
      : base(extent)
    { }

    private void Init(int nx, int ny, Type type, double z0, double dz)
    {
      if (type == typeof(byte))
      {
        _iLength = 1;
        _bValue = new byte[nx, ny];
      }
      else if (type == typeof(short))
      {
        _iLength = 2;
        _sValue = new short[nx, ny];
      }
      else if (type == typeof(int))
      {
        _iLength = 4;
        _iValue = new int[nx, ny];
      }
      else if (type == typeof(float))
      {
        _iLength = 4;
        _fValue = new float[nx, ny];
        dz = 0.0;
      }
      else if (type == typeof(double))
      {
        _iLength = 8;
        _dValue = new double[nx, ny];
        dz = 0.0;
      }
      else
      {
        throw new Exception("Invalid Type " + type);
      }
      _type = type;
      _z0 = z0;
      _dz = dz;
    }
    public DataDoubleGrid(int nx, int ny, Type type,
      double x0, double y0, double dx, double z0, double dz)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      Init(nx, ny, type, z0, dz);
    }
    public DataDoubleGrid(int nx, int ny, Type type,
      double x0, double y0, double dx, double z0)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      Init(nx, ny, type, z0, 1.0);
    }
    public DataDoubleGrid(int nx, int ny, Type type,
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      Init(nx, ny, type, 0.0, 1.0);
    }

    /// <summary>
    /// Grid from Binary file
    /// </summary>
    public static DataDoubleGrid FromBinaryFile(string name)
    {
      FileStream file = File.Open(name, FileMode.Open);
      DataDoubleGrid grd;

      try
      {
        grd = FromBinaryFile(file);
      }
      finally
      { file.Close(); }

      return grd;
    }

    public static DataDoubleGrid FromBinaryFile(FileStream file)
    {

      BinarGrid.GetHeader(file, out int nx, out int ny, out BinarGrid.EGridType eType, out short iLength,
        out double x0, out double y0, out double dx, out double z0, out double dz);
      if (eType != BinarGrid.EGridType.eDouble)
      {
        throw new Exception("Invalid File Type " + eType);
      }

      // get type
      Type type;
      if (iLength == 1)
      {
        type = typeof(byte);
      }
      else if (iLength == 2)
      {
        type = typeof(short);
      }
      else if (iLength == 4 && dz != 0)
      {
        type = typeof(int);
      }
      else if (iLength == 4 && dz == 0)
      {
        type = typeof(float);
      }
      else if (iLength == 8)
      {
        type = typeof(double);
      }
      else throw new Exception("Unhandled Length " + iLength);

      DataDoubleGrid grd = new DataDoubleGrid(nx, ny, type, x0, y0, dx, z0, dz);

      using (BinaryReader pReader = new BinaryReader(file))
      {
        for (int iy = 0; iy < ny; iy++)
        {
          for (int ix = 0; ix < nx; ix++)
          {
            if (type == typeof(byte)) grd._bValue[ix, iy] = pReader.ReadByte();
            else if (type == typeof(short)) grd._sValue[ix, iy] = pReader.ReadInt16();
            else if (type == typeof(int)) grd._iValue[ix, iy] = pReader.ReadInt32();
            else if (type == typeof(float)) grd._fValue[ix, iy] = pReader.ReadSingle();
            else if (type == typeof(double)) grd._dValue[ix, iy] = pReader.ReadDouble();
            else throw new Exception("Unhandled Type " + type);
          }
        }
      }
      return grd;
    }

    /// <summary>
    /// Grid from ASCII file
    /// </summary>
    public static DataDoubleGrid FromAsciiFile(string name, double z0, double dz, Type type)
    {
      DataDoubleGrid grd;
      using (TextReader file = new StreamReader(name))
      {
        grd = FromAsciiFile(file, z0, dz, type);
      }

      return grd;
    }

    public static DataDoubleGrid FromAsciiFile(TextReader file, double z0, double dz, Type type)
    {
      int iPos = -1;
      int iNList = 0;
      string[] sList = null;
      DataDoubleGrid grd;
      using (new InvariantCulture())
      {
        GridExtent extent = GridExtent.GetAsciiHeader(file);

        if (extent == null)
        { return null; }

        grd = new DataDoubleGrid(extent.Nx, extent.Ny, type,
          extent.X0, extent.Y0, extent.Dx, z0, dz);

        for (int iy = 0; iy < extent.Ny; iy++)
        {
          for (int ix = 0; ix < extent.Nx; ix++)
          {
            if (iPos < 0 || iPos >= iNList)
            {
              sList = file.ReadLine().Trim().Split();
              iNList = sList.GetLength(0);
              iPos = 0;
            }
            Debug.Assert(sList != null, "Error in software design assumption");
            double d = Convert.ToDouble(sList[iPos], System.Globalization.CultureInfo.InvariantCulture);
            if (d == extent.NN)
            { d = double.NaN; }
            grd[ix, iy] = d;
            iPos++;
          }
        }
      }
      return grd;
    }

    protected override void GetStoreProps(out short iLength, out double z0, out double dz)
    {
      iLength = _iLength;
      z0 = _z0;
      dz = _dz;
    }

    protected override void WriteValue(BinaryWriter writer, int ix, int iy)
    {
      if (_type == typeof(byte)) writer.Write(_bValue[ix, iy]);
      else if (_type == typeof(short)) writer.Write(_sValue[ix, iy]);
      else if (_type == typeof(int)) writer.Write(_iValue[ix, iy]);
      else if (_type == typeof(float)) writer.Write(_fValue[ix, iy]);
      else if (_type == typeof(double)) writer.Write(this[ix, iy]);
      else throw new Exception("Unhandled Type " + _type);
    }

    public override double this[int ix, int iy]
    {
      get
      {
        // data
        try
        {
          if (ix < 0 || iy < 0)
            return double.NaN;
          if (ix >= Extent.Nx || iy >= Extent.Ny)
            return double.NaN;

          if (_iLength == 1) return _z0 + _bValue[ix, iy] * _dz;
          else if (_iLength == 2) return _z0 + _sValue[ix, iy] * _dz;
          else if (_iLength == 8) return _dValue[ix, iy];
          else if (_type == typeof(int)) return _z0 + _iValue[ix, iy] * _dz;
          else if (_type == typeof(float)) return _fValue[ix, iy];
          else
          {
            throw new Exception("Unhandled type " + _type);
          }
        }
        catch (IndexOutOfRangeException)
        { return double.NaN; }
      }
      set
      {
        if (_iLength == 1) _bValue[ix, iy] = (byte)((value - _z0) / _dz);
        else if (_iLength == 2) _sValue[ix, iy] = (short)((value - _z0) / _dz);
        else if (_iLength == 8) _dValue[ix, iy] = value;
        else if (_type == typeof(int)) _iValue[ix, iy] = (int)((value - _z0) / _dz);
        else if (_type == typeof(float)) _fValue[ix, iy] = (float)value;
        else
        {
          throw new Exception("Unhandled type " + _type);
        }
      }
    }
  }

  public class TiledDoubleGrid : TiledGrid<double>, IDoubleGrid
  {
    private readonly Type _storeType;
    public TiledDoubleGrid(int nx, int ny, Type storeType,
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      _storeType = storeType;
    }

    protected override IGrid<double> CreateTile(int nx, int ny, double x0, double y0)
    {
      DataDoubleGrid tile = new DataDoubleGrid(nx, nx, _storeType, x0, y0, Extent.Dx);
      return tile;
    }

    public double Value(double x, double y, EGridInterpolation interpolation)
    {
      return DoubleGrid.Value(this, x, y, interpolation);
    }

    public double Slope2(double x, double y)
    {
      return DoubleGrid.Slope2(this, x, y);
    }
  }
}
