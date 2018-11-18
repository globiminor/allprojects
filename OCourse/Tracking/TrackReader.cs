
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
    private const string _trkSeg = "trkseg";

    private const string _trkpt = "trkpt";
    private const string _lat = "lat";
    private const string _lon = "lon";
    private const string _ele = "ele";
    private const string _time = "time";

    public static IList<TrackPoint> ReadData(string file)
    {
      DataSet ds = new DataSet();
      ds.ReadXml(file);

      if (ds.Tables.Contains(_trkpt) == false)
      {
        return null;
      }

      DataTable tbl = ds.Tables[_trkpt];

      List<TrackPoint> pntList = new List<TrackPoint>();
      foreach (DataRow row in tbl.Rows)
      {
        TrackPoint pnt = new TrackPoint
        {
          Lat = double.Parse(row[_lat].ToString()),
          Long = double.Parse(row[_lon].ToString()),
          H = double.Parse(row[_ele].ToString())
        };

        string t = row[_time].ToString();
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
        pnt.Puls = 0;

        pntList.Add(pnt);
      }
      return pntList;
    }

    public static void Append(XmlNode trkseg, Point xyz, DateTime t)
    {
      double la = xyz.Y * 180.0 / Math.PI;
      double lo = xyz.X * 180.0 / Math.PI;

      XmlNode trkpt = trkseg.OwnerDocument.CreateElement(_trkpt);

      XmlAttribute lat = trkseg.OwnerDocument.CreateAttribute(_lat);
      lat.Value = la.ToString("N7");
      trkpt.Attributes.Append(lat);

      XmlAttribute lon = trkseg.OwnerDocument.CreateAttribute(_lon);
      lon.Value = lo.ToString("N7");
      trkpt.Attributes.Append(lon);

      XmlElement ele = trkseg.OwnerDocument.CreateElement(_ele);
      {
        XmlText txt = trkseg.OwnerDocument.CreateTextNode(xyz.Z.ToString("N7"));
        ele.AppendChild(txt);
      }
      trkpt.AppendChild(ele);

      XmlElement time = trkseg.OwnerDocument.CreateElement(_time);
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

      XmlElement trkseg = doc.CreateElement(_trkSeg);
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
