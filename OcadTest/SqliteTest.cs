
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace OcadTest
{
  [TestClass]
  public class SqliteTest
  {
    [TestMethod]
    public void CanInterpretSqlite()
    {
      //SQLiteConnection.CreateFile("MyDatabase.sqlite");
      Batteries.Init();

      string dbPath = @"C:\daten\felix\kapreolo\karten\wangenerwald\2020\NOM\routes.gpkg";
      SqliteConnection conn = new SqliteConnection($"Data Source={dbPath};");
      conn.Open();
      using (var cmd = conn.CreateCommand())
      {
        using (var ada = new SqliteDataAdapter(cmd))
        {
          ada.SelectCommand.CommandText = "SELECT * FROM sqlite_master";

          DataTable schema = new DataTable();
          ada.Fill(schema);
        }

        using (var ada = new SqliteDataAdapter(cmd))
        {
          ada.SelectCommand.CommandText = "SELECT * FROM gpkg_geometry_columns";

          DataTable schema = new DataTable();
          ada.Fill(schema);
        }


        using (var ada = new SqliteDataAdapter(cmd))
        {
          ada.SelectCommand.CommandText = "SELECT * FROM routes";
          DataTable schema = new DataTable();
          ada.Fill(schema);

          foreach (DataRow row in schema.Rows)
          {
            object geom = row["geom"];
            byte[] oGeom = (byte[])geom;

            using (System.IO.Stream s = new System.IO.MemoryStream(oGeom))
            using (System.IO.BinaryReader r = new EndianReader(s, isLittleEndian: true))
            {
              List<short> h = new List<short>();

              h.Add(r.ReadInt16());
              h.Add(r.ReadInt16());
              h.Add(r.ReadInt16());
              h.Add(r.ReadInt16());

              List<double> d = new List<double>();
              d.Add(r.ReadDouble());
              d.Add(r.ReadDouble());
              d.Add(r.ReadDouble());
              d.Add(r.ReadDouble());

              short u = r.ReadInt16();

              List<short> us = new List<short>();
              double zMin = r.ReadDouble();
              double zMax = r.ReadDouble();

              for (int i = 0; i < 6; i++)
              {
                us.Add(r.ReadInt16());
              }

              int nPoints = r.ReadInt32();
              List<double> x = new List<double>();
              List<double> y = new List<double>();
              List<double> z = new List<double>();
              for (int i = 0; i < nPoints; i++)
              {
                x.Add(r.ReadDouble());
                y.Add(r.ReadDouble());
                z.Add(r.ReadDouble());
              }

            }
          }
        }

      }
    }
  }

  public class SqliteDataAdapter : DbDataAdapter
  {
    public SqliteDataAdapter(SqliteCommand selectCommand)
    {
      base.SelectCommand = selectCommand;
    }
  }
  //  Point {
  //  double x;
  //  double y;
  //};
  //LinearRing
  //{
  //  uint32 numPoints;
  //  Point points[numPoints];
  //};
  //enum wkbGeometryType
  //{
  //  wkbPoint = 1,
  //  wkbLineString = 2,
  //  wkbPolygon = 3,
  //  wkbMultiPoint = 4,
  //  wkbMultiLineString = 5,
  //  wkbMultiPolygon = 6
  //};
  //enum wkbByteOrder
  //{
  //  wkbXDR = 0,     // Big Endian
  //  wkbNDR = 1     // Little Endian
  //};
  //WKBPoint
  //{
  //  byte byteOrder;
  //  uint32 wkbType;     // 1=wkbPoint
  //  Point point;
  //};
  //WKBLineString
  //{
  //  byte byteOrder;
  //  uint32 wkbType;     // 2=wkbLineString
  //  uint32 numPoints;
  //  Point points[numPoints];
  //};

  //WKBPolygon
  //{
  //  byte byteOrder;
  //  uint32 wkbType;     // 3=wkbPolygon
  //  uint32 numRings;
  //  LinearRing rings[numRings];
  //};
  //WKBMultiPoint
  //{
  //  byte byteOrder;
  //  uint32 wkbType;     // 4=wkbMultipoint
  //  uint32 num_wkbPoints;
  //  WKBPoint WKBPoints[num_wkbPoints];
  //};
  //WKBMultiLineString
  //{
  //  byte byteOrder;
  //  uint32 wkbType;     // 5=wkbMultiLineString
  //  uint32 num_wkbLineStrings;
  //  WKBLineString WKBLineStrings[num_wkbLineStrings];
  //};

  //wkbMultiPolygon
  //{
  //  byte byteOrder;
  //  uint32 wkbType;     // 6=wkbMultiPolygon
  //  uint32 num_wkbPolygons;
  //  WKBPolygon wkbPolygons[num_wkbPolygons];
  //};

  //WKBGeometry
  //{
  //  union {
  //    WKBPoint point;
  //    WKBLineString linestring;
  //    WKBPolygon polygon;
  //    WKBMultiPoint mpoint;
  //    WKBMultiLineString mlinestring;
  //    WKBMultiPolygon mpolygon;
  //  }
  //};
}
