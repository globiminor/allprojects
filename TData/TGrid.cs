using Basics.Geom;

namespace TData
{
  public class TGrid : Data
  {
    Grid.BaseGrid _grd;
    public TGrid(Grid.BaseGrid image)
    {
      _grd = image;
    }
    public static TGrid FromGrid(Grid.BaseGrid grid)
    {
      return new TGrid(grid);
    }
    public override IBox Extent
    {
      get
      { return _grd.Extent.Extent; }
    }
    public Grid.BaseGrid Raster
    {
      get
      { return _grd; }
    }

    public static TGrid FromFile(string filePath)
    {
      Grid.DoubleGrid.FileType type = Grid.DoubleGrid.GetFileType(filePath);
      if (type == Grid.DoubleGrid.FileType.Ascii)
      {
        Grid.DoubleGrid dgrd = Grid.DoubleGrid.FromAsciiFile(filePath, 0, 0.001, typeof(double));
        TGrid data = FromData(dgrd);
        return data;
      }
      else if (type == Grid.DoubleGrid.FileType.Binary)
      {
        Grid.DoubleGrid dgrd = Grid.DoubleGrid.FromBinaryFile(filePath);
        TGrid data = FromData(dgrd);
        return data;        
      }
      Grid.ImageGrid img = Grid.ImageGrid.FromFile(filePath);
      if (img != null)
      {
        TGrid data = FromGrid(img);
        data.Path = filePath;
        return data;
      }
      return null;
    }

    public static TGrid FromData(Grid.BaseGrid grid)
    {
      return FromGrid(grid);
    }

    public Grid.BaseGrid BaseData
    {
      get { return _grd; }
      set { _grd = value; }
    }
  }

  /// <summary>
  /// Summary description for Row.
  /// </summary>
  public class RowDelete
  {
    IGeometry _geometry;

    public IGeometry Geometry
    {
      get
      { return _geometry; }
      set
      { _geometry = value; }
    }
  }
}
