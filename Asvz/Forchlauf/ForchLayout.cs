using Basics.Geom;
using Ocad;
using System;
using System.Collections.Generic;

namespace Asvz.Forchlauf
{
  public class ForchLayout : Layout
  {
    private Ocad.Symbol.TextSymbol _kmTxtSymbol;
    private Ocad.Symbol.PointSymbol _startSymbol;
    private Ocad.Symbol.TextSymbol _txtSchwarz;
    private Ocad.Symbol.TextSymbol _txtLang;
    private Ocad.Symbol.TextSymbol _txtMittel;
    private Ocad.Symbol.TextSymbol _txtKurz;

    private GeoElement _elemLegende;

    private double TextWidth
    {
      get { return 32400; }
    }

    public ForchLayout(string fileName)
      : base(fileName)
    { }

    protected override Ocad.Symbol.TextSymbol KmTxtSymbol
    {
      get { return _kmTxtSymbol; }
    }
    protected override int KmStrichSymbol
    {
      get { return SymF.KmDist; }
    }

    public void Update(ForchCategorie categorie)
    {
      Update(categorie, UpdateCatecorie);
    }

    private void UpdateCatecorie(OcadWriter writer, ForchCategorie baseCategorie)
    {
      ForchCategorie cat = baseCategorie;

      writer.DeleteElements(new[]
          {
            SymF.StreckeLang, SymF.StreckeMittel, SymF.StreckeKurz,
            SymF.StreckeLangMittelKurz, SymF.StreckeLangMittel,
            SymF.KmDist, SymF.TextKmDist, SymF.Ziel,
            SymF.Verpflegung, SymF.LinieBreit, SymF.Deckweiss
          });

      int symStrecke = SymbolStrecke(cat.Kategorie);
      int symDir = SymbolLaufrichtung(cat.Kategorie);
      Ocad.Symbol.TextSymbol symText = SymbolText(cat.Kategorie);
      string name = KategorieName(cat.Kategorie);

      IPoint start = cat.Strecke.Points.First.Value;
      Polyline startClip = InitStart(writer, start);
      Polyline part = ReducedStrecke(cat.Strecke, startClip);

      Element elem = new GeoElement(part);
      elem.Symbol = symStrecke;
      elem.Type = GeomType.line;
      writer.Append(elem);

      double f = cat.Faktor();
      WriteKm(writer, cat.Strecke, f, null, false);
      IPoint top = ((IList<IPoint>)_elemLegende.Geometry)[0];
      IPoint above = WriteText(writer, name, symText, cat, top);
      WriteBackground(writer, top, above);

      WriteVerpf(writer, cat.Strecke, cat.Data);

      writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
      writer.SymbolsSetState(new[] {
        symDir, symStrecke, /*SymF.TextKmDist, */ SymF.KmDist, SymF.Ziel,
        SymF.TextKlein, symText.Number,
        SymF.Verpflegung, SymF.LinieBreit, SymF.Deckweiss
      }, Ocad.Symbol.SymbolStatus.Protected);
    }

    private void WriteVerpf(OcadWriter writer, Polyline line, Data data)
    {
      List<Data.VerpflegungSym> verpfList = data.Verpflegung(line);
      foreach (var verpf in verpfList)
      {
        Element elem = new GeoElement(verpf.Symbol);
        elem.Symbol = SymF.Verpflegung;
        elem.Type = GeomType.point;
        writer.Append(elem);

        elem = new GeoElement(verpf.Index);
        elem.Symbol = SymF.LinieBreit;
        elem.Type = GeomType.line;
        writer.Append(elem);
      }
    }

    public void Update(ForchData data)
    {
      Update(data, UpdateData);
    }

    private void UpdateData(OcadWriter writer, ForchData data)
    {
      writer.DeleteElements(new[]
          {
            SymF.StreckeLang, SymF.StreckeMittel, SymF.StreckeKurz,
            SymF.StreckeLangMittelKurz, SymF.StreckeLangMittel,
            SymF.KmDist, SymF.TextKmDist, SymF.TextKmDistL, SymF.TextKmDistM, SymF.TextKmDistK,
            SymF.Ziel, SymF.Verpflegung, SymF.LinieBreit, SymF.Deckweiss
          });

      Categorie lang = data.GetKategorie(Kategorie.Lang);

      IPoint start = lang.Strecke.Points.First.Value;
      Polyline startClip = InitStart(writer, start);

      foreach (var dataElement in data.StreckenTeile)
      {
        Polyline strecke = (Polyline)dataElement.Geometry;
        strecke = ReducedStrecke(strecke, startClip);

        Element elem = new GeoElement(strecke);
        elem.Symbol = dataElement.Symbol;
        elem.Type = GeomType.line;
        writer.Append(elem);
      }

      IPoint top = ((IList<IPoint>)_elemLegende.Geometry)[0];
      IPoint above = top;
      Kategorie[] kats = { Kategorie.Lang, Kategorie.Mittel, Kategorie.Kurz };
      IEqualityComparer<KmElem> cmp = new KmComparer();
      Dictionary<KmElem, List<ForchCategorie>> kmElems = new Dictionary<KmElem, List<ForchCategorie>>(cmp);
      foreach (var kat in kats)
      {
        ForchCategorie cat = data.GetKategorie(kat);

        double f = cat.Faktor();
        List<KmElem> elems = GetKm(writer.Setup, cat.Strecke, f, null, false);

        foreach (var elem in elems)
        {
          if (!kmElems.TryGetValue(elem, out List<ForchCategorie> cats))
          {
            cats = new List<ForchCategorie>();
            kmElems.Add(elem, cats);
          }
          cats.Add(cat);
        }

        string name = KategorieName(kat);
        Ocad.Symbol.TextSymbol symText = SymbolText(kat);
        above = WriteText(writer, name, symText, cat, above);
      }

      foreach (var pair in kmElems)
      {
        KmElem elem = pair.Key;
        List<ForchCategorie> cats = pair.Value;
        if (cats.Count == 1)
        {
          if (cats[0].Kategorie == Kategorie.Lang)
          { elem.Text.Symbol = SymF.TextKmDistL; }
          else if (cats[0].Kategorie == Kategorie.Mittel)
          { elem.Text.Symbol = SymF.TextKmDistM; }
          else if (cats[0].Kategorie == Kategorie.Kurz)
          { elem.Text.Symbol = SymF.TextKmDistK; }
        }
        writer.Append(elem.Text);
        writer.Append(elem.Strich);
      }

      WriteBackground(writer, top, above);

      WriteVerpf(writer, data.GetKategorie(Kategorie.Lang).Strecke, data);

      writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
      writer.SymbolsSetState(new[]
        {
          SymF.StreckeKurz, SymF.StreckeMittel, SymF.StreckeLang,
          SymF.LaufrichtungKurz, SymF.LaufrichtungLang, SymF.LaufrichtungMittel,
          SymF.StreckeLangMittel, SymF.StreckeLangMittelKurz, SymF.Ziel,
          SymF.TextKlein, SymF.TextKurz, SymF.TextMittel, SymF.TextLang,
          SymF.KmDist, /*SymF.TextKmDist, SymF.TextKmDistL, SymF.TextKmDistM, SymF.TextKmDistK,*/
          SymF.Verpflegung, SymF.LinieBreit, SymF.Deckweiss
        }, Ocad.Symbol.SymbolStatus.Protected);
    }

    protected override void ReadUpdateObjects(OcadReader template)
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
        if (elem.Symbol == _txtSchwarz.Number && elem.Text.Contains("Fluntern"))
        { _elemLegende = elem; }
      }
    }

    protected override void ReadUpdateSymbols(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      List<int> pIndexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        pIndexList.Add(i);
        iIndex++;
      }

      Setup setup = reader.ReadSetup();
      setup.PrjRotation = 0;
      setup.PrjTrans.X = 0;
      setup.PrjTrans.Y = 0;

      foreach (var iPos in pIndexList)
      {
        Ocad.Symbol.BaseSymbol symbol = reader.ReadSymbol(iPos);
        if (symbol == null)
        { continue; }

        if (symbol.Number == SymF.TextKmDist)
        { _kmTxtSymbol = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymF.Ziel)
        { _startSymbol = (Ocad.Symbol.PointSymbol)symbol; }

        else if (symbol.Number == SymF.TextKlein)
        { _txtSchwarz = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymF.TextLang)
        { _txtLang = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymF.TextMittel)
        { _txtMittel = (Ocad.Symbol.TextSymbol)symbol; }
        else if (symbol.Number == SymF.TextKurz)
        { _txtKurz = (Ocad.Symbol.TextSymbol)symbol; }
      }
    }

    protected override void ReadUpdateStringParams(OcadReader template)
    {
      //IList<StringParamIndex> pStrIdxList = template.ReadStringParamIndices();
      //Template pTpl = new Template();
      //foreach (var pStrIdx in pStrIdxList)
      //{
      //}
    }

    private IPoint WriteText(OcadWriter writer, string name, Ocad.Symbol.TextSymbol symText,
      ForchCategorie categorie, IPoint above)
    {
      IPoint min = above;
      Point prj = Point.CastOrCreate(min.Project(Setup.Prj2Map));
      prj.Y -= symText.LineSpace / 100.0 * symText.Size * 2.54;

      min = prj.Project(Setup.Map2Prj);
      Element elem = Common.CreateText(name + ":", min.X, min.Y, Setup, symText);
      writer.Append(elem);
      IPoint result = min;


      string info = string.Format("{0:N1} km / {1,3:N0} m HD", categorie.DispLength / 1000,
        categorie.SteigungRound(5));
      PointCollection points = Common.GetGeometry(info, prj, symText);
      IBox box = points.Extent;
      double width = box.Max.X - box.Min.X;
      prj.X = prj.X + TextWidth - width;

      min = prj.Project(Setup.Map2Prj);
      elem = Common.CreateText(info, min.X, min.Y, Setup, symText);

      writer.Append(elem);

      return result;
    }

    private void WriteBackground(OcadWriter writer, IPoint max, IPoint min)
    {
      IPoint prjMax = max.Project(Setup.Prj2Map);
      IPoint prjMin = min.Project(Setup.Prj2Map);

      double xMin = prjMax.X - 100;
      double xMax = prjMax.X + TextWidth + 100;
      double yMax = prjMax.Y + 2000;
      double yMin = prjMin.Y - 400;
      Polyline border = new Polyline();

      border.Add(new Point2D(xMin, yMin));
      border.Add(new Point2D(xMax, yMin));
      border.Add(new Point2D(xMax, yMax));
      border.Add(new Point2D(xMin, yMax));
      border.Add(new Point2D(xMin, yMin));

      Area area = new Area(border);

      Element elem = new MapElement(Coord.EnumCoords(area));
      elem.Type = GeomType.area;
      elem.Symbol = SymF.Deckweiss;

      writer.Append(elem);
    }

    private Polyline ReducedStrecke(Polyline full, Polyline startClip)
    {
      IList<ParamGeometryRelation> splits = GeometryOperator.CreateRelations(full, startClip);
      if (splits != null && splits.Count > 0)
      {
        IList<Polyline> parts = full.Split(splits);
        full = parts[1];
      }
      return full;
    }

    private Polyline InitStart(OcadWriter writer, IPoint start)
    {
      Element elem = new GeoElement(start);
      elem.Symbol = _startSymbol.Number;
      elem.Type = GeomType.point;

      writer.Append(elem);

      IGeometry max = null;
      foreach (var graphics in _startSymbol.Graphics)
      {
        if (max == null || max.Extent.GetMaxExtent() <
          graphics.MapGeometry.Extent.GetMaxExtent())
        { max = graphics.MapGeometry; }
      }

      IPoint translate = start.Project(Setup.Prj2Map);
      Basics.Geom.Projection.Translate trans = new Basics.Geom.Projection.Translate(translate);
      max = max.Project(trans);
      max = max.Project(Setup.Map2Prj);
      return (Polyline)max;
    }
    private int SymbolStrecke(Kategorie kategorie)
    {
      if (kategorie == Kategorie.Lang)
      { return SymF.StreckeLang; }
      else if (kategorie == Kategorie.Mittel)
      { return SymF.StreckeMittel; }
      else if (kategorie == Kategorie.Kurz)
      { return SymF.StreckeKurz; }
      else
      { throw new InvalidOperationException("Unhandled Kategorie " + kategorie); }
    }

    private int SymbolLaufrichtung(Kategorie kategorie)
    {
      if (kategorie == Kategorie.Lang)
      { return SymF.LaufrichtungLang; }
      else if (kategorie == Kategorie.Mittel)
      { return SymF.LaufrichtungMittel; }
      else if (kategorie == Kategorie.Kurz)
      { return SymF.LaufrichtungKurz; }
      else
      { throw new InvalidOperationException("Unhandled Kategorie " + kategorie); }

    }

    private string KategorieName(Kategorie kategorie)
    {
      if (kategorie == Kategorie.Lang)
      { return "Original"; }
      else if (kategorie == Kategorie.Mittel)
      { return "Mittel"; }
      else if (kategorie == Kategorie.Kurz)
      { return "Kurz"; }
      else
      { throw new InvalidOperationException("Unhandled Kategorie " + kategorie); }

    }

    private Ocad.Symbol.TextSymbol SymbolText(Kategorie kategorie)
    {
      if (kategorie == Kategorie.Lang)
      { return _txtLang; }
      else if (kategorie == Kategorie.Mittel)
      { return _txtMittel; }
      else if (kategorie == Kategorie.Kurz)
      { return _txtKurz; }
      else
      { throw new InvalidOperationException("Unhandled Kategorie " + kategorie); }
    }

    private class KmComparer : IEqualityComparer<KmElem>
    {
      readonly double _prec = 10;
      public bool Equals(KmElem x, KmElem y)
      {
        int[] ox = GetValues(x);
        int[] oy = GetValues(x);

        return ox[0] == oy[0] && ox[1] == oy[1];
      }

      public int GetHashCode(KmElem obj)
      {
        int[] o = GetValues(obj);
        Point p = (Point)obj.Strich.Geometry;
        return o[0].GetHashCode() ^ o[1].GetHashCode();
      }
      private int[] GetValues(KmElem obj)
      {
        Point p = (Point)obj.Strich.Geometry;
        return new[] { (int)Math.Round(p.X * _prec), (int)Math.Round(p.Y * _prec) };
      }
    }
  }
}
