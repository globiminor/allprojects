using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ocad;
using Ocad.StringParams;
using OCourse.Route;

namespace OCourse.Ext
{
  public class RelayAdapt
  {
    private readonly string _origOcd;
    private string _origXml;


    private readonly int _symStartNr = 722000;

    private readonly int _symVariant = 722001;

    private readonly int _symPoBeTitle = 17003;

    private readonly int _symPoBeCode = 17002;

    public RelayAdapt()
    {

    }
    public RelayAdapt(string origOcd)
    {
      _origOcd = origOcd;
      _origXml = Path.Combine(Path.GetDirectoryName(origOcd),
                              Path.GetFileNameWithoutExtension(origOcd)) + ".Courses.xml";
    }

    public RelayAdapt(string origOcd, string origXml)
    {
      _origOcd = origOcd;
      _origXml = origXml;
    }
    public string OrigOcad
    {
      get { return _origOcd; }
      set { _origXml = value; }
    }

    public string OrigXml
    {
      get { return _origXml; }
      set { _origXml = value; }
    }

    public void EstimateFromTo()
    {
      CourseInfo info = new CourseInfo(_origOcd);

      string copy = _origOcd + "1.ocd";
      File.Copy(_origOcd, copy, true);
      File.SetAttributes(copy, FileAttributes.Normal);
      Ocad9Writer writer = Ocad9Writer.AppendTo(copy);
      writer.Remove(StringType.Class);
      writer.Remove(StringType.Course);

      int iStart = 1;
      foreach (CoursePar coursePar in info.ParList)
      {
        if (coursePar.NrRunners != null)
        {
          Course course = null;
          foreach (Course t in info.CourseList)
          {
            if (t.Name == coursePar.Name)
            {
              course = t;
              break;
            }
          }
          if (course == null)
          { throw new InvalidOperationException("Course not found: " + coursePar.Name); }
          double f = course.ValidCombinationsFactor();
          double n = ((int)coursePar.NrRunners + 1) / f;

          coursePar.MinStartNr = iStart;
          coursePar.MaxStartNr = coursePar.MinStartNr + (int)n;
          iStart += (int)coursePar.NrRunners;
        }

        writer.Append(StringType.Course, 0, coursePar.StringPar);
      }
      writer.Close();

    }
    public void VerifyFromTo()
    {
      CourseInfo info = new CourseInfo(_origOcd);

      CourseXmlDocument combinationDoc = new CourseXmlDocument();
      combinationDoc.Load(_origXml);

      string copy = _origOcd + "1.ocd";
      File.Copy(_origOcd, copy, true);
      Ocad9Writer writer = Ocad9Writer.AppendTo(copy);
      writer.Remove(StringType.Class);
      writer.Remove(StringType.Course);

      int iStart = 1;
      foreach (CoursePar coursePar in info.ParList)
      {
        if (coursePar.NrRunners != null)
        {
          Course course = null;
          foreach (Course t in info.CourseList)
          {
            if (t.Name == coursePar.Name)
            {
              course = t;
              break;
            }
          }
          IList<string> startNrs = combinationDoc.GetValidStartNumbers(course);
          int maxStartNr = int.Parse(startNrs[(int)coursePar.NrRunners - 1]);

          coursePar.MinStartNr = iStart;
          coursePar.MaxStartNr = maxStartNr;
          iStart += (int)coursePar.NrRunners;
        }

        writer.Append(StringType.Course, 0, coursePar.StringPar);
      }
      writer.Close();

    }

    internal void ExportCoursesTxt(string exportFileName, List<CostFromTo> routeCosts)
    {
      List<Course> courses;
      using (OcadReader reader = OcadReader.Open(_origOcd))
      {
        courses = new List<Course>(reader.ReadCourses());
      }

      TextWriter writer = new StreamWriter(exportFileName);
      try
      {
        foreach (Course course in courses)
        {
          writer.Write(course.ToShortString());

          StringBuilder list = new StringBuilder();
          int nLegs = course.LegCount();
          for (int iLeg = 0; iLeg < nLegs; iLeg++)
          {
            WriteList(list, course, iLeg, nLegs, null, routeCosts);
          }
        }
      }
      finally
      { writer.Close(); }

    }

    private class ForkInfo
    {
      private readonly RelayAdapt _parent;
      private readonly IEnumerator<ISection> _sections;
      private Control _control;
      private IList<PreInfo> _preList;
      private readonly List<CostFromTo> _costInfos;
      private double _cost;

      public ForkInfo(RelayAdapt parent,
        IEnumerator<ISection> sections, IList<PreInfo> preList,
        List<CostFromTo> routeCosts)
      {
        _parent = parent;
        _costInfos = routeCosts;

        _sections = sections;
        _preList = preList;

      }

      public bool MoveNext()
      {
        if (_sections.MoveNext())
        {
          _control = (Control)_sections.Current;
          _cost = _parent.GetCost(_preList, _control, _costInfos);

          _preList = new List<PreInfo>();
          PreInfo pre = new PreInfo(_control, _cost);
          _preList.Add(pre);

          return true;
        }
        else
        { return false; }
      }
      public ISection Current
      { get { return _sections.Current; } }
      public Control LastControl
      { get { return _control; } }

      public double Cost
      { get { return _cost; } }
    }
    private class PreInfo
    {
      public Control Control { get; }
      public double Cost { get; }
      public PreInfo(Control cntr, double cost)
      {
        Control = cntr;
        Cost = cost;
      }
    }
    private double GetCost(IList<PreInfo> preList, Control control, List<CostFromTo> costInfos)
    {
      double cost = 0;
      if (preList != null)
      {
        int nCost = 0;
        foreach (PreInfo pre in preList)
        {
          double addCost = 0;
          if (pre.Control != null)
          {
            CostFromTo route = GetCostInfo(costInfos, pre.Control, control);
            if (route != null)
            { addCost = route.OptimalCost; }
          }
          cost += pre.Cost + addCost;
          nCost++;
        }
        cost = cost / nCost;
      }
      return cost;
    }
    private IList<PreInfo> WriteList(StringBuilder list, SectionCollection sections,
      int leg, int nLegs, IList<PreInfo> preList, List<CostFromTo> routeCosts)
    {
      foreach (ISection section in sections)
      {
        if (section is Control)
        {
          Control c = (Control)section;
          list.Append(c.Name);
          for (int i = 0; i < nLegs; i++)
          { list.Append(";"); }
          double cost = GetCost(preList, c, routeCosts);
          list.Append(cost);
          //for (int i = 0; i < nLegs; i++)
          //{ list.Append(";"); }
          list.AppendLine();

          preList = new List<PreInfo>();
          PreInfo pre = new PreInfo(c, cost);
          preList.Add(pre);
        }
        else if (section is Fork fork)
        {
          List<ForkInfo> branches = new List<ForkInfo>();
          foreach (Fork.Branch branch in fork.Branches)
          {
            branches.Add(new ForkInfo(this, branch.GetEnumerator(), preList, routeCosts));
          }
          bool finished = false;
          while (finished == false)
          {
            finished = true;
            StringBuilder line = new StringBuilder();
            foreach (ForkInfo branch in branches)
            {
              if (branch.MoveNext())
              {
                finished = false;
                Control c = (Control)branch.Current;
                line.AppendFormat("{0};", c.Name);
              }
              else
              { line.Append(";"); }
            }
            if (finished == false)
            {
              for (int i = branches.Count; i < nLegs; i++)
              { line.Append(";"); }
              foreach (ForkInfo branch in branches)
              {
                if (branch.Current != null)
                { line.Append(branch.Cost); }
                line.Append(";");
              }
              list.AppendLine(line.ToString());
            }
          }
          preList = new List<PreInfo>();
          foreach (ForkInfo branch in branches)
          {
            PreInfo pre = new PreInfo(branch.LastControl, branch.Cost);
            preList.Add(pre);
          }
        }
        else if (section is Variation var)
        {
          Variation.Branch branch = var.GetBranch(leg + 1);
          preList = WriteList(list, branch, leg, nLegs, preList, routeCosts);
        }
        else
        {
          throw new NotImplementedException(section.GetType().ToString());
        }
      }
      return preList;
    }

    public void ExportCoursesCsv(string csvExport)
    {
      CourseXmlDocument combinationDoc = new CourseXmlDocument();
      combinationDoc.Load(_origXml);

      List<Course> courses;
      using (OcadReader reader = OcadReader.Open(_origOcd))
      { courses = new List<Course>(reader.ReadCourses()); }

      combinationDoc.ExportCsvCourses(courses, csvExport);
    }

    public void ExportCoursesTxtV8(string txtExport, string dummyPrefix)
    {
      CourseXmlDocument combinationDoc = new CourseXmlDocument();
      combinationDoc.Load(_origXml);

      List<Course> courses;
      using (OcadReader reader = OcadReader.Open(_origOcd))
      { courses = new List<Course>(reader.ReadCourses()); }

      combinationDoc.ExportCoursesV8(courses, txtExport, dummyPrefix);
    }

    internal void AdaptMaps(IList<string> bahnen, string template, string pre,
                            List<CostFromTo> routeCosts)
    {
      List<Course> courses;
      using (OcadReader reader = OcadReader.Open(_origOcd))
      { courses = new List<Course>(reader.ReadCourses()); }

      Dictionary<string, Control> controls = GetControls(courses);
      List<Grafics> grafics = null;
      if (string.IsNullOrEmpty(template) == false)
      {
        using (OcadReader reader = OcadReader.Open(template))
        { grafics = Grafics.GetGrafics(reader, null, controls); }
      }

      CourseXmlDocument combinationDoc = new CourseXmlDocument();
      combinationDoc.Load(_origXml);

      Course secCourse = null;
      IList<SimpleSection> secList = null;
      Dictionary<string, int> teamMapping = null;

      foreach (string bahn in bahnen)
      {
        string inFile = Path.GetFileName(bahn);
        if (inFile.StartsWith(pre))
        { continue; }

        string[] parts = inFile.Split('.');
        int nParts = parts.Length;
        string fileCourseName = parts[nParts - 4];
        string fileTeam = parts[nParts - 3];
        int fileLeg = int.Parse(parts[nParts - 2]);

        List<Control> controlList;
        RelayMapInfo mapInfo;
        using (OcadReader reader = OcadReader.Open(bahn))
        {
          mapInfo = GetCourse(courses, reader);

          if (mapInfo.Error != null)
          { continue; }

          if (mapInfo.Leg != fileLeg) throw new InvalidOperationException("Leg");
          if (mapInfo.TeamName != fileTeam) throw new InvalidOperationException("Team");
          if (mapInfo.Course.Name != fileCourseName) throw new InvalidOperationException("Course");

          if (secCourse != mapInfo.Course)
          {
            secList = mapInfo.Course.GetAllSections();

            teamMapping = new Dictionary<string, int>();
            List<string> validTeams = combinationDoc.GetValidStartNumbers(mapInfo.Course);
            int min = combinationDoc.GetMinimumStartNr(mapInfo.Course);
            foreach (string team in validTeams)
            {
              teamMapping.Add(team, min);
              min++;
            }

            secCourse = mapInfo.Course;
          }

          if (combinationDoc.IsValid(mapInfo.Course.Name, mapInfo.TeamName, secList)
              == false)
          {
            continue;
          }

          if (secCourse == null)
          { throw new InvalidProgramException("secCourse == null"); }
          // verify

          IEnumerator<string> infoControl = mapInfo.Codes.GetEnumerator();
          controlList = secCourse.GetCombination(mapInfo.Combination);
          foreach (Control control in controlList)
          {
            if (control.Element.Symbol != 702000)
            { continue; }

            if (infoControl.MoveNext() == false)
            { throw new InvalidDataException("No Control found in info"); }

            if (infoControl.Current != control.Name)
            {
              throw new InvalidDataException(string.Format("Expected {0}, got {1}",
                                                           control.Name, infoControl.Current));
            }
          }
          if (infoControl.MoveNext())
          { throw new InvalidDataException("Additional Control found in info"); }
        }

        StringBuilder outFile = new StringBuilder(Path.Combine(Path.GetDirectoryName(bahn), pre));
        int mappedTeam = teamMapping[mapInfo.TeamName];
        for (int iPart = 0; iPart < nParts - 4; iPart++)
        {
          outFile.Append(parts[0]);
          outFile.Append(".");
        }
        outFile.Append(fileCourseName);
        outFile.Append(".");
        outFile.AppendFormat("{0:000}", mappedTeam);
        outFile.Append(".");
        outFile.Append(fileLeg);
        outFile.Append(".ocd");

        File.Copy(bahn, outFile.ToString(), true);

        double climb = Climb(controlList, routeCosts);
        using (Ocad9Writer writer = Ocad9Writer.AppendTo(outFile.ToString()))
        {
          AdaptTeamName(writer, mapInfo, mappedTeam, fileLeg);
          int iClimb = (int)Basics.Utils.Round(climb, 5);
          WriteClimb(writer, iClimb);
          if (grafics != null)
          {
            foreach (Grafics grafic in grafics)
            {
              if (grafic.IsApplicable(secCourse, controlList))
              {
                grafic.Apply(writer); // TODO
              }
            }
          }

        }
      }
    }

    private double Climb(List<Control> controls, List<CostFromTo> routeCosts)
    {
      if (routeCosts == null)
      { return 100; }

      double climb = 0;
      Control post = null;
      foreach (Control control in controls)
      {
        Control pre = post;
        post = control;

        if (pre == null)
        { continue; }

        CostFromTo costInfo = GetCostInfo(routeCosts, pre, post);
        if (costInfo != null)
        {
          climb += costInfo.Climb;
        }
      }
      return climb;
    }

    private CostFromTo GetCostInfo(List<CostFromTo> routeCosts, Control pre, Control post)
    {
      if (routeCosts == null)
      { return null; }
      foreach (CostFromTo routeCost in routeCosts)
      {
        if (routeCost.From.Name == pre.Name && routeCost.To.Name == post.Name)
        {
          return routeCost;
        }
      }
      return null;
    }

    private Dictionary<string, Control> GetControls(List<Course> courses)
    {
      Dictionary<string, Control> controls = new Dictionary<string, Control>();
      foreach (Course course in courses)
      {
        foreach (List<Control> list in course.GetExplicitCombinations())
        {
          foreach (Control control in list)
          {
            if (controls.ContainsKey(control.Name) == false)
            {
              controls.Add(control.Name, control);
            }
          }
        }
      }
      return controls;
    }

    private void WriteClimb(OcadWriter writer, int climb)
    {
      AdaptClimb adapt = new AdaptClimb(this, writer);
      writer.DeleteElements(adapt.DeleteClimb);
      if (adapt.Climb == null) return;

      adapt.Climb.Text = string.Format("{0} m", climb);
      writer.Append(adapt.Climb);

    }

    private void AdaptTeamName(OcadWriter writer, RelayMapInfo orig, int newTeam, int leg)
    {
      AdaptTeam adapt = new AdaptTeam(this, writer);
      writer.DeleteElements(adapt.DeleteTeamName);
      if (adapt.StartNr == null) throw new InvalidOperationException("No StartNr");

      adapt.StartNr.Text = string.Format("{0}.{1}", newTeam, leg);
      writer.Append(adapt.StartNr);

      adapt.Combination.Text = string.Format("{0}.{1}", orig.Combination, orig.TeamName);
      writer.Append(adapt.Combination);
    }

    private RelayMapInfo GetCourse(IList<Course> courses, OcadReader reader)
    {
      RelayMapInfo info = new RelayMapInfo();
      foreach (Element element in reader.Elements(null, false, reader.GetIndices()))
      {
        if (element.Symbol == _symStartNr)
        {
          if (info.StartNr == null)
          { info.StartNr = element.Text; }
          else
          {
            info.Error = string.Format("Multiple StartNr : {0}, {1}", info.StartNr, element.Text);
            info.StartNr = null;
            return info;
          }
        }
        if (element.Symbol == _symPoBeTitle)
        {
          string courseName = element.Text;
          foreach (Course course in courses)
          {
            if (course.Name == courseName)
            {
              if (info.Course == null)
              { info.Course = course; }
              else
              {
                info.Error = string.Format("Multiple Course : {0}, {1}",
                  info.Course.Name, courseName);
                info.Course = null;
                return info;
              }
            }
          }
        }
        if (element.Symbol == _symVariant)
        {
          if (info.Combination == null)
          { info.Combination = element.Text; }
          else
          {
            info.Error = string.Format("Multiple Variants : {0}, {1}", info.Combination, element.Text);
            info.Combination = null;
            return info;
          }
        }
        if (element.Symbol == _symPoBeCode)
        {
          info.Codes.Add(element.Text);
        }
      }

      if (info.Course == null)
      { info.Error = "missing course"; }
      else if (info.StartNr == null)
      { info.Error = "missing startNr"; }
      else if (info.Combination == null)
      { info.Error = "missing combination"; }
      else
      {
        string[] parts = info.StartNr.Split('.');
        if (parts.Length != 2)
        {
          info.Error = "Invalid StartNr";
          return info;
        }

        info.TeamName = parts[0];
        if (int.TryParse(parts[1], out int leg) == false)
        {
          info.Error = "Invalid Leg in StartNr";
          return info;
        }
        info.Leg = leg;

        if (info.Combination.StartsWith(parts[1]) == false)
        {
          info.Error = "StartNr and Combination differ";
          return info;
        }
      }

      return info;
    }

    private class RelayMapInfo
    {
      public string StartNr;
      public Course Course;
      public string Combination;
      public readonly List<string> Codes = new List<string>();

      public string TeamName;
      public int Leg;

      public string Error;
    }

    private class CourseInfo
    {
      public readonly List<CoursePar> ParList;
      public readonly List<Course> CourseList;

      public CourseInfo(string path)
      {
        using (OcadReader reader = OcadReader.Open(path))
        {
          ParList = GetCoursePars(reader);
          CourseList = new List<Course>(reader.ReadCourses());
        }

      }

      private List<CoursePar> GetCoursePars(OcadReader reader)
      {
        List<CoursePar> courseList = new List<CoursePar>();
        foreach (StringParamIndex idx in reader.ReadStringParamIndices())
        {
          if (idx.Type == StringType.Course)
          {
            CoursePar coursePar = new CoursePar(reader.ReadStringParam(idx));

            courseList.Add(coursePar);
          }
        }
        return courseList;
      }

    }

    private class AdaptClimb
    {
      private readonly RelayAdapt _parent;
      private readonly OcadWriter _writer;

      public Element Climb;

      public AdaptClimb(RelayAdapt parent, OcadWriter writer)
      {
        _parent = parent;
        _writer = writer;
      }

      public bool DeleteClimb(ElementIndex index)
      {
        if (index.Symbol != _parent._symPoBeTitle)
        { return false; }

        Element elem = _writer.Reader.ReadElement(index);
        if (elem.Text == null || elem.Text.EndsWith(" m") == false)
        { return false; }

        if (Climb != null) { throw new InvalidOperationException("Multiple Climb"); }

        Climb = elem;
        return true;
      }
    }

    private class AdaptTeam
    {
      private readonly RelayAdapt _parent;
      private readonly OcadWriter _writer;

      public Element StartNr;
      public Element Combination;

      public AdaptTeam(RelayAdapt parent, OcadWriter writer)
      {
        _parent = parent;
        _writer = writer;
      }

      public bool DeleteTeamName(ElementIndex index)
      {
        if (index.Symbol == _parent._symStartNr)
        {
          if (StartNr != null)
          { throw new InvalidOperationException("Multiple StartNr"); }
          StartNr = _writer.Reader.ReadElement(index);
        }
        else if (index.Symbol == _parent._symVariant)
        {
          if (Combination != null)
          { throw new InvalidOperationException("Multiple Combination"); }
          Combination = _writer.Reader.ReadElement(index);
        }
        else
        { return false; }
        return true;
      }
    }
  }
}
