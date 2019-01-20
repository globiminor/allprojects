using System;
using System.Collections.Generic;
using System.Diagnostics;
using Basics.Geom;
using Ocad;
using System.IO;
using Ocad.StringParams;
using Basics.Geom.Projection;
using System.Xml;

namespace Asvz.Sola
{
  public class Transport
  {
    private List<GeoElement> _sorted;
    private Polyline _combined;

    public int From { get; private set; }
    public int To { get; private set; }
    public List<GeoElement> Elements { get; private set; }

    public Polyline Combined { get { return _combined ?? (_combined = GetCombined()); } }
    public List<GeoElement> SortedElements { get { return _sorted ?? (_sorted = SortElements()); } }

    public Transport(int from, int to)
    {
      From = from;
      To = to;
      Elements = new List<GeoElement>();
    }

    private Polyline GetCombined()
    {
      Polyline combined = new Polyline();
      foreach (var elem in SortedElements)
      {
        Polyline add = (Polyline)elem.Geometry;
        combined.Add(add.Points.First.Value);
        foreach (var seg in add.Segments)
        { combined.Add(seg); }
      }
      return combined;
    }

    private List<GeoElement> SortElements()
    {
      Dictionary<Element, GeoElement> lines = new Dictionary<Element, GeoElement>();
      foreach (var elem in Elements)
      { lines.Add(elem, null); }

      List<GeoElement> startLines = new List<GeoElement>();
      foreach (var elem in Elements)
      {
        Point start = Point.CastOrCreate(((Polyline)elem.Geometry).Points.First.Value);
        double minDist = double.MaxValue;
        Element pre = null;
        foreach (var nb in Elements)
        {
          IPoint end = ((Polyline)nb.Geometry).Points.Last.Value;
          double d2 = start.Dist2(end);
          if (d2 < 2000 && d2 < minDist)
          {
            minDist = d2;
            pre = nb;
          }
        }
        if (pre != null)
        { lines[pre] = elem; }
        else
        { startLines.Add(elem); }
      }
      if (startLines.Count == 0)
      { throw new InvalidOperationException("No start element found"); }
      if (startLines.Count > 1)
      { throw new InvalidOperationException(string.Format("Found {0} start elements", startLines.Count)); }

      List<GeoElement> sorted = new List<GeoElement>();
      GeoElement next = startLines[0];
      while (next != null)
      {
        if (sorted.Contains(next))
        { throw new InvalidOperationException("Element already added"); }
        sorted.Add(next);
        if (!lines.TryGetValue(next, out next))
        { next = null; }
      }
      if (sorted.Count != Elements.Count)
      { throw new InvalidOperationException(string.Format("Sorted has {0} elements, expected", sorted.Count, Elements.Count)); }

      return sorted;
    }

    public void ExportKml(string path, string from, string to)
    {
      XmlElement dc = KmlUtils.InitDoc("SOLA Kleidertransport");
      XmlDocument doc = dc.OwnerDocument;

      TransferProjection prj = KmlUtils.GetTransferProjection(new Ch1903());
      Projection wgs = new Geographic();

      XmlElement style = KmlUtils.GetStyle(doc, "transport", "C0ff0000", 3);
      dc.AppendChild(style);

      XmlElement elem = doc.CreateElement("Placemark");
      XmlUtils.AppendElement(doc, elem, "name", "Transport");

      XmlUtils.AppendElement(doc, elem, "description",
        string.Format("Kleidertransport von {0} nach {1}", from, to));

      XmlUtils.AppendElement(doc, elem, "styleUrl", "#" + "transport");

      dc.AppendChild(elem);

      KmlUtils.AppendLine(elem, Combined, prj);
      doc.Save(path);
    }
    public void ExportGpx(string path)
    {
      TransferProjection prj = GpxUtils.GetTransferProjection(new Ch1903());

      Trk trk = new Trk { Segments = new List<TrkSeg>() };
      TrkSeg seg = GpxUtils.GetStreckeGpx(Combined, prj);
      trk.Segments.Add(seg);

      Gpx gpx = new Gpx { Trk = trk };
      GpxUtils.Write(path, gpx);
    }
  }

  public class UebergabeTransport
  {
    public enum Typ { Normal, Spezial, Kein }
    public class Info
    {
      private Typ _vonTyp = Typ.Normal;
      private Typ _nachTyp = Typ.Normal;
      private int _von = -1;
      private int _nach = -1;

      public Typ VonTyp
      {
        get { return _vonTyp; }
        set { _vonTyp = value; }
      }
      public Typ NachTyp
      {
        get { return _nachTyp; }
        set { _nachTyp = value; }
      }
      public int Von
      {
        get { return _von; }
        set { _von = value; }
      }
      public int Nach
      {
        get { return _nach; }
        set { _nach = value; }
      }
      public int GetVonStrecke(int idStrecke)
      {
        if (_vonTyp == Typ.Kein)
        { return -1; }
        else if (_von >= 0)
        { return _von; }
        else if (_vonTyp == Typ.Normal)
        { return idStrecke - 1; }
        else if (_vonTyp == Typ.Spezial)
        { return idStrecke; }
        else
        { throw new InvalidProgramException("Not handled"); }
      }
      public int GetNachStrecke(int idStrecke)
      {
        if (_nachTyp == Typ.Kein)
        { return -1; }
        else if (_nach >= 0)
        { return _nach; }
        else if (_nachTyp == Typ.Normal)
        { return idStrecke + 1; }
        else if (_nachTyp == Typ.Spezial)
        { return idStrecke; }
        else
        { throw new InvalidProgramException("Not handled"); }
      }
    }

    private Polyline _transportBox;
    private Point _legendPos;

    private Polyline _symUebergabe;
    private Polyline _symUeCircle;

    private Ocad.Symbol.TextSymbol _textSymbol;
    private Ocad.Symbol.TextSymbol _legendText;
    private Ocad.Symbol.PointSymbol _legendBox;

    private IList<GeoElement> _textList;

    // string parameters
    private ViewPar _viewParam;
    private ExportPar _exportParam;
    private PrintPar _printParam;
    private string _mapTemplate;

    private readonly int[] _symbols = {
        SymD.UeNeustart, SymD.UeCircle, SymD.TrAusschnitt,
        SymD.GepBleibt, SymD.GepVon, SymD.GepNach, SymD.Sanitaet,
        SymD.Bus, SymD.Tram, SymD.Bahn, SymD.Lsb,
        SymD.IdxRot, SymD.IdxSchwarz, SymD.UeAusserhalb, SymD.Objekt,
        SymD.TextGross, SymD.TextMittel, SymD.BoxText,
        SymD.Abfahrt, SymD.AbfahrtU, SymD.Anfahrt, SymD.AnfahrtU,
        SymD.TrLegendeBox, SymD.TrTextLegende, SymD.TrMassstab, SymD.TrNordpfeil
      };

    private readonly Polyline _runFrom;
    private readonly Polyline _runTo;
    private readonly Transport _transFrom;
    private readonly Transport _transTo;
    private readonly int _strecke;

    private Setup _templateSetup;

    public static void GetLayout(int strecke, out Polyline box, out Point legendPos)
    {
      if (strecke >= 0)
      {
        GetLayout(strecke, out box, out legendPos, true);
      }
      else
      {
        box = null;
        legendPos = null;
      }
    }

    public Transport TransFrom { get { return _transFrom; } }
    public Transport TransTo { get { return _transTo; } }

    public static void GetLayout(int strecke, out Polyline box, out Point legendPos,
      bool checkEqualUebergabe)
    {
      UebergabeTransport t = new UebergabeTransport(null, strecke);
      string template = Ddx.Uebergabe[strecke].Vorlage;

      OcadReader pTemplate = OcadReader.Open(template);
      t.ReadTemplate(pTemplate);

      box = t._transportBox;
      legendPos = t._legendPos;

      if (checkEqualUebergabe && box == null)
      {
        string name = Ddx.Uebergabe[strecke].Name;
        strecke = 0;
        while (box == null && strecke < Ddx.Uebergabe.Count)
        {
          if (Ddx.Uebergabe[strecke].Name == name)
          { GetLayout(strecke, out box, out legendPos, false); }
          strecke++;
        }
      }
    }

    public UebergabeTransport(SolaData data, int idStrecke)
    {
      if (data == null)
      { return; }

      _strecke = idStrecke;

      IList<SolaStrecke> strecken = data.Strecken;
      if (strecken == null)
      { return; }

      Info transInfo = Ddx.Uebergabe[idStrecke].Transport;
      int vonStrecke;
      int nachStrecke;
      if (transInfo == null)
      {
        vonStrecke = idStrecke - 1;
        nachStrecke = idStrecke + 1;
      }
      else
      {
        vonStrecke = transInfo.GetVonStrecke(idStrecke);
        nachStrecke = transInfo.GetNachStrecke(idStrecke);
      }


      _transFrom = data.Transport(vonStrecke, idStrecke);
      if (idStrecke > 0)
      { _runFrom = strecken[idStrecke - 1].GetCategorie(Kategorie.Default).Strecke; }
      else
      { _runFrom = null; }

      _transTo = data.Transport(idStrecke, nachStrecke);
      if (idStrecke < strecken.Count)
      { _runTo = strecken[idStrecke].GetCategorie(Kategorie.Default).Strecke; }
      else
      { _runTo = null; }
    }

    public void CompleteStrecken()
    {
      string karte = Ddx.Uebergabe[_strecke].Karte;

      foreach (var uebergabe in Ddx.Uebergabe)
      {
        if (uebergabe.Karte == karte)
        {
          string template = uebergabe.Vorlage;

          OcadReader pTemplate = OcadReader.Open(template);
          ReadTransport(pTemplate);
          pTemplate.Close();
        }
      }
    }

    public void Write(string outFile)
    {
      string template = Ddx.Uebergabe[_strecke].Vorlage;

      File.Copy(template, outFile, true);
      OcadWriter writer = OcadWriter.AppendTo(outFile);

      OcadReader pTemplate = OcadReader.Open(template);
      ReadTemplate(pTemplate);
      pTemplate.Close();
      if (_transportBox == null)
      {
        string name = Ddx.Uebergabe[_strecke].Name;
        int strecke = 0;
        while (_transportBox == null && strecke < Ddx.Uebergabe.Count)
        {
          if (Ddx.Uebergabe[strecke].Name == name)
          { GetLayout(strecke, out _transportBox, out _legendPos); }
          strecke++;
        }
      }

      try
      {

        //pWriter.DeleteElements(
        //  new int[] { SymD.Abfahrt, SymD.AbfahrtU, SymD.Anfahrt, SymD.AnfahrtU, SymD.TrAusschnitt });
        writer.DeleteElements(
          new[] { SymD.TrAusschnitt,
            SymD.Abfahrt, SymD.AbfahrtU, SymD.Anfahrt, SymD.AnfahrtU});

        DelElements del = new DelElements(writer);
        writer.DeleteElements(del.DeleteSpecificElements);

        if (_transportBox != null)
        {
          GetBorder(_transportBox, out Polyline partEnd, out Polyline partStart,
            out IPoint centerEnd, out IPoint centerStart);

          WriteStartEnd(writer, centerStart, centerEnd, _transportBox);

          AddLegend(writer, _transportBox, _legendPos);

          AdaptText(writer, _templateSetup, _textList, _textSymbol);

          writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
          writer.SymbolsSetState(_symbols, Ocad.Symbol.SymbolStatus.Normal);

          writer.Remove(StringType.Template);

          WriteParams(writer, _transportBox);
        }

        WriteKarte(writer);
      }
      finally
      {
        writer.Close();
      }
    }

    private class DelElements
    {
      private readonly OcadReader _reader;
      public DelElements(OcadReader reader)
      {
        _reader = reader;
      }
      public bool DeleteSpecificElements(ElementIndex elemIdx)
      {
        if (elemIdx == null)
        { return false; }

        if (elemIdx.Symbol == SymD.TextMittel)
        {
          _reader.ReadElement(elemIdx, out MapElement element);
          if (element.Text.StartsWith("von"))
          { return true; }
          if (element.Text.StartsWith("nach"))
          { return true; }
        }
        return false;
      }
    }

    private void WriteKarte(OcadWriter writer)
    {
      TemplatePar tplPar;

      if (_mapTemplate != null)
      { tplPar = new TemplatePar(_mapTemplate); }
      else
      {
        tplPar = new TemplatePar();
        string sTmpl = Ddx.Uebergabe[_strecke].Karte;
        tplPar.Name = sTmpl;

        string sWorldFile = Path.GetDirectoryName(sTmpl) + Path.DirectorySeparatorChar +
          Path.GetFileNameWithoutExtension(sTmpl) + ".tfw";
        if (File.Exists(sWorldFile))
        {
          Grid.ImageGrid grd = Grid.ImageGrid.FromFile(sTmpl);
          int nx = grd.Extent.Nx;
          int ny = grd.Extent.Nx;
          grd.Close();

          TextReader pWorld = new StreamReader(sWorldFile);

          tplPar.Georeference(
            Convert.ToDouble(pWorld.ReadLine()),
            Convert.ToDouble(pWorld.ReadLine()),
            Convert.ToDouble(pWorld.ReadLine()),
            Convert.ToDouble(pWorld.ReadLine()),
            Convert.ToDouble(pWorld.ReadLine()),
            Convert.ToDouble(pWorld.ReadLine()),
            nx, ny, writer.Setup);

          pWorld.Close();
        }
      }
      tplPar.Dim = 30;
      tplPar.Transparent = false;
      tplPar.Visible = true;

      writer.Overwrite(StringType.Template, 0, tplPar.StringPar);
    }

    private void WriteParams(OcadWriter writer, Polyline border)
    {
      if (_viewParam != null)
      {
        Point pViewCenter = 0.5 * (Point.Create(border.Extent.Min) +
          Point.Create(border.Extent.Max));
        pViewCenter = pViewCenter.Project(_templateSetup.Prj2Map);
        _viewParam.XOffset = pViewCenter.X / 100;
        _viewParam.YOffset = pViewCenter.Y / 100;

        writer.Overwrite(StringType.ViewPar, 0, _viewParam.StringPar);
      }

      if (_printParam == null)
      { _printParam = new PrintPar(); }
      if (_printParam != null)
      {
        Polyline pLine = border.Project(_templateSetup.Prj2Map);
        _printParam.Scale = 6000;
        _printParam.Range = PrintPar.RangeType.PartialMap;
        _printParam.Left = pLine.Extent.Min.X / 100;
        _printParam.Right = pLine.Extent.Max.X / 100;
        _printParam.Top = pLine.Extent.Max.Y / 100;
        _printParam.Bottom = pLine.Extent.Min.Y / 100;

        writer.Overwrite(StringType.PrintPar, 0, _printParam.StringPar);
      }
      if (_exportParam == null)
      { _exportParam = new ExportPar(); }
      if (_exportParam != null)
      {
        _exportParam.Resolution = 75.0;
        writer.Overwrite(StringType.Export, 0, _exportParam.StringPar);
      }
    }

    private void WriteStartEnd(OcadWriter writer,
      IPoint centerStart, IPoint centerEnd, Polyline transportBox)
    {
      if (centerEnd != null)
      {
        Element pElem = new GeoElement(centerEnd);
        pElem.Symbol = SymD.UeCircle;
        pElem.Type = GeomType.point;
        writer.Append(pElem);
      }
      if (centerStart != null && centerStart != centerEnd)
      {
        Element pElem = new GeoElement(centerStart);
        pElem.Symbol = SymD.UeCircle;
        pElem.Type = GeomType.point;
        writer.Append(pElem);
      }

      IBox full = transportBox.Extent;
      Point center = 0.5 * PointOperator.Add(full.Max, full.Min);
      Polyline border = new Polyline();
      foreach (var point in transportBox.Points)
      { border.Add(center + 1.5 * (point - center)); }
      Area box = new Area(border);

      if (_transFrom != null)
      {
        foreach (var elem in _transFrom.Elements)
        {
          GeometryCollection col = GeometryOperator.Intersection(elem.Geometry, box);
          if (col == null)
          { continue; }
          foreach (var part in col)
          {
            GeoElement pElem = new GeoElement(part);
            pElem.Type = GeomType.line;
            if (elem.Symbol == SymT.Transport)
            { pElem.Symbol = SymD.Anfahrt; }
            else if (elem.Symbol == SymT.TransportUi)
            { pElem.Symbol = SymD.AnfahrtU; }
            else if (elem.Symbol == SymT.TransportHilf)
            { continue; }
            else
            { throw new ArgumentException("Unhandled Symbol " + pElem.Symbol); }
            writer.Append(pElem);
          }
        }
      }
      if (_transTo != null)
      {
        foreach (var elem in _transTo.Elements)
        {
          GeometryCollection col = GeometryOperator.Intersection(elem.Geometry, box);
          if (col == null)
          { continue; }
          foreach (var part in col)
          {
            GeoElement pElem = new GeoElement(part);
            pElem.Type = GeomType.line;
            if (elem.Symbol == SymT.Transport)
            { pElem.Symbol = SymD.Abfahrt; }
            else if (elem.Symbol == SymT.TransportUi)
            { pElem.Symbol = SymD.AbfahrtU; }
            else if (elem.Symbol == SymT.TransportHilf)
            { continue; }
            else
            { throw new ArgumentException("Unhandled Symbol " + pElem.Symbol); }
            writer.Append(pElem);
          }
        }
      }
    }

    private void GetBorder(Polyline border, out Polyline partEnd, out Polyline partStart,
      out IPoint centerEnd, out IPoint centerStart)
    {
      IList<ParamGeometryRelation> pEnd = null;
      IList<ParamGeometryRelation> pStart = null;

      partEnd = null;
      partStart = null;

      if (_runFrom != null)
      {
        pEnd = GeometryOperator.CreateRelations(border, _runFrom);
        Trace.Assert(pEnd != null);
      }

      if (_runTo != null)
      {
        pStart = GeometryOperator.CreateRelations(border, _runTo);
        Trace.Assert(pStart != null);
      }

      ParamGeometryRelation pCutBorder;
      ParamGeometryRelation pCutCircle;

      centerEnd = null;
      centerStart = null;

      if (_runFrom != null)
      { centerEnd = _runFrom.Points.Last.Value; }
      if (_runTo != null)
      { centerStart = _runTo.Points.First.Value; }

      if (centerEnd != null && centerStart != null && PointOperator.Dist2(centerEnd, centerStart) < 100)
      { centerStart = centerEnd; }

      Polyline pCircle = null;

      if (pEnd != null)
      {
        pCutBorder = pEnd[pEnd.Count - 1];
        pCircle = Circle(centerEnd);
        IList<ParamGeometryRelation> pList = GeometryOperator.CreateRelations(_runFrom, pCircle);
        pCutCircle = pList[pList.Count - 1];
        partEnd = _runFrom.Split(new[] { pCutBorder, pCutCircle })[1];
      }
      if (pStart != null)
      {
        pCutBorder = pStart[0];
        if (centerStart != centerEnd)
        { pCircle = Circle(centerStart); }
        IList<ParamGeometryRelation> pList = GeometryOperator.CreateRelations(_runTo, pCircle);
        pCutCircle = pList[0];
        partStart = _runTo.Split(new[] { pCutCircle, pCutBorder })[1];
      }
    }

    private Polyline BorderGeometry(GeoElement border)
    {
      Setup pSetup = new Setup();
      Point p = (Point)border.Geometry;
      pSetup.PrjTrans.X = p.X;
      pSetup.PrjTrans.Y = p.Y;
      pSetup.Scale = 1 / FileParam.OCAD_UNIT;
      pSetup.PrjRotation = border.Angle + _templateSetup.PrjRotation;
      Polyline pBorder = _symUebergabe.Project(pSetup.Map2Prj);
      return pBorder;
    }

    private Polyline Circle(IPoint center)
    {
      Setup centerPrj = new Setup();
      centerPrj.PrjTrans.X = center.X;
      centerPrj.PrjTrans.Y = center.Y;
      centerPrj.Scale = 1.0 / FileParam.OCAD_UNIT;
      return _symUeCircle.Project(centerPrj.Map2Prj);
    }

    private void ReadTransport(OcadReader template)
    {
      template.ReadSetup();
      Dictionary<int, Ocad.Symbol.BaseSymbol> symbols = ReadTransportSymbols(template);
      IList<GeoElement> elements = ReadTransportObjects(template);
      GetGepaeck(symbols, elements);
    }

    private void ReadTemplate(OcadReader template)
    {
      _templateSetup = template.ReadSetup();

      ReadSymbols(template);
      ReadObjects(template);
      ReadStringParams(template);
    }

    private void ReadObjects(OcadReader template)
    {
      ElementIndex pIndex;
      IList<ElementIndex> pIndexList = new List<ElementIndex>();
      int iIndex = 0;
      while ((pIndex = template.ReadIndex(iIndex)) != null)
      {
        pIndexList.Add(pIndex);
        iIndex++;
      }

      _transportBox = null;
      _legendPos = null;
      _textList = new List<GeoElement>();

      foreach (var elem in template.EnumGeoElements(pIndexList))
      {
        if (elem.Symbol == SymD.TrAusschnitt)
        {
          Trace.Assert(_transportBox == null, "Multiple Transport");
          _transportBox = BorderGeometry(elem);
        }
        else if (elem.Symbol == SymD.TrNordRoh)
        {
          Trace.Assert(_legendPos == null, "Multiple Transport Legendeposition");
          _legendPos = (Point)elem.Geometry;
        }
        else if (elem.Symbol == SymD.TextGross)
        { _textList.Add(elem); }
      }
    }

    private List<GeoElement> ReadTransportObjects(OcadReader template)
    {
      ElementIndex pIndex;
      IList<ElementIndex> pIndexList = new List<ElementIndex>();
      int iIndex = 0;
      while ((pIndex = template.ReadIndex(iIndex)) != null)
      {
        pIndexList.Add(pIndex);
        iIndex++;
      }

      List<GeoElement> list = new List<GeoElement>();

      foreach (var elem in template.EnumGeoElements(pIndexList))
      {
        int symbol = elem.Symbol;
        if (symbol == SymD.GepVon || symbol == SymD.GepNach || symbol == SymD.GepBleibt
          || symbol == SymD.IdxSchwarz || symbol == SymD.Objekt)
        {
          list.Add(elem);
        }
      }
      return list;
    }

    private void GetGepaeck(Dictionary<int, Ocad.Symbol.BaseSymbol> symbols, IList<GeoElement> elements)
    {
      GetGepaeck(symbols, elements, SymD.GepVon);
      GetGepaeck(symbols, elements, SymD.GepNach);
      GetGepaeck(symbols, elements, SymD.GepBleibt);
    }

    private void GetGepaeck(Dictionary<int, Ocad.Symbol.BaseSymbol> symbols, IList<GeoElement> elements, int symbol)
    {
      GeoElement gepaeck = null;
      foreach (var elem in elements)
      {
        if (elem.Symbol == symbol)
        {
          if (gepaeck != null)
          { throw new InvalidOperationException("Multiple elements found for symbol " + symbol); }
          gepaeck = elem;
        }
      }

      if (gepaeck == null)
      { return; }

      Ocad.Symbol.PointSymbol sym = (Ocad.Symbol.PointSymbol)symbols[symbol];
      IBox box = sym.Graphics.Extent();
      new Box((Point)gepaeck.Geometry + Point.Create(box.Min),
        (Point)gepaeck.Geometry + Point.Create(box.Max));

      foreach (var elem in elements)
      {
        if (elem.Symbol == SymD.IdxSchwarz)
        {
          Polyline line = (Polyline)elem.Geometry;

          ISegment c0 = line.Segments.First;
          ISegment c1 = line.Segments.Last;

          Element elemStart = FindIndexElement(c0.Start, PointOperator.Scale(-1, c0.TangentAt(0)), elements);
          Element elemEnd = FindIndexElement(c1.End, c1.TangentAt(1), elements);
        }
      }

    }
    private Element FindIndexElement(IPoint p0, IPoint direction, IList<GeoElement> elements)
    {
      Element indexElement = null;
      double x0 = 0;
      foreach (var elem in elements)
      {
        if (!(elem.Geometry is Point p))
        { continue; }

        Point d = p - p0;
        IPoint dirPrj = direction.Project(Geometry.ToXY);
        double x = PointOperator.SkalarProduct( dirPrj, d);
        double y = PointOperator.VectorProduct(dirPrj, d);

        if (x > 0 &&
          (indexElement == null || (Math.Abs(y / x) < 0.3 && x < x0)))
        {
          indexElement = elem;
        }
      }
      return indexElement;
    }

    private Dictionary<int, Ocad.Symbol.BaseSymbol> ReadTransportSymbols(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      IList<int> pIndexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        pIndexList.Add(i);
        iIndex++;
      }

      Dictionary<int, Ocad.Symbol.BaseSymbol> list = new Dictionary<int, Ocad.Symbol.BaseSymbol>();

      foreach (var iPos in pIndexList)
      {
        Ocad.Symbol.BaseSymbol pSymbol = reader.ReadSymbol(iPos);
        if (pSymbol == null)
        { continue; }

        if (pSymbol.Number == SymD.GepBleibt)
        { list.Add(SymD.GepBleibt, pSymbol); }
        else if (pSymbol.Number == SymD.GepNach)
        { list.Add(SymD.GepNach, pSymbol); }
        else if (pSymbol.Number == SymD.GepVon)
        { list.Add(SymD.GepVon, pSymbol); }
        else if (pSymbol.Number == SymD.Objekt)
        { list.Add(SymD.Objekt, pSymbol); }
      }

      return list;
    }

    private void ReadSymbols(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      IList<int> pIndexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        pIndexList.Add(i);
        iIndex++;
      }

      Setup pSetup = reader.ReadSetup();
      pSetup.PrjRotation = 0;
      pSetup.PrjTrans.X = 0;
      pSetup.PrjTrans.Y = 0;

      foreach (var iPos in pIndexList)
      {
        Ocad.Symbol.BaseSymbol symbol = reader.ReadSymbol(iPos);
        if (symbol == null)
        { continue; }

        if (symbol.Number == SymD.TextGross)
        { _textSymbol = (Ocad.Symbol.TextSymbol)symbol; }
        if (symbol.Number == SymD.TrTextLegende)
        { _legendText = (Ocad.Symbol.TextSymbol)symbol; }
        if (symbol.Number == SymD.TrLegendeBox)
        { _legendBox = (Ocad.Symbol.PointSymbol)symbol; }

        if (symbol.Number == SymD.TrAusschnitt)
        {
          if (!(symbol.Graphics.Count == 1)) throw new InvalidOperationException($"Expected 1 'TrAusschnitt', got {symbol.Graphics.Count}");
          _symUebergabe = (Polyline)symbol.Graphics[0].MapGeometry.Project(pSetup.Map2Prj);
        }
        else if (symbol.Number == SymD.UeCircle)
        {
          if (!(symbol.Graphics.Count == 1)) throw new InvalidOperationException($"Expected 1 'UeCircle', got {symbol.Graphics.Count}");
          _symUeCircle = (Polyline)symbol.Graphics[0].MapGeometry.Project(pSetup.Map2Prj);
        }

      }
    }

    private void ReadStringParams(OcadReader template)
    {
      IList<StringParamIndex> pStrIdxList = template.ReadStringParamIndices();
      foreach (var pStrIdx in pStrIdxList)
      {
        if (pStrIdx.Type == StringType.Template)
        {
          string sTemp = template.ReadStringParam(pStrIdx);
          TemplatePar tplPar = new TemplatePar(sTemp);
          if (tplPar.Name == Ddx.Uebergabe[_strecke].Karte)
          {
            _mapTemplate = sTemp;
          }
        }
        else if (pStrIdx.Type == StringType.ViewPar)
        { _viewParam = new ViewPar(template.ReadStringParam(pStrIdx)); }
        else if (pStrIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(template.ReadStringParam(pStrIdx)); }
        else if (pStrIdx.Type == StringType.Export)
        { _exportParam = new ExportPar(template.ReadStringParam(pStrIdx)); }
      }
    }

    private void AdaptText(OcadWriter writer, Setup setup,
      IList<GeoElement> textList, Ocad.Symbol.TextSymbol textSymbol)
    {
      Ocad.Symbol.TextSymbol.RectFraming pFrame = (Ocad.Symbol.TextSymbol.RectFraming)textSymbol.Frame;

      double dx0 = pFrame.Left;
      double dx1 = pFrame.Right;
      double dy0 = pFrame.Bottom - (2.0 / 3.0) * textSymbol.Size;
      double dy1 = pFrame.Top - textSymbol.Size;

      foreach (var pText in textList)
      {
        PointCollection pList = (PointCollection)pText.Geometry;
        pList = pList.Project(setup.Prj2Map);

        Polyline pLine = new Polyline();

        pLine.Add(new Point2D(pList[1].X - dx0, pList[1].Y - dy0));
        pLine.Add(new Point2D(pList[2].X + dx1, pList[2].Y - dy0));
        pLine.Add(new Point2D(pList[3].X + dx1, pList[3].Y + dy1));
        pLine.Add(new Point2D(pList[4].X - dx0, pList[4].Y + dy1));
        pLine.Add(new Point2D(pList[1].X - dx0, pList[1].Y - dy0));

        pLine = pLine.Project(setup.Map2Prj);

        Element pElem = new GeoElement(pLine);
        pElem.Symbol = SymD.BoxText;
        pElem.Type = GeomType.line;
        writer.Append(pElem);
      }
    }

    private void AddLegend(OcadWriter writer, Polyline border, Point rawPosition)
    {
      IPoint edge = null;

      Element pElem = new GeoElement(border);
      pElem.Symbol = SymD.TrMassstab;
      pElem.Type = GeomType.line;
      writer.Append(pElem);

      border = border.Project(_templateSetup.Prj2Map);
      rawPosition = rawPosition.Project(_templateSetup.Prj2Map);

      double dist2 = -1;
      foreach (var pnt in border.Points)
      {
        double d2 = PointOperator.Dist2(pnt, rawPosition);
        if (dist2 < 0 || dist2 > d2)
        {
          dist2 = d2;
          edge = pnt;
        }
      }
      if (edge == null)
      { return; }

      Point pPos0 = new Point2D();
      Point pPos1 = new Point2D();
      IBox pBxBord = border.Extent;
      IBox pBxLgd = _legendBox.Graphics[0].MapGeometry.Extent;
      double dx = pBxLgd.Max.X - pBxLgd.Min.X;
      double dy = pBxLgd.Max.Y - pBxLgd.Min.Y;

      if (Math.Abs(edge.X - border.Extent.Min.X) <
        Math.Abs(edge.X - border.Extent.Max.X))
      {
        pPos0.X = pBxBord.Min.X - pBxLgd.Min.X;
        pPos1.X = pPos0.X + dx;
      }
      else
      {
        pPos0.X = pBxBord.Max.X - pBxLgd.Max.X;
        pPos1.X = pPos0.X - dx;
      }
      if (Math.Abs(edge.Y - border.Extent.Min.Y) <
        Math.Abs(edge.Y - border.Extent.Max.Y))
      {
        pPos0.Y = pBxBord.Min.Y - pBxLgd.Min.Y;
        pPos1.Y = pPos0.Y;
      }
      else
      {
        pPos0.Y = pBxBord.Max.Y - pBxLgd.Max.Y;
        pPos1.Y = pPos0.Y;
      }

      pElem = new GeoElement(pPos0.Project(_templateSetup.Map2Prj));
      pElem.Symbol = SymD.TrLegendeBox;
      pElem.Type = GeomType.point;
      writer.Append(pElem);

      pElem = new GeoElement(pPos1.Project(_templateSetup.Map2Prj));
      pElem.Symbol = SymD.TrLegendeBox;
      pElem.Type = GeomType.point;
      writer.Append(pElem);

      pElem = new GeoElement(pPos0.Project(_templateSetup.Map2Prj));
      pElem.Angle = _templateSetup.PrjRotation;
      pElem.Symbol = SymD.TrNordpfeil;
      pElem.Type = GeomType.point;
      writer.Append(pElem);

      Point p = pPos1.Project(_templateSetup.Map2Prj);
      pElem = Common.CreateText("50 m", p.X, p.Y, _templateSetup, _legendText);
      writer.Append(pElem);

      Polyline pScale = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(50, 0) });
      pScale = pScale.Project(_templateSetup.Prj2Map);
      dx = pScale.Project(Geometry.ToXY).Length() / 2.0;

      pScale = new Polyline();
      pScale.Add(new Point2D(pPos1.X - dx, pPos1.Y - dy / 10.0));
      pScale.Add(new Point2D(pPos1.X - dx, pPos1.Y - dy / 5.0));
      pScale.Add(new Point2D(pPos1.X + dx, pPos1.Y - dy / 5.0));
      pScale.Add(new Point2D(pPos1.X + dx, pPos1.Y - dy / 10.0));

      pElem = new GeoElement(pScale.Project(_templateSetup.Map2Prj));
      pElem.Symbol = SymD.TrMassstab;
      pElem.Type = GeomType.line;
      writer.Append(pElem);
    }

  }
}
