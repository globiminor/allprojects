using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grid;
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using Ocad.StringParams;
using OCourse.Ext;

namespace OcadTest
{
  [TestClass]
  public class CourseTest
  {
    [TestMethod]
    public void CombChaefer()
    {
      List<DoubleGrid> grs = new List<DoubleGrid>();
      grs.Add(DoubleGrid.FromAsciiFile(@"D:\daten\ASVZ\Daten\Dhm\109112_dtm_grid2_03_02.asc", 0, 0.01, typeof(double)));
      grs.Add(DoubleGrid.FromAsciiFile(@"D:\daten\ASVZ\Daten\Dhm\109114_dtm_grid2_03_02.asc", 0, 0.01, typeof(double)));
      grs.Add(DoubleGrid.FromAsciiFile(@"D:\daten\ASVZ\Daten\Dhm\109121_dtm_grid2_03_02.asc", 0, 0.01, typeof(double)));
      grs.Add(DoubleGrid.FromAsciiFile(@"D:\daten\ASVZ\Daten\Dhm\109123_dtm_grid2_03_02.asc", 0, 0.01, typeof(double)));

      DoubleGrid comb = new DoubleGrid(1500, 1000, typeof(double), 680001, 251999, 2);
      for (int ix = 0; ix < comb.Extent.Nx; ix++)
      {
        for (int iy = 0; iy < comb.Extent.Ny; iy++)
        {
          IPoint p = comb.Extent.CellCenter(ix, iy);
          foreach (DoubleGrid gr in grs)
          {
            int tx, ty;
            bool inside = gr.Extent.GetNearest(p, out tx, out ty);
            if (inside)
            {
              comb[ix, iy] = gr[tx, ty];
              break;
            }
          }
        }
      }
      comb.SaveASCII(@"D:\daten\temp\chaefer.asc", "N2");
    }

    [TestMethod]
    public void CombChaefer1()
    {
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      for (int i = 0; i < 256;i++)
      { r[i] = (byte)i; g[i] = (byte)i; b[i] = (byte)i; }

      string dir = @"D:\daten\felix\kapreolo\karten\chaeferberg\2012\GlattalOL2012\";
      List<DoubleGrid> grs = new List<DoubleGrid>();
      grs.Add(DoubleGrid.FromAsciiFile(dir + "ETH_Hoenggerberg_velo_e.agr", 0, 0.01, typeof(double)));
      grs.Add(DoubleGrid.FromAsciiFile(dir + "Chaeferberg_velo_e.agr", 0, 0.01, typeof(double)));
      ImageGrid.GridToTif((grs[0] * 10).ToIntGrid() % 256, dir + "dhm0.tif", r, g, b);
      ImageGrid.GridToTif((grs[1] * 10).ToIntGrid() % 256, dir + "dhm1.tif", r, g, b);

      DoubleGrid comb = new DoubleGrid(1500, 1000, typeof(double), 680001, 251999, 2);
      for (int ix = 0; ix < comb.Extent.Nx; ix++)
      {
        for (int iy = 0; iy < comb.Extent.Ny; iy++)
        {
          IPoint p = comb.Extent.CellCenter(ix, iy);
          foreach (DoubleGrid gr in grs)
          {
            if (p.X > 681006 && gr == grs[0])
            { continue; }
            int tx, ty;
            bool inside = gr.Extent.GetNearest(p, out tx, out ty);
            if (inside)
            {
              comb[ix, iy] = gr[tx, ty];
              break;
            }
          }
        }
      }
      comb.SaveASCII(dir + "dhm.asc", "N2");
      ImageGrid.GridToTif((comb * 10).ToIntGrid() % 256, dir + "dhm.tif", r, g, b);
    }

    [TestMethod]
    public void CanAnalyzeCourse()
    {
      Course course = new Course("Test");

      course.AddLast(new Control("S1", CoursePar.StartKey));
      course.AddLast(new Control("31", CoursePar.ControlKey));
      course.AddLast(new Control("32", CoursePar.ControlKey));
      course.AddLast(new Control("33", CoursePar.ControlKey));
      course.AddLast(new Control("Z1", CoursePar.FinishKey));

      List<SimpleSection> sects = course.GetAllSections();
      Assert.IsNotNull(sects);
      Assert.AreEqual(4, sects.Count);
    }

    [TestMethod]
    public void CanAnalyzeVariation()
    {
      Course course = new Course("Test");

      course.AddLast(new Control("S1", CoursePar.StartKey));
      Variation var = new Variation();
      Control c31 = new Control("31", CoursePar.ControlKey);
      Control c32 = new Control("32", CoursePar.ControlKey);
      Control c33 = new Control("33", CoursePar.ControlKey);
      Variation.Branch var1 = new Variation.Branch(new int[] { 1 }, new Control[] { c31 });
      Variation.Branch var2 = new Variation.Branch(new int[] { 2 }, new Control[] { c32, c33 });
      var.Branches.Add(var1);
      var.Branches.Add(var2);
      course.AddLast(var);
      course.AddLast(new Control("Z1", CoursePar.FinishKey));

      List<SimpleSection> sects = course.GetAllSections();
      Assert.AreEqual(5, sects.Count);
      Assert.IsFalse(string.IsNullOrEmpty(sects[0].Where));
      Assert.IsTrue(string.IsNullOrEmpty(sects[1].Where));
    }

    [TestMethod]
    public void PerformanceTest()
    {
      Course course;
      using (OcadReader reader = OcadReader.Open(
        @"D:\daten\felix\kapreolo\karten\wangenerwald\2008\5erStaffel2010\5erStaffel2010_test.ocd"))
      {
        course = reader.ReadCourse("Staffel4");
      }
      VariationBuilder b = new VariationBuilder();
      Stopwatch w = new Stopwatch();
      w.Start();
      b.BuildPermutations(course, 200);
      w.Stop();
      Console.WriteLine(w.Elapsed.TotalSeconds);
    }
  }
}
