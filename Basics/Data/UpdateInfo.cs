using System.Collections.Generic;
using System.Data;

namespace Basics.Data
{
  public class UpdateInfo
  {
    private List<DataTable> _cascadeTables;
    private List<DataTable> _singleTables;

    public UpdateInfo(DataTable table)
      : this(table, false)
    { }

    public UpdateInfo(DataTable table, bool cascade)
      : this(new DataTable[] { table }, cascade)
    { }

    public UpdateInfo(IList<DataTable> tableList, bool cascade)
    {
      if (cascade)
      {
        _cascadeTables = new List<DataTable>(tableList);
        _singleTables = new List<DataTable>();
      }
      else
      {
        _cascadeTables = new List<DataTable>();
        _singleTables = new List<DataTable>(tableList);
      }
    }

    public IList<DataTable> CascadeTableList
    { get { return _cascadeTables; } }
    public IList<DataTable> SingleTableList
    { get { return _singleTables; } }

    public void Add(UpdateInfo add)
    {
      _cascadeTables.AddRange(add.CascadeTableList);
      _singleTables.AddRange(add.SingleTableList);
    }

    public void ClearKeys()
    {
      foreach (var table in _cascadeTables)
      {
        Updater.ClearKey(table, Updater.DeleteDone);
        Updater.ClearKey(table, Updater.UpdateDone);
      }
    }

    public void AcceptChanges()
    {
      foreach (var table in _cascadeTables)
      { AcceptChanges(table, true); }

      foreach (var table in _singleTables)
      { AcceptChanges(table, false); }
    }

    public static void AcceptChanges(DataTable table, bool cascade)
    {
      if (table.GetChanges() != null)
      {
        table.AcceptChanges();
      }
      if (cascade)
      {
        foreach (var o in table.ChildRelations)
        {
          DataRelation rel = (DataRelation)o;
          AcceptChanges(rel.ChildTable, cascade);
        }
      }
    }

  }
}