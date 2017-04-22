
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
  }
}
