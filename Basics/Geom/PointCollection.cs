using System;
using System.Collections.Generic;

namespace Basics.Geom
{
  /// <summary>
  /// Summary description for PointCollection.
  /// </summary>
  public class PointCollection : List<IPoint>, IGeometry, IMultipartGeometry
  {
    public new IPoint this[int index]
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

    public bool EqualGeometry(IGeometry other)
    { return this == other; }

    public IGeometry Border
    { get { return null; } }

    public int Topology
    { get { return 0; } }

    public int Dimension
    {
      get
      {
        int iDim = 0;
        foreach (IPoint p in this)
        { iDim = Math.Max(iDim,p.Dimension); }
        return iDim;
      }
    }

    public bool IsWithin(IPoint point)
    {
      return false;
    }
    public IBox Extent
    {
      get
      {
        Box box = null;
        foreach (IPoint p in this)
        {
          if (box == null)
          { box = new Box(Point.Create(p), Point.Create(p)); }
          else
          { box.Include(p); }
        }

        return box;
      }
    }

    public PointCollection Project(IProjection projection)
    {
      PointCollection pPrjList = new PointCollection();
      foreach (IPoint p in this)
      { pPrjList.Add(p.Project(projection)); }

      return pPrjList;
    }
    IGeometry IGeometry.Project(IProjection projection)
    { return Project(projection); }

    public bool Intersects(IGeometry other)
    {
      return GeometryOperator.Intersects(this, other);
    }

    public PointCollection Clone()
    {
      PointCollection clone = new PointCollection();
      foreach (IPoint p in this)
      {
        clone.Add(Point.Create(p));
      }
      return clone;
    }

    public IGeometry Split(IGeometry[] border)
    {
      throw new NotImplementedException();
    }

    #region IMultipartGeometry Members

    public bool IsContinuous
    {
      get
      { return false; }
    }
    public bool HasSubparts
    {
      get
      { return true;	}
    }

    public IEnumerable<IPoint> Subparts()
    {
      return this;
    }
    IEnumerable<IGeometry> IMultipartGeometry.Subparts()
    { return Subparts(); }

    #endregion
  }
}
