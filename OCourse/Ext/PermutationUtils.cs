using Basics.Geom;
using Basics.Geom.Projection;
using Grid;
using Ocad;
using Ocad.StringParams;
using OCourse.Route;
using OCourse.Tracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace OCourse.Ext
{
  public class PermutationUtils
  {
    private class LayoutList : IDisposable
    {
      private class LayoutHelper
      {
        public string Id;
        public string FileName;
        public OcadWriter Writer;
      }

      private LayoutHelper _default;
      private List<LayoutHelper> _list = new List<LayoutHelper>();

      public void Dispose()
      {
        if (_list == null) { return; }
        foreach (var helper in _list)
        {
          if (helper.Writer != null)
          {
            helper.Writer.Close();
          }
        }
        _list.Clear();
        _list = null;
      }

      public void Add(Settings.LayoutTable.Row layout, string name)
      {
        LayoutHelper helper = new LayoutHelper
        {
          Id = layout.Id,
          FileName = name
        };
        if (layout.IsDefaultNull() == false && layout.Default)
        {
          if (_default != null)
          { throw new InvalidOperationException("Default already set"); }
          _default = helper;
        }
        _list.Add(helper);
      }

      public OcadWriter GetWriterFromFullName(string fullName)
      {
        string layoutId = GetLayoutId(fullName);
        LayoutHelper layout = GetLayoutHelper(layoutId);
        if (layout == null)
        { return null; }

        if (layout.Writer == null) layout.Writer = OcadWriter.AppendTo(layout.FileName);

        return layout.Writer;
      }
      private LayoutHelper GetLayoutHelper(string layoutId)
      {
        foreach (var layout in _list)
        {
          if (layout.Id == layoutId)
          {
            return layout;
          }
        }
        if (_default != null)
        {
          return _default;
        }
        if (_list.Count == 0)
        {
          return null;
        }
        return _list[0];
      }
    }

    public static string GetLayoutId(string fullName)
    {
      string[] parts = fullName.Split('(');
      foreach (var part in parts)
      {
        if (part.StartsWith("l_"))
        {
          string layoutId = part.Substring(2).Trim(')');
          return layoutId;
        }
      }
      return null;

    }


    private static void WriteCourseParts(OcadWriter writer, IList<Course> courseParts,
      RouteCalculator routeCalc, double resolution)
    {
      foreach (var comb in courseParts)
      {
        CoursePar coursePar = CoursePar.Create(comb);
        coursePar.Climb = GetClimb(comb, routeCalc, resolution);
        writer.Append(StringType.Course, -1, coursePar.StringPar);
      }
    }

    private static int? GetClimb(Course course,
      RouteCalculator routeCalc, double resolution)
    {
      if (routeCalc == null)
      { return null; }

      List<CostSectionlist> sections =
        routeCalc.CalcCourse(course, resolution); // replace resolution
      int climb = 0;
      foreach (var section in sections)
      {
        climb = (int)Basics.Utils.Round(section.Climb, 5);
        break;
      }
      return climb;
    }

    public static Course GetCourse(string prefix, CostSectionlist comb, bool split)
    {
      Course course = comb.Sections.ToSimpleCourse();

      if (split)
      {
        VariationBuilder.Split(course);
      }

      string name;
      if (string.IsNullOrEmpty(comb.Name))
      { name = prefix; }
      else
      { name = $"{prefix}.{comb.Name}"; }
      course.Name = name;
      course.Climb = Basics.Utils.Round(comb.Climb, 5);

      return course;
    }

    public static void AdaptCourseFiles(string orig, string rawDir, string cleanDir,
      IList<string> courseNames, bool showNr, bool showCodes, string combi)
    {
      if (combi != null)
      {
        File.Copy(orig, combi, true);
        using (OcadWriter writer = OcadWriter.AppendTo(combi))
        {
          writer.SymbolsSetState(null, Ocad.Symbol.SymbolStatus.Hidden);
          AdaptCourseFiles(orig, rawDir, cleanDir, courseNames, writer, showNr, showCodes);
        }
      }
      else
      {
        AdaptCourseFiles(orig, rawDir, cleanDir, courseNames, null, showNr, showCodes);
      }
    }
    private static void AdaptCourseFiles(string orig, string rawDir, string cleanDir,
      IList<string> courseNames, OcadWriter combiWriter, bool showNr, bool showCodes)
    {
      using (OcadReader reader = OcadReader.Open(orig))
      {
        reader.ReadSetup();
        Dictionary<string, IList<Grafics>> customLayouts = GetCustomLayouts(reader);

        foreach (var courseName in courseNames)
        {
          Course course = reader.ReadCourse(courseName);
          IList<Course> parts = null; // PermutationBuilder.CreateDistinctParts(course);
          Dictionary<string, Course> distinctPairs = null;
          if (combiWriter != null)
          {
            distinctPairs = null; // PermutationBuilder.CreateDistinctPairsParts(course);
          }

          string courseFileName = GetFileName(rawDir, "*" + courseName + ".ocd");
          Dictionary<string, IList<GeoElement>> layouts = GetLayouts(courseFileName);

          List<string> cleanNames = new List<string>(parts.Count);
          foreach (var part in parts)
          {
            string partName = GetFileName(rawDir, "*" + part.Name + ".ocd");
            string cleanName = Path.Combine(cleanDir, Path.GetFileName(partName));
            SetLayouts(cleanName, partName, part, layouts, customLayouts, showNr, showCodes);

            if (distinctPairs != null && distinctPairs.ContainsKey(part.Name))
            {
              cleanNames.Add(cleanName);
            }
          }

          if (combiWriter != null)
          {
            cleanNames.Sort();
            foreach (var cleanName in cleanNames)
            {
              TemplatePar tmpl = new TemplatePar(cleanName);
              combiWriter.Append(StringType.Template, -1, tmpl.StringPar);
            }
          }

        }
      }
    }

    public static void CreateCategories(string orig, string cleanDir, string categoryDir,
      IList<string> courseNames, int runnersConst)
    {
      List<Category> categories;
      using (OcadReader reader = OcadReader.Open(orig))
      {
        categories = Category.CreateCategories(reader, courseNames, runnersConst);
        reader.ReadSetup();
      }

      foreach (var cat in categories)
      {
        if (cat.IsSimple())
        {
          continue;
        }
        Course course = cat.Course;
        List<int> varIndexList = cat.VariationIndexList;

        int nTeams = varIndexList.Count;
        int min = cat.MinStartNr;

        for (int i = 0; i < nTeams; i++)
        {
          int startNr = min + i;

          SectionList permut = cat.GetStartNrPermutation(startNr);

          int leg = 1;
          string teamFile = Path.Combine(categoryDir, string.Format("{0}_{1:0000}",
                                                                    course.Name, startNr));

          foreach (var part in permut.Parts)
          {
            string match = string.Format("*.{0}.{1}.ocd", course.Name, part.Name);
            string partFile = GetFileName(cleanDir, match);
            string teamPartFile = teamFile + "_" + leg + ".ocd";
            File.Copy(partFile, teamPartFile, true);

            List<Element> startNrElements = new List<Element>();
            using (OcadReader partReader = OcadReader.Open(teamPartFile))
            {
              foreach (var element in partReader.EnumMapElements(null))
              {
                if (element.Symbol == 17011)
                {
                  startNrElements.Add(element);
                }
              }
            }
            using (OcadWriter writer = OcadWriter.AppendTo(teamPartFile))
            {
              writer.DeleteElements(new int[] { 17011 });
              foreach (var element in startNrElements)
              {
                element.Text = startNr + "." + leg;
                writer.Append(element);
              }
            }
            leg++;
          }
        }
      }
    }

    public static string GetTeamCombinations(List<Category> categories, Setup setup)
    {
      StringBuilder builder = new StringBuilder();
      foreach (var cat in categories)
      {
        string name = GetCoreCourseName(cat.Course.Name);
        if (cat.IsSimple()) // No PermutationUtils
        {
          AppendCourse(builder, name, -1, cat.GetIdxPermutation(0), null, setup, ExportType.Distance);
        }
        else
        {
          int nr = cat.MinStartNr;
          foreach (var varIndex in cat.VariationIndexList)
          {
            SectionList permut = cat.GetIdxPermutation(varIndex);
            AppendCourse(builder, name, nr, permut, null, setup, ExportType.Distance);

            nr++;
          }
        }
      }
      return builder.ToString();
    }

    public static string GetTeamIndices(List<Category> categories)
    {
      StringBuilder builder = new StringBuilder();
      foreach (var cat in categories)
      {
        string name = GetCoreCourseName(cat.Course.Name);
        if (cat.IsSimple()) // No PermutationUtils
        {
          continue;
        }

        List<Course> mainSections = cat.GetMainSections();

        int nr = cat.MinStartNr;
        foreach (var varIndex in cat.VariationIndexList)
        {
          SectionList permut = cat.GetIdxPermutation(varIndex);

          string mainParts = "";
          foreach (var part in permut.Parts)
          {
            mainParts += GetMainSection(part, mainSections);
          }

          builder.AppendFormat("{0};{1,4};{2,4};{3,4}:{4}", name, nr, varIndex, mainParts, permut);
          builder.AppendLine();

          nr++;
        }
      }
      return builder.ToString();
    }

    public static string GetMainSection(Course part, IList<Course> mainSections)
    {
      string partString = part.ToShortString(1);
      foreach (var section in mainSections)
      {
        string sectionString = section.ToShortString(1);
        if (partString.Contains(sectionString))
        {
          return section.Name;
        }
      }
      return null;
    }

    [Flags]
    private enum ExportType { None = 0, Distance = 1, Start = 2, Variation = 4, MainSection = 8, NoControls = 16 }
    private static void AppendCourse(StringBuilder text, string courseName,
      int startNr, SectionList permutation, IList<Course> mainSections, Setup setup, ExportType export)
    {
      double distTotal = 0;
      string controls = "";
      if ((export & ExportType.NoControls) != ExportType.NoControls)
      {
        Course course = new Course(permutation.GetName());
        foreach (var control in permutation.Controls)
        {
          course.AddLast(control);
        }

        controls = GetControlsString(course, setup, export, out distTotal);
      }

      StringBuilder lineBuilder = new StringBuilder();
      if (startNr > 0)
      { lineBuilder.AppendFormat(";{0};{1};", courseName, startNr); }
      else
      { lineBuilder.AppendFormat(";{0};{1};", courseName, 0); }
      if ((export & ExportType.MainSection) == ExportType.MainSection)
      {
        foreach (var part in permutation.Parts)
        {
          string code = GetMainSection(part, mainSections);
          lineBuilder.Append(code);
        }
        lineBuilder.Append(";");
      }
      if ((export & ExportType.Variation) == ExportType.Variation)
      {
        foreach (var part in permutation.Parts)
        {
          lineBuilder.Append(part.Name);
        }
        lineBuilder.Append(";");
      }
      if ((export & ExportType.Distance) == ExportType.Distance)
      { lineBuilder.AppendFormat("{0:N1}00;{1:N0};", distTotal / 1000.0, 0.0); }


      lineBuilder.Append(controls);

      string line = lineBuilder.ToString();
      if ((export & ExportType.Distance) != ExportType.Distance)
      { line = line.Trim(';'); }

      text.AppendLine(line);
    }


    private static string GetControlsString(Course course, Setup setup, ExportType export, out double distTotal)
    {
      StringBuilder courseBuilder = new StringBuilder();
      double dist = 0;
      distTotal = 0;
      Control lastControl = null;
      foreach (var section in course)
      {
        Control control = (Control)section;
        if (courseBuilder.Length == 0)
        {
          courseBuilder.AppendFormat("{0};", control.Name);
          lastControl = control;
        }
        else
        {
          if (control.Element != null)
          {
            dist += Utils.GetLength(lastControl, control, setup);
            lastControl = control;
          }

          if (control.Code != ControlCode.Control && control.Code != ControlCode.Finish)
          {
            if ((export & ExportType.Start) == 0 || control.Code != ControlCode.Start)
            {
              continue;
            }
          }
          if ((export & ExportType.Distance) == ExportType.Distance)
          {
            courseBuilder.AppendFormat("{0:N3};", dist / 1000.0);
          }
          courseBuilder.AppendFormat("{0};", control.Name);
          distTotal += dist;
          dist = 0;
        }
      }
      return courseBuilder.ToString();
    }

    private static Dictionary<string, IList<GeoElement>> GetLayouts(string courseFileName)
    {
      using (OcadReader reader = OcadReader.Open(courseFileName))
      {
        Dictionary<string, IList<GeoElement>> dict = new Dictionary<string, IList<GeoElement>>();
        foreach (var element in reader.EnumGeoElements(null))
        {
          if (element.Symbol == 703000)
          {
            if (dict.TryGetValue(element.Text, out IList<GeoElement> layouts) == false)
            {
              layouts = new List<GeoElement>();
              dict.Add(element.Text, layouts);
            }
            layouts.Add(element);
          }
        }
        return dict;
      }
    }

    public static string GetFileName(string dir, string search)
    {
      IList<string> matches = Directory.GetFiles(dir, search);
      if (matches.Count == 1)
      {
        return matches[0];
      }
      else
      {
        throw new FileLoadException("No or multiple file found in " + dir, search);
      }
    }
    public static void CreateCourseVariations(string orig, string export,
      IList<string> courseNames)
    {
      CreateCourseVariations(orig, export, courseNames, null, -1);
    }

    public static string GetCoreCourseName(string fullCourseName)
    {
      int br = fullCourseName.IndexOf('(');
      if (br < 0)
      { return fullCourseName; }
      string coreName = fullCourseName.Substring(0, br);
      return coreName;
    }

    public static void CreateCourseVariations(string orig, string export,
      IList<string> courseNames, RouteCalculator routeCalc, double resolution)
    {
      Settings settings = Settings.GetSettings(orig);
      using (LayoutList layoutList = new LayoutList())
      {
        if (settings != null)
        {
          foreach (var row in settings.Layout.Rows)
          {
            Settings.LayoutTable.Row layout = (Settings.LayoutTable.Row)row;
            string template = layout.Course;
            string layoutName = TransferCourseSetting(orig, template);
            layoutList.Add(layout, layoutName);
          }
        }

        File.Copy(orig, export, true);

        using (OcadReader reader = OcadReader.Open(orig))
        using (OcadWriter writer = OcadWriter.AppendTo(export))
        {
          List<CoursePar> climbs = new List<CoursePar>();
          foreach (var fullCourseName in courseNames)
          {
            Course course = reader.ReadCourse(fullCourseName);
            if (routeCalc != null)
            {
              string coreName = GetCoreCourseName(fullCourseName);
              CoursePar climb = CoursePar.Create(course);
              climb.Name = coreName + "._height";
              climb.Climb = GetClimb(course, routeCalc, resolution);
              climbs.Add(climb);
            }
            IList<Course> parts = null; // PermutationBuilder.CreateDistinctParts(course);

            WriteCourseParts(writer, parts, routeCalc, resolution);

            OcadWriter layoutWriter = layoutList.GetWriterFromFullName(fullCourseName);
            if (layoutWriter != null)
            { WriteCourseParts(layoutWriter, parts, routeCalc, resolution); }
          }
          foreach (var climb in climbs)
          {
            writer.Append(StringType.Course, -1, climb.StringPar);
          }
        }
      }
    }

    private static string TransferCourseSetting(string orig, string template)
    {
      string tmpl = Path.Combine(Path.GetDirectoryName(orig), template);
      string trans = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(tmpl)),
        Path.GetFileNameWithoutExtension(tmpl) + "." + "_All_" + ".ocd");

      File.Copy(tmpl, trans, true);

      using (OcadWriter writer = OcadWriter.AppendTo(trans))
      {
        writer.DeleteElements(new int[] { 701000, 70200, 705000, 706000, 709000 });

        using (OcadReader reader = OcadReader.Open(orig))
        {
          reader.ReadSetup();

          List<ControlHelper> controls = new List<ControlHelper>();
          foreach (var index in reader.ReadStringParamIndices())
          {
            if (index.Type != StringType.Control)
            { continue; }

            ControlHelper control = new ControlHelper
            {
              ParIndex = index,
              Par = reader.ReadStringParam(index)
            };

            controls.Add(control);
          }
          foreach (var control in controls)
          {
            reader.ReadElement(control.ParIndex.ElemNummer - 1, out GeoElement elem);
            control.ElementIndex = writer.Append(elem);
          }

          foreach (var control in controls)
          {
            writer.Append(StringType.Control, control.ElementIndex + 1, control.Par);
          }


          IList<int> transferSymbols = new int[] { 709000 };
          foreach (var element in reader.EnumMapElements(null))
          {
            if (transferSymbols.Contains(element.Symbol))
            {
              writer.Append(element);
            }
          }
        }

      }
      return trans;
    }
    private class ControlHelper
    {
      public string Par;
      public StringParamIndex ParIndex;
      public int ElementIndex;
    }

    private static void SetLayouts(string cleanName, string fileName, Course course,
      Dictionary<string, IList<GeoElement>> layouts, Dictionary<string, IList<Grafics>> grafics,
      bool showNr, bool showCodes)
    {
      int titleSymbol = 17003;
      int variationSymbol = 17010;

      Dictionary<string, GeoElement> controlNames = new Dictionary<string, GeoElement>();
      Dictionary<ControlCode, GeometryCollection> symbols = new Dictionary<ControlCode, GeometryCollection>();
      List<Element> titleElements = new List<Element>();
      List<Element> variElements = new List<Element>();
      GeoElement startElem = null;

      Setup setup;
      using (OcadReader reader = OcadReader.Open(fileName))
      {
        setup = reader.ReadSetup();
        foreach (var element in reader.EnumGeoElements(null))
        {
          if (element.Symbol == 703000) // Postennummer
          {
            controlNames.Add(element.Text, element);
          }
          else if (element.Symbol == 701000)
          {
            startElem = element;
          }
          else if (element.Symbol == titleSymbol)
          {
            titleElements.Add(element);
          }
          else if (element.Symbol == variationSymbol)
          {
            variElements.Add(element);
          }
        }

        Setup symSetup = reader.ReadSetup();
        symSetup.PrjTrans.X = 0;
        symSetup.PrjTrans.Y = 0;
        foreach (var symbol in reader.ReadSymbols())
        {
          if (!(symbol is Ocad.Symbol.PointSymbol pntSym)) continue;

          if (symbol.Number == 702000) // Postenkreis
          {
            symbols.Add(ControlCode.Control, pntSym.GetSymbolGeometry(symSetup));
          }
          if (symbol.Number == 701000) // Start
          {
            symbols.Add(ControlCode.Start, pntSym.GetSymbolGeometry(symSetup));
          }
        }
      }

      int controlIdx = 0;

      List<Control> controls = new List<Control>(course.Count);

      foreach (var section in course)
      {
        if (!(section is Control control)) continue;

        if (startElem != null &&
          control.Code == ControlCode.Start &&
          PointOp.Dist2(((GeoElement.Point)control.Element.Geometry).BaseGeometry, ((GeoElement.Point)startElem.Geometry).BaseGeometry) < 1)
        {
          control.Element = startElem;
        }

        controls.Add(control);
        if (control.Code != ControlCode.Control) continue;

        controlIdx++;

        if (controlNames.TryGetValue(control.Name, out GeoElement element) == false)
        { continue; }
        if (showNr)
        {
          if (element.Text == control.Name)
          {
            element.Text = "";
            element.Geometry = layouts[control.Name][0].Geometry;
          }
          else
          {
            element.Text = element.Text.Split('-')[0];
            element.Text += "/";
          }
          element.Text += controlIdx;
          if (showCodes)
          {
            element.Text += "-" + control.Name;
          }
        }
        else if (showCodes == false)
        {
          element.Text = "";
        }
      }

      Control pre = null;
      List<Grafics> applicableList = new List<Grafics>();
      foreach (var section in course)
      {
        if (!(section is Control control)) continue;

        if (pre != null)
        {
          string key = pre.Name + "-" + control.Name;
          if (grafics.TryGetValue(key, out IList<Grafics> candidates))
          {
            foreach (var candi in candidates)
            {
              if (candi.IsApplicable(course, controls))
              {
                applicableList.Add(candi);
                break;
              }
            }
          }
        }

        pre = control;
      }

      Polyline clippedStart = ClipStart(controls[0], controls[controls.Count - 2], setup, symbols[ControlCode.Start], symbols[ControlCode.Control]);

      File.Copy(fileName, cleanName, true);
      using (OcadWriter writer = OcadWriter.AppendTo(cleanName))
      {
        writer.DeleteElements(new int[] { 703000, titleSymbol, variationSymbol });
        if (clippedStart != null)
        {
          writer.DeleteElements(new int[] { 701000 });
          Element start = new GeoElement(clippedStart);
          start.Symbol = 704000;
          start.Type = GeomType.line;
          writer.Append(start);
        }
        foreach (var element in controlNames.Values)
        {
          writer.Append(element);
        }
        foreach (var element in titleElements)
        {
          if (element.Text == course.Name)
          {
            element.Text = course.Name.Split('.')[0];
          }
          writer.Append(element);
        }
        foreach (var element in variElements)
        {
          element.Text = course.Name;
          writer.Append(element);
        }

        foreach (var appl in applicableList)
        {
          Control from = FindControl(appl.FromName, controls);
          Control to = FindControl(appl.ToName, controls);
          GeometryCollection fromSymbol = symbols[from.Code];
          GeometryCollection toSymbol = symbols[to.Code];
          Polyline geom = appl.Clip(((GeoElement.Line)appl.Geometry).BaseGeometry, from.Element, fromSymbol, Grafics.ClipParam.AtStart);
          geom = appl.Clip(geom, to.Element, toSymbol, Grafics.ClipParam.AtEnd);
          appl.Apply(writer, new GeoElement.Line(geom));
        }
      }
    }

    private static Polyline ClipStart(Control startControl, Control lastControl, Setup setup,
      GeometryCollection startSymbol, GeometryCollection controlSymbol)
    {
      if (lastControl.Code == ControlCode.Finish)
      { return null; }
      if (lastControl.Code == ControlCode.MarkedRoute)
      { return null; }

      GeoElement startElem = startControl.Element;
      GeometryCollection startColl = Ocad.Utils.SymbolGeometry(startSymbol, ((GeoElement.Point)startElem.Geometry).Project(setup.Map2Prj).BaseGeometry, startElem.Angle);
      Polyline startGeom = (Polyline)startColl[0];
      GeoElement controlElem = lastControl.Element;
      GeometryCollection controlGeom = Ocad.Utils.SymbolGeometry(controlSymbol, ((GeoElement.Point)controlElem.Geometry).Project(setup.Map2Prj).BaseGeometry, 0);

      IEnumerable<ParamGeometryRelation> cuts =
        GeometryOperator.CreateRelations(startGeom, controlGeom, new TrackOperatorProgress());

      if (cuts == null)
      {
        return null;
      }
      IList<Polyline> parts = startGeom.Split(cuts);

      // dummy implementation
      Polyline clipped = parts[2].Clone();
      clipped.RemoveAt(-1);
      foreach (var add in parts[0].Points)
      { clipped.Add(Point.Create(add)); }

      return clipped;
    }

    private static Control FindControl(string name, IList<Control> controls)
    {
      foreach (var control in controls)
      {
        if (name == control.Name)
        {
          return control;
        }
      }
      return null;
    }

    private static Dictionary<string, IList<Grafics>> GetCustomLayouts(OcadReader reader)
    {
      IList<Control> controlList = reader.ReadControls();
      Dictionary<string, Control> controlDict = new Dictionary<string, Control>();
      foreach (var control in controlList)
      {
        controlDict.Add(control.Name, control);
      }

      Dictionary<int, int> graficSymbols = new Dictionary<int, int> { { 704002, 704000 } };

      IList<Grafics> graficList = Grafics.GetGrafics(reader, graficSymbols, controlDict);
      Dictionary<string, IList<Grafics>> graficDict = new Dictionary<string, IList<Grafics>>();
      foreach (var grafics in graficList)
      {
        string key = grafics.Key;
        if (graficDict.TryGetValue(key, out IList<Grafics> lst) == false)
        {
          lst = new List<Grafics>();
          graficDict.Add(key, lst);
        }
        lst.Add(grafics);
      }
      return graficDict;
    }

    public static void ExportV8(string ocadFile, IList<string> courseNames,
      string exportFileName, int runnnersConst)
    {
      List<Category> categories;
      Setup setup;
      Settings settings;
      using (OcadReader reader = OcadReader.Open(ocadFile))
      {
        categories = Category.CreateCategories(reader, courseNames, runnnersConst);
        setup = reader.ReadSetup();
        settings = Settings.GetSettings(reader, null);
        settings.CourseFile = ocadFile;
      }

      string teamCombs = GetTeamCombinations(categories, setup);
      using (TextWriter writer = new StreamWriter(exportFileName, false))
      {
        writer.WriteLine(teamCombs);

        writer.Close();
      }
    }

    public static void CreateRunnerMapLinks(string orig, string reinDir, string olEinzelFile)
    {
      throw new NotImplementedException();
      //List<string> lines = new List<string>();
      //List<CoursePermutations> courses = new List<CoursePermutations>();

      //List<string> courseNames = Utils.GetCourseList(orig);

      //using (OcadReader reader = OcadReader.Open(orig))
      //{
      //  Settings settings = Settings.GetSettings(reader, null);
      //  settings.CourseFile = orig;

      //  foreach (var courseName in courseNames)
      //  {
      //    Course course = reader.ReadCourse(courseName);
      //    string coreName = GetCoreCourseName(courseName);
      //    CoursePermutations permuts = new CoursePermutations(course);

      //    courses.Add(permuts);

      //    if (permuts.Permutations.Count < 2)
      //    { continue; }

      //    using (TextReader olEinzelReader = new StreamReader(olEinzelFile))
      //    {
      //      for (string line = olEinzelReader.ReadLine(); line != null; line = olEinzelReader.ReadLine())
      //      {
      //        if (line.Trim() == "")
      //        { continue; }
      //        string[] parts = line.Split(';');

      //        if (parts[1] != coreName)
      //        {
      //          if (coreName != "hal_h20")
      //          { continue; }

      //          if (parts[1] != "hal" && parts[1] != "h20")
      //          { continue; }
      //        }

      //        string startNr = parts[2];

      //        List<string> controlNames = new List<string>();
      //        for (int i = 5; i < parts.Length; i += 2)
      //        { controlNames.Add(parts[i]); }

      //        PermutationFull permut = permuts.FindPermutation(controlNames);
      //        if (permut == null)
      //        { throw new InvalidOperationException("Not found : " + line); }

      //        int leg = 1;
      //        foreach (var part in permut.Parts)
      //        {
      //          string match = string.Format("*.{0}.{1}.pdf*.lnk", coreName, part.Name);
      //          string partFile = GetFileName(reinDir, match);
      //          string linkName = string.Format("{0}.{1}.{2,3}.{3}.lnk", coreName, leg, startNr, part.Name);
      //          linkName = Path.Combine(reinDir, linkName);
      //          AppShortcutToDesktop(partFile, linkName);
      //          leg++;
      //        }
      //      }
      //      olEinzelReader.Close();
      //    }
      //  }
      //}

      //string allCombs = PermutationUtils.GetAllCombinations(courses);

      //string allParts = PermutationUtils.GetAllDistinctParts(courses);
    }

    private class SectionTime
    {
      public string FromName;
      public string ToName;
      public double FromSecs;
      public double ToSecs;

      public bool IsStart;
      public bool IsPassage;

      public double GetSecs()
      {
        if (ToSecs > 0)
        {
          return ToSecs - FromSecs;
        }
        return ToSecs;
      }

      public override string ToString()
      {
        return string.Format("{0}-{1}: {2} {3}", FromName, ToName, FromSecs, ToSecs);
      }
    }

    public static void CompleteResult(string courseFile, string inResult, string outResult)
    {
      Dictionary<string, Course> courses = new Dictionary<string, Course>();
      using (OcadReader reader = OcadReader.Open(courseFile))
      {
        foreach (var course in reader.ReadCourses())
        {
          courses.Add(GetCoreCourseName(course.Name).ToUpper(), course);
        }
      }


      Encoding enc = Encoding.GetEncoding("iso-8859-1");
      using (StreamReader resultStream = new StreamReader(inResult, enc))
      using (TextWriter resultWriter = new StreamWriter(outResult, false, resultStream.CurrentEncoding))
      {
        TextReader resultReader = resultStream;
        resultWriter.WriteLine(resultReader.ReadLine());
        for (string resultLine = resultReader.ReadLine(); resultLine != null;
          resultLine = resultReader.ReadLine())
        {
          StringBuilder normalLine = new StringBuilder();
          IList<string> parts = resultLine.Split(';');

          string cat = parts[25];

          Course course = courses[cat.ToUpper()];
          if (course == null) throw new InvalidOperationException("Course " + cat + " not found");

          LinkedListNode<ISection> current = course.First;
          string pre = null;
          int nStart = 0;
          while (current != null)
          {
            Control c = (Control)current.Value;
            if (c.Code == ControlCode.Start)
            {
              LinkedListNode<ISection> preNode = current.Previous;
              while (preNode != null && ((Control)preNode.Value).Code != ControlCode.Control)
              {
                preNode = preNode.Previous;
              }
              if (preNode != null)
              {
                pre = ((Control)preNode.Value).Name;
                nStart++;
              }
            }
            current = current.Next;
          }

          int nControl = int.Parse(parts[56]) + nStart;
          parts[56] = nControl.ToString();

          for (int iPart = 0; iPart < 60; iPart++)
          {
            normalLine.AppendFormat("{0};", parts[iPart]);
          }

          for (int iControl = 60; iControl < parts.Count - 1; iControl += 2)
          {
            normalLine.AppendFormat("{0};{1};", parts[iControl], parts[iControl + 1]);
            if (parts[iControl] == pre)
            {
              normalLine.AppendFormat("{0};", ((Control)course.First.Value).Name);
              string time = parts[iControl + 1];

              double secs = GetSecs(time);
              if (secs > 0)
              {
                normalLine.AppendFormat("0;");
              }
              else
              {
                normalLine.AppendFormat("{0};", parts[iControl + 1]);
              }
            }
          }

          resultWriter.WriteLine(normalLine);
        }
      }
    }


    public static void NormalizeResult(string courseFile, string result,
      string starts, string normalized, bool createChoice, RouteCalculator calculator)
    {
      Dictionary<string, Course> courses = new Dictionary<string, Course>();
      using (OcadReader reader = OcadReader.Open(courseFile))
      {
        foreach (var course in reader.ReadCourses())
        {
          courses.Add(GetCoreCourseName(course.Name).ToUpper(), course);
        }
      }

      Course lastCourse = null;
      IList<string> winnerParts = null;
      using (TextWriter resultWriter = new StreamWriter(normalized))
      using (TextReader resultReader = new StreamReader(result))
      {
        resultWriter.WriteLine(resultReader.ReadLine());
        for (string resultLine = resultReader.ReadLine(); resultLine != null;
          resultLine = resultReader.ReadLine())
        {
          StringBuilder normalLine = new StringBuilder();
          IList<string> parts = resultLine.Split(';');

          string startNr = parts[1];
          string cat = parts[25];

          Course course = courses[cat.ToUpper()];
          if (course == null) throw new InvalidOperationException("Course " + cat + " not found");

          if (course != lastCourse)
          {
            if (lastCourse != null && createChoice)
            {
              WriteRouteFindersChoice(resultWriter, lastCourse, calculator, normalized, winnerParts);
            }
            winnerParts = new List<string>(parts);
          }
          lastCourse = course;

          LinkedListNode<ISection> current = course.First;
          string pre = null;
          while (current != null)
          {
            Control c = (Control)current.Value;
            if (c.Code == ControlCode.Start)
            {
              LinkedListNode<ISection> preNode = current.Previous;
              while (preNode != null && ((Control)preNode.Value).Code != ControlCode.Control)
              {
                preNode = preNode.Previous;
              }
              if (preNode != null)
              {
                pre = ((Control)preNode.Value).Name;
              }
            }
            current = current.Next;
          }


          string ctr0 = ((Control)course.First.Value).Name;
          double time0 = 0;
          List<SectionTime> times = new List<SectionTime>();
          for (int iControl = 60; iControl < parts.Count - 1; iControl += 2)
          {
            string control = parts[iControl];
            string time = parts[iControl + 1];

            double secs = GetSecs(time);

            {
              SectionTime t = new SectionTime
              {
                FromName = ctr0,
                ToName = control,
                FromSecs = time0,
                ToSecs = secs
              };

              times.Add(t);
            }

            if (secs > 0)
            {
              time0 = secs;
            }
            ctr0 = control;

            if (ctr0 == pre)
            {
              SectionTime t = new SectionTime
              {
                FromName = ctr0,
                FromSecs = time0,
                ToName = ((Control)course.First.Value).Name
              };
              if (secs >= 0)
              {
                t.ToSecs = secs + 30;
                time0 += 30;
              }
              else
              {
                t.ToSecs = secs;
              }

              times.Add(t);

              ctr0 = t.ToName;
            }
          }

          LinkedList<SectionTime> reduceds = new LinkedList<SectionTime>();
          SectionTime last = null;
          foreach (var section in course)
          {
            Control control = (Control)section;
            if (control.Code == ControlCode.MarkedRoute ||
              control.Code == ControlCode.TextBlock)
            {
              continue;
            }
            if (last != null)
            {
              last.ToName = control.Name;
              if (control.Code == ControlCode.Start)
              {
                last.IsStart = true;
              }
              if (control.Code == ControlCode.TextBlock)
              {
                last.IsPassage = true;
              }
              reduceds.AddLast(last);
            }

            last = new SectionTime { FromName = control.Name };
          }
          reduceds.RemoveLast(); // Ziel

          List<SectionTime> sorted = new List<SectionTime>();
          foreach (var time in reduceds)
          {
            SectionTime sort = FindSections(time, times);
            sorted.Add(sort);
          }

          parts[56] = sorted.Count.ToString();
          winnerParts[56] = parts[56];
          for (int i = 0; i < 60; i++)
          {
            normalLine.AppendFormat("{0};", parts[i]);
          }
          double t0 = 0;
          foreach (var time in sorted)
          {
            if (time.ToSecs >= 0)
            {
              t0 += time.GetSecs();
              string sTime = FormatTime(t0);
              normalLine.AppendFormat("{0};{1};", time.ToName, sTime);
            }
            else if (time.ToSecs == -1)
            {
              normalLine.AppendFormat("{0};-----;", time.ToName);
            }
            else if (time.ToSecs == -2)
            {
              normalLine.AppendFormat("{0};;", time.ToName);
            }
            else if (time.ToSecs == -3)
            {
              normalLine.AppendFormat("{0};0.00;", time.ToName);
            }
            else
            {
              throw new NotImplementedException();
            }
          }
          resultWriter.WriteLine(normalLine);
        }
      }
    }

    private static string FormatTime(double t0)
    {
      int h = (int)t0 / 60 / 60;
      int m = (int)t0 / 60 - h * 60;
      int s = (int)t0 % 60;
      return string.Format("{0}:{1:00}:{2:00}", h, m, s);
    }

    private static void WriteRouteFindersChoice(TextWriter writer,
      Course course, RouteCalculator calculator, string normalized, IList<string> parts)
    {
      double time = GetSecs(parts[13]);

      StringBuilder line = new StringBuilder();

      parts[1] = "";
      parts[3] = "";
      parts[4] = "";
      parts[5] = "Choice";
      parts[6] = "Route Finder's";
      parts[7] = "";
      parts[8] = "";
      parts[14] = "3";
      parts[18] = "999";
      parts[19] = "";
      parts[20] = "-";
      parts[21] = "SUI";

      parts[38] = parts[5];
      parts[39] = parts[6];
      parts[40] = "";
      parts[41] = "";
      parts[42] = "";
      parts[43] = "";
      parts[44] = "";
      parts[45] = "";
      parts[46] = "";
      parts[47] = "";
      parts[57] = "";

      for (int i = 0; i < 60; i++)
      {
        line.AppendFormat("{0};", parts[i]);
      }
      Control to = null;
      double sumCosts = 0;
      List<string> controls = new List<string>();
      List<double> costs = new List<double>();
      List<CostFromTo> tracks = new List<CostFromTo>();
      foreach (var control in course)
      {
        Control from = to;
        to = (Control)control;

        if (from == null)
        {
          continue;
        }
        if (to.Code != ControlCode.Control && to.Code != ControlCode.Start && to.Code != ControlCode.Finish)
        {
          to = from;
          continue;
        }

        bool success = false;
        foreach (var info in calculator.RouteCostDict.Keys)
        {
          if (info.From.Name == from.Name && info.To.Name == to.Name)
          {
            controls.Add(info.To.Name);
            costs.Add(info.OptimalCost);
            tracks.Add(info);
            sumCosts += info.OptimalCost;
            success = true;
            break;
          }
        }
        if (success == false)
        { }
      }
      double costPerSec = sumCosts / (time - 25); // Zieleinlauf
      Point finish = null;
      if (calculator != null)
      {
        Control t = (Control)course.Last.Value;
        foreach (var info in calculator.RouteCostDict.Keys)
        {
          if (info.To.Name == t.Name)
          {
            IPoint f = info.Route.Points.Last();
            finish = new Point3D(f.X, f.Y, 25 * costPerSec);
          }
        }
      }

      double s = GetSecs(parts[11]);
      DateTime start = new DateTime(2011, 7, 3, 10, 0, 0) + new TimeSpan(0, 0, (int)s);

      sumCosts = 0;
      XmlNode trkSeg = GpxIO.CreateTrkSeg();
      for (int iPart = 0; iPart < costs.Count; iPart++)
      {
        if (calculator != null)
        {
          ExportTrack(trkSeg, tracks[iPart], calculator, costPerSec, sumCosts / costPerSec, start);
        }

        double cost = costs[iPart];
        sumCosts += cost;

        line.AppendFormat("{0};{1};", controls[iPart], FormatTime(sumCosts / costPerSec));
      }
      if (finish != null)
      {
        AppendNode(trkSeg, finish, time, start, calculator);
      }
      writer.WriteLine(line);
      if (calculator != null)
      {

        string dir = Path.GetDirectoryName(normalized);
        string name = Path.GetFileNameWithoutExtension(normalized);
        string gpx = string.Format("{0}.{1}.gpx", Path.Combine(dir, name), course.Name);

        using (TextWriter gpxWriter = new StreamWriter(gpx))
        {
          gpxWriter.Write(trkSeg.OwnerDocument.OuterXml);
          gpxWriter.Close();
        }
      }
    }

    private static void ExportTrack(XmlNode trkseg, CostFromTo info, RouteCalculator calc,
      double costPerSec, double time0, DateTime start)
    {
      double dt = 5;
      double iTime = (int)Math.Ceiling((time0 / dt)) * dt;
      double cost = (iTime - time0) * costPerSec;
      if (cost < costPerSec && trkseg.ChildNodes.Count > 0)
      {
        cost += dt * costPerSec;
      }

      IEnumerator<IPoint> points = info.Route.Points.GetEnumerator();

      IPoint p0 = null;
      while (points.MoveNext())
      {
        while (points.Current.Z < cost)
        {
          p0 = points.Current;
          if (!points.MoveNext())
          {
            break;
          }
        }

        Point p1 = Point.CastOrCreate(points.Current);
        if (p1.Z == cost)
        {
          AppendNode(trkseg, p1, cost / costPerSec + time0, start, calc);
        }
        else
        {
          double dCost = p1.Z - p0.Z;
          double d = (cost - p1.Z) / dCost;
          Point dp = p1 + d * (p1 - p0);
          AppendNode(trkseg, dp, cost / costPerSec + time0, start, calc);
        }

        cost += dt * costPerSec;

        p0 = p1;
      }
      {
        IPoint last = info.Route.Points.Last();
        double x = (last.Z - (cost - dt * costPerSec)) / costPerSec;
        if (x < 1)
        {
          XmlNode rm = trkseg.ChildNodes[trkseg.ChildNodes.Count - 1];
          trkseg.RemoveChild(rm);
        }
        double lastTime = last.Z / costPerSec + time0;
        AppendNode(trkseg, Point.CastOrCreate(last), lastTime, start, calc);
      }
    }

    private static void AppendNode(XmlNode trkseg, Point p, double time, DateTime start, RouteCalculator calc)
    {
      Projection prj = new Ch1903();
      double h1 = calc.HeightGrid.Value(p.X, p.Y, EGridInterpolation.bilinear);


      IPoint pPrj = prj.Prj2Gg(p);

      Point3D xyz = new Point3D(pPrj.X, pPrj.Y, h1);
      DateTime t = start + new TimeSpan(0, 0, (int)Math.Round(time));
      GpxIO.Append(trkseg, xyz, t);
    }

    private static SectionTime FindSections(SectionTime time, IList<SectionTime> times)
    {
      foreach (var t in times)
      {
        if (t.ToName == time.ToName && t.FromName == time.FromName)
        {
          return t;
        }
      }
      throw new InvalidOperationException();
    }

    private static double GetSecs(string time)
    {
      if (time == "")
      { return -2; }
      if (time == "-----")
      { return -1; }
      if (time == "0.00")
      { return -3; }
      string[] timeParts = time.Split(':');
      return (int.Parse(timeParts[0]) * 60 + int.Parse(timeParts[1])) * 60 + double.Parse(timeParts[2]);
    }
  }
}
