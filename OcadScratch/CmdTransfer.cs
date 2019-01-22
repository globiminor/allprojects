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
    private class MyPrjRotate : Basics.Geom.IProjection
    {
      private readonly double _sin;
      private readonly double _cos;

      public MyPrjRotate(double azimuth)
      {
        _sin = Math.Sin(azimuth);
        _cos = Math.Cos(azimuth);
      }

      public Basics.Geom.IPoint Project(Basics.Geom.IPoint point)
      {
        return new Basics.Geom.Point2D(_cos * point.X + _sin * point.Y, -_sin * point.X + _cos * point.Y);
      }
    }

    private class MyPrjLocal : Basics.Geom.IProjection
    {
      private readonly Basics.Geom.IPoint _translate;
      private readonly double _scale;

      public MyPrjLocal(Basics.Geom.IPoint translate, double scale)
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

    public CmdTransfer(Map map, IList<WorkElemVm> elems, string ocdFile)
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

      using (OcadWriter w = OcadWriter.AppendTo(_ocdFile))
      {
        foreach (var elem in _elems)
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

    private int GetLineWidth(OcadWriter w, double width)
    {
      return (int)(width * _symbolScale * 100000 / w.Setup.Scale);
    }
    private void Transfer(Elem e, OcadWriter w)
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
        TransferPoint((Basics.Geom.Point2D)geom, (eGeom as DirectedPnt)?.Azimuth, e.Symbol.Curves, color, w);
      }
      else if (type == SymbolType.Text)
      {
        if (_charSymbols.TryGetValue(e.Symbol.Text, out List<SymbolCurve> curves))
        {
          Basics.Geom.Point2D center = (Basics.Geom.Point2D)geom;
          TransferPoint(new Basics.Geom.Point2D(center.X - 2, center.Y - 2), (eGeom as DirectedPnt)?.Azimuth, curves, color, w);
        }
        else
        {
          Basics.Geom.PointCollection points = new Basics.Geom.PointCollection();
          Basics.Geom.Point2D center = (Basics.Geom.Point2D)geom;
          points.Add(new Basics.Geom.Point2D(center.X - 2, center.Y - 2));
          //points.Add(center);
          //points.Add(new Basics.Geom.Point2D(center.X - 0.1, center.Y - 0.1));
          //points.Add(new Basics.Geom.Point2D(center.X + 0.1, center.Y - 0.1));
          //points.Add(new Basics.Geom.Point2D(center.X + 0.1, center.Y + 0.2));
          //points.Add(new Basics.Geom.Point2D(center.X - 0.1, center.Y + 0.2));

          Element elem = new GeoElement(points);
          elem.Type = GeomType.unformattedText;
          elem.Color = color;
          elem.Text = e.Symbol.Text;
          elem.Symbol = -3;

          w.Append(elem);
        }
      }
      else if (type == SymbolType.Line)
      {
        TransferLine((Basics.Geom.Polyline)geom, e.Symbol.Curves, color, w);
      }
      else
      { throw new NotImplementedException($"unhandled type {type}"); }
    }

    private Dictionary<string, List<SymbolCurve>> _charSymbols = new Dictionary<string, List<SymbolCurve>>
    {
      { "0", GetGeom(" a 0 3 3 270 180"," m 3 3 l 3 -3"," a 0 -3 3 90 180"," m -3 -3 l -3 3") },
      { "1", GetGeom(" m -2.0 4 l 2 6 l 2 -6") },
      { "2", GetGeom(" a 0 3 3 270 210", " m 2.0 1.5 l -3 -6 l 3 -6") },
      { "3", GetGeom(" a 0 3 3 270 270", " a 0 -3 3 0 270") },
      { "4", GetGeom(" m 1 -6 l 1 6 l -3 -2 l 3 -2") },
      { "5", GetGeom(" m 3 6 l -3 6 l -3 1 l 0 1", " a -0.5 -2.5 3.5 0 240") },
      { "6", GetGeom(" a 3 0 6 270 90", " m -3 -3 l -3 0 l 0 0", " a 0 -3 3 0 270") },
      { "7", GetGeom(" m -3 6 l 3 6 l -3 -6") },
      { "8", GetGeom(" r 0 3 3", " r 0 -3 3") },
      { "A", GetGeom(" m -3 -6 l 0 6 l 3 -6", " m -2 -2 l 2 -2 ") },
      { "B", GetGeom(" a 0 3 3 0 180", " a 0 -3 3 0 180", " m 0 -6 l -3 -6 l -3 6 l 0 6", " m -3 0 l 0 0") },
      { "C", GetGeom(" a 0 3 3 270 180", " a 0 -3 3 90 180", " m -3 -3 l -3 3") },
    };

    private static List<SymbolCurve> GetGeom(params string[] parts)
    {
      List<SymbolCurve> curves = new List<SymbolCurve>();
      foreach (var part in parts)
      {
        SymbolCurve curve = new SymbolCurve { Curve = (Curve)DrawableUtils.GetGeometry(part), LineWidth = 1, Stroke = true };
        curves.Add(curve);
      }

      return curves;
    }

    private void TransferLine(Basics.Geom.Polyline line, IEnumerable<SymbolCurve> symParts, int color, OcadWriter w)
    {
      foreach (var sym in symParts)
      {
        if (sym.Curve == null)
        {
          if (sym.Dash == null)
          {
            Element elem = new GeoElement(line);
            elem.Type = GeomType.line;
            elem.LineWidth = GetLineWidth(w, sym.LineWidth);
            elem.Color = color;
            elem.Symbol = -3;

            w.Append(elem);
          }
          else
          {
            double lUnscaled = line.Length();
            double lScaled = lUnscaled / _symbolScale;
            double pre = -1;
            int i = 0;
            foreach (var posScaled in sym.Dash.GetPositions(lScaled))
            {
              i++;
              double posUnscaled = posScaled * _symbolScale;
              if (i % 2 == 0)
              {
                Basics.Geom.Polyline dash = line.SubPart(pre, Math.Min(posUnscaled, lUnscaled) - pre);

                Element elem = new GeoElement(dash);
                elem.Type = GeomType.line;
                elem.LineWidth = GetLineWidth(w, sym.LineWidth);
                elem.Color = color;
                elem.Symbol = -3;

                w.Append(elem);
              }
              pre = posUnscaled;
              if (posUnscaled > lUnscaled)
              { break; }
            }
          }
        }
        else if (sym.Dash != null)
        {
          double lUnscaled = line.Length();
          double lScaled = lUnscaled / _symbolScale;
          foreach (var posScaled in sym.Dash.GetPositions(lScaled))
          {
            double posUnscaled = posScaled * _symbolScale;
            if (posUnscaled > lUnscaled)
            { break; }

            double[] pars = line.ParamAt(posUnscaled);
            Basics.Geom.ISegment seg = line.GetSegment((int)pars[0]);
            Basics.Geom.IPoint tan = seg.TangentAt(pars[1]);

            double azimuth = Math.Atan2(tan.X, tan.Y) + Math.PI / 2;
            TransferPoint(seg.PointAt(pars[1]), azimuth, new[] { sym }, color, w);
          }
        }
      }

    }
    private void TransferPoint(Basics.Geom.IPoint pnt, double? azimuth, IEnumerable<SymbolCurve> symParts, int color, OcadWriter w)
    {
      MyPrjRotate rotate = null;
      if (azimuth != null) { rotate = new MyPrjRotate(azimuth.Value); }
      foreach (var curve in symParts)
      {
        IDrawable lin = curve.Curve;
        Basics.Geom.Polyline localLine = (Basics.Geom.Polyline)lin.GetGeometry();
        if (rotate != null)
        { localLine = localLine.Project(rotate); }

        TransferLocal(localLine, curve.LineWidth, pnt, color, w);
      }
    }

    private void TransferLocal(Basics.Geom.Polyline localLine, double lineWidth, Basics.Geom.IPoint pnt, int color, OcadWriter w)
    {
      Basics.Geom.Polyline line = localLine.Project(new MyPrjLocal(pnt, _symbolScale));
      if (line.HasNonBezier())
      { line = line.ToBeziers(); }

      GeomType geomType;

      Element elem;
      if (lineWidth <= 0)
      {
        elem = new GeoElement(new Basics.Geom.Area(line));
        geomType = GeomType.area;
      }
      else
      {
        elem = new GeoElement(line);
        geomType = GeomType.line;
      }

      elem.Type = geomType;
      elem.Color = color;
      elem.LineWidth = GetLineWidth(w, lineWidth);
      elem.Symbol = -3;

      w.Append(elem);
    }

  }
}
