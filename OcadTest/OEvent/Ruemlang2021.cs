using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Cmd.Commands;
using OCourse.Commands;
using System.Collections.Generic;
using System.IO;

namespace OcadTest.OEvent
{
  [TestClass]
  public class Ruemlang2021
  {
    [TestMethod]
    public void TestLayout()
    {
      CourseMap cm = new CourseMap();
      cm.InitFromFile(@"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.1.ocd");
      CmdCourseVerifyLayout cmd = new CmdCourseVerifyLayout(cm);
      cmd.Execute();
    }

    [TestMethod]
    public void TestPlaceNrs()
    {
      //string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.2.ocd";
      string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_10k_A4_V.Bahn2.86.2.ocd";
      CourseMap cm = new CourseMap();
      cm.InitFromFile(mapFile);
      string tmp = @"C:\daten\temp\bahn.ocd";
      File.Copy(mapFile, tmp, overwrite: true);

      using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(tmp))
      {
        CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
        cmd.Execute();
      }
    }

    [TestMethod]
    public void PrepareCourses()
    {
      string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";
      string tmpl = Path.Combine(root, "Karten", "Bahnen_10k_A4_V.ocd");
      File.Copy(Path.Combine(root, "tmpl_10k_A4_V.ocd"), tmpl, overwrite: true);

      BuildParameters pars = new BuildParameters
      {
        ConfigPath = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\bahnen_1.xml",
        TemplatePath = tmpl,
        OutputPath = tmpl,
        SplitCourses = true
      };

      BuildCommand cmd = new BuildCommand(pars);
      foreach (var cat in new[] {
        new Cat("Bahn1", 50, 69),
        new Cat("Bahn2", 80, 105),
      })
      {
        pars.BeginStartNr = cat.MinStartNr.ToString();
        pars.EndStartNr = cat.MaxStartNr.ToString();
        pars.Course = cat.Course;
        pars.Validate();

        cmd.Execute(out string error);
      }
    }

    [TestMethod]
    public void AdaptCourses()
    {
      string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\Karten";
      foreach (var mapFile in Directory.GetFiles(root))
      {
        IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
        if (parts.Count != 5)
        { continue; }

        CourseMap cm = new CourseMap();
        cm.InitFromFile(mapFile);

        using (System.Drawing.Bitmap bmpFont = new System.Drawing.Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
        using (System.Drawing.Graphics grpFont = System.Drawing.Graphics.FromImage(bmpFont))
        using (System.Drawing.Font font = new System.Drawing.Font(cm.CourseNameFont, 12))
        using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(mapFile))
        {
          CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
          cmd.Execute();

          if (parts[3] == "1")
          {
            AdaptCourseName(w, cm, grpFont, font);
          }
        }
      }
    }

    private void AdaptCourseName(Ocad.OcadWriter w, CourseMap cm, System.Drawing.Graphics grpFont, System.Drawing.Font font)
    {
      if (string.IsNullOrWhiteSpace(cm.CourseNameFont))
      { return; }
      w.ProjectElements((elem) =>
      {
        if (!cm.CourseNameSymbols.Contains(elem.Symbol))
        { return null; }

        string fullCn = elem.Text;
        int idx = fullCn.IndexOf("(");
        if (idx < 0)
        { return null; }
        string coreCn = fullCn.Substring(0, idx - 1);
        float fullW = grpFont.MeasureString(fullCn, font).Width;
        float coreW = grpFont.MeasureString(coreCn, font).Width;

        Ocad.GeoElement.Geom geom = elem.GetMapGeometry();
        Basics.Geom.PointCollection pts = (Basics.Geom.PointCollection)geom.GetGeometry();
        double f = coreW / fullW;
        double fullWidth = pts[2].X - pts[0].X;
        ((Basics.Geom.Point)pts[2]).X = pts[0].X + f * fullWidth;
        ((Basics.Geom.Point)pts[3]).X = pts[2].X;
        elem.SetMapGeometry(Ocad.GeoElement.Geom.Create(pts));
        elem.Text = coreCn;

        return elem;
      });
    }

    private class Cat
    {
      public int MinStartNr;
      public int MaxStartNr;
      public string Course;
      public Cat(string course, int min, int max)
      {
        Course = course;
        MinStartNr = min;
        MaxStartNr = max;
      }
    }
  }
}
