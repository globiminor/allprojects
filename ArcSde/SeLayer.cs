using System;

namespace ArcSde
{
  public class SeLayer : SeTable, IDisposable
  {
    private SeLayerInfo _layerInfo;

    public SeLayerInfo LayerInfo
    {
      get
      {
        if (_layerInfo == null)
        {
          _layerInfo = new SeLayerInfo(Connection.Conn, Name, GeomCol.Name);
        }
        return _layerInfo;
      }
    }

    public void Dispose()
    {
      if (_layerInfo != null)
      { _layerInfo.Dispose(); }
    }

    public SeLayer(SeConnection conn, string name)
      : base(conn, name)
    {
      _layerInfo = null;
    }
  }
}