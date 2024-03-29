using Basics.Geom;
using Basics.Geom.Projection;
using Ocad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Asvz.Sola
{
  /// <summary>
  /// Summary description for Sola.
  /// </summary>
  public class SolaData : StreckenData<SolaStrecke>
  {
    private Dictionary<Polyline, int> _transport; // geometry / Symbol

    public SolaData(string fileName, string dhmName)
      : base(dhmName)
    {
      ReadDefaultData(fileName);
      ReadSpecialStrecken(fileName);
    }

    public override string Name
    {
      get { return "SOLA Stafette"; }
    }
    private void ReadSpecialStrecken(string fileName)
    {
      foreach (var info in Strecken)
      {
        foreach (var o in info.Categories)
        {
          SolaCategorie cat = (SolaCategorie)o;
          if ((cat.Typ & Kategorie.Default) == Kategorie.Default)
          {
            if (!(cat.Strecke != null)) throw new InvalidOperationException($"Cat {cat}: Strecke == null");
            continue;
          }

          string sCat = string.Format("{0}", cat.Typ);
          int i = sCat.IndexOf(",");
          if (i > 0)
          {
            sCat = sCat.Substring(0, i);
          }
          string f = Path.GetFileNameWithoutExtension(fileName) + "." + sCat + ".ocd";
          f = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + f;

          cat.SetGeometry(
            ReadSpecialStrecke(f, info.GetCategorie(Kategorie.Default).Strecke),
            this);
        }
      }
    }

    protected override Polyline ReadSpecialStreckeCore(string fileName, Polyline defaultStrecke)
    {
      OcadReader reader = OcadReader.Open(fileName);

      int iIndex = 0;
      ElementIndex pIndex;
      IList<ElementIndex> indexList = new List<ElementIndex>();
      while ((pIndex = reader.ReadIndex(iIndex)) != null)
      {
        if (pIndex.Symbol == SymT.Strecke)
        { indexList.Add(pIndex); }
        iIndex++;
      }

      IPoint start = defaultStrecke.Points[0];
      IPoint end = defaultStrecke.Points.Last();
      Polyline line0 = null;
      foreach (var elem in reader.EnumGeoElements(indexList))
      {
        Polyline line = ((GeoElement.Line)elem.Geometry).BaseGeometry;
        if (PointOp.Dist2(line.Points[0], start) < 100 &&
          PointOp.Dist2(line.Points.Last(), end) < 100)
        {
          line0 = line;
          break;
        }
      }

      reader.Close();

      return line0;
    }

    protected override void ReadDefaultElementsCore(OcadReader reader)
    {
      int iIndex = 0;
      ElementIndex pIndex;
      IList<ElementIndex> pIndexList = new List<ElementIndex>();
      while ((pIndex = reader.ReadIndex(iIndex)) != null)
      {
        pIndexList.Add(pIndex);
        iIndex++;
      }

      IList<GeoElement> strecken = new List<GeoElement>();
      IList<GeoElement> nummern = new List<GeoElement>();

      _transport = new Dictionary<Polyline, int>();

      foreach (var elem in reader.EnumGeoElements(pIndexList))
      {
        if (elem.Symbol == SymT.Strecke)
        { strecken.Add(elem); }
        else if (elem.Symbol == SymT.TextStrecke)
        {
          if (int.TryParse(elem.Text, out int iStrecke))
          { nummern.Add(elem); }
        }
        else if (elem.Symbol == SymT.Transport)
        { _transport.Add(((GeoElement.Line)elem.Geometry).BaseGeometry, elem.Symbol); }
        else if (elem.Symbol == SymT.TransportUi)
        { _transport.Add(((GeoElement.Line)elem.Geometry).BaseGeometry, elem.Symbol); }
        else if (elem.Symbol == SymT.TransportHilf)
        { _transport.Add(((GeoElement.Line)elem.Geometry).BaseGeometry, elem.Symbol); }
        else if (elem.Symbol == SymT.LinieBreit)
        { IndexList.Add(((GeoElement.Line)elem.Geometry).BaseGeometry); }
        else if (elem.Symbol == SymT.Verpflegung)
        { VerpfList.Add(((GeoElement.Point)elem.Geometry).BaseGeometry); }
        else if (elem.Symbol == SymT.Wald)
        { Wald.Add(((GeoElement.Surface)elem.Geometry).BaseGeometry); }
        else if (elem.Symbol == SymT.Siedlung)
        { Siedlung.Add(((GeoElement.Surface)elem.Geometry).BaseGeometry); }
        else if (elem.Symbol == SymT.Teer)
        { Teer.Add(((GeoElement.Surface)elem.Geometry).BaseGeometry); }
      }
      if (!(strecken.Count == Ddx.Strecken.Count)) throw new InvalidOperationException($"#strecken ({strecken.Count}) != #Ddx.Strecken ({Ddx.Strecken.Count})");
      if (!(nummern.Count == Ddx.Strecken.Count)) throw new InvalidOperationException($"#nummern ({nummern.Count}) != #Ddx.Strecken ({Ddx.Strecken.Count})");

      SortDefaultStrecken(strecken, nummern);
    }

    protected override void CreateStyles(XmlDocument doc, XmlNode dc)
    {
      XmlElement style;

      style = KmlUtils.GetStyle(doc, "einfach", "C000ff00", 3);
      dc.AppendChild(style);

      style = KmlUtils.GetStyle(doc, "mittel", "C00080ff", 3);
      dc.AppendChild(style);

      style = KmlUtils.GetStyle(doc, "schwierig", "C00000ff", 3);
      dc.AppendChild(style);

      style = KmlUtils.GetStyle(doc, "special", "80ff0000", 5);
      dc.AppendChild(style);

      style = doc.CreateElement("Style");
      {
        XmlAttribute attr = doc.CreateAttribute("id");
        attr.Value = "pin";
        style.Attributes.Append(attr);
      }

      XmlElement iconStyle = doc.CreateElement("IconStyle");
      //attr = doc.CreateAttribute("color");
      //attr.Value = "ff00ff00";
      //iconStyle.Attributes.Append(attr);

      XmlElement icon = doc.CreateElement("Icon");
      XmlUtils.AppendElement(doc, icon, "href", "sola.png");
      //attr.Value = "http://maps.google.com/mapfiles/kml/pal3/icon21.png";

      iconStyle.AppendChild(icon);
      style.AppendChild(iconStyle);

      dc.AppendChild(style);
    }

    private void SortDefaultStrecken(IList<GeoElement> strecken, IList<GeoElement> nummern)
    {
      foreach (var rawStrecke in Ddx.Strecken)
      {
        SolaStrecke strecke = new SolaStrecke(rawStrecke.Nummer, rawStrecke.Vorlage);
        foreach (var o in rawStrecke.Categories)
        {
          SolaCategorie rawCat = (SolaCategorie)o;
          SolaCategorie cat = new SolaCategorie(rawCat.Typ,
            rawCat.UserLength, rawCat.OffsetStart, rawCat.OffsetEnd,
            rawCat.Stufe);
          strecke.Categories.Add(cat);
        }
        Strecken.Add(strecke);
      }

      for (int i = 0; i < Strecken.Count; i++)
      {
        GeoElement pStrecke = nummern[i];
        int iStrecke = Convert.ToInt32(pStrecke.Text);
        Point pNummer = 0.5 * (pStrecke.Geometry.Extent.Max +
          (Point)pStrecke.Geometry.Extent.Min);

        double dMinMin = -1;
        int iMin = -1;
        for (int iGeom = 0; iGeom < Strecken.Count; iGeom++)
        {
          Polyline pLine = ((GeoElement.Line)strecken[iGeom].Geometry).BaseGeometry;
          double dMin = -1;
          foreach (var pSeg in pLine.EnumSegments())
          {
            Point pX = 0.5 * (pSeg.Extent.Max +
              (Point)pSeg.Extent.Min);
            double d = pX.Project(Geometry.ToXY).Dist2(pNummer);
            if (dMin < 0 || d < dMin)
            { dMin = d; }
          }

          if (dMinMin < 0 || dMin < dMinMin)
          {
            dMinMin = dMin;
            iMin = iGeom;
          }
        }

        Strecken[iStrecke - 1].GetCategorie(Kategorie.Default).SetGeometry(
          ((GeoElement.Line)strecken[iMin].Geometry).BaseGeometry, this);
      }
    }

    protected override void ReadSymbolsCore(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      Ocad.Symbol.BaseSymbol pSymbol;
      IList<int> pIndexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        pIndexList.Add(i);
        iIndex++;
      }

      Setup = reader.ReadSetup();
      Setup.PrjTrans.X = 0;
      Setup.PrjTrans.Y = 0;
      foreach (var iPos in pIndexList)
      {
        pSymbol = reader.ReadSymbol(iPos);

        if (pSymbol.Number == SymT.Verpflegung)
        { SymVerpf = (Ocad.Symbol.PointSymbol)pSymbol; }
      }
    }

    private IList<Polyline> Index(int strecke)
    {
      Polyline line = Strecken[strecke].GetCategorie(Kategorie.Default).Strecke;
      List<Polyline> index = new List<Polyline>();

      foreach (var idx in IndexList)
      {
        double t0 = -idx.GetSegment(0).ParamAt(DistIndex);
        double l_1 = idx.GetSegment(-1).Length();
        double t1 = 1 + idx.GetSegment(-1).ParamAt(DistIndex);
        Polyline cross = idx.Clone();
        cross.Insert(0, idx.GetSegment(0).PointAt(t0));
        cross.Add(idx.GetSegment(-1).PointAt(t1));

        if (line.Intersection(cross) != null)
        { index.Add(idx); }
      }
      return index;
    }

    /// <summary>
    /// Liste(oberirdisch, unterirdisch, oberirdisch, ...) des Transportweges von 'strecke'
    /// Falls streckeFrom &lt 0 oder streckeTp &lt 0
    /// </summary>
    /// <param name="streckeFrom"></param>
    /// <param name="streckeTo"></param>
    /// <returns>
    /// null; falls streckeFrom &lt 0 oder streckeTp &lt 0
    /// spezialStrecke; falls streckeFrom == streckeTo
    /// Transportstrecke; sonst
    /// </returns>
    public Transport Transport(int streckeFrom, int streckeTo)
    {
      if (streckeFrom < 0 || streckeTo < 0)
      { return null; }
      if (streckeFrom >= Ddx.Uebergabe.Count || streckeTo >= Ddx.Uebergabe.Count)
      { return null; }

      Transport t = new Transport(streckeFrom, streckeTo);

      UebergabeTransport.GetLayout(streckeFrom, out Polyline borderFrom, out IPoint legendPos);

      UebergabeTransport.GetLayout(streckeTo, out Polyline borderTo, out legendPos);


      IBox boxFrom = null;
      if (borderFrom != null)
      { boxFrom = borderFrom.Extent; }

      IBox boxTo = null;
      if (borderTo != null)
      { boxTo = borderTo.Extent; }

      if (boxFrom != null && boxTo != null &&
        streckeFrom != streckeTo)
      {
        foreach (var pair in _transport)
        {
          Polyline trans = pair.Key;
          if (BoxOp.IsWithin(boxFrom, trans.Points[0])
            && BoxOp.IsWithin(boxTo, trans.Points.Last()))
          {
            t.Elements.AddRange(CombineTransport(trans, pair.Value));
            return t;
          }
        }
      }
      else if (boxFrom != null && streckeFrom == streckeTo)
      {
        foreach (var pair in _transport)
        {
          if (pair.Value != SymT.TransportHilf)
          { continue; }

          Polyline trans = pair.Key;
          if (BoxOp.IsWithin(boxFrom, trans.Points[0]))
          {
            t.Elements.AddRange(CombineTransport(trans, SymT.Transport));
            return t;
          }
        }
      }
      else if (boxTo != null && streckeFrom == streckeTo)
      {
        foreach (var pair in _transport)
        {
          if (pair.Value != SymT.TransportHilf)
          { continue; }

          Polyline trans = pair.Key;
          if (BoxOp.IsWithin(boxTo, trans.Points.Last()))
          {
            t.Elements.AddRange(CombineTransport(trans, SymT.Transport));
            return t;
          }
        }
      }
      return null;
    }

    public Categorie Categorie(int strecke, Kategorie kategorie)
    {
      return Strecken[strecke].GetCategorie(kategorie);
    }

    private IList<GeoElement> CombineTransport(Polyline trans, int symbol)
    {
      List<GeoElement> elemList = new List<GeoElement>();

      GeoElement elem;
      elem = new GeoElement(trans);
      elem.Type = GeomType.line;
      elem.Symbol = symbol;
      elemList.Add(elem);

      IPoint s = trans.Points[0];
      IPoint e = trans.Points.Last();

      //TODO
      bool next = true;
      int currentFirstSymbol = symbol;
      int currentLastSymbol = symbol;
      while (next != false)
      {
        next = false;
        foreach (var pair in _transport)
        {
          symbol = pair.Value;

          Polyline p = pair.Key;
          if (symbol != currentFirstSymbol && PointOp.Dist2(p.Points.Last(), s) < 20.0)
          {
            next = true;
            s = p.Points[0];
            currentFirstSymbol = symbol;
          }

          if (symbol != currentLastSymbol && PointOp.Dist2(p.Points[0], e) < 20.0)
          {
            next = true;
            e = p.Points.Last();
            currentLastSymbol = symbol;
          }

          if (next)
          {
            elem = new GeoElement(p);
            elem.Type = GeomType.line;
            elem.Symbol = symbol;
            elemList.Add(elem);

            break;
          }
        }
      }
      return elemList;
    }

    private class PointCmpr : IComparer<IPoint>
    {
      private readonly double _dist;
      public PointCmpr(double dist)
      {
        _dist = dist;
      }
      public int Compare(IPoint x, IPoint y)
      {
        double d;

        d = x.X - y.X;
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

    protected override XmlElement CreateLookAt(XmlDocument doc)
    {
      XmlElement elem = doc.CreateElement("LookAt");
      XmlUtils.AppendElement(doc, elem, "longitude", "8.5750");
      XmlUtils.AppendElement(doc, elem, "latitude", "47.3400");
      XmlUtils.AppendElement(doc, elem, "altitude", "1000");
      XmlUtils.AppendElement(doc, elem, "range", "22888");
      XmlUtils.AppendElement(doc, elem, "tilt", "26");
      XmlUtils.AppendElement(doc, elem, "heading", "17");
      return elem;
    }

    private static void WriteStrecke(XmlDocument doc, XmlNode parent,
      int iStrecke, SolaCategorie cat,
      TransferProjection prj)
    {
      XmlElement elem;
      XmlAttribute attr;

      Polyline strecke = cat.Strecke;
      Polyline s = strecke.Linearize(3.0);
      s = s.Project(prj);
      StringBuilder builder = new StringBuilder();
      foreach (var p in s.Points)
      {
        builder.AppendFormat("{0:F6},{1:F6},0 ", p.X, p.Y);
      }

      elem = doc.CreateElement("Placemark");
      attr = doc.CreateAttribute("name");
      attr.Value = "Strecke " + (iStrecke + 1);
      elem.Attributes.Append(attr);

      string k = string.Format("{0}", cat.Typ);
      k = k.Replace("Default, ", "");
      attr = doc.CreateAttribute("description");
      attr.Value = string.Format("{1}{0}L�nge {2:N2} km{0}Steigung {3:N0} m",
        Environment.NewLine,
        k, cat.DispLength / 1000.0,
        cat.SteigungRound(5.0));
      elem.Attributes.Append(attr);

      attr = doc.CreateAttribute("styleUrl");
      if ((cat.Typ & Kategorie.Default) == Kategorie.Default)
      { attr.Value = "#" + cat.Stufe; }
      else
      { attr.Value = "#special"; }
      elem.Attributes.Append(attr);

      parent.AppendChild(elem);

      XmlElement line = doc.CreateElement("LineString");
      attr = doc.CreateAttribute("extrude");
      attr.Value = "1";
      line.Attributes.Append(attr);
      attr = doc.CreateAttribute("tessellate");
      attr.Value = "1";
      line.Attributes.Append(attr);
      attr = doc.CreateAttribute("coordinates");
      attr.Value = builder.ToString();
      line.Attributes.Append(attr);

      elem.AppendChild(line);
    }

    protected override void WriteMarks(XmlDocument doc, XmlNode parent,
      SortedList<IPoint, List<int>> marks,
      TransferProjection prj)
    {
      XmlElement elem;

      foreach (var pair in marks)
      {
        IList<int> strecken = pair.Value;

        string desc = "";
        string name = "";
        foreach (var i in strecken)
        {
          if (string.IsNullOrEmpty(desc) == false)
          {
            desc += Environment.NewLine;
          }
          if (i < 0)
          {
            desc += string.Format("Ziel {0}. Strecke", -i);
            name = Ddx.Uebergabe[-i].Name;
          }
          else
          {
            desc += string.Format("Start {0}. Strecke", i);
            name = Ddx.Uebergabe[i - 1].Name;
          }
        }

        elem = doc.CreateElement("Placemark");
        XmlUtils.AppendElement(doc, elem, "name", name);
        XmlUtils.AppendElement(doc, elem, "description", desc);
        XmlUtils.AppendElement(doc, elem, "styleUrl", "#pin");

        parent.AppendChild(elem);

        KmlUtils.AppendPoint(elem, pair.Key, prj);
      }
    }

  }
}
