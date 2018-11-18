using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;


namespace Ocad
{
  public class CourseXmlDocument : XmlDocument
  {
    public XmlNode CourseNode(string courseName)
    {
      string name = " " + courseName + " ";
      string search = "/CourseData/Course[CourseName='" + name + "']";
      return SelectSingleNode(search);
    }

    public string CourseId(XmlNode nodeCourse)
    {
      return nodeCourse.SelectSingleNode("CourseId").InnerText;
    }

    public XmlNodeList CourseVariationList(XmlNode nodeCourse)
    {
      return nodeCourse.SelectNodes("CourseVariation");
    }

    public XmlNode CourseVariation(XmlNode nodeCourse, string variationId)
    {
      string search = "CourseVariation[CourseVariationId=' " + variationId + " ']";
      return nodeCourse.SelectSingleNode(search);
    }

    public double CourseLength(XmlNode nodeVariation)
    {
      return double.Parse(nodeVariation.SelectSingleNode("CourseLength").InnerText.Trim());
    }
    public double CourseClimb(XmlNode nodeVariation)
    {
      string sClimb = nodeVariation.SelectSingleNode("CourseClimb").InnerText.Trim();
      if (double.TryParse(sClimb, out double climb) == false)
      { return 0; }
      return climb;
    }

    public string StartPointCode(XmlNode nodeVariation)
    {
      return nodeVariation.SelectSingleNode("StartPointCode").InnerText.Trim();
    }
    public string FinishPointCode(XmlNode nodeVariation)
    {
      return nodeVariation.SelectSingleNode("FinishPointCode").InnerText.Trim();
    }
    public XmlNodeList CourseControls(XmlNode nodeVariation)
    {
      return nodeVariation.SelectNodes("CourseControl");
    }
    public double DistanceToFinish(XmlNode nodeVariation)
    {
      return double.Parse(nodeVariation.SelectSingleNode("DistanceToFinish").InnerText.Trim());
    }
    public string ControlCode(XmlNode nodeControl)
    {
      return nodeControl.SelectSingleNode("ControlCode").InnerText.Trim();
    }
    public double LegLength(XmlNode nodeControl)
    {
      return double.Parse(nodeControl.SelectSingleNode("LegLength").InnerText.Trim());
    }


    public List<string> CourseVariation(XmlNode nodeVariation)
    {
      List<string> sCourse = new List<string> { StartPointCode(nodeVariation)};
      foreach (XmlNode nodeControl in CourseControls(nodeVariation))
      {
        string code = ControlCode(nodeControl);
        sCourse.Add(code);
      }
      sCourse.Add(FinishPointCode(nodeVariation));

      return sCourse;
    }

    public string VariationName(XmlNode variation)
    {
      string name = variation.SelectSingleNode("Name").InnerText.Trim();
      return name;
    }
    public XmlNodeList TeamCombinations(string courseId)
    {
      string search = "/CourseData/TeamCombination[CourseId='" + courseId + "']";
      return SelectNodes(search);
    }

    public string StartNo(XmlNode teamCombination)
    {
      return teamCombination.SelectSingleNode("StartNo").InnerText.Trim();
    }
    public XmlNodeList Legs(XmlNode teamCombination)
    {
      return teamCombination.SelectNodes("Leg");
    }

    public List<string> GetValidStartNumbers(Course course)
    {
      XmlNode nodeCourse = CourseNode(course.Name);
      string courseId = CourseId(nodeCourse);

      return GetValidCombinations(courseId, course);
    }

    public int GetMinimumStartNr(Course course)
    {
      XmlNode nodeCourse = CourseNode(course.Name);
      string courseId = CourseId(nodeCourse);

      return GetMinimumStartNr(courseId);
    }

    public string LegName(XmlNode leg)
    {
      return leg.SelectSingleNode("Name").InnerText.Trim();
    }
    public string LegNo(XmlNode leg)
    {
      return leg.SelectSingleNode("LegNo").InnerText.Trim();
    }

    public string LegVariationId(XmlNode leg)
    {
      return leg.SelectSingleNode("CourseVariationId").InnerText.Trim();
    }


    public List<string> GetValidCombinations(string courseId, Course course)
    {
      List<string> validNrList = new List<string>();

      XmlNode nodeCourse = CourseNode(course.Name);

      List<SimpleSection> allSections = course.GetAllSections();

      int legCount = course.LegCount();

      foreach (XmlNode comb in TeamCombinations(courseId))
      {
        string startNo = StartNo(comb);

        XmlNodeList legList = Legs(comb);
        Trace.Assert(legCount == legList.Count, "Leg Count do not correspond");

        string varKomb = TeamCombinationString(nodeCourse, legList);

        bool valid = Validate(nodeCourse, legList, allSections);

        if (valid)
        {
          validNrList.Add(startNo);
        }
      }
      return validNrList;
    }

    public int GetMinimumStartNr(string courseId)
    {
      int minStartNo = int.MaxValue;
      foreach (XmlNode comb in TeamCombinations(courseId))
      {
        int startNo = int.Parse(StartNo(comb));
        if (startNo < minStartNo)
        { minStartNo = startNo; }
      }

      return minStartNo;
    }

    public void ExportCsvCourses(IList<Course> courses, string csvExport)
    {
      TextWriter csv = new StreamWriter(csvExport);
      string s = "\t";

      csv.WriteLine("Strecke" + s + "Distanz" + s + "Code" + s);

      foreach (Course course in courses)
      {
        XmlNode nodeCourse = CourseNode(course.Name);
        string courseId = CourseId(nodeCourse);
        int iStartNo = GetMinimumStartNr(courseId);

        List<SimpleSection> allSections = course.GetAllSections();

        int legCount = course.LegCount();

        foreach (XmlNode comb in TeamCombinations(courseId))
        {
          XmlNodeList legList = Legs(comb);
          Trace.Assert(legCount == legList.Count, "Leg Count do not correspond");

          string varKomb = TeamCombinationString(nodeCourse, legList);

          bool valid = Validate(nodeCourse, legList, allSections);

          if (valid)
          {
            WriteTeamCsv(csv, nodeCourse, legList, iStartNo, s);

            iStartNo++;
          }

          iStartNo++;
        }
      }

      csv.Close();
    }

    public void ExportCoursesV8(IList<Course> courses, string txtV8Export,
      string dummyPrefix)
    {
      TextWriter writer = new StreamWriter(txtV8Export);
      Dictionary<ISection, Control> dummyControls = null;
      if (string.IsNullOrEmpty(dummyPrefix) == false)
      {
        dummyControls = new Dictionary<ISection, Control>();
      }

      foreach (Course course in courses)
      {
        XmlNode nodeCourse = CourseNode(course.Name);
        string courseId = CourseId(nodeCourse);
        int iStartNo = GetMinimumStartNr(courseId);

        List<SimpleSection> allSections = course.GetAllSections();

        int legCount = course.LegCount();

        foreach (XmlNode comb in TeamCombinations(courseId))
        {
          XmlNodeList legList = Legs(comb);
          Trace.Assert(legCount == legList.Count, "Leg Count do not correspond");

          string varKomb = TeamCombinationString(nodeCourse, legList);

          bool valid = Validate(nodeCourse, legList, allSections);

          if (valid)
          {
            WriteTeamV8(writer, course, nodeCourse, legList, iStartNo,
              dummyControls, dummyPrefix);

            iStartNo++;
          }
        }
      }
      writer.Close();
    }

    private void WriteTeamCsv(TextWriter csv, XmlNode nodeCourse, XmlNodeList legList, int iStartNo, string s)
    {
      StringBuilder line;
      foreach (XmlNode leg in legList)
      {
        string variationId = LegVariationId(leg);
        XmlNode variation = CourseVariation(nodeCourse, variationId);

        line = new StringBuilder();
        line.AppendFormat("{0}.{1}" + s, iStartNo, LegNo(leg));
        line.AppendFormat("{0:N2}" + s, Basics.Utils.Round(CourseLength(variation) / 100.0, 0.1));
        line.Append(LegName(leg) + s);
        foreach (string code in CourseVariation(variation))
        {
          line.Append(code + s);
        }
        csv.WriteLine(line);
      }
    }

    public class ControlDist
    {
      public string Control;
      public double Dist;
      public ControlDist(double dist, string cntr)
      {
        Control = cntr;
        Dist = dist;
      }
    }

    private void WriteTeamV8(TextWriter writer,
      Course course, XmlNode nodeCourse, XmlNodeList legList,
      int iStartNo, Dictionary<ISection, Control> dummies, string dummyPrefix)
    {
      foreach (XmlNode leg in legList)
      {
        string variationId = LegVariationId(leg);
        XmlNode variation = CourseVariation(nodeCourse, variationId);

        string comb = VariationName(variation);
        IList<Control> cntls = course.GetCombinationWithDummies(comb, dummies, dummyPrefix);

        List<ControlDist> xmlCourseList = new List<ControlDist>(cntls.Count)
        { new ControlDist(0, StartPointCode(variation))};
        foreach (XmlNode control in CourseControls(variation))
        {
          xmlCourseList.Add(new ControlDist(LegLength(control), ControlCode(control)));
        }
        xmlCourseList.Add(new ControlDist(DistanceToFinish(variation),
          FinishPointCode(variation)));

        int iXml = 0;
        List<ControlDist> listWithDummies = new List<ControlDist>(cntls.Count);
        for (int i = 0; i < cntls.Count; i++)
        {
          Control cntl = cntls[i];
          if (cntl.Element != null && cntl.Element.Symbol == 705000)
          { continue; }

          if (cntl.Element == null)
          {
            listWithDummies.Add(new ControlDist(0, cntl.Name));
            continue;
          }

          if (cntl.Name != xmlCourseList[iXml].Control)
          {
            string msg = string.Format("Controls do not correspond: {0} {1}", cntl.Name,
                                       xmlCourseList[iXml]);
            throw new InvalidOperationException(msg);
          }

          listWithDummies.Add(xmlCourseList[iXml]);
          iXml++;
        }

        string legNo = LegNo(leg);
        //if (legNo == "4")
        //{ continue; }
        //if (legNo == "5")
        //{ legNo = "4"; }
        string line = GetLineV8(course.Name, string.Format("{0}.{1}", iStartNo, legNo),
          CourseLength(variation), CourseClimb(variation), listWithDummies);

        writer.WriteLine(line);
      }
    }

    public static string GetLineV8(string name, string nr, double lengthM, double climbM, List<ControlDist> controls)
    {
      const string s = ";";
      StringBuilder line = new StringBuilder();
      line.Append(s + name);
      line.AppendFormat(s + "{0}", nr);
      line.AppendFormat(s + "{0:N3}", Basics.Utils.Round(lengthM / 1000.0, 0.1));
      line.AppendFormat(s + "{0:N0}", climbM);

      bool first = true;
      foreach (ControlDist dist in controls)
      {
        if (first == false)
        { line.AppendFormat("{0}{1:N3}", s, dist.Dist / 1000.0); }
        line.AppendFormat("{0}{1}", s, dist.Control);
        first = false;
      }

      return line.ToString();
    }

    private string TeamCombinationString(XmlNode nodeCourse, XmlNodeList legList)
    {
      StringBuilder varKomb = new StringBuilder();
      foreach (XmlNode leg in legList)
      {
        string variationId = LegVariationId(leg);
        XmlNode variation = CourseVariation(nodeCourse, variationId);
        varKomb.AppendFormat("{0};", VariationName(variation));
      }
      return varKomb.ToString();
    }

    private bool Validate(XmlNode nodeCourse, XmlNodeList legList,
      IList<SimpleSection> allSections)
    {
      bool valid = true;

      List<SimpleSection> cloneSections = new List<SimpleSection>(allSections);

      foreach (XmlNode leg in legList)
      {
        string variationId = LegVariationId(leg);
        XmlNode variation = CourseVariation(nodeCourse, variationId);

        string post = StartPointCode(variation);
        foreach (XmlNode control in CourseControls(variation))
        {
          string pre = post;
          post = ControlCode(control);

          valid &= TryRemoveSection(post, cloneSections, pre);
          if (valid == false)
          {
            break;
          }
        }

        if (valid)
        {
          valid &= TryRemoveSection(FinishPointCode(variation), cloneSections, post);
        }
        if (valid == false)
        { break; }
      }

      if (cloneSections.Count != 0)
      {
        valid = false;
      }
      return valid;
    }

    private bool TryRemoveSection(string post, List<SimpleSection> sectionList, string pre)
    {
      foreach (SimpleSection section in sectionList)
      {
        if (section.From.Name == pre && section.To.Name == post)
        {
          sectionList.Remove(section);
          return true;
        }
      }
      return false;
    }

    public bool IsValid(string courseName, string teamName, IList<SimpleSection> allSections)
    {
      XmlNode nodeCourse = CourseNode(courseName);
      string courseId = CourseId(nodeCourse);

      string search = string.Format(
        "/CourseData/TeamCombination[CourseId='{0}'][StartNo=' {1} ']", courseId, teamName);
      //string search = "/CourseData/TeamCombination[CourseId='" + courseId + "']";
      XmlNodeList list = SelectNodes(search);

      if (list.Count != 1)
      { return false; }

      XmlNode teamComb = list[0];

      XmlNodeList legList = Legs(teamComb);

      string varKomb = TeamCombinationString(nodeCourse, legList);

      bool valid = Validate(nodeCourse, legList, allSections);


      return valid;
    }
  }
}
