
using System;
using System.Collections.Generic;
using Basics.Geom.Process;

namespace Grid.Processors
{
  public class SchartenhoeheInit : ContainerProcessor
  {
    private readonly IDoubleGrid _grid;
    private GridTable _gridTable;

    public SchartenhoeheInit(IDoubleGrid grid)
    {
      _grid = grid;
    }

    protected override IList<TableAction> Init()
    {
      GridTable gridTbl = new GridTable(_grid);
      TableAction action = TableAction.Create(gridTbl, HandleGridRow);
      return new List<TableAction> { action };
    }

    private void HandleGridRow(GridRow gridRow)
    { }
  }
}
