
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OMapScratch;

namespace OcadTest
{
  [TestClass]
  public class OscratchTest
  {
    [TestMethod]
    public void TestWorld()
    {
      Map map = new Map();

      Pnt pIn = new Pnt(10, 100);
      map.SynchLocation(pIn, 47.0, 8.5, 100, 0.5);
      MapVm mapVm = new MapVm(map);

      Pnt pOut = mapVm.SetCurrentLocation(47.0, 8.5, 100, 0.5);

      Assert.AreEqual(pIn.X, pOut.X);
      Assert.AreEqual(pIn.Y, pOut.Y);
    }

    [TestMethod]
    public void TestInverse()
    {
      double[] mat = new double[] { 2, 7, 3, -2, 600000, 200000 };
      MatrixPrj prj = new MatrixPrj(mat);
      MatrixPrj inv = prj.GetInverse();

      {
        Pnt o = new Pnt(7, 3);
        Pnt p = prj.Project(o);
        Pnt i = inv.Project(p);
        Assert.AreEqual(o.X, i.X);
        Assert.AreEqual(o.Y, i.Y);
      }
    }

  }
}
