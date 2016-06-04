using Basics.Geom;

namespace TData
{
  public class TGrid : Data
  {
    Grid.IGrid _grd;
    public TGrid(Grid.IGrid image)
    {
      _grd = image;
    }
    public static TGrid FromGrid(Grid.IGrid grid)
    {
      return new TGrid(grid);
    }
    public override IBox Extent
    {
      get
      { return _grd.Extent.Extent; }
    }
    public Grid.IGrid Raster
    {
      get
      { return _grd; }
    }

    public static TGrid FromFile(string filePath)
    {
      Grid.DataDoubleGrid.FileType type = Grid.DataDoubleGrid.GetFileType(filePath);
      if (type == Grid.DataDoubleGrid.FileType.Ascii)
      {
        Grid.DataDoubleGrid dgrd = Grid.DataDoubleGrid.FromAsciiFile(filePath, 0, 0.001, typeof(double));
        TGrid data = FromData(dgrd);
        return data;
      }
      else if (type == Grid.DataDoubleGrid.FileType.Binary)
      {
        Grid.DataDoubleGrid dgrd = Grid.DataDoubleGrid.FromBinaryFile(filePath);
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

    public static TGrid FromData(Grid.IGrid grid)
    {
      return FromGrid(grid);
    }

    public Grid.IGrid BaseData
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
