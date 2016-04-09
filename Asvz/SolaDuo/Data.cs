using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Basics.Geom;
using Basics.Geom.Projection;
using Ocad;
using System.Xml.Serialization;
using System.IO;

namespace Asvz
{
  /// <summary>
  /// Summary description for Sola.
  /// </summary>
  public abstract class StreckenData<T> : Data where T : Strecke
  {
    private class PointCmpr : IComparer<IPoint>
    {
      private readonly double _dist;
      public PointCmpr(double dist)
      {
        _dist = dist;
      }
      public int Compare(IPoint x, IPoint y)
      {
        double d = x.X - y.X;
        if (Math.Abs(d) > _dist)
        {
          return Math.Sign(d);
        }

        d = x.Y - y.Y;
        if (Math.Abs(d) > _dist)
        {
          return Math.Sign(d);
        }
        return 0;
      }
    }

    private IList<T> _strecken;

    protected StreckenData(string dhmName)
      : base(dhmName)
    { }

    public IList<T> Strecken
    {
      get
      {
        if (_strecken == null)
        { _strecken = new List<T>(); }
        return _strecken;
      }
    }
    public KmlConfig KmlConfig { get; } = new KmlConfig();

    public abstract string Name { get; }

    public void ExportKml(string path)
    {
      ExportKml(path, _strecken);
    }

    public void ExportKml(int iStrecke, string path)
    {
      ExportKml(path, new[] { _strecken[iStrecke - 1] });
    }

    private void ExportKml(string path, IEnumerable<T> strecken)
    {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(
        //        "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
        //        "<kml xmlns=\"http://earth.google.com/kml/2.1\"/>");
        "<kml xmlns=\"http://www.opengis.net/kml/2.2\"/>");
      XmlNode node = doc.LastChild;

      XmlNode dc = doc.CreateElement("Document");
      AppendElement(doc, dc, "name", Name);
      AppendElement(doc, dc, "open", "1");

      node.AppendChild(dc);

      if (KmlConfig.IncludeLookAt)
      {
        XmlElement elem = CreateLookAt(doc);
        if (elem != null)
        {
          dc.AppendChild(elem);
        }
      }

      Projection wgs = new Geographic();
      Ellipsoid ell = new Ellipsoid.Wgs84();
      ell.Datum = new Datum.ITRS();
      ell.Datum.Center.X = -0;
      ell.Datum.Center.Y = 0;
      ell.Datum.Center.Z = 0.0;
      wgs.SetEllipsoid(ell);
      TransferProjection prj = new TransferProjection(new Ch1903(), wgs);

      CreateStyles(doc, dc);


      foreach (Strecke strecke in strecken)
      {
        foreach (Categorie cat in strecke.Categories)
        {
          WriteStreckeKml(doc, dc, strecke, cat, prj);
        }
      }

      // Start / Ziel
      if (KmlConfig.IncludeMarks)
      {
        SortedList<IPoint, List<int>> marks = new SortedList<IPoint, List<int>>(new PointCmpr(100));
        foreach (T info in strecken)
        {
          int iStrecke = _strecken.IndexOf(info);
          GetStartZiel(iStrecke, marks, info.Categories[0].Strecke);
        }

        WriteMarks(doc, dc, marks, prj);
      }

      doc.Save(path);
    }

    protected abstract void CreateStyles(XmlDocument doc, XmlNode dc);

    public void ExportGpx(string path)
    {
      ExportGpxDoc(path, _strecken);
    }

    public void ExportGpx(int iStrecke, string path)
    {
      ExportGpxDoc(path, new[] { _strecken[iStrecke - 1] });
    }

    public void ExportGpx(string path, Categorie cat)
    {
      Projection wgs = new Geographic();
      Ellipsoid ell = new Ellipsoid.Wgs84();
      ell.Datum = new Datum.ITRS();
      ell.Datum.Center.X = -0;
      ell.Datum.Center.Y = 0;
      ell.Datum.Center.Z = 0.0;
      wgs.SetEllipsoid(ell);
      TransferProjection prj = new TransferProjection(new Ch1903(), wgs);


      Trk trk = new Trk { Segments = new List<TrkSeg>() };
      TrkSeg seg = GetStreckeGpx(cat, prj);
      trk.Segments.Add(seg);

      Gpx gpx = new Gpx { Trk = trk };
      XmlSerializer ser = new XmlSerializer(typeof(Gpx));
      using (TextWriter w = new StreamWriter(path))
      {
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        //ns.Add("ogr", "http://osgeo.org/gdal");
        //xmlns = "http://www.topografix.com/GPX/1/1" xsi:
        // schemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd" >
        ser.Serialize(w, gpx, ns);
      }
    }

    private void ExportGpxDoc(string path, IEnumerable<T> strecken)
    {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(
        "<gpx creator=\"SOLA - ASVZ\" version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd\"/>");
      XmlNode node = doc.LastChild;

      XmlNode dc = doc.CreateElement("trk");

      node.AppendChild(dc);

      Projection wgs = new Geographic();
      Ellipsoid ell = new Ellipsoid.Wgs84();
      ell.Datum = new Datum.ITRS();
      ell.Datum.Center.X = -0;
      ell.Datum.Center.Y = 0;
      ell.Datum.Center.Z = 0.0;
      wgs.SetEllipsoid(ell);
      TransferProjection prj = new TransferProjection(new Ch1903(), wgs);

      foreach (Strecke strecke in strecken)
      {
        foreach (Categorie cat in strecke.Categories)
        {
          WriteStreckeGpx(doc, dc, strecke, cat, prj);
        }
      }

      doc.Save(path);
    }

    protected virtual XmlElement CreateLookAt(XmlDocument doc)
    { return null; }
  }

  public abstract class Data
  {
    private List<Polyline> _indexList;
    private List<Point> _verpfList;

    private IList<Area> _wald;
    private IList<Area> _siedlung;
    private IList<Area> _teer;

    // symbols

    // Dhm
    private readonly string _dhmName;
    private Grid.DoubleGrid _dhm;

    protected Data(string dhmName)
    {
      _dhmName = dhmName;
    }

    public Grid.DoubleGrid Dhm
    {
      get
      {
        if (_dhm == null)
        {
          _dhm = Grid.DoubleGrid.FromAsciiFile(_dhmName, 0, 1, typeof(double));
          if (_dhm == null)
          { _dhm = Grid.DoubleGrid.FromBinaryFile(_dhmName); }
        }
        return _dhm;
      }
    }

    protected void ReadDefaultData(string fileName)
    {
      OcadReader pReader = OcadReader.Open(fileName);

      ReadDefaultElements(pReader);
      ReadSymbols(pReader);

      pReader.Close();
    }

    protected Polyline ReadSpecialStrecke(string fileName, Polyline defaultStrecke)
    {
      return ReadSpecialStreckeCore(fileName, defaultStrecke);
    }
    protected virtual Polyline ReadSpecialStreckeCore(string fileName, Polyline defaultStrecke)
    {
      return null;
    }

    protected void ReadDefaultElements(OcadReader reader)
    {
      Wald.Clear();
      Siedlung.Clear();
      Teer.Clear();

      IndexList.Clear();
      VerpfList.Clear();

      ReadDefaultElementsCore(reader);
    }

    protected abstract void ReadDefaultElementsCore(OcadReader reader);

    protected void ReadSymbols(OcadReader reader)
    {
      ReadSymbolsCore(reader);
    }
    protected abstract void ReadSymbolsCore(OcadReader reader);

    internal IList<Area> Siedlung
    {
      get
      {
        if (_siedlung == null)
        { _siedlung = new List<Area>(); }
        return _siedlung;
      }
    }
    public Ocad.Symbol.PointSymbol SymVerpf { get; protected set; }

    protected Setup Setup { get; set; }

    protected virtual double DistIndex { get; } = 100.0;

    internal IList<Area> Teer
    {
      get
      {
        if (_teer == null)
        { _teer = new List<Area>(); }
        return _teer;
      }
    }
    internal IList<Area> Wald
    {
      get
      {
        if (_wald == null)
        { _wald = new List<Area>(); }
        return _wald;
      }
    }

    public List<Polyline> IndexList
    {
      get
      {
        if (_indexList == null)
        { _indexList = new List<Polyline>(); }
        return _indexList;
      }
    }

    public List<Point> VerpfList
    {
      get
      {
        if (_verpfList == null)
        { _verpfList = new List<Point>(); }
        return _verpfList;
      }
    }

    public struct VerpflegungSym
    {
      public Polyline Index;
      public Point Symbol;

      public VerpflegungSym(Polyline idx, Point sym)
      {
        Index = idx;
        Symbol = sym;
      }
    }

    public List<VerpflegungSym> Verpflegung(Polyline strecke)
    {
      List<VerpflegungSym> list = new List<VerpflegungSym>();
      foreach (Polyline idx in Index(strecke))
      {
        double t0 = -idx.Segments.First.ParamAt(DistIndex);
        double l1 = idx.Segments.Last.Length();
        double t1 = 2 - idx.Segments.Last.ParamAt(l1 - DistIndex);
        Polyline cross = idx.Clone();
        cross.AddFirst(idx.Segments.First.PointAt(t0));
        cross.Add(idx.Segments.Last.PointAt(t1));

        foreach (Point verpf in VerpfList)
        {
          Box box = (Box)SymVerpf.Graphics.Extent().Project(Setup.Map2Prj).Extent;
          box = new Box(Point.Create(box.Min) + verpf, Point.Create(box.Max) + verpf);
          if (box.Intersection(cross) != null)
          { list.Add(new VerpflegungSym(idx, verpf)); }
        }
      }
      return list;
    }

    private IList<Polyline> Index(Polyline line)
    {
      List<Polyline> index = new List<Polyline>();

      foreach (Polyline idx in IndexList)
      {
        IPoint p = idx.Points.First.Value;
        Arc a = new Arc(p, DistIndex, 0, Math.PI * 2);
        if (line.Intersects(a))
        { index.Add(idx); }
        else
        {
          p = idx.Points.Last.Value;
          a = new Arc(p, DistIndex, 0, Math.PI * 2);
          if (line.Intersects(a))
          { index.Add(idx); }
        }

        //double t0 = -idx.Segments.First.ParamAt(DistIndex);
        //double l_1 = idx.Segments.Last.Length();
        //double t1 = 1 + idx.Segments.Last.ParamAt(DistIndex);
        //Polyline cross = idx.Clone();
        //cross.AddFirst(idx.Segments.First.PointAt(t0));
        //cross.Add(idx.Segments.Last.PointAt(t1));

        //if (line.Intersection(cross) != null)
        //{ index.Add(idx); }
      }
      return index;
    }

    protected void WriteStreckeGpx(XmlDocument doc, XmlNode parent,
      Strecke strecke, Categorie cat, TransferProjection prj)
    {
      XmlNode trkSeg = doc.CreateElement("trkseg");
      parent.AppendChild(trkSeg);

      Polyline line = cat.Strecke;
      Polyline s = line.Linearize(3.0);
      s = s.Project(prj);

      foreach (IPoint p in s.Points)
      {
        XmlElement elem = doc.CreateElement("trkpt");

        XmlAttribute latAttr = doc.CreateAttribute("lat");
        latAttr.Value = string.Format("{0:F10}", p.Y);
        elem.Attributes.Append(latAttr);

        XmlAttribute lonAttr = doc.CreateAttribute("lon");
        lonAttr.Value = string.Format("{0:F10}", p.X);
        elem.Attributes.Append(lonAttr);

        trkSeg.AppendChild(elem);
      }
    }

    protected TrkSeg GetStreckeGpx(Categorie cat, TransferProjection prj)
    {
      Polyline line = cat.Strecke;
      Polyline s = line.Linearize(3.0);
      s = s.Project(prj);

      TrkSeg seg = new TrkSeg { Points = new List<TrkPt>() };
      foreach (IPoint p in s.Points)
      {
        TrkPt pt = new TrkPt { Lat = p.Y, Lon = p.X };
        seg.Points.Add(pt);
      }
      return seg;
    }

    protected void WriteStreckeKml(XmlDocument doc, XmlNode parent,
      Strecke strecke, Categorie cat,
      TransferProjection prj)
    {
      Polyline line = cat.Strecke;
      Polyline s = line.Linearize(3.0);
      s = s.Project(prj);
      StringBuilder builder = new StringBuilder();
      foreach (Point p in s.Points)
      {
        builder.AppendFormat("{0:F6},{1:F6},0 ", p.X, p.Y);
      }

      XmlElement elem = doc.CreateElement("Placemark");
      AppendElement(doc, elem, "name", strecke.Name(cat));

      string k = string.Format("{0}", cat.Name);
      k = k.Replace("Default, ", "");
      AppendElement(doc, elem, "description",
        string.Format("{1}{0}Länge {2:N2} km{0}Steigung {3:N0} m",
        Environment.NewLine,
        k, cat.Laenge() / 1000.0,
        cat.SteigungRound(5.0)));

      AppendElement(doc, elem, "styleUrl", "#" + cat.KmlStyle);

      parent.AppendChild(elem);

      XmlElement xmlLine = doc.CreateElement("LineString");
      AppendElement(doc, xmlLine, "extrude", "1");
      AppendElement(doc, xmlLine, "tessellate", "1");
      AppendElement(doc, xmlLine, "coordinates", builder.ToString());

      elem.AppendChild(xmlLine);
    }

    protected virtual void WriteMarks(XmlDocument doc, XmlNode parent,
      SortedList<IPoint, List<int>> marks,
      TransferProjection prj)
    { }

    protected void GetStartZiel(int iStrecke, SortedList<IPoint, List<int>> marks, Polyline strecke)
    {
      IPoint start = strecke.Points.First.Value;
      IPoint ziel = strecke.Points.Last.Value;

      List<int> l;
      if (marks.TryGetValue(start, out l) == false)
      {
        l = new List<int>();
        marks.Add(start, l);
      }
      l.Add(iStrecke + 1);

      if (marks.TryGetValue(ziel, out l) == false)
      {
        l = new List<int>();
        marks.Add(ziel, l);
      }
      l.Add(-(iStrecke + 1));
    }
    public static void AppendElement(XmlDocument doc, XmlNode element, string name, string value)
    {
      XmlElement app = doc.CreateElement(name);
      XmlText text = doc.CreateTextNode(value);
      app.AppendChild(text);
      element.AppendChild(app);
    }

    protected static XmlElement GetStyle(XmlDocument doc, string id, string color, int width)
    {
      XmlElement style = doc.CreateElement("Style");
      {
        XmlAttribute attr = doc.CreateAttribute("id");
        attr.Value = id;
        style.Attributes.Append(attr);
      }

      XmlElement lineStyle = doc.CreateElement("LineStyle");
      AppendElement(doc, lineStyle, "color", color);
      AppendElement(doc, lineStyle, "width", width.ToString());

      style.AppendChild(lineStyle);
      return style;
    }
  }
  [XmlRoot("gpx")]
  public class Gpx
  {
    [XmlElement("trk")]
    public Trk Trk { get; set; }
  }
  public class Trk
  {
    [XmlElement("trkseg")]
    public List<TrkSeg> Segments { get; set; }
  }
  public class TrkSeg
  {
    [XmlElement("trkpt")]
    public List<TrkPt> Points { get; set; }
  }
  public class TrkPt
  {
    [XmlAttribute("lat")]
    public double Lat { get; set; }
    [XmlAttribute("lon")]
    public double Lon { get; set; }
  }
}
