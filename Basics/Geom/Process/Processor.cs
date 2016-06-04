
using System;
using System.Collections.Generic;

namespace Basics.Geom.Process
{
  public interface IProcessor
  { }
  public interface ISearchProcessor
  {
    ISearchEngine SearchEngine { get; set; }
  }
  public interface IContainerProcessor
  { }
  public interface ITablesProcessor
  {
    IList<TableAction> InvolvedTables { get; }
  }

  public class TableAction
  {
    public static TableAction Create<T>(ISpatialTable<T> table, Action<T> execute)
      where T : ISpatialRow
    {
      TableAction created = new TableAction();
      created.Table = table;
      created.Execute = (x) => { execute((T)x); };
      return created;
    }
    private TableAction()
    { }
    public ITable Table { get; private set; }
    public string DbConstraint { get; private set; }
    public Func<IRow, bool> Constraint { get; private set; }

    public Action<IRow> Execute { get; private set; }

    public bool IsQueried { get; set; }
    public double SearchDistance { get; private set; }
  }
  public class Processor : IProcessor
  {
  }
  public abstract class ContainerProcessor : Processor, ISearchProcessor, IContainerProcessor, ITablesProcessor
  {
    private ISearchEngine _searchEngine;
    private IList<TableAction> _involvedTables;

    public IList<TableAction> InvolvedTables
    {
      get { return _involvedTables ?? (_involvedTables = Init()); }
    }

    protected abstract IList<TableAction> Init();

    ISearchEngine ISearchProcessor.SearchEngine { get { return _searchEngine; } set { _searchEngine = value; } }
  }
}
