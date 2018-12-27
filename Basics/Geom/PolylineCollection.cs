using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for Area.
  /// </summary>
  public class PolylineCollection : List<Polyline>, IMultipartGeometry
  {
    Box _extent;

    private void UpdateExtent(IBox extent)
    {
      if (_extent == null)
      {
        Point min = Point.Create(extent.Min);
        Point max = Point.Create(extent.Max);
        _extent = new Box(min, max);
      }
      else
      { _extent.Include(extent); }
    }

    private void Init()
    {
      foreach (var line in this)
      {
        UpdateExtent(line.Extent);
      }
    }

    public PolylineCollection()
    {
      Init();
    }

    public PolylineCollection(IEnumerable<Polyline> lineList)
    {
      AddRange(lineList);
      Init();
    }
    public PolylineCollection(Polyline line)
    {
      Add(line);
      Init();
    }

    public bool EqualGeometry(IGeometry other)
    { return this == other; }


    #region IGeometry Members

    public int Dimension
    { get { return Extent.Dimension; } }

    public int Topology
    { get { return 1; } }

    public bool IsWithin(IPoint point)
    {
      return false;
    }

    public Box Extent
    {
      get
      {
        if (_extent == null)
        { Init(); }
        return _extent;
      }
    }
    IBox IGeometry.Extent
    { get { return Extent; } }

    public PointCollection Border
    {
      get // TODO get
      {
        PointCollection border = new PointCollection();
        foreach (var line in this)
        {
          PointCollection lineBorder = line.Border;
          border.AddRange(lineBorder);
        }
        return border; 
      }
    }
    IGeometry IGeometry.Border
    { get { return Border; } }

    public PolylineCollection Project(IProjection projection)
    {
      PolylineCollection pPrj = new PolylineCollection();
      foreach (var polyline in this)
      { pPrj.Add(polyline.Project(projection)); }

      return pPrj;
    }
    IGeometry IGeometry.Project(IProjection projection)
    { return Project(projection); }

    public IGeometry Split(IGeometry[] border)
    {
      throw new NotImplementedException();
    }

    #endregion

    public int PointCount
    {
      get
      {
        int n = 0;
        foreach (var polyline in this)
        { n += polyline.Points.Count; }
        return n;
      }
    }

    #region IMultipartGeometry Members

    public bool IsContinuous
    {
      get { return false; }
    }

    public bool HasSubparts
    {
      get { return true; }
    }

    public IEnumerable<Polyline> Subparts()
    {
      return this;
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }
    #endregion
  }
}
