using Basics.Geom;
using System;
using System.Collections.Generic;

namespace Ocad
{
  public class OcadElemsInfo
  {
    private readonly string _fullName;
    private DateTime _changeDate = DateTime.MinValue;

    private IList<ElementIndex> _indexList;
    private IBox _extent;
    private Setup _setup;

    public OcadElemsInfo(string fullName)
    {
      _fullName = fullName;
    }

    private bool Validate()
    {
      DateTime t = System.IO.File.GetLastWriteTime(_fullName);
      if (t <= _changeDate)
      { return true; }

      _indexList = null;
      _extent = null;
      _setup = null;

      _changeDate = t;

      return false;
    }
    public IBox GetExtent()
    {
      Validate();
      if (_extent == null)
      {
        Box extent = null;
        foreach (var idx in GetIndexList())
        {
          IBox box = idx.MapBox;
          if (extent == null)
          { extent = new Box(box); }
          else
          { extent.Include(box); }
        }
        if (extent != null)
        {
          IBox box = extent;
          _extent = BoxOp.ProjectRaw(box, GetSetup().Map2Prj).Extent;
        }
      }
      return _extent;
    }

    public Setup GetSetup()
    {
      Validate();
      if (_setup == null)
      { InitData(); }
      return _setup;
    }

    public IList<ElementIndex> GetIndexList()
    {
      Validate();
      if (_indexList == null)
      {
        InitData();
      }
      return _indexList;
    }

    private void InitData()
    {
      using (OcadReader reader = OcadReader.Open(_fullName))
      {
        _indexList = reader.GetIndices();
        _setup = reader.ReadSetup();
      }
    }

    public OcadReader CreateReader()
    {
      OcadReader reader = OcadReader.Open(_fullName);
      return reader;
    }
  }
}