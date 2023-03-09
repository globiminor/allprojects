
using System.Collections.Generic;

namespace Basics.Geom.Index
{
  public interface IBoxTreeSearch
  {
    IEnumerable<BoxTile> EnumTiles(BoxTile tile, Box box);
    bool CheckExtent(IBox extent);

    void Init(IBox search);
  }
}
