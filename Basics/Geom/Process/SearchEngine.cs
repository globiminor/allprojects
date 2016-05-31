using System;
using System.Collections.Generic;

namespace Basics.Geom.Process
{
  public interface ISearchEngine
  {
    IEnumerable<IRow> Search(ISpatialTable table, IBox box);
  }
  internal class SearchEngine : ISearchEngine
  {
    public void Load()
    { }

    public IEnumerable<IRow> Search(ISpatialTable table, IBox box)
    {
      throw new NotImplementedException();
    }
  }
}
