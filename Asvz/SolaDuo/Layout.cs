using Basics.Geom;
using Ocad;
using System;
using System.Collections.Generic;

namespace Asvz
{
  public abstract class Layout
  {
    private readonly string _fileName;
    private Setup _templateSetup;

    protected Layout(string fileName)
    {
      _fileName = fileName;
    }

    protected delegate void Function<T>(OcadWriter writer, T data);

    protected void Update<T>(T data, Function<T> function)
    {
      OcadReader update = OcadReader.Open(_fileName);
      ReadUpdate(update);
      update.Close();

      using (OcadWriter writer = OcadWriter.AppendTo(_fileName))
      {
        function(writer, data);
        writer.Close();
      }
    }

    internal Setup Setup
    {
      get { return _templateSetup; }
    }

    private void ReadUpdate(OcadReader template)
    {
      _templateSetup = template.ReadSetup();

      ReadUpdateSymbols(template);
      ReadUpdateObjects(template);
      ReadUpdateStringParams(template);
    }

    protected abstract void ReadUpdateSymbols(OcadReader template);
    protected abstract void ReadUpdateObjects(OcadReader template);
    protected abstract void ReadUpdateStringParams(OcadReader template);

    protected abstract Ocad.Symbol.TextSymbol KmTxtSymbol { get; }
    protected abstract int KmStrichSymbol { get; }

    protected void WriteKm(OcadWriter writer, Polyline strecke, double f, string suffix, bool flipOnConflict)
    {
      List<KmElem> elems = GetKm(writer.Setup, strecke, f, suffix, flipOnConflict);
      foreach (var elem in elems)
      {
        writer.Append(elem.Text);
        writer.Append(elem.Strich);
      }
    }

    protected List<KmElem> GetKm(Setup wSetup, Polyline strecke, double f, string suffix, bool flipOnConflict)
    {
      Setup setup = _templateSetup;
      Ocad.Symbol.TextSymbol kmTxtSymbol = KmTxtSymbol;

      double dLength = strecke.Project(Geometry.ToXY).Length() / 1000.0;
      double dSum = f;
      int iKm = 1;

      List<KmElem> elems = new List<KmElem>();
      while (dSum < dLength)
      {
        KmElem kmElem = new KmElem();
        elems.Add(kmElem);

        double[] param = strecke.ParamAt(iKm * 1000.0 * f);
        IPoint p = strecke.Segments[(int)param[0]].PointAt(param[1]);
        IPoint t = strecke.Segments[(int)param[0]].TangentAt(param[1]);
        GeoElement elem = CreateKmText(p, t, setup, iKm, kmTxtSymbol, suffix);
        PointCollection points = ((PointCollection)elem.Geometry).Clone();
        points.Add(points[1]);
        points.Insert(0, p + 0.01 * PointOperator.Sub(points[0], p));
        Polyline pPoly = Polyline.Create(points);
        if (flipOnConflict && strecke.Intersection(pPoly) != null)
        {
          t = PointOperator.Scale(-1.0, t);
          elem = CreateKmText(p, t, setup, iKm, kmTxtSymbol, suffix);
        }
        kmElem.Text = elem;

        elem = new GeoElement(p);
        elem.Angle = wSetup.PrjRotation + Math.Atan2(t.X, -t.Y);
        elem.Symbol = KmStrichSymbol;
        elem.Type = GeomType.point;

        kmElem.Strich = elem;

        dSum += f;
        iKm += 1;
      }
      return elems;
    }

    protected class KmElem
    {
      public Element Text { get; set; }
      public GeoElement Strich { get; set; }
    }

    private static GeoElement CreateKmText(IPoint p, IPoint t, Setup setup, int km,
    Ocad.Symbol.TextSymbol kmTxtSymbol, string suffix)
    {
      string sKm = km.ToString();
      if (suffix != null)
      { sKm += suffix; }

      Point text = new Point2D(t.Y, -t.X);
      text = 130.0 * 1.0 / Math.Sqrt(text.OrigDist2()) * text;
      text = text + p;

      GeoElement elem = Common.CreateText(sKm, text.X, text.Y, setup, kmTxtSymbol);
      PointCollection list = (PointCollection)elem.Geometry;
      Point pM = 0.5 * PointOperator.Add(list[1], list[3]);
      text = 2.0 * text - pM;

      return Common.CreateText(sKm, text.X, text.Y, setup, kmTxtSymbol);
    }

  }
}
