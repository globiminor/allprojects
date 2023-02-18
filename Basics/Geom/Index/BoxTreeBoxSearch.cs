
using System.Collections.Generic;
using System;

namespace Basics.Geom.Index
{
  public class BoxTreeBoxSearch : IBoxTreeSearch
  {
    private IBox _box;
    public BoxTreeBoxSearch(IBox box)
    {
      if (box == null) throw new ArgumentNullException(nameof(box));
      _box = box;
    }

    void IBoxTreeSearch.Init(IBox search) 
    { _box = search; }

    public IEnumerable<TileEntry> EnumEntries(BoxTile startTile, Box startBox)
    {
      if (!BoxOp.Intersects(startBox, _box))
      {
        yield break;
      }

      foreach (TileEntry entry in startTile.EnumElems())
      {
        if (!BoxOp.Intersects(entry.Box, _box))
        { continue; }

        yield return entry;
      }

      foreach (var child in new[] { startTile.Child0, startTile.Child1 })
      {
        if (child == null)
        { continue; }

        Box childBox = startBox.Clone();
        int splitDim = startTile.SplitDimension;
        childBox.Min[splitDim] = child.MinInParentSplitDim;
        childBox.Max[splitDim] = child.MaxInParentSplitDim;

        foreach (var entry in EnumEntries(child, childBox))
        { yield return entry; }
      }
    }

  }

}
