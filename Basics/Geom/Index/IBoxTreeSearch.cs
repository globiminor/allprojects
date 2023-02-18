
using System.Collections.Generic;

namespace Basics.Geom.Index
{
  public interface IBoxTreeSearch
  {
    IEnumerable<TileEntry> EnumEntries(BoxTile tile, Box box);
    void Init(IBox search);
  }
}
