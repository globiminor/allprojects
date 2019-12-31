

using Basics.Geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ocad;
using Ocad.StringParams;
using Ocad.Symbol;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace OTextSharp.Models
{
  internal class ImagePoBeWriter : PdfWrite
  {
    private readonly OcadReader _reader;
    private readonly Dictionary<string, ControlInfo> _infos;
    private readonly string _export;
    private readonly string _imageTemplate;
    private int _ix;
    private int _iy;

    private readonly int _maxX = 2;
    private readonly int _maxY = 4;

    private readonly float _w;
    private readonly float _h;

    private readonly float _mx = 10;
    private readonly float _my = 10;
    private Dictionary<string, ControlPar> _controlPars;
    private Dictionary<int, PointSymbol> _symbs;
    private readonly Setup _setup;

    public ImagePoBeWriter(OcadReader reader, IList<ControlInfo> infos, string export, string imageTemplate,
      int maxCols = 2, int maxRows = 4)
      : base(export)
    {
      _reader = reader;
      _infos = infos.ToDictionary(x => x.Key);
      _export = export;
      _imageTemplate = imageTemplate;

      _maxX = maxCols;
      _maxY = maxRows;

      _ix = 0;
      _iy = 0;

      _w = (DefaultWidth - 2 * _mx) / _maxX;
      _h = (DefaultHeight - 2 * _my) / _maxY;

      DescriptionSize = 16;

      _controlPars = new Dictionary<string, ControlPar>();
      IList<StringParamIndex> idxs = _reader.ReadStringParamIndices();
      foreach (var idx in idxs)
      {
        if (idx.Type == StringType.Control)
        {
          string stringParam = reader.ReadStringParam(idx);
          ControlPar ctrPar = new ControlPar(stringParam);
          _controlPars.Add(ctrPar.Name, ctrPar);
        }
      }
      foreach (var elem in _reader.EnumMapElements(null))
      {
        if (elem.ObjectStringType == ObjectStringType.None)
        { continue; }

        if (elem.ObjectStringType != ObjectStringType.CsObject)
        { continue; }

        if (elem.ObjectString.Length < 2)
        { continue; }

        ControlPar ctrPar = new ControlPar(elem.ObjectString.Substring(1).TrimStart('0'));
        _controlPars.Add(ctrPar.Name, ctrPar);
      }
      _setup = reader.ReadSetup();

      _symbs = new Dictionary<int, PointSymbol>();
      foreach (var symbol in _reader.ReadSymbols())
      {
        if (!(symbol is PointSymbol p))
        {
          continue;
        }
        _symbs.Add(symbol.Number, p);
      }
    }

    public void SetY(int iy)
    {
      _iy = iy;
    }

    public void AddImage(Control ctr)
    {
      if (_ix >= _maxX)
      {
        _ix = 0;
        _iy++;
      }

      if (_iy >= _maxY)
      {
        _iy = 0;
        NewPage();
      }

      string name = _imageTemplate.Replace("*", ctr.Name);
      Uri uri = new Uri(name);
      float x0 = _ix * _w + _mx;
      float y0 = (_maxY - 1 - _iy) * _h + _my;
      if (uri.IsFile && !System.IO.File.Exists(name))
      { }
      else
      {
        Image image = new Jpeg(uri);

        float w, h;
        float fx = _w / image.Width;
        float fy = _h / image.Height;
        if (fx > fy)
        {
          w = fy * image.Width;
          h = _h;
        }
        else
        {
          w = _w;
          h = fx * image.Height;
        }

        float f = 0.85f;
        Pcb.AddImage(image, w * f, 0, 0, h * f, x0 + (1 - f) / 2 * w, y0 + 12 + (1 - f) / 2 * h);
      }

      DrawControl(ctr.Name, x0 + 12, y0 + 12);

      Pcb.MoveTo(x0, y0);
      Pcb.LineTo(x0, y0 + _h);
      Pcb.LineTo(x0 + _w, y0 + _h);
      Pcb.LineTo(x0 + _w, y0);
      Pcb.LineTo(x0, y0);
      Pcb.Stroke();

      _ix++;
    }

    public void DrawControl(string name, float x0, float y0)
    {
      ControlPar p = _controlPars[name];

      string txt = null;
      if (_infos.TryGetValue(p.Name, out ControlInfo info))
      {
        txt = info.Info;
      }
      DrawControl(p, txt, x0, y0);
    }

    public float DescriptionSize { get; set; }
    private void DrawControl(ControlPar p, string info, float x0, float y0)
    {
      float d = DescriptionSize;
      x0 += 1.5f * d;
      SetFontSize(8);
      ShowText(PdfContentByte.ALIGN_CENTER, p.Name, x0, y0 - 4, 0);

      Pcb.SaveState();
      Pcb.MoveTo(x0 - 1.5f * d, y0 + d / 2);
      Pcb.LineTo(x0 + 6.5f * d, y0 + d / 2);
      Pcb.LineTo(x0 + 6.5f * d, y0 - d / 2);
      Pcb.LineTo(x0 - 1.5f * d, y0 - d / 2);
      Pcb.LineTo(x0 - 1.5f * d, y0 + d / 2);
      Pcb.Stroke();
      for (int i = 0; i < 7; i++)
      {
        Pcb.MoveTo(x0 + (i - 0.5f) * d, y0 + d / 2);
        Pcb.LineTo(x0 + (i - 0.5f) * d, y0 - d / 2);
        Pcb.Stroke();
      }
      Pcb.RestoreState();

      DrawSymbol(p.GetString(ControlPar.SymCKey), x0 + 1 * d, y0);
      DrawSymbol(p.GetString(ControlPar.SymDKey), x0 + 2 * d, y0);
      DrawSymbol(p.GetString(ControlPar.SymEKey), x0 + 3 * d, y0);
      DrawSymbol(p.GetString(ControlPar.SymFKey), x0 + 4 * d, y0);
      DrawSymbol(p.GetString(ControlPar.SymGKey), x0 + 5 * d, y0);
      DrawSymbol(p.GetString(ControlPar.SymHKey), x0 + 6 * d, y0);

      if (!string.IsNullOrWhiteSpace(info))
      {
        ShowText(PdfContentByte.ALIGN_LEFT, info, x0 + 7 * d, y0 - 4, 0);
      }
    }

    private void DrawSymbol(string sym, float x0, float y0)
    {
      if (sym == null)
      { return; }

      int symIdx = (int)Math.Round(double.Parse(sym) * 1000);
      PointSymbol ps = _symbs[symIdx];

      Pcb.SaveState();
      Pcb.Transform(new Matrix(0.028f, 0, 0, 0.028f, x0, y0));
      foreach (var graphics in ps.Graphics)
      {
        if (graphics.Type == SymbolGraphicsType.Line)
        {
          Pcb.SetLineWidth(graphics.LineWidth);
          DrawCurve(((GeoElement.Line)graphics.MapGeometry).BaseGeometry);
          Pcb.Stroke();
        }
        if (graphics.Type == SymbolGraphicsType.Area)
        {
          foreach (var border in (((GeoElement.Area)graphics.MapGeometry).BaseGeometry).Border)
          {
            DrawCurve(border);
            Pcb.Fill();
          }
        }
        else if (graphics.Type == SymbolGraphicsType.Circle)
        {
          Pcb.SetLineWidth(graphics.LineWidth);
          DrawCurve(((GeoElement.Line)graphics.MapGeometry).BaseGeometry);
          //IPoint center = (IPoint)graphics.Geometry;
          //Pcb.MoveTo((float)center.X + graphics.Diameter, (float)center.Y);
          //Pcb.Circle((float)center.X, (float)center.Y, graphics.Diameter);
          Pcb.Stroke();
        }
        else if (graphics.Type == SymbolGraphicsType.Dot)
        {
          DrawCurve(((GeoElement.Area)graphics.MapGeometry).BaseGeometry.Border[0]); 
          Pcb.Fill();
        }
      }
      Pcb.RestoreState();
    }
  }
}