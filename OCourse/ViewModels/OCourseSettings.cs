
using Grid.Lcp;
using System.Xml.Serialization;

namespace OCourse.ViewModels
{
  public class OCourseSettings
  {
    [XmlAttribute]
    public string CourseFile { get; set; }
    [XmlAttribute]
    public string HeightFile { get; set; }
    [XmlAttribute]
    public string VeloFile { get; set; }
    [XmlIgnore]
    public double? Resolution { get; set; }
    [XmlAttribute("Resolution")]
    public string SResol
    {
      get { return Resolution?.ToString(); }
      set { Resolution = double.TryParse(value, out double r) ? r : (double?)null; }
    }
  }
}