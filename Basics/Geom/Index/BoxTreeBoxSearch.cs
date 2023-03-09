
using System.Collections.Generic;
using System;
using Basics.Cmd;

namespace Basics.Geom.Index
{
  public class BoxTreeBoxSearch : IBoxTreeSearch
  {
    private IBox _box;
    public bool ChildFirst { get; set; }
    public BoxTreeBoxSearch(IBox box)
    {
      if (box == null) throw new ArgumentNullException(nameof(box));
      _box = box;
    }

    void IBoxTreeSearch.Init(IBox search) 
    { _box = search; }

    public bool CheckExtent(IBox extent)
    {
      return BoxOp.Intersects(extent, _box);
    }

    public IEnumerable<BoxTile> EnumTiles(BoxTile startTile, Box startBox)
    {
      if (!BoxOp.Intersects(startBox, _box))
      {
        yield break;
      }

      if (!ChildFirst) yield return startTile;

      foreach (var child in new[] { startTile.Child0, startTile.Child1 })
      {
        if (child == null)
        { continue; }

        Box childBox = startBox.Clone();
        int splitDim = startTile.SplitDimension;
        childBox.Min[splitDim] = child.MinInParentSplitDim;
        childBox.Max[splitDim] = child.MaxInParentSplitDim;

        foreach (var tile in EnumTiles(child, childBox))
        { yield return tile; }
      }

      if (ChildFirst) yield return startTile;
    }
  }
}
