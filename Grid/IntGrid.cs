using System;
using System.IO;

namespace Grid
{
  /// <summary>
  /// Integer Grid
  /// </summary>
  public class IntGrid : BaseGrid<int>
  {
    private Type _type;
    private short _iLength;
    private Array _value;

    private IntGrid _iGrid;
    private int _operator;
    private EOperator _eOperator;
    private DoubleBaseGrid _dGrid;


    #region operators

    internal IntGrid(DoubleBaseGrid grd)
      : base(grd.Extent)
    {
      _dGrid = grd;
      _eOperator = EOperator.none;
    }

    private IntGrid(IntGrid grd, int val, EOperator op)
      : base(grd.Extent)
    {
      _iGrid = grd;
      _operator = val;
      _eOperator = op;
    }

    public static IntGrid operator -(IntGrid grd, int val)
    {
      return new IntGrid(grd, -val, EOperator.addVal);
    }

    public static IntGrid operator %(IntGrid grd, int mod)
    {
      return new IntGrid(grd, mod, EOperator.gridModVal);
    }

    public IntGrid Abs()
    {
      return new IntGrid(this, 0, EOperator.abs);
    }

    #endregion

    #region constructors

    public IntGrid(int nx, int ny, Type type,
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      if (type == typeof(sbyte) || type == typeof(byte))
      { _iLength = 1; }
      else if (type == typeof(short))
      { _iLength = 2; }
      else if (type == typeof(int))
      { _iLength = 4; }
      else
      { throw new ArgumentException("Invalid Type " + type); }

      _value = Array.CreateInstance(type, new int[] { nx, ny });
      _type = type;
    }

    public static IntGrid FromBinaryFile(string name)
    {
      short iLength;
      Type type;
      int nx, ny;
      double x0, y0, dx, z0, dz;
      BinarGrid.EGridType eType;
      BinaryReader pReader;
      FileStream pFile = File.Open(name, FileMode.Open);

      BinarGrid.GetHeader(pFile, out nx, out ny, out eType, out iLength, out x0, out y0, out dx,
        out z0, out dz);
      if (eType != BinarGrid.EGridType.eInt)
      {
        throw new Exception("Invalid File Type " + eType);
      }

      // get type
      if (iLength == 1)
      { type = typeof(byte); }
      else if (iLength == 2)
      { type = typeof(short); }
      else if (iLength == 4)
      { type = typeof(int); }
      else
      { throw new ArgumentException("Unhandled Length " + iLength); }

      IntGrid grd = new IntGrid(nx, ny, type, x0, y0, dx);
      grd._value = Array.CreateInstance(type, new int[] { nx, ny });

      pReader = new BinaryReader(pFile);
      for (int iy = 0; iy < ny; iy++)
      {
        for (int ix = 0; ix < nx; ix++)
        {
          if (type == typeof(byte))
          { grd._value.SetValue(pReader.ReadByte(), ix, iy); }
          else if (type == typeof(short))
          { grd._value.SetValue(pReader.ReadInt16(), ix, iy); }
          else if (type == typeof(int))
          { grd._value.SetValue(pReader.ReadInt32(), ix, iy); }
          else
          { throw new ArgumentException("Unhandled Type " + type); }
        }
      }

      pFile.Close();

      return grd;
    }
    #endregion

    public void Save(string name)
    {
      BinaryWriter pWriter = new BinaryWriter(new FileStream(name, FileMode.Create));
      BinarGrid.PutHeader(pWriter.BaseStream, Extent.Nx, Extent.Ny,
        BinarGrid.EGridType.eInt, _iLength, Extent.X0, Extent.Y0, Extent.Dx,
        0, 0);
      pWriter.Seek(BinarGrid.START_DATA, SeekOrigin.Begin);

      int nx = Extent.Nx;
      int ny = Extent.Ny;
      for (int iy = 0; iy < ny; iy++)
      {
        for (int ix = 0; ix < nx; ix++)
        {
          if (_type == typeof(sbyte))
          { pWriter.Write(Convert.ToSByte(_value.GetValue(ix, iy))); }
          else if (_type == typeof(byte))
          { pWriter.Write(Convert.ToByte(_value.GetValue(ix, iy))); }
          else if (_type == typeof(short))
          { pWriter.Write(Convert.ToInt16(_value.GetValue(ix, iy))); }
          else if (_type == typeof(int))
          { pWriter.Write(Convert.ToInt32(_value.GetValue(ix, iy))); }
          else throw new Exception("Unhandled Type " + _type);
        }
      }
      pWriter.Close();
    }

    public override int this[int ix, int iy]
    {
      get
      {
        int value;
        // operators
        if (_eOperator == EOperator.gridModVal) value = _iGrid[ix, iy] % _operator;
        else if (_eOperator == EOperator.addVal) value = _iGrid[ix, iy] + _operator;
        else if (_eOperator == EOperator.abs) value = Math.Abs(_iGrid[ix, iy]);
        // converters
        else if (_eOperator == EOperator.none && _dGrid != null) value = (int)_dGrid[ix, iy];
        // data
        else
        { value = Convert.ToInt32(_value.GetValue(ix, iy)); }
        return value;
      }
      set
      {
        _value.SetValue(Convert.ChangeType(value, _type), ix, iy);
      }
    }
  }
}
