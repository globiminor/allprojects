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
      XmlElement dc = KmlUtils.InitDoc(Name);
      XmlDocument doc = dc.OwnerDocument;

      if (KmlConfig.IncludeLookAt)
      {
        XmlElement elem = CreateLookAt(doc);
        if (elem != null)
        {
          dc.AppendChild(elem);
        }
      }

      TransferProjection prj = KmlUtils.GetTransferProjection(new Ch1903());

      CreateStyles(doc, dc);


      foreach (var strecke in strecken)
      {
        foreach (var cat in strecke.Categories)
        {
          WriteStreckeKml(doc, dc, strecke, cat, prj);
        }
      }

      // Start / Ziel
      if (KmlConfig.IncludeMarks)
      {
        SortedList<IPoint, List<int>> marks = new SortedList<IPoint, List<int>>(new PointCmpr(100));
        foreach (var info in strecken)
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

    public void ExportGpx(string path, IList<Categorie> cats, bool joined = false)
    {
      TransferProjection prj = GpxUtils.GetTransferProjection(new Ch1903());

      Trk trk = new Trk { Segments = new List<TrkSeg>() };
      TrkSeg seg = new TrkSeg { Points = new List<Pt>() };
      foreach (var cat in cats)
      {
        TrkSeg catSeg = GpxUtils.GetStreckeGpx(cat.Strecke, prj);
        if (joined)
        { seg.Points.AddRange(catSeg.Points); }
        else
        { trk.Segments.Add(catSeg); }
      }
      if (joined)
      { trk.Segments.Add(seg); }

      Gpx gpx = new Gpx { Trk = trk };
      GpxUtils.Write(path, gpx);
    }
    public void ExportKmGpx(string path, Categorie cat)
    {
      TransferProjection prj = GpxUtils.GetTransferProjection(new Ch1903());

      Polyline line = cat.Strecke;
      double lengthMeas = cat.DispLength / 1000;
      int iKm = 1;

      List<Pt> kms = new List<Pt>();
      while (iKm < lengthMeas)
      {
        double[] param = cat.GetLineParams(iKm * 1000.0);
        Point p = line.Segments[(int)param[0]].PointAt(param[1]);

        p = p.Project(prj);

        Pt wpt = new Pt { Lat = p.X, Lon = p.Y, Name = string.Format("Km {0}", iKm) };
        kms.Add(wpt);

        iKm++;
      }

      Gpx gpx = new Gpx { WayPoints = kms };
      GpxUtils.Write(path, gpx);
    }


    private void ExportGpxDoc(string path, IEnumerable<T> strecken)
    {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(
        "<gpx creator=\"SOLA - ASVZ\" version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd\"/>");
      XmlNode node = doc.LastChild;

      XmlNode dc = doc.CreateElement("trk");

      node.AppendChild(dc);

      TransferProjection prj = GpxUtils.GetTransferProjection(new Ch1903());

      foreach (var strecke in strecken)
      {
        foreach (var cat in strecke.Categories)
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
    private Grid.IDoubleGrid _dhm;

    protected Data(string dhmName)
    {
      _dhmName = dhmName;
    }

    public Grid.IDoubleGrid Dhm
    {
      get
      {
        if (_dhm == null)
        {
          _dhm = Grid.DataDoubleGrid.FromAsciiFile(_dhmName, 0, 1, typeof(double));
          if (_dhm == null)
          { _dhm = Grid.DataDoubleGrid.FromBinaryFile(_dhmName); }
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
      foreach (var idx in Index(strecke))
      {
        double t0 = -idx.Segments.First.ParamAt(DistIndex);
        double l1 = idx.Segments.Last.Length();
        double t1 = 2 - idx.Segments.Last.ParamAt(l1 - DistIndex);
        Polyline cross = idx.Clone();
        cross.AddFirst(idx.Segments.First.PointAt(t0));
        cross.Add(idx.Segments.Last.PointAt(t1));

        foreach (var verpf in VerpfList)
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

      foreach (var idx in IndexList)
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

      foreach (var p in s.Points)
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

    protected void WriteStreckeKml(XmlDocument doc, XmlNode parent,
      Strecke strecke, Categorie cat,
      TransferProjection prj)
    {
      Polyline line = cat.Strecke;

      XmlElement elem = doc.CreateElement("Placemark");
      XmlUtils.AppendElement(doc, elem, "name", strecke.Name(cat));

      string k = string.Format("{0}", cat.Name);
      k = k.Replace("Default, ", "");
      XmlUtils.AppendElement(doc, elem, "description",
        string.Format("{1}{0}Länge {2:N2} km{0}Steigung {3:N0} m",
        Environment.NewLine,
        k, cat.DispLength / 1000.0,
        cat.SteigungRound(5.0)));

      XmlUtils.AppendElement(doc, elem, "styleUrl", "#" + cat.KmlStyle);

      parent.AppendChild(elem);

      KmlUtils.AppendLine(elem, line, prj);
    }

    protected virtual void WriteMarks(XmlDocument doc, XmlNode parent,
      SortedList<IPoint, List<int>> marks,
      TransferProjection prj)
    { }

    protected void GetStartZiel(int iStrecke, SortedList<IPoint, List<int>> marks, Polyline strecke)
    {
      IPoint start = strecke.Points.First.Value;
      IPoint ziel = strecke.Points.Last.Value;

      if (marks.TryGetValue(start, out List<int> l) == false)
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
  }
}
