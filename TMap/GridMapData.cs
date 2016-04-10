using System;
using TData;
using Grid;
using System.Drawing;

namespace TMap
{
  public class GridMapData : GraphicMapData
  {
    TGrid _data;
    GridSymbolisation _symbol;

    private GridMapData(string name, TGrid data)
      : base(name, data)
    {
      _data = data;
      _symbol = new LookupSymbolisation();
    }

    public GridSymbolisation Symbolisation
    {
      get { return _symbol; }
      set { _symbol = value; }
    }

    public static GridMapData FromData(BaseGrid grid)
    {
      GridMapData mapData = new GridMapData("grid", TGrid.FromData(grid));

      if (grid.Type != typeof(Color))
      {
        mapData._symbol = new LookupSymbolisation();
      }
      return mapData;
    }

    public static new GridMapData FromFile(string dataPath)
    {
      TGrid grid = TGrid.FromFile(dataPath);
      if (grid == null)
      { return null; }

      GridMapData mapData = new GridMapData(
        System.IO.Path.GetFileNameWithoutExtension(dataPath), grid);

      if (grid.Raster.Type != typeof(Color))
      {
        mapData._symbol = new LookupSymbolisation();
      }
      return mapData;
    }

    public override void Draw(IDrawable drawable)
    {
      if (_data.Raster is ILockable)
      { ((ILockable)_data.Raster).LockBits(); }

      try
      { drawable.DrawRaster(this); }
      finally
      {
        if (_data.Raster is ILockable)
        { ((ILockable)_data.Raster).UnlockBits(); }
      }
    }

    public Color? Color(double x, double y)
    {
      int ix, iy;
      BaseGrid grd = _data.Raster;
      if (grd.Extent.GetNearest(x, y, out ix, out iy) == false)
      { return null; }

      if (grd.Type == typeof(Color))
      {
        return (Color)grd[ix, iy];
      }
      return _symbol.Color(Convert.ToDouble(grd[ix,iy]));
    }

    public TGrid Data
    {
      get { return _data; }
    }
  }
}
