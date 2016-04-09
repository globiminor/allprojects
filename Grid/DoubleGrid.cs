using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Basics.Geom;

namespace Grid
{
  public interface IDoubleGrid
  {
    double Value(double x, double y, EGridInterpolation interpol);
    double Slope2(double x, double y);
  }
  /// <summary>
  /// Double Grid
  /// </summary>
  public class DoubleGrid : BaseGrid<double>, IDoubleGrid
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

    EOperator _eOperator;
    private DoubleGrid _dGrd0;
    private DoubleGrid _dGrd1;
    private double _dOperator;
    private IntGrid _iGrd;
    private double[] _dArray;

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
    protected DoubleGrid(GridExtent extent)
      : base(extent)
    {
      _eOperator = EOperator.none;
    }
    private DoubleGrid(DoubleGrid grd, double val, EOperator op)
      : base(grd.Extent)
    {
      _dGrd0 = grd;
      _dOperator = val;
      _eOperator = op;
    }
    private DoubleGrid(DoubleGrid grd0, DoubleGrid grd1, EOperator op)
      : base(grd0.Extent)
    {
      _dGrd0 = grd0;
      _dGrd1 = grd1;
      _eOperator = op;
    }
    public DoubleGrid(double[] dArray, IntGrid grd)
      : base(grd.Extent)
    {
      _dArray = dArray;
      _iGrd = grd;
      _eOperator = EOperator.bracket;
    }
    public static DoubleGrid operator +(DoubleGrid grd, double val)
    {
      return new DoubleGrid(grd, val, EOperator.addVal);
    }
    public static DoubleGrid operator +(DoubleGrid grd0, DoubleGrid grd1)
    {
      return new DoubleGrid(grd0, grd1, EOperator.addGrd);
    }
    public static DoubleGrid operator -(DoubleGrid grd, double val)
    {
      return new DoubleGrid(grd, -val, EOperator.addVal);
    }
    public static DoubleGrid operator *(DoubleGrid grd, double val)
    {
      return new DoubleGrid(grd, val, EOperator.multVal);
    }
    public static DoubleGrid operator /(DoubleGrid grd, double val)
    {
      return new DoubleGrid(grd, 1.0 / val, EOperator.multVal);
    }

    public IntGrid ToIntGrid()
    {
      return new IntGrid(this);
    }

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
    public DoubleGrid(int nx, int ny, Type type,
      double x0, double y0, double dx, double z0, double dz)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      Init(nx, ny, type, z0, dz);
    }
    public DoubleGrid(int nx, int ny, Type type,
      double x0, double y0, double dx, double z0)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      Init(nx, ny, type, z0, 1.0);
    }
    public DoubleGrid(int nx, int ny, Type type,
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      Init(nx, ny, type, 0.0, 1.0);
    }

    /// <summary>
    /// Grid from Binary file
    /// </summary>
    public static DoubleGrid FromBinaryFile(string name)
    {
      FileStream file = File.Open(name, FileMode.Open);
      DoubleGrid grd;

      try
      {
        grd = FromBinaryFile(file);
      }
      finally
      { file.Close(); }

      return grd;
    }

    public static DoubleGrid FromBinaryFile(FileStream file)
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

      DoubleGrid grd = new DoubleGrid(nx, ny, type, x0, y0, dx, z0, dz);

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
    public static DoubleGrid FromAsciiFile(string name, double z0, double dz, Type type)
    {
      DoubleGrid grd;
      using (TextReader file = new StreamReader(name))
      {
        grd = FromAsciiFile(file, z0, dz, type);
      }

      return grd;
    }

    public static DoubleGrid FromAsciiFile(TextReader file, double z0, double dz, Type type)
    {
      int iPos = -1;
      int iNList = 0;
      string[] sList = null;
      DoubleGrid grd;
      using (new InvariantCulture())
      {
        GridExtent extent = GridExtent.GetAsciiHeader(file);

        if (extent == null)
        { return null; }

        grd = new DoubleGrid(extent.Nx, extent.Ny, type,
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

    public void SaveASCII(string name, string format)
    {
      using (TextWriter w = new StreamWriter(name, false))
      {
        w.WriteLine("NCOLS {0}", Extent.Nx);
        w.WriteLine("NROWS {0}", Extent.Ny);
        w.WriteLine("XLLCORNER {0}", Extent.X0);
        w.WriteLine("YLLCORNER {0}", Extent.Y0 + (Extent.Ny - 1) * Extent.Dy);
        w.WriteLine("CELLSIZE {0}", Extent.Dx);
        w.WriteLine("NODATA_VALUE {0}", Extent.NN);
        for (int iy = 0; iy < Extent.Ny; iy++)
        {
          for (int ix = 0; ix < Extent.Nx; ix++)
          {
            double val = this[ix, iy];
            if (double.IsNaN(val)) val = Extent.NN;
            w.Write("{0} ", val.ToString(format));
          }
          w.WriteLine();
        }
      }
    }

    public void Save(string name)
    {
      short iLength;
      Type pType;
      // check for operator grids
      if (_type == null)
      {
        pType = typeof(double);
        iLength = 8;
      }
      else
      {
        pType = _type;
        iLength = _iLength;
      }

      BinaryWriter writer = new BinaryWriter(new FileStream(name, FileMode.Create));
      BinarGrid.PutHeader(writer.BaseStream, Extent.Nx, Extent.Ny,
        BinarGrid.EGridType.eDouble, iLength, Extent.X0, Extent.Y0, Extent.Dx,
        _z0, _dz);
      writer.Seek(BinarGrid.START_DATA, SeekOrigin.Begin);

      int nx = Extent.Nx;
      int ny = Extent.Ny;
      for (int iy = 0; iy < ny; iy++)
      {
        for (int ix = 0; ix < nx; ix++)
        {
          if (pType == typeof(byte)) writer.Write(_bValue[ix, iy]);
          else if (pType == typeof(short)) writer.Write(_sValue[ix, iy]);
          else if (pType == typeof(int)) writer.Write(_iValue[ix, iy]);
          else if (pType == typeof(float)) writer.Write(_fValue[ix, iy]);
          else if (pType == typeof(double)) writer.Write(this[ix, iy]); // normal and operator
          else throw new Exception("Unhandled Type " + pType);
        }
      }
      writer.Close();
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

    public double Slope2(double x, double y)
    {
      Point3D vertical = Vertical(x, y);

      return (vertical.X * vertical.X + vertical.Y * vertical.Y);
    }

    public delegate double HillShadingFunction(Point3D vertical);

    public DoubleGrid HillShading(HillShadingFunction shadeFunction)
    {
      int nx = Extent.Nx;
      int ny = Extent.Ny;

      DoubleGrid shade = new DoubleGrid(nx - 1, ny - 1, typeof(double),
        Extent.X0, Extent.Y0, Extent.Dx);

      for (int ix = 0; ix < nx - 1; ix++)
      {
        for (int iy = 0; iy < ny - 1; iy++)
        {
          Point3D v = Vertical(ix, iy, 0, 0);

          double s = shadeFunction(v);

          shade.SetThis(ix, iy, s);
        }
      }

      return shade;
    }

    public double Min()
    {
      double m;
      m = this[0, 0];
      for (int j = 0; j < Extent.Ny; j++)
      {
        for (int i = 0; i < Extent.Nx; i++)
        {
          double t = this[i, j];
          if (t < m)
          {
            m = t;
          }
        }
      }
      return m;
    }

    public Polyline Profil(Polyline line)
    {
      Polyline profile = new Polyline();
      IPoint pos;

      double sumDist = 0;
      double h;
      pos = line.Points.First.Value;
      h = Value(pos.X, pos.Y, EGridInterpolation.bilinear);
      profile.Add(new Point2D(0, h));
      foreach (Curve segment in line.Segments)
      {
        double dist = segment.Length();
        sumDist += dist;

        pos = segment.End;
        h = Value(pos.X, pos.Y, EGridInterpolation.bilinear);

        profile.Add(new Point2D(sumDist, h));
      }
      return profile;
    }


    public double CalcDh(int i0, int j0, int n)
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
      h0 = this[i0, j0];
      h1 = this[i1, j0];
      h2 = this[i0, j1];
      a0 = -(h1 - h0); // eliminated common factor n !!!
      b0 = -(h2 - h0);
      c0 = n;
      d0 = -(a0 * i0 + b0 * j0 + c0 * h0);

      // second triangle, must correspond to triangles in drawCell
      h0 = this[i1, j1];
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
          h = this[i, j];
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
}
