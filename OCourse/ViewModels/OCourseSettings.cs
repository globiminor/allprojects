
using Grid.Lcp;
using System.Xml.Serialization;

namespace OCourse.ViewModels
{
  public class OCourseSettings
  {
    [XmlAttribute]
    public string CourseFile { get; set; }
    [XmlAttribute]
    public string DhmFile { get; set; }
    [XmlAttribute]
    public string VeloFile { get; set; }
    [XmlAttribute]
    public double Resolution { get; set; }
  }
}