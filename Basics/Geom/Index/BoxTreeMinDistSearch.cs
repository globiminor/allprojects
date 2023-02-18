
using System.Collections.Generic;
using System;

namespace Basics.Geom.Index
{
  public class BoxTreeMinDistSearch : IBoxTreeSearch
  {
    private readonly Point _distPt;
    private IBox _box;
    private double _minDist2;

    public BoxTreeMinDistSearch(IPoint distPt)
    {
      if (distPt == null) throw new ArgumentNullException(nameof(distPt));
      _distPt = Point.CastOrCreate(distPt);
    }

    public void Init(IBox search)
    { _box = search; }

    public bool ElemsBeforeChildren { get; set; }

    public void SetMinDist2(double dist2)
    {
      double minDist = Math.Sqrt(dist2);
      Point pd = Point.Create_0(_distPt.Dimension);
      for (int i = 0; i < _distPt.Dimension; i++)
      { pd[i] = minDist; }
      _box = new Box(_distPt - pd, _distPt + pd);

    }

    public IEnumerable<TileEntry> EnumEntries(BoxTile startTile, Box startBox)
    {
      if (_box != null && !BoxOp.Intersects(startBox, _box))
      {
        yield break;
      }
      if (_box != null && BoxOp.GetMinDist(_distPt, startBox).OrigDist2() > _minDist2)
      {
        yield break;
      }

      if (ElemsBeforeChildren) foreach (var entry in EnumElems(startTile)) yield return entry;

      int splitDim = startTile.SplitDimension;
      var children = (startTile.Child1?.MinInParentSplitDim < _distPt[splitDim])
        ? new[] { startTile.Child1, startTile.Child0 }
        : new[] { startTile.Child0, startTile.Child1 };
      foreach (var child in children)
      {
        if (child == null)
        { continue; }

        Box childBox = startBox.Clone();
        childBox.Min[splitDim] = child.MinInParentSplitDim;
        childBox.Max[splitDim] = child.MaxInParentSplitDim;

        foreach (var entry in EnumEntries(child, childBox))
        { yield return entry; }
      }

      if (!ElemsBeforeChildren) foreach (var entry in EnumElems(startTile)) yield return entry;

    }

    private IEnumerable<TileEntry> EnumElems(BoxTile tile)
    {
      foreach (TileEntry entry in tile.EnumElems())
      {
        if (_box != null && !BoxOp.Intersects(entry.Box, _box))
        { continue; }

        yield return entry;
      }

    }
  }

}
