﻿
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
      //string address = "https://www.ngdc.noaa.gov/geomag-web/calculators/calculateDeclination?browserRequest=true&magneticComponent=d&lat1=48&lat1Hemisphere=N&lon1=7&lon1Hemisphere=E&model=WMM&startYear=2018&startMonth=12&startDay=14&resultFormat=xml";
      //address = "https://www.ngdc.noaa.gov/geomag-web/calculators/calculateDeclination?lat1=40&lon1=-105.25&resultFormat=xml ";
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

      System.Text.StringBuilder request = new System.Text.StringBuilder();
      foreach (var pair in values)
      {
        request.Append($"{pair}={Uri.EscapeDataString(values[pair.ToString()])}&");
      }

      string escapedRequest = request.ToString().Trim('&');
      string url = $"{address}?{escapedRequest}";

      try
      {
        byte[] result;
        using (WebClient w = new WebClient())
        {
          result = w.DownloadData(url);  // needs .net-Framework >= 4.6.2, otherwise SSL/TLS-error
          //result = w.UploadValues(address, values);
        }

        string xml = System.Text.Encoding.Default.GetString(result);

        using (TextReader r = new StringReader(xml))
        { 
          Basics.Serializer.Deserialize(out XmlGeomag xmlGeomag, r);
          Declination = xmlGeomag.Result.Declination.Value;
        }
      }
      catch { }
    }
  }
}
