using System;
using System.Collections.Generic;

namespace Basics.Geom.Process
{
  public class ProcessorContainer
  {
    private readonly List<IProcessor> _processors;
    private readonly double _tileSize;
    public ProcessorContainer(IEnumerable<IProcessor> processors, double tileSize)
    {
      _processors = new List<IProcessor>(processors);
      _tileSize = tileSize;
    }
    public double TileSize { get { return _tileSize; } }


    public void Execute()
    {
      Execute(null);
    }
    private void Execute(IBox extent)
    {
      List<IProcessor> nonSpatialProcs = new List<IProcessor>();
      List<ITablesProcessor> spatialProcs = new List<ITablesProcessor>();
      SortProcs(_processors, nonSpatialProcs, spatialProcs);

      RowEnumerator rowEnumerator = new RowEnumerator(spatialProcs, extent, _tileSize);
      ISearchEngine searchEngine = rowEnumerator.GetSearchEngine();
      foreach (var spatialProc in spatialProcs)
      {
        if (spatialProc is ISearchProcessor searchProc)
        { searchProc.SearchEngine = searchEngine; }
      }
      foreach (var row in rowEnumerator.GetProcessRows())
      {
        foreach (var action in row.GetActions())
        {
          action(row.Row);
        }
      }
    }

    private static void SortProcs<T>(IEnumerable<T> procs, IList<IProcessor> nonSpatialProcs, IList<ITablesProcessor> spatialProcs)
      where T : IProcessor
    {
      foreach (var proc in procs)
      {
        if (!(proc is ITablesProcessor tablesProc))
        {
          nonSpatialProcs.Add(proc);
          continue;
        }

        bool isSpatial = true;
        foreach (var action in tablesProc.TableActions)
        {
          if (!(action.Table is ISpatialTable))
          {
            isSpatial = false;
            break;
          }
        }
        if (!isSpatial)
        {
          nonSpatialProcs.Add(proc);
          continue;
        }

        spatialProcs.Add(tablesProc);
      }
    }
  }
}
