using System;
using System.Collections.Generic;
using System.Xml;
using Basics.Geom;
using Ocad;
using Grid.Lcp;
using Ocad.StringParams;

namespace OCourse
{
  public static class Utils
  {
    public static List<string> GetCourseList(string courseFile)
    {
      List<string> courseList;
      using (OcadReader courseReader = OcadReader.Open(courseFile))
      {
        IList<StringParamIndex> idxList = courseReader.ReadStringParamIndices();

        courseList = GetCourseList(courseReader, idxList);
      }
      return courseList;
    }

    public static List<string> GetCourseList(OcadReader courseReader, IList<StringParamIndex> idxList)
    {
      List<string> courseList = new List<string>();
      foreach (var index in idxList)
      {
        if (index.Type == StringType.Course)
        {
          string course = courseReader.ReadCourseName(index);
          courseList.Add(course);
        }
      }
      return courseList;
    }

    public static Config GetConfig(OcadReader courseReader, IList<StringParamIndex> idxList)
    {
      foreach (var index in idxList)
      {
        if (index.Type == StringType.MapNotes)
        {
          string notes = courseReader.ReadStringParam(index);
          return GetConfig(notes);
        }
      }
      return null;
    }

    private static Config GetConfig(string notes)
    {
      Config config = new Config();

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(notes);
      XmlNode ocourseNode = doc.SelectSingleNode("//OCourse");
      if (ocourseNode == null)
      { return config; }
      if (ocourseNode.SelectSingleNode("./Dhm") is XmlElement dhmNode)
      {
        XmlAttribute attr = dhmNode.Attributes["name"];
        if (attr != null) config.Dhm = attr.Value;
      }
      if (ocourseNode.SelectSingleNode("./Velocity") is XmlElement veloNode)
      {
        XmlAttribute attr = veloNode.Attributes["name"];
        if (attr != null) config.Velo = attr.Value;
      }

      if (ocourseNode.SelectSingleNode("./Step") is XmlElement stepNode)
      {
        XmlAttribute attr = stepNode.Attributes["size"];
        if (attr != null) config.Resolution = double.Parse(attr.Value);
      }

      if (ocourseNode.SelectSingleNode("./Routes") is XmlElement routesNode)
      {
        XmlAttribute attr = routesNode.Attributes["name"];
        if (attr != null) config.Routes = attr.Value;
      }

      return config;
    }


    public static double GetLength(Control from, Control to, Setup setup)
    {
      double length = 0;
      IGeometry toGeom = to.Element.Geometry;
      if (setup != null)
      { toGeom = toGeom.Project(setup.Map2Prj); }

      Polyline line = toGeom as Polyline;
      if (line != null)
      {
        length += line.Length();
      }
      if (from == null)
      {
        return length;
      }

      IGeometry fromGeom = from.Element.Geometry;
      if (setup != null)
      { fromGeom = fromGeom.Project(setup.Map2Prj); }

      if (!(fromGeom is IPoint fromPoint))
      {
        fromPoint = ((Polyline)fromGeom).Points.Last.Value;
      }

      IPoint toPoint;
      if (line != null)
      {
        toPoint = line.Points.First.Value;
      }
      else
      {
        toPoint = (IPoint)toGeom;
      }

      length += Math.Sqrt(Point2D.Dist2(fromPoint, toPoint));
      return length;
    }
  }
}
