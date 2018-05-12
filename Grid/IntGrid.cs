using System;
using System.IO;
using Grid.Lcp;

namespace Grid
{
  /// <summary>
  /// Integer Grid
  /// </summary>
  public class IntGrid : BaseGrid<int>, IGridStoreType
  {
    private Type _type;
    private Array _value;

    private IGrid<int> _iGrid;
    private int _operator;
    private EOperator _eOperator;
    private IGrid<double> _dGrid;


    #region operators

    internal IntGrid(IGrid<double> grd)
      : base(grd.Extent)
    {
      _dGrid = grd;
      _eOperator = EOperator.none;
    }

    private IntGrid(IGrid<int> grd, int val, EOperator op)
      : base(grd.Extent)
    {
      _iGrid = grd;
      _operator = val;
      _eOperator = op;
    }
    Type IGridStoreType.StoreType { get { return _type; } }

    public static IntGrid operator -(IntGrid grd, int val)
    { return Add(grd, -val); }
    public static IntGrid Add(IGrid<int> grd, int val)
    {
      return new IntGrid(grd, val, EOperator.addVal);
    }

    public static IntGrid operator %(IntGrid grd, int mod)
    { return Mod(grd, mod); }
    public static IntGrid Mod(IGrid<int> grd, int mod)
    {
        return new IntGrid(grd, mod, EOperator.gridModVal);
    }

    public IntGrid Abs()
    { return Abs(this); }
    public static IntGrid Abs(IGrid<int> grid)
    {
      return new IntGrid(grid, 0, EOperator.abs);
    }

    #endregion

    #region constructors

    public IntGrid(int nx, int ny, Type type,
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {

      _value = Array.CreateInstance(type, new int[] { nx, ny });
      _type = type;
    }

    private static short GetStoreLength(Type type)
    {
      short storeLength;
      if (type == typeof(sbyte) || type == typeof(byte))
      { storeLength = 1; }
      else if (type == typeof(short))
      { storeLength = 2; }
      else if (type == typeof(int))
      { storeLength = 4; }
      else
      { throw new ArgumentException("Invalid Type " + type); }

      return storeLength;
    }

    public static IntGrid FromBinaryFile(string name)
    {
      Type type;
      BinaryReader pReader;
      using (FileStream pFile = File.Open(name, FileMode.Open))
      {
        BinarGrid.GetHeader(pFile,
          out int nx, out int ny, out BinarGrid.EGridType eType,
          out short iLength, out double x0, out double y0, out double dx,
          out double z0, out double dz);
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

        IntGrid grd = new IntGrid(nx, ny, type, x0, y0, dx)
        { _value = Array.CreateInstance(type, new int[] { nx, ny }) };

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
    }
    #endregion

    public void Save(string name)
    {
      Save(this, name, _type);
    }

    public static void Save(IGrid<int> grid, string name, Type type = null)
    {
      type = type ?? (grid as IGridStoreType)?.StoreType ?? typeof(int);
      short storeLength = GetStoreLength(type);
      BinaryWriter pWriter = new BinaryWriter(new FileStream(name, FileMode.Create));
      BinarGrid.PutHeader(pWriter.BaseStream, grid.Extent.Nx, grid.Extent.Ny,
        BinarGrid.EGridType.eInt, storeLength, grid.Extent.X0, grid.Extent.Y0, grid.Extent.Dx,
        0, 0);
      pWriter.Seek(BinarGrid.START_DATA, SeekOrigin.Begin);

      int nx = grid.Extent.Nx;
      int ny = grid.Extent.Ny;
      for (int iy = 0; iy < ny; iy++)
      {
        for (int ix = 0; ix < nx; ix++)
        {
          if (type == typeof(sbyte))
          { pWriter.Write(Convert.ToSByte(grid[ix, iy])); }
          else if (type == typeof(byte))
          { pWriter.Write(Convert.ToByte(grid[ix, iy])); }
          else if (type == typeof(short))
          { pWriter.Write(Convert.ToInt16(grid[ix, iy])); }
          else if (type == typeof(int))
          { pWriter.Write(Convert.ToInt32(grid[ix, iy])); }
          else throw new Exception("Unhandled Type " + type);
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

  public class TiledIntGrid : TiledGrid<int>, IGridStoreType
  {
    private readonly Type _storeType;

    public TiledIntGrid(int nx, int ny, Type storeType, 
      double x0, double y0, double dx)
      : base(new GridExtent(nx, ny, x0, y0, dx))
    {
      _storeType = storeType;
    }
    Type IGridStoreType.StoreType => _storeType;

    protected override IGrid<int> CreateTile(int nx, int ny, double x0, double y0)
    {
      IntGrid tile = new IntGrid(nx, ny, _storeType, x0, y0, Extent.Dx);
      return tile;
    }
  }
}
