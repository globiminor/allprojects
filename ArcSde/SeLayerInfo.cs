using System;

namespace ArcSde
{
  public class SeLayerInfo : IDisposable
  {
    private IntPtr _ptr;
    private SeCoordRef _coord;
    private readonly bool _noDispose;

    private string _table;
    private string _spatialCol;

    internal SeLayerInfo(IntPtr ptr)
    {
      _ptr = ptr;
      _noDispose = true;
    }

    internal SeLayerInfo(IntPtr connPtr, string table, string geomCol)
    {
      _coord = new SeCoordRef();
      Check(CApi.SE_layerinfo_create(_coord.Ptr, out _ptr));
      Check(CApi.SE_layer_get_info(connPtr, table, geomCol, _ptr));
    }

    public string Table
    {
      get
      {
        if (_table == null)
        { InitTable(); }
        return _table;
      }
    }

    public string SpatialColumn
    {
      get
      {
        if (_spatialCol == null)
        { InitTable(); }
        return _spatialCol;
      }
    }

    public int SpatialTypes
    {
      get
      {
        int types;
        Check(CApi.SE_layerinfo_get_shape_types(_ptr, out types));
        return types;
      }
    }

    private void InitTable()
    {
      char[] cTbl = new char[SdeType.SE_QUALIFIED_TABLE_NAME];
      char[] cCol = new char[SdeType.SE_MAX_COLUMN_LEN];
      Check(CApi.SE_layerinfo_get_spatial_column(_ptr, cTbl, cCol));
      _table = new string(cTbl).Trim('\0');
      _spatialCol = new string(cCol).Trim('\0');
    }
    public void Dispose()
    {
      if (!_noDispose && _ptr != IntPtr.Zero)
      {
        CApi.SE_layerinfo_free(_ptr);
        _ptr = IntPtr.Zero;
      }
      if (_coord != null)
      {
        _coord.Dispose();
        _coord = null;
      }
    }

    private void Check(int rc)
    {
      ErrorHandling.checkRC(IntPtr.Zero, IntPtr.Zero, rc);
    }

    private SeCoordRef QCoord
    {
      get
      {
        if (_coord == null)
        { _coord = new SeCoordRef();}
        return _coord;
      }
    }
    public SeCoordRef CoordRef
    {
      get
      {
        Check(CApi.SE_layerinfo_get_coordref(_ptr, QCoord.Ptr));
        return QCoord;
      }
    }

    public int GetShapeTypes()
    {
      int shapeTypes;
      Check(CApi.SE_layerinfo_get_shape_types(_ptr, out shapeTypes));
      return shapeTypes;
    }

    public Se_Envelope GetExtent()
    {
      Se_Envelope extent = new Se_Envelope();
      Check(CApi.SE_layerinfo_get_envelope(_ptr, ref extent));
      return extent;
    }
  }
}