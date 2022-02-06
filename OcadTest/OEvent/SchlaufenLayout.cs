using Basics;
using Basics.Geom;
using Macro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Cmd.Commands;
using OCourse.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace OcadTest.OEvent
{
  [TestClass]
  public class SchlaufenLayout
  {
    [TestMethod]
    public void TestLayout()
    {
      CourseMap cm = new CourseMap();
      cm.InitFromFile(@"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.1.ocd");
      CmdCourseVerifyLayout cmd = new CmdCourseVerifyLayout(cm);
      cmd.Execute();
    }

    [TestMethod]
    public void TestPlaceNrs()
    {
      //string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.2.ocd";
      string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_10k_A4_V.Bahn2.86.2.ocd";
      CourseMap cm = new CourseMap();
      cm.InitFromFile(mapFile);
      string tmp = @"C:\daten\temp\bahn.ocd";
      File.Copy(mapFile, tmp, overwrite: true);

      using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(tmp))
      {
        CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
        cmd.Execute();
      }
    }

    // private string _root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";
    private string _root = @"C:\daten\felix\kapreolo\karten\wangenerwald\2020\NOM\";
    private string _exportFolder = "Karten";
    private string _allVarsName = @"allVars\allVars.ocd";
    private string _io3File = "NOM.Courses.xml";

    [TestMethod]
    public void T01_PrepareCourses()
    {
      //string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";
      string root = _root;
      string configPath = Path.Combine(root, "NOM.xml");
      string eventName = "NOM 2022";

      string exportFolder = _exportFolder;
      Tmpl a4_15k = new Tmpl("tmpl_15k_A4_V.ocd", "tmpl_15k_A4_H.ocd");
      Tmpl a3_10k = new Tmpl("tmpl_10k_A3_V.ocd", "tmpl_10k_A3_H.ocd");
      Tmpl a4_10k = new Tmpl("tmpl_10k_A4_V.ocd", "tmpl_10k_A4_H.ocd");
      Func<string, string> exportCourseFileFct = (tmpl) => tmpl.Replace("tmpl", "Bahnen");
      string io3File = Path.Combine(root, exportFolder, _io3File);

      List<Conf> confs = new List<Conf>
      {
        new Conf("D14", a4_10k,14),
        new Conf("D16", a4_10k,30),
        new Conf("D18", a3_10k,23),
        new Conf("D20", a3_10k,16),
        new Conf("DE",  a3_10k,23),
        new Conf("DAL", a3_10k,11),
        new Conf("DAM", a4_10k,13),
        new Conf("DAK", a4_10k,20),
        new Conf("DB",  a4_10k,10),
        new Conf("D35", a3_10k,7),
        new Conf("D40", a3_10k,11),
        new Conf("D45", a4_10k,12),
        new Conf("D50", a4_10k,18),
        new Conf("D55", a4_10k,10),
        new Conf("D60", a4_10k,13),
        new Conf("D65", a4_10k,5),
        new Conf("D70", a4_10k,2),
        new Conf("D75", a4_10k,3),

        new Conf("H14", a4_10k,22),
        new Conf("H16", a3_10k,38),
        new Conf("H18", a3_10k,26),
        new Conf("H20", a3_10k,28),
        new Conf("HE",  a3_10k,43),
        new Conf("HAL", a3_10k,21),
        new Conf("HAM", a3_10k,18),
        new Conf("HAK", a4_10k,39),
        new Conf("HB",  a4_10k,10),
        new Conf("H35", a3_10k,17),
        new Conf("H40", a3_10k,12),
        new Conf("H45", a3_10k,24),
        new Conf("H50", a3_10k,35),
        new Conf("H55", a3_10k,39),
        new Conf("H60", a3_10k,21),
        new Conf("H65", a4_10k,13),
        new Conf("H70", a4_10k,23),
        new Conf("H75", a4_10k,9),
        new Conf("H80", a4_10k,9),
      };

      int iStart = 101;
      Dictionary<Tmpl, List<Conf>> tmplDict = new Dictionary<Tmpl, List<Conf>>();
      foreach (var conf in confs)
      {
        conf.StartNr = iStart;
        if (conf.N_Nr <= 0)
        {
          conf.EndNr = iStart + 7;
          iStart += 20;
        }
        else
        {
          conf.EndNr = iStart + conf.N_Nr - 1;
          iStart += conf.N_Nr + 1;
        }
        tmplDict.GetOrCreateValue(conf.Tmpl).Add(conf);
      }

      BuildCommand.Parameters pars = new BuildCommand.Parameters
      {
        ConfigPath = configPath,
        SplitParameters = new OCourse.Ext.SplitBuilder.Parameters()
      };
      BuildCommand buildCmd = new BuildCommand(pars);

      OCourse.Iof3.OCourse2Io3 io3Exporter = null;

      string allVarsPath = Path.Combine(root, exportFolder, _allVarsName);
      Directory.CreateDirectory(Path.GetDirectoryName(allVarsPath));
      File.Copy(Path.Combine(root, a3_10k.Front), allVarsPath, overwrite: true);

      pars.EnsureOCourseVm();
      // copy background files to local folders
      using (var r = Ocad.OcadReader.Open(pars.OCourseVm.CourseFile))
      {
        foreach (var pStrIdx in r.ReadStringParamIndices())
        {
          if (pStrIdx.Type == Ocad.StringParams.StringType.Template)
          {
            string sTemp = r.ReadStringParam(pStrIdx);
            Ocad.StringParams.TemplatePar tplPar = new Ocad.StringParams.TemplatePar(sTemp);
            if (!tplPar.Visible)
            { continue; }
            string tpl = tplPar.Name;
            string courseDir = Path.GetDirectoryName(pars.OCourseVm.CourseFile);
            //if (!Path.IsPathRooted(tpl) ||
            //	Path.GetDirectoryName(tpl).Equals(courseDir, StringComparison.OrdinalIgnoreCase))
            {
              string sourcePath = Path.IsPathRooted(tpl) ? tpl : Path.Combine(courseDir, tpl);
              string sourceName = Path.GetFileName(sourcePath);

              File.Copy(sourcePath, Path.Combine(root, exportFolder, sourceName), overwrite: true);
              File.Copy(sourcePath, Path.Combine(Path.GetDirectoryName(allVarsPath), sourceName), overwrite: true);
            }
          }
        }
      }
      CopyElems(pars.OCourseVm.CourseFile, allVarsPath);

      foreach (var pair in tmplDict)
      {
        Tmpl t = pair.Key;

        Dictionary<string, List<Ocad.Course>> coursesDict
          = new Dictionary<string, List<Ocad.Course>>();
        Dictionary<string, List<Ocad.Course>> variationsDict
          = new Dictionary<string, List<Ocad.Course>>();

        foreach (var c in pair.Value)
        {
          pars.Course = c.Course;

          //iStart = 101;
          iStart = c.StartNr;
          pars.BeginStartNr = iStart.ToString();
          pars.EndStartNr = iStart.ToString();
          pars.SplitParameters.CustomWeight = c.CustomSplitWeight;
          pars.UseAllPermutations = true;
          pars.OutputPath = null;
          string valdError = pars.Validate();
          Assert.IsTrue(string.IsNullOrEmpty(valdError), valdError);

          //pars.EndStartNr = (pars.OCourseVm.PermutEstimate + 100).ToString();
          pars.EndStartNr = c.EndNr.ToString();
          pars.Validate();

          buildCmd.Execute(out string error);

          IEnumerable<OCourse.Route.CostSectionlist> selectedCombs =
            OCourse.Route.CostSectionlist.GetCostSectionLists(pars.OCourseVm.Permutations,
            pars.OCourseVm.RouteCalculator, pars.OCourseVm.LcpConfig.Resolution);

          List<Ocad.Course> selectedCourses = selectedCombs.Select(
            comb => OCourse.Ext.PermutationUtils.GetCourse(c.Course, comb, pars.SplitParameters)).ToList();
          coursesDict.Add(c.Course, selectedCourses);

          pars.SplitParameters.CustomWeight = c.CustomSplitWeight;
          IEnumerable<OCourse.Route.CostSectionlist> allCombs = pars.OCourseVm.Info.OfType<OCourse.Route.CostSectionlist>();
          List<Ocad.Course> allVariations = allCombs.Select(
            comb => OCourse.Ext.PermutationUtils.GetCourse(c.Course, comb, pars.SplitParameters)).ToList();
          variationsDict.Add(c.Course, allVariations);


          io3Exporter = io3Exporter ?? OCourse.Iof3.OCourse2Io3.Init(eventName, pars.OCourseVm.CourseFile);
          io3Exporter.AddCurrentPermutations(pars.OCourseVm);
        }

        // Synchronize Print Extent
        using (Ocad.OcadReader r = Ocad.OcadReader.Open(Path.Combine(root, t.Front)))
        using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(Path.Combine(root, t.Back)))
        {
          foreach (var rStrIdx in r.ReadStringParamIndices())
          {
            if (rStrIdx.Type == Ocad.StringParams.StringType.PrintPar)
            {
              var printPar = new Ocad.StringParams.PrintPar(r.ReadStringParam(rStrIdx));
              foreach (var wStrIdx in w.ReadStringParamIndices())
              {
                if (wStrIdx.Type == Ocad.StringParams.StringType.PrintPar)
                {
                  w.Overwrite(wStrIdx, printPar.StringPar);
                  break;
                }
              }
              break;
            }
          }
        }

        // Export courses of same template
        foreach (var s in new[] { t.Front, t.Back })
        {
          string tmpl = Path.Combine(root, exportFolder, exportCourseFileFct(s));
          File.Copy(Path.Combine(root, s), tmpl, overwrite: true);
          pars.TemplatePath = tmpl;
          pars.OutputPath = tmpl;

          bool first = true;
          foreach (var c in pair.Value)
          {
            pars.Course = c.Course;

            List<Ocad.Course> selectedCourses = coursesDict[c.Course];

            if (first)
            {
              CopyElems(pars.OCourseVm.CourseFile, pars.TemplatePath);
              first = false;
            }

            string courseName = c.Course;

            using (CmdCourseTransfer cmdTrans = new CmdCourseTransfer(pars.OutputPath, pars.TemplatePath ?? pars.OCourseVm.CourseFile, pars.OCourseVm.CourseFile))
            {
              cmdTrans.Export(selectedCourses, courseName);
            }

            //cmd.Execute(out string error);
          }
        }

        foreach (var c in pair.Value)
        {
          List<Ocad.Course> selectedCourses = variationsDict[c.Course];
          string courseName = c.Course;
          using (CmdCourseTransfer cmdTrans = new CmdCourseTransfer(allVarsPath, allVarsPath ?? pars.OCourseVm.CourseFile, pars.OCourseVm.CourseFile))
          {
            cmdTrans.Export(selectedCourses, courseName);
          }
        }
      }
      DropLayout(allVarsPath, includeControlSpecs: true);

      io3Exporter.InitMapInfo();
      io3Exporter.Export(io3File);
    }

    private void DropLayout(string courseFile, bool includeControlSpecs)
    {
      using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(courseFile))
      {
        w.DeleteElements((elemIdx) => { return elemIdx.Symbol < 700000 || elemIdx.Symbol > 1000000; });
        foreach (var strIdx in w.ReadStringParamIndices())
        {
          if (strIdx.Type == Ocad.StringParams.StringType.Template)
          {
            Ocad.StringParams.TemplatePar par = new Ocad.StringParams.TemplatePar(w.ReadStringParam(strIdx));
            par.Visible = false;
            w.Overwrite(strIdx, par.StringPar);
          }
        }

        if (includeControlSpecs)
        {
          IList<int> controlSymbs = new[] { 701000, 703000, 706000 };
          w.AdaptElements((elem) =>
          {
            if (string.IsNullOrWhiteSpace(elem.ObjectString))
            {
              return null;
            }
            if (!controlSymbs.Contains(elem.Symbol))
            { return null; }

            Ocad.StringParams.ControlPar p = new Ocad.StringParams.ControlPar(elem.ObjectString);
            p.ClearSymbols();
            elem.ObjectString = p.StringPar;

            return elem;
          });
        }
      }
    }

    [TestMethod]
    public void T02_ExportCourseMaps()
    {
      string rootFolder = Path.Combine(_root, _exportFolder);
      foreach (var mapFile in Directory.GetFiles(rootFolder))
      {
        IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
        if (parts.Count != 2)
        { continue; }
        if (parts[1] != "ocd")
        { continue; }

        if (!(parts[0].EndsWith("_V") || parts[0].EndsWith("_H")))
        { continue; }

        ExportCourseMaps(mapFile);

        // Drop not needed files
        foreach (var expFile in Directory.GetFiles(rootFolder, $"{parts[0]}.*.ocd"))
        {
          IList<string> ps = Path.GetFileName(expFile).Split(new[] { '.' });
          if (ps.Count != 5 || ps[4] != "ocd")
          { continue; }
          if (ps[0].EndsWith("_V") && ps[3] != "1")
          { File.Delete(expFile); }
          if (ps[0].EndsWith("_H") && ps[3] != "2")
          { File.Delete(expFile); }
        }
      }
    }
    [TestMethod]
    public void T02_Z_ExportAllVars()
    {
      string allVarsPath = Path.Combine(_root, _exportFolder, _allVarsName);
      ExportCourseMaps(allVarsPath);
    }

    private void ExportCourseMaps(string mapFile)
    {
      string rootFolder = Path.GetDirectoryName(mapFile);

      string mapName = Path.GetFileNameWithoutExtension(mapFile);
      string lastCourse = null;
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(mapFile))
      {
        foreach (var course in r.ReadCourses())
        {
          lastCourse = course.Name;
          foreach (var filePath in Directory.GetFiles(rootFolder, $"{mapName}.{lastCourse}.*"))
          {
            if (filePath.EndsWith(".ocd"))
            { File.Delete(filePath); }
          }
        }
      }
      string exe = Utils.FindExecutable(mapFile);
      string script = Path.Combine(rootFolder, "exportCourse.xml");
      using (Ocad.Scripting.Script expPdf = new Ocad.Scripting.Script(script))
      {
        using (Ocad.Scripting.Node node = expPdf.FileOpen())
        { node.File(mapFile); }
      }
      string pName = new Ocad.Scripting.Utils().RunScript(script, exe, wait: 3000);
      IList<Process> processes = Process.GetProcessesByName(pName);

      if (processes.Count < 1)
      { return; }
      Processor m = new Processor();
      m.SetForegroundProcess(processes[0]);
      m.SetForegroundWindow("OCAD", out string fullText);
      System.Threading.Thread.Sleep(2000);

      // Export Course maps...
      m.SendCommand('c', new List<byte> { Ui.VK_ALT });
      m.SendCommand('x');
      m.SendCommand('x');

      System.Threading.Thread.Sleep(1000);

      //IntPtr p = Ui.GetFocus();
      //var root = AutomationElement.FromHandle(processes[0].MainWindowHandle);

      // Click "Select All"
      SetFocusByName("Select all", m);
      m.SendCode(new[] { Ui.VK_RETURN });
      System.Threading.Thread.Sleep(500);

      // Click "Select OK"
      SetFocusByName("OK", m);
      m.SendCode(new[] { Ui.VK_RETURN });
      System.Threading.Thread.Sleep(500);

      // "Save" exported course maps
      m.SendCommand('s', new[] { Ui.VK_ALT });
      System.Threading.Thread.Sleep(500);

      while (!(Directory.GetFiles(rootFolder, $"{mapName}.{lastCourse}.*")?.Length > 0))
      {
        System.Threading.Thread.Sleep(500);
      }
      System.Threading.Thread.Sleep(1000);
      m.SetForegroundWindow("OCAD", out fullText);
      System.Threading.Thread.Sleep(1000);


      // Close map
      m.SendCommand('w', new[] { Ui.VK_CONTROL });
      System.Threading.Thread.Sleep(500);
      // Check for "Save File ...",
      if (AutomationElement.FocusedElement.Current.ControlType.ProgrammaticName == "ControlType.Button")
      {
        // Do not save file
        m.SendCommand('n', new[] { Ui.VK_ALT });
        System.Threading.Thread.Sleep(500);
      }
      var c = AutomationElement.FocusedElement.Current;
    }

    private void SetFocusByName(string name, Processor m)
    {
      var parent = TreeWalker.ContentViewWalker.GetParent(AutomationElement.FocusedElement);
      var toFocus = parent.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, name));
      toFocus?.SetFocus();
      System.Threading.Thread.Sleep(500);
      // Click "Select All"
      while (!AutomationElement.FocusedElement.Current.Name.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
      {
        m.SendCommand('\t');
        System.Threading.Thread.Sleep(500);
      }

    }


    private int SplitHD12(int nDouble, Ocad.Control split, Dictionary<string, List<Ocad.Control>> dict, Dictionary<string, List<Ocad.Control>> dictPre)
    {
      if (!dictPre.ContainsKey("89"))
      { return nDouble + 2; }
      return nDouble;
    }
    private int SplitH45(int nDouble, Ocad.Control split, Dictionary<string, List<Ocad.Control>> dict, Dictionary<string, List<Ocad.Control>> dictPre)
    {
      if (split.Name == "70")
      { return nDouble + 2; }
      return nDouble;
    }

    private IPoint GetCustomPositionDH12(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
    {
      if (cntrText != "8-63")
      { return null; }

      return new Point2D(10986, -4069);
    }
    private IPoint GetCustomPositionDAM(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
    {
      if (cntrText.EndsWith("-38"))
      { return new Point2D(6927 - textWidth, -3968); }

      if (cntrText.EndsWith("-86"))
      { return new Point2D(7578 - textWidth, -5157); }

      return null;
    }

    private IPoint GetCustomPositionD20(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-73"))
      { return new Point2D(14032 - textWidth, 4642); }

      return null;
    }

    private IPoint GetCustomPositionD35(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-60"))
      { return new Point2D(15014, 5213); }

      return null;
    }

    private IPoint GetCustomPositionD40(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-60"))
      {
        foreach (var elem in cm.ConnectionElems)
        {
          if (string.IsNullOrWhiteSpace(elem.ObjectString))
          { continue; }
          Ocad.StringParams.ControlPar par = new Ocad.StringParams.ControlPar(elem.ObjectString);
          string f = par.GetString('f');
          if (f != "69")
          { continue; }
          string t = par.GetString('t');
          if (t != "52")
          { continue; }

          Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)elem.GetMapGeometry();
          Polyline b = new Polyline();
          b.Add(line.BaseGeometry.Points[0]);
          b.Add(new Ocad.Coord.CodePoint(17148, 5076) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
          b.Add(new Point2D(16794, 4378));
          b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

          Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
          cm.CustomGeometries.Add(elem, l1);
        }
        return new Point2D(16166, 4539);
      }

      return null;
    }

    private IPoint GetCustomPositionD70(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-66"))
      { return new Point2D(6942, 7819); }

      return null;
    }

    private IPoint GetCustomPositionDAL(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-73"))
      {
        if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt73") == true) != null
          && cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null)
        {
          return new Point2D(13099, 5462);
        }
      }

      if (cntrText.EndsWith("-60"))
      {
        if (//int.Parse(cntrText.Split('-')[0]) < 10 
          cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null
          && cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("75\tt46") == true) is Ocad.MapElement eTo46)
        {
          //					return new Point2D(15222, 5090);

          Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)eTo46.GetMapGeometry();
          Polyline b = new Polyline();
          b.Add(line.BaseGeometry.Points[0]);
          b.Add(new Ocad.Coord.CodePoint(16388, 4401) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
          b.Add(new Point2D(15604, 3937));
          b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

          Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
          cm.CustomGeometries.Add(eTo46, l1);

          return new Point2D(15547, 3938);
        }
      }


      return null;
    }

    private IPoint GetCustomPositionH16(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      // as DAL
      if (cntrText.EndsWith("-73"))
      {
        if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt73") == true) != null
          && cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null)
        {
          return new Point2D(13099, 5462);
        }
      }

      return null;
    }

    private IPoint GetCustomPositionH20(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-71"))
      {
        if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("47\tt39") == true) != null)
        {
          return new Point2D(13848, 4068);
        }
        else
        {
          return new Point2D(13427, 4087);
        }
      }

      return null;
    }

    private IPoint GetCustomPositionH70(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-80"))
      {
        if (//int.Parse(cntrText.Split('-')[0]) < 10 
          cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt80") == true) != null
          && cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("80\tt31") == true) != null
          && cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("31\tt38") == true) is Ocad.MapElement eTo38)
        {
          //					return new Point2D(15222, 5090);

          Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)eTo38.GetMapGeometry();
          Polyline b = new Polyline();
          b.Add(line.BaseGeometry.Points[0]);
          b.Add(new Ocad.Coord.CodePoint(13207, 4920) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
          b.Add(new Point2D(13195, 4284));
          b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

          Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
          cm.CustomGeometries.Add(eTo38, l1);

          return new Point2D(11782, 4377);
        }
      }
      return null;
    }


    private IPoint GetCustomPositionH80(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-66"))
      { return new Point2D(7370 - textWidth, 6565); }

      return null;
    }

    private IPoint GetCustomPositionHAM(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      // as DAL
      if (cntrText.EndsWith("-73"))
      {
        if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt73") == true) != null
          && cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null)
        {
          return new Point2D(13099, 5462);
        }
      }

      return null;
    }

    private IPoint GetCustomPositionHAK(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      // see H80
      if (cntrText.EndsWith("-66"))
      { return new Point2D(7370 - textWidth, 6565); }

      return null;
    }

    private IPoint GetCustomPositionHE(CourseMap cm, string cntrText, double textWidth,
      IReadOnlyList<Polyline> freeParts)
    {
      if (freeParts.Count > 0)
      { return null; }

      if (cntrText.EndsWith("-82"))
      {
        if (//int.Parse(cntrText.Split('-')[0]) < 10 
          cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("50\tt43") == true) is Ocad.MapElement e50To43)
        {
          //					return new Point2D(15222, 5090);

          Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)e50To43.GetMapGeometry();
          Polyline b = new Polyline();
          b.Add(line.BaseGeometry.Points[0]);
          b.Add(new Ocad.Coord.CodePoint(20865, -1920) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
          b.Add(new Point2D(21024, -2545));
          b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

          Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
          cm.CustomGeometries.Add(e50To43, l1);

          return new Point2D(20289, -2454);
        }
        if (//int.Parse(cntrText.Split('-')[0]) < 10 
          cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("34\tt43") == true) is Ocad.MapElement e34To43)
        {
          //					return new Point2D(15222, 5090);

          Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)e34To43.GetMapGeometry();
          Polyline b = new Polyline();
          b.Add(line.BaseGeometry.Points[0]);
          b.Add(new Ocad.Coord.CodePoint(21455, -1988) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
          b.Add(new Point2D(21343, -2514));
          b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

          Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
          cm.CustomGeometries.Add(e34To43, l1);

          return new Point2D(20289, -2454);
        }
      }
      return null;
    }

    private class Tmpl
    {
      public Tmpl(string front, string back)
      {
        Front = front;
        Back = back;
      }

      public string Front { get; }
      public string Back { get; }
    }
    private class Conf
    {
      public Conf(string course, Tmpl tmpl, int nNr)
        : this(course, tmpl)
      {
        N_Nr = nNr;
      }
      public Conf(string course, Tmpl tmpl)
      {
        Course = course;
        Tmpl = tmpl;
      }

      public int N_Nr { get; }
      public string Course { get; }
      public Tmpl Tmpl { get; }
      public int StartNr { get; set; }
      public int EndNr { get; set; }
      public Func<int, Ocad.Control, Dictionary<string, List<Ocad.Control>>, Dictionary<string, List<Ocad.Control>>, int> CustomSplitWeight { get; set; }

      public override string ToString()
      {
        return $"{Course}; {StartNr} - {EndNr}";
      }
    }

    private void CopyElems(string source, string target)
    {
      List<int> copySymbols = new List<int> { 708000, 709000, 709001, 709002, 709003, 709004 };
      using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(target))
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(source))
      {
        foreach (var elem in r.EnumMapElements())
        {
          if (copySymbols.Contains(elem.Symbol))
          {
            w.Append(elem);
          }
        }
      }
    }

    [TestMethod]
    public void T03_AdaptCourses()
    {
      AdaptCourses(Path.Combine(_root, _exportFolder));
    }

    [TestMethod]
    public void T03_Z_AdaptAllVars()
    {
      AdaptCourses(Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName)));
    }

    private void AdaptCourses(string root)
    {
      string io3File = Path.Combine(root, _io3File);
      Dictionary<string, string> courses = null;
      if (File.Exists(io3File))
      {
        using (TextReader r = new StreamReader(io3File))
        {
          Serializer.Deserialize(out OCourse.Iof3.CourseData courseData, r);
          courses = courseData.RaceCourseData.PersonCourseAssignments.ToDictionary(x => $"{x.BibNumber}", x => x.CourseName);
        }
      }

      foreach (var mapFile in Directory.GetFiles(root))
      {
        AdaptCourse(mapFile, courses);
      }
    }
    private void AdaptCourse(string mapFile, Dictionary<string, string> courses)
    {
      string mapName = Path.GetFileName(mapFile);
      IList<string> parts = mapName.Split(new[] { '.' });
      if (parts.Count != 5 || parts[4] != "ocd")
      { return; }

      CourseMap cm = new CourseMap();
      cm.ControlNrOverprintSymbol = 704005;
      cm.ControlNrSymbols.Add(cm.ControlNrOverprintSymbol);
      cm.InitFromFile(mapFile);

      using (System.Drawing.Bitmap bmpFont = new System.Drawing.Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
      using (System.Drawing.Graphics grpFont = System.Drawing.Graphics.FromImage(bmpFont))
      using (System.Drawing.Font font = new System.Drawing.Font(cm.CourseNameFont, 12))
      using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(mapFile))
      {
        CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
        //if (parts[1].EndsWith("HE"))
        //{ cmd.GetCustomPosition = GetCustomPositionHE; }
        //if (parts[1].EndsWith("HAL"))
        //{ cmd.GetCustomPosition = GetCustomPositionHAL; }
        //if (parts[1].EndsWith("H45"))
        //{ cmd.GetCustomPosition = GetCustomPositionH45; }
        if (parts[1].EndsWith("D20"))
        { cmd.GetCustomPosition = GetCustomPositionD20; }
        if (parts[1].EndsWith("D35"))
        { cmd.GetCustomPosition = GetCustomPositionD35; }
        if (parts[1].EndsWith("D40"))
        { cmd.GetCustomPosition = GetCustomPositionD40; }
        if (parts[1].EndsWith("D70"))
        { cmd.GetCustomPosition = GetCustomPositionD70; }
        if (parts[1].EndsWith("DAL"))
        { cmd.GetCustomPosition = GetCustomPositionDAL; }
        if (parts[1].EndsWith("H16"))
        { cmd.GetCustomPosition = GetCustomPositionH16; }
        if (parts[1].EndsWith("H20"))
        { cmd.GetCustomPosition = GetCustomPositionH20; }
        if (parts[1].EndsWith("H70"))
        { cmd.GetCustomPosition = GetCustomPositionH70; }
        if (parts[1].EndsWith("H80"))
        { cmd.GetCustomPosition = GetCustomPositionH80; }
        if (parts[1].EndsWith("HAK"))
        { cmd.GetCustomPosition = GetCustomPositionHAK; }
        if (parts[1].EndsWith("HAM"))
        { cmd.GetCustomPosition = GetCustomPositionHAM; }
        if (parts[1].EndsWith("HE"))
        { cmd.GetCustomPosition = GetCustomPositionHE; }

        //if (parts[1].EndsWith("12"))
        //{ cmd.GetCustomPosition = GetCustomPositionDH12; }
        //if (parts[1].EndsWith("D14") || parts[1].EndsWith("DAM") || parts[1].EndsWith("D60"))
        //{ cmd.GetCustomPosition = GetCustomPositionDAM; }

        cmd.Execute();

        if (parts[3] == "1")
        {
          AdaptCourseName(w, cm, grpFont, font);
        }
        if (courses != null)
        {
          string nr = parts[2];
          AdaptPermutation(w, parts[2], courses[nr], grpFont, font);
        }
      }

      if (cm.UnplacedControls.Count > 0)
      { }

    }

    [TestMethod]
    public void T04_OptimizeCourses()
    {
      string root = Path.Combine(_root, _exportFolder);
      OptimizeCourses(root, template => template.EndsWith("_V"));
    }
    [TestMethod]
    public void T04_Z_OptimizeAllVars()
    {
      string root = Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName));
      OptimizeCourses(root, template => true);
    }

    private static void OptimizeCourses(string root, Func<string, bool> verifyTemplate)
    {
      List<string> exports = GetExports(root, verifyTemplate);
      Ocad.Scripting.Utils.Optimize(exports, Path.Combine($"{root}", "optimize.xml"), wait: true);
    }

    [TestMethod]
    public void T05_CleanPdfCourses()
    {
      string root = Path.Combine(_root, _exportFolder);
      CleanPdfs(root);
    }
    [TestMethod]
    public void T05_Z_CleanPdfAllVars()
    {
      string root = Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName));
      CleanPdfs(root);
    }

    private void CleanPdfs(string root)
    {
      foreach (var pdfFile in Directory.GetFiles(root, "*.pdf"))
      {
        File.Delete(pdfFile);
      }
    }

    [TestMethod]
    public void T06_ValidateCourses()
    {
      string root = Path.Combine(_root, _exportFolder);
      string io3File = Path.Combine(root, _io3File);
      OCourse.Iof3.CourseData courseData;
      using (TextReader r = new StreamReader(io3File))
      {
        Serializer.Deserialize(out courseData, r);
      }

      Dictionary<string, OCourse.Iof3.Course> courseDict = courseData.RaceCourseData.Courses.ToDictionary(x => x.Name);
      foreach (var assignment in courseData.RaceCourseData.PersonCourseAssignments)
      {
        ValidateCourse(assignment, courseDict[assignment.CourseName], root);
      }
    }

    private void ValidateCourse(OCourse.Iof3.PersonCourseAssignment assignment, OCourse.Iof3.Course course, string root)
    {
      int n = 0;
      string allMap = null;
      string front = null;
      string back = null;
      foreach (var fileName in Directory.EnumerateFiles(root, $"*.{assignment.BibNumber}.*.ocd"))
      {
        IList<string> parts = Path.GetFileName(fileName).Split('.');
        Assert.AreEqual(5, parts.Count);
        Assert.AreEqual(assignment.CourseFamily, parts[1]);
        Assert.AreEqual($"{assignment.BibNumber}", parts[2]);
        Assert.IsTrue(parts[0].EndsWith("_V") || parts[0].EndsWith("_H"));
        Assert.IsTrue(parts[3] == "1" || parts[3] == "2");
        string map = parts[0].Substring(0, parts[0].Length - 2);
        allMap = allMap ?? map;
        Assert.AreEqual(allMap, map);

        if (parts[0].EndsWith("_V") && parts[3] == "1")
        {
          Assert.IsNull(front);
          front = fileName;
        }
        if (parts[0].EndsWith("_H") && parts[3] == "2")
        {
          Assert.IsNull(back);
          back = fileName;
        }

        n++;
      }

      //Assert.AreEqual(4, n);
      Assert.IsNotNull(front);
      Assert.IsNotNull(back);

      GetElements(front, out List<Ocad.Element> fControls, out List<Ocad.Element> fControlNrs, out List<Ocad.Element> fStarts, out List<Ocad.Element> fFinishs);
      Assert.AreEqual(fControls.Count, fControlNrs.Count);
      Assert.AreEqual(1, fStarts.Count);
      Assert.AreEqual(0, fFinishs.Count);

      GetElements(back, out List<Ocad.Element> bControls, out List<Ocad.Element> bControlNrs, out List<Ocad.Element> bStarts, out List<Ocad.Element> bFinishs);
      Assert.AreEqual(bControls.Count, bControlNrs.Count);
      Assert.AreEqual(1, bStarts.Count);
      Assert.AreEqual(1, bFinishs.Count);

      List<Ocad.Element> controlNrs = new List<Ocad.Element>();
      controlNrs.AddRange(fControlNrs);
      controlNrs.AddRange(bControlNrs);

      Dictionary<int, Ocad.Element> controlsDict = new Dictionary<int, Ocad.Element>();
      foreach (var controlNr in controlNrs)
      {
        string text = controlNr.Text;
        IList<string> parts = text.Split('-');
        Assert.AreEqual(2, parts.Count);
        string nrs = parts[0];
        string id = parts[1];
        foreach (var nr in nrs.Split('/'))
        {
          controlsDict.Add(int.Parse(nr), controlNr);
        }
      }

      int nCtrs = course.CourseControls.Count;
      Assert.AreEqual(nCtrs - 2, controlsDict.Count);
      Assert.AreEqual("Start", course.CourseControls[0].type);
      Assert.AreEqual("Finish", course.CourseControls[nCtrs - 1].type);
      for (int iControl = 1; iControl < nCtrs - 1; iControl++)
      {
        int controlNr = int.Parse(course.CourseControls[iControl].Control);
        Assert.AreEqual(controlNr, int.Parse(controlsDict[iControl].Text.Split('-')[1]));
      }
    }

    private void GetElements(string file, out List<Ocad.Element> controls, out List<Ocad.Element> controlNrs, out List<Ocad.Element> starts, out List<Ocad.Element> finishs)
    {
      controls = new List<Ocad.Element>();
      controlNrs = new List<Ocad.Element>();
      starts = new List<Ocad.Element>();
      finishs = new List<Ocad.Element>();

      using (Ocad.OcadReader r = Ocad.OcadReader.Open(file))
      {
        foreach (var elem in r.EnumMapElements())
        {
          if (elem.Symbol == 704005 && elem.Text == "tttt")
          { continue; }

          if (elem.Symbol == 703000)
          { controls.Add(elem); }
          if (elem.Symbol == 704000 || elem.Symbol == 704005)
          { controlNrs.Add(elem); }
          if (elem.Symbol == 701000)
          { starts.Add(elem); }
          if (elem.Symbol == 706000)
          { finishs.Add(elem); }
        }
      }
    }

    [TestMethod]
    public void T07_ExportCourses()
    {
      string root = Path.Combine(_root, _exportFolder);
      ExportCourses(root, template => template.EndsWith("_V"));
    }

    [TestMethod]
    public void T07_Z_ExportAllVars()
    {
      string root = Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName));
      ExportCourses(root, x => true);
    }

    private static void ExportCourses(string root, Func<string, bool> verifyTemplate)
    {
      bool success = false;
      while (success == false)
      {
        success = true;
        List<string> exports = GetExports(root, verifyTemplate);
        int maxExport = short.MaxValue;
        List<string> maxExports = new List<string>();
        foreach (var export in exports)
        {
          if (maxExports.Count >= maxExport)
          {
            success &= ExportCourses(maxExports);
            maxExports.Clear();
            System.Threading.Thread.Sleep(500);
          }

          string ocdTmpl = export.Replace(".1.ocd", ".Front.ocd").Replace(".2.ocd", ".Rueck.ocd");
          string pdfPath = PrepPdf(ocdTmpl, deleteFile: false);
          if (File.Exists(pdfPath))
          { continue; }
          maxExports.Add(export);
        }

        success &= ExportCourses(maxExports);
      }
    }

    private static bool ExportCourses(List<string> exports)
    {
      if (exports.Count <= 0)
      { return true; }
      string scriptFile = Path.Combine(Path.GetDirectoryName(exports.First()), "createPdfs.xml");
      string defaultExe;
      string lastPdf;
      string firstOcd;
      using (Ocad.Scripting.Script s = new Ocad.Scripting.Script(scriptFile))
      {
        defaultExe = ExportBackgrounds(s, exports);
        CreatePdfScript(exports, s);
        firstOcd = s.ScriptNode.FirstChild.FirstChild.InnerText;
        var lastExportNode = s.ScriptNode.LastChild.PreviousSibling.PreviousSibling;
        lastPdf = lastExportNode.FirstChild.InnerText;
      }

      //ProcessStartInfo si = new ProcessStartInfo(defaultExe);
      //si.WindowStyle = ProcessWindowStyle.Normal;
      //Process proc = Process.Start(si);
      //{
      //  IntPtr hwnd = IntPtr.Zero;
      //  string version = null;
      //  System.Threading.Thread.Sleep(2000);
      //  while (hwnd == IntPtr.Zero)
      //  {
      //    Processor m = new Processor();
      //    hwnd = string.IsNullOrEmpty(version)
      //      ? IntPtr.Zero
      //      : m.SetForegroundWindow(version, out string fullText);
      //    if (hwnd == IntPtr.Zero)
      //    {
      //      var procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(defaultExe));
      //      if (procs.Length > 0)
      //      {
      //        if (procs.Length > 1)
      //        {

      //        }
      //        version = procs[0].MainModule.FileVersionInfo.FileVersion;
      //        version = version.Substring(0, version.LastIndexOf('.'));

      //        m.GlobalMove(1120, 1060);
      //        m.MausClick(1120, 1060);
      //        //m.SetForegroundProcess(procs[0]);
      //        //Ui.ShowWindow(procs[0].MainWindowHandle, ShowWindowCommands.Restore);
      //      }
      //    }
      //    System.Threading.Thread.Sleep(2000);
      //  }
      //  Processor.Minimize(hwnd);
      //  System.Threading.Thread.Sleep(2000);
      //}


      string procName = new Ocad.Scripting.Utils { Exe = defaultExe }.RunScript(scriptFile, defaultExe, wait: 20000);
      int nPrePdf = 0;
      while (!File.Exists(lastPdf))
      {
        System.Threading.Thread.Sleep(4000);
        int nPdf = Directory.GetFiles(Path.GetDirectoryName(lastPdf), "*.pdf").Length;
        if (nPdf == nPrePdf)
        {
          Ocad.Scripting.Utils.CloseOcadProcs(defaultExe, force: true);
          return false;
        }
        nPrePdf = nPdf;
      }
      Ocad.Scripting.Utils.CloseOcadProcs(defaultExe, force: true);

      return true;
    }

    private static List<string> GetExports(string root, Func<string, bool> verifyTemplate)
    {
      List<string> exports = new List<string>();
      foreach (var mapFile in Directory.GetFiles(root))
      {
        IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
        if (parts.Count != 5)
        { continue; }
        if (parts[4] != "ocd")
        { continue; }

        if (!verifyTemplate(parts[0]) || parts[3] != "1")
        { continue; }

        string cat = parts[1];
        string nr = parts[2];

        string back = parts[0].Replace("_V", "_H");
        string backFile = Path.Combine(root, $"{back}.{parts[1]}.{parts[2]}.2.ocd");
        if (!File.Exists(backFile))
        { throw new InvalidOperationException($"{backFile} not found"); }
        exports.Add(mapFile);
        exports.Add(backFile);
      }
      return exports;
    }

    private static string ExportBackgrounds(Ocad.Scripting.Script script, List<string> exports)
    {
      Dictionary<string, string> bgs = new Dictionary<string, string>();
      foreach (var exp in exports)
      {
        string bg = Path.GetFileName(exp).Split('.')[0];
        bgs[bg] = exp;
      }
      bool first = true;
      string exe = null;
      foreach (var pair in bgs)
      {
        string bg = pair.Value;
        exe = exe ?? Utils.FindExecutable(bg);
        string bgFileName = Path.Combine(Path.GetDirectoryName(bg), $"{pair.Key}_bg.ocd");
        File.Copy(bg, bgFileName, overwrite: true);
        using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(bgFileName))
        {
          w.DeleteElements((i) => true);
        }

        string pdfPath = PrepPdf(bgFileName);
        using (Ocad.Scripting.Node node = script.FileOpen())
        { node.File(bgFileName); }
        using (Ocad.Scripting.Node node = script.MapOptimize())
        { node.Enabled(true); }
        using (Ocad.Scripting.Node node = script.FileExport())
        {
          node.File(pdfPath);
          node.Format(Ocad.Scripting.Format.Pdf);
        }
        using (Ocad.Scripting.Node node = script.FileSave())
        { node.Enabled(true); }

        if (!first)
        {
          using (Ocad.Scripting.Node node = script.FileClose())
          { node.Enabled(true); }
        }
        first = false;
      }

      return exe;
    }

    [TestMethod]
    public void T08_JoinCoursePdfs()
    {
      string root = Path.Combine(_root, _exportFolder);
      JoinPdfs(root);
    }

    [TestMethod]
    public void T08_Z_JoinAllVarsPdfs()
    {
      string root = Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName));
      JoinPdfs(root);
    }

    [TestMethod]
    public void JoinPdfs(string root)
    {
      List<PdfId> exports = new List<PdfId>();
      foreach (var mapFile in Directory.GetFiles(root))
      {
        IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
        if (parts.Count != 5)
        { continue; }
        if (parts[4] != "pdf")
        { continue; }

        PdfId p = new PdfId { Bg = parts[0], Cat = parts[1], Nr = parts[2], Side = parts[3], File = mapFile };
        exports.Add(p);
      }
      exports.Sort((x, y) =>
      {
        int d = x.Cat.CompareTo(y.Cat);
        if (d != 0) return d;
        d = x.Nr.CompareTo(y.Nr);
        if (d != 0) return d;
        d = x.Side.CompareTo(y.Side);
        return d;
      });

      Dictionary<string, List<PdfId>> catFiles = new Dictionary<string, List<PdfId>>();
      foreach (var export in exports)
      {
        catFiles.GetOrCreateValue(export.Cat).Add(export);
      }

      foreach (var pair in catFiles)
      {
        string cat = pair.Key;

        List<string> files = pair.Value.Select(x => x.File).ToList();

        string front = Path.Combine(root, $"{pair.Value[0].Bg}_bg.pdf");
        string rueck = Path.Combine(root, $"{pair.Value[1].Bg}_bg.pdf");

        OTextSharp.Models.PdfText.OverprintFrontBack(
          Path.Combine(root, $"Comb_{cat}.pdf"), files, front, rueck);
      }
    }
    private class PdfId
    {
      public string Bg { get; set; }
      public string Cat { get; set; }
      public string Nr { get; set; }
      public string Side { get; set; }
      public string File { get; set; }
    }

    private static string PrepPdf(string ocdFile, bool deleteFile = true)
    {
      string pdfFile = Path.GetFileNameWithoutExtension(ocdFile) + ".pdf";
      string dir = Path.GetDirectoryName(ocdFile);
      string pdfPath = Path.Combine(dir, pdfFile);
      if (deleteFile)
      {
        if (File.Exists(pdfPath))
        { File.Delete(pdfPath); }
      }

      return pdfPath;
    }

    private static string CreatePdfScript(IEnumerable<string> files, Ocad.Scripting.Script expPdf)
    {
      string exe = null;
      foreach (var ocdFile in files)
      {
        exe = exe ?? Utils.FindExecutable(ocdFile);
        string ocdTmpl = ocdFile.Replace(".1.ocd", ".Front.ocd").Replace(".2.ocd", ".Rueck.ocd");
        string pdfPath = PrepPdf(ocdTmpl);

        using (Ocad.Scripting.Node node = expPdf.FileOpen())
        { node.File(ocdFile); }
        using (Ocad.Scripting.Node node = expPdf.BackgroundMapRemove())
        { }
        using (Ocad.Scripting.Node node = expPdf.FileExport())
        {
          node.File(pdfPath);
          node.Format(Ocad.Scripting.Format.Pdf);
          //node.Child("ExportScale","10000");
          //node.Child("Colors", "normal");
          //using (Ocad.Scripting.Node ext = node.Child("PartOfMap"))
          //{
          //	ext.Enabled(true);
          //	ext.Child("Coordinates", "mm");
          //	ext.Child("L", "0");
          //	ext.Child("R", "100");
          //	ext.Child("B", "0");
          //	ext.Child("T", "100");
          //}
        }
        //using (Ocad.Scripting.Node node = expPdf.FileSave())
        //{ node.Enabled(true); }
        using (Ocad.Scripting.Node node = expPdf.FileClose())
        { node.Enabled(true); }
      }
      if (exe != null)
      {
        using (Ocad.Scripting.Node node = expPdf.FileExit())
        {
          node.Enabled(true);
        }
      }
      return exe;
    }


    private void AdaptCourseName(Ocad.OcadWriter w, CourseMap cm, System.Drawing.Graphics grpFont, System.Drawing.Font font)
    {
      if (string.IsNullOrWhiteSpace(cm.CourseNameFont))
      { return; }
      w.AdaptElements((elem) =>
      {
        if (!cm.CourseNameSymbols.Contains(elem.Symbol))
        { return null; }

        string fullCn = elem.Text;
        string coreCn = fullCn;
        int idx = coreCn.IndexOf("(");
        if (idx > 0)
        { coreCn = coreCn.Substring(0, idx - 1); }
        idx = coreCn.IndexOf(".");
        if (idx > 0)
        { coreCn = coreCn.Substring(idx + 1); }

        if (coreCn == fullCn)
        { return null; }

        float fullW = grpFont.MeasureString(fullCn, font).Width;
        float coreW = grpFont.MeasureString(coreCn, font).Width;

        Ocad.GeoElement.Geom geom = elem.GetMapGeometry();
        Basics.Geom.PointCollection pts = (Basics.Geom.PointCollection)geom.GetGeometry();
        double f = coreW / fullW;
        double fullWidth = pts[2].X - pts[0].X;
        ((Basics.Geom.Point)pts[2]).X = pts[0].X + f * fullWidth;
        ((Basics.Geom.Point)pts[3]).X = pts[2].X;
        elem.SetMapGeometry(Ocad.GeoElement.Geom.Create(pts));
        elem.Text = coreCn;

        return elem;
      });
    }
    private void AdaptPermutation(Ocad.OcadWriter w, string nr, string permutation, System.Drawing.Graphics grpFont, System.Drawing.Font font)
    {
      w.AdaptElements((elem) =>
      {
        if (elem.Text != "permutation_combination")
        { return null; }

        string fullCn = elem.Text;
        string coreCn = permutation;

        if (coreCn == fullCn)
        { return null; }

        float fullW = grpFont.MeasureString(fullCn, font).Width;
        float coreW = grpFont.MeasureString(coreCn, font).Width;

        Ocad.GeoElement.Geom geom = elem.GetMapGeometry();
        PointCollection pts = (PointCollection)geom.GetGeometry();
        double f = coreW / fullW;
        double fullWidth = pts[2].X - pts[0].X;
        ((Point)pts[2]).X = pts[0].X + f * fullWidth;
        ((Point)pts[3]).X = pts[2].X;
        elem.SetMapGeometry(Ocad.GeoElement.Geom.Create(pts));
        elem.Text = coreCn;

        return elem;
      });
    }
  }
}
