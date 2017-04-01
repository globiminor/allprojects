using Ocad;
using OMapScratch;
using System;
using System.IO;

namespace OcadScratch
{
  public class CmdTransfer : IDisposable
  {
    private readonly string _scratchFile;
    private readonly string _ocdFile;

    private Basics.Geom.IProjection _prj;
    private double _symbolScale;

    public CmdTransfer(string scratchFile, string ocdFile)
    {
      _scratchFile = scratchFile;
      _ocdFile = ocdFile;
    }

    void IDisposable.Dispose()
    { }

    public void Execute()
    {
      _prj = null;
      _symbolScale = 1;

      XmlConfig config;
      using (TextReader reader = new StreamReader(_scratchFile))
      { Serializer.Deserialize(out config, reader); }
      Map map = new Map();
      map.Load(_scratchFile, config);

      float[] offset = map.GetOffset();
      _prj = new MyPrjGlobal(offset[0], offset[1]);
      _symbolScale = map.SymbolScale;

      using (Ocad9Writer w = Ocad9Writer.AppendTo(_ocdFile))
      {
        foreach (Elem elem in map.Elems)
        {
          Transfer(elem, w);
        }
      }
    }

    private class MyPrjGlobal : Basics.Geom.IProjection
    {
      private readonly double _x0;
      private readonly double _y0;

      public MyPrjGlobal(double x0, double y0)
      {
        _x0 = x0;
        _y0 = y0;
      }

      public Basics.Geom.IPoint Project(Basics.Geom.IPoint point)
      {
        return new Basics.Geom.Point2D(point.X + _x0, -point.Y + _y0);
      }
    }


    private class MyPrjLocal : Basics.Geom.IProjection
    {
      private readonly Basics.Geom.Point2D _translate;
      private readonly double _scale;

      public MyPrjLocal(Basics.Geom.Point2D translate, double scale)
      {
        _translate = translate;
        _scale = scale;
      }

      public Basics.Geom.IPoint Project(Basics.Geom.IPoint point)
      {
        return new Basics.Geom.Point2D(point.X / _scale + _translate.X, point.Y / _scale + _translate.Y); 
      }
    }

    private void Transfer(SymbolCurve symCurve, Basics.Geom.Point2D pnt, int color, Ocad9Writer w)
    {
      IDrawable curve = symCurve.Curve;
      Basics.Geom.IGeometry curveGeom = curve.GetGeometry();

      Basics.Geom.IGeometry geom = curveGeom.Project(new MyPrjLocal(pnt, _symbolScale));
      GeomType geomType;

      if (symCurve.LineWidth <= 0)
      {
        geom = new Basics.Geom.Area((Basics.Geom.Polyline)geom);
        geomType = GeomType.area;
      }
      else
      { geomType = GeomType.line; }

      ElementV9 elem = new ElementV9(true);
      elem.Type = geomType;
      elem.Color = color;
      elem.LineWidth = (int)(symCurve.LineWidth / _symbolScale * 100);
      elem.Geometry = geom;
      elem.Symbol = -3;

      w.Append(elem);
    }

    private void Transfer(Elem e, Ocad9Writer w)
    {
      IDrawable eGeom = e.Geometry;
      if (eGeom == null)
      { return; }
      Basics.Geom.IGeometry geom = eGeom.GetGeometry().Project(_prj);
      System.Windows.Media.Color eColor = e.Color?.Color?? System.Windows.Media.Colors.Red;
      int color = Color.FromRgb(eColor.R, eColor.G, eColor.B).ToNumber();

      SymbolType type = e.Symbol.GetSymbolType();
      if (type == SymbolType.Point)
      {
        foreach (SymbolCurve curve in e.Symbol.Curves)
        { Transfer(curve, (Basics.Geom.Point2D)geom, color, w); }
      }
      else if (type == SymbolType.Text)
      {
        ElementV9 elem = new ElementV9(true);
        elem.Type = GeomType.unformattedText;
        elem.Color = color;
        elem.Geometry = geom;
        elem.Text = e.Symbol.Text;
        elem.Symbol = -3;

        w.Append(elem);
      }
      else if (type == SymbolType.Line)
      {
        ElementV9 elem = new ElementV9(true);
        elem.Type = GeomType.line;
        elem.LineWidth = (int)(e.Symbol.Curves[0].LineWidth / _symbolScale * 100);
        elem.Color = color;
        elem.Geometry = geom;
        elem.Symbol = -3;

        w.Append(elem);
      }
      else
      { throw new NotImplementedException($"unhandled type {type}"); }
    }
  }
}
