using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ArcSde
{
  public class SeLayerInfoList : IDisposable
  {
    private readonly int _nLayers;
    private readonly IntPtr _layers;
    private IntPtr[] _layerArray;

    internal SeLayerInfoList(int nLayers, IntPtr layers)
    {
      _nLayers = nLayers;
      _layers = layers;

      _layerArray = new IntPtr[nLayers];
      Marshal.Copy(layers, _layerArray, 0, nLayers);
    }

    public void Dispose()
    {
      CApi.SE_layer_free_info_list(_nLayers, _layers);
    }

    public IEnumerable<SeLayerInfo> GetLayers()
    {
      for (int i = 0; i < _nLayers; i++)
      {
        SeLayerInfo info = new SeLayerInfo(_layerArray[i]);
        yield return info;
      }
    }
  }
}
