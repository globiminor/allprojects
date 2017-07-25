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

    private int GetLineWidth(OcadWriter w, double width)
    {
      return (int)(width * _symbolScale * 100000 / w.Setup.Scale);
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
        TransferPoint((Basics.Geom.Point2D)geom, (eGeom as DirectedPnt)?.Azimuth, e.Symbol.Curves, color, w);
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
        TransferLine((Basics.Geom.Polyline)geom, e.Symbol.Curves, color, w);
      }
      else
      { throw new NotImplementedException($"unhandled type {type}"); }
    }

    private void TransferLine(Basics.Geom.Polyline line, IEnumerable<SymbolCurve> symParts, int color, Ocad9Writer w)
    {
      foreach (SymbolCurve sym in symParts)
      {
        if (sym.Curve == null)
        {
          if (sym.Dash == null)
          {
            ElementV9 elem = new ElementV9(true);
            elem.Type = GeomType.line;
            elem.LineWidth = GetLineWidth(w, sym.LineWidth);
            elem.Color = color;
            elem.Geometry = line;
            elem.Symbol = -3;

            w.Append(elem);
          }
          else
          {
            double length = line.Length();
            float pre = -1;
            int i = 0;
            foreach (float pos in sym.Dash.GetPositions((float)length))
            {
              i++;
              if (i % 2 == 0)
              {
                Basics.Geom.Polyline dash = line.SubPart(pre, Math.Min(pos, length) - pre);

                ElementV9 elem = new ElementV9(true);
                elem.Type = GeomType.line;
                elem.LineWidth = GetLineWidth(w, sym.LineWidth);
                elem.Color = color;
                elem.Geometry = dash;
                elem.Symbol = -3;

                w.Append(elem);
              }
              pre = pos;
              if (pos > length)
              { break; }
            }
          }
        }
        else if (sym.Dash != null)
        {
          double length = line.Length();
          foreach (float pos in sym.Dash.GetPositions((float)length))
          {
            if (pos > length)
            { break; }

            double[] pars = line.ParamAt(pos);
            Basics.Geom.Curve seg = line.Segments[(int)pars[0]];
            Basics.Geom.Point tan = seg.TangentAt(pars[1]);

            double azimuth = Math.Atan2(tan.X, tan.Y) + Math.PI / 2;
            TransferPoint(seg.PointAt(pars[1]), (float)azimuth, new[] { sym }, color, w);
          }
        }
      }

    }
    private void TransferPoint(Basics.Geom.IPoint pnt, float? azimuth, IEnumerable<SymbolCurve> symParts, int color, Ocad9Writer w)
    {
      MyPrjRotate rotate = null;
      if (azimuth != null) { rotate = new MyPrjRotate(azimuth.Value); }
      foreach (SymbolCurve curve in symParts)
      {
        IDrawable lin = curve.Curve;
        Basics.Geom.Polyline localLine = (Basics.Geom.Polyline)lin.GetGeometry();
        if (rotate != null)
        { localLine = localLine.Project(rotate); }

        TransferLocal(localLine, curve.LineWidth, pnt, color, w);
      }
    }

    private void TransferLocal(Basics.Geom.Polyline localLine, double lineWidth, Basics.Geom.IPoint pnt, int color, Ocad9Writer w)
    {
      Basics.Geom.Polyline line = localLine.Project(new MyPrjLocal(pnt, _symbolScale));
      if (line.HasNonBezier())
      { line = line.ToBeziers(); }

      GeomType geomType;

      ElementV9 elem = new ElementV9(true);
      if (lineWidth <= 0)
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
      elem.LineWidth = GetLineWidth(w, lineWidth);
      elem.Symbol = -3;

      w.Append(elem);
    }

  }
}
