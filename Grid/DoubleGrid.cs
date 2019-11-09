using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Grid
{  public abstract class DoubleGrid : BaseGrid<double>
  {
    protected DoubleGrid(GridExtent extent)
      : base(extent)
    { }

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
  }

  public class DataDoubleGrid : DoubleGrid, IGridW<double>
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

    public sealed override double GetCell(int ix, int iy) => this[ix, iy];
    public new double this[int ix, int iy]
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

  public class TiledDoubleGrid : TiledGridW<double>
  {
    private readonly Type _storeType;
    public TiledDoubleGrid(int nx, int ny, Type storeType,
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      _storeType = storeType;
    }

    protected override IGridW<double> CreateTile(int nx, int ny, double x0, double y0)
    {
      DataDoubleGrid tile = new DataDoubleGrid(nx, nx, _storeType, x0, y0, Extent.Dx);
      return tile;
    }
  }
}
