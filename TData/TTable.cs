using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Basics.Data;
using Basics.Geom;
using Shape.Data;

namespace TData
{
  public class TTable : Data
  {
    private readonly string _tableName;
    private readonly TTableConnection _connection;

    private DataTable _fullSchema;
    private DataColumn _defaultGeomCol = null;
    private DataView _edits;
    private string _where;

    public TTable(string tableName, DbBaseConnection connection)
    {
      _tableName = tableName;
      _connection = new TTableConnection(this, connection);
      Path = System.IO.Path.Combine(connection.ConnectionString, tableName);
    }
    public string Where
    {
      get { return _where; }
      set { _where = value; }
    }

    public DataView Edits
    {
      get { return _edits; }
    }

    public void SaveEdits()
    {
      if (_edits == null)
      { return; }

      DataTable copy = _edits.Table.Copy();
      copy.TableName = _tableName;
      foreach (var row in copy.Rows.Enum())
      {
        if (row.RowState == DataRowState.Unchanged)
        { row.Delete(); }
      }
      TTableAdapter adapter = new TTableAdapter(_connection);
      Updater updater = new Updater(adapter,
        Updater.CommandFormatMode.Auto | Updater.CommandFormatMode.KeyVariable);
      updater.UpdateDB(null, copy);

      _edits.Dispose();
      _edits = null;
    }

    public DbBaseConnection Connection
    {
      get { return _connection; }
    }
    public override IBox Extent
    {
      get
      {
        using (new OpenConnection(_connection))
        {
          IBox extent = _connection.GetExtent(_tableName);
          return extent;
        }
      }
    }

    private static void SetSdePath()
    {
      SetSdePath(@"C:\Program Files (x86)\ArcGIS\Bin");
    }
    private static void SetSdePath(string sdePath)
    {
      string path = Environment.GetEnvironmentVariable("PATH");
      if (path != null && !path.Contains(sdePath))
      {
        Environment.SetEnvironmentVariable("PATH", string.Format("{0};{1}", path, sdePath));
      }
    }

    public static DbBaseConnection GetDbConnection(string path)
    {
      DbBaseConnection conn = null;
      if (File.Exists(path) && System.IO.Path.GetExtension(path) == ".sde")
      {
        SetSdePath();
        string connString;
        using (TextReader reader = new StreamReader(path))
        {
          connString = reader.ReadLine();
        }
        conn = new ArcSde.Data.SdeDbConnection(connString);
      }
      else if (File.Exists(path) && System.IO.Path.GetExtension(path) == ".ocd")
      {
        conn = new Ocad.Data.OcadConnection(path);
      }
      else if (File.Exists(path) && System.IO.Path.GetExtension(path) == ".mdb")
      {
        conn = new AccessConnection(path);
      }
      else if (Directory.Exists(path))
      {
        conn = new ShapeConnection(path);
      }
      return conn;
    }

    public static TTable FromFile(string filePath)
    {
      string dir = System.IO.Path.GetDirectoryName(filePath);
      DbBaseConnection conn = GetDbConnection(dir);
      if (conn == null)
      { return null; }

      string tblName = System.IO.Path.GetFileName(filePath);
      if (conn.GetTableSchema(tblName) == null)
      { return null; }
      TTable tbl = new TTable(tblName, conn);
      return tbl;
    }

    public static TTable FromData(object data)
    {
      if (data is Mesh)
      {
        throw new NotImplementedException();
        //return new MeshTable((Mesh)data);
      }
      return null;
    }

    public DataColumn DefaultGeometryColumn
    {
      get
      {
        if (_defaultGeomCol == null)
        {
          foreach (var col in FullSchema.Columns.Enum())
          {
            if (typeof(IGeometry).IsAssignableFrom(col.DataType))
            {
              _defaultGeomCol = col;
              break;
            }
          }
        }
        return _defaultGeomCol;
      }
    }

    public DataTable FullSchema
    {
      get
      {
        if (_fullSchema == null)
        {
          using (new OpenConnection(_connection))
          {
            IDbCommand cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM " + _tableName;
            IDataReader reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
            _fullSchema = DbBaseReader.GetSchema(reader);
          }
        }
        return _fullSchema;
      }
    }

    public DbBaseCommand GetSelectCmd(GeometryQuery query, string fields)
    {
      if (fields == null)
      { fields = "*"; }
      else
      {
        Dictionary<string, string> fieldsDict = new Dictionary<string, string>();
        foreach (var field in fields.Split(','))
        {
          string f = field.Trim();
          fieldsDict.Add(f.ToUpper(), f);
        }
        if (!string.IsNullOrWhiteSpace(_where))
        {
          string upperWhere = _where.ToUpper();
          foreach (var col in FullSchema.Columns.Enum())
          {
            string upperCol = col.ColumnName.ToUpper();
            if (!upperWhere.Contains(upperCol))
            { continue; }
            if (fieldsDict.ContainsKey(upperCol))
            { continue; }

            fieldsDict.Add(upperCol, upperCol);
          }

        }
        if (FullSchema.PrimaryKey != null && FullSchema.PrimaryKey.Length == 1)
        {
          string keyField = FullSchema.PrimaryKey[0].ColumnName;
          if (!fieldsDict.ContainsKey(keyField.ToUpper()))
          { fieldsDict.Add(keyField.ToUpper(), keyField); }
        }
        StringBuilder fieldsBuilder = new StringBuilder();
        foreach (var field in fieldsDict.Values)
        {
          if (fieldsBuilder.Length > 0)
          { fieldsBuilder.Append(", "); }
          fieldsBuilder.Append(field);
        }
        fields = fieldsBuilder.ToString();
      }

      List<DbBaseParameter> paramList = new List<DbBaseParameter>();
      DbBaseCommand cmd = _connection.CreateCommand();
      StringBuilder stmt = new StringBuilder();
      stmt.AppendFormat("SELECT {0} FROM {1} ", fields, _tableName);
      if (!string.IsNullOrWhiteSpace(_where))
      { stmt.AppendFormat(" WHERE {0} ", _where); }

      if (query != null)
      {
        if (string.IsNullOrWhiteSpace(_where))
        { stmt.Append(" WHERE "); }
        else
        { stmt.Append(" AND "); }

        if (query.Relation == Relation.Intersect)
        {
          stmt.AppendFormat(" ST_Intersects({0}, :p1)", query.Column.ColumnName);
          DbBaseParameter param = cmd.CreateParameter();
          param.ParameterName = "p1";
          param.Value = query.Geometry;
          paramList.Add(param);
        }
        else
        {
          throw new NotImplementedException("Unhandled relation " + query.Relation);
        }
      }
      cmd.CommandText = stmt.ToString();
      foreach (var param in paramList)
      { cmd.Parameters.Add(param); }
      return cmd;
    }

    private DataRow GetRow(DataRow row)
    {
      if (_edits == null)
      {
        _edits = new DataView(FullSchema.Clone());
        _edits.Sort = _edits.Table.PrimaryKey[0].ColumnName;
      }
      object key = row[_edits.Table.PrimaryKey[0].ColumnName];
      int id = _edits.Find(key);
      DataRow editRow;
      if (id < 0)
      {
        editRow = _edits.Table.NewRow();
        CopyValues(row, editRow);
      }
      else
      {
        editRow = _edits[id].Row;
      }
      return editRow;
    }
    private void CopyValues(DataRow row, DataRow editRow)
    {
      foreach (var column in _edits.Table.Columns.Enum())
      { editRow[column] = row[column.ColumnName]; }
    }

    public void Delete(DataRow row)
    {
      DataRow delRow = GetRow(row);
      if (delRow.RowState == DataRowState.Detached)
      {
        _edits.Table.Rows.Add(delRow);
        delRow.AcceptChanges();
      }
      else if (delRow.RowState == DataRowState.Modified)
      {
        delRow.AcceptChanges();
      }
      else if (delRow.RowState == DataRowState.Added)
      {
        delRow.Delete();
      }
    }
    public void Update(DataRow row)
    {
      DataRow editRow = GetRow(row);
      if (editRow.RowState == DataRowState.Detached)
      {
        _edits.Table.Rows.Add(editRow);
        editRow.AcceptChanges();
        editRow.SetModified();
      }
      else if (editRow.RowState == DataRowState.Modified)
      {
        CopyValues(row, editRow);
      }
      else if (editRow.RowState == DataRowState.Added)
      {
        CopyValues(row, editRow);
      }
    }

    public void Insert(DataRow row)
    {
      DataRow editRow = GetRow(row);
      if (editRow.RowState == DataRowState.Detached)
      { _edits.Table.Rows.Add(editRow); }
      else
      { throw new InvalidOperationException("Row already exists"); }
    }

  }

  /// <summary>
  /// Summary description for Table.
  /// </summary>
  public abstract class TTableX : Data, IEnumerable<DataRow>
  {
    protected DataTable _table;
    protected IList<string> _whereList;
    protected IList<GeometryQuery> _queryList;

    protected TTableX()
    {
      _table = new DataTable();
      _whereList = new List<string>();
      _queryList = new List<GeometryQuery>();
    }

    public abstract DataColumnCollection Schema { get; }
    public abstract TTableX Select(string where);
    public abstract TTableX Select(GeometryQuery query);
    System.Collections.IEnumerator IEnumerable.GetEnumerator()
    { return GetEnumerator(); }
    public abstract IEnumerator<DataRow> GetEnumerator();
    public abstract int Topology { get; }
    public abstract DataColumn DefaultGeometryColumn { get; }

    public bool CheckConditions(DataRow row)
    {
      IGeometry p = null;
      foreach (var geomQry in _queryList)
      {
        if (p == null)
        { p = row[geomQry.Column.ColumnName] as IGeometry; }
        if (p == null)
        { return false; }
        IGeometry qGeom = geomQry.Geometry;
        if (qGeom == null)
        { continue; }

        if (GeometryOperator.Intersects(qGeom, p) == false)
        { return false; }
      }
      if (_whereList.Count > 0)
      {
        object[] items = row.ItemArray;
        bool success = true;
        DataView pView = new DataView(_table);
        _table.Rows.Add(row);
        foreach (var sWhere in _whereList)
        {
          pView.RowFilter = sWhere;
          if (pView.Count == 0)
          {
            success = false;
            break;
          }
        }
        _table.Clear();
        row.ItemArray = items;
        if (success == false)
        { return false; }
      }
      return true;
    }

    public static TTableX FromFile(string filePath)
    {
      TTableX data;
      if (System.IO.Path.GetExtension(filePath) == ".shp" &&
        Shape.ShapeReader.Exists(filePath))
      {
        Shape.ShapeReader shp = new Shape.ShapeReader(filePath);
        data = new ShapeReader(shp);
        data.Path = filePath;
        return data;
      }
      else if (Shape.ShpReader.Exists(filePath))
      {
        Shape.ShpReader shp = new Shape.ShpReader(filePath);
        data = new ShpReader(shp);
        data.Path = filePath;
        return data;
      }
      else if (System.IO.Path.GetExtension(filePath) == ".ocd" &&
        Ocad.OcadReader.Exists(filePath))
      {
        Ocad.OcadReader ocd = Ocad.OcadReader.Open(filePath);
        data = new OcadReader(ocd);
        data.Path = filePath;
        return data;
      }
      return null;
    }

    public static TTable FromData(object data)
    {
      if (data is Mesh)
      {
        return new TTable("Mesh", new SimpleConnection<Mesh.MeshLine>(new MeshSimpleData((Mesh)data)));
      }
      return null;
    }
  }

  internal class ShpReader : TTableX
  {
    #region nested classes
    private class Enumerator : IEnumerator<DataRow>
    {
      IEnumerator<IGeometry> _enumerator;
      ShpReader _schema;
      DataRow _currentRow;

      public Enumerator(ShpReader schema, Shape.ShpReader shape)
      {
        _schema = schema;
        _enumerator = shape.GetEnumerator();
      }

      #region IEnumerator Members

      public void Dispose()
      { }

      public void Reset()
      {
        _enumerator.Reset();
        _currentRow = null;
      }

      object IEnumerator.Current
      { get { return Current; } }
      public DataRow Current
      {
        get
        { return _currentRow; }
      }

      public bool MoveNext()
      {
        bool value = _enumerator.MoveNext();
        bool bConditions = false;
        _currentRow = _schema._table.NewRow();
        while (value && bConditions == false)
        {
          IGeometry geom = _enumerator.Current;
          _currentRow[0] = geom;
          bConditions = _schema.CheckConditions(_currentRow);

          if (bConditions == false)
          { value = _enumerator.MoveNext(); }
        }
        return value;
      }

      #endregion
    }
    #endregion

    private Shape.ShpReader _shape;

    public ShpReader(Shape.ShpReader shape)
    {
      _shape = shape;
      _table.Columns.Add("Shape", typeof(IGeometry));
    }
    #region TTable Members

    public override DataColumnCollection Schema
    {
      get
      { return _table.Columns; }
    }

    public override TTableX Select(string select)
    {
      return this; // TODO
    }
    public override TTableX Select(GeometryQuery query)
    {
      ShpReader pReader = new ShpReader(_shape);
      // TODO
      //      pReader.mWhereList = this.mWhereList.Clone();
      //      pReader.mQueryList = this.mQueryList.Clone();
      pReader._queryList.Add(query);
      return pReader;
    }

    public override IBox Extent
    {
      get
      { return _shape.Extent; }
    }

    public override int Topology
    {
      get
      { return _shape.Topology; }
    }

    public override DataColumn DefaultGeometryColumn
    {
      get
      { return _table.Columns[0]; }
    }


    #endregion

    #region IEnumerable Members

    public override IEnumerator<DataRow> GetEnumerator()
    {
      return new Enumerator(this, _shape);
    }

    #endregion

  }


  internal class ShapeReader : TTableX
  {
    #region nested classes
    private class Enumerator : IEnumerator<DataRow>
    {
      IEnumerator<DataRow> _enumerator;
      ShapeReader _schema;
      DataRow _currentRow;

      public Enumerator(ShapeReader schema, Shape.ShapeReader shape)
      {
        _schema = schema;
        _enumerator = shape.GetEnumerator();
      }

      #region IEnumerator Members

      public void Dispose()
      { }

      public void Reset()
      {
        _enumerator.Reset();
        _currentRow = null;
      }

      object IEnumerator.Current
      { get { return Current; } }
      public DataRow Current
      {
        get { return _currentRow; }
      }

      public bool MoveNext()
      {
        bool value = _enumerator.MoveNext();
        bool bConditions = false;
        _currentRow = _schema._table.NewRow();
        while (value && bConditions == false)
        {
          _currentRow = _enumerator.Current;

          bConditions = _schema.CheckConditions(_currentRow);
          if (bConditions == false)
          { value = _enumerator.MoveNext(); }
        }
        return value;
      }

      #endregion
    }
    #endregion

    private Shape.ShapeReader _shape;

    public ShapeReader(Shape.ShapeReader shape)
    {
      _shape = shape;
      _table.Columns.Add("Shape", typeof(IGeometry));
    }
    #region TTable Members

    public override DataColumnCollection Schema
    {
      get
      { return _table.Columns; }
    }

    public override TTableX Select(string select)
    {
      return this; // TODO
    }
    public override TTableX Select(GeometryQuery query)
    {
      ShapeReader pReader = new ShapeReader(_shape);
      // TODO
      //      pReader.mWhereList = this.mWhereList.Clone();
      //      pReader.mQueryList = this.mQueryList.Clone();
      pReader._queryList.Add(query);
      return pReader;
    }

    public override IBox Extent
    {
      get
      { return _shape.Extent; }
    }

    public override int Topology
    {
      get
      { return _shape.Topology; }
    }

    public override DataColumn DefaultGeometryColumn
    {
      get
      { return _table.Columns[0]; }
    }


    #endregion

    #region IEnumerable Members

    public override IEnumerator<DataRow> GetEnumerator()
    {
      return new Enumerator(this, _shape);
    }

    #endregion

  }


  internal class OcadReader : TTableX
  {
    #region nested classes
    private class Enumerator : IEnumerator<DataRow>
    {
      private Ocad.OcadReader.ElementEnumerator _enumerator;
      private OcadReader _schema;
      private DataRow _currentRow;

      public Enumerator(OcadReader schema, Ocad.OcadReader ocad)
      {
        Box box = null;
        foreach (var pQuery in schema._queryList)
        {
          if (pQuery.Relation == Relation.Intersect ||
            pQuery.Relation == Relation.ExtentIntersect)
          {
            if (box == null)
            { box = new Box(pQuery.Geometry.Extent); }
            else
            { box.Include(pQuery.Geometry.Extent); }
          }
        }

        _enumerator = new Ocad.OcadReader.ElementEnumerator(ocad,
          box, true, schema.IndexList);
        _schema = schema;
      }

      #region IEnumerator Members

      public void Dispose()
      { }

      public void Reset()
      {
        _enumerator.Reset();
        _currentRow = null;
      }

      object IEnumerator.Current
      { get { return Current; } }
      public DataRow Current
      {
        get { return _currentRow; }
      }

      public bool MoveNext()
      {
        bool value = _enumerator.MoveNext();
        bool bConditions = false;
        _currentRow = _schema._table.NewRow();
        while (value && bConditions == false)
        {
          if (_enumerator.Current.Geometry != null)
          { _currentRow[_fieldShape] = _enumerator.Current.Geometry; }
          else
          { _currentRow[_fieldShape] = DBNull.Value; }

          _currentRow[_fieldSymbol] = _enumerator.Current.Symbol;
          if (_enumerator.Current.Geometry is Point)
          { _currentRow[_fieldAngle] = _enumerator.Current.Angle; }

          bConditions = _schema.CheckConditions(_currentRow);

          if (bConditions == false)
          { value = _enumerator.MoveNext(); }
        }
        return value;
      }

      #endregion
    }
    #endregion

    private Ocad.OcadReader _ocad;
    private readonly Ocad.Setup _setup;
    private IList<Ocad.ElementIndex> _indexList;
    private const string _fieldShape = "Shape";
    private const string _fieldSymbol = "Symbol";
    private const string _fieldAngle = "Angle";

    public OcadReader(Ocad.OcadReader ocad)
    {
      _ocad = ocad;
      _setup = _ocad.ReadSetup();

      _table.Columns.Add(_fieldShape, typeof(IGeometry));
      _table.Columns.Add(_fieldSymbol, typeof(int));
      _table.Columns.Add(_fieldAngle, typeof(double));
    }
    #region TTable Members

    public override DataColumnCollection Schema
    {
      get
      { return _table.Columns; }
    }

    public override TTableX Select(string select)
    {
      if (select == null || select == string.Empty)
      { return this; }

      OcadReader reader = new OcadReader(_ocad);
      reader._indexList = IndexList;

      reader._whereList.Add(select);
      return reader;
    }
    public override TTableX Select(GeometryQuery query)
    {
      if (query == null)
      { return this; }

      OcadReader pReader = new OcadReader(_ocad);
      pReader._indexList = IndexList;

      // TODO
      //      pReader.mWhereList = this.mWhereList.Clone();
      //      pReader.mQueryList = this.mQueryList.Clone();
      pReader._queryList.Add(query);
      return pReader;
    }

    public override IBox Extent
    {
      get
      {
        Box extent = null;
        foreach (var elem in _ocad.Elements(true, IndexList))
        {
          if (extent == null)
          { extent = new Box(elem.Geometry.Extent); }
          else
          { extent.Include(elem.Geometry.Extent); }
        }
        return extent;
      }
    }

    public IList<Ocad.ElementIndex> IndexList
    {
      get
      {
        if (_indexList == null)
        {
          Ocad.ElementIndex pIndex;
          _indexList = new List<Ocad.ElementIndex>();
          int iElem = 0;
          do
          {
            pIndex = _ocad.ReadIndex(iElem);
            if (pIndex != null)
            { _indexList.Add(pIndex); }
            iElem++;
          } while (pIndex != null);
        }
        return _indexList;
      }
    }
    public override int Topology
    {
      get
      { return -1; }
    }

    public override DataColumn DefaultGeometryColumn
    {
      get
      { return _table.Columns[0]; }
    }


    #endregion

    #region IEnumerable Members

    public override IEnumerator<DataRow> GetEnumerator()
    {
      return new Enumerator(this, _ocad);
    }

    #endregion
  }
}
