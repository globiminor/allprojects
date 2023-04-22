using Basics.Geom;
using Ocad;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.Linq;

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

    protected void WriteLayout(OcadWriter writer, double sumDist)
    {
      Element elem = new GeoElement(Polyline.Create(new[]
          {
            new Point2D(sumDist / _fHeight, _bottomDist),
            new Point2D(sumDist / _fHeight, _topDist)
          }));
      elem.Symbol = ProfileSymbol.Grenze;
      elem.Type = GeomType.line;
      writer.Append(elem);

      elem = Common.CreateText("MüM", -50, 1000, _templateSetup, _symTextH);
      writer.Append(elem);
      elem = Common.CreateText("900", -50, 900, _templateSetup, _symTextH);
      writer.Append(elem);
      elem = Common.CreateText("800", -50, 800, _templateSetup, _symTextH);
      writer.Append(elem);
      elem = Common.CreateText("700", -50, 700, _templateSetup, _symTextH);
      writer.Append(elem);
      elem = Common.CreateText("600", -50, 600, _templateSetup, _symTextH);
      writer.Append(elem);
      elem = Common.CreateText("500", -50, 500, _templateSetup, _symTextH);
      writer.Append(elem);
      elem = Common.CreateText("400", -50, 400, _templateSetup, _symTextH);
      writer.Append(elem);

      double dRaster = 0;
      while (dRaster < sumDist)
      {
        elem = new GeoElement(Polyline.Create(new[]
          {
            new Point2D(dRaster / _fHeight, 300.0),
            new Point2D(dRaster / _fHeight, 900.0)
          }));
        elem.Symbol = ProfileSymbol.Raster;
        elem.Type = GeomType.line;
        writer.Append(elem);

        dRaster += 1000;
      }


      elem = new GeoElement(Polyline.Create(new[]
          {
            new Point2D(0,_teerUnten),
            new Point2D(sumDist / _fHeight, _teerUnten)
          }));
      elem.Symbol = ProfileSymbol.Raster;
      elem.Type = GeomType.line;
      writer.Append(elem);

      elem = new GeoElement(Polyline.Create(new[]
          {
            new Point2D(0,_teerOben),
            new Point2D(sumDist / _fHeight, _teerOben)
          }));
      elem.Symbol = ProfileSymbol.Raster;
      elem.Type = GeomType.line;
      writer.Append(elem);

      for (dRaster = 100 * (int)(1 + _teerOben / 100); dRaster < 950; dRaster += 100)
      {
        elem = new GeoElement(Polyline.Create(new[]
          {
            new Point2D(0,dRaster),
            new Point2D(sumDist / _fHeight, dRaster)
          }));
        elem.Symbol = ProfileSymbol.Raster;
        elem.Type = GeomType.line;
        writer.Append(elem);
      }
    }

    protected void InitHausWald(out double nextHaus, out double nextWald)
    {
      nextHaus = -_haus0;
      nextWald = -_wald0;
    }
    protected void WriteStart(OcadWriter writer, string text, double distStart, double sumDist)
    {
      string sText = string.Format("{0:" + FormatDist + "} km", distStart / 1000.0);
      Element elem = Common.CreateText(sText, (sumDist + 100.0) / _fHeight,
        _bottomDist, _templateSetup, _symTextDist);
      writer.Append(elem);

      sText = text;
      elem = Common.CreateText(sText, sumDist, _bottomOrt, _templateSetup, _symTextOrt);
      writer.Append(elem);
    }

    protected void WriteEnd(OcadWriter writer, string text, double sumDist)
    {
      Element elem = Common.CreateText(text, sumDist / _fHeight,
        _topStrecke, _templateSetup, _symTextStrecke);
      writer.Append(elem);
    }

    protected void WriteParams(OcadWriter writer, double sumDist)
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
    protected void WriteProfile(OcadWriter writer, Polyline profile, double sumDist)
    {
      double dist = profile.Points.Last().X;

      //Console.WriteLine(
      //  "{0,2}:    {1,6:N2}    {2,3:N0}    {3,5:N1}     {4,6:N1}    {5,6:N1}",
      //  iStrecke + 1, dist / 1000.0, iSteigung5m, (dist + sumDist) / 1000.0,
      //  profile.Points.First.Value.Y, profile.Points.Last.Value.Y);

      List<Point> points = new List<Point>(profile.Points.Select(x => Point.Create(x)));
      points.Add(new Point2D(profile.Points.Last().X, 300));
      points.Add(new Point2D(profile.Points[0].X, 300));
      points.Add(new Point2D(profile.Points[0].X, profile.Points[0].Y));
      foreach (var p in points)
      {
        p.X = (p.X + sumDist) / _fHeight;
      }
      Surface area = new Surface(Polyline.Create(points));

      Element elem = new GeoElement(area);
      elem.Symbol = ProfileSymbol.Profile;
      elem.Type = GeomType.area;

      writer.Append(elem);
    }

    protected void WriteLayoutStrecke(OcadWriter writer, Categorie cat, string startName,
      string zielName, double sumDist, double distStart)
    {
      double dist = cat.DispLength;
      double steigung = cat.Steigung;
      int iSteigung5M = (int)cat.SteigungRound(5);

      string text = string.Format("{0:" + FormatDist + "} km" + Environment.NewLine +
                                  "{1} m\u2191", dist / 1000.0, iSteigung5M);
      Element elem = Common.CreateText(text, (sumDist + _dOffsetDist) / _fHeight,
        _topDist, _templateSetup, _symTextDist);
      writer.Append(elem);

      text = string.Format("{0}", startName);
      elem = Common.CreateText(text, sumDist / _fHeight,
        _topStrecke, _templateSetup, _symTextStrecke);
      writer.Append(elem);

      elem = new GeoElement(Polyline.Create(new[]
          {
            new Point2D(sumDist / _fHeight, _bottomDist),
            new Point2D(sumDist / _fHeight, _topDist)
          }));
      elem.Symbol = ProfileSymbol.Grenze;
      elem.Type = GeomType.line;
      writer.Append(elem);

      sumDist += dist;

      text = string.Format("{0:" + FormatDist + "} km", (distStart + sumDist) / 1000.0);
      elem = Common.CreateText(text, (sumDist + _dOffsetDist) / _fHeight,
        _bottomDist, _templateSetup, _symTextDist);
      writer.Append(elem);

      text = zielName;
      elem = Common.CreateText(text, sumDist / _fHeight,
        _bottomOrt, _templateSetup, _symTextOrt);
      writer.Append(elem);
    }

    protected void WriteUmgebung(OcadWriter writer, Polyline strecke, Polyline profile,
      double sumDist, Random random, ref double nextWald, ref double nextHaus)
    {
      bool[] bWald = Intersect(strecke, _data.Wald);
      bool[] bSied = Intersect(strecke, _data.Siedlung);
      bool[] bTeer = Intersect(strecke, _data.Teer);

      int iPoint = 0;
      double dTeerStart = 0;
      double dTeerEnd = dTeerStart - 1;
      foreach (var seg in profile.EnumSegments())
      {
        double dStart = seg.Start.X + sumDist;
        double dEnd = seg.End.X + sumDist;

        if (bWald[iPoint] == false && bWald[iPoint + 1])
        { nextWald = Math.Max(nextWald, dEnd - _wald0); }
        if (bSied[iPoint] == false && bSied[iPoint + 1])
        { nextHaus = Math.Max(nextHaus, dEnd - _haus0); }

        Element elem;
        if (bWald[iPoint] && bWald[iPoint + 1] && dEnd > nextWald)
        {
          double dX = nextWald;
          double dH = seg.Start.Y +
            (seg.End.Y - seg.Start.Y) * (dX - dStart) / (dEnd - dStart);
          nextWald = dX + (_dWald1 - _wald0);
          nextHaus = dX + (_dWald1 - _haus0);

          elem = new GeoElement(new Point2D(dX / _fHeight, dH));
          elem.Symbol = ProfileSymbol.Baum + random.Next(2);
          elem.Type = GeomType.point;

          writer.Append(elem);
        }
        else if (bSied[iPoint] && bSied[iPoint + 1] && dEnd > nextHaus)
        {
          double dX = nextHaus;
          double dH = seg.Start.Y +
            (seg.End.Y - seg.Start.Y) * (dX - dStart) / (dEnd - dStart);

          nextWald = dX + (_dHaus1 - _wald0);
          nextHaus = dX + (_dHaus1 - _haus0);

          elem = new GeoElement(new Point2D(dX / _fHeight, dH));
          elem.Symbol = ProfileSymbol.Haus;
          elem.Type = GeomType.point;

          writer.Append(elem);
        }
        if (bTeer[iPoint] && bTeer[iPoint + 1])
        {
          if (dTeerEnd < dTeerStart)
          { dTeerStart = seg.Start.X; }
          dTeerEnd = seg.End.X;
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

    private void WriteTeer(OcadWriter writer, double start, double end, int symbol)
    {
      Element pElem = new GeoElement(new Surface(Polyline.Create(new[]
        {
          new Point2D(start, _teerUnten),
          new Point2D(end, _teerUnten),
          new Point2D(end, _teerOben),
          new Point2D(start, _teerOben),
          new Point2D(start, _teerUnten)
        })));
      pElem.Symbol = symbol;
      pElem.Type = GeomType.area;

      writer.Append(pElem);
    }

    private static bool[] Intersect(Polyline line, IList<Surface> polygons)
    {
      bool[] inside = new bool[line.Points.Count];
      int iPoint = 0;
      foreach (var pnt in line.Points)
      {
        foreach (var poly in polygons)
        {
          if (BoxOp.Intersects(poly.Extent, Point.CastOrWrap(pnt).Extent) && poly.IsWithin(pnt))
          {
            inside[iPoint] = true;
            break;
          }
        }
        iPoint++;
      }
      return inside;
    }

    protected void ReadTemplate(OcadReader template)
    {
      _templateSetup = template.ReadSetup();

      ReadObjects(template);
      ReadSymbols(template);
      ReadStringParams(template);
    }

    private void ReadObjects(OcadReader template)
    {
      ElementIndex elemIdx;
      IList<ElementIndex> indexList = new List<ElementIndex>();
      int iIndex = 0;
      while ((elemIdx = template.ReadIndex(iIndex)) != null)
      {
        indexList.Add(elemIdx);
        iIndex++;
      }

      foreach (var elem in template.EnumGeoElements(indexList))
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

      foreach (var iPos in pIndexList)
      {
        IBox pExtent;
        Ocad.Symbol.BaseSymbol pSymbol = reader.ReadSymbol(iPos);
        if (pSymbol == null)
        { continue; }

        if (pSymbol.Number == ProfileSymbol.Haus)
        {
          pExtent = BoxOp.ProjectRaw(pSymbol.Graphics.Extent(), _templateSetup.Map2Prj).Extent;
          _haus0 = _fHeight * pExtent.Min.X;
          _dHaus1 = _fHeight * pExtent.Max.X;
        }
        else if (pSymbol.Number == ProfileSymbol.Baum)
        {
          pExtent = BoxOp.ProjectRaw(pSymbol.Graphics.Extent(), _templateSetup.Map2Prj).Extent;
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

    private void ReadStringParams(OcadReader template)
    {
      IList<StringParamIndex> strIdxList = template.ReadStringParamIndices();
      foreach (var strIdx in strIdxList)
      {
        if (strIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(template.ReadStringParam(strIdx)); }
      }
    }
  }
}
