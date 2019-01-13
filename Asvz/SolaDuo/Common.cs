using System;
using System.Collections.Generic;
using Basics.Geom;
using Ocad;

namespace Asvz
{
  public static class Common
  {
    internal static GeoElement CreateText(string text, double x, double y, Setup setup, Ocad.Symbol.TextSymbol symbol)
    {
      Point p = new Point2D(x, y);
      p = p.Project(setup.Prj2Map);
      IGeometry geom = GetGeometry(text, p, symbol);
      geom = geom.Project(setup.Map2Prj);

      GeoElement elem = new GeoElement(geom);
      AssignText(elem, text, symbol);

      return elem;
    }

    internal static MapElement CreateText(string text, double x, double y, Ocad.Symbol.TextSymbol symbol)
    {
      Point p = new Point2D(x, y);
      IGeometry geom = GetGeometry(text, p, symbol);
      List<Coord> coords = new List<Coord>(Coord.EnumCoords(geom));

      MapElement elem = new MapElement(coords);
      AssignText(elem, text, symbol);

      return elem;
    }

    private static void AssignText(Element elem, string text, Ocad.Symbol.TextSymbol symbol)
    {
      elem.Symbol = symbol.Number;
      elem.UnicodeText = true;
      elem.Type = GeomType.unformattedText;
      elem.Text = text;
    }


    public static PointCollection GetGeometry(string text, IPoint position, Ocad.Symbol.TextSymbol symbol)
    {
      System.Windows.Forms.Label pLabel = new System.Windows.Forms.Label();
      pLabel.CreateControl();
      System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(pLabel.Handle);
      System.Drawing.SizeF f = gr.MeasureString(text,
        new System.Drawing.Font(symbol.FontName, (float)(symbol.Size)));

      double dRight = f.Width;// *1.05;

      double x0;
      double x1;
      if (symbol.Alignment == Ocad.Symbol.TextSymbol.AlignmentMode.Left ||
        symbol.Alignment == Ocad.Symbol.TextSymbol.AlignmentMode.Justified)
      {
        x0 = 0;
        x1 = dRight * 2.54;
      }
      else if (symbol.Alignment == Ocad.Symbol.TextSymbol.AlignmentMode.Right)
      {
        x0 = -dRight * 2.54;
        x1 = 0;
      }
      else if (symbol.Alignment == Ocad.Symbol.TextSymbol.AlignmentMode.Center)
      {
        x0 = -dRight * 2.54 / 2.0;
        x1 = -x0;
      }
      else
      { throw new ArgumentException("Unhandled AlignmentMode " + symbol.Alignment.ToString()); }

      double y = f.Height * 2.54;

      PointCollection pList = new PointCollection();
      pList.Add(position);
      pList.Add(position + new Point2D(x0, -0.15 * y));
      pList.Add(position + new Point2D(x1, -0.15 * y));
      pList.Add(position + new Point2D(x1, 0.85 * y));
      pList.Add(position + new Point2D(x0, 0.85 * y));

      return pList;
    }
  }
}
