
using Basics.Views;

namespace TMap
{
  public abstract class MapData : BaseVm
  {
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

    private bool _isVisible = true;
    public bool Visible
    {
      get { return _isVisible; }
      set
      {
        _isVisible = value;
        Changed();
      }
    }
    private double _transparency;
    public double Transparency
    {
      get { return _transparency; }
      set
      {
        if (value < 0 || value > 1)
        { return; }
        _transparency = value;
        Changed();
      }
    }

    public abstract Basics.Geom.IBox Extent { get; }
    public abstract void Draw(IDrawable drawable);

    public System.Collections.Generic.IEnumerable<MapData> GetAllData()
    {
      yield return this;

      if (this is GroupMapData grpData)
      {
        foreach (var subpart in grpData.Subparts)
        {
          foreach (var data in subpart.GetAllData())
          {
            yield return data;
          }
        }
      }
    }
  }
}

