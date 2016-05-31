using System;
using System.Collections.Generic;

namespace Basics.Geom.Process
{
  internal class ProcessRow
  {
    private readonly IList<TableAction> _actions;
    public ProcessRow(IRow row, IList<TableAction> actions)
    {
      Row = row;
      _actions = actions;
    }

    public IRow Row { get; private set; }

    public IEnumerable<Action<IRow>> GetActions()
    {
      foreach (TableAction action in _actions)
      {
        if (action.Constraint != null && !action.Constraint(Row))
        { continue; }

        yield return action.Execute;
      }
    }
  }
}
