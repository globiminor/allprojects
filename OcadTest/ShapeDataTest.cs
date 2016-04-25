using System.Data;
using Ocad.Data;
using Shape;
using Shape.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OcadTest
{
  [TestClass]
  public class ShapeDataTest
  {
    [TestMethod]
    public void CanReadZM()
    {
      using (ShapeReader reader = new ShapeReader(@"H:\shp\53_59r.shp"))
      {
        reader.ReadShpHeader();
        foreach (DataRow row in reader)
        {
          
        }
      }

    }

    [TestMethod]
    public void CanReadData()
    {
      ShapeConnection conn = new ShapeConnection
        (@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\OL20110703");
      ShapeAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT * FROM routes";
      DataTable tbl = new DataTable();
      ada.Fill(tbl);
      Assert.AreEqual(7, tbl.Columns.Count);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }

    [TestMethod]
    public void CanReadAngle()
    {
      ShapeConnection conn = new ShapeConnection
        (@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\OL20110703");
      ShapeAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT Angle FROM routes";
      DataTable tbl = new DataTable();
      ada.Fill(tbl);
      Assert.AreEqual(1, tbl.Columns.Count);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }

  }
}
