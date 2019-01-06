using System;
using System.Collections.Generic;
using System.IO;
using Basics.Geom;
using Ocad;
using Ocad.StringParams;
using System.Text;
using Ocad.Data;

namespace Asvz.Sola
{
  /// <summary>
  /// Summary description for Gesamtplan.
  /// </summary>
  public class Gesamtplan
  {
    [Flags]
    private enum Layout
    {
      Rahmen = 1,
      Titel = 2,
      LaufDatum = 4,
      Bewilligung = 8,
      KmNetz = 16
    }
    private Ocad.Symbol.TextSymbol _symGross;
    private Ocad.Symbol.TextSymbol _symKlein;
    private Ocad.Symbol.TextSymbol _symSpital;
    private Ocad.Symbol.TextSymbol _symKmText;
    private Ocad.Symbol.TextSymbol _symKmGrid;
    private Ocad.Symbol.TextSymbol _symBewilligung;

    private IList<Element> _lstStrecke;
    private IList<Element> _lstStreckeInfo;
    private IList<Element> _lstGross;
    private IList<Element> _lstKlein;
    private IList<Element> _lstSpital;
    private IList<Element> _lstSanBg;

    private ExportPar _exportParam;
    private PrintPar _printParam;
    private DisplayPar _displayParam;
    private StringParamIndex _displayIdx;

    private readonly int[] _symbolsStrecken = {
        SymT.TextStrecke, SymT.TextStreckeBox, SymT.TextGross, SymT.TextKlein, SymT.TextRahmen,
        SymT.Start, SymT.Uebergabe, SymT.UebergabeTeil, SymT.Ziel, SymT.ZielTeil,
        SymT.Strecke, SymT.StreckeKurz, SymT.Laufrichtung,
        SymT.Verpflegung, SymT.LinieBreit
      };
    private readonly int[] _symbolsEinsatz = {
        SymT.TextStrecke, SymT.TextStreckeBox, SymT.TextGross, SymT.TextKlein, SymT.TextRahmen,
        SymT.Start, SymT.Uebergabe, SymT.UebergabeTeil, SymT.Ziel, SymT.ZielTeil,
        SymT.Strecke, SymT.StreckeKurz, SymT.Laufrichtung, SymT.StreckenInfo,
        SymT.Verpflegung, SymT.LinieBreit,
        SymT.Deckweiss, SymT.TextKmRaster, SymT.Bewilligung, // SymT.KmRasterLinie, // km raster
        SymT.KmDist, SymT.TextKmDist, SymT.KmStartEnd, // Streckenkilometrierung
        SymT.TextSpital, SymT.TextSanTitel, SymT.TextSanDetail, SymT.TextSanBg, SymT.Sanitaet // Sanität
      };
    private readonly int[] _symbolsTransport = {
        SymT.TextStrecke, SymT.TextStreckeBox, SymT.TextGross, SymT.TextRahmen,
        SymT.Start, SymT.Uebergabe, SymT.UebergabeTeil, SymT.Ziel, SymT.ZielTeil,
        SymT.Strecke, SymT.StreckeKurz,
        SymT.Transport, SymT.TransportUi,
        SymT.LinieBreit, SymT.TextKmDist,
        SymT.TextKlein, SymT.Bewilligung, SymT.Deckweiss
      };


    private readonly string _template;
    private readonly string _dhm;

    private Setup _templateSetup;

    public Gesamtplan(string template, string dhm)
    {
      _template = template;
      _dhm = dhm;
      OcadReader ocdTmpl = OcadReader.Open(_template);
      ReadTemplate(ocdTmpl);
      ocdTmpl.Close();
    }

    public void WriteStrecken(string outFile)
    {
      File.Copy(_template, outFile, true);
      using (OcadWriter writer = OcadWriter.AppendTo(outFile))
      {
        foreach (var elemStrecke in _lstStrecke)
        {
          PointCollection points = (PointCollection)elemStrecke.Geometry;
          Point center = 0.5 * PointOperator.Add(points[1], points[3]);

          Element elemBox = new Element(true);
          elemBox.Symbol = SymT.TextStreckeBox;
          elemBox.Geometry = center;
          elemBox.Type = GeomType.point;
          writer.Append(elemBox);
        }

        AdaptText(writer, _templateSetup, _lstGross, _symGross, SymT.TextRahmen);
        AdaptText(writer, _templateSetup, _lstKlein, _symKlein, SymT.TextRahmen);

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(_symbolsStrecken, Ocad.Symbol.SymbolStatus.Protected);

        writer.Close();
      }
    }

    public void WriteEinsatz(string outFile, string bewilligung, DateTime laufdatum)
    {
      string dir = Path.GetDirectoryName(outFile);
      string name = Path.GetFileNameWithoutExtension(outFile);
      string constr = Path.Combine(dir, name + OcadLayouter.NewConstr);

      File.Copy(_template, outFile, true);
      File.Copy(_template, constr, true);
      using (OcadWriter wConstr = OcadWriter.AppendTo(constr))
      using (OcadWriter writer = OcadWriter.AppendTo(outFile))
      {
        SolaData data = new SolaData(_template, _dhm);
        wConstr.DeleteElements(x => { return true; });
        wConstr.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Normal);

        Point2D ll = new Point2D(672800, 237000);
        Point2D ur = new Point2D(696800, 254000);
        double rahmenBreite = 1000;

        WriteLayout(writer, ll, ur, rahmenBreite,
          bewilligung, "SOLA " + laufdatum.Year,
          laufdatum.ToString("dddd d. MMMM"), laufdatum,
          Layout.Rahmen | Layout.Titel | Layout.Bewilligung |
          Layout.LaufDatum | Layout.KmNetz);
        WriteDefaultStrecken(writer, data.Strecken); // TODO: additional strecken
        WriteDefaultStrecken(wConstr, data.Strecken);

        writer.DeleteElements(new[] { SymT.StreckenInfo });

        foreach (var strecke in _lstStrecke)
        {
          PointCollection points = (PointCollection)strecke.Geometry;
          Point center = 0.5 * PointOperator.Add(points[1], points[3]);

          Element elem = new Element(true);
          elem.Symbol = SymT.TextStreckeBox;
          elem.Geometry = center;
          elem.Type = GeomType.point;
          writer.Append(elem);

          Element minInfo = null;
          double minDist2 = double.MaxValue;
          int iStrecke = int.Parse(strecke.Text);
          foreach (var elemInfo in _lstStreckeInfo)
          {
            PointCollection infoPoints = (PointCollection)elemInfo.Geometry;
            Point infoCenter = 0.5 * PointOperator.Add(infoPoints[1], infoPoints[3]);
            double dist2 = center.Dist2(infoCenter);
            if (dist2 < minDist2)
            {
              minInfo = elemInfo;
              minDist2 = dist2;
            }
          }
          double maxDist = 500;
          if (minInfo == null)
          { throw new InvalidOperationException("No minInfo found for Strecke " + iStrecke); }
          if (minDist2 < maxDist * maxDist)
          {
            Categorie cat = data.Strecken[iStrecke - 1].Categories[0];
            StringBuilder text = new StringBuilder();
            text.AppendFormat("{0:0.00} km", cat.DispLength / 1000.0);
            text.AppendLine();
            text.AppendFormat("{0} m", cat.SteigungRound(5));
            minInfo.Text = text.ToString();

            PointCollection infoPoints = (PointCollection)minInfo.Geometry;
            Point infoCenter = 0.5 * PointOperator.Add(infoPoints[1], infoPoints[3]);
            if (infoCenter.X > points[3].X)
            { minInfo.Geometry = Transfer(center + new Point2D(240, 70), infoPoints); }
            else if (infoCenter.Y < points[3].Y)
            { minInfo.Geometry = Transfer(center + new Point2D(-150, -260), infoPoints); }

            writer.Append(minInfo);
            wConstr.Append(minInfo);
          }
        }

        AdaptText(writer, _templateSetup, _lstGross, _symGross, SymT.TextRahmen);
        AdaptText(writer, _templateSetup, _lstKlein, _symKlein, SymT.TextRahmen);
        AdaptText(writer, _templateSetup, _lstSpital, _symSpital, SymT.TextRahmen);

        foreach (var pSanBg in _lstSanBg)
        {
          Element pElem = new Element(true);
          pElem.Symbol = SymT.TextRahmen;
          pElem.Geometry = ((Area)pSanBg.Geometry).Border[0];
          pElem.Type = GeomType.line;
          writer.Append(pElem);
        }

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(_symbolsEinsatz, Ocad.Symbol.SymbolStatus.Protected);

        Point2D dd = new Point2D(rahmenBreite - 50, rahmenBreite - 50);
        WriteParams(writer, ll - dd, ur + dd);
      }

      OcadLayouter layouter = new OcadLayouter(outFile);
      layouter.UpdateUserValues();
    }

    private PointCollection Transfer(Point start, PointCollection points)
    {
      Point diff = start - points[0];
      PointCollection trans = new PointCollection();
      foreach (var p in points)
      { trans.Add(p + diff); }
      return trans;
    }

    private void WriteParams(OcadWriter writer, IPoint ll, IPoint ur)
    {
      if (_printParam == null)
      { _printParam = new PrintPar(); }
      if (_printParam != null)
      {
        ll = ll.Project(_templateSetup.Prj2Map);
        ur = ur.Project(_templateSetup.Prj2Map);
        _printParam.Scale = 60000;
        _printParam.Range = PrintPar.RangeType.PartialMap;
        _printParam.Left = ll.X / 100;
        _printParam.Right = ur.X / 100;
        _printParam.Top = ur.Y / 100;
        _printParam.Bottom = ll.Y / 100;

        writer.Overwrite(StringType.PrintPar, 0, _printParam.StringPar);
      }
      if (_exportParam == null)
      { _exportParam = new ExportPar(); }
      if (_exportParam != null)
      {
        _exportParam.Resolution = 600.0;
        writer.Overwrite(StringType.Export, 0, _exportParam.StringPar);
      }
      if (_displayParam != null)
      {
        _displayParam.ImageObjectMode = 0;
        writer.Overwrite(_displayIdx, _displayParam.StringPar);
      }
    }

    public void WriteTransport(string outFile, string bewilligung, DateTime laufdatum)
    {
      File.Copy(_template, outFile, true);
      OcadWriter writer = OcadWriter.AppendTo(outFile);

      try
      {
        Point2D ll = new Point2D(672800, 237000);
        Point2D ur = new Point2D(696800, 254000);
        double rahmenBreite = 1000;
        writer.DeleteElements(new[] { SymT.TextKlein, SymT.LinieBreit });
        WriteLayout(writer, ll, ur, rahmenBreite,
          bewilligung, "Kleidertransport",
          laufdatum.ToString("dddd d. MMMM yyyy"), laufdatum,
          Layout.Rahmen | Layout.Titel | Layout.LaufDatum | Layout.Bewilligung);

        WriteTransport(writer);

        AdaptText(writer, _templateSetup, _lstGross, _symGross, SymT.TextRahmen);

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(_symbolsTransport, Ocad.Symbol.SymbolStatus.Protected);

        Point2D dd = new Point2D(rahmenBreite - 50, rahmenBreite - 50);
        WriteParams(writer, ll - dd, ur + dd);

      }
      finally
      {
        writer.Close();
      }
    }

    private void WriteDefaultStrecken(OcadWriter writer, IList<SolaStrecke> strecken)
    {
      Polyline strecke0 = null;
      foreach (var info in strecken)
      {
        Element elem;

        SolaCategorie cat = info.GetCategorie(Kategorie.Default);
        Polyline strecke = cat.Strecke;

        Streckenplan.WriteKm(writer, cat, _symKmText, _templateSetup, false);

        if (strecke0 == null ||
          PointOperator.Dist2(strecke0.Points.Last.Value, strecke.Points.First.Value) > 10)
        {
          elem = new Element(true);
          elem.Symbol = SymT.KmStartEnd;
          elem.Geometry = strecke.Points.First.Value;
          elem.Type = GeomType.point;
          writer.Append(elem);
        }
        elem = new Element(true);
        elem.Symbol = SymT.KmStartEnd;
        elem.Geometry = strecke.Points.Last.Value;
        elem.Type = GeomType.point;
        writer.Append(elem);

        strecke0 = strecke;
      }
    }

    private void WriteTransport(OcadWriter writer)
    {
      int n = Ddx.Uebergabe.Count;
      for (int i = 0; i < n; i++)
      {
        UebergabeTransport.GetLayout(i, out Polyline box, out Point pos, false);

        if (box != null)
        {
          Element elem = new Element(true);
          elem.Geometry = box;
          elem.Symbol = SymT.LinieBreit;
          elem.Type = GeomType.line;
          writer.Append(elem);

          IPoint p0 = box.Points.First.Value;
          IPoint p1 = box.Points.First.Next.Value;
          IPoint p2 = box.Points.First.Next.Next.Value;
          if (Math.Abs(p1.X - p0.X) < Math.Abs(p1.Y - p0.Y))
          {
            elem = Common.CreateText("D", p0.X + 50, p0.Y - 85, _symKmText, _templateSetup);
          }
          else
          {
            elem = Common.CreateText("D", p0.X - 85, p0.Y - 50, _symKmText, _templateSetup);
            elem.Angle = -Math.PI / 2.0;
          }
          writer.Append(elem);
        }
      }
    }

    private void WriteLayout(OcadWriter writer, Point2D ll, Point2D ur, double rahmenBreite,
      string bewilligung, string kartentitel, string subtitel, DateTime laufdatum,
      Layout layout)
    {
      if ((layout & Layout.Rahmen) != 0)
      {
        WriteRahmen(writer, ll, ur, rahmenBreite);
      }

      if ((layout & Layout.Titel) != 0)
      {
        WriteTitel(writer, kartentitel, subtitel, ur);
      }

      if ((layout & Layout.KmNetz) != 0)
      {
        WriteKmRaster(writer, ll, ur);
      }

      if ((layout & Layout.LaufDatum) != 0)
      {
        WriteLaufDatum(writer, layout, ll, kartentitel, laufdatum);
      }

      if ((layout & Layout.Bewilligung) != 0)
      {
        WriteBewilligung(writer, ll, ur, bewilligung);
      }
    }

    private void WriteTitel(OcadWriter writer, string titel,
      string subtitel, Point2D ur)
    {


      Element elem = new Element(true);
      elem.Symbol = _symGross.Number;
      elem.UnicodeText = true;

      double xStart = 1000 * (int)((ur.X - 1500) / 1000);

      string sText = titel;
      elem = Common.CreateText(sText, xStart + 100, ur.Y - 300, _symGross, _templateSetup);
      while (elem.Geometry.Extent.Max.X > ur.X)
      {
        xStart -= 1000;
        elem = Common.CreateText(sText, xStart + 100, ur.Y - 300, _symGross, _templateSetup);
      }
      writer.Append(elem);

      sText = subtitel;

      elem = Common.CreateText(sText, xStart + 100, ur.Y - 550, _symKlein, _templateSetup);
      writer.Append(elem);

      elem = new Element(true);
      elem.Symbol = SymT.Deckweiss;
      elem.Type = GeomType.area;

      Polyline border = Polyline.Create(new[] {
        new Point2D(xStart, ur.Y), new Point2D(xStart, ur.Y - 800),
        new Point2D(ur.X, ur.Y - 800), new Point2D(ur.X, ur.Y), new Point2D(xStart, ur.Y)
      });
      Area poly = new Area(border);
      elem.Geometry = poly;
      writer.Append(elem);
    }

    private void WriteBewilligung(OcadWriter writer, Point2D ll, Point2D ur, string bewilligung)
    {
      Element elem = new Element(true);
      elem.Symbol = _symBewilligung.Number;
      elem.UnicodeText = true;

      string sText = bewilligung;
      elem = Common.CreateText(sText, ur.X, ll.Y - 200, _symBewilligung, _templateSetup);
      writer.Append(elem);
    }

    private void WriteLaufDatum(OcadWriter writer, Layout layout, Point2D ll,
      string titel, DateTime laufdatum)
    {

      Element elem = new Element(true);
      elem.Symbol = _symGross.Number;
      elem.UnicodeText = true;

      string sText = titel;
      elem = Common.CreateText(sText, ll.X, ll.Y - 600, _symGross, _templateSetup);
      writer.Append(elem);

      double xStart = elem.Geometry.Extent.Max.X + 200;

      // Datum
      if ((layout & Layout.LaufDatum) != 0)
      {
        sText = laufdatum.ToString("dddd d. MMMM yyyy");

        elem = Common.CreateText(sText, xStart, ll.Y - 600, _symKlein, _templateSetup);
        writer.Append(elem);
      }
    }


    private static void WriteRahmen(OcadWriter writer, Point2D ll, Point2D ur, double off)
    {
      Polyline inside = Polyline.Create(new[] {
          new Point2D(ll.X,ll.Y),
          new Point2D(ur.X,ll.Y),
          new Point2D(ur.X,ur.Y),
          new Point2D(ll.X,ur.Y),
          new Point2D(ll.X,ll.Y),
        });

      Polyline outside = Polyline.Create(new[] {
          new Point2D(ll.X - off,ll.Y - off),
          new Point2D(ll.X - off,ur.Y + off),
          new Point2D(ur.X + off,ur.Y + off),
          new Point2D(ur.X + off,ll.Y - off),
          new Point2D(ll.X - off,ll.Y - off),
        });

      Area area = new Area(new[] { outside, inside });

      Element elem = new Element(true);
      elem.Symbol = SymT.Deckweiss;
      elem.Geometry = area;
      elem.Type = GeomType.area;
      writer.Append(elem);

      elem = new Element(true);
      elem.Symbol = SymT.LinieBreit;
      elem.Geometry = inside;
      elem.Type = GeomType.line;
      writer.Append(elem);
    }

    private void WriteKmRaster(OcadWriter writer, Point2D ll, Point2D ur)
    {
      Element elem;

      for (int km = (int)ll.X; km < ur.X; km += 1000)
      {
        int iKm = km / 1000 + 1;

        string sKm = iKm.ToString();
        double x = iKm * 1000;
        double y = ur.Y + 200;

        elem = Common.CreateText(sKm, x, y, _symKmGrid, _templateSetup);
        writer.Append(elem);

        elem = new Element(true);
        elem.Symbol = SymT.KmRasterLinie;
        elem.Geometry = Polyline.Create(new Point[] {
          new Point2D(x, ur.Y), new Point2D(x, ll.Y) });
        elem.Type = GeomType.line;
        writer.Append(elem);
      }

      for (int km = (int)ll.Y; km < ur.Y; km += 1000)
      {
        int iKm = km / 1000 + 1;

        string sKm = iKm.ToString();
        double x = ll.X - 400;
        double y = iKm * 1000;

        elem = Common.CreateText(sKm, x, y - 100, _symKmGrid, _templateSetup);
        writer.Append(elem);

        elem = new Element(true);
        elem.Symbol = SymT.KmRasterLinie;
        elem.Geometry = Polyline.Create(new Point[] {
          new Point2D(ur.X, y), new Point2D(ll.X, y) });
        elem.Type = GeomType.line;
        writer.Append(elem);
      }
    }

    private void ReadTemplate(OcadReader template)
    {
      _templateSetup = template.ReadSetup();

      ReadObjects(template);
      ReadSymbols(template);
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

      _lstStrecke = new List<Element>();
      _lstStreckeInfo = new List<Element>();
      _lstGross = new List<Element>();
      _lstKlein = new List<Element>();
      _lstSpital = new List<Element>();
      _lstSanBg = new List<Element>();

      foreach (var elem in template.Elements(true, pIndexList))
      {
        if (elem.Symbol == SymT.TextStrecke)
        { _lstStrecke.Add(elem); }
        else if (elem.Symbol == SymT.StreckenInfo)
        { _lstStreckeInfo.Add(elem); }
        else if (elem.Symbol == SymT.TextGross)
        { _lstGross.Add(elem); }
        else if (elem.Symbol == SymT.TextKlein)
        { _lstKlein.Add(elem); }
        else if (elem.Symbol == SymT.TextSpital)
        { _lstSpital.Add(elem); }
        else if (elem.Symbol == SymT.TextSanBg)
        { _lstSanBg.Add(elem); }
      }
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
        Ocad.Symbol.BaseSymbol pSymbol = reader.ReadSymbol(iPos);
        if (pSymbol == null)
        { continue; }

        if (pSymbol.Number == SymT.TextGross)
        { _symGross = (Ocad.Symbol.TextSymbol)pSymbol; }
        if (pSymbol.Number == SymT.TextKlein)
        { _symKlein = (Ocad.Symbol.TextSymbol)pSymbol; }
        if (pSymbol.Number == SymT.TextSpital)
        { _symSpital = (Ocad.Symbol.TextSymbol)pSymbol; }
        if (pSymbol.Number == SymT.TextKmDist)
        { _symKmText = (Ocad.Symbol.TextSymbol)pSymbol; }
        if (pSymbol.Number == SymT.TextKmRaster)
        { _symKmGrid = (Ocad.Symbol.TextSymbol)pSymbol; }
        if (pSymbol.Number == SymT.Bewilligung)
        { _symBewilligung = (Ocad.Symbol.TextSymbol)pSymbol; }
      }
    }

    private void ReadStringParams(OcadReader template)
    {
      IList<StringParamIndex> pStrIdxList = template.ReadStringParamIndices();

      foreach (var strIdx in pStrIdxList)
      {
        if (strIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(template.ReadStringParam(strIdx)); }
        else if (strIdx.Type == StringType.Export)
        { _exportParam = new ExportPar(template.ReadStringParam(strIdx)); }
        else if (strIdx.Type == StringType.DisplayPar)
        {
          DisplayPar dsp = new DisplayPar(template.ReadStringParam(strIdx));
          if (dsp.ImageObjectMode != null)
          {
            _displayParam = new DisplayPar(template.ReadStringParam(strIdx));
            _displayIdx = strIdx;
          }
        }
      }
    }

    private void AdaptText(OcadWriter writer, Setup setup,
      IList<Element> textList, Ocad.Symbol.TextSymbol textSymbol, int symRahmen)
    {
      Ocad.Symbol.TextSymbol.RectFraming pFrame = (Ocad.Symbol.TextSymbol.RectFraming)textSymbol.Frame;

      double dx0 = pFrame.Left;
      double dx1 = pFrame.Right;
      double dy0 = pFrame.Bottom - (2.0 / 3.0) * textSymbol.Size * 1.1;
      double dy1 = pFrame.Top - textSymbol.Size * 1.1;
      Element pElem = new Element(true);

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

        pElem.Geometry = pLine;
        pElem.Symbol = symRahmen;
        pElem.Type = GeomType.line;
        writer.Append(pElem);
      }
    }
  }
}
