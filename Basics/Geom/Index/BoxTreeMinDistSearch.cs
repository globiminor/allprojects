
using System.Collections.Generic;
using System;

namespace Basics.Geom.Index
{
  public class BoxTreeMinDistSearch : IBoxTreeSearch
  {
    private readonly IBox _distBox;
    private IBox _box;
    private double _minDist2;

    public BoxTreeMinDistSearch(IBox distBox)
    {
      if (distBox == null) throw new ArgumentNullException(nameof(distBox));
      _distBox =  distBox;
    }

    public void Init(IBox search)
    { _box = search; }

    public bool ChildFirst { get; set; }

    public double? MinDist { get; private set; }
    public void SetMinDist2(double dist2)
    {
      SetMinDist(Math.Sqrt(dist2));
      _minDist2 = dist2;
    }
    private void SetMinDist(double minDist)
    {
      Point pd = Point.Create_0(_distBox.Dimension);
      for (int i = 0; i < _distBox.Dimension; i++)
      { pd[i] = minDist; }
      MinDist = minDist;
      _box = new Box(_distBox.Min - pd, _distBox.Max + pd);
    }

    public IEnumerable<BoxTile> EnumTiles(BoxTile startTile, Box startBox)
    {
      if (_box != null && !BoxOp.Intersects(startBox, _box))
      {
        yield break;
      }
      if (_box != null && BoxOp.GetMinDist(_distBox, startBox).OrigDist2() > _minDist2)
      {
        yield break;
      }

      if (!ChildFirst && startTile.ElemsCount > 0) yield return startTile;

      int splitDim = startTile.SplitDimension;
      var children = (startTile.Child1?.MinInParentSplitDim < _distBox.Min[splitDim])
        ? new[] { startTile.Child1, startTile.Child0 }
        : new[] { startTile.Child0, startTile.Child1 };
      foreach (var child in children)
      {
        if (child == null)
        { continue; }

        Box childBox = startBox.Clone();
        childBox.Min[splitDim] = child.MinInParentSplitDim;
        childBox.Max[splitDim] = child.MaxInParentSplitDim;

        foreach (var tile in EnumTiles(child, childBox))
        { yield return tile; }
      }

      if (ChildFirst && startTile.ElemsCount > 0) yield return startTile;

    }

    public bool CheckExtent(IBox extent)
    {
      bool valid = _box == null || BoxOp.Intersects(extent, _box);
      return valid;
    }
  }

}
