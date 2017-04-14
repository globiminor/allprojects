using Basics.Geom;
using Basics.Geom.Projection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Asvz
{
  public class KmlConfig
  {
    private bool _includeLookAt;
    private bool _includeMarks;

    public bool IncludeLookAt
    {
      get { return _includeLookAt; }
      set { _includeLookAt = value; }
    }

    public bool IncludeMarks
    {
      get { return _includeMarks; }
      set { _includeMarks = value; }
    }
  }

  public static class XmlUtils
  {
    public static void AppendElement(XmlDocument doc, XmlNode element, string name, string value)
    {
      XmlElement app = doc.CreateElement(name);
      XmlText text = doc.CreateTextNode(value);
      app.AppendChild(text);
      element.AppendChild(app);
    }
  }

  public static class GpxUtils
  {
    public static void Write(string path, Gpx gpx)
    {
      XmlSerializer ser = new XmlSerializer(typeof(Gpx));
      using (TextWriter w = new StreamWriter(path))
      {
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        //ns.Add("ogr", "http://osgeo.org/gdal");
        //xmlns = "http://www.topografix.com/GPX/1/1" xsi:
        // schemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd" >
        ser.Serialize(w, gpx, ns);
      }
    }

    public static TrkSeg GetStreckeGpx(Polyline line, TransferProjection prj, double linearize = 3.0)
    {
      Polyline s = line.Linearize(linearize);
      s = s.Project(prj);

      TrkSeg seg = new TrkSeg { Points = new List<Pt>() };
      foreach (IPoint p in s.Points)
      {
        Pt pt = new Pt { Lat = p.Y, Lon = p.X };
        seg.Points.Add(pt);
      }
      return seg;
    }

    public static TransferProjection GetTransferProjection(Projection dataProjection)
    {
      Projection wgs = new Geographic();
      Ellipsoid ell = new Ellipsoid.Wgs84();
      ell.Datum = new Datum.ITRS();
      ell.Datum.Center.X = -0;
      ell.Datum.Center.Y = 0;
      ell.Datum.Center.Z = 0.0;
      wgs.SetEllipsoid(ell);

      TransferProjection prj = new TransferProjection(dataProjection, wgs);
      return prj;
    }

  }
  public static class KmlUtils
  {
    public static TransferProjection GetTransferProjection(Projection dataProjection)
    {
      TransferProjection prj = new TransferProjection(dataProjection, Geographic.Wgs84());
      return prj;
    }

    public static XmlElement InitDoc(string name)
    {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(
        //        "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
        //        "<kml xmlns=\"http://earth.google.com/kml/2.1\"/>");
        "<kml xmlns=\"http://www.opengis.net/kml/2.2\"/>");
      XmlNode node = doc.LastChild;

      XmlElement dc = doc.CreateElement("Document");
      XmlUtils.AppendElement(doc, dc, "name", name);
      XmlUtils.AppendElement(doc, dc, "open", "1");

      node.AppendChild(dc);
      return dc;
    }
    public static XmlElement GetStyle(XmlDocument doc, string id, string color, int width)
    {
      XmlElement style = doc.CreateElement("Style");
      {
        XmlAttribute attr = doc.CreateAttribute("id");
        attr.Value = id;
        style.Attributes.Append(attr);
      }

      XmlElement lineStyle = doc.CreateElement("LineStyle");
      XmlUtils.AppendElement(doc, lineStyle, "color", color);
      XmlUtils.AppendElement(doc, lineStyle, "width", width.ToString());

      style.AppendChild(lineStyle);
      return style;
    }

    public static void AppendPoint(XmlElement elem, IPoint p, IProjection prj)
    {
      XmlDocument doc = elem.OwnerDocument;

      XmlElement pElem = doc.CreateElement("Point");
      XmlUtils.AppendElement(doc, pElem, "extrude", "1");

      IPoint x = p;
      x = x.Project(prj);
      XmlUtils.AppendElement(doc, pElem, "coordinates", string.Format("{0:F6},{1:F6},0 ", x.X, x.Y));

      elem.AppendChild(pElem);
    }
    public static void AppendLine(XmlElement elem, Polyline line, IProjection prj)
    {
      XmlDocument doc = elem.OwnerDocument;

      Polyline s = line.Linearize(3.0);
      s = s.Project(prj);
      StringBuilder builder = new StringBuilder();
      foreach (Point p in s.Points)
      {
        builder.AppendFormat("{0:F6},{1:F6},0 ", p.X, p.Y);
      }

      XmlElement xmlLine = doc.CreateElement("LineString");
      XmlUtils.AppendElement(doc, xmlLine, "extrude", "1");
      XmlUtils.AppendElement(doc, xmlLine, "tessellate", "1");
      XmlUtils.AppendElement(doc, xmlLine, "coordinates", builder.ToString());

      elem.AppendChild(xmlLine);

    }

  }

  [XmlRoot("gpx")]
  public class Gpx
  {
    [XmlElement("wpt")]
    public List<Pt> WayPoints { get; set; }
    [XmlElement("trk")]
    public Trk Trk { get; set; }
  }
  public class Trk
  {
    [XmlElement("trkseg")]
    public List<TrkSeg> Segments { get; set; }
  }
  public class TrkSeg
  {
    [XmlElement("trkpt")]
    public List<Pt> Points { get; set; }
  }
  public class Pt
  {
    [XmlAttribute("lat")]
    public double Lat { get; set; }
    [XmlAttribute("lon")]
    public double Lon { get; set; }
    [XmlElement("name")]
    public string Name { get; set; }
  }
}
