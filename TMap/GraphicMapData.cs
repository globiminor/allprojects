namespace TMap
{
  public abstract class GraphicMapData : MapData
  {
    TData.Data _data;

    public GraphicMapData(string name, TData.Data data)
      : base(name)
    { _data = data; }

    public override Basics.Geom.IBox Extent
    {
      get { return _data.Extent; }
    }
    public static GraphicMapData FromFile(string dataPath)
    {
      GraphicMapData data;

      data = TableMapData.FromFile(dataPath);
      if (data != null)
      { return data; }

      data = GridMapData.FromFile(dataPath);
      if (data != null)
      { return data; }

      return null;
    }
  }
}