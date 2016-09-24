using Basics.Geom.Process;
using Grid;
using Grid.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OcadTest
{
  [TestClass]
  public class SchartenhoeheTest
  {
    [TestMethod]
    public void TestSimpleSaddle()
    {
      List<double[]> x = new List<double[]>
      {
        new [] { 1.1, 0.9, 1.1},
        new [] { 1.2, 1.0, 1.2},
        new [] { 1.1, 0.8, 1.0},
      };

      DoubleGrid grd = CreateGrid(x);

      SchartenhoeheInit proc = new SchartenhoeheInit(grd);
      ProcessorContainer container = new ProcessorContainer(new[] { proc }, 1000);
      container.Execute();
    }

    [TestMethod]
    public void TestSynth()
    {
      List<double[]> x = new List<double[]>
      {
        new [] { 1.0, 1.1, 1.2, 1.3},
        new [] { 1.2, 0.5, 0.5, 1.4},
        new [] { 1.3, 1.4, 1.6, 1.5},
      };

      DoubleGrid grd = CreateGrid(x);

      SchartenhoeheInit proc = new SchartenhoeheInit(grd);
      ProcessorContainer container = new ProcessorContainer(new[] { proc }, 1000);
      container.Execute();
    }

    [TestMethod]
    public void TestSynth1()
    {
      List<double[]> x = new List<double[]>
      {
        new [] { 1.0, 1.1, 1.2, 1.3},
        new [] { 1.2, 0.5, 0.5, 1.4},
        new [] { 1.5, 0.5, 0.5, 1.2},
        new [] { 1.3, 1.4, 1.6, 1.5},
      };

      DoubleGrid grd = CreateGrid(x);

      SchartenhoeheInit proc = new SchartenhoeheInit(grd);
      ProcessorContainer container = new ProcessorContainer(new[] { proc }, 1000);
      container.Execute();
    }


    private DoubleGrid CreateGrid<T>(IList<T> data, double x0 = 0, double y0 = 0, double dx = 1)
      where T : IList<double>
    {
      DataDoubleGrid grd = new DataDoubleGrid(data[0].Count, data.Count, typeof(double), x0, y0, dx, 0, 0);
      for (int iy = 0; iy < data.Count; iy++)
      {
        IList<double> xs = data[iy];
        for (int ix = 0; ix < xs.Count; ix++)
        {
          grd[ix, iy] = xs[ix];
        }
      }
      return grd;
    }
    [TestMethod]
    public void TestMm1091()
    {
      DataDoubleGrid grd = DataDoubleGrid.FromAsciiFile(@"C:\daten\ASVZ\Daten\Dhm\mm1091.agr", 0, 0, typeof(double));
      SchartenhoeheInit proc = new SchartenhoeheInit(grd);
      ProcessorContainer container = new ProcessorContainer(new[] { proc }, 1000);
      container.Execute();
    }
  }
}
