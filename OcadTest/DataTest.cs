using Basics.Geom;
using Dhm;
using Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using Ocad.Data;
using Shape;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace OcadTest
{
  [TestClass]
  public class DataTest
  {
    [TestMethod]
    public void TestBildObjekte()
    {
      using (OcadReader r = OcadReader.Open(@"C:\daten\felix\kapreolo\scool\regensdorf_ruggenacher\test.ocd"))
      {
        foreach (ElementIndex idx in r.GetIndices())
        {
          ElementV9 e = (ElementV9)r.ReadElement(idx);
          if (e == null)
          { continue; }
          byte[] color = BitConverter.GetBytes(e.Color);
        }
      }
    }

    [TestMethod]
    public void TestLasShp()
    {
      int n = 0;
      List<Point3D> pts = new List<Point3D>(15100100);
      using (ShpReader r = new ShpReader(@"C:\daten\felix\kapreolo\karten\hardwald\2017\26870_12535"))
      {
        foreach (Point3D p in r)
        {
          //pts.Add(p);
          n++;
        }
      }
      Console.WriteLine(n);
    }

    [TestMethod]
    public void TestLasTxt()
    {
      double resolution = 1;
      string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (string path in Directory.EnumerateFiles(dir))
      {
        string key = Path.GetFileNameWithoutExtension(path);
        tiles[key] = path;
      }
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Grid.Common.InitColors(r, g, b);
      r[0] = 255;
      g[0] = 255;
      b[0] = 255;

      foreach (string key in tiles.Keys)
      {
        Export(dir, key, $"obstr{key}", resolution, LasUtils.Obstruction, r, g, b);
      }
    }

    [TestMethod]
    public void TestLasVege()
    {
      double resolution = 1;
      string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (string path in Directory.EnumerateFiles(dir))
      {
        string key = Path.GetFileNameWithoutExtension(path);
        tiles[key] = path;
      }
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Grid.Common.InitColors(r, g, b);
      r[0] = 255;
      g[0] = 255;
      b[0] = 255;
      {
        string key = "26880_12545";
        Export(dir, key, $"vege{key}", resolution, LasUtils.VegeHeight, r, g, b);
      }
    }

    [TestMethod]
    public void TestStructScript()
    {
      //double resolution = 1;
      // string dir = @"C:\daten\felix\kapreolo\karten\blauen\2018\lidar";
      string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (string path in Directory.EnumerateFiles(dir))
      {
        if (!path.EndsWith(".tif"))
        { continue; }
        Console.WriteLine("  <BackgroundMap.Open>");
        Console.WriteLine($"    <FileName>{path}</FileName>");
        Console.WriteLine("  </BackgroundMap.Open>");
      }
    }

    [TestMethod]
    public void TestLasStruct()
    {
      double resolution = 1;
      string dir = @"C:\daten\felix\kapreolo\karten\blauen\2018\lidar";
      //string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (string path in Directory.EnumerateFiles(dir))
      {
        string key = Path.GetFileNameWithoutExtension(path);
        if (!char.IsDigit(key[0]))
        { continue; }
        tiles[key] = path;
      }
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Grid.Common.InitColors(r, g, b);
      LasUtils.InitStructColors(r, g, b);
      foreach (string key in tiles.Keys)
      {
        //string key = "26880_12545";
        Export(dir, key, $"strc{key}", resolution, LasUtils.Struct, r, g, b);
      }
    }

    [TestMethod]
    public void TestLasDom()
    {
      double resolution = 1;
      string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (string path in Directory.EnumerateFiles(dir))
      {
        string key = Path.GetFileNameWithoutExtension(path);
        if (!char.IsDigit(key[0]))
        { continue; }
        tiles[key] = path;
      }
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      for (int i = 0; i < 256; i++)
      {
        byte bb = (byte)(255 - i);
        r[i] = bb; g[i] = bb; b[i] = bb; 
      }
      foreach (string key in tiles.Keys)
      {
        //string key = "26880_12545"; 
        Export(dir, key, $"dom{key}", resolution, LasUtils.Dom, r, g, b);
      }
    }

    private void Export(string dir, string key, string resName, double resolution,
      Func<int, int, Func<int, int, List<Vector>>, double> grdFct,
       byte[] r, byte[] g, byte[] b)
    {
      string lazName = Path.Combine(dir, key + ".laz");
      string tifPath = Path.Combine(dir, $"{resName}.tif");

      if (File.Exists(tifPath))
      { return; }

      if (File.Exists(Path.Combine(dir, $"{key}.tif")))
      {
        File.Move(Path.Combine(dir, $"{key}.tif"), tifPath);
        File.Move(Path.Combine(dir, $"{key}.tfw"), Path.Combine(dir, $"{resName}.tfw"));
        return;
      }

      if (!File.Exists(Path.ChangeExtension(lazName, ".txt")))
      {
        if (!File.Exists(lazName))
        {
          if (File.Exists(Path.ChangeExtension(lazName, ".html")))
          { File.Move(Path.ChangeExtension(lazName, ".html"), lazName); }
        }
        if (!File.Exists(lazName))
        { return; }

        Process p = new Process
        {
          StartInfo = new ProcessStartInfo
          {
            FileName = @"C:\daten\felix\src\temp\LAStools\bin\las2txt.exe",
            Arguments = $"-i {lazName} -parse xyzi"
          }
        };
        p.Start();
        p.WaitForExit();
      }

      DoubleGrid grd;
      using (TextReader reader = new StreamReader(Path.ChangeExtension(lazName, ".txt")))
      {
        grd = LasUtils.CreateGrid(reader, resolution, grdFct);
      }

      ImageGrid.GridToTif(grd.ToIntGrid(), tifPath, r, g, b);
    }

    [TestMethod]
    public void TestWriteBildObjekte()
    {
      using (OcadWriter w = Ocad9Writer.AppendTo(@"C:\daten\felix\kapreolo\scool\regensdorf_ruggenacher\test.ocd"))
      {
        {
          ElementV9 elem = new ElementV9(true)
          {
            Type = GeomType.unformattedText,
            Color = Color.ColorToCmyk(System.Drawing.Color.Blue).ToNumber(),
            Geometry = new Point2D(677000, 254000),
            Text = "A",
            Symbol = -3
          };

          w.Append(elem);
        }

        {
          ElementV9 elem = new ElementV9(true)
          {
            Type = GeomType.line,
            Color = Color.ColorToCmyk(System.Drawing.Color.Green).ToNumber(),
            LineWidth = 30,
            Geometry = Polyline.Create(new[] { new Point2D(677010, 254000), new Point2D(677020, 254010) }),
            Symbol = -3
          };

          w.Append(elem);
        }

      }
    }


    [TestMethod]
    public void TestText()
    {
      using (OcadReader r = OcadReader.Open(@"C:\daten\felix\kapreolo\scool\regensdorf_ruggenacher\ruggenacher_2017.ocd"))
      {
        foreach (ElementIndex idx in r.GetIndices())
        {
          ElementV9 e = (ElementV9)r.ReadElement(idx);
          if (!string.IsNullOrWhiteSpace(e.Text))
          {
          }
        }
      }
    }

    [TestMethod]
    public void SetColor()
    {
      using (OcadWriter w = Ocad9Writer.AppendTo(@"C:\daten\ASVZ\SOLA\2015\OCAD Vorlagen\sola10k - Kopie.ocd"))
      {
        ElementIndex idx = w.Reader.ReadIndex(264);
        ElementV9 elem = (ElementV9)w.Reader.ReadElement(idx);
        uint c = 0xff000000;
        elem.Color = (int)c;
        w.Overwrite(elem, 264);
      }
    }

    [TestMethod]
    public void SetRasterOrientation()
    {
      using (OcadWriter w = Ocad9Writer.AppendTo(@"C:\daten\felix\kapreolo\karten\hardwald\2016\Hardwald_2016.ocd"))
      {
        foreach (ElementIndex idx in w.Reader.GetIndices())
        {
          ElementV9 elem = (ElementV9)w.Reader.ReadElement(idx);
          if (elem != null && elem.Geometry is Area)
          {
            elem.Angle = 0;
            w.Overwrite(elem, idx.Index);
          }
        }
      }
    }


    [TestMethod]
    public void TestBoxTree()
    {
      BoxTree<Point> t1 = new BoxTree<Point>(2, 1, true);
      t1.InitSize(new IGeometry[] { new Box(new Point2D(0, 0), new Point2D(2, 2)) });
      {
        Point2D p = new Point2D(0.5, 0.5);
        t1.Add(p, p);
      }
      {
        Point2D p = new Point2D(1.5, 0.5);
        t1.Add(p, p);
      }
      {
        Point2D p = new Point2D(0.5, 1.5);
        t1.Add(p, p);
      }
      {
        Point2D p = new Point2D(1.5, 1.5);
        t1.Add(p, p);
      }
      Assert.AreEqual(4, t1.GetLeaves().Count);

      BoxTree<Point> t2 = new BoxTree<Point>(2, 1, true);
      t2.InitSize(new IGeometry[] { new Box(new Point2D(1.1, 1.1), new Point2D(3, 3)) });
      {
        Point2D p = new Point2D(1.5, 1.5);
        t2.Add(p, p);
      }
      {
        Point2D p = new Point2D(2.5, 1.5);
        t2.Add(p, p);
      }
      {
        Point2D p = new Point2D(1.5, 2.5);
        t2.Add(p, p);
      }
      {
        Point2D p = new Point2D(2.5, 2.5);
        t2.Add(p, p);
      }
      Assert.AreEqual(4, t2.GetLeaves().Count);

      int nPairs = 0;
      foreach (BoxTree<Point>.Neighborhood<Point> pair in t1.EnumerateNeighborhoods(t2, 0.2))
      {
        nPairs++;
      }
      Assert.AreEqual(1, nPairs);
    }

    [TestMethod]
    public void TestHiddenSegments()
    {
      using (OcadReader r = OcadReader.Open(@"C:\daten\felix\kapreolo\karten\eigental\test.ocd"))
      {
        foreach (ElementIndex idx in r.GetIndices())
        {
          r.ReadElement(idx);
        }
      }
    }
    [TestMethod]
    public void CanRunBoxTree()
    {
      BoxTree<Curve> t1;
      BoxTree<Curve> t2;
      using (OcadReader reader = OcadReader.Open(@"C:\daten\felix\kapreolo\karten\stadlerberg\Stadlerberg_2007_ocd11.ocd"))
      {
        Element h1 = reader.ReadElement(326);
        Element h2 = reader.ReadElement(3583);
        t1 = CreateTree((Polyline)h1.Geometry);
        t2 = CreateTree((Polyline)h2.Geometry);
      }

      int nPairs = 0;
      foreach (BoxTree<Curve>.Neighborhood<Curve> pair in t1.EnumerateNeighborhoods(t2, 120))
      {
        nPairs++;
      }
      System.Console.WriteLine(nPairs);
    }

    [ClassInitialize]
    public static void ReadShape(TestContext context)
    {
      _curves = new BoxTree<CurveInfo>(2, 4, true);
      using (ShpReader reader = new ShpReader(@"C:\daten\felix\test\Export_Output.shp"))
      {
        foreach (Area geom in reader)
        {
          _curves.InitSize(new[] { geom });
          int i = 0;

          foreach (Polyline line in geom.Border)
          {
            foreach (Curve curve in line.Segments)
            {
              _curves.Add(curve.Extent, new CurveInfo { Curve = curve, Index = i });
              i++;
            }
          }

          break;
        }
      }
    }
    private static BoxTree<CurveInfo> _curves;

    private class CurveInfo
    {
      public Curve Curve;
      public int Index;
      public int NbCount;
      public int TreeCount;
    }

    [TestMethod]
    public void CanRunShapeBoxTree()
    {
      Stopwatch w = new Stopwatch();
      w.Start();
      Point2D search = new Point2D(0.1, 0.1);
      int nSearch = 0;
      int nNeighbor = 0;
      foreach (var entry in _curves.Search(null))
      {
        nSearch++;
        Box box = new Box(entry.Box.Min - search, entry.Box.Max + search);
        int n0 = nNeighbor;
        foreach (var neighbor in _curves.Search(box))
        { nNeighbor++; }
        entry.Value.TreeCount = nNeighbor - n0;
      }
      w.Stop();
      Console.WriteLine(string.Format("{0} {1}", nSearch, nNeighbor));
      Console.WriteLine(w.ElapsedMilliseconds);
    }

    [TestMethod]
    public void CanRunShapeBoxPairs()
    {
      Stopwatch w = new Stopwatch();
      w.Start();
      int nSearch = 0;
      int nNeighbor = 0;
      foreach (var entry in _curves.EnumerateNeighborhoods(_curves, 0.1))
      {
        nSearch++;
        int n0 = nNeighbor;
        foreach (var neighbor in entry.Neighbours)
        { nNeighbor++; }
        entry.Entry.Value.NbCount = nNeighbor - n0;
      }
      w.Stop();
      Console.WriteLine(string.Format("{0} {1}", nSearch, nNeighbor));
      Console.WriteLine(w.ElapsedMilliseconds);
    }

    [TestMethod]
    public void CanRunShapeBoxCompare()
    {
      double d = 0.1;
      Point2D search = new Point2D(d, d);

      foreach (var entry in _curves.EnumerateNeighborhoods(_curves, d))
      {
        int n0 = 0;
        foreach (var neighbor in entry.Neighbours)
        { n0++; }

        int n1 = 0;
        Box box = new Box(entry.Entry.Box.Min - search, entry.Entry.Box.Max + search);
        foreach (var neighbor in _curves.Search(box))
        { n1++; }

        if (n0 != n1)
        { }
      }
    }

    private BoxTree<Curve> CreateTree(Polyline line)
    {
      BoxTree<Curve> t = new BoxTree<Curve>(2, 1, true);
      t.InitSize(new IGeometry[] { line });

      foreach (Curve segment in line.Segments)
      {
        //for (int i = 0; i < 50; i++)
        {
          t.Add(segment.Extent, segment);
        }
      }
      return t;
    }

    [TestMethod]
    public void CanReadElements()
    {
      OcadConnection conn = new OcadConnection(@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\velo.ocd");
      OcadAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT * FROM Elements";
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Symbol", typeof(int));
      ada.Fill(tbl);
      Assert.AreEqual(5, tbl.Columns.Count);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }

    [TestMethod]
    public void CanReadElementsAngle()
    {
      OcadConnection conn = new OcadConnection(@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\velo.ocd");
      OcadAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT Angle FROM elements";
      DataTable tbl = new DataTable();
      ada.Fill(tbl);
      Assert.AreEqual(1, tbl.Columns.Count);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }

    [TestMethod]
    public void CanReadSymbols()
    {
      OcadConnection conn = new OcadConnection(@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\velo.ocd");
      OcadAdapter ada = conn.CreateAdapter();
      ada.SelectCommand.CommandText = "SELECT * FROM Symbols";
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Symbol", typeof(int));
      ada.Fill(tbl);
      Assert.AreEqual(5, tbl.Columns.Count);
      Assert.IsTrue(tbl.Rows.Count > 0);
    }
  }
}
