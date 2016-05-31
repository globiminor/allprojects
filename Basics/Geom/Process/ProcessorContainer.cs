using System;
using System.Collections.Generic;

namespace Basics.Geom.Process
{
  public class ProcessorContainer
  {
    private readonly List<IProcessor> _processors;
    public ProcessorContainer(IEnumerable<IProcessor> processors)
    {
      _processors = new List<IProcessor>(processors);
    }

    public void Execute()
    {
      List<IProcessor> nonSpatialProcs = new List<IProcessor>();
      List<ITablesProcessor> spatialProcs = new List<ITablesProcessor>();
      SortProcs(_processors, nonSpatialProcs, spatialProcs);

      RowEnumerator rowEnumerator = new RowEnumerator(spatialProcs);
      ISearchEngine searchEngine = rowEnumerator.GetSearchEngine();
      foreach (ITablesProcessor spatialProc in spatialProcs)
      {
        ISearchProcessor searchProc = spatialProc as ISearchProcessor;
        if (searchProc != null)
        { searchProc.SearchEngine = searchEngine; }
      }
      foreach (ProcessRow row in rowEnumerator.GetProcessRows())
      {
        foreach (Action<IRow> action in row.GetActions())
        {
          action(row.Row);
        }
      }
    }

    private static void SortProcs<T>(IEnumerable<T> procs, IList<IProcessor> nonSpatialProcs, IList<ITablesProcessor> spatialProcs)
      where T : IProcessor
    {
      foreach (IProcessor proc in procs)
      {
        ITablesProcessor tablesProc = proc as ITablesProcessor;
        if (tablesProc == null)
        {
          nonSpatialProcs.Add(proc);
          continue;
        }

        bool isSpatial = true;
        foreach (ITable table in tablesProc.InvolvedTables)
        {
          if (!(table is ISpatialTable))
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
