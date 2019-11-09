using Basics.Geom;
using Ocad;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Asvz.SolaDuo
{
  public enum Kategorie { Strecke }
  class DuoData : StreckenData<Strecke>
  {
    internal class Kontrolle
    {
      private readonly string _id;
      private readonly string _name;

      public Kontrolle(string id, string name)
      {
        _id = id;
        _name = name;
      }

      public string Id
      { get { return _id; } }
      public string Name
      { get { return _name; } }
    }

    private readonly IList<Kontrolle> _postenLst;

    private List<IPoint> _ausschnitt;
    private GeometryCollection _symAusschnitt;

    private List<IPoint> _verzweigung;
    private List<IPoint> _helfer;

    private GeometryCollection _symStart;
    private GeometryCollection _symUebergabe;
    private GeometryCollection _symZiel;
    private GeometryCollection _symVerzweigung;
    private GeometryCollection _symHelfer;

    // string params
    private ViewPar _viewParam;
    private ExportPar _exportParam;
    private PrintPar _printParam;

    public DuoData(string ocdName, string dhmName)
      : base(dhmName)
    {
      ReadDefaultData(ocdName);

      _postenLst = new Kontrolle[]
      {
        new Kontrolle("Start", "St. Gallen"),
        new Kontrolle("1", "Gossau"),
        new Kontrolle("2", "Flawil"),
        new Kontrolle("3", "Uzwil"),
        new Kontrolle("4", "Rickenbach"),
        new Kontrolle("5", "Sirnach"),
        new Kontrolle("6", "Eschlikon"),
        new Kontrolle("7", "Turbenthal"),
        new Kontrolle("8", "Fehraltdorf"),
        new Kontrolle("9", "Gutenswil    "),
        new Kontrolle("10", "    Nänikon"),
        new Kontrolle("11", "Schwerzenbach"),
        new Kontrolle("12", "Pfaffhausen"),
        new Kontrolle("Ziel", "Zürich")
      };
    }

    public override string Name
    {
      get { return "SOLA Duo"; }
    }

    public IList<Kontrolle> PostenListe
    {
      get { return _postenLst; }
    }

    private List<IPoint> Ausschnitt
    {
      get
      {
        if (_ausschnitt == null)
        { _ausschnitt = new List<IPoint>(); }
        return _ausschnitt;
      }
    }
    private List<IPoint> Verzweigung
    {
      get
      {
        if (_verzweigung == null)
        { _verzweigung = new List<IPoint>(); }
        return _verzweigung;
      }
    }
    private List<IPoint> Helfer
    {
      get
      {
        if (_helfer == null)
        { _helfer = new List<IPoint>(); }
        return _helfer;
      }
    }

    protected override void ReadDefaultElementsCore(OcadReader reader)
    {
      int iIndex = 0;
      ElementIndex index;
      IList<ElementIndex> indexList = new List<ElementIndex>();
      while ((index = reader.ReadIndex(iIndex)) != null)
      {
        indexList.Add(index);
        iIndex++;
      }

      List<GeoElement> streckenTeile = new List<GeoElement>();

      foreach (var elem in reader.EnumGeoElements(indexList))
      {
        if (elem.Symbol == SymDD.Strecke)
        { streckenTeile.Add(elem); }
        else if (elem.Symbol == SymDD.StreckeOhneDtm)
        { streckenTeile.Add(elem); }
        else if (elem.Symbol == SymDD.StreckeBisAbzweigung)
        {
          streckenTeile.Add(elem);
          Polyline line = ((GeoElement.Line)elem.Geometry).BaseGeometry;
          Verzweigung.Add(Point.Create(line.Points.Last()));
        }
        else if (elem.Symbol == SymDD.StreckeBisHelfer)
        {
          streckenTeile.Add(elem);
          Polyline line = ((GeoElement.Line)elem.Geometry).BaseGeometry;
          Helfer.Add(Point.Create(line.Points.Last()));
        }
        else if (elem.Symbol == SymDD.StreckeBisEnde)
        { streckenTeile.Add(elem); }

        else if (elem.Symbol == SymDD.Ausschnitt)
        { Ausschnitt.Add(((GeoElement.Point)elem.Geometry).BaseGeometry); }
      }

      GetStrecken(streckenTeile);
      Ausschnitt.Sort(ComparePoint);
    }
    private int ComparePoint(IPoint x, IPoint y)
    {
      return -x.X.CompareTo(y.X);
    }

    protected override void ReadSymbolsCore(OcadReader reader)
    {
      int iIndex = 0;
      int i;
      Ocad.Symbol.BaseSymbol symbol;
      IList<int> indexList = new List<int>();
      while ((i = reader.ReadSymbolPosition(iIndex)) > 0)
      {
        indexList.Add(i);
        iIndex++;
      }

      Setup = reader.ReadSetup();
      Setup.PrjTrans.X = 0;
      Setup.PrjTrans.Y = 0;

      foreach (var iPos in indexList)
      {
        symbol = reader.ReadSymbol(iPos);

        if (symbol is Ocad.Symbol.PointSymbol pntSym)
        {
          if (symbol.Number == SymDD.Ausschnitt)
          { _symAusschnitt = pntSym.GetSymbolGeometry(Setup); }

          else if (symbol.Number == SymDD.Start)
          { _symStart = pntSym.GetSymbolGeometry(Setup); }
          else if (symbol.Number == SymDD.Uebergabe)
          { _symUebergabe = pntSym.GetSymbolGeometry(Setup); }
          else if (symbol.Number == SymDD.Ziel)
          { _symZiel = pntSym.GetSymbolGeometry(Setup); }
          else if (symbol.Number == SymDD.Verzweigung)
          { _symVerzweigung = pntSym.GetSymbolGeometry(Setup); }
          else if (symbol.Number == SymDD.Helfer)
          { _symHelfer = pntSym.GetSymbolGeometry(Setup); }
        }
      }
    }

    protected override void CreateStyles(XmlDocument doc, XmlNode dc)
    {
      XmlAttribute attr;
      XmlElement style;

      style = KmlUtils.GetStyle(doc, Kategorie.Strecke.ToString(), "C00000ff", 3);
      dc.AppendChild(style);

      style = doc.CreateElement("Style");
      attr = doc.CreateAttribute("id");
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

    private void GetStrecken(List<GeoElement> strecken)
    {
      int n = strecken.Count;
      int[] from = new int[n];
      int[] to = new int[n];

      for (int ix = 0; ix < n; ix++)
      {
        Polyline xPart = ((GeoElement.Line)strecken[ix].Geometry).BaseGeometry;

        for (int iy = 0; iy < n; iy++)
        {
          Polyline yPart = ((GeoElement.Line)strecken[iy].Geometry).BaseGeometry;

          if (PointOp.Dist2(xPart.Points.Last(), yPart.Points[0]) < 20)
            if (PointOp.Dist2(xPart.Points.Last(), yPart.Points[0]) < 20)
            {
              to[ix] = iy + 1;
              from[iy] = ix + 1;
            }
        }
      }

      int iStart = -1;
      for (int i = 0; i < n; i++)
      {
        if (from[i] == 0)
        {
          iStart = i + 1;
          break;
        }
      }

      int t = iStart;

      DuoCategorie cat = null;
      while (t > 0)
      {
        if (cat == null)
        { cat = new DuoCategorie(this); }

        GeoElement elem = strecken[t - 1];
        Polyline line = ((GeoElement.Line)elem.Geometry).BaseGeometry;
        line = line.Clone();

        if (elem.Symbol == SymDD.StreckeOhneDtm)
        { cat.AddGeometry(line, false); }
        else
        { cat.AddGeometry(line, true); }

        if (elem.Symbol == SymDD.StreckeBisEnde)
        {
          Strecke s = new Strecke();
          s.Categories.Add(cat);

          cat = null;
          Strecken.Add(s);
        }

        t = to[t - 1];
      }
      if (cat != null)
      { throw new InvalidOperationException("Missing end Strecke"); }
    }


    public void ExportDetails(string template, string dir)
    {
      OcadReader reader = OcadReader.Open(template);
      try
      { ReadSymbolsCore(reader); }
      finally
      { reader.Close(); }

      List<Element> elements = new List<Element>();
      Element element;

      int nStrecken = Strecken.Count;
      for (int iStrecke = 0; iStrecke < nStrecken; iStrecke++)
      {
        Strecke s = Strecken[iStrecke];
        DuoCategorie cat = (DuoCategorie)s.Categories[0];
        Polyline str = cat.Strecke;

        if (iStrecke == 0)
        {
          ISegment c = str.GetSegment(0);
          IPoint tangent = c.TangentAt(0);

          element = new GeoElement(c.Start);
          element.Angle = -Math.Atan2(tangent.X, tangent.Y);
          element.Symbol = SymDD.Start;
          element.Type = GeomType.point;
          elements.Add(element);

          str = Utils.Split(str, _symStart, c.Start, element.Angle)[1];
        }
        else
        {
          element = new GeoElement(str.Points[0]);
          element.Symbol = SymDD.Uebergabe;
          element.Type = GeomType.point;
          elements.Add(element);

          str = Utils.Split(str, _symUebergabe, str.Points[0], 0)[1];
        }

        if (iStrecke == nStrecken - 1)
        {
          element = new GeoElement(str.Points.Last());
          element.Symbol = SymDD.Ziel;
          element.Type = GeomType.point;
          elements.Add(element);

          str = Utils.Split(str, _symZiel, str.Points.Last(), 0)[0];
        }
        else
        {
          str = Utils.Split(str, _symUebergabe, str.Points.Last(), 0)[0];
        }

        foreach (var verzweigung in Verzweigung)
        {
        }
        foreach (var helfer in Helfer)
        {
        }
        element = new GeoElement(str);
        element.Symbol = SymDD.Strecke;
        element.Type = GeomType.line;

        elements.Add(element);
      }

      foreach (var verzweigung in Verzweigung)
      {
        element = new GeoElement(verzweigung);
        element.Symbol = SymDD.Verzweigung;
        element.Type = GeomType.point;

        elements.Add(element);
      }

      foreach (var helfer in Helfer)
      {
        element = new GeoElement(helfer);
        element.Symbol = SymDD.Helfer;
        element.Type = GeomType.point;

        elements.Add(element);
      }

      int nAusschnitt = Ausschnitt.Count;
      for (int iAusschnitt = 0; iAusschnitt < nAusschnitt; iAusschnitt++)
      {
        GeometryCollection boxCollection = Utils.SymbolGeometry(_symAusschnitt,
          Ausschnitt[iAusschnitt], 0);
        char a = (char)('a' + iAusschnitt);
        ExportDetail(template, Path.Combine(dir, string.Format("ausschnitt_{0}.ocd", a)),
          elements, boxCollection.Extent);
      }
    }

    private void ReadStringParamsCore(OcadReader reader)
    {
      IList<StringParamIndex> strIdxList = reader.ReadStringParamIndices();
      foreach (var strIdx in strIdxList)
      {
        if (strIdx.Type == StringType.ViewPar)
        { _viewParam = new ViewPar(reader.ReadStringParam(strIdx)); }
        else if (strIdx.Type == StringType.PrintPar)
        { _printParam = new PrintPar(reader.ReadStringParam(strIdx)); }
        else if (strIdx.Type == StringType.Export)
        { _exportParam = new ExportPar(reader.ReadStringParam(strIdx)); }
      }
    }

    private void ExportDetail(string template, string outFile,
      IList<Element> elements, IBox extent)
    {
      File.Copy(template, outFile, true);
      OcadWriter writer = OcadWriter.AppendTo(outFile);

      OcadReader pTemplate = OcadReader.Open(template);
      Setup setup;
      try
      {
        ReadStringParamsCore(pTemplate);
        setup = pTemplate.ReadSetup();
      }
      finally
      { pTemplate.Close(); }

      writer.DeleteElements(new int[] { SymDD.Start,
        SymDD.Strecke, SymDD.StreckeBisAbzweigung, SymDD.StreckeBisAbzweigung, SymDD.StreckeBisEnde, SymDD.StreckeOhneDtm,
        SymDD.Uebergabe, SymDD.Verzweigung, SymDD.Helfer, SymDD.Ziel});

      writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);

      writer.SymbolsSetState(new int[] { SymDD.Start,
        SymDD.Strecke, SymDD.Uebergabe, SymDD.Verzweigung, SymDD.Helfer, SymDD.Ziel,
        SymDD.Verpflegung, SymDD.Text}, Ocad.Symbol.SymbolStatus.Protected);

      foreach (var element in elements)
      {
        writer.Append(element);
      }

      IBox e = BoxOp.ProjectRaw(extent, setup.Prj2Map).Extent;

      PrintPar printParam = new PrintPar(_printParam.StringPar);
      printParam.Left = e.Min.X / 100;
      printParam.Right = e.Max.X / 100;
      printParam.Top = e.Max.Y / 100;
      printParam.Bottom = e.Min.Y / 100;
      printParam.Range = PrintPar.RangeType.PartialMap;
      printParam.Scale = 50000.0;
      writer.Overwrite(StringType.PrintPar, 0, printParam.StringPar);

      ViewPar viewParam = new ViewPar(_viewParam.StringPar);
      viewParam.XOffset = (printParam.Left + printParam.Right) / 2.0;
      viewParam.YOffset = (printParam.Top + printParam.Bottom) / 2.0;
      writer.Overwrite(StringType.ViewPar, 0, viewParam.StringPar);

      writer.Close();
    }

    public static void CreateDtm()
    {
      IList<Grid.DataDoubleGrid> grids = new Grid.DataDoubleGrid[]
      {
        Grid.DataDoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\exp00001.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1051.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1052.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1053.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1054.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1055.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1056.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1071.agr", 0, 1, typeof(double)),
        Grid.DataDoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1091.agr", 0, 1, typeof(double)),
        Grid.DataDoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1092.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1111.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1112.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1113.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1114.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1115.agr", 0, 1, typeof(double)),
        //Grid.DoubleGrid.FromAsciiFile("C:\\Daten\\ASVZ\\Daten\\Dhm\\mm1116.agr", 0, 1, typeof(double)),
      };

      Box extent = grids[0].Extent.Extent.Clone();
      double dx = grids[0].Extent.Dx;

      foreach (var grid in grids)
      {
        double n;
        IBox ext = grid.Extent.Extent;

        n = (extent.Min.X - ext.Min.X) / dx;
        if (n >= 1)
        { extent.Min.X = extent.Min.X - (int)n * dx; }

        n = (extent.Min.Y - ext.Min.Y) / dx;
        if (n >= 1)
        { extent.Min.Y = extent.Min.Y - (int)n * dx; }

        n = (ext.Max.X - extent.Max.X) / dx;
        if (n >= 1)
        { extent.Max.X = extent.Max.X + (int)n * dx; }

        n = (ext.Max.Y - extent.Max.Y) / dx;
        if (n >= 1)
        { extent.Max.Y = extent.Max.Y + (int)n * dx; }
      }

      double nx = (extent.Max.X - extent.Min.X) / dx + 1;
      double ny = (extent.Max.Y - extent.Min.Y) / dx + 1;
      Grid.DataDoubleGrid total = new Grid.DataDoubleGrid((int)nx, (int)ny,
        typeof(double), extent.Min.X, extent.Min.Y + ny * dx, dx, 0, 1);

      int iNx = total.Extent.Nx;
      int iNy = total.Extent.Ny;
      for (int ix = 0; ix < iNx; ix++)
      {
        for (int iy = 0; iy < iNy; iy++)
        {
          IPoint p = total.Extent.CellCenter(ix, iy);

          int n = 0;
          double sumD = 0;
          foreach (var grid in grids)
          {
            bool inside = grid.Extent.GetNearest(p, out int gx, out int gy);
            if (inside == false)
            { continue; }

            double d = grid[gx, gy];
            if (double.IsNaN(d))
            { continue; }

            sumD += d;
            n++;
          }

          double d0;
          if (n == 0)
          { d0 = double.NaN; }
          else
          { d0 = sumD / n; }

          total[ix, iy] = d0;
        }
      }
      Grid.DoubleGrid.Save(total, "C:\\Daten\\ASVZ\\Daten\\Dhm\\solaDuo.grd");
    }
  }
}
