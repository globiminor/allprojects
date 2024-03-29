﻿
using Basics.Geom;
using Basics.Geom.Index;
using Ocad;
using Ocad.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCourse.Commands
{
  public class CourseMap
  {
    public class ControlElement
    {
      public ControlElement(MapElement elem)
      {
        P = elem.GetMapGeometry().GetGeometry() as Point;
        Elem = elem;
      }
      public Point P { get; }
      public MapElement Elem { get; }
    }
    public class ConnectionElement
    {
      public ConnectionElement(MapElement elem)
      {
        L = elem.GetMapGeometry().GetGeometry() as Polyline;
        Elem = elem;
      }

      public Polyline L { get; }
      public MapElement Elem { get; }
    }
    public class AreaElement
    {
      public AreaElement(MapElement elem)
      {
        A = elem.GetMapGeometry().GetGeometry() as Surface;
        Elem = elem;
      }

      public Surface A { get; set; }
      public MapElement Elem { get; }
    }

    public class ControlNrElement
    {
      public ControlNrElement(MapElement elem, double textHeight)
      {
        PointCollection pts = (PointCollection)elem.GetMapGeometry().GetGeometry();
        E = new Box(new Point2D(pts[0].X, pts[0].Y), new Point2D(pts[2].X, pts[0].Y + textHeight));
        Elem = elem;
      }
      public Box E { get; }
      public MapElement Elem { get; }
    }

    public List<int> StartSymbols { get; set; } = new List<int> { 701000 };
    public List<int> ControlSymbols { get; set; } = new List<int> { 703000 };
    public List<int> ControlNrSymbols { get; set; } = new List<int> { 704000 };
    public int ControlNrOverprintSymbol { get; set; }
    public List<int> ConnectionSymbols { get; set; } = new List<int> { 705000 };
    public List<int> FinishSymbols { get; set; } = new List<int> { 706000 };
    public List<int> CourseNameSymbols { get; set; } = new List<int> { 721000 };
    public List<int> LayoutSymbols { get; set; } = new List<int> { 713000, 20001 };
    public List<int> ForbiddenAreaSymbols { get; set; } = new List<int> { 709000, 709003 };
    public List<int> ForbiddenLineSymbols { get; set; } = new List<int> { 708000 };


    public double ControlDistance { get; set; }
    public List<MapElement> StartElems { get; set; }
    public List<MapElement> FinishElems { get; set; }
    public List<MapElement> AllControls { get; set; }
    public List<MapElement> ConnectionElems { get; set; }
    public List<MapElement> ControlNrElems { get; set; }
    public double ControlNrHeight { get; set; }

    public List<MapElement> LayoutElems { get; set; }
    public List<MapElement> ForbiddenAreaElems { get; set; }
    public List<MapElement> ForbiddenLineElems { get; set; }

    public Dictionary<Element, GeoElement.Geom> CustomGeometries { get; set; }
    public Dictionary<string, string> CustomTexts { get; set; }


    public string CourseNameFont { get; set; }
    public TextSymbol ControlNrTextSymbol { get; set; }

    public List<string> UnplacedControls => _unplacedControls ?? (_unplacedControls = new List<string>());
    private List<string> _unplacedControls;


    public BoxTree<ControlElement> ControlsTree => _controlsTree ??
      (_controlsTree = BoxTree.Create(AllControls.Select(e => new ControlElement(e)), (c) => c.P.Extent, 4));
    private BoxTree<ControlElement> _controlsTree;
    public BoxTree<ConnectionElement> ConnectionTree => _connectionTree ??
      (_connectionTree = BoxTree.Create(ConnectionElems.Select(e => new ConnectionElement(e)), (c) => c.L.Extent, 4));
    private BoxTree<ConnectionElement> _connectionTree;

    public BoxTree<ControlNrElement> ControlNrsTree => _controlNrsTree ??
      (_controlNrsTree = BoxTree.Create(ControlNrElems.Select(e => new ControlNrElement(e, ControlNrHeight)), (c) => c.E, 4));
    private BoxTree<ControlNrElement> _controlNrsTree;

    public BoxTree<AreaElement> LayoutsTree => _layoutsTree ??
      (_layoutsTree = BoxTree.Create(LayoutElems.Select(e => new AreaElement(e)), (c) => c.A.Extent, 4));
    private BoxTree<AreaElement> _layoutsTree;

    public BoxTree<AreaElement> ForbiddenAreaTree => _forbiddenAreaTree ??
      (_forbiddenAreaTree = BoxTree.Create(ForbiddenAreaElems.Select(e => new AreaElement(e)), (c) => c.A.Extent, 4));
    private BoxTree<AreaElement> _forbiddenAreaTree;

    public void InitFromFile(string courseFile)
    {
      List<MapElement> startElems = new List<MapElement>();
      List<MapElement> controlElems = new List<MapElement>();
      List<MapElement> controlNrElems = new List<MapElement>();
      List<MapElement> connectionElems = new List<MapElement>();
      List<MapElement> finishElems = new List<MapElement>();
      List<MapElement> layoutElems = new List<MapElement>();
      List<MapElement> forbiddenAreaElems = new List<MapElement>();
      List<MapElement> forbiddenLineElems = new List<MapElement>();
      double controlDistance = 0;
      double textHeight = 0;
      string courseNameFont = "";
      TextSymbol controlNrTextSymbol = null;

      using (OcadReader r = OcadReader.Open(courseFile))
      {
        double mapDistance = 0;
        Dictionary<int, Box> layoutExts = new Dictionary<int, Box>();
        foreach (var symbol in r.ReadSymbols())
        {
          if (ControlSymbols.Contains(symbol.Number) && symbol is PointSymbol p)
          {
            foreach (var g in p.Graphics)
            {
              IBox ext = g.MapGeometry.Extent;
              mapDistance = Math.Max(mapDistance, Math.Abs(ext.Min.X));
              mapDistance = Math.Max(mapDistance, Math.Abs(ext.Min.Y));
              mapDistance = Math.Max(mapDistance, Math.Abs(ext.Max.X));
              mapDistance = Math.Max(mapDistance, Math.Abs(ext.Max.Y));
            }
          }
          if (ControlNrSymbols.Contains(symbol.Number) && symbol is TextSymbol t)
          {
            double pts = t.Size / 10.0;
            textHeight = Math.Max(textHeight, pts / 4.05); // mm
            controlNrTextSymbol = controlNrTextSymbol ?? t;
          }
          if (CourseNameSymbols.Contains(symbol.Number) && symbol is TextSymbol cn)
          {
            courseNameFont = cn.FontName;
          }
          if (LayoutSymbols.Contains(symbol.Number) && symbol is PointSymbol ps)
          {
            Box allBox = null;
            foreach (var g in ps.Graphics)
            {
              IBox ext = g.MapGeometry.Extent;
              if (allBox == null)
              { allBox = new Box(ext); }
              else
              { allBox.Include(ext); }
            }
            layoutExts.Add(symbol.Number, allBox);
          }
        }

        controlDistance = mapDistance; // * FileParam.OCAD_UNIT * r.Setup.Scale;
        textHeight = textHeight * 0.001 / FileParam.OCAD_UNIT; // * 0.001 * r.Setup.Scale; // m

        foreach (var elem in r.EnumMapElements())
        {
          if (StartSymbols.Contains(elem.Symbol))
          { startElems.Add(elem); }
          else if (ControlSymbols.Contains(elem.Symbol))
          { controlElems.Add(elem); }
          else if (ControlNrSymbols.Contains(elem.Symbol))
          { controlNrElems.Add(elem); }
          else if (ConnectionSymbols.Contains(elem.Symbol))
          { connectionElems.Add(elem); }
          else if (FinishSymbols.Contains(elem.Symbol))
          { finishElems.Add(elem); }
          else if (LayoutSymbols.Contains(elem.Symbol))
          {
            if (elem.GetMapGeometry() is GeoElement.Point gp && layoutExts.TryGetValue(elem.Symbol, out Box ext))
            {
              IPoint p = gp.BaseGeometry;
              elem.Type = GeomType.area;
              elem.SetMapGeometry(new GeoElement.Surface(new Surface(Polyline.Create(new Point[] {
                ext.Min + p, new Point2D(ext.Max.X, ext.Min.Y) + p, ext.Max + p, new Point2D(ext.Min.X, ext.Max.Y) + p, ext.Min + p  }))));
            }
            layoutElems.Add(elem);
          }
          else if (ForbiddenAreaSymbols.Contains(elem.Symbol))
          { forbiddenAreaElems.Add(elem); }
          else if (ForbiddenLineSymbols.Contains(elem.Symbol))
          { forbiddenLineElems.Add(elem); }
        }

      }

      List<MapElement> allControls = new List<MapElement>(controlElems);
      allControls.AddRange(startElems);
      allControls.AddRange(finishElems);

      StartElems = startElems;
      FinishElems = finishElems;
      ControlDistance = controlDistance;
      AllControls = allControls;
      ConnectionElems = connectionElems;
      ControlNrElems = controlNrElems;
      ControlNrHeight = textHeight;
      LayoutElems = layoutElems;

      ForbiddenAreaElems = forbiddenAreaElems;
      ForbiddenLineElems = forbiddenLineElems;

      CustomGeometries = new Dictionary<Element, GeoElement.Geom>(new ObjectStringComparer());

      CourseNameFont = courseNameFont;
      ControlNrTextSymbol = controlNrTextSymbol;
    }

    private class ObjectStringComparer : IEqualityComparer<Element>
    {
      public bool Equals(Element x, Element y)
      {
        return x.ObjectString?.Equals(y.ObjectString) ?? false;
      }

      public int GetHashCode(Element obj)
      {
        return obj?.ObjectString?.GetHashCode() ?? 0;
      }
    }
  }
}
