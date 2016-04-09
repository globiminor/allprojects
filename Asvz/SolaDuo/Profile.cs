using System;
using System.Diagnostics;
using System.Collections.Generic;
using Ocad;
using Basics.Geom;
using Ocad.StringParams;

namespace Asvz
{
  public class Profile
  {
    // profil parameter
    private readonly double _fHeight = 10;
    private double _wald0, _dWald1;
    private double _haus0, _dHaus1;

    // Layout
    private readonly double _teerUnten = 250.0;
    private readonly double _teerOben = 300.0;

    private readonly double _topStrecke = 1050.0;
    private readonly double _topDist = 1000.0;
    private readonly double _bottomDist = 180.0;
    private readonly double _bottomOrt = 80.0;

    private readonly Data _data;

    private Ocad.Symbol.TextSymbol _symTextStrecke;
    private Ocad.Symbol.TextSymbol _symTextOrt;
    private Ocad.Symbol.TextSymbol _symTextDist;
    private Ocad.Symbol.TextSymbol _symTextH;

    private readonly double _dOffsetDist = 300.0;
    private Setup _templateSetup;

    private PrintPar _printParam;

    public string FormatDist = "N2";

    public Profile(Data data)
    {
      _data = data;
    }

    protected void WriteLayout(Ocad9Writer writer, double sumDist)
    {
      Element elem = new ElementV9(true);
      elem.Symbol = ProfileSymbol.Grenze;
      elem.Geometry = Polyline.Create(new []
          {
            new Point2D(sumDist / _fHeight, _bottomDist),
            new Point2D(sumDist / _fHeight, _topDist)
          });
      elem.Type = GeomType.line;
      writer.Append(elem);

      elem = Common.CreateText("MüM", -50, 1000, _symTextH, _templateSetup);
      writer.Append(elem);
      elem = Common.CreateText("900", -50, 900, _symTextH, _templateSetup);
      writer.Append(elem);
      elem = Common.CreateText("800", -50, 800, _symTextH, _templateSetup);
      writer.Append(elem);
      elem = Common.CreateText("700", -50, 700, _symTextH, _templateSetup);
      writer.Append(elem);
      elem = Common.CreateText("600", -50, 600, _symTextH, _templateSetup);
      writer.Append(elem);
      elem = Common.CreateText("500", -50, 500, _symTextH, _templateSetup);
      writer.Append(elem);
      elem = Common.CreateText("400", -50, 400, _symTextH, _templateSetup);
      writer.Append(elem);

      double dRaster = 0;
      while (dRaster < sumDist)
      {
        elem = new ElementV9(true);
        elem.Symbol = ProfileSymbol.Raster;
        elem.Geometry = Polyline.Create(new []
          {
            new Point2D(dRaster / _fHeight, 300.0),
            new Point2D(dRaster / _fHeight, 900.0)
          });
        elem.Type = GeomType.line;
        writer.Append(elem);

        dRaster += 1000;
      }


      elem = new ElementV9(true);
      elem.Symbol = ProfileSymbol.Raster;
      elem.Geometry = Polyline.Create(new []
          {
            new Point2D(0,_teerUnten),
            new Point2D(sumDist / _fHeight, _teerUnten)
          });
      elem.Type = GeomType.line;
      writer.Append(elem);

      elem = new ElementV9(true);
      elem.Symbol = ProfileSymbol.Raster;
      elem.Geometry = Polyline.Create(new []
          {
            new Point2D(0,_teerOben),
            new Point2D(sumDist / _fHeight, _teerOben)
          });
      elem.Type = GeomType.line;
      writer.Append(elem);

      for (dRaster = 100 * (int)(1 + _teerOben / 100); dRaster < 950; dRaster += 100)
      {
        elem = new ElementV9(true);
        elem.Symbol = ProfileSymbol.Raster;
        elem.Geometry = Polyline.Create(new []
          {
            new Point2D(0,dRaster),
            new Point2D(sumDist / _fHeight, dRaster)
          });
        elem.Type = GeomType.line;
        writer.Append(elem);
      }
    }

    protected void InitHausWald(out double nextHaus, out double nextWald)
    {
      nextHaus = -_haus0;
      nextWald = -_wald0;
    }
    protected void WriteStart(Ocad9Writer writer, string text, double distStart, double sumDist)
    {
      string sText = string.Format("{0:" + FormatDist + "} km", distStart / 1000.0);
      Element elem = Common.CreateText(sText, (sumDist + 100.0) / _fHeight,
        _bottomDist, _symTextDist, _templateSetup);
      writer.Append(elem);

      sText = text;
      elem = Common.CreateText(sText, sumDist, _bottomOrt, _symTextOrt, _templateSetup);
      writer.Append(elem);
    }

    protected void WriteEnd(Ocad9Writer writer, string text, double sumDist)
    {
      Element elem = Common.CreateText(text, sumDist / _fHeight,
        _topStrecke, _symTextStrecke, _templateSetup);
      writer.Append(elem);
    }

    protected void WriteParams(Ocad9Writer writer, double sumDist)
    {
      if (_printParam == null)
      { _printParam = new PrintPar(); }
      if (_printParam != null)
      {
        _printParam.Scale = 10000;
        _printParam.Range = PrintPar.RangeType.PartialMap;
        _printParam.Left = -28.0;
        _printParam.Right = sumDist / _fHeight / 10.0 + 32.0;
        _printParam.Top = 124.0;
        _printParam.Bottom = -1.5;

        writer.Overwrite(StringType.PrintPar, 0, _printParam.StringPar);
      }
    }
    protected void WriteProfile(Ocad9Writer writer, Polyline profile, double sumDist)
    {
      double dist = profile.Points.Last.Value.X;

      //Console.WriteLine(
      //  "{0,2}:    {1,6:N2}    {2,3:N0}    {3,5:N1}     {4,6:N1}    {5,6:N1}",
      //  iStrecke + 1, dist / 1000.0, iSteigung5m, (dist + sumDist) / 1000.0,
      //  profile.Points.First.Value.Y, profile.Points.Last.Value.Y);

      Polyline pLine = profile.Clone();
      pLine.Add(new Point2D(pLine.Points.Last.Value.X, 300));
      pLine.Add(new Point2D(pLine.Points.First.Value.X, 300));
      pLine.Add(new Point2D(pLine.Points.First.Value.X, pLine.Points.First.Value.Y));
      foreach (IPoint p in pLine.Points)
      {
        p.X = (p.X + sumDist) / _fHeight;
      }
      Area area = new Area(pLine);

      Element elem = new ElementV9(true);
      elem.Symbol = ProfileSymbol.Profile;
      elem.Geometry = area;
      elem.Type = GeomType.area;

      writer.Append(elem);
    }

    protected void WriteLayoutStrecke(Ocad9Writer writer, Categorie cat, string startName,
      string zielName, double sumDist, double distStart)
    {
      double dist = cat.Laenge();
      double steigung = cat.Steigung;
      int iSteigung5M = (int)cat.SteigungRound(5);

      string text = string.Format("{0:" + FormatDist + "} km" + Environment.NewLine +
                                  "{1} m\u2191", dist / 1000.0, iSteigung5M);
      Element elem = Common.CreateText(text, (sumDist + _dOffsetDist) / _fHeight,
        _topDist, _symTextDist, _templateSetup);
      writer.Append(elem);

      text = string.Format("{0}", startName);
      elem = Common.CreateText(text, sumDist / _fHeight,
        _topStrecke, _symTextStrecke, _templateSetup);
      writer.Append(elem);

      elem = new ElementV9(true);
      elem.Symbol = ProfileSymbol.Grenze;
      elem.Geometry = Polyline.Create(new []
          {
            new Point2D(sumDist / _fHeight, _bottomDist),
            new Point2D(sumDist / _fHeight, _topDist)
          });
      elem.Type = GeomType.line;
      writer.Append(elem);

      sumDist += dist;

      text = string.Format("{0:" + FormatDist + "} km", (distStart + sumDist) / 1000.0);
      elem = Common.CreateText(text, (sumDist + _dOffsetDist) / _fHeight,
        _bottomDist, _symTextDist, _templateSetup);
      writer.Append(elem);

      text = zielName;
      elem = Common.CreateText(text, sumDist / _fHeight,
        _bottomOrt, _symTextOrt, _templateSetup);
      writer.Append(elem);
    }

    protected void WriteUmgebung(Ocad9Writer writer, Polyline strecke, Polyline profile,
      double sumDist, Random random, ref double nextWald, ref double nextHaus)
    {
      bool[] bWald = Intersect(strecke, _data.Wald);
      bool[] bSied = Intersect(strecke, _data.Siedlung);
      bool[] bTeer = Intersect(strecke, _data.Teer);

      int iPoint = 0;
      double dTeerStart = 0;
      double dTeerEnd = dTeerStart - 1;
      foreach (Curve pSeg in profile.Segments)
      {
        double dStart = pSeg.Start.X + sumDist;
        double dEnd = pSeg.End.X + sumDist;

        if (bWald[iPoint] == false && bWald[iPoint + 1])
        { nextWald = Math.Max(nextWald, dEnd - _wald0); }
        if (bSied[iPoint] == false && bSied[iPoint + 1])
        { nextHaus = Math.Max(nextHaus, dEnd - _haus0); }

        Element elem;
        if (bWald[iPoint] && bWald[iPoint + 1] && dEnd > nextWald)
        {
          double dX = nextWald;
          double dH = pSeg.Start.Y +
            (pSeg.End.Y - pSeg.Start.Y) * (dX - dStart) / (dEnd - dStart);
          nextWald = dX + (_dWald1 - _wald0);
          nextHaus = dX + (_dWald1 - _haus0);

          elem = new ElementV9(true);
          elem.Symbol = ProfileSymbol.Baum + random.Next(2);
          elem.Geometry = new Point2D(dX / _fHeight, dH);
          elem.Type = GeomType.point;

          writer.Append(elem);
        }
        else if (bSied[iPoint] && bSied[iPoint + 1] && dEnd > nextHaus)
        {
          double dX = nextHaus;
          double dH = pSeg.Start.Y +
            (pSeg.End.Y - pSeg.Start.Y) * (dX - dStart) / (dEnd - dStart);

          nextWald = dX + (_dHaus1 - _wald0);
          nextHaus = dX + (_dHaus1 - _haus0);

          elem = new ElementV9(true);
          elem.Symbol = ProfileSymbol.Haus;
          elem.Geometry = new Point2D(dX / _fHeight, dH);
          elem.Type = GeomType.point;

          writer.Append(elem);
        }
        if (bTeer[iPoint] && bTeer[iPoint + 1])
        {
          if (dTeerEnd < dTeerStart)
          { dTeerStart = pSeg.Start.X; }
          dTeerEnd = pSeg.End.X;
        }
        else
        {
          if (dTeerEnd > dTeerStart)
          {
            WriteTeer(writer, (dTeerStart + sumDist) / _fHeight,
              (dTeerEnd + sumDist) / _fHeight, ProfileSymbol.Teer);
          }
          dTeerEnd = dTeerStart - 1;
        }

        iPoint++;
      }
      if (dTeerEnd > dTeerStart)
      {
        WriteTeer(writer, (dTeerStart + sumDist) / _fHeight,
          (dTeerEnd + sumDist) / _fHeight, ProfileSymbol.Teer);
      }
    }

    private void WriteTeer(Ocad9Writer writer, double start, double end, int symbol)
    {
      Element pElem = new ElementV9(true);
      pElem.Symbol = symbol;
      pElem.Geometry = new Area(Polyline.Create(new []
        {
          new Point2D(start, _teerUnten),
          new Point2D(end, _teerUnten),
          new Point2D(end, _teerOben),
          new Point2D(start, _teerOben),
          new Point2D(start, _teerUnten)
        }));
      pElem.Type = GeomType.area;

      writer.Append(pElem);
    }

    private static bool[] Intersect(Polyline line, IList<Area> polygons)
    {
      bool[] inside = new bool[line.Points.Count];
      int iPoint = 0;
      foreach (IPoint pnt in line.Points)
      {
        foreach (Area poly in polygons)
        {
          if (poly.Extent.Intersects(pnt.Extent) && poly.IsWithin(pnt))
          {
            inside[iPoint] = true;
            break;
          }
        }
        iPoint++;
      }
      return inside;
    }

    protected void ReadTemplate(Ocad9Reader template)
    {
      _templateSetup = template.ReadSetup();

      ReadObjects(template);
      ReadSymbols(template);
      ReadStringParams(template);
    }

    private void ReadObjects(Ocad9Reader template)
    {
      ElementIndex elemIdx;
      IList<ElementIndex> indexList = new List<ElementIndex>();
      int iIndex = 0;
      while ((elemIdx = template.ReadIndex(iIndex)) != null)
      {
        indexList.Add(elemIdx);
        iIndex++;
      }

      foreach (Element elem in template.Elements(true, indexList))
      {
        // nothing needed yet
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

      foreach (int iPos in pIndexList)
      {
        IBox pExtent;
        Ocad.Symbol.BaseSymbol pSymbol = reader.ReadSymbol(iPos);
        if (pSymbol == null)
        { continue; }

        if (pSymbol.Number == ProfileSymbol.Haus)
        {
          pExtent = pSymbol.Graphics.Extent().Project(_templateSetup.Map2Prj).Extent;
          _haus0 = _fHeight * pExtent.Min.X;
          _dHaus1 = _fHeight * pExtent.Max.X;
        }
        else if (pSymbol.Number == ProfileSymbol.Baum)
        {
          pExtent = pSymbol.Graphics.Extent().Project(_templateSetup.Map2Prj).Extent;
          _wald0 = _fHeight * pExtent.Min.X;
          _dWald1 = _fHeight * pExtent.Max.X;
        }

        else if (pSymbol.Number == ProfileSymbol.TxtStrecke)
        { _symTextStrecke = (Ocad.Symbol.TextSymbol)pSymbol; }
        else if (pSymbol.Number == ProfileSymbol.TxtOrt)
        { _symTextOrt = (Ocad.Symbol.TextSymbol)pSymbol; }
        else if (pSymbol.Number == ProfileSymbol.TextDist)
        { _symTextDist = (Ocad.Symbol.TextSymbol)pSymbol; }
        else if (pSymbol.Number == ProfileSymbol.TextH)
        { _symTextH = (Ocad.Symbol.TextSymbol)pSymbol; }
      }
    }

    private void ReadStringParams(Ocad9Reader template)
    {
      Debug.Assert(template != null);
      IList<StringParamIndex> strIdxList = template.ReadStringParamIndices();
      foreach (StringParamIndex strIdx in strIdxList)
      {
        if (strIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(template.ReadStringParam(strIdx)); }
      }
    }
  }
}
