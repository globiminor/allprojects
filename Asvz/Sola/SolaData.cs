using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Ocad;
using Basics.Geom;
using Basics.Geom.Projection;

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
      foreach (SolaStrecke info in Strecken)
      {
        foreach (SolaCategorie cat in info.Categories)
        {
          if ((cat.Typ & Kategorie.Default) == Kategorie.Default)
          {
            Debug.Assert(cat.Strecke != null);
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

      IPoint start = defaultStrecke.Points.First.Value;
      IPoint end = defaultStrecke.Points.Last.Value;
      Polyline line0 = null;
      foreach (Element elem in reader.Elements(true, indexList))
      {
        Polyline line = (Polyline)elem.Geometry;
        if (PointOperator.Dist2(line.Points.First.Value, start) < 100 &&
          PointOperator.Dist2(line.Points.Last.Value, end) < 100)
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

      IList<Element> strecken = new List<Element>();
      IList<Element> nummern = new List<Element>();

      _transport = new Dictionary<Polyline, int>();

      foreach (Element elem in reader.Elements(true, pIndexList))
      {
        if (elem.Symbol == SymT.Strecke)
        { strecken.Add(elem); }
        else if (elem.Symbol == SymT.TextStrecke)
        {
          int iStrecke;
          if (int.TryParse(elem.Text, out iStrecke))
          { nummern.Add(elem); }
        }
        else if (elem.Symbol == SymT.Transport)
        { _transport.Add((Polyline)elem.Geometry, elem.Symbol); }
        else if (elem.Symbol == SymT.TransportUi)
        { _transport.Add((Polyline)elem.Geometry, elem.Symbol); }
        else if (elem.Symbol == SymT.TransportHilf)
        { _transport.Add((Polyline)elem.Geometry, elem.Symbol); }
        else if (elem.Symbol == SymT.LinieBreit)
        { IndexList.Add((Polyline)elem.Geometry); }
        else if (elem.Symbol == SymT.Verpflegung)
        { VerpfList.Add((Point)elem.Geometry); }
        else if (elem.Symbol == SymT.Wald)
        { Wald.Add((Area)elem.Geometry); }
        else if (elem.Symbol == SymT.Siedlung)
        { Siedlung.Add((Area)elem.Geometry); }
        else if (elem.Symbol == SymT.Teer)
        { Teer.Add((Area)elem.Geometry); }
      }
      Debug.Assert(strecken.Count == Ddx.Strecken.Count);
      Debug.Assert(nummern.Count == Ddx.Strecken.Count);

      SortDefaultStrecken(strecken, nummern);
    }

    protected override void CreateStyles(XmlDocument doc, XmlNode dc)
    {
      XmlElement style;

      style = GetStyle(doc, "einfach", "C000ff00", 3);
      dc.AppendChild(style);

      style = GetStyle(doc, "mittel", "C00080ff", 3);
      dc.AppendChild(style);

      style = GetStyle(doc, "schwierig", "C00000ff", 3);
      dc.AppendChild(style);

      style = GetStyle(doc, "special", "80ff0000", 5);
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
      AppendElement(doc, icon, "href", "sola.png");
      //attr.Value = "http://maps.google.com/mapfiles/kml/pal3/icon21.png";

      iconStyle.AppendChild(icon);
      style.AppendChild(iconStyle);

      dc.AppendChild(style);
    }

    private void SortDefaultStrecken(IList<Element> strecken, IList<Element> nummern)
    {
      foreach (SolaStrecke rawStrecke in Ddx.Strecken)
      {
        SolaStrecke strecke = new SolaStrecke(rawStrecke.Nummer, rawStrecke.Vorlage);
        foreach (SolaCategorie rawCat in rawStrecke.Categories)
        {
          SolaCategorie cat = new SolaCategorie(rawCat.Typ,
            rawCat.Distance, rawCat.OffsetStart, rawCat.OffsetEnd,
            rawCat.Stufe);
          strecke.Categories.Add(cat);
        }
        Strecken.Add(strecke);
      }

      for (int i = 0; i < Strecken.Count; i++)
      {
        Element pStrecke = nummern[i];
        int iStrecke = Convert.ToInt32(pStrecke.Text);
        Point pNummer = 0.5 * (pStrecke.Geometry.Extent.Max +
          (Point)pStrecke.Geometry.Extent.Min);

        double dMinMin = -1;
        int iMin = -1;
        for (int iGeom = 0; iGeom < Strecken.Count; iGeom++)
        {
          Polyline pLine = (strecken[iGeom]).Geometry as Polyline;
          double dMin = -1;
          foreach (Curve pSeg in pLine.Segments)
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
          (Polyline)((Element)strecken[iMin]).Geometry, this);
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
      foreach (int iPos in pIndexList)
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

      foreach (Polyline idx in IndexList)
      {
        double t0 = -idx.Segments.First.ParamAt(DistIndex);
        double l_1 = idx.Segments.Last.Length();
        double t1 = 1 + idx.Segments.Last.ParamAt(DistIndex);
        Polyline cross = idx.Clone();
        cross.AddFirst(idx.Segments.First.PointAt(t0));
        cross.Add(idx.Segments.Last.PointAt(t1));

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
    public IList<Element> Transport(int streckeFrom, int streckeTo)
    {
      if (streckeFrom < 0 || streckeTo < 0)
      { return null; }
      if (streckeFrom >= Ddx.Uebergabe.Count || streckeTo >= Ddx.Uebergabe.Count)
      { return null; }

      Point legendPos;
      Polyline borderFrom;
      Sola.Transport.GetLayout(streckeFrom, out borderFrom, out legendPos);

      Polyline borderTo;
      Sola.Transport.GetLayout(streckeTo, out borderTo, out legendPos);


      IBox boxFrom = null;
      if (borderFrom != null)
      { boxFrom = borderFrom.Extent; }

      IBox boxTo = null;
      if (borderTo != null)
      { boxTo = borderTo.Extent; }

      if (boxFrom != null && boxTo != null &&
        streckeFrom != streckeTo)
      {
        foreach (KeyValuePair<Polyline, int> pair in _transport)
        {
          Polyline trans = pair.Key;
          if (boxFrom.IsWithin(trans.Points.First.Value)
            && boxTo.IsWithin(trans.Points.Last.Value))
          {
            return CombineTransport(trans, pair.Value);
          }
        }
      }
      else if (boxFrom != null && streckeFrom == streckeTo)
      {
        foreach (KeyValuePair<Polyline, int> pair in _transport)
        {
          if (pair.Value != SymT.TransportHilf)
          { continue; }

          Polyline trans = pair.Key;
          if (boxFrom.IsWithin(trans.Points.First.Value))
          { return CombineTransport(trans, SymT.Transport); }
        }
      }
      else if (boxTo != null && streckeFrom == streckeTo)
      {
        foreach (KeyValuePair<Polyline, int> pair in _transport)
        {
          if (pair.Value != SymT.TransportHilf)
          { continue; }

          Polyline trans = pair.Key;
          if (boxTo.IsWithin(trans.Points.Last.Value))
          { return CombineTransport(trans, SymT.Transport); }
        }
      }
      return null;
    }

    public Categorie Categorie(int strecke, Kategorie kategorie)
    {
      return Strecken[strecke].GetCategorie(kategorie);
    }

    private IList<Element> CombineTransport(Polyline trans, int symbol)
    {
      List<Element> elemList = new List<Element>();

      ElementV9 elem;
      elem = new ElementV9(true);
      elem.Geometry = trans;
      elem.Type = GeomType.line;
      elem.Symbol = symbol;
      elemList.Add(elem);

      IPoint s = trans.Points.First.Value;
      IPoint e = trans.Points.Last.Value;

      //TODO
      bool next = true;
      int currentFirstSymbol = symbol;
      int currentLastSymbol = symbol;
      while (next != false)
      {
        next = false;
        foreach (KeyValuePair<Polyline, int> pair in _transport)
        {
          symbol = pair.Value;

          Polyline p = pair.Key;
          if (symbol != currentFirstSymbol && PointOperator.Dist2(p.Points.Last.Value, s) < 20.0)
          {
            next = true;
            s = p.Points.First.Value;
            currentFirstSymbol = symbol;
          }

          if (symbol != currentLastSymbol && PointOperator.Dist2(p.Points.First.Value, e) < 20.0)
          {
            next = true;
            e = p.Points.Last.Value;
            currentLastSymbol = symbol;
          }

          if (next)
          {
            elem = new ElementV9(true);
            elem.Geometry = p;
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
      private double _dist;
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
      AppendElement(doc, elem, "longitude", "8.5750");
      AppendElement(doc, elem, "latitude", "47.3400");
      AppendElement(doc, elem, "altitude", "1000");
      AppendElement(doc, elem, "range", "22888");
      AppendElement(doc, elem, "tilt", "26");
      AppendElement(doc, elem, "heading", "17");
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
      foreach (Point p in s.Points)
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
      attr.Value = string.Format("{1}{0}Länge {2:N2} km{0}Steigung {3:N0} m",
        Environment.NewLine,
        k, cat.Laenge() / 1000.0,
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

      foreach (KeyValuePair<IPoint, List<int>> pair in marks)
      {
        IList<int> strecken = pair.Value;

        string desc = "";
        string name = "";
        foreach (int i in strecken)
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
        AppendElement(doc, elem, "name", name);
        AppendElement(doc, elem, "description", desc);
        AppendElement(doc, elem, "styleUrl", "#pin");

        parent.AppendChild(elem);

        XmlElement pp = doc.CreateElement("Point");
        AppendElement(doc, pp, "extrude", "1");

        IPoint x = pair.Key;
        x = x.Project(prj);
        AppendElement(doc, pp, "coordinates", string.Format("{0:F6},{1:F6},0 ", x.X, x.Y));

        elem.AppendChild(pp);
      }
    }

  }
}
