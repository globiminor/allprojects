using Ocad;
using OcadScratch.ViewModels;
using OMapScratch;
using System;
using System.Collections.Generic;
using System.IO;

namespace OcadScratch
{
  public class CmdTransfer : IDisposable
  {
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
        return new Basics.Geom.Point2D(point.X * _scale + _translate.X, point.Y * _scale + _translate.Y);
      }
    }

    private readonly IList<WorkElemVm> _elems;
    private readonly string _ocdFile;
    private readonly Map _map;

    private Basics.Geom.IProjection _prj;
    private double _symbolScale;

    public bool UseBackupFile { get; set; }

    public CmdTransfer(Map map,IList<WorkElemVm> elems, string ocdFile)
    {
      _map = map;
      _elems = elems;
      _ocdFile = ocdFile;
      UseBackupFile = true;
    }

    void IDisposable.Dispose()
    { }

    public void Execute()
    {
      if (_map == null)
      { return; }
      if (_elems == null)
      { return; }
      if (_ocdFile == null)
      { return; }

      if (UseBackupFile)
      { HandleBackup(_ocdFile); }

      _prj = null;
      _symbolScale = 1;

      _prj = _map.GetGlobalPrj();
      _symbolScale = _map.SymbolScale;

      using (Ocad9Writer w = Ocad9Writer.AppendTo(_ocdFile))
      {
        foreach (WorkElemVm elem in _elems)
        {
          Transfer(elem.Elem, w);
        }
      }
    }

    private void HandleBackup(string file)
    {
      string ext = Path.GetExtension(file);
      string backup = Path.ChangeExtension(file, $".orig{ext}");
      if (File.Exists(backup))
      { File.Copy(backup, file, true); }
      else
      { File.Copy(file, backup); }
    }

    private void Transfer(SymbolCurve symCurve, Basics.Geom.Point2D pnt, int color, Ocad9Writer w)
    {
      IDrawable curve = symCurve.Curve;
      Basics.Geom.Polyline localLine = (Basics.Geom.Polyline)curve.GetGeometry();

      Basics.Geom.Polyline line = localLine.Project(new MyPrjLocal(pnt, _symbolScale));
      if (line.HasNonBezier())
      { line = line.ToBeziers(); }

      GeomType geomType;

      ElementV9 elem = new ElementV9(true);
      if (symCurve.LineWidth <= 0)
      {
        elem.Geometry = new Basics.Geom.Area(line);
        geomType = GeomType.area;
      }
      else
      {
        geomType = GeomType.line;
        elem.Geometry = line;
      }

      elem.Type = geomType;
      elem.Color = color;
      elem.LineWidth = (int)(symCurve.LineWidth * _symbolScale / 100);
      elem.Symbol = -3;

      w.Append(elem);
    }

    private void Transfer(Elem e, Ocad9Writer w)
    {
      IDrawable eGeom = e.Geometry;
      if (eGeom == null)
      { return; }
      Basics.Geom.IGeometry geom = eGeom.GetGeometry().Project(_prj);
      System.Windows.Media.Color eColor = e.Color?.Color ?? System.Windows.Media.Colors.Red;
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
        Basics.Geom.PointCollection points = new Basics.Geom.PointCollection();
        Basics.Geom.Point2D center = (Basics.Geom.Point2D)geom;
        points.Add(new Basics.Geom.Point2D(center.X - 2, center.Y - 2));
        //points.Add(center);
        //points.Add(new Basics.Geom.Point2D(center.X - 0.1, center.Y - 0.1));
        //points.Add(new Basics.Geom.Point2D(center.X + 0.1, center.Y - 0.1));
        //points.Add(new Basics.Geom.Point2D(center.X + 0.1, center.Y + 0.2));
        //points.Add(new Basics.Geom.Point2D(center.X - 0.1, center.Y + 0.2));
        elem.Geometry = points;
        elem.Text = e.Symbol.Text;
        elem.Symbol = -3;

        w.Append(elem);
      }
      else if (type == SymbolType.Line)
      {
        ElementV9 elem = new ElementV9(true);
        elem.Type = GeomType.line;
        elem.LineWidth = (int)(e.Symbol.Curves[0].LineWidth * _symbolScale / 100);
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
