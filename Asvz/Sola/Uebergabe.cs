using Basics.Geom;
using Ocad;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Asvz.Sola
{
  /// <summary>
  /// Summary description for Uebergabe.
  /// </summary>
  public class Uebergabe
  {
    public class Info : IComparable<Info>
    {
      private readonly int _from;

      public Info(string name, int from, string vorlage, string karte)
      {
        Name = name;
        _from = from;
        Vorlage = vorlage;
        Karte = karte;
      }


      public string Name { get; }

      public int From
      {
        get { return _from; }
      }
      public int To
      {
        get { return _from + 1; }
      }
      public string Vorlage { get; }

      public string Karte { get; }

      public UebergabeTransport.Info Transport { get; set; }

      public int CompareTo(Info other)
      {
        return _from - other._from;
      }
    }

    private GeoElement _uebergabe;
    private GeoElement _legendPos;

    private Polyline _symUebergabe;
    private Polyline _symUeCircle;
    private Polyline _symUeNeustart;

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
        SymD.UeVorherNachher, SymD.UeNeustart, SymD.UeCircle, SymD.Uebergabe, SymD.Detail,
        SymD.GepBleibt, SymD.GepVon, SymD.GepNach, SymD.Sanitaet,
        SymD.Bus, SymD.Tram, SymD.Bahn, SymD.Lsb, SymD.WC, SymD.WC_D, SymD.WC_H, SymD.Imbiss,
        SymD.IdxRot, SymD.IdxSchwarz, SymD.UeAusserhalb,  SymD.Objekt,
        SymD.TextGross, SymD.TextMittel, SymD.BoxText,
        SymD.UeLegendeBox,  SymD.UeTextLegende, SymD.UeMassstab, SymD.UeNordpfeil
      };

    private readonly IList<SolaStrecke> _streckenList;
    private readonly int _strecke;

    private Setup _templateSetup;

    public Uebergabe(IList<SolaStrecke> strecken, int strecke)
    {
      _streckenList = strecken;
      _strecke = strecke;
    }

    public void Write(string outFile)
    {
      string template = Ddx.Uebergabe[_strecke].Vorlage;

      File.Copy(template, outFile, true);
      OcadWriter writer = OcadWriter.AppendTo(outFile);

      OcadReader pTemplate = OcadReader.Open(template);
      ReadTemplate(pTemplate);
      pTemplate.Close();

      try
      {
        Polyline border = GetBorder(_uebergabe, out Polyline partEnd, out Polyline partStart,
          out IPoint centerEnd, out IPoint centerStart);

        WriteStartEnd(writer, partStart, partEnd, centerStart, centerEnd);

        AddLegend(writer, border, ((GeoElement.Point)_legendPos.Geometry).BaseGeometry);

        AdaptText(writer, _templateSetup, _textList, _textSymbol);

        writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
        writer.SymbolsSetState(_symbols, Ocad.Symbol.SymbolStatus.Protected);

        writer.Remove(StringType.Template);

        WriteParams(writer, border);

        WriteKarte(writer);
      }
      finally
      {
        writer.Close();
      }
    }

    private void WriteKarte(OcadWriter writer)
    {
      TemplatePar tmpl;

      if (_mapTemplate != null)
      { tmpl = new TemplatePar(_mapTemplate); }
      else
      {
        tmpl = new TemplatePar();
        string sTmpl = Ddx.Uebergabe[_strecke].Karte;
        tmpl.Name = sTmpl;

        string sWorldFile = Path.GetDirectoryName(sTmpl) + Path.DirectorySeparatorChar +
          Path.GetFileNameWithoutExtension(sTmpl) + ".tfw";
        if (File.Exists(sWorldFile))
        {
          Grid.ImageGrid grd = Grid.ImageGrid.FromFile(sTmpl);
          int nx = grd.Extent.Nx;
          int ny = grd.Extent.Nx;
          grd.Close();

          TextReader pWorld = new StreamReader(sWorldFile);

          tmpl.Georeference(
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
      tmpl.Dim = 50;
      tmpl.Transparent = false;
      tmpl.Visible = true;

      writer.Overwrite(StringType.Template, 0, tmpl.StringPar);
    }

    private void WriteParams(OcadWriter writer, Polyline border)
    {
      if (_viewParam != null)
      {
        Point viewCenter = 0.5 * (Point.Create(border.Extent.Min) +
          Point.Create(border.Extent.Max));
        viewCenter = viewCenter.Project(_templateSetup.Prj2Map);
        _viewParam.XOffset = viewCenter.X / 100;
        _viewParam.YOffset = viewCenter.Y / 100;

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
        _exportParam.Resolution = 300.0;
        writer.Overwrite(StringType.Export, 0, _exportParam.StringPar);
      }
    }

    private void WriteStartEnd(OcadWriter writer, Polyline partStart, Polyline partEnd,
      IPoint centerStart, IPoint centerEnd)
    {
      Element elem;

      if (centerEnd != null)
      {
        elem = new GeoElement(centerEnd);
        elem.Symbol = SymD.UeCircle;
        elem.Type = GeomType.point;
        writer.Append(elem);
      }
      if (partEnd != null)
      {
        elem = new GeoElement(partEnd);
        elem.Symbol = SymD.UeVorherNachher;
        elem.Type = GeomType.line;
        writer.Append(elem);
      }

      if (centerStart != null && centerStart != centerEnd)
      {
        elem = new GeoElement(centerStart);
        elem.Symbol = SymD.UeNeustart;
        elem.Type = GeomType.point;

        IPoint p1 = PointOp.Project(partStart.Points[0], _templateSetup.Prj2Map);
        IPoint p0 = PointOp.Project(centerStart, _templateSetup.Prj2Map);
        Point p = PointOp.Sub(p1, p0);
        double angle = -Math.Atan2(p.X, p.Y);
        elem.Angle = angle;
        writer.Append(elem);
      }
      if (partStart != null)
      {
        elem = new GeoElement(partStart);
        elem.Symbol = SymD.UeVorherNachher;
        elem.Type = GeomType.line;
        writer.Append(elem);
      }
    }

    private Polyline GetBorder(GeoElement border, out Polyline partEnd, out Polyline partStart,
      out IPoint centerEnd, out IPoint centerStart)
    {
      // Get Uebergabe Geometry
      Setup pSetup = new Setup();
      IPoint p = ((GeoElement.Point)border.Geometry).BaseGeometry;
      pSetup.PrjTrans.X = p.X;
      pSetup.PrjTrans.Y = p.Y;
      pSetup.Scale = 1 / FileParam.OCAD_UNIT;
      pSetup.PrjRotation = border.Angle + _templateSetup.PrjRotation;
      Polyline symBorder = _symUebergabe.Project(pSetup.Map2Prj);

      List<ParamGeometryRelation> pEnd = null;
      List<ParamGeometryRelation> pStart = null;

      partEnd = null;
      partStart = null;

      Polyline streckeEnd = null;
      Polyline streckeStart = null;

      if (_strecke > 0)
      {
        streckeEnd = _streckenList[_strecke - 1].GetCategorie(Kategorie.Default).Strecke;
        pEnd = new List<ParamGeometryRelation>(GeometryOperator.CreateRelations(symBorder, streckeEnd));
        Trace.Assert(pEnd != null);
      }

      if (_strecke < _streckenList.Count)
      {
        streckeStart = _streckenList[_strecke].GetCategorie(Kategorie.Default).Strecke;
        pStart = new List<ParamGeometryRelation>(GeometryOperator.CreateRelations(symBorder, streckeStart));
        Trace.Assert(pStart != null);
      }

      ParamGeometryRelation cutBorder;
      ParamGeometryRelation cutCircle;

      centerEnd = null;
      centerStart = null;

      if (streckeEnd != null)
      { centerEnd = streckeEnd.Points.Last(); }
      if (streckeStart != null)
      { centerStart = streckeStart.Points[0]; }

      if (centerEnd != null && centerStart != null && PointOp.Dist2(centerEnd, centerStart) < 100)
      { centerStart = centerEnd; }

      Polyline circle = null;

      if (pEnd != null)
      {
        cutBorder = pEnd[pEnd.Count - 1];
        circle = Circle(centerEnd, Radius(_symUeCircle));
        List<ParamGeometryRelation> pList = new List<ParamGeometryRelation>(GeometryOperator.CreateRelations(streckeEnd, circle));
        cutCircle = pList[pList.Count - 1];
        partEnd = streckeEnd.Split(new[] { cutBorder, cutCircle })[1];
      }
      if (pStart != null)
      {
        cutBorder = pStart[0];

        if (centerStart != centerEnd)
        { circle = Circle(centerStart, Radius(_symUeNeustart)); }

        List<ParamGeometryRelation> pList = new List<ParamGeometryRelation>(GeometryOperator.CreateRelations(streckeStart, circle));
        cutCircle = pList[0];
        partStart = streckeStart.Split(new[] { cutCircle, cutBorder })[1];
      }

      return symBorder;
    }

    private double Radius(Polyline symbol)
    {
      double r2 = PointOp.OrigDist2(symbol.Points[0]);
      double r = Math.Sqrt(r2);
      return r;
    }

    private Polyline Circle(IPoint center, double radius)
    {
      Arc arc = new Arc(center, radius, 0, 2 * Math.PI);
      Polyline circle = new Polyline();
      circle.Add(arc);
      return circle;
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

      _uebergabe = null;
      _legendPos = null;
      _textList = new List<GeoElement>();

      foreach (var elem in template.EnumGeoElements(pIndexList))
      {
        if (elem.Symbol == SymD.Uebergabe)
        {
          Trace.Assert(_uebergabe == null, "Multiple Uebergabe");
          _uebergabe = elem;
        }
        else if (elem.Symbol == SymD.UeNordRoh)
        {
          Trace.Assert(_legendPos == null, "Multiple Legendeposition");
          _legendPos = elem;
        }
        else if (elem.Symbol == SymD.TextGross)
        { _textList.Add(elem); }
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
        Ocad.Symbol.BaseSymbol symbol = reader.ReadSymbol(iPos);
        if (symbol == null)
        { continue; }

        if (symbol.Number == SymD.TextGross)
        { _textSymbol = (Ocad.Symbol.TextSymbol)symbol; }
        if (symbol.Number == SymD.UeTextLegende)
        { _legendText = (Ocad.Symbol.TextSymbol)symbol; }
        if (symbol.Number == SymD.UeLegendeBox)
        { _legendBox = (Ocad.Symbol.PointSymbol)symbol; }

        if (symbol.Number == SymD.Uebergabe)
        {
          if (!(symbol.Graphics.Count == 1)) throw new InvalidOperationException($"Expected 1 'Uebergabe', got {symbol.Graphics.Count}");
          _symUebergabe = ((GeoElement.Line)symbol.Graphics[0].MapGeometry).Project(pSetup.Map2Prj).BaseGeometry;
        }
        else if (symbol.Number == SymD.UeCircle)
        {
          if (!(symbol.Graphics.Count == 1)) throw new InvalidOperationException($"Expected 1 'UeCircle', got {symbol.Graphics.Count}");
          _symUeCircle = ((GeoElement.Line)symbol.Graphics[0].MapGeometry).Project(pSetup.Map2Prj).BaseGeometry;
        }
        else if (symbol.Number == SymD.UeNeustart)
        {
          if (!(symbol.Graphics.Count == 1)) throw new InvalidOperationException($"Expected 1 'UeNeuStart', got {symbol.Graphics.Count}");
          _symUeNeustart = ((GeoElement.Line)symbol.Graphics[0].MapGeometry).Project(pSetup.Map2Prj).BaseGeometry;
        }
      }
    }

    private void ReadStringParams(OcadReader template)
    {
      IList<StringParamIndex> strIdxList = template.ReadStringParamIndices();
      foreach (var strIdx in strIdxList)
      {
        if (strIdx.Type == StringType.Template)
        {
          string sTemp = template.ReadStringParam(strIdx);
          TemplatePar pTpl = new TemplatePar(sTemp);
          if (pTpl.Name == Ddx.Uebergabe[_strecke].Karte)
          {
            _mapTemplate = sTemp;
          }
        }
        else if (strIdx.Type == StringType.ViewPar)
        { _viewParam = new ViewPar(template.ReadStringParam(strIdx)); }
        else if (strIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(template.ReadStringParam(strIdx)); }
        else if (strIdx.Type == StringType.Export)
        { _exportParam = new ExportPar(template.ReadStringParam(strIdx)); }
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

      foreach (var text in textList)
      {
        PointCollection pList = ((GeoElement.Points)text.Geometry).BaseGeometry;
        pList = pList.Project(setup.Prj2Map);

        Polyline line = new Polyline();

        line.Add(new Point2D(pList[1].X - dx0, pList[1].Y - dy0));
        line.Add(new Point2D(pList[2].X + dx1, pList[2].Y - dy0));
        line.Add(new Point2D(pList[3].X + dx1, pList[3].Y + dy1));
        line.Add(new Point2D(pList[4].X - dx0, pList[4].Y + dy1));
        line.Add(new Point2D(pList[1].X - dx0, pList[1].Y - dy0));

        line = line.Project(setup.Map2Prj);

        Element pElem = new GeoElement(line);
        pElem.Symbol = SymD.BoxText;
        pElem.Type = GeomType.line;
        writer.Append(pElem);
      }
    }

    private void AddLegend(OcadWriter writer, Polyline border, IPoint rawPosition)
    {
      IPoint edge = null;

      border = border.Project(_templateSetup.Prj2Map);
      rawPosition = PointOp.Project(rawPosition, _templateSetup.Prj2Map);

      double dDist = -1;
      foreach (var pnt in border.Points)
      {
        double d2 = PointOp.Dist2(pnt, rawPosition);
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
      IBox boxLgd = _legendBox.Graphics[0].MapGeometry.Extent;
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

      Element elem = new GeoElement(pos0.Project(_templateSetup.Map2Prj));
      elem.Symbol = SymD.UeLegendeBox;
      elem.Type = GeomType.point;
      writer.Append(elem);

      elem = new GeoElement(pos1.Project(_templateSetup.Map2Prj));
      elem.Symbol = SymD.UeLegendeBox;
      elem.Type = GeomType.point;
      writer.Append(elem);

      elem = new GeoElement(pos0.Project(_templateSetup.Map2Prj));
      elem.Angle = _templateSetup.PrjRotation;
      elem.Symbol = SymD.UeNordpfeil;
      elem.Type = GeomType.point;
      writer.Append(elem);

      Point p = pos1.Project(_templateSetup.Map2Prj);
      elem = Common.CreateText("50 m", p.X, p.Y, _templateSetup, _legendText);
      writer.Append(elem);

      Polyline pScale = Polyline.Create(new[] { new Point2D(0, 0), new Point2D(50, 0) });
      pScale = pScale.Project(_templateSetup.Prj2Map);
      dx = pScale.Project(Geometry.ToXY).Length() / 2.0;

      pScale = new Polyline();
      pScale.Add(new Point2D(pos1.X - dx, pos1.Y - dy / 10.0));
      pScale.Add(new Point2D(pos1.X - dx, pos1.Y - dy / 5.0));
      pScale.Add(new Point2D(pos1.X + dx, pos1.Y - dy / 5.0));
      pScale.Add(new Point2D(pos1.X + dx, pos1.Y - dy / 10.0));

      elem = new GeoElement(pScale.Project(_templateSetup.Map2Prj));
      elem.Symbol = SymD.UeMassstab;
      elem.Type = GeomType.line;
      writer.Append(elem);
    }
  }
}
