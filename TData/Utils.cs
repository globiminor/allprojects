
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TData
{
  public static class Utils
  {
    private class TableInfo
    {
      public DataTable Table;
      public List<DataRelation> Relations;
      public int Hierarchie;
      public TableInfo(DataTable table, int hierarchie, DataRelation relation)
      {
        Table = table;
        Hierarchie = hierarchie;

        Relations = new List<DataRelation>();
        if (relation != null)
        { Relations.Add(relation); }
      }

      public static int Compare(TableInfo x, TableInfo y)
      { return x.Hierarchie.CompareTo(y.Hierarchie); }
    }

    public static void ClearCascade(DataTable table)
    {
      foreach (DataRelation rel in table.ChildRelations)
      {
        ClearCascade(rel.ChildTable);
      }
      table.Clear();
    }
    /// <summary>
    /// Deletes row and all its child rows (cascading)
    /// Remark: use DataRelation properties (enable constraints) ?
    /// </summary>
    // History: FAR 19.05.05 initial coding
    public static void DeleteCascade(DataRow row)
    {
      foreach (DataRelation rel in row.Table.ChildRelations)
      {
        foreach (DataRow child in row.GetChildRows(rel))
        {
          DeleteCascade(child);
        }
      }
      row.Delete();
    }

    public static void RemoveCascade(DataRow row)
    {
      foreach (DataRelation rel in row.Table.ChildRelations)
      {
        foreach (DataRow child in row.GetChildRows(rel))
        {
          RemoveCascade(child);
        }
      }
      row.Table.Rows.Remove(row);
    }

    /// <summary>
    /// Checked, ob row oder - falls cascade - Childrows (rekursiv) geaendert wurden
    /// </summary>
    // History FAR 18.01.2006
    public static bool HasChanges(DataRow row, bool cascade)
    {
      bool bHasChanges = (row.RowState != DataRowState.Unchanged);
      if (cascade && bHasChanges == false)
      {
        foreach (DataRelation rel in row.Table.ChildRelations)
        {
          DataRow[] pChildRows = row.GetChildRows(rel);
          if (row.GetChildRows(rel, DataRowVersion.Original).Length != pChildRows.Length)
          { return true; }
          foreach (DataRow pChildRow in pChildRows)
          {
            if (HasChanges(pChildRow, cascade))
            { return true; }
          }
        }
      }
      return bHasChanges;
    }

    /// <summary>
    /// Checked, ob table oder - falls cascade - Childtables (rekursiv) geaendert wurden
    /// </summary>
    // History FAR 18.01.2006
    public static bool HasChanges(DataTable table, bool cascade)
    {
      bool bHasChanges = (table.GetChanges() != null);
      if (cascade && bHasChanges == false)
      {
        foreach (DataRelation rel in table.ChildRelations)
        {
          if (HasChanges(rel.ChildTable, cascade))
          { return true; }
        }
      }
      return bHasChanges;
    }

    /// <summary>
    /// Verwirft Aenderungen an table und - falls cascade - Childtables (rekursiv)
    /// </summary>
    // History FAR 18.01.2006
    public static void RejectChanges(DataTable table, bool cascade)
    {
      if (cascade)
      {
        foreach (DataRelation rel in table.ChildRelations)
        { RejectChanges(rel.ChildTable, cascade); }
      }
      table.RejectChanges();
    }

    /// <summary>
    /// Finds relation. If parentCol is null, only child column is checked for a match
    /// </summary>
    /// <param name="parentCol">can be null</param>
    /// <param name="childCol">must be set</param>
    /// <returns></returns>
    public static DataRelation FindRelation(DataColumn parentCol, DataColumn childCol)
    {
      DataTable tblChild = childCol.Table;
      foreach (DataRelation rel in tblChild.ParentRelations)
      {
        if (rel.ChildColumns.Length == 1 && rel.ChildColumns[0] == childCol
          && (parentCol == null || rel.ParentColumns[0] == parentCol))
        {
          return rel;
        }
      }
      return null;
    }

    public static DataRelation FindRelation<U, V>(
      U parentTab, V childTab)
      where U : DataTable
      where V : DataTable
    {
      if (parentTab == null)
      {
        Type parentTabType = typeof(U);
        foreach (DataRelation rel in childTab.ParentRelations)
        {
          if (rel.ParentTable.GetType() == parentTabType)
          { return rel; }
        }
      }
      else if (childTab == null)
      {
        Type childTabType = typeof(V);
        foreach (DataRelation rel in parentTab.ChildRelations)
        {
          if (rel.ChildTable.GetType() == childTabType)
          { return rel; }
        }
      }
      else
      {
        foreach (DataRelation rel in parentTab.ChildRelations)
        {
          if (rel.ChildTable == childTab)
          { return rel; }
        }
      }

      return null;
    }

    public static DataView GetChildView(DataRow row, DataRelation relation)
    {
      if (relation.ParentTable != row.Table) throw new InvalidOperationException("Tables do not match");
      DataView view = new DataView(relation.ChildTable);

      StringBuilder filter = new StringBuilder();
      int n = relation.ParentColumns.Length;
      for (int i = 0; i < n; i++)
      {
        if (filter.Length > 0)
        { filter.Append(" AND "); }

        DataColumn parentCol = relation.ParentColumns[i];
        DataColumn childCol = relation.ChildColumns[i];
        filter.AppendFormat("{0} = {1}", childCol.ColumnName, row[parentCol]);
      }
      view.RowFilter = filter.ToString();
      return view;
    }

    public static string GetInListStatement(IEnumerable<DataRow> rows, DataColumn valueColumn, DataColumn whereColumn)
    {
      string preString = "";
      string postString = "";

      if (whereColumn.DataType == typeof(string))
      {
        preString = "'";
        postString = "'";
      }

      StringBuilder inList = new StringBuilder();
      foreach (DataRow row in rows)
      {
        if (inList.Length > 0)
        { inList.Append(", "); }

        inList.AppendFormat("{0}{1}{2}", preString, row[valueColumn.ColumnName], postString);
      }
      if (inList.Length == 0)
      {
        return "1 = 0"; // false expression
      }
      string where = string.Format("{0} IN ({1})", whereColumn.ColumnName, inList);

      return where;
    }

    /// <summary>
    /// Fill Data recursive
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public static int Fill(Adapter adapter, DataRow row)
    {
      DataColumn key = row.Table.PrimaryKey[0];
      string where = string.Format("{0} = {1}", key, row[key]);
      List<DataRelation> relations = new List<DataRelation>();
      foreach (DataRelation relation in row.Table.ChildRelations)
      { relations.Add(relation); }

      return Fill(adapter, row.Table, where, relations);
    }

    /// <summary>
    /// Fill Data recursive
    /// </summary>
    public static int Fill(Adapter adapter, DataTable table, string where, IList<DataRelation> topRelations)
    {
      // Get Parent Table rows
      Dictionary<string, DataTable> clones = new Dictionary<string, DataTable>();
      DataTable parentClone = table.Clone();

      DataSet ds = table.DataSet;
      bool origEnforce = ds.EnforceConstraints;
      int n;

      try
      {
        ds.EnforceConstraints = false;
        n = adapter.Fill(parentClone, where);

        foreach (DataRow row in parentClone.Rows)
        { table.LoadDataRow(row.ItemArray, true); }

        clones.Add(parentClone.TableName, parentClone);

        n += Fill(adapter, topRelations, clones);
      }
      finally
      {
        try
        {
          ds.EnforceConstraints = origEnforce;
        }
        catch (ConstraintException exp)
        {
          string rowError = GetRowErrors(ds);
          throw new ConstraintException(rowError, exp);
        }
      }

      return n;
    }

    /// <summary>
    /// Fill Data recursive
    /// </summary>
    public static int Fill(Adapter adapter, IList<DataRelation> topRelations)
    {
      if (topRelations == null || topRelations.Count == 0)
      { return 0; }

      DataSet ds = topRelations[0].DataSet;
      Dictionary<string, DataTable> clones = new Dictionary<string, DataTable>();
      foreach (DataRelation relation in topRelations)
      {
        DataTable tbl = relation.ParentTable;
        DataTable clone;
        if (clones.TryGetValue(tbl.TableName, out clone) == false)
        {
          clone = tbl.Copy();
          clones.Add(clone.TableName, clone);
        }
      }

      bool origEnforce = ds.EnforceConstraints;
      int n = 0;

      try
      {
        n += Fill(adapter, topRelations, clones);
      }
      finally
      {
        try
        {
          ds.EnforceConstraints = origEnforce;
        }
        catch (ConstraintException exp)
        {
          string rowError = GetRowErrors(ds);
          throw new ConstraintException(rowError, exp);
        }
      }

      return n;
    }

    /// <summary>
    /// Fills rows into table and loads missing rows, that are required by parent relations
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="table"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public static int FillRelated(Adapter adapter, DataTable table, string where)
    {
      DataTable clone;
      int n = 0;
      if (where != null)
      {
        clone = table.Clone();
        n = adapter.Fill(clone, where);
      }
      else
      {
        clone = table.Copy();
      }

      foreach (DataRelation rel in table.ParentRelations)
      {
        DataTable parentTable = rel.ParentTable;
        DataColumn parentColumn = rel.ParentColumns[0];
        DataColumn childColumn = rel.ChildColumns[0];
        DataView v = new DataView(parentTable);

        List<DataRow> missing = new List<DataRow>();

        foreach (DataRow row in clone.Rows)
        {
          object key = row[childColumn.ColumnName];
          if (key == DBNull.Value)
          { continue; }

          v.RowFilter = string.Format("{0} = {1}", parentColumn.ColumnName, key);
          if (v.Count == 0)
          { missing.Add(row); }
        }

        if (missing.Count > 0)
        {
          string parentWhere = GetInListStatement(missing, childColumn, parentColumn);
          FillRelated(adapter, parentTable, parentWhere);
        }
      }

      if (where != null)
      {
        foreach (DataRow row in clone.Rows)
        { table.LoadDataRow(row.ItemArray, true); }
      }

      return n;
    }

    public static bool ValidateParents(DataSet dataSet, out string error)
    {
      foreach (DataTable table in dataSet.Tables)
      {
        foreach (DataRelation relation in table.ParentRelations)
        {
          if (relation.ParentColumns.Length != 1)
          { continue; }

          foreach (DataRow row in table.Rows)
          {
            if (row[relation.ChildColumns[0]] == DBNull.Value)
            { continue; }

            if (row.GetParentRow(relation) == null)
            {
              error = string.Format("Ungueltige Parent-Verknüpfung : " + Environment.NewLine +
                "ParentColumn: {0}.{1}; Id: {2}; ChildColumn: {3}.{4}",
                relation.ParentTable.TableName, relation.ParentColumns[0].ColumnName,
                row[relation.ChildColumns[0]],
                relation.ChildTable.TableName, relation.ChildColumns[0].ColumnName);

              return false;
            }
          }
        }
      }

      error = "";
      return true;
    }

    public static void AddForeignKeyConstraint(DataRelation relation)
    {
      ForeignKeyConstraint constraint = new ForeignKeyConstraint(
        relation.ParentColumns, relation.ChildColumns);

      constraint.UpdateRule = Rule.Cascade;
      constraint.DeleteRule = Rule.Cascade;
      constraint.AcceptRejectRule = AcceptRejectRule.None;

      relation.ChildColumns[0].Table.Constraints.Add(constraint);
    }

    public static DataRow Find(DataTable table, params object[] primaryKeys)
    {
      StringBuilder command = new StringBuilder();
      int n = primaryKeys.Length;
      if (table.PrimaryKey.Length != n)
      { throw new InvalidOperationException("Keys do not match"); }

      for (int iCol = 0; iCol < n; iCol++)
      {
        DataColumn col = table.PrimaryKey[iCol];
        object val = primaryKeys[iCol];
        if (command.Length != 0)
        { command.Append(" AND "); }
        command.AppendFormat("{0} = {1}", col.ColumnName, val);
      }
      DataRow[] rows = table.Select(command.ToString());
      if (rows.Length == 0)
      { return null; }
      else if (rows.Length == 1)
      { return rows[0]; }
      else
      { throw new DataException(string.Format("Invalid primary key {0}", command)); }
    }

    public static DataTable CopyPlusEmptyRow(DataTable table)
    {
      DataTable copy = table.Copy();
      DataRow emptyRow = copy.NewRow();
      copy.Rows.Add(emptyRow);

      return copy;
    }

    private static void GetTables(DataTable table, DataRelation parentRelation,
                                  Dictionary<DataTable, TableInfo> tables, int hierarchy)
    {
      TableInfo h;
      if (tables.TryGetValue(table, out h) == false)
      { tables.Add(table, new TableInfo(table, hierarchy, parentRelation)); }
      else
      {
        hierarchy = Math.Max(hierarchy, tables[table].Hierarchie);
        tables[table].Hierarchie = hierarchy;
        tables[table].Relations.Add(parentRelation);
      }

      foreach (DataRelation rel in table.ChildRelations)
      {
        GetTables(rel.ChildTable, rel, tables, hierarchy + 1);
      }
    }

    private static int Fill(Adapter adapter, IList<DataRelation> topRelations, Dictionary<string, DataTable> clones)
    {
      int n = 0;
      // Prepare tables
      Dictionary<DataTable, TableInfo> tables = new Dictionary<DataTable, TableInfo>();
      foreach (DataRelation relation in topRelations)
      { GetTables(relation.ChildTable, relation, tables, 0); }

      List<TableInfo> tableInfos = new List<TableInfo>(tables.Values);
      tableInfos.Sort(TableInfo.Compare);

      // get rows cascading
      foreach (TableInfo info in tableInfos)
      {
        if (info.Table.Rows.Count > 0)
        { continue; }

        DataTable clone = info.Table.Clone();
        StringBuilder childWhere = new StringBuilder();
        foreach (DataRelation relation in info.Relations)
        {
          DataTable parent = clones[relation.ParentTable.TableName];
          string parentCol = relation.ParentColumns[0].ColumnName;
          string childCol = relation.ChildColumns[0].ColumnName;

          string list = GetInListStatement(parent.Select(), parent.Columns[parentCol], clone.Columns[childCol]);
          if (childWhere.Length > 0)
          { childWhere.Append(" OR "); }
          childWhere.Append(list);
        }

        n += adapter.Fill(clone, childWhere.ToString());
        foreach (DataRow row in clone.Rows)
        { info.Table.LoadDataRow(row.ItemArray, true); }

        clones.Add(clone.TableName, clone);
      }
      return n;
    }

    private static string GetRowErrors(DataSet ds)
    {
      StringBuilder error = new StringBuilder();
      foreach (DataTable tbl in ds.Tables)
      {
        if (tbl.HasErrors)
        {
          error.AppendFormat("Table {0} has errors:", tbl.TableName);
          error.AppendLine();
          foreach (DataRow row in tbl.GetErrors())
          {
            error.AppendLine(row.RowError);
            //foreach (DataColumn column in row.GetColumnsInError())
            //{
            //  row.GetColumnError(column);
            //}
          }
        }
      }
      return error.ToString();
    }
  }
}
