
using System.Collections.Generic;

namespace Basics.Geom.Index
{
  public class BoxTreeFullSearch: IBoxTreeSearch
  {
    public IEnumerable<TileEntry> EnumEntries(BoxTile startTile, Box dummy)
    {
      foreach (TileEntry entry in startTile.EnumElems())
      {
        yield return entry;
      }

      foreach (var child in new[] { startTile.Child0, startTile.Child1 })
      {
        if (child == null)
        { continue; }

        foreach (var entry in EnumEntries(child, null))
        { yield return entry; }
      }
    }
    void IBoxTreeSearch.Init(IBox search) { }
  }
}
