using System;
using System.Collections.Generic;
using System.Diagnostics;
using Basics.Geom;
using Ocad;
using System.IO;
using Ocad.StringParams;

namespace Asvz.Sola
{
  /// <summary>
  /// Summary description for Uebergabe.
  /// </summary>
  public class Detail
  {
    private Element _detail;
    private IList<Element> _legendPos;

    private Polyline _symUebergabe;

    private Ocad.Symbol.TextSymbol _legendText;
    private Ocad.Symbol.PointSymbol _legendBox;

    private readonly int[] _symbols =
      {
        SymD.LinieBreit, SymD.LinieMittel, SymD.LinieSchmal,
        SymD.Absperrband, SymD.Uebergaberaum, SymD.DtAusserhalb,
        SymD.TextKlein, SymD.DtTextList,
        SymD.DtAusschnitt, SymD.DtVorherNachher, SymD.DtNeustart, SymD.DtZiel,
        SymD.Sponser, SymD.Zeitnahme, SymD.Speaker, SymD.StickAusgabe,
        SymD.DtLegendeBox, SymD.DtTextLegende, SymD.DtMassstab, SymD.DtNordpfeil
      };

    private readonly int _strecke;
    private readonly int _indexDetail;

    Setup _templateSetup;

    public Detail(int strecke, int detail)
    {
      _strecke = strecke;
      _indexDetail = detail;
    }

    public void Write(string outFile)
    {
      string template = Ddx.Uebergabe[_strecke].Vorlage;

      ViewPar viewPar = null;
      PrintPar printPar = null;

      Ocad9Reader tplReader = (Ocad9Reader)OcadReader.Open(template);
      try
      {
        IList<Element> textList = new List<Element>();
        ElementIndex idxElem;
        IList<ElementIndex> indexList = new List<ElementIndex>();
        int iIndex = 0;
        while ((idxElem = tplReader.ReadIndex(iIndex)) != null)
        {
          indexList.Add(idxElem);
          iIndex++;
        }

        _detail = null;
        _legendPos = new List<Element>();
        _templateSetup = tplReader.ReadSetup();

        int iDetail = 0;
        foreach (Element elem in tplReader.Elements(true, indexList))
        {
          if (elem.Symbol == SymD.Detail)
          {
            if (iDetail == _indexDetail)
            {
              _detail = elem;
              _detail.Symbol = SymD.DtAusschnitt;
            }
            iDetail++;
          }
          else if (elem.Symbol == SymD.DtNordRoh)
          {
            _legendPos.Add(elem);
          }
          else if (elem.Symbol == SymD.TextGross)
          { textList.Add(elem); }
        }
        ReadSymbols(tplReader);

        IList<StringParamIndex> strIdxList = tplReader.ReadStringParamIndices();
        foreach (StringParamIndex strIdx in strIdxList)
        {
          if (strIdx.Type == StringType.ViewPar)
          { viewPar = new ViewPar(tplReader.ReadStringParam(strIdx)); }
          else if (strIdx.Type == StringType.PrintPar)
          { printPar = new PrintPar(tplReader.ReadStringParam(strIdx)); }
        }

        tplReader.Close();

      }
      finally
      {
        tplReader.Close();
      }

      if (_detail == null)
      { return; }

      File.Copy(template, outFile, true);
      Ocad9Writer writer = Ocad9Writer.AppendTo(outFile);
      try
      {
        Polyline border = GetBorder(_detail);
        writer.Append(_detail);

        foreach (Element legPos in _legendPos)
        {
          if (border.Extent.Intersects(legPos.Geometry))
          {
            AddLegend(writer, border, (Point)legPos.Geometry);
            break;
          }
        }

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(_symbols, Ocad.Symbol.SymbolStatus.Protected);

        writer.Remove(StringType.Template);
        if (viewPar != null)
        {
          Point viewCenter = 0.5 * (Point.Create(border.Extent.Min) +
            Point.Create(border.Extent.Max));
          viewCenter = viewCenter.Project(_templateSetup.Prj2Map);
          viewPar.XOffset = viewCenter.X / 100;
          viewPar.YOffset = viewCenter.Y / 100;

          writer.Overwrite(StringType.ViewPar, 0, viewPar.StringPar);
        }

        if (printPar == null)
        { printPar = new PrintPar(); }

        Polyline line = border.Project(_templateSetup.Prj2Map);
        printPar.Scale = 2000;
        double d = 10; // sonst wird die Linie beim Export ignoriert
        printPar.Range = PrintPar.RangeType.PartialMap;
        printPar.Left = (line.Extent.Min.X - d) / 100;
        printPar.Right = (line.Extent.Max.X + d) / 100;
        printPar.Top = (line.Extent.Max.Y + d) / 100;
        printPar.Bottom = (line.Extent.Min.Y - d) / 100;

        writer.Overwrite(StringType.PrintPar, 0, printPar.StringPar);
      }
      finally
      {
        writer.Close();
      }
    }

    private Polyline GetBorder(Element border)
    {
      // Get Uebergabe Geometry
      Setup setup = new Setup();
      Point p = (Point)border.Geometry;
      setup.PrjTrans.X = p.X;
      setup.PrjTrans.Y = p.Y;
      setup.Scale = 1 / FileParam.OCAD_UNIT;
      setup.PrjRotation = border.Angle + _templateSetup.PrjRotation;
      Polyline borderGeometry = _symUebergabe.Project(setup.Map2Prj);

      return borderGeometry;
    }


    private void ReadSymbols(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      IList<int> indexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        indexList.Add(i);
        iIndex++;
      }

      Setup setup = reader.ReadSetup();
      setup.PrjRotation = 0;
      setup.PrjTrans.X = 0;
      setup.PrjTrans.Y = 0;

      foreach (int iPos in indexList)
      {
        Ocad.Symbol.BaseSymbol symbol = reader.ReadSymbol(iPos);
        if (symbol == null)
        { continue; }

        if (symbol.Number == SymD.DtTextLegende)
        { _legendText = (Ocad.Symbol.TextSymbol)symbol; }
        if (symbol.Number == SymD.DtLegendeBox)
        { _legendBox = (Ocad.Symbol.PointSymbol)symbol; }

        if (symbol.Number == SymD.Detail)
        {
          Debug.Assert(symbol.Graphics.Count == 1);
          _symUebergabe = (Polyline)symbol.Graphics[0].Geometry.Project(setup.Map2Prj);
        }
      }
    }

    private void AddLegend(OcadWriter writer, Polyline border, Point rawPosition)
    {
      Point edge = null;

      border = border.Project(_templateSetup.Prj2Map);
      rawPosition = rawPosition.Project(_templateSetup.Prj2Map);

      double dDist = -1;
      foreach (Point pnt in border.Points)
      {
        double d2 = pnt.Dist2(rawPosition);
        if (dDist < 0 || dDist > d2)
        {
          dDist = d2;
          edge = pnt;
        }
      }
      if (edge == null)
      { return; }

      Point pos0 = new Point2D();
      Point pos1 = new Point2D();
      IBox boxBord = border.Extent;
      IBox boxLgd = _legendBox.Graphics[0].Geometry.Extent;
      double dx = boxLgd.Max.X - boxLgd.Min.X;
      double dy = boxLgd.Max.Y - boxLgd.Min.Y;

      if (Math.Abs(edge.X - border.Extent.Min.X) <
        Math.Abs(edge.X - border.Extent.Max.X))
      {
        pos0.X = boxBord.Min.X - boxLgd.Min.X;
        pos1.X = pos0.X + dx;
      }
      else
      {
        pos0.X = boxBord.Max.X - boxLgd.Max.X;
        pos1.X = pos0.X - dx;
      }
      if (Math.Abs(edge.Y - border.Extent.Min.Y) <
        Math.Abs(edge.Y - border.Extent.Max.Y))
      {
        pos0.Y = boxBord.Min.Y - boxLgd.Min.Y;
        pos1.Y = pos0.Y;
      }
      else
      {
        pos0.Y = boxBord.Max.Y - boxLgd.Max.Y;
        pos1.Y = pos0.Y;
      }

      Element elem = new ElementV9(true);
      elem.Geometry = pos0.Project(_templateSetup.Map2Prj);
      elem.Symbol = SymD.DtLegendeBox;
      elem.Type = GeomType.point;
      writer.Append(elem);

      elem = new ElementV9(true);
      elem.Geometry = pos1.Project(_templateSetup.Map2Prj);
      elem.Symbol = SymD.DtLegendeBox;
      elem.Type = GeomType.point;
      writer.Append(elem);

      elem = new ElementV9(true);
      elem.Geometry = pos0.Project(_templateSetup.Map2Prj);
      elem.Angle = _templateSetup.PrjRotation;
      elem.Symbol = SymD.DtNordpfeil;
      elem.Type = GeomType.point;
      writer.Append(elem);

      Point p = pos1.Project(_templateSetup.Map2Prj);
      elem = Common.CreateText("10 m", p.X, p.Y, _legendText, _templateSetup);
      writer.Append(elem);

      Polyline scale = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(10, 0) });
      scale = scale.Project(_templateSetup.Prj2Map);
      dx = scale.Project(Geometry.ToXY).Length() / 2.0;

      scale = new Polyline();
      scale.Add(new Point2D(pos1.X - dx, pos1.Y - dy / 10.0));
      scale.Add(new Point2D(pos1.X - dx, pos1.Y - dy / 5.0));
      scale.Add(new Point2D(pos1.X + dx, pos1.Y - dy / 5.0));
      scale.Add(new Point2D(pos1.X + dx, pos1.Y - dy / 10.0));

      elem = new ElementV9(true);
      elem.Geometry = scale.Project(_templateSetup.Map2Prj);
      elem.Symbol = SymD.DtMassstab;
      elem.Type = GeomType.line;
      writer.Append(elem);
    }
  }
}
