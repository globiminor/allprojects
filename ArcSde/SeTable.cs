using System;
using System.Runtime.InteropServices;

namespace ArcSde
{
  public class SeTable
  {
    #region static
    public static SeTable Create(SeConnection connection,
      string table,
      Se_Column_Def[] column_defs,
      string config_keyword)
    {
      ErrorHandling.CheckRC(connection.Conn, IntPtr.Zero,
          CApi.SE_table_create(connection.Conn, table,
          (Int16)column_defs.Length, column_defs, config_keyword));
      return new SeTable(connection, table);
    }

    public static SeTable CreateView(SeConnection connection,
       string view,
       string[] viewColumns,
       string[] tableColumns,
       Se_Sql_Construct sqlc)
    {
      Int16 nViewCols = (Int16)viewColumns.Length;
      Int16 nTblCols = (Int16)tableColumns.Length;

      IntPtr[] vCols = Api.GetPtrs(viewColumns);
      IntPtr[] tCols = Api.GetPtrs(tableColumns);

      int rc = CApi.SE_table_create_view(connection.Conn, view, nViewCols, nTblCols,
                                         vCols, tCols, ref sqlc);
      ErrorHandling.CheckRC(connection.Conn, IntPtr.Zero, rc);
      return new SeTable(connection, view);
    }
    #endregion

    private SeConnection _conn;
    private string _name;

    private SeColumn[] _columns;

    public SeTable(SeConnection conn, string name)
    {
      _conn = conn;
      _name = name;
    }

    protected SeConnection Connection
    { get { return _conn; } }
    public string Name
    { get { return _name; } }

    public SeColumn[] Columns
    {
      get
      {
        if (_columns == null)
        {
          Check(CApi.SE_table_get_column_list(_conn.Conn, _name, out IntPtr columnsPtr, out short nColumns));

          IntPtr[] columnsArray = new IntPtr[nColumns];
          Marshal.Copy(columnsPtr, columnsArray, 0, nColumns);
          SeColumn[] columns = new SeColumn[nColumns];
          for (int iCol = 0; iCol < nColumns; iCol++)
          {
            SeColumn def = new SeColumn(columnsArray[iCol]);
            columns[iCol] = def;
          }
          CApi.SE_table_free_column_list(nColumns, columnsPtr);

          _columns = columns;
        }
        return _columns;
      }
    }

    private SeColumn _geomCol;
    public SeColumn GeomCol
    {
      get
      {
        if (_geomCol == null)
        {
          foreach (SeColumn column in Columns)
          {
            if (column.Type == SdeType.SE_SHAPE_TYPE)
            {
              _geomCol = column;
              break;
            }
          }
        }
        return _geomCol;
      }
      set { _geomCol = value; }
    }

    public void Delete()
    {
      DeleteCore();
    }
    protected virtual void DeleteCore()
    {
      Check(CApi.SE_table_delete(_conn.Conn, _name));

      _conn = null;
      _name = null;
    }

    protected void Check(int rc)
    {
      ErrorHandling.CheckRC(_conn.Conn, IntPtr.Zero, rc);
    }

  }
}
