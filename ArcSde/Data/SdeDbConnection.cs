
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Basics.Data;
using Basics.Geom;

namespace ArcSde.Data
{
  [ToolboxItem(false)]
  public class SdeDbConnection : DbBaseConnection
  {
    public event CancelEventHandler StartingOperation;
    public event EventHandler CommittingOperation;
    public event EventHandler AbortingOperation;

    private SdeDbTransaction _trans;
    private SeConnection _connection;

    private static Dictionary<string, SeConnection> _connectionPool =
      new Dictionary<string, SeConnection>();
    private Dictionary<string, SchemaColumnsTable> _tableSchemas =
      new Dictionary<string, SchemaColumnsTable>();

    public SdeDbConnection(SeConnection connection)
    {
      _connection = connection;
    }
    public SdeDbConnection(string connectionString)
    {
      _connection = GetFreeConnection(connectionString);
    }

    private SeConnection GetFreeConnection(string connectionString)
    {
      if (_connectionPool.TryGetValue(connectionString, out SeConnection connection) == false)
      {
        connection = new SeConnection(connectionString);
        _connectionPool.Add(connectionString, connection);
      }
      return connection;
    }

    public SdeDbConnection(string server, string instance, string database, string user, string password)
    {
      _connection = new SeConnection(server, instance, database, user, password);
    }

    public SeConnection SeConnection
    {
      get { return _connection; }
    }
    public override void Close()
    { }
    public override void Open()
    { }

    public override string ConnectionString
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public override void ChangeDatabase(string databaseName)
    { throw new Exception("The method or operation is not implemented."); }
    public override string Database
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override string DataSource
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override string ServerVersion
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }
    public override ConnectionState State
    {
      get { return ConnectionState.Open; }
    }

    public override DataTable GetSchema(string collectionName)
    {
      if ("Tables".Equals(collectionName, StringComparison.InvariantCultureIgnoreCase))
      {
        return GetTables();
      }
      else if ("Layers".Equals(collectionName, StringComparison.InvariantCultureIgnoreCase))
      {
        return GetLayers();
      }
      else if ("TablesGeom".Equals(collectionName, StringComparison.InvariantCultureIgnoreCase))
      {
        SchemaTablesTable tables = GetTables();
        DataView tablesView = new DataView(tables);
        tablesView.Sort = SchemaTablesTable.TableNameColumn.Name;

        SchemaLayersTable layers = GetLayers();
        DataView layersView = new DataView(layers);
        layersView.Sort = SchemaLayersTable.ColTableName;

        SchemaColumnsTable colTbl = new SchemaColumnsTable();
        foreach (var vTbl in tablesView.Enum())
        {
          SchemaTablesTable.Row rowTbl = (SchemaTablesTable.Row)vTbl.Row;
          IList<DataRowView> rowsLyr = layersView.FindRows(rowTbl.TableName);
          if (rowsLyr == null || rowsLyr.Count == 0)
          {
            string table = rowTbl.TableName;
            AddRow(colTbl, table, "", typeof(object));
          }
          else
          {
            foreach (var vLyr in rowsLyr)
            {
              SchemaLayersTable.Row rowLyr = (SchemaLayersTable.Row)vLyr.Row;
              AddRow(colTbl, rowLyr.TableName, rowLyr.SpatialColumn, GetType(rowLyr));
            }
          }
        }
        return colTbl;
      }
      else
      { throw new NotImplementedException(); }
    }

    private void AddRow(SchemaColumnsTable colTbl, string table, string column, Type type)
    {
      SchemaColumnsTable.Row row = colTbl.NewRow();
      row.TableName = table;
      row.ColumnName = column;
      row.DataType = type;
      colTbl.AddRow(row);
    }

    private static Type GetType(SchemaLayersTable.Row lyr)
    {
      Type geom;
      if ((lyr.SpatialTypes & SdeType.SE_POINT_TYPE_MASK) == SdeType.SE_POINT_TYPE_MASK)
      { geom = typeof(IPoint); }
      else if ((lyr.SpatialTypes & SdeType.SE_LINE_TYPE_MASK) == SdeType.SE_LINE_TYPE_MASK)
      { geom = typeof(Polyline); }
      else if ((lyr.SpatialTypes & SdeType.SE_AREA_TYPE_MASK) == SdeType.SE_AREA_TYPE_MASK)
      { geom = typeof(Surface); }
      else
      { geom = typeof(IGeometry); }
      return geom;
    }

    private SchemaLayersTable GetLayers()
    {
      SchemaLayersTable tbl = new SchemaLayersTable();

      using (SeLayerInfoList layers = _connection.GetLayers())
      {
        foreach (var layer in layers.GetLayers())
        {
          SchemaLayersTable.Row row = tbl.NewRow();
          row.TableName = layer.Table;
          row.SpatialColumn = layer.SpatialColumn;
          row.SpatialTypes = layer.SpatialTypes;
          tbl.AddRow(row);
          row.AcceptChanges();
        }
      }
      return tbl;
    }

    private SchemaTablesTable GetTables()
    {
      SchemaTablesTable tbl = new SchemaTablesTable();
      IList<string> names = _connection.GetTables(SdeType.SE_SELECT_PRIVILEGE);
      foreach (var name in names)
      {
        SchemaTablesTable.Row row = tbl.NewRow();
        row.TableName = name;
        tbl.AddRow(row);
        row.AcceptChanges();
      }
      return tbl;
    }

    public override SchemaColumnsTable GetTableSchema(string name)
    {
      if (_tableSchemas.TryGetValue(name, out SchemaColumnsTable schema))
      {
        return schema;
      }

      schema = new SchemaColumnsTable();
      SeTable tbl = new SeTable(_connection, name);
      foreach (var column in tbl.Columns)
      {
        Type type;
        if (column.Type == SdeType.SE_SMALLINT_TYPE) type = typeof(Int16);
        else if (column.Type == SdeType.SE_INTEGER_TYPE) type = typeof(int);
        else if (column.Type == SdeType.SE_DOUBLE_TYPE) type = typeof(double);
        else if (column.Type == SdeType.SE_DATE_TYPE) type = typeof(DateTime);
        else if (column.Type == SdeType.SE_UUID_TYPE) type = typeof(Guid);
        else if (column.Type == SdeType.SE_SHAPE_TYPE) type = typeof(IGeometry);
        else if (column.Type == SdeType.SE_NSTRING_TYPE) type = typeof(string);
        else
        {
          throw new NotImplementedException("Unhandled sde column type " + column.Type);
        }
        schema.AddSchemaColumn(column.Name, type);
      }

      _tableSchemas.Add(name, schema);
      return schema;
    }

    public override IBox GetExtent(string name)
    {
      SeLayer lyr = new SeLayer(SeConnection, name);
      Se_Envelope seExt = lyr.LayerInfo.GetExtent();
      Box extent = new Box(new Point2D(seExt.minx, seExt.miny), new Point2D(seExt.maxx, seExt.maxy));
      return extent;
    }
    public new SdeDbTransaction BeginTransaction()
    {
      return (SdeDbTransaction)base.BeginTransaction();
    }
    public new SdeDbTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
      return (SdeDbTransaction)base.BeginTransaction(isolationLevel);
    }
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      _trans = SdeDbTransaction.Start(this, isolationLevel);
      return _trans;
    }

    public new SdeDbCommand CreateCommand()
    {
      return (SdeDbCommand)CreateDbCommand();
    }
    protected override DbCommand CreateDbCommand()
    {
      return new SdeDbCommand(this);
    }

    internal void OnStartingOperation(CancelEventArgs args)
    {
      StartingOperation?.Invoke(this, args);
    }

    internal void OnCommittingOperation(EventArgs args)
    {
      CommittingOperation?.Invoke(this, args);
    }

    internal void OnAbortingOperation(EventArgs args)
    {
      AbortingOperation?.Invoke(this, args);
    }

    public new SdeDbAdapter CreateAdapter()
    {
      return (SdeDbAdapter)base.CreateAdapter();
    }

    protected override DbBaseAdapter CreateDbAdapter()
    {
      SdeDbAdapter apt = new SdeDbAdapter(this);
      return apt;
    }

  }
}
