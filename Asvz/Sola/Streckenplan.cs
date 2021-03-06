using Basics.Geom;
using Ocad;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Asvz.Sola
{
  /// <summary>
  /// Summary description for StreckenPlan.
  /// </summary>
  public class Streckenplan
  {
    public enum Plantyp { Laeufer, Begleiter }

    private readonly SolaData _data;
    private readonly string _template;
    private Setup _templateSetup;
    private readonly Polyline _streckePre;
    private readonly Polyline _streckeNext;
    private readonly Polyline _strecke;
    private readonly int _iStrecke;

    private readonly IList<GeoElement> _elemUeKreis;
    private readonly IList<GeoElement> _elemUeZiel;
    private readonly IList<GeoElement> _elemInfo;
    private readonly IList<GeoElement> _elemPlace;
    private readonly IList<GeoElement> _elemStrecke;
    private Polyline _ausschnittBox;

    private Ocad.Symbol.TextSymbol _kmTxtSymbol;
    private Ocad.Symbol.PointSymbol _kmStrichSymbol;
    private Ocad.Symbol.TextSymbol _infoSymbol;
    private Ocad.Symbol.TextSymbol _placeSymbol;
    private Ocad.Symbol.TextSymbol _symBewilligung;
    private Ocad.Symbol.PointSymbol _nrBackSymbol;
    private Polyline _symAusschnitt;
    private double _rahmenBorder;
    private Ocad.Symbol.LineSymbol _symStrecke;

    private PrintPar _printParam;
    private Dictionary<StringParamIndex, ColorPar> _colors;

    public Streckenplan(string template, string dhmName, int strecke)
    {
      _template = template;
      _iStrecke = strecke;

      _data = new SolaData(template, dhmName);
      _strecke = _data.Categorie(_iStrecke - 1, Kategorie.Default).Strecke;
      if (_iStrecke > 1)
      { _streckePre = _data.Categorie(_iStrecke - 2, Kategorie.Default).Strecke; }
      if (_iStrecke < _data.Strecken.Count)
      { _streckeNext = _data.Categorie(_iStrecke, Kategorie.Default).Strecke; }

      _elemUeKreis = new List<GeoElement>();
      _elemUeZiel = new List<GeoElement>();
      _elemInfo = new List<GeoElement>();
      _elemPlace = new List<GeoElement>();
      _elemStrecke = new List<GeoElement>();
    }

    public void Update(Kategorie kat, bool updateStartZiel)
    {
      bool verkuerzt = (kat == Kategorie.Damen);

      OcadReader pUpdate = OcadReader.Open(_template);
      ReadTemplate(pUpdate);
      pUpdate.Close();

      string updateFile = _data.Strecken[_iStrecke - 1].Vorlage;
      pUpdate = OcadReader.Open(updateFile);
      ReadUpdate(pUpdate);
      pUpdate.Close();

      OcadWriter writer = OcadWriter.AppendTo(updateFile);

      try
      {
        List<int> delElems;
        if (verkuerzt)
        {
          delElems = new List<int>
          {
            SymS.Verkuerzt, SymS.Laufrichtung, SymS.VorherNachher,
            SymS.TextStrecke, SymS.TextInfo,
            SymS.Verpflegung, SymS.LinieBreit
          };
        }
        else
        {
          delElems = new List<int>
          {
            SymS.Strecke, SymS.Verkuerzt, SymS.Laufrichtung, SymS.VorherNachher,
            SymS.TextStrecke, SymS.TextInfo, SymS.KmStrich, SymS.TextKm,
            SymS.Verpflegung, SymS.LinieBreit
          };
        }
        if (updateStartZiel)
        { delElems.AddRange(new[] { SymS.UebergabeTeil, SymS.ZielTeil }); }


        writer.DeleteElements(delElems);

        bool first = true;
        foreach (var o in _data.Strecken[_iStrecke - 1].Categories)
        {
          SolaCategorie cat = (SolaCategorie)o;
          Element pElem = new GeoElement(cat.Strecke);
          if (first)
          { pElem.Symbol = SymS.Strecke; }
          else
          { pElem.Symbol = SymS.Verkuerzt; }
          pElem.Type = GeomType.line;
          writer.Append(pElem);

          WriteKm(writer, cat, _kmTxtSymbol, _templateSetup, !first);
          WriteUe(writer, cat.Strecke.Points[0], _streckePre, -1, updateStartZiel);
          WriteUe(writer, cat.Strecke.Points.Last(), _streckeNext, 0, updateStartZiel);

          WriteInfo(writer, cat.Typ);
          first = false;
        }

        WriteVerpf(writer);

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(new[] {
          SymS.TextUebergabe, SymS.TextInfo, SymS.TextStreckeNr,
          SymS.Verpflegung, SymS.VorherNachher, SymS.LinieBreit,
          SymS.Nordpfeil }, Ocad.Symbol.SymbolStatus.Normal);
        writer.SymbolsSetState(new[] {
          SymS.UebergabeTeil, SymS.ZielTeil, SymS.Strecke, SymS.Verkuerzt,
          SymS.TextKm, SymS.KmStrich }, Ocad.Symbol.SymbolStatus.Protected);
      }
      finally
      {
        writer.Close();
      }
    }

    public void Export(string outFile, string bewilligung, Plantyp plantyp)
    {
      string templateName = _data.Strecken[_iStrecke - 1].Vorlage;

      File.Copy(templateName, outFile, true);
      OcadWriter writer = OcadWriter.AppendTo(outFile);

      OcadReader template = OcadReader.Open(templateName);
      ReadExport(template);
      Setup setup = template.ReadSetup();
      template.Close();

      try
      {
        writer.DeleteElements(new[] {
          SymS.RahmenText, SymS.TextStreckeNr,
          SymS.BoxStreckeNr, SymS.RahmenStrecke,
          SymS.TextBewilligung, SymS.TextBewilligungVoid});

        List<GeoElement> infoElements = AdaptText(writer, setup, _elemInfo, _infoSymbol);
        AdaptText(writer, setup, _elemPlace, _placeSymbol);
        AdaptStreckenNr(writer, _elemStrecke, _nrBackSymbol, infoElements, setup);

        if (_ausschnittBox != null)
        {
          WritePrintParam(writer, _ausschnittBox);
        }

        Setup orig = _templateSetup;
        try
        {
          _templateSetup = setup;
          WriteBewilligung(writer, bewilligung);
          ColorPar topRed = WriteColors(writer, _colors);
          WriteScale(writer, infoElements, topRed);
        }
        finally
        { _templateSetup = orig; }

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(new[] {
          SymS.TextUebergabe, SymS.RahmenText, SymS.TextInfo,
          SymS.TextStreckeNr, SymS.BoxStreckeNr, SymS.RahmenStrecke,
          SymS.Verpflegung, SymS.VorherNachher, SymS.LinieBreit, SymS.Nordpfeil,
          SymS.UebergabeTeil, SymS.ZielTeil, SymS.Strecke, SymS.Verkuerzt,
          SymS.TextKm, SymS.KmStrich, SymS.TextBewilligung }, Ocad.Symbol.SymbolStatus.Protected);

        if (plantyp == Plantyp.Begleiter)
        {
          writer.SymbolsSetState(new[] {
            SymS.Sanitaet, SymS.SanitaetPfeil, SymS.SanitaetText, SymS.Treffpunkt, SymS.Treffpunkt,
            SymS.TreffpunktText, SymS.StreckenHinweis, SymS.AchtungPfeil,
            SymS.AchtungZeichen}, Ocad.Symbol.SymbolStatus.Protected);
        }
      }
      finally
      {
        writer.Close();
      }
    }

    private double GetOffset()
    {
      return 3 * _rahmenBorder;
    }

    private void AdaptStreckenNr(OcadWriter writer, IList<GeoElement> elemStrecke,
      Ocad.Symbol.PointSymbol symNrBack, IList<GeoElement> elemInfo, Setup setup)
    {
      if (elemStrecke.Count > 1 || elemInfo.Count > 1)
      {
        foreach (var strNr in elemStrecke)
        {
          DrawStreckenNrBack(writer, ((GeoElement.Points)strNr.Geometry).BaseGeometry[0], setup, symNrBack);

          writer.Append(strNr);
        }
      }
      else
      {
        IBox infoBox = elemInfo[0].Geometry.Project(setup.Prj2Map).Extent;
        Point lrInfo = new Point2D(infoBox.Max.X, infoBox.Min.Y);
        //IBox infoBox = elemInfo[0].Geometry.Extent;
        //Point lrInfo = new Point2D(infoBox.Max.X, infoBox.Min.Y).Project(setup.Prj2Map);

        IBox box = symNrBack.Graphics[0].MapGeometry.Extent;
        Point p = new Point2D(lrInfo.X - box.Max.X,
          lrInfo.Y - (box.Max.Y + GetOffset()));
        p = p.Project(setup.Map2Prj);

        PointCollection pnts = ((GeoElement.Points)elemStrecke[0].Geometry).BaseGeometry;
        Point dp = p - pnts[0];
        for (int iPoint = 0; iPoint < pnts.Count; iPoint++)
        {
          pnts[iPoint] = pnts[iPoint] + dp;
        }

        writer.Append(elemStrecke[0]);

        DrawStreckenNrBack(writer, p, setup, symNrBack);
      }
    }

    private void DrawStreckenNrBack(OcadWriter writer, IPoint point, Setup setup,
      Ocad.Symbol.PointSymbol symNrBack)
    {
      point = PointOp.Project(point, setup.Prj2Map);

      IBox box = symNrBack.Graphics[0].MapGeometry.Extent;

      Polyline line = new Polyline();
      line.Add(point + new Point2D(box.Min.X, box.Max.Y));
      line.Add(point + new Point2D(box.Max.X, box.Max.Y));
      line.Add(point + new Point2D(box.Max.X, box.Min.Y));
      line.Add(point + new Point2D(box.Min.X, box.Min.Y));
      line.Add(point + new Point2D(box.Min.X, box.Max.Y));

      line = line.Project(setup.Map2Prj);

      Element elem = new GeoElement(line);
      elem.Symbol = SymS.RahmenStrecke;
      elem.Type = GeomType.area;

      writer.Append(elem);
    }

    private void WriteBewilligung(OcadWriter writer, string bewilligung)
    {
      string sText = bewilligung;
      double xMap = _printParam.Right * 100;
      double yMap = _printParam.Bottom * 100;
      if (_symBewilligung.Frame is Ocad.Symbol.TextSymbol.RectFraming frame)
      {
        xMap -= frame.Right;
        yMap += frame.Bottom;
      }

      IPoint pos = _templateSetup.Map2Prj.Project(new Point2D(xMap, yMap));
      Element elem = Common.CreateText(sText, pos.X, pos.Y, _templateSetup, _symBewilligung);
      //elem.Symbol = SymS.TextBewilligungVoid;
      writer.Append(elem);
    }

    private ColorPar WriteColors(OcadWriter writer, Dictionary<StringParamIndex, ColorPar> colors)
    {
      ColorPar topRed = null;
      int max = int.MinValue;
      foreach (var pair in colors)
      {
        max = Math.Max(pair.Value.Number, max);
        if (pair.Value.Number == _symStrecke.LineColor)
        { topRed = new ColorPar(pair.Value.StringPar); }
      }

      topRed.Name = "Rot Top";
      topRed.Number = max + 1;

      ColorPar pre = topRed;
      foreach (var pair in colors)
      {
        writer.Overwrite(pair.Key, pre.StringPar);
        pre = pair.Value;
      }

      writer.Append(StringType.Color, 0, pre.StringPar);
      return topRed;
    }
    private void WriteScale(OcadWriter writer, IList<GeoElement> infoElems, ColorPar topRed)
    {
      Point pMin = null;
      foreach (var infoElem in infoElems)
      {
        IPoint p = infoElem.Geometry.Project(_templateSetup.Prj2Map).Extent.Min;
        //IPoint p = PointOp.Project(infoElem.Geometry.Extent.Min, _templateSetup.Prj2Map);
        if (pMin == null || pMin.Y > p.Y)
        { pMin = Point.Create(p); }
      }
      if (pMin == null)
      { throw new NotImplementedException("Position not defined"); }

      pMin.Y -= GetOffset();
      double dx = 1000.0 * 1000.0 / _templateSetup.Scale * 100;
      double dy = 0.3 * dx;

      Point2D p0 = new Point2D(pMin.X + 0.12 * dx, pMin.Y - dy);
      Element elem = Common.CreateText("0 km", p0.X, p0.Y + 0.3 * dy, _kmTxtSymbol);
      writer.Append(elem);

      double strichOffset = -_kmStrichSymbol.Graphics[0].MapGeometry.Extent.Max.X;
      elem = new MapElement(Coord.EnumCoords(new Point2D(p0.X, p0.Y - strichOffset)));
      elem.Angle = 3.0 * Math.PI / 2.0;
      elem.Symbol = SymS.KmStrich;
      elem.Type = GeomType.point;
      writer.Append(elem);

      elem = Common.CreateText("1 km", p0.X + dx, p0.Y + 0.3 * dy, _kmTxtSymbol);
      writer.Append(elem);

      elem = new MapElement(Coord.EnumCoords(new Point2D(p0.X + dx, p0.Y - strichOffset)));
      elem.Angle = 3.0 * Math.PI / 2.0;
      elem.Symbol = SymS.KmStrich;
      elem.Type = GeomType.point;
      writer.Append(elem);

      elem = new MapElement(Coord.EnumCoords(Polyline.Create(new[] { new Point2D(p0.X, p0.Y), new Point2D(p0.X + dx, p0.Y) })));
      elem.Symbol = -2;
      elem.LineWidth = _symStrecke.LineWidth;
      elem.Color = topRed.Number;
      elem.Type = GeomType.line;
      int idx = writer.Append(elem);
      //LayoutObjectPar par = new LayoutObjectPar("Scaleline");
      //par.ActObjectIndex = idx + 1;
      //par.Visible = true;
      //writer.Append(StringType.LayoutObject, idx + 1, par.StringParam);

      double x0 = pMin.X;
      double x1 = p0.X + dx + (p0.X - pMin.X);
      double y1 = pMin.Y;
      double y0 = p0.Y - 0.3 * dy;
      elem = new MapElement(Coord.EnumCoords(new Area(
        Polyline.Create(new[] { new Point2D(x0, y0), new Point2D(x1, y0),
        new Point2D(x1, y1), new Point2D(x0, y1), new Point2D(x0, y0)}))));
      elem.Symbol = SymS.RahmenStrecke;
      elem.Type = GeomType.area;
      writer.Append(elem);
    }

    private List<GeoElement> AdaptText(OcadWriter writer, Setup setup,
      IList<GeoElement> textList, Ocad.Symbol.TextSymbol textSymbol)
    {
      Ocad.Symbol.TextSymbol.RectFraming frame = (Ocad.Symbol.TextSymbol.RectFraming)textSymbol.Frame;

      double dx0 = frame.Left;
      double dx1 = frame.Right;
      double dy0 = frame.Bottom - (2.0 / 3.0) * textSymbol.Size;
      double dy1 = frame.Top - textSymbol.Size;

      List<GeoElement> elems = new List<GeoElement>(textList.Count);
      foreach (var txtElem in textList)
      {
        PointCollection txtGeom = ((GeoElement.Points)txtElem.Geometry).BaseGeometry;
        txtGeom = txtGeom.Project(setup.Prj2Map);

        Polyline line = new Polyline();

        line.Add(new Point2D(txtGeom[1].X - dx0, txtGeom[1].Y - dy0));
        line.Add(new Point2D(txtGeom[2].X + dx1, txtGeom[2].Y - dy0));
        line.Add(new Point2D(txtGeom[3].X + dx1, txtGeom[3].Y + dy1));
        line.Add(new Point2D(txtGeom[4].X - dx0, txtGeom[4].Y + dy1));
        line.Add(new Point2D(txtGeom[1].X - dx0, txtGeom[1].Y - dy0));

        GeoElement elem = new GeoElement(line.Project(setup.Map2Prj));
        elem.Symbol = SymS.RahmenText;
        elem.Type = GeomType.line;
        writer.Append(elem);

        elems.Add(elem);
      }

      return elems;
    }

    private void WriteVerpf(OcadWriter writer)
    {
      Polyline line = _data.Strecken[_iStrecke - 1].GetCategorie(Kategorie.Default).Strecke;

      List<Data.VerpflegungSym> verpfList = _data.Verpflegung(line);
      foreach (var verpf in verpfList)
      {
        Element pElem = new GeoElement(verpf.Symbol);
        pElem.Symbol = SymS.Verpflegung;
        pElem.Type = GeomType.point;
        writer.Append(pElem);

        pElem = new GeoElement(verpf.Index);
        pElem.Symbol = SymS.LinieBreit;
        pElem.Type = GeomType.line;
        writer.Append(pElem);
      }
    }

    private void WriteUe(OcadWriter writer, IPoint point, Polyline strecke, int iPoint, bool updateStartZiel)
    {
      Element pElem;
      if (strecke != null)
      {
        IPoint p;
        if (iPoint == 0)
        { p = strecke.Points[0]; }
        else if (iPoint == -1)
        { p = strecke.Points.Last(); }
        else
        { throw new ArgumentException(string.Format("{0} != 0 && != -1", iPoint)); }

        Arc arc = new Arc(p, 800, 0, 2 * Math.PI);

        IEnumerable<ParamGeometryRelation> intersects =
          GeometryOperator.CreateRelations(strecke, arc);

        IList<Polyline> parts = strecke.Split(intersects);
        if (iPoint == 0)
        { strecke = parts[0]; }
        else
        { strecke = parts[parts.Count - 1]; }

        pElem = new GeoElement(strecke);
        pElem.Symbol = SymS.VorherNachher;
        pElem.Type = GeomType.line;
        writer.Append(pElem);
      }

      if (updateStartZiel)
      {
        Box box = new Box(Point.CastOrWrap(point));
        box.Min.X -= 700;
        box.Min.Y -= 700;
        box.Max.X += 700;
        box.Max.Y += 700;
        IBox iBox = box;

        foreach (var e in _elemUeKreis)
        {
          if (BoxOp.Intersects(e.Geometry.Extent, iBox))
          {
            pElem = new GeoElement(e.Geometry);
            pElem.Symbol = SymS.UebergabeTeil;
            pElem.Type = GeomType.line;
            writer.Append(pElem);
          }
        }
        foreach (var e in _elemUeZiel)
        {
          if (BoxOp.Intersects(e.Geometry.Extent, iBox))
          {
            pElem = new GeoElement(e.Geometry);
            pElem.Symbol = SymS.ZielTeil;
            pElem.Type = GeomType.line;
            writer.Append(pElem);
          }
        }
      }
    }

    private void WriteInfo(OcadWriter writer, Kategorie kat)
    {
      Categorie cat = _data.Categorie(_iStrecke - 1, kat);
      double d = cat.DispLength / 1000.0;
      double h = cat.SteigungRound(5.0);

      foreach (var e in _elemInfo)
      {
        if (e.Text.StartsWith("L�nge:") && (kat & Kategorie.Default) == Kategorie.Default)
        {
          e.Text = string.Format(
            "L�nge: {0:N2} km" + Environment.NewLine +
            "Steigung: {1:N0} m",
            d, h);
        }
        else if (e.Text.StartsWith("Damen") && (kat & Kategorie.Damen) == Kategorie.Damen)
        {
          e.Text = string.Format(
            "Damen/Senioren:" + Environment.NewLine +
            "L�nge: {0:N2} km" + Environment.NewLine +
            "Steigung: {1:N0} m",
            d, h);
        }
        else
        { continue; }
        writer.Append(e);
      }
    }

    public static void WriteKm(OcadWriter writer, SolaCategorie cat,
      Ocad.Symbol.TextSymbol kmTxtSymbol, Setup setup, bool damen)
    {
      Polyline line = cat.Strecke;
      double lengthMeas = cat.DispLength / 1000;
      int iKm = 1;

      while (iKm < lengthMeas)
      {
        double[] param = cat.GetLineParams(iKm * 1000.0);
        IPoint p = line.GetSegment((int)param[0]).PointAt(param[1]);
        IPoint t = line.GetSegment((int)param[0]).TangentAt(param[1]);
        GeoElement textKm = CreateKmText(p, t, setup, iKm, kmTxtSymbol, damen);
        PointCollection textKmBox = (((GeoElement.Points)textKm.Geometry).BaseGeometry).Clone();
        textKmBox.Add(textKmBox[1]);
        textKmBox.Insert(0, p + 0.01 * PointOp.Sub(textKmBox[0], p));
        Polyline textKmPoly = Polyline.Create(textKmBox);
        if (line.Intersection(textKmPoly) != null)
        {
          t = PointOp.Scale(-1.0, t);
          textKm = CreateKmText(p, t, setup, iKm, kmTxtSymbol, damen);
        }
        writer.Append(textKm);

        textKm = new GeoElement(p);
        textKm.Angle = writer.Setup.PrjRotation + Math.Atan2(t.X, -t.Y);
        textKm.Symbol = SymS.KmStrich;
        textKm.Type = GeomType.point;

        writer.Append(textKm);

        iKm += 1;
      }
    }

    private static GeoElement CreateKmText(IPoint p, IPoint t, Setup setup,
      int km, Ocad.Symbol.TextSymbol kmTxtSymbol, bool damen)
    {
      string sKm = km.ToString();
      if (damen)
      { sKm += "d"; }

      Point pText = new Point2D(t.Y, -t.X);
      pText = 130.0 * 1.0 / Math.Sqrt(pText.OrigDist2()) * pText;
      pText = PointOp.Add(pText, p);

      GeoElement elem = Common.CreateText(sKm, pText.X, pText.Y, setup, kmTxtSymbol);
      PointCollection list = ((GeoElement.Points)elem.Geometry).BaseGeometry;
      Point pM = 0.5 * PointOp.Add(list[1], list[3]);
      pText = 2.0 * pText - pM;

      return Common.CreateText(sKm, pText.X, pText.Y, setup, kmTxtSymbol);
    }

    public void Write(string outFile)
    {
      File.Copy(_template, outFile, true);
      OcadWriter writer = OcadWriter.AppendTo(outFile);

      try
      {
        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(new[] { 1 }, Ocad.Symbol.SymbolStatus.Protected);

        Element elem = new GeoElement(_strecke);
        elem.Symbol = 1;
        elem.Type = GeomType.line;
        writer.Append(elem);

      }
      finally
      {
        writer.Close();
      }
    }

    private void ReadTemplate(OcadReader template)
    {
      ReadTplObjects(template);
    }

    private void ReadTplObjects(OcadReader template)
    {
      ElementIndex pIndex;
      List<ElementIndex> pIndexList = new List<ElementIndex>();
      int iIndex = 0;
      while ((pIndex = template.ReadIndex(iIndex)) != null)
      {
        pIndexList.Add(pIndex);
        iIndex++;
      }

      foreach (var elem in template.EnumGeoElements(pIndexList))
      {
        if (elem.Symbol == SymS.UebergabeTeil)
        { _elemUeKreis.Add(elem); }
        else if (elem.Symbol == SymS.ZielTeil)
        { _elemUeZiel.Add(elem); }
      }
    }

    private void ReadExport(OcadReader template)
    {
      _templateSetup = template.ReadSetup();

      ReadUpSymbols(template);
      ReadUpObjects(template);
      ReadUpStringParams(template);
    }

    private void ReadUpdate(OcadReader template)
    {
      _templateSetup = template.ReadSetup();

      ReadUpSymbols(template);
      ReadUpObjects(template);
      ReadUpStringParams(template);
    }

    private void ReadUpObjects(OcadReader template)
    {
      ElementIndex pIndex;
      List<ElementIndex> pIndexList = new List<ElementIndex>();
      int iIndex = 0;
      while ((pIndex = template.ReadIndex(iIndex)) != null)
      {
        pIndexList.Add(pIndex);
        iIndex++;
      }

      foreach (var elem in template.EnumGeoElements(pIndexList))
      {
        if (elem.Symbol == _infoSymbol.Number)
        {
          bool b = false;
          foreach (var existing in _elemInfo)
          {
            if (existing.Text == elem.Text)
            {
              b = true;
              break;
            }
          }
          if (b == false)
          { _elemInfo.Add(elem); }
        }
        else if (elem.Symbol == _placeSymbol.Number)
        { _elemPlace.Add(elem); }
        else if (elem.Symbol == SymS.TextStreckeNr)
        { _elemStrecke.Add(elem); }
        else if (elem.Symbol == SymS.Ausschnitt)
        {
          if (_ausschnittBox != null) throw new InvalidOperationException("Multiple Ausschnitt");
          _ausschnittBox = GetPrintExtent(elem);
        }
      }
    }

    private void ReadUpSymbols(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      List<int> indexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        indexList.Add(i);
        iIndex++;
      }

      Setup setup = reader.ReadSetup();
      setup.PrjRotation = 0;
      setup.PrjTrans.X = 0;
      setup.PrjTrans.Y = 0;

      Dictionary<int, Ocad.Symbol.BaseSymbol> symbols = new Dictionary<int, Ocad.Symbol.BaseSymbol>();
      foreach (var iPos in indexList)
      {
        Ocad.Symbol.BaseSymbol symbol = reader.ReadSymbol(iPos);
        if (symbol == null)
        { continue; }

        symbols.Add(symbol.Number, symbol);

        if (symbol.Number == SymS.TextKm)
        { _kmTxtSymbol = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymS.KmStrich)
        { _kmStrichSymbol = (Ocad.Symbol.PointSymbol)symbol; }
        else if (symbol.Number == SymS.TextUebergabe)
        { _placeSymbol = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymS.TextInfo)
        { _infoSymbol = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymS.BoxStreckeNr)
        { _nrBackSymbol = (Ocad.Symbol.PointSymbol)symbol; }
        else if (symbol.Number == SymS.TextBewilligung)
        { _symBewilligung = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymS.Ausschnitt)
        {
          if (symbol.Graphics.Count != 1)
          {
            string msg = string.Format("Ausschnittsymbol {0} hat {1} Geometrien",
              symbol.Number, symbol.Graphics.Count);
            throw new InvalidOperationException(msg);
          }
          _symAusschnitt = ((GeoElement.Line)symbol.Graphics[0].MapGeometry).Project(setup.Map2Prj).BaseGeometry;
        }
      }
      Ocad.Symbol.AreaSymbol rahmen = (Ocad.Symbol.AreaSymbol)symbols[SymS.RahmenStrecke];
      Ocad.Symbol.LineSymbol rahmenBorder = (Ocad.Symbol.LineSymbol)symbols[rahmen.BorderSym];
      _rahmenBorder = rahmenBorder.LineWidth;

      _symStrecke = (Ocad.Symbol.LineSymbol)symbols[SymS.Strecke];
    }

    private void ReadUpStringParams(OcadReader template)
    {
      IList<StringParamIndex> indexEnum = template.ReadStringParamIndices();
      _colors = new Dictionary<StringParamIndex, ColorPar>();
      //Template pTpl = new Template();
      foreach (var strIdx in indexEnum)
      {
        if (strIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(template.ReadStringParam(strIdx)); }
        if (strIdx.Type == StringType.Color)
        { _colors.Add(strIdx, new ColorPar(template.ReadStringParam(strIdx))); }
      }
    }

    private Polyline GetPrintExtent(GeoElement printElem)
    {
      Setup setup = new Setup();
      IPoint p = ((GeoElement.Point)printElem.Geometry).BaseGeometry;
      setup.PrjTrans.X = p.X;
      setup.PrjTrans.Y = p.Y;
      setup.Scale = 1 / FileParam.OCAD_UNIT;
      setup.PrjRotation = printElem.Angle + _templateSetup.PrjRotation;
      Polyline printExtent = _symAusschnitt.Project(setup.Map2Prj);
      return printExtent;
    }

    private void WritePrintParam(OcadWriter writer, Polyline border)
    {
      if (_printParam == null)
      { _printParam = new PrintPar(); }
      if (_printParam != null)
      {
        Polyline extentLine = border.Project(_templateSetup.Prj2Map);
        IBox extent = extentLine.Extent;
        _printParam.Scale = 25000;
        _printParam.Range = PrintPar.RangeType.PartialMap;
        _printParam.Left = extent.Min.X / 100;
        _printParam.Right = extent.Max.X / 100;
        _printParam.Top = extent.Max.Y / 100;
        _printParam.Bottom = extent.Min.Y / 100;

        writer.Overwrite(StringType.PrintPar, 0, _printParam.StringPar);
      }
    }
  }
}
