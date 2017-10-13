using Asvz;
using Basics.Geom;
using Basics.Geom.Projection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OcadTest
{
  [TestClass]
  public class AsvzTest
  {
    [TestMethod]
    public void Test9()
    {
      //string path = @"C:\daten\ASVZ\SOLA\2018\Strecke 9.gpx";
      string path = @"C:\daten\ASVZ\temp\Strecke09.xml";
      Gpx gpx;
      using (TextReader r = new StreamReader(path))
      {
        Basics.Serializer.Deserialize(out gpx, r);
      }

      TransferProjection prj = GpxUtils.GetTransferProjection(new Ch1903());
      prj = new TransferProjection(prj.Prj1, prj.Prj0);

      foreach (var seg in gpx.Trk.Segments)
      {
        IPoint pre = null;
        double dist = 0;
        foreach (Pt p in seg.Points)
        {
          IPoint pr = prj.Project(new Point2D(p.Lon, p.Lat));
          dist += System.Math.Sqrt(((Point2D)pre)?.Dist2(pr) ?? 0);
          pre = pr;
        }
      }
    }
  }
}
