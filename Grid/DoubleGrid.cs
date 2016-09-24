using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Basics.Geom;
using System.Collections.Generic;

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
      private IDoubleGrid _dGrd0;
      private IDoubleGrid _dGrd1;
      private double _dOperator;
      private IntGrid _iGrd;

      private double[] _dArray;
      private Func<int, int, double> _opFunc;

      public DoubleOpGrid(IDoubleGrid grid, Func<int, int, double> op)
        : base(grid.Extent)
      {
        _dGrd0 = grid;
        _eOperator = EOperator.none;
        _opFunc = op;
      }
      public DoubleOpGrid(IDoubleGrid grd, double val, EOperator op)
      : base(grd.Extent)
      {
        _dGrd0 = grd;
        _dOperator = val;
        _eOperator = op;
      }

      public DoubleOpGrid(IDoubleGrid grd0, IDoubleGrid grd1, EOperator op)
      : base(grd0.Extent)
      {
        _dGrd0 = grd0;
        _dGrd1 = grd1;
        _eOperator = op;
      }

      public DoubleOpGrid(double[] dArray, IntGrid grd)
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
          else if (_eOperator == EOperator.addGrd) return _dGrd0[ix, iy] + _dGrd1[ix, iy];
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

    public static DoubleGrid Create(double[] dArray, IntGrid grd)
    {
      return new DoubleOpGrid(dArray, grd);
    }

    public static DoubleGrid operator +(DoubleGrid grd, double val)
    { return Sum(grd, val); }
    public static DoubleGrid Sum(IDoubleGrid grd, double val)
    {
      return new DoubleOpGrid(grd, val, EOperator.addVal);
    }
    public static DoubleGrid operator +(DoubleGrid grd0, IDoubleGrid grd1)
    { return Sum(grd0, grd1); }
    public static DoubleGrid Sum(IDoubleGrid grd0, IDoubleGrid grd1)
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
    public static IntGrid ToIntGrid(IDoubleGrid grid)
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
    public static void Save(IDoubleGrid grid, string name)
    {
      DoubleGrid g = grid as DoubleGrid;
      if (g != null) { g.Save(name); }
      else
      {
        Save(grid, name, delegate (BinaryWriter writer, int ix, int iy)
        { writer.Write(grid[ix, iy]); });
      }
    }
    private static void Save(IDoubleGrid grid, string name, Action<BinaryWriter, int, int> writeAction)
    {
      short iLength;
      double z0;
      double dz;
      GetStoreProps(grid, out iLength, out z0, out dz);

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

    private static void GetStoreProps(IDoubleGrid grid, out short iLength, out double z0, out double dz)
    {
      DoubleGrid g = grid as DoubleGrid;
      if (g != null)
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
      int ix, iy;
      double dx, dy;

      Extent.GetBilinear(x0, y0, out ix, out iy, out dx, out dy);

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
      if (eInter == EGridInterpolation.nearest)
      {
        return Value(x, y);
      }
      else if (eInter == EGridInterpolation.bilinear)
      {
        int ix, iy;
        double dx, dy;
        double h00, h01, h10, h11;
        double h0, h1;

        Extent.GetBilinear(x, y, out ix, out iy, out dx, out dy);

        h00 = this[ix, iy];
        if (dx == 0)
        {
          if (dy == 0)
          {
            return h00;
          }
          h01 = this[ix, iy + 1];
          return h00 + (h01 - h00) * dy;
        }
        if (dy == 0)
        {
          h10 = this[ix + 1, iy];
          return h00 + (h10 - h00) * dx;
        }

        h01 = this[ix, iy + 1];
        h10 = this[ix + 1, iy];
        h11 = this[ix + 1, iy + 1];

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
      Point3D vertical = Vertical(x, y);

      return (vertical.X * vertical.X + vertical.Y * vertical.Y);
    }

    public Point3D Vertical(double x, double y)
    {
      int ix, iy;
      double dx, dy;

      Extent.GetBilinear(x, y, out ix, out iy, out dx, out dy);

      Point3D p = Vertical(ix, iy, dx, dy);
      return p;
    }

    private Point3D Vertical(int ix, int iy, double dx, double dy)
    {
      double h00 = this[ix, iy];
      double h01 = this[ix, iy + 1];
      double h10 = this[ix + 1, iy];
      double h11 = this[ix + 1, iy + 1];

      double h0 = h00 + (h10 - h00) * dx;
      double h1 = h01 + (h11 - h01) * dx;

      double dhy = h1 - h0;
      double dhx = (h10 + (h11 - h10) * dy) - (h00 + (h01 - h00) * dy);

      double h = h0 + dhy * dy;

      dhy = dhy / Extent.Dy;
      dhx = dhx / Extent.Dx;

      Point3D p = new Point3D(dhx, dhy, h);

      return p;
    }

    public delegate double HillShadingFunction(Point3D vertical);

    public DoubleGrid HillShading(HillShadingFunction shadeFunction)
    {
      return new DoubleOpGrid(this, delegate (int ix, int iy)
      {
        Point3D v = Vertical(ix, iy, 0, 0);
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
    public static double Min(IDoubleGrid grid)
    {
      double m;
      m = grid[0, 0];
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
      pos = line.Points.First.Value;
      h = grid.Value(pos.X, pos.Y, EGridInterpolation.bilinear);
      profile.Add(new Point2D(0, h));
      foreach (Curve segment in line.Segments)
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
      double h0, h1, h2;
      double a0, b0, c0, d0;
      double a1, b1, c1, d1;
      double dhMax;
      int i1, j1;
      i1 = i0 + n;
      j1 = j0 + n;
      dhMax = 0;

      // first triangle, must correspond to triangles in drawCell
      h0 = grid[i0, j0];
      h1 = grid[i1, j0];
      h2 = grid[i0, j1];
      a0 = -(h1 - h0); // eliminated common factor n !!!
      b0 = -(h2 - h0);
      c0 = n;
      d0 = -(a0 * i0 + b0 * j0 + c0 * h0);

      // second triangle, must correspond to triangles in drawCell
      h0 = grid[i1, j1];
      a1 = (h2 - h0); // eliminated common factor n !!!
      b1 = (h1 - h0);
      c1 = n;
      d1 = -(a1 * i1 + b1 * j1 + c1 * h0);

      for (int i = i0; i <= i1; i++)
      {
        for (int j = j0; j <= j1; j++)
        {
          double dhTemp;
          double h;
          h = grid[i, j];
          if (double.IsNaN(h) || h < 0)
          {
            return double.NaN;
          }
          if (i - i0 + j - j0 <= n)
          {
            dhTemp = Math.Abs(h + (a0 * i + b0 * j + d0) / c0);
          }
          else
          {
            dhTemp = Math.Abs(h + (a1 * i + b1 * j + d1) / c1);
          }
          if (dhTemp > dhMax)
          {
            dhMax = dhTemp;
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

        short iLength;
        int nx, ny;
        double x0, y0, dx;
        double z0, dz;
        BinarGrid.EGridType eType;

        BinarGrid.GetHeader(s, out nx, out ny, out eType, out iLength, out x0, out y0, out dx,
          out z0, out dz);
        if (nx > 0 && ny > 0 && iLength > 0)
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
      short iLength;
      int nx, ny;
      double x0, y0, dx;
      double z0, dz;
      BinarGrid.EGridType eType;
      BinaryReader pReader;
      Type type;

      BinarGrid.GetHeader(file, out nx, out ny, out eType, out iLength, out x0, out y0, out dx,
        out z0, out dz);
      if (eType != BinarGrid.EGridType.eDouble)
      {
        throw new Exception("Invalid File Type " + eType);
      }

      // get type
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

      pReader = new BinaryReader(file);
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
          if (ix < 0 || iy < 0) return double.NaN;
          if (ix >= Extent.Nx || iy >= Extent.Ny) return double.NaN;

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
}
