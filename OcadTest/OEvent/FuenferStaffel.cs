
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using OTextSharp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace OcadTest.OEvent
{
  [TestClass]
  public class FuenferStaffel
  {
    private class FuenferSettings
    {
      public string PdfRoot;
      public string OutRoot;
      public string FrontBackground;
      public string RueckBackgroud;
    }

    [TestMethod]
    [Obsolete]
    public void RunCreatePdfs2015()
    {
      CreatePdfs2015();
    }

    [TestMethod]
    public void RunCreatePdfs2017()
    {
      CreatePdfs2017();
    }

    [TestMethod]
    public void Join2015()
    {
      FuenferSettings settings = new FuenferSettings
      {
        PdfRoot = @"C:\daten\felix\kapreolo\karten\chomberg_bruetten\2015\Bahnen\",
        FrontBackground = @"C:\daten\felix\kapreolo\karten\chomberg_bruetten\2015\Bahnen\_Staffel.Front.pdf",
        RueckBackgroud = @"C:\daten\felix\kapreolo\karten\chomberg_bruetten\2015\Bahnen\_Staffel.Rueck.pdf",
        OutRoot = @"C:\daten\felix\kapreolo\karten\chomberg_bruetten\2015\Druck\Staffel"
      };

      int maxPages = 80;

      FuenferJoinPdfs(settings, maxPages);
      string pdfRoot = @"C:\daten\felix\kapreolo\karten\chomberg_bruetten\2015\";

      {
        string exp = string.Format("{0}__4a.pdf", settings.OutRoot);
        PdfText.OverprintFrontBack(exp, new string[] {
          Path.Combine(pdfRoot, "Staffel_Front_4a.pdf"),
          Path.Combine(pdfRoot, "Rueck.4a.pdf")},
          settings.FrontBackground, settings.RueckBackgroud);
      }
      {
        string exp = string.Format("{0}__4b.pdf", settings.OutRoot);
        PdfText.OverprintFrontBack(exp, new string[] {
          Path.Combine(pdfRoot, "Staffel_Front_4b.pdf"),
          Path.Combine(pdfRoot, "Rueck.4b.pdf")},
          settings.FrontBackground, settings.RueckBackgroud);
      }
      {
        string exp = string.Format("{0}__4c.pdf", settings.OutRoot);
        PdfText.OverprintFrontBack(exp, new string[] {
          Path.Combine(pdfRoot, "Staffel_Front_4c.pdf"),
          Path.Combine(pdfRoot, "Rueck.4c.pdf")},
          settings.FrontBackground, settings.RueckBackgroud);
      }
      List<int[]> pairs = new List<int[]> { new[] { 201, 210 }, new[] { 211, 220 }, new[] { 221, 230 } };
      foreach (var pair in pairs)
      {
        List<string> files = new List<string>();
        for (int n = pair[0]; n <= pair[1]; n++)
        {
          files.Add(Path.Combine(pdfRoot, string.Format("Kids.Front_1.pdf", n)));
          files.Add(Path.Combine(pdfRoot, string.Format("Rueck.Kids-Staffel.{0}.1.pdf", n)));
          files.Add(Path.Combine(pdfRoot, string.Format("Kids.Front_2.pdf", n)));
          files.Add(Path.Combine(pdfRoot, string.Format("Rueck.Kids-Staffel.{0}.2.pdf", n)));
          files.Add(Path.Combine(pdfRoot, string.Format("Kids.Front_3.pdf", n)));
          files.Add(Path.Combine(pdfRoot, string.Format("Rueck.Kids-Staffel.{0}.3.pdf", n)));
        }
        string root = Path.GetDirectoryName(pdfRoot);
        string exp = Path.Combine(pdfRoot, "Druck", string.Format("Kids_{0}_{1}.pdf", pair[0], pair[1]));
        PdfText.OverprintFrontBack(exp, files,
          settings.FrontBackground, settings.RueckBackgroud);
      }
      return;
      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Kids_2.pdf", root);
      //  PdfText.OverprintFrontBack(exp, new string[] {
      //    Path.Combine(root, "Kids.2.Front.pdf"),
      //    Path.Combine(root, "Kids.2.Rueck.pdf")},
      //    settings.FrontBackground, settings.RueckBackgroud);
      //}
      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Kids_3.pdf", root);
      //  PdfText.OverprintFrontBack(exp, new string[] {
      //    Path.Combine(root, "Kids.3.Front.pdf"),
      //    Path.Combine(root, "Kids.3.Rueck.pdf")},
      //    settings.FrontBackground, settings.RueckBackgroud);
      //}

      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Posten.pdf", root);
      //  PdfText.Overprint(null, new string[] {
      //    settings.FrontBackground,
      //    Path.Combine(root, "PostenNetzFront.pdf")
      //    }, exp);
      //}

      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Hardwald.pdf", root);
      //  PdfText.Overprint(null, new string[] { settings.FrontBackground }, exp);
      //}
    }

    [TestMethod]
    public void Join2017()
    {
      FuenferSettings settings = new FuenferSettings
      {
        PdfRoot = @"C:\daten\felix\kapreolo\karten\stadlerberg\2017\Bahnen\",
        FrontBackground = @"C:\daten\felix\kapreolo\karten\stadlerberg\2017\Bahnen\_Staffel.Front.pdf",
        RueckBackgroud = @"C:\daten\felix\kapreolo\karten\stadlerberg\2017\Bahnen\_Staffel.Rueck.pdf",
        OutRoot = @"C:\daten\felix\kapreolo\karten\stadlerberg\2017\Druck\Staffel"
      };

      int maxPages = 80;

      FuenferJoinPdfs(settings, maxPages);
      string pdfRoot = @"C:\daten\felix\kapreolo\karten\stadlerberg\2017\Bahnen";

      {
        string exp = string.Format("{0}_4a.pdf", settings.OutRoot);
        PdfText.OverprintFrontBack(exp, new string[] {
          Path.Combine(pdfRoot, "Strecke4a.pdf"),
          Path.Combine(pdfRoot, "Rueck.4a.pdf")},
          settings.FrontBackground, settings.RueckBackgroud);
      }
      {
        string exp = string.Format("{0}_4b.pdf", settings.OutRoot);
        PdfText.OverprintFrontBack(exp, new string[] {
          Path.Combine(pdfRoot, "Strecke4b.pdf"),
          Path.Combine(pdfRoot, "Rueck.4b.pdf")},
          settings.FrontBackground, settings.RueckBackgroud);
      }
      {
        string exp = string.Format("{0}_4c.pdf", settings.OutRoot);
        PdfText.OverprintFrontBack(exp, new string[] {
          Path.Combine(pdfRoot, "Strecke4c.pdf"),
          Path.Combine(pdfRoot, "Rueck.4c.pdf")},
          settings.FrontBackground, settings.RueckBackgroud);
      }

      return;

      //List<int[]> pairs = new List<int[]> { new[] { 201, 210 }, new[] { 211, 220 }, new[] { 221, 230 } };
      //foreach (var pair in pairs)
      //{
      //  List<string> files = new List<string>();
      //  for (int n = pair[0]; n <= pair[1]; n++)
      //  {
      //    files.Add(Path.Combine(pdfRoot, string.Format("Kids.Front_1.pdf", n)));
      //    files.Add(Path.Combine(pdfRoot, string.Format("Rueck.Kids-Staffel.{0}.1.pdf", n)));
      //    files.Add(Path.Combine(pdfRoot, string.Format("Kids.Front_2.pdf", n)));
      //    files.Add(Path.Combine(pdfRoot, string.Format("Rueck.Kids-Staffel.{0}.2.pdf", n)));
      //    files.Add(Path.Combine(pdfRoot, string.Format("Kids.Front_3.pdf", n)));
      //    files.Add(Path.Combine(pdfRoot, string.Format("Rueck.Kids-Staffel.{0}.3.pdf", n)));
      //  }
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = Path.Combine(pdfRoot, "Druck", string.Format("Kids_{0}_{1}.pdf", pair[0], pair[1]));
      //  PdfText.OverprintFrontBack(exp, files,
      //    settings.FrontBackground, settings.RueckBackgroud);
      //}
      //return;
      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Kids_2.pdf", root);
      //  PdfText.OverprintFrontBack(exp, new string[] {
      //    Path.Combine(root, "Kids.2.Front.pdf"),
      //    Path.Combine(root, "Kids.2.Rueck.pdf")},
      //    settings.FrontBackground, settings.RueckBackgroud);
      //}
      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Kids_3.pdf", root);
      //  PdfText.OverprintFrontBack(exp, new string[] {
      //    Path.Combine(root, "Kids.3.Front.pdf"),
      //    Path.Combine(root, "Kids.3.Rueck.pdf")},
      //    settings.FrontBackground, settings.RueckBackgroud);
      //}

      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Posten.pdf", root);
      //  PdfText.Overprint(null, new string[] {
      //    settings.FrontBackground,
      //    Path.Combine(root, "PostenNetzFront.pdf")
      //    }, exp);
      //}

      //{
      //  string root = Path.GetDirectoryName(pdfRoot);
      //  string exp = string.Format("{0}\\Comb_Hardwald.pdf", root);
      //  PdfText.Overprint(null, new string[] { settings.FrontBackground }, exp);
      //}
    }

    private void CreatePdfs2015()
    {
      string dir = @"C:\daten\felix\kapreolo\karten\chomberg_bruetten\2015\Bahnen";
      List<string> files = new List<string>();
      foreach (var file in Directory.GetFiles(dir))
      {
        string fileName = Path.GetFileName(file);
        if (!fileName.StartsWith("Front.Staffel") &&
          !fileName.StartsWith("Rueck.Staffel"))
        { continue; }

        if (!fileName.EndsWith(".ocd"))
        { continue; }

        if (fileName.EndsWith("4.ocd"))
        { continue; }

        AdaptBahn2015(file);
        files.Add(file);
      }
      string optScript = Path.Combine(dir, "optScript.xml");
      Ocad.Scripting.Utils.Optimize(files, optScript);

      foreach (var file in files)
      {
        string pdf = Path.ChangeExtension(file, ".pdf");
        if (File.Exists(pdf))
        { File.Delete(pdf); }
      }
      string pdfScript = Path.Combine(dir, "pdfScript.xml");
      Ocad.Scripting.Utils.CreatePdf(pdfScript, files);
    }

    private void CreatePdfs2017()
    {
      string dir = @"C:\daten\felix\kapreolo\karten\stadlerberg\2017\Bahnen";
      List<string> files = new List<string>();
      foreach (var file in Directory.GetFiles(dir))
      {
        string fileName = Path.GetFileName(file);
        if (
          //!fileName.StartsWith("Front.Staffel") &&
          !fileName.StartsWith("Rueck.Staffel"))
        { continue; }

        if (!fileName.EndsWith(".ocd"))
        { continue; }

        if (fileName.EndsWith(".4.ocd"))
        { continue; }

        AdaptBahn2017(file);
        files.Add(file);
      }
      string optScript = Path.Combine(dir, "optScript.xml");
      Ocad.Scripting.Utils.Optimize(files, optScript);

      foreach (var file in files)
      {
        string pdf = Path.ChangeExtension(file, ".pdf");
        if (File.Exists(pdf))
        { File.Delete(pdf); }
      }
      string pdfScript = Path.Combine(dir, "pdfScript.xml");
      Ocad.Scripting.Utils.CreatePdf(pdfScript, files);
    }

    private void AdaptBahn2015(string fileName)
    {
      using (OcadWriter w = OcadWriter.AppendTo(fileName))
      {
        Setup setup = w.ReadSetup();
        IList<ElementIndex> indices = w.GetIndices();

        ElementIndex delIndex = null;
        ElementIndex controlIndex = null;
        GeoElement replaceElem = null;
        foreach (var e in indices)
        {
          w.ReadElement(e, out GeoElement elem);
          if (elem == null)
          { continue; }

          if (elem.Geometry is Polyline line && line.Points.Count == 2)
          {
            line = line.Project(setup.Map2Prj);
            IPoint s = line.Points.First.Value;
            IPoint t = line.Points.Last.Value;
            if (Math.Abs(s.X - 693505.3) < 1 && Math.Abs(s.Y - 260302.3) < 1 &&
              Math.Abs(t.X - 693058.2) < 1 && Math.Abs(t.Y - 260252.2) < 1)
            {
              delIndex = e;
              replaceElem = elem;
            }
          }
          Point x = elem.Geometry as Point;
          if (x == null && elem.Geometry is PointCollection)
          { x = ((PointCollection)(elem.Geometry))[0] as Point; }
          if (x != null)
          {
            x = x.Project(setup.Map2Prj);
            if (Math.Abs(x.X - 693421.7) < 1 && Math.Abs(x.Y - 260274.1) < 1)
            { controlIndex = e; }
          }
        }
        if (controlIndex != null && delIndex != null)
        {
          w.DeleteElements(delegate (ElementIndex i)
          { return i.Index == delIndex.Index; });

          Polyline replace = Polyline.Create(new Point[]
          { new Point2D(693503.4,  260320.9),
            new Point2D(693431.4,  260347.1),
            new Point2D(693058.2,  260252.2) });
          replaceElem.Geometry = replace;

          w.Append(replaceElem);
        }
      }
    }

    private void AdaptBahn2017(string fileName)
    {
      if (!fileName.Contains(".2.ocd") && !fileName.Contains(".5.ocd"))
      { return; }

      using (OcadWriter w = OcadWriter.AppendTo(fileName))
      {
        Setup setup = w.ReadSetup();
        IList<ElementIndex> indices = w.GetIndices();

        ElementIndex delIndex = null;
        GeoElement replaceElem = null;
        foreach (var e in indices)
        {
          w.ReadElement(e, out GeoElement elem);
          if (elem == null)
          { continue; }

          if (elem.Text == null)
          { continue; }

          if (!elem.Text.Contains("-138"))
          { continue; }

          delIndex = e;
          replaceElem = elem;
        }
        if (delIndex != null)
        {
          w.DeleteElements((i) => { return i.Index == delIndex.Index; });

          PointCollection replace = new PointCollection
          {
            new Point2D(2675554.3, 1266229.6),
            new Point2D(2675554.3, 1266217.6),
            new Point2D(2675726.4, 1266217.6),
            new Point2D(2675726.4, 1266287.6),
            new Point2D(2675554.3, 1266287.6)
          };
          replaceElem.Geometry = replace;

          w.Append(replaceElem);
        }
      }
    }

    private void FuenferJoinPdfs(FuenferSettings settings, int maxPages)
    {
      string pdfRoot = settings.PdfRoot;

      IList<string> pdfs = Directory.GetFiles(Path.GetDirectoryName(pdfRoot),
                                             Path.GetFileName(pdfRoot) + "*.pdf");

      Dictionary<double, FrontRueck> legs = new Dictionary<double, FrontRueck>();

      foreach (var pdf in pdfs)
      {
        string[] parts = pdf.Split('.');

        if (parts.Length < 3)
        { continue; }
        if (!int.TryParse(parts[parts.Length - 3], out int nr))
        { continue; }

        if (!int.TryParse(parts[parts.Length - 2], out int leg))
        { continue; }

        double id = nr + 0.1 * leg;
        if (!legs.TryGetValue(id, out FrontRueck frontRueck))
        {
          frontRueck = new FrontRueck();
          legs.Add(id, frontRueck);
        }

        if (pdf.ToLower().Contains("front"))
        { frontRueck.Front = pdf; }
        if (pdf.ToLower().Contains("rueck"))
        { frontRueck.Rueck = pdf; }
      }

      List<double> sorted = new List<double>(legs.Keys);
      sorted.Sort();

      List<string> combs = new List<string>(maxPages);
      int startNr = -1;
      int lastNr = 0;
      foreach (var key in sorted)
      {
        if (combs.Count >= maxPages)
        {
          string exp = string.Format("{0}_{1}_{2}.pdf", settings.OutRoot, startNr, lastNr);
          PdfText.OverprintFrontBack(exp, combs, settings.FrontBackground, settings.RueckBackgroud);
          combs.Clear();
          startNr = -1;
        }

        lastNr = (int)key;
        if (startNr < 0)
        { startNr = lastNr; }

        FrontRueck parts = legs[key];
        combs.Add(parts.Front);
        combs.Add(parts.Rueck);
      }
      if (combs.Count > 0)
      {
        string exp = string.Format("{0}_{1}_{2}.pdf", settings.OutRoot, startNr, lastNr);
        PdfText.OverprintFrontBack(exp, combs, settings.FrontBackground, settings.RueckBackgroud);
      }
    }

  }
}
