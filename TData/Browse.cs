using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Basics.Data;
using Basics.Geom;

namespace TData
{
  public static class Browse
  {
    public static readonly string Arbeitsplatz = "Arbeitsplatz";

    public static List<string> GetParents(string startDir)
    {
      List<string> parents = new List<string>();
      string dir = startDir;
      while (string.IsNullOrEmpty(dir) == false)
      {
        string file = Path.GetFileName(dir);
        if (string.IsNullOrEmpty(file) == false)
        { parents.Insert(0, file); }
        else
        { parents.Insert(0, dir); }

        dir = Path.GetDirectoryName(dir);
      }

      parents.Insert(0, Arbeitsplatz);

      return parents;
    }
    public static List<DatasetInfo> GetObjects(string dir)
    {
      List<DatasetInfo> list = new List<DatasetInfo>();
      if (dir == null)
      { return null; }

      if (dir == "")
      {
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
          list.Add(new DatasetInfo(drive.Name, null, DataType.Folder));
        }
        return list;
      }

      if (Directory.Exists(dir))
      {
        GetDirectoryObjects(dir, list);
      }
      else
      {
        DbBaseConnection conn = TTable.GetDbConnection(dir);
        if (conn != null)
        {
          using (conn)
          {
            GetDbObjects(conn, list);
          }
        }
      }
      return list;
    }

    private static void GetDirectoryObjects(string dir, List<DatasetInfo> list)
    {
      List<string> subDirs = new List<string>(Directory.GetDirectories(dir));
      foreach (string subDir in subDirs)
      {
        list.Add(new DatasetInfo(subDir, null, DataType.Folder));
      }

      List<string> files = new List<string>(Directory.GetFiles(dir));
      files.Sort();

      foreach (string file in files)
      {
        string full = Path.Combine(dir, file);
        string ext = Path.GetExtension(file);
        if (ext == ".shp" && Shape.ShapeReader.Exists(full))
        {
          Shape.ShapeType shpType;
          using (Shape.ShpReader reader = new Shape.ShpReader(full))
          { shpType = reader.ShapeType; }

          if (shpType == Shape.ShapeType.Point || shpType == Shape.ShapeType.PointZ)
          { list.Add(new DatasetInfo(file, null, DataType.PointFc)); }
          else if (shpType == Shape.ShapeType.Line || shpType == Shape.ShapeType.LineZ)
          { list.Add(new DatasetInfo(file, null, DataType.LineFc)); }
          else if (shpType == Shape.ShapeType.Area || shpType == Shape.ShapeType.AreaZ)
          { list.Add(new DatasetInfo(file, null, DataType.PolyFc)); }
          else
          { list.Add(new DatasetInfo(file, null, DataType.UnknownFc)); }
        }
        else if (ext == ".ocd")
        {
          list.Add(new DatasetInfo(file, null, DataType.UnknownFc));
        }
        else if (ext == ".tif")
        {
          list.Add(new DatasetInfo(file, null, DataType.Raster));
        }
        else if (ext == ".sde")
        {
          list.Add(new DatasetInfo(file, null, DataType.FileGdb));
        }
        else if (ext == ".mdb")
        {
          list.Add(new DatasetInfo(file, null, DataType.FileGdb));
        }
        else
        {
          list.Add(new DatasetInfo(file, null, DataType.Unknown));
        }
      }
    }

    private static void GetDbObjects(DbBaseConnection conn, List<DatasetInfo> list)
    {
      using (new OpenConnection(conn))
      {
        // TODO : wo nur GetTableSchema implementiert ist, Methode GetSchema erweitern
        // DataTable tblCols = conn.GetTableSchema("tablesgeom");
        DataTable tblCols = conn.GetSchema("tablesgeom");
        DataView vwCols = new DataView(tblCols);
        vwCols.Sort = SchemaColumnsTable.TableNameColumn.Name;

        string preTbl = null;
        Dictionary<string, DataType> geometryType = new Dictionary<string, DataType>();
        foreach (DataRowView vTbl in vwCols)
        {
          DataRow rowTbl = vTbl.Row;
          string table = SchemaColumnsTable.TableNameColumn.GetValue(rowTbl,"");
          string column = SchemaColumnsTable.ColumnNameColumn.GetValue(rowTbl);
          Type columnType = SchemaColumnsTable.DataTypeColumn.GetValue(rowTbl);
          if (typeof(IGeometry).IsAssignableFrom(columnType))
          {
            DataType geomType = DataType.UnknownFc;
            if (typeof(IPoint).IsAssignableFrom(columnType))
            { geomType = DataType.PointFc; }
            else if (typeof(Polyline).IsAssignableFrom(columnType))
            { geomType = DataType.LineFc; }
            else if (typeof(Area).IsAssignableFrom(columnType))
            { geomType = DataType.PolyFc; }

            geometryType.Add(column, geomType);
          }
          if (table != preTbl)
          {
            if (preTbl != null)
            {
              if (geometryType.Count == 0)
              { list.Add(new DatasetInfo(preTbl, null, DataType.Table)); }
              else
              {
                foreach (KeyValuePair<string, DataType> pair in geometryType)
                {
                  string fullName;
                  if (geometryType.Count == 1)
                  { fullName = preTbl; }
                  else
                  { fullName = string.Format("{0},{1}", preTbl, pair.Key); }

                  list.Add(new DatasetInfo(fullName, null, pair.Value));
                }
              }
            }
            geometryType.Clear();
          }
          preTbl = table;
        }
      }
    }
  }

  public class DatasetInfo
  {
    private string _name;
    private object _dsName;
    private readonly DataType _dataType;

    public DatasetInfo(string name, object dsName, DataType dataType)
    {
      _name = name;
      _dsName = dsName;
      _dataType = dataType;
    }

    public string Name
    {
      get { return _name; }
    }
    public object DsName
    {
      get { return _dsName; }
    }

    public DataType DataType
    {
      get { return _dataType; }
    }
  }

}
