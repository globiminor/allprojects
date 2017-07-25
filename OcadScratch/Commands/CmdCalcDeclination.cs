
using Basics;
using OMapScratch;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace OcadScratch.Commands
{
  public class CmdCalcDeclination
  {
    public enum ModelType { IGRF, WMM };

    [XmlRoot("maggridresult")]
    public class XmlGeomag
    {
      [XmlElement("verion")]
      public string Version { get; set; }
      [XmlElement("result")]
      public XmlResult Result { get; set; }
    }

    public class XmlResult
    {
      [XmlElement("date")]
      public double Year { get; set; }
      [XmlElement("latitude")]
      public XmlUnit Latitude { get; set; }
      [XmlElement("longitude")]
      public XmlUnit Longitude { get; set; }
      [XmlElement("elevation")]
      public XmlUnit Elevation { get; set; }
      [XmlElement("declination")]
      public XmlUnit Declination { get; set; }
      [XmlElement("declination_sv")]
      public XmlUnit Decl_Sv { get; set; }
      [XmlElement("declination_uncertainty")]
      public XmlUnit DeclUncertainty { get; set; }
    }
    public class XmlUnit
    {
      [XmlAttribute("units")]
      public string Unit { get; set; }
      [XmlText]
      public double Value { get; set; }
    }

    public CmdCalcDeclination(double lat, double lon)
    {
      Lat = lat;
      Lon = lon;
    }

    public double Lat { get; }
    public double Lon { get; }

    public DateTime? Date { get; set; }
    public ModelType? Model { get; set; }

    public double? Declination { get; private set; }

    public void Execute()
    {
      // see https://www.ngdc.noaa.gov/geomag-web/calculators/declinationHelp

      string address = "https://www.ngdc.noaa.gov/geomag-web/calculators/calculateDeclination";
      NameValueCollection values = new NameValueCollection();
      values.Add("lat1", string.Format(CultureInfo.InvariantCulture, "{0:N4}", Lat));
      values.Add("lon1", string.Format(CultureInfo.InvariantCulture, "{0:N4}", Lon));

      if (Model.HasValue)
      { values.Add("model", $"{Model}"); }

      if (Date.HasValue)
      {
        values.Add("startYear", $"{Date.Value.Year}");
        values.Add("startMonth", $"{Date.Value.Month}");
        values.Add("startDay", $"{Date.Value.Day}");
      }

      values.Add("resultFormat", "xml");

      byte[] result;
      using (WebClient w = new WebClient())
      {
        result = w.UploadValues(address, values);
      }

      string xml = System.Text.Encoding.Default.GetString(result);

      XmlGeomag xmlGeomag;
      using (TextReader r = new StringReader(xml))
      { Basics.Serializer.Deserialize(out xmlGeomag, r); }

      Declination = xmlGeomag.Result.Declination.Value;
    }
  }
}
