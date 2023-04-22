using System;
using System.Data;
using System.Data.Common;
using ArcSde;
using ArcSde.Data;
using Basics.Data;
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OcadTest
{
  [TestClass]
  public class SdeDataTest
  {
    [TestMethod]
    public void CanConnectAccess()
    {
      string c =
        @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:\daten\kapreolo\Kapreolo_Adressliste.mdb";
      System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(c);
      conn.Open();
      DataTable metaData = conn.GetSchema();

      DataTable allTables = conn.GetSchema("Tables");

      Assert.IsNotNull(metaData);
      Assert.IsNotNull(allTables);
    }
    [TestMethod]
    public void CanReadObjectID()
    {
      SeConnection c = GetUtahConnection();

      SdeDbConnection conn = new SdeDbConnection(c);

      DataTable tables = conn.GetSchema("Tables");
      string firstTable = ((SchemaTablesTable.Row)tables.Rows[0]).TableName;

      SdeDbAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT OBJECTID FROM " + firstTable;
      DataTable tbl = new DataTable();
      ada.Fill(tbl);
      Assert.AreEqual(1, tbl.Columns.Count);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }

    [TestMethod]
    public void CanReadStar()
    {
      SeConnection c = GetUtahConnection();

      SdeDbConnection conn = new SdeDbConnection(c);

      DataTable tables = conn.GetSchema("Tables");
      string firstTable = ((SchemaTablesTable.Row)tables.Rows[0]).TableName;

      SdeDbAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT * FROM " + firstTable;
      DataTable tbl = new DataTable();
      ada.Fill(tbl);
      Assert.IsTrue(tbl.Columns.Count > 0);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }

    [TestMethod]
    public void CanGetExtent()
    {
      SeConnection c = GetUtahConnection();

      SdeDbConnection conn = new SdeDbConnection(c);

      DataTable tables = conn.GetSchema("Tables");
      string firstTable = ((SchemaTablesTable.Row)tables.Rows[0]).TableName;

      IBox box = conn.GetExtent(firstTable);
      Assert.IsNotNull(box);
    }

    [TestMethod]
    public void CanSelectShape()
    {
      SeConnection c = GetUtahConnection();

      SdeDbConnection conn = new SdeDbConnection(c);

      DataTable tables = conn.GetSchema("Tables");
      string firstTable = ((SchemaTablesTable.Row)tables.Rows[0]).TableName;
      IBox box = conn.GetExtent(firstTable);

      SdeDbAdapter apt = conn.CreateAdapter();
      SdeDbCommand cmd = apt.SelectCommand;
      cmd.CommandText = string.Format("SELECT ObjectId FROM {0} WHERE ST_Intersects(Shape, :p1)", firstTable);
      DbParameter param = cmd.CreateParameter();
      param.ParameterName = "p1";
      param.Value = new Surface(new[] { Polyline.Create(new[] { box.Min, box.Max, new Point2D(box.Max.X, box.Min.Y), box.Min }) });

      cmd.Parameters.Add(param);

      DataTable tbl = new DataTable();
      apt.Fill(tbl);
      Assert.AreEqual(1, tbl.Columns.Count);

      SdeDbAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT * FROM " + firstTable;
      DataTable tblFull = new DataTable();
      ada.Fill(tblFull);
      Assert.IsTrue(tbl.Rows.Count < tblFull.Rows.Count);
    }

    private SeConnection GetUtahConnection()
    {
      SetSdePath(@"C:\Program Files (x86)\ArcGIS\Bin");
      return new SeConnection("gdb93.agrc.utah.gov", "5151", "SGID93", "agrc", "agrc");
    }

    private void SetSdePath(string sdePath)
    {
      string path = Environment.GetEnvironmentVariable("PATH");
      if (path != null && !path.Contains(sdePath))
      {
        Environment.SetEnvironmentVariable("PATH", string.Format("{0};{1}", path, sdePath));
      }
    }

  }
}
