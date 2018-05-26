namespace TMap
{
  public abstract class MapData
  {
    private bool _isVisible = true;
    private string _name = null;

    public MapData()
    { }
    public MapData(string name)
    {
      _name = name;
    }
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }
    public bool Visible
    {
      get { return _isVisible; }
      set { _isVisible = value; }
    }
    public abstract Basics.Geom.IBox Extent { get; }
    public abstract void Draw(IDrawable drawable);

    public System.Collections.Generic.IEnumerable<MapData> GetAllData()
    {
      yield return this;

      if (this is GroupMapData grpData)
      {
        foreach (MapData subpart in grpData.Subparts)
        {
          foreach (MapData data in subpart.GetAllData())
          {
            yield return data;
          }
        }
      }
    }
  }
}

