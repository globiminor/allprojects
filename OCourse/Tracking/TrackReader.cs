
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using Basics.Geom;

namespace OCourse.Tracking
{
  class TrackPoint
  {
    public double Lat;
    public double Long;
    public double H;
    public DateTime Time;
    public double Puls;
  }

  class GpxIO
  {
    private const string TrkSeg = "trkseg";

    private const string Trkpt = "trkpt";
    private const string Lat = "lat";
    private const string Lon = "lon";
    private const string Ele = "ele";
    private const string Time = "time";

    public static IList<TrackPoint> ReadData(string file)
    {
      DataSet ds = new DataSet();
      ds.ReadXml(file);

      if (ds.Tables.Contains(Trkpt) == false)
      {
        return null;
      }

      DataTable tbl = ds.Tables[Trkpt];

      List<TrackPoint> pntList = new List<TrackPoint>();
      foreach (DataRow row in tbl.Rows)
      {
        TrackPoint pnt = new TrackPoint
        {
          Lat = double.Parse(row[Lat].ToString()),
          Long = double.Parse(row[Lon].ToString()),
          H = double.Parse(row[Ele].ToString())
        };

        string t = row[Time].ToString();
        string[] dateTime = t.Split('T');
        string[] date = dateTime[0].Split('-');
        string[] time = dateTime[1].Split('Z')[0].Split(':');

        int year = int.Parse(date[0]);
        int month = int.Parse(date[1]);
        int day = int.Parse(date[2]);

        int hour = int.Parse(time[0]);
        int min = int.Parse(time[1]);
        double sec = double.Parse(time[2]);

        pnt.Time = new DateTime(year, month, day, hour, min, (int)sec, (int)(sec - (int)sec) * 1000);

        pntList.Add(pnt);
      }
      return pntList;
    }

    public static void Append(XmlNode trkseg, Point xyz, DateTime t)
    {
      double la = xyz.Y * 180.0 / Math.PI;
      double lo = xyz.X * 180.0 / Math.PI;

      XmlNode trkpt = trkseg.OwnerDocument.CreateElement(Trkpt);

      XmlAttribute lat = trkseg.OwnerDocument.CreateAttribute(Lat);
      lat.Value = la.ToString("N7");
      trkpt.Attributes.Append(lat);

      XmlAttribute lon = trkseg.OwnerDocument.CreateAttribute(Lon);
      lon.Value = lo.ToString("N7");
      trkpt.Attributes.Append(lon);

      XmlElement ele = trkseg.OwnerDocument.CreateElement(Ele);
      {
        XmlText txt = trkseg.OwnerDocument.CreateTextNode(xyz.Z.ToString("N7"));
        ele.AppendChild(txt);
      }
      trkpt.AppendChild(ele);

      XmlElement time = trkseg.OwnerDocument.CreateElement(Time);
      {
        XmlText txt = trkseg.OwnerDocument.CreateTextNode(t.ToString("yyyy-MM-ddThh:mm:ssZ"));
        time.AppendChild(txt);
      }
      trkpt.AppendChild(time);

      trkseg.AppendChild(trkpt);
    }

    public static XmlNode CreateTrkSeg()
    {
      XmlDocument doc = new XmlDocument();
      XmlElement gpx = doc.CreateElement("gpx");
      doc.AppendChild(gpx);

      XmlElement trk = doc.CreateElement("trk");
      gpx.AppendChild(trk);

      XmlElement trkseg = doc.CreateElement(TrkSeg);
      trk.AppendChild(trkseg);

      return trkseg;
    }
  }

  class TcxReader
  {
    public static IList<TrackPoint> ReadData(string file)
    {
      string trkpt = "Trackpoint";
      string pos = "Position";

      DataSet ds = new DataSet();
      ds.ReadXml(file);

      if (ds.Tables.Contains(trkpt) == false || ds.Tables.Contains(pos) == false)
      {
        return null;
      }

      DataTable tblTrackpoint = ds.Tables[trkpt];
      DataTable tblPosition = ds.Tables[pos];

      tblTrackpoint.PrimaryKey = new DataColumn[] { tblTrackpoint.Columns["Trackpoint_Id"] };

      List<TrackPoint> pntList = new List<TrackPoint>();
      foreach (DataRow rowPosition in tblPosition.Rows)
      {
        TrackPoint pnt = new TrackPoint
        {
          Lat = double.Parse(rowPosition["LatitudeDegrees"].ToString()),
          Long = double.Parse(rowPosition["LongitudeDegrees"].ToString())
        };
        int trackpointId = int.Parse(rowPosition["Trackpoint_id"].ToString());

        DataRow rowTrack = tblTrackpoint.Rows.Find(trackpointId);
        pnt.H = double.Parse(rowTrack["AltitudeMeters"].ToString());

        string t = rowTrack["Time"].ToString();
        string[] dateTime = t.Split('T');
        string[] date = dateTime[0].Split('-');
        string[] time = dateTime[1].Split('Z')[0].Split(':');

        int year = int.Parse(date[0]);
        int month = int.Parse(date[1]);
        int day = int.Parse(date[2]);

        int hour = int.Parse(time[0]);
        int min = int.Parse(time[1]);
        double sec = double.Parse(time[2]);

        pnt.Time = new DateTime(year, month, day, hour, min, (int)sec, (int)(sec - (int)sec) * 1000);

        pntList.Add(pnt);
      }
      return pntList;
    }
  }
}
