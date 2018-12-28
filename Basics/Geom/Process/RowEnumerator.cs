using System;
using System.Collections.Generic;

namespace Basics.Geom.Process
{
  internal class RowEnumerator
  {
    private class Tile
    {
      private Box _box;
      public IBox Box { get { return _box; } }
      public Tile(Box box)
      {
        _box = box;
      }

      public void LoadData(ISpatialTable table, TableActions actions, SearchEngine searchEngine)
      {
        string dbConstraint = null;
        if (actions.Actions.Count == 1)
        { dbConstraint = actions.Actions[0].DbConstraint; }

        IBox searchBox = _box;
        if (actions.MaxSearchDistance > 0)
        {
          Point2D p = new Point2D(actions.MaxSearchDistance, actions.MaxSearchDistance);
          searchBox = new Box(_box.Min - p, _box.Max + p);
        }
        table.Search(searchBox, dbConstraint);
      }
    }

    private class TableActions
    {
      public readonly List<TableAction> Actions = new List<TableAction>();
      public bool IsExecuted { get; set; }
      public bool IsQueried { get; set; }
      public double MaxSearchDistance { get; set; }

      private List<TableAction> _executeActions;

      public List<TableAction> ExecuteActions
      {
        get
        {
          if (_executeActions == null)
          {
            List<TableAction> executeActions = new List<TableAction>();
            foreach (var action in Actions)
            {
              if (action.Execute == null) { continue; }
              executeActions.Add(action);
            }
            _executeActions = executeActions;
          }
          return _executeActions;
        }

      }
    }

    private readonly SearchEngine _searchEngine = new SearchEngine();
    private readonly List<ITablesProcessor> _processors;
    private readonly IBox _extent;
    private readonly double _tileSize;

    public RowEnumerator(List<ITablesProcessor> processors, IBox extent, double tileSize)
    {
      _processors = processors;
      _extent = extent;
      _tileSize = tileSize;
    }

    internal ISearchEngine GetSearchEngine()
    {
      return _searchEngine;
    }

    public IEnumerable<ProcessRow> GetProcessRows()
    {
      Dictionary<ISpatialTable, TableActions> involvedTables = new Dictionary<ISpatialTable, TableActions>();
      foreach (var processor in _processors)
      {
        foreach (var tableAction in processor.TableActions)
        {
          ISpatialTable table = (ISpatialTable)tableAction.Table;
          if (!involvedTables.TryGetValue(table, out TableActions involved))
          {
            involved = new TableActions();
            involvedTables.Add(table, involved);
          }
          involved.Actions.Add(tableAction);
          if (tableAction.Execute == null)
          {
            // if not executed, it implicitly is queried
            involved.IsQueried = true;
          }
          else
          {
            involved.IsExecuted = true;
            if (tableAction.IsQueried)
            {
              involved.IsQueried = true;
            }
          }
          involved.MaxSearchDistance = Math.Max(tableAction.SearchDistance, tableAction.SearchDistance);
        }
      }

      if (involvedTables.Count == 0)
      { yield break; }

      IBox extent = _extent ?? GetExtent(involvedTables.Keys);

      foreach (var tile in GetTiles(extent))
      {
        foreach (var pair in involvedTables)
        {
          ISpatialTable table = pair.Key;
          TableActions actions = pair.Value;
          if (actions.IsQueried)
          {
            tile.LoadData(table, actions, _searchEngine);
          }
        }

        foreach (var pair in involvedTables)
        {
          ISpatialTable table = pair.Key;
          TableActions actions = pair.Value;
          if (!actions.IsExecuted)
          {
            continue;
          }

          if (actions.IsQueried)
          {
            foreach (var row in _searchEngine.Search(table, tile.Box))
            {
              yield return new ProcessRow(row, actions.ExecuteActions);
            }
          }
          else
          {
            string dbConstraint = null;
            if (actions.Actions.Count == 1)
            { dbConstraint = actions.Actions[0].DbConstraint; }

            foreach (var row in table.Search(tile.Box, dbConstraint))
            {
              yield return new ProcessRow(row, actions.ExecuteActions);
            }
          }
        }

        CompleteTile();
      }
    }

    private IBox GetExtent(IEnumerable<ISpatialTable> tables)
    {
      Box allExtent = null;
      foreach (var table in tables)
      {
        if (allExtent == null)
        { allExtent = new Box(table.Extent); }
        else
        { allExtent.Include(table.Extent); }
      }
      return allExtent;
    }
    private void CompleteTile()
    { }

    private IEnumerable<Tile> GetTiles(IBox extent)
    {
      int dim = extent.Dimension;
      if (dim <= 0)
      {
        yield break;
      }

      Point min = Point.Create(dim);
      Point max = Point.Create(dim);
      foreach (var tile in GetTiles(extent, min, max, 0))
      {
        yield return tile;
      }
    }

    private IEnumerable<Tile> GetTiles(IBox extent, Point min, Point max, int dim)
    {
      double d0 = extent.Min[dim];
      double d1 = extent.Max[dim];
      for (double d = d0; d < d1; d += _tileSize)
      {
        min[dim] = d;
        max[dim] = Math.Min(d1, d + _tileSize);

        if (dim + 1 < extent.Dimension)
        {
          foreach (var tile in GetTiles(extent, min, max, dim + 1))
          {
            yield return tile;
          }
        }
        else
        {
          yield return new Tile(new Box(Point.Create(min), Point.Create(max)));
        }
      }
    }
  }
}
