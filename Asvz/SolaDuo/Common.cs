using System;
using Basics.Geom;
using Ocad;

namespace Asvz
{
  public static class Common
  {
    internal static ElementV9 CreateText(string text, double x, double y, Ocad.Symbol.TextSymbol symbol, Setup setup)
    {
      bool project = setup != null;
      ElementV9 elem = new ElementV9(project);
      elem.Symbol = symbol.Number;
      elem.UnicodeText = true;

      Point p = new Point2D(x, y);
      if (project)
      {  p = p.Project(setup.Prj2Map);}

      IGeometry geom = GetGeometry(text, p, symbol);
      if (project) 
      { geom = geom.Project(setup.Map2Prj); }

      elem.Geometry = geom;
      elem.Type = GeomType.unformattedText;
      elem.Text = text;

      return elem;
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
