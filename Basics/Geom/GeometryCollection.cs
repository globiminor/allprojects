using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  public class GeometryCollection : List<IGeometry>, IMultipartGeometry
  {
    public GeometryCollection()
    { }
    public GeometryCollection(IList<IGeometry> list)
    {
      AddRange(list);
    }

    public bool EqualGeometry(IGeometry other)
    { return this == other; }

    public new IGeometry this[int index]
    {
      get
      {
        if (index < 0)
        { index = Count + index; }
        return base[index];
      }
      set
      {
        if (index < 0)
        { index = Count + index; }
        base[index] = value;
      }
    }
    int IGeometry.Dimension
    {
      get
      {
        int dim = 0;
        foreach (IGeometry geom in this)
        {
          dim = Math.Max(geom.Dimension, dim);
        }
        return dim;
      }
    }
    int IGeometry.Topology
    {
      get
      {
        int topo = 0;
        foreach (IGeometry geom in this)
        {
          topo = Math.Max(geom.Topology, topo);
        }
        return topo;
      }
    }

    public IBox Extent
    {
      get
      {
        if (Count == 0)
        { return null; }
        IBox box = this[0].Extent.Clone();
        foreach (IGeometry geom in this)
        {
          box.Include(geom.Extent);
        }
        return box;
      }
    }
    public bool IsWithin(IPoint point)
    {
      foreach (IGeometry geom in this)
      {
        if (geom.IsWithin(point))
        { return true; }
      }
      return false;
    }
    public IGeometry Border
    {
      get
      {
        GeometryCollection border = new GeometryCollection();
        foreach (IGeometry geom in this)
        {
          border.Add(geom.Border);
        }
        return border;
      }
    }
    public IGeometry Split(IGeometry[] border)
    {
      throw new NotImplementedException("not implemented");
    }
    public IGeometry Project(IProjection projection)
    {
      GeometryCollection prj = new GeometryCollection();
      foreach (IGeometry geom in this)
      {
        prj.Add(geom.Project(projection));
      }
      return prj;
    }
    public bool IsContinuous
    { get { return false; } }
    public bool HasSubparts
    { get { return true; } }

    public IEnumerable<IGeometry> Subparts()
    {
      return this;
    }
    System.Collections.IEnumerable IMultipartGeometry.Subparts()
    { return Subparts(); }
  }
}
