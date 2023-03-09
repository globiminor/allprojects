
using System.Collections.Generic;

namespace Basics.Geom.Index
{
  public class BoxTreeFullSearch: IBoxTreeSearch
  {
    public bool ChildFirst { get; set; }

    public IEnumerable<TileEntry> EnumEntries(BoxTile startTile)
    {
      foreach (var tile in EnumTiles(startTile))
      {
        foreach (TileEntry entry in tile.EnumElems())
        {
          yield return entry;
        }
      }
    }

    IEnumerable<BoxTile> IBoxTreeSearch.EnumTiles(BoxTile startTile, Box startBox) => EnumTiles(startTile);
    public IEnumerable<BoxTile> EnumTiles(BoxTile startTile)
    {
      if (!ChildFirst) yield return startTile;

      foreach (var child in new[] { startTile.Child0, startTile.Child1 })
      {
        if (child == null)
        { continue; }

        foreach (var entry in EnumTiles(child))
        { yield return entry; }
      }

      if (ChildFirst) yield return startTile;
    }
    bool IBoxTreeSearch.CheckExtent(IBox extent) => true;
    void IBoxTreeSearch.Init(IBox search) { }
  }
}
