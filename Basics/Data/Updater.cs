using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Basics.Data
{
  public class Updater
  {
    private readonly DbDataAdapter _adapter;
    private readonly CommandFormatMode _formatMode;

    [Flags]
    public enum CommandFormatMode
    {
      None = 0,
      /// <summary>
      /// Key fields are auto generated
      /// </summary>
      Auto = 1,
      /// <summary>
      /// Key fields can be changed
      /// </summary>
      KeyVariable = 2,
      /// <summary>
      /// return keys on insert
      /// </summary>
      NoKeysReturn = 4
    }

    public const string UpdateDone = "UpdateDone";
    public const string DeleteDone = "DeleteDone";

    public Updater(DbDataAdapter adapter, CommandFormatMode formatMode)
    {
      _adapter = adapter;
      _formatMode = formatMode;
    }

    public int UpdateDB(DbTransaction transaction, DataSet dataset)
    {
      int n;
      bool bHasChanges = false;

      foreach (DataTable tbl in dataset.Tables)
      {
        if (tbl.GetChanges() != null)
        {
          bHasChanges = true;
          break;
        }
      }
      if (bHasChanges == false)
      { return 0; }

      List<DataTable> tables = new List<DataTable>(dataset.Tables.Count);
      foreach (DataTable table in dataset.Tables)
      { tables.Add(table); }

      n = UpdateDB(transaction, tables, false);

      return n;
    }

    public int UpdateDB(DbTransaction transaction, DataTable table)
    {
      return UpdateDB(transaction, table, false);
    }

    public int UpdateDB(DbTransaction transaction, DataTable table, bool cascade)
    {
      return UpdateDB(transaction, new DataTable[] { table }, cascade);
    }

    public int UpdateDB(DbTransaction transaction,
                        IList<DataTable> tableList, bool cascade)
    {
      UpdateInfo up = new UpdateInfo(tableList, cascade);
      return UpdateDB(transaction, up);
    }
    public int UpdateDB(DbTransaction transaction, UpdateInfo updateInfo)
    {
      DbTransaction t = transaction;
      if (t == null)
      {
        t = _adapter.SelectCommand.Connection.BeginTransaction();
      }
      // procedure :
      //
      // delete in child tables
      // delete in table
      // update / insert in table
      // update / insert in child tables
      //
      int n = 0;

      try
      {
        // First handle single Tables, because cascading tables may need updated data!
        foreach (DataTable table in updateInfo.SingleTableList)
        {
          n += Delete(t, table, false) +
               UpdateInsert(t, table, false);
        }

        foreach (DataTable table in updateInfo.CascadeTableList)
        {
          n += Delete(t, table, true) +
               UpdateInsert(t, table, true);
        }

        if (transaction == null)
        { t.Commit(); }
      }
      catch (Exception exp)
      {
        try
        {
          if (transaction == null)
          { t.Rollback(); }
        }
        catch (SystemException) { }
        throw new Exception("Unable to save data", exp);
      }
      finally
      {
        updateInfo.ClearKeys();
      }

      if (transaction == null)
      {
        updateInfo.AcceptChanges();
      }
      return n;
    }

    public static int Delete(DbDataAdapter adapter,
                             DbTransaction transaction, DataTable table,
                             DataTableMapping mapping)
    {
      int n;
      DataTable tblUpdate = table.GetChanges(DataRowState.Deleted);
      if (tblUpdate != null)
      {
        DbCommand cmd;

        cmd = adapter.SelectCommand.Connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = string.Format("DELETE FROM {0} WHERE {1}",
                                        mapping == null ? table.TableName : mapping.SourceTable, Where(table));
        AppendKeyParameters(cmd, table, DataRowVersion.Original, ParameterDirection.Input);

        adapter.DeleteCommand = cmd;

        n = adapter.Update(tblUpdate);
      }
      else
      { n = 0; }

      return n;
    }

    public int Delete(DbTransaction transaction, DataTable table, bool cascade)
    {
      // procedure :
      //
      // delete child tables
      // delete table
      //
      DbTransaction t = transaction;
      if (t == null)
      {
        t = _adapter.SelectCommand.Connection.BeginTransaction();
      }

      int n = 0;

      if (cascade && table.ExtendedProperties.ContainsKey(DeleteDone))
      { return 0; }

      if (cascade)
      {
        foreach (DataRelation rel in table.ChildRelations)
        { n += Delete(t, rel.ChildTable, cascade); }
      }

      n += Delete(_adapter, t, table, GetMapping(table));

      if (cascade)
      { table.ExtendedProperties.Add(DeleteDone, true); }
      return n;
    }

    private DataTableMapping GetMapping(DataTable table)
    {
      string name = table.TableName;
      if (_adapter.TableMappings == null)
      { return null; }

      if (!_adapter.TableMappings.Contains(name))
      { return null; }

      DataTableMapping mapping = _adapter.TableMappings.GetByDataSetTable(name);
      return mapping;
    }
    private static string Where(DataTable table)
    {
      StringBuilder where = new StringBuilder();
      foreach (DataColumn column in table.PrimaryKey)
      {
        if (where.Length > 0)
        { where.Append(" AND "); }
        where.AppendFormat("{0} = ?", column.ColumnName);
      }
      return where.ToString();
    }

    private static string UpdateFields(DataTable table, CommandFormatMode formatMode)
    {
      IList<DataColumn> keys = table.PrimaryKey;

      bool keyFix = (formatMode & CommandFormatMode.KeyVariable) == 0;

      StringBuilder set = new StringBuilder();
      foreach (DataColumn column in table.Columns)
      {
        if (keyFix && keys.Contains(column))
        { continue; }

        if (column.ReadOnly)
        { continue; }

        if (set.Length > 0)
        { set.Append(" , "); }
        set.AppendFormat("{0} = ?", column.ColumnName);
      }
      return set.ToString();
    }

    private static void AppendKeyParameters(DbCommand command, DataTable table,
                                            DataRowVersion sourceVersion, ParameterDirection direction)
    {
      foreach (DataColumn column in table.PrimaryKey)
      {
        if (!column.AutoIncrement)
        { continue; }

        DbParameter p = command.CreateParameter();
        p.ParameterName = column.ColumnName;
        p.SourceColumn = column.ColumnName;
        p.SourceVersion = sourceVersion;
        p.Direction = direction;

        command.Parameters.Add(p);
      }
    }

    private static void AppendFieldParameters(DbCommand command, DataTable table,
                                              CommandFormatMode formatMode)
    {
      IList<DataColumn> keys = table.PrimaryKey;

      bool keyFix = (formatMode & CommandFormatMode.KeyVariable) == 0;

      foreach (DataColumn column in table.Columns)
      {
        if (keyFix && keys.Contains(column) && column.AutoIncrement)
        { continue; }

        if (column.ReadOnly)
        { continue; }

        DbParameter p = command.CreateParameter();
        p.ParameterName = column.ColumnName;
        p.SourceColumn = column.ColumnName;
        p.SourceVersion = DataRowVersion.Current;
        p.Direction = ParameterDirection.Input;

        command.Parameters.Add(p);
      }
    }

    public static int Update(DbDataAdapter adapter,
                             DbTransaction transaction, DataTable table,
                             DataTableMapping mapping, CommandFormatMode formatMode)
    {
      int n;
      DataRow[] rowUpdateLst = table.Select("", "", DataViewRowState.ModifiedCurrent);
      if (rowUpdateLst.Length > 0)
      {
        DbCommand cmd;
        cmd = adapter.SelectCommand.Connection.CreateCommand();
        cmd.Transaction = transaction;

        cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}",
                                        mapping == null ? table.TableName : mapping.SourceTable,
                                        UpdateFields(table, formatMode), Where(table));
        AppendFieldParameters(cmd, table, formatMode);
        AppendKeyParameters(cmd, table, DataRowVersion.Current, ParameterDirection.Input);
        adapter.UpdateCommand = cmd;
        n = adapter.Update(rowUpdateLst);
      }
      else
      { n = 0; }

      return n;
    }

    public static int Insert(DbDataAdapter adapter,
                             DbTransaction transaction, DataTable table,
                             DataTableMapping mapping, CommandFormatMode formatMode)
    {
      int n;
      DbTransaction t = transaction;
      if (t == null)
      {
        //TODO: dies geht nicht!!!
        t = adapter.SelectCommand.Connection.BeginTransaction();
      }

      DataRow[] rowInsertLst = table.Select("", "", DataViewRowState.Added);
      if (rowInsertLst.Length > 0)
      {
        DbCommand cmd = adapter.SelectCommand.Connection.CreateCommand();
        cmd.Transaction = transaction;

        cmd.CommandText = string.Format("INSERT INTO {0}{1}{2}",
                                        mapping == null ? table.TableName : mapping.SourceTable,
                                        ValueFields(table, formatMode), ReturnFields(table, formatMode));
        AppendFieldParameters(cmd, table, formatMode);
        if ((formatMode & CommandFormatMode.Auto) == CommandFormatMode.Auto &&
          (formatMode & CommandFormatMode.NoKeysReturn) != CommandFormatMode.NoKeysReturn)
        { AppendKeyParameters(cmd, table, DataRowVersion.Current, ParameterDirection.Output); }
        adapter.InsertCommand = cmd;

        try
        {
          table.ColumnChanging += InsertTable_OnRowChanging;
          n = adapter.Update(rowInsertLst);
        }
        finally
        { table.ColumnChanging -= InsertTable_OnRowChanging; }
      }
      else
      { n = 0; }

      return n;
    }

    private static bool _inRowChanging = false;
    private static void InsertTable_OnRowChanging(object sender, DataColumnChangeEventArgs e)
    {
      if (_inRowChanging)
      { return; }
      try
      {
        _inRowChanging = true;

        DataRow row = e.Row;
        // ensure unique primary keys for autoIncrement fields
        // assumption : ObjectID is primary key
        if (row.Table.PrimaryKey.Length < 1 || row.Table.Rows.Count < 2)
        { return; }

        bool autoIncrement = true;
        bool match = false;
        foreach (DataColumn keyCol in row.Table.PrimaryKey)
        {
          if (keyCol.AutoIncrement == false)
          {
            autoIncrement = false;
            break;
          }
          if (keyCol == e.Column)
          { match = true; }
        }

        if (autoIncrement == false)
        { return; }
        if (match == false)
        { return; }

        if (row.Table.PrimaryKey.Length != 1)
        { throw new NotImplementedException("Cannot handle multiple field keys"); }

        DataRow rowConflict;

        rowConflict = row.Table.Rows.Find(e.ProposedValue);
        if (rowConflict != null && rowConflict != row)
        {
          if ((rowConflict.RowState == DataRowState.Added) == false)
          { throw new InvalidProgramException("invalid row state"); }

          rowConflict[e.Column] = row.Table.NewRow()[e.Column];
        }
      }
      finally
      { _inRowChanging = false; }
    }


    private int UpdateInsert(DbTransaction transaction,
                             DataTable table, bool cascade)
    {
      // procedure :
      //
      // update / insert in table
      // update / insert in child tables
      //
      int n = 0;

      if (cascade && table.ExtendedProperties.ContainsKey(UpdateDone))
      { return 0; }

      DataTableMapping mapping = GetMapping(table);
      n += Update(_adapter, transaction, table, mapping, _formatMode);
      n += Insert(_adapter, transaction, table, mapping, _formatMode);

      if (cascade)
      {
        foreach (DataRelation rel in table.ChildRelations)
        { UpdateInsert(transaction, rel.ChildTable, cascade); }
      }

      if (cascade)
      { table.ExtendedProperties.Add(UpdateDone, true); }

      return n;
    }

    private static string ValueFields(DataTable table, CommandFormatMode formatMode)
    {
      IList<DataColumn> keys = table.PrimaryKey;

      bool autoKeys = (formatMode & CommandFormatMode.Auto) == CommandFormatMode.Auto;

      StringBuilder attrs = new StringBuilder();
      StringBuilder values = new StringBuilder();
      foreach (DataColumn column in table.Columns)
      {
        if (autoKeys && keys.Contains(column) && column.AutoIncrement)
        { continue; }

        if (column.ReadOnly)
        { continue; }

        if (attrs.Length > 0)
        {
          attrs.Append(", ");
          values.Append(", ");
        }
        attrs.AppendFormat("{0}", column.ColumnName);
        values.AppendFormat("?");
      }
      string set = string.Format("({0}) VALUES ({1})", attrs, values);
      return set;
    }

    private static string ReturnFields(DataTable table, CommandFormatMode formatMode)
    {
      if ((formatMode & CommandFormatMode.Auto) == CommandFormatMode.Auto)
      { return null; }

      if ((formatMode & CommandFormatMode.NoKeysReturn) == CommandFormatMode.NoKeysReturn)
      { return null; }

      bool first;
      StringBuilder keys = new StringBuilder();

      first = true;
      foreach (DataColumn column in table.PrimaryKey)
      {
        if (!column.AutoIncrement)
        { continue; }

        if (first == false)
        { keys.Append(", "); }
        else
        { keys.Append(" RETURNING "); }
        keys.Append(column.ColumnName);
        first = false;
      }

      first = true;
      foreach (DataColumn column in table.PrimaryKey)
      {
        if (!column.AutoIncrement)
        { continue; }

        if (first == false)
        { keys.Append(", "); }
        else
        { keys.Append(" INTO "); }
        keys.Append("?");
        first = false;
      }

      return keys.ToString();
    }


    internal static void ClearKey(DataTable table, string key)
    {
      if (table.ExtendedProperties.ContainsKey(key))
      { table.ExtendedProperties.Remove(key); }

      foreach (DataRelation rel in table.ChildRelations)
      { ClearKey(rel.ChildTable, key); }
    }
  }
}