using Basics.Geom;
using Dhm;
using Grid;
using laszip.net;
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
        foreach (var idx in r.GetIndices())
        {
          r.ReadElement(idx, out GeoElement e);
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
        foreach (var p in r)
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
      foreach (var path in Directory.EnumerateFiles(dir))
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

      foreach (var key in tiles.Keys)
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
      foreach (var path in Directory.EnumerateFiles(dir))
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
      foreach (var path in Directory.EnumerateFiles(dir))
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
      //string dir = @"C:\daten\felix\kapreolo\karten\blauen\2018\lidar";
      //string dir = @"C:\daten\felix\kapreolo\karten\irchel\2019\lidar";
      //string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";
      string dir = @"C:\daten\felix\kapreolo\scool\boppelsen_maiacher\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (var path in Directory.EnumerateFiles(dir))
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
      foreach (var key in tiles.Keys)
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
      foreach (var path in Directory.EnumerateFiles(dir))
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
      foreach (var key in tiles.Keys)
      {
        //string key = "26880_12545"; 
        Export(dir, key, $"dom{key}", resolution, LasUtils.Dom, r, g, b);
      }
    }

    [TestMethod]
    public void TestLasMulti()
    {
      double resolution = 0.5;
      string dir = @"C:\daten\felix\kapreolo\scool\boppelsen_maiacher\lidar";
      //string dir = @"C:\daten\felix\kapreolo\karten\irchel\2019\lidar";
      //      string dir = @"C:\daten\felix\kapreolo\karten\hardwald\2017\lidar";

      Dictionary<string, string> tiles = new Dictionary<string, string>();
      foreach (var path in Directory.EnumerateFiles(dir))
      {
        string key = Path.GetFileNameWithoutExtension(path);
        if (!char.IsDigit(key[0]))
        { continue; }
        tiles[key] = path;
      }
      foreach (var key in tiles.Keys)
      {
        //string key = "26880_12545";

        DoubleGrid grd = GetGrid(dir, key, resolution, LasUtils.Dtm);

        string tifPath = Path.Combine(dir, $"multi{key}.tif");
        ImageGrid.WriteWorldFile(grd, ImageGrid.GetWorldPath(tifPath));
        ImageGrid.GridToImage(tifPath, grd.Extent.Nx, grd.Extent.Ny,
           getRValue: (ix, iy) =>
           {
             double dh = ix > 0 ? grd[ix, iy] - grd[ix - 1, iy] : grd[1, iy] - grd[0, iy];
             return (byte)Math.Max(0, Math.Min(255, (int)(128 + 128 * dh)));
           },
           getGValue: (ix, iy) =>
           {
             double dh = iy > 0 ? grd[ix, iy] - grd[ix, iy - 1] : grd[ix, 1] - grd[ix, 0];
             return (byte)Math.Max(0, Math.Min(255, (int)(128 + 128 * dh)));
           }
           );
      }
    }

    private void Export(string dir, string key, string resName, double resolution,
      Func<int, int, Func<int, int, List<Point>>, double> grdFct,
       byte[] r, byte[] g, byte[] b)
    {
      string pngPath = Path.Combine(dir, $"{resName}.png");

      if (File.Exists(pngPath))
      { return; }

      if (File.Exists(Path.Combine(dir, $"{key}.png")))
      {
        File.Move(Path.Combine(dir, $"{key}.png"), pngPath);
        File.Move(Path.Combine(dir, $"{key}.pgw"), Path.Combine(dir, $"{resName}.pgw"));
        return;
      }

      DoubleGrid grd = GetGrid(dir, key, resolution, grdFct);
      ImageGrid.GridToImage(grd.ToIntGrid(), pngPath, r, g, b, System.Drawing.Imaging.ImageFormat.Png);
    }

    private DoubleGrid GetGrid(string dir, string key, double resolution,
      Func<int, int, Func<int, int, List<Point>>, double> grdFct)
    {
      string lazName = Path.Combine(dir, key + ".laz");

      IEnumerable<LasUtils.ILasPoint> lasPts = GetLazPoints(lazName);
      //IEnumerable<LasUtils.ILasPoint> lasPts = GetTextLasPoints(lazName);

      DoubleGrid grd = LasUtils.CreateGrid(lasPts, resolution, grdFct);
      return grd;
    }

    private IEnumerable<LasUtils.ILasPoint> GetLazPoints(string lazPath)
    {
      var lazReader = new laszip_dll();
      var compressed = true;
      try
      {
        lazReader.laszip_open_reader(lazPath, ref compressed);
        var numberOfPoints = lazReader.header.number_of_point_records;

        //// Check some header values
        //Debug.WriteLine(lazReader.header.min_x);
        //Debug.WriteLine(lazReader.header.min_y);
        //Debug.WriteLine(lazReader.header.min_z);
        //Debug.WriteLine(lazReader.header.max_x);
        //Debug.WriteLine(lazReader.header.max_y);
        //Debug.WriteLine(lazReader.header.max_z);

        var coordArray = new double[3];

        LasPoint point = new LasPoint();

        // Loop through number of points indicated
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
          // Read the point
          lazReader.laszip_read_point();

          // Get precision coordinates
          lazReader.laszip_get_coordinates(coordArray);
          point.X = coordArray[0];
          point.Y = coordArray[1];
          point.Z = coordArray[2];

          // Get classification value
          point.Intensity = lazReader.point.intensity;

          yield return point;
        }
      }
      finally
      {
        // Close the reader
        lazReader.laszip_close_reader();
      }
    }

    private class LasPoint : LasUtils.ILasPoint
    {
      public double X { get; set; }
      public double Y { get; set; }
      public double Z { get; set; }
      public int Intensity { get; set; }
    }

    private class LasTextReader
    {
      public static IEnumerable<LasUtils.ILasPoint> GetPoints(string lasFilePath)
      {
        using (TextReader reader = new StreamReader(lasFilePath))
        {
          return GetPoints(reader);
        }
      }
      public static IEnumerable<LasUtils.ILasPoint> GetPoints(TextReader reader)
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          IList<string> parts = line.Split();
          yield return new LasPoint
          {
            X = double.Parse(parts[0]),
            Y = double.Parse(parts[1]),
            Z = double.Parse(parts[2]),
            Intensity = int.Parse(parts[3])
          };
        }
      }
    }

    [TestMethod]
    public void DownloadLaz()
    {
      // Boppelsen maiacher
      List<string> keys = new List<string> { "26725_12580", "26725_12575" };
      string root = @"C:\daten\felix\kapreolo\scool\boppelsen_maiacher\lidar";

      foreach (string key in keys)
      {
        string path = Path.Combine(root, $"{key}.laz");
        if (!File.Exists(path))
        {
          //          continue;
          using (System.Net.WebClient wc = new System.Net.WebClient())
          {
            string url = $"http://maps.zh.ch/download/hoehen/2014/lidar/{key}.laz";
            wc.DownloadFile(url, path);
          }
        }
      }
    }

    [TestMethod]
    public void DhmIrchel()
    {
      List<string> keys = new List<string> {
        "26850_12660", "26850_12670", "26850_12680",
        "26860_12660", "26860_12670", "26860_12680",
        "26870_12660", "26870_12670", "26870_12680"
      };
      double x0 = double.MaxValue;
      double y0 = double.MinValue;
      double x1 = double.MinValue;
      double y1 = double.MaxValue;
      double size = 2;
      string root = @"C:\daten\felix\kapreolo\karten\irchel\2019\";
      foreach (string key in keys)
      {
        string path = $"{root}{key}.asc.zip";
        if (!File.Exists(path))
        {
          //          continue;
          using (System.Net.WebClient wc = new System.Net.WebClient())
          {
            string url = $"https://maps.zh.ch/download/hoehen/2014/dtm/asc/{key}.asc.zip";
            wc.DownloadFile(url, path);
          }
        }

        DoubleGrid grdHeight;
        using (var zipStream = new FileStream(path, FileMode.Open))
        using (var archive = new System.IO.Compression.ZipArchive(zipStream))
        {
          var entry = archive.GetEntry($"{key}.asc");

          using (var stream = entry.Open())
          using (var reader = new StreamReader(stream))
          {
            grdHeight = DataDoubleGrid.FromAsciiFile(reader, 0, 0.01, typeof(double));
          }
        }
        GridExtent ext = grdHeight.Extent;

        x0 = Math.Min(x0, ext.X0);
        y0 = Math.Max(y0, ext.Y0);
        x1 = Math.Max(x1, ext.X1);
        y1 = Math.Min(y1, ext.Y1);
        DoubleGrid grdResample = new DataDoubleGrid(
          (int)(ext.Nx / (size / ext.Dx)), (int)(ext.Ny / (size / ext.Dx)),
          typeof(double), ext.X0, ext.Y0, size);

        DoubleGrid grdRes = grdResample + grdHeight;
        grdRes.SaveASCII($"{root}{key}__.asc", "N2");
      }

      DoubleGrid grdSum = new DataDoubleGrid((int)((x1 - x0) / size), (int)((y0 - y1) / size),
          typeof(double), x0, y0, size);
      foreach (string key in keys)
      {
        string path = $"{root}{key}__.asc";
        if (!File.Exists(path))
          continue;

        DoubleGrid grdHeight = DataDoubleGrid.FromAsciiFile(path, 0, 0.01, typeof(double));
        grdSum = DoubleGrid.Max(grdSum, grdHeight);
      }
      grdSum.SaveASCII($"{root}irchel.asc", "N2");
    }
    [TestMethod]
    public void TestWriteBildObjekte()
    {
      using (OcadWriter w = OcadWriter.AppendTo(@"C:\daten\felix\kapreolo\scool\regensdorf_ruggenacher\test.ocd"))
      {
        {
          Element elem = new GeoElement(new Point2D(677000, 254000));
          elem.Type = GeomType.unformattedText;
          elem.Color = Color.ColorToCmyk(System.Drawing.Color.Blue).ToNumber();
          elem.Text = "A";
          elem.Symbol = -3;

          w.Append(elem);
        }

        {
          Element elem = new GeoElement(Polyline.Create(new[] { new Point2D(677010, 254000), new Point2D(677020, 254010) }));
          elem.Type = GeomType.line;
          elem.Color = Color.ColorToCmyk(System.Drawing.Color.Green).ToNumber();
          elem.LineWidth = 30;
          elem.Symbol = -3;

          w.Append(elem);
        }

      }
    }


    [TestMethod]
    public void TestText()
    {
      using (OcadReader r = OcadReader.Open(@"C:\daten\felix\kapreolo\scool\regensdorf_ruggenacher\ruggenacher_2017.ocd"))
      {
        foreach (var e in r.EnumGeoElements())
        {
          if (!string.IsNullOrWhiteSpace(e.Text))
          {
          }
        }
      }
    }

    [TestMethod]
    public void SetColor()
    {
      using (OcadWriter w = OcadWriter.AppendTo(@"C:\daten\ASVZ\SOLA\2015\OCAD Vorlagen\sola10k - Kopie.ocd"))
      {
        ElementIndex idx = w.ReadIndex(264);
        w.ReadElement(idx, out GeoElement elem);
        uint c = 0xff000000;
        elem.Color = (int)c;
        w.Overwrite(elem, 264);
      }
    }

    [TestMethod]
    public void SetRasterOrientation()
    {
      using (OcadWriter w = OcadWriter.AppendTo(@"C:\daten\felix\kapreolo\karten\hardwald\2016\Hardwald_2016.ocd"))
      {
        foreach (var idx in w.GetIndices())
        {
          w.ReadElement(idx, out GeoElement elem);
          if (elem != null && elem.Geometry is GeoElement.Area)
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
      foreach (var pair in t1.EnumerateNeighborhoods(t2, 0.2))
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
        foreach (var idx in r.GetIndices())
        {
          r.ReadElement(idx, out GeoElement e);
        }
      }
    }
    [TestMethod]
    public void CanRunBoxTree()
    {
      BoxTree<ISegment> t1;
      BoxTree<ISegment> t2;
      using (OcadReader reader = OcadReader.Open(@"C:\daten\felix\kapreolo\karten\stadlerberg\Stadlerberg_2007_ocd11.ocd"))
      {
        reader.ReadElement(326, out GeoElement h1);
        reader.ReadElement(3583, out GeoElement h2);
        t1 = CreateTree(((GeoElement.Line)h1.Geometry).BaseGeometry);
        t2 = CreateTree(((GeoElement.Line)h2.Geometry).BaseGeometry);
      }

      int nPairs = 0;
      foreach (var pair in t1.EnumerateNeighborhoods(t2, 120))
      {
        nPairs++;
      }
      System.Console.WriteLine(nPairs);
    }

    private BoxTree<CurveInfo> _curves;
    private BoxTree<CurveInfo> Curves => _curves ?? (_curves = ReadShape());
    private BoxTree<CurveInfo> ReadShape()
    {
      BoxTree<CurveInfo> curves = new BoxTree<CurveInfo>(2, 4, true);
      using (ShpReader reader = new ShpReader(@"C:\daten\felix\test\Export_Output.shp"))
      {
        foreach (var geom in reader)
        {
          Area area = (Area)geom;
          curves.InitSize(new[] { geom });
          int i = 0;

          foreach (var line in area.Border)
          {
            foreach (var curve in line.EnumSegments())
            {
              curves.Add(curve.Extent, new CurveInfo { Curve = curve, Index = i });
              i++;
            }
          }

          break;
        }
      }
      return curves;
    }

    private class CurveInfo
    {
      public ISegment Curve;
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
      foreach (var entry in Curves.Search(null))
      {
        nSearch++;
        Box box = new Box(entry.Box.Min - search, entry.Box.Max + search);
        int n0 = nNeighbor;
        foreach (var neighbor in Curves.Search(box))
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
      foreach (var entry in Curves.EnumerateNeighborhoods(Curves, 0.1))
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

      foreach (var entry in Curves.EnumerateNeighborhoods(Curves, d))
      {
        int n0 = 0;
        foreach (var neighbor in entry.Neighbours)
        { n0++; }

        int n1 = 0;
        Box box = new Box(entry.Entry.Box.Min - search, entry.Entry.Box.Max + search);
        foreach (var neighbor in Curves.Search(box))
        { n1++; }

        if (n0 != n1)
        { }
      }
    }

    private BoxTree<ISegment> CreateTree(Polyline line)
    {
      BoxTree<ISegment> t = new BoxTree<ISegment>(2, 1, true);
      t.InitSize(new IGeometry[] { line });

      foreach (var segment in line.EnumSegments())
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
      using (OcadConnection conn = new OcadConnection(@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\velo.ocd"))
      {
        OcadAdapter ada = conn.CreateAdapter();
        ada.SelectCommand.CommandText = "SELECT * FROM Elements";
        DataTable tbl = new DataTable();
        tbl.Columns.Add("Symbol", typeof(int));
        ada.Fill(tbl);
        Assert.AreEqual(5, tbl.Columns.Count);
        Assert.IsTrue(tbl.Rows.Count > 0);
      }
    }

    [TestMethod]
    public void CanReadElementsAngle()
    {
      using (OcadConnection conn = new OcadConnection(@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\velo.ocd"))
      {
        OcadAdapter ada = conn.CreateAdapter();
        ada.SelectCommand.CommandText = "SELECT Angle FROM elements";
        DataTable tbl = new DataTable();
        ada.Fill(tbl);
        Assert.AreEqual(1, tbl.Columns.Count);
        Assert.IsTrue(tbl.Rows.Count > 0);
      }
    }

    [TestMethod]
    public void CanReadSymbols()
    {
      using (OcadConnection conn = new OcadConnection(@"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\velo.ocd"))
      {
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
}
