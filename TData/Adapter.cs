using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Basics.Geom;

namespace TData
{
  public class Adapter
  {
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
      KeyVariable = 2
    }

    public const string UpdateDone = "UpdateDone";
    public const string DeleteDone = "DeleteDone";

    private DbDataAdapter _adapter;

    private CommandFormatMode _formatMode = CommandFormatMode.Auto;

    public Adapter(DbDataAdapter adapter)
    {
      _adapter = adapter;
    }

    public DbDataAdapter DataAdapter
    {
      get { return _adapter; }
    }

    public CommandFormatMode FormatMode
    {
      get { return _formatMode; }
      set { _formatMode = value; }
    }

    /// <summary>
    /// Fill rows related to row from ArcGIS into relation child table
    /// </summary>
    public int SelectRelated(DataRow row, DataRelation relation)
    {
      if (relation.ParentColumns.Length != 1) throw new InvalidOperationException("parent columns");
      if (relation.ChildColumns.Length != 1) throw new InvalidOperationException("child columns");

      string where = string.Format("{0} = {1}",
        relation.ChildColumns[0].ColumnName,
        row[relation.ParentColumns[0]]);

      int n = Fill(relation.ChildTable, where);
      return n;
    }

    /// <summary>
    /// Fill rows related to row from ArcGIS into relation child table
    /// </summary>
    public int SelectRelated(IList<DataRow> rows, DataRelation relation)
    {
      if (relation.ParentColumns.Length != 1) throw new InvalidOperationException("parent columns");
      if (relation.ChildColumns.Length != 1) throw new InvalidOperationException("child columns");

      string where = Utils.GetInListStatement(rows, relation.ParentColumns[0], relation.ChildColumns[0]);

      int n = Fill(relation.ChildTable, where);
      return n;
    }

    /// <summary>
    /// loads all data of table corresponding to where form database to local and deletes all data in datatable
    /// </summary>
    public int ClearTable(DataTable table, string where)
    {
      Fill(table, where);
      table.AcceptChanges();

      int n = table.Rows.Count;
      for (int i = 0; i < n; i++)
      {
        table.Rows[i].Delete();
      }
      return n;
    }

    public bool Exists(string name)
    {
      bool exists = Exists(_adapter, _adapter.TableMappings.GetByDataSetTable(name).SourceTable);
      return exists;
    }

    public static bool Exists(DbDataAdapter adapter, string fullTableName)
    {
      try
      {
        DataTable tbl = adapter.SelectCommand.Connection.GetSchema(fullTableName);
        return tbl != null;
      }
      catch
      { return false; }
    }

    public bool CanRead(string name)
    {
      bool canRead = CanRead(_adapter, _adapter.TableMappings.GetByDataSetTable(name).SourceTable);
      return canRead;
    }

    public static bool CanRead(DbDataAdapter adapter, string fullTableName)
    {
      adapter.SelectCommand.CommandText = "SELECT * FROM " + fullTableName;
      DbDataReader reader = adapter.SelectCommand.ExecuteReader(CommandBehavior.KeyInfo);
      if (reader == null)
      { return false; }
      try
      {
        reader.Read();
        return true;
      }
      catch
      { return false; }
    }

    public static DataTable GetSchema(DbDataAdapter adapter, string tableName)
    {
      DbCommand cmd = adapter.SelectCommand;
      cmd.CommandText = "SELECT * FROM " + tableName;
      DbDataReader reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
      DataTable tbl = reader.GetSchemaTable();
      return tbl;
    }

    public int Fill(DataTable table, int id)
    {
      if (table.PrimaryKey == null || table.PrimaryKey.Length != 1)
      {
        throw new InvalidOperationException(
        "Invalid number of columns in primary Key of table " + table.TableName);
      }
      string where = string.Format("{0} = {1}", table.PrimaryKey[0].ColumnName, id);
      return Fill("*", table, where, null);
    }

    public int Fill(DataTable table, string where)
    {
      return Fill("*", table, where, null);
    }

    public int Fill(DataTable table, string where, IGeometry intersect)
    {
      return Fill("*", table, where, intersect);
    }

    public int Fill(string fields, DataTable table, string where)
    {
      return Fill(fields, table, where, null);
    }

    public int Fill(string fields, DataTable table, string where, IGeometry intersect)
    {
      string tableName = GetSourceTableName(table);

      return Fill(_adapter, fields, table, tableName, where, intersect);
    }

    private string GetSourceTableName(DataTable table)
    {
      DataTableMapping mapping = _adapter.TableMappings.GetByDataSetTable(table.TableName);
      return mapping.SourceTable;
    }

    public static int Fill(DbDataAdapter adapter,
      string fields, DataTable table, string tableName,
      string where, IGeometry intersect)
    {
      DbCommand command = adapter.SelectCommand;
      BuildSelectCommand(command, fields, table, tableName, where, intersect);

      int n = adapter.Fill(table);

      command.Parameters.Clear();
      return n;
    }

    public static void BuildSelectCommand(DbCommand command,
      string fields, DataTable table, string tableName,
      string where, IGeometry intersect)
    {
      if (fields == null)
      { fields = "*"; }

      command.Parameters.Clear();

      StringBuilder select = new StringBuilder();
      select.AppendFormat("SELECT {0} FROM {1}", fields, tableName);

      if (intersect != null)
      {
        //TODO: was machen falls GetShapeColumn(table) == null ist??
        string si = string.Format("INTERSECT({0}, ?)", GetShapeColumn(table).ColumnName);

        if (string.IsNullOrEmpty(where) == false)
        {
          where = string.Format("({0}) AND {1}", where, si);
        }
        else
        {
          where = si;
        }
      }
      if (string.IsNullOrEmpty(where) == false)
      {
        select.AppendFormat(" WHERE {0}", where);
      }

      command.CommandText = select.ToString();
      if (intersect != null)
      {
        DbParameter param = command.CreateParameter();
        param.Value = intersect;
        command.Parameters.Add(param);
      }
    }

    public static DataColumn GetShapeColumn(DataTable table)
    {
      foreach (DataColumn column in table.Columns)
      {
        if (typeof(IGeometry).IsAssignableFrom(column.DataType))
        {
          return column;
        }
      }
      return null;
    }

    /// <summary>
    /// Add a shape column to the table(s)
    /// </summary>
    public void AddShapeCols(DataTable table)
    {
      AddShapeCols(_adapter, table, GetSourceTableName(table), false);
    }

    /// 
    public void AddShapeCols(DataTable table, bool asIGeometry)
    {
      AddShapeCols(_adapter, table, GetSourceTableName(table), asIGeometry);
    }
    public static void AddShapeCols(DbDataAdapter adapter, DataTable table,
      string fullTableName, bool asIGeometry)
    {
      DbCommand cmd = adapter.SelectCommand;
      cmd.CommandText = string.Format("SELECT * FROM {0}", fullTableName);
      DbDataReader reader = cmd.ExecuteReader();
      DataTable schema = reader.GetSchemaTable();
      reader.Dispose();
      foreach (DataColumn column in schema.Columns)
      {
        if (typeof(IGeometry).IsAssignableFrom(column.DataType))
        {
          Type t;
          if (asIGeometry)
          { t = typeof(IGeometry); }
          else
          { t = column.DataType; }
          if (!table.Columns.Contains(column.ColumnName))
          {
            table.Columns.Add(column.ColumnName, t);
          }
        }
      }
    }

    //[CLSCompliant(false)]
    //public ISpatialReference GetSpatialReference(string tableName)
    //{
    //  DbCommand cmd = _adapter.SelectCommand;
    //  string fullTableName = GetMapping(tableName);
    //  cmd.CommandText = string.Format("SELECT * FROM {0}", fullTableName);
    //  DbDataReader reader = cmd.ExecuteReader();
    //  DataTable schema = reader.GetSchemaTable();
    //  reader.Dispose();

    //  foreach (DataColumn column in schema.Columns)
    //  {
    //    if (typeof(IGeometry).IsAssignableFrom(column.DataType))
    //    {
    //      ISpatialReference sr = (ISpatialReference)column.ExtendedProperties[GdbUtils.SpatialReferenceProp];
    //      return sr;
    //    }
    //  }
    //  return null;
    //}

    #region UpdateDB

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
        foreach (DataTable table in tableList)
        {
          n += Delete(t, table, cascade) +
            UpdateInsert(t, table, cascade);
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
        if (cascade)
        {
          foreach (DataTable table in tableList)
          {
            ClearKey(table, DeleteDone);
            ClearKey(table, UpdateDone);
          }
        }
      }

      if (transaction == null)
      {
        foreach (DataTable table in tableList)
        { AcceptChanges(table, cascade); }
      }
      return n;
    }

    public static int Delete(DbDataAdapter adapter,
      DbTransaction transaction, DataTable table,
      string fullTableName)
    {
      int n;
      DataTable tblUpdate = table.GetChanges(DataRowState.Deleted);
      if (tblUpdate != null)
      {
        DbCommand cmd;

        cmd = adapter.SelectCommand.Connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = string.Format("DELETE FROM {0} WHERE {1}",
          fullTableName, Where(table));
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

      n += Delete(_adapter, t, table, GetSourceTableName(table));

      if (cascade)
      { table.ExtendedProperties.Add(DeleteDone, true); }
      return n;
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
        if (keyFix && keys.Contains(column))
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
      string fullTableName, CommandFormatMode formatMode)
    {
      int n;
      DataRow[] rowUpdateLst = table.Select("", "", DataViewRowState.ModifiedCurrent);
      if (rowUpdateLst.Length > 0)
      {
        DbCommand cmd;
        cmd = adapter.SelectCommand.Connection.CreateCommand();
        cmd.Transaction = transaction;

        cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}",
          fullTableName, UpdateFields(table, formatMode), Where(table));
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
      string fullTableName, CommandFormatMode formatMode)
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
          fullTableName, ValueFields(table, formatMode), ReturnFields(table, formatMode));
        AppendFieldParameters(cmd, table, formatMode);
        if ((formatMode & CommandFormatMode.Auto) == CommandFormatMode.Auto)
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

      string fullName = GetSourceTableName(table);
      n += Update(_adapter, transaction, table, fullName, _formatMode);
      n += Insert(_adapter, transaction, table, fullName, _formatMode);

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
        if (autoKeys && keys.Contains(column))
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
      bool autoKeys = (formatMode & CommandFormatMode.Auto) == CommandFormatMode.Auto;
      if (autoKeys == false)
      { return null; }

      bool first;
      StringBuilder keys = new StringBuilder();

      first = true;
      foreach (DataColumn column in table.PrimaryKey)
      {
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
        if (first == false)
        { keys.Append(", "); }
        else
        { keys.Append(" INTO "); }
        keys.Append("?");
        first = false;
      }

      return keys.ToString();
    }


    private static void ClearKey(DataTable table, string key)
    {
      if (table.ExtendedProperties.ContainsKey(key))
      { table.ExtendedProperties.Remove(key); }

      foreach (DataRelation rel in table.ChildRelations)
      { ClearKey(rel.ChildTable, key); }
    }

    public static void AcceptChanges(DataTable table, bool cascade)
    {
      if (table.GetChanges() != null)
      {
        table.AcceptChanges();
      }
      if (cascade)
      {
        foreach (DataRelation rel in table.ChildRelations)
        { AcceptChanges(rel.ChildTable, cascade); }
      }
    }

    #endregion
  }
}

