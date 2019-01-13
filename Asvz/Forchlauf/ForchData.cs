using System;
using System.Collections.Generic;
using System.Xml;
using Basics.Geom;
using Ocad;

namespace Asvz.Forchlauf
{
  public enum Kategorie { Lang, Mittel, Kurz }

  public class ForchData : StreckenData<Strecke>
  {
    #region nested classes
    private class StreckeInfo
    {
      public readonly List<int> PartIndexList;
      public readonly Polyline Line;
      public StreckeInfo(int index, Polyline line)
      {
        PartIndexList = new List<int>();
        PartIndexList.Add(index);
        Line = line;
      }

      public Kategorie GetKategorie(IList<GeoElement> elements)
      {
        foreach (var elemIdx in PartIndexList)
        {
          if (elemIdx < 0)
          { continue; }

          if (elements[elemIdx].Symbol == SymF.StreckeKurz)
          { return Kategorie.Kurz; }
          else if (elements[elemIdx].Symbol == SymF.StreckeMittel)
          { return Kategorie.Mittel; }
        }
        return Kategorie.Lang;
      }
      public StreckeInfo Clone()
      {
        StreckeInfo clone = new StreckeInfo(0, Line.Clone());
        clone.PartIndexList.Clear();
        clone.PartIndexList.AddRange(PartIndexList);

        return clone;
      }

      public void Append(List<GeoElement> streckenTeile, int index)
      {
        Polyline f = (Polyline)streckenTeile[index].Geometry;
        AppendLine(Line, f);

        PartIndexList.Add(index);
      }
    }
    #endregion

    private IList<GeoElement> _streckenTeile;

    public ForchData(string fileName, string dhm)
      : base(dhm)
    {
      ReadDefaultData(fileName);
    }

    public override string Name
    {
      get { return "Forchlauf"; }
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

      List<GeoElement> streckenTeile = new List<GeoElement>();

      foreach (var elem in reader.EnumGeoElements(pIndexList))
      {
        if (elem.Symbol == SymF.StreckeLang)
        { streckenTeile.Add(elem); }
        else if (elem.Symbol == SymF.StreckeMittel)
        { streckenTeile.Add(elem); }
        else if (elem.Symbol == SymF.StreckeKurz)
        { streckenTeile.Add(elem); }
        else if (elem.Symbol == SymF.StreckeLangMittel)
        { streckenTeile.Add(elem); }
        else if (elem.Symbol == SymF.StreckeLangMittelKurz)
        { streckenTeile.Add(elem); }

        else if (elem.Symbol == SymF.LinieBreit)
        { IndexList.Add((Polyline)elem.Geometry); }
        else if (elem.Symbol == SymF.Verpflegung)
        { VerpfList.Add((Point)elem.Geometry); }

        else if (elem.Symbol == SymF.Wald)
        { Wald.Add((Area)elem.Geometry); }
        else if (elem.Symbol == SymF.Siedlung)
        { Siedlung.Add((Area)elem.Geometry); }
        else if (elem.Symbol == SymF.Teer)
        { Teer.Add((Area)elem.Geometry); }
      }

      GetStrecken(streckenTeile);
      _streckenTeile = streckenTeile;
    }

    internal IList<GeoElement> StreckenTeile
    {
      get { return _streckenTeile; }
    }

    private void GetStrecken(List<GeoElement> streckenTeile)
    {
      int n = streckenTeile.Count;
      bool[] hasFrom = new bool[n];
      List<int>[] toStrecken = new List<int>[n];

      for (int ix = 0; ix < n; ix++)
      {
        GeoElement x = streckenTeile[ix];
        Polyline xPart = (Polyline)x.Geometry;

        for (int iy = 0; iy < n; iy++)
        {
          GeoElement y = streckenTeile[iy];
          Polyline yPart = (Polyline)y.Geometry;

          if (PointOperator.Dist2(xPart.Points.Last.Value, yPart.Points.First.Value) < 20)
          {
            hasFrom[iy] = true;

            if (toStrecken[ix] == null)
            { toStrecken[ix] = new List<int>(); }
            toStrecken[ix].Add(iy);
          }
        }
      }

      int iStart = -1;
      for (int iStrecke = 0; iStrecke < n; iStrecke++)
      {
        if (hasFrom[iStrecke] == false)
        {
          iStart = iStrecke;
          break;
        }
      }

      int t = iStart;
      Polyline line = (Polyline)streckenTeile[t].Geometry;
      line = line.Clone();

      StreckeInfo init = new StreckeInfo(t, line);
      List<StreckeInfo> strecken = GetStreckeInfo(init, streckenTeile, toStrecken);

      Strecke s = new Strecke();
      foreach (var strecke in strecken)
      {
        ForchCategorie cat = new ForchCategorie(strecke.GetKategorie(streckenTeile));
        cat.SetGeometry(strecke.Line, this);
        s.Categories.Add(cat);
      }

      Strecken.Add(s);
    }

    private List<StreckeInfo> GetStreckeInfo(StreckeInfo start, List<GeoElement> streckenTeile,
      List<int>[] toStrecken)
    {
      int n = start.PartIndexList.Count;
      int t = start.PartIndexList[n - 1];

      while (toStrecken[t] != null && toStrecken[t].Count == 1)
      {
        int t1 = toStrecken[t][0];
        if (start.PartIndexList.Contains(t1))
        {
          int pos = start.PartIndexList.IndexOf(t1);
          for (int iPos = pos - 1; iPos >= 0; iPos--)
          {
            int rev = start.PartIndexList[iPos];
            Polyline rLine = (Polyline)streckenTeile[rev].Geometry;
            rLine = rLine.Invert();
            AppendLine(start.Line, rLine);
            start.PartIndexList.Add(-rev - 1);
          }
          List<StreckeInfo> result = new List<StreckeInfo>();
          result.Add(start);
          return result;
        }
        Polyline f = (Polyline)streckenTeile[t1].Geometry;
        f = AppendLine(start.Line, f);

        start.PartIndexList.Add(t1);
        t = t1;
      }
      if (toStrecken[t] != null)
      {
        List<StreckeInfo> result = new List<StreckeInfo>();
        foreach (var t1 in toStrecken[t])
        {
          StreckeInfo g = start.Clone();
          g.Append(streckenTeile, t1);
          List<StreckeInfo> partResult = GetStreckeInfo(g, streckenTeile, toStrecken);
          result.AddRange(partResult);
        }
        return result;
      }

      throw new InvalidProgramException("Should not reach this point");
    }

    private static Polyline AppendLine(Polyline line, Polyline append)
    {
      append = append.Clone();
      append.Points.RemoveFirst();
      append.AddFirst(Point.Create(line.Points.Last.Value));

      foreach (var seg in append.Segments)
      {
        line.Add(seg);
      }
      return append;
    }

    public ForchCategorie GetKategorie(Kategorie kategorie)
    {
      Strecke s = Strecken[0];
      foreach (var o in s.Categories)
      {
        ForchCategorie cat = (ForchCategorie)o;
        if (cat.Kategorie == kategorie)
        { return cat; }
      }
      return null;
    }

    protected override void ReadSymbolsCore(OcadReader reader)
    {
      int iIndex = 0;
      int i;
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
        Ocad.Symbol.BaseSymbol pSymbol = reader.ReadSymbol(iPos);

        if (pSymbol.Number == SymF.Verpflegung)
        { SymVerpf = (Ocad.Symbol.PointSymbol)pSymbol; }
      }
    }

    protected override void CreateStyles(XmlDocument doc, XmlNode dc)
    {
      XmlElement style = KmlUtils.GetStyle(doc, Kategorie.Kurz.ToString(), "C0ff00ff", 3);
      dc.AppendChild(style);

      style = KmlUtils.GetStyle(doc, Kategorie.Mittel.ToString(), "C0ff0000", 3);
      dc.AppendChild(style);

      style = KmlUtils.GetStyle(doc, Kategorie.Lang.ToString(), "C00000ff", 3);
      dc.AppendChild(style);

      style = doc.CreateElement("Style");
      XmlAttribute attr = doc.CreateAttribute("id");
      attr.Value = "pin";
      style.Attributes.Append(attr);

      XmlElement iconStyle = doc.CreateElement("IconStyle");
      //attr = doc.CreateAttribute("color");
      //attr.Value = "ff00ff00";
      //iconStyle.Attributes.Append(attr);

      XmlElement icon = doc.CreateElement("Icon");
      attr = doc.CreateAttribute("href");
      attr.Value = "sola.png";
      //attr.Value = "http://maps.google.com/mapfiles/kml/pal3/icon21.png";
      icon.Attributes.Append(attr);

      iconStyle.AppendChild(icon);
      style.AppendChild(iconStyle);

      dc.AppendChild(style);
    }

  }
}
