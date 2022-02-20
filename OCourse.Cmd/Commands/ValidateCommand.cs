
using Basics;
using Basics.Cmd;
using Basics.Geom;
using OCourse.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OCourse.Cmd.Commands
{
  /// <summary>
  /// Validate Controls in OCAD against IO3-CourseExport
  /// </summary>
  public class ValidateCommand : ICommand
  {
    public class Parameters : IDisposable
    {
      private static readonly List<Command<Parameters>> _cmds = new List<Command<Parameters>>
        {
            new Command<Parameters>
            {
                Key = "-c",
                Parameters = "<course file>[,<course file 1>, ...]",
                Read = (p, args, i) =>
                {
                    p.CourseFile = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-f",
                Parameters = "<courseFamily>",
                Read = (p, args, i) =>
                {
                    p.CourseFamily = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-b",
                Parameters = "<bibNr>",
                Read = (p, args, i) =>
                {
                    p.Bib = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-x",
                Parameters = "<io3-course export xml>",
                Read = (p, args, i) =>
                {
                    p.CourseExport = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-p",
                Parameters = "<overprint symbol>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.OverprintSymbol = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-r",
                Parameters = "<text to replace>",
                Info = "OCAD element with text <text to replace> will be replaced by course name in <io3-course export xml>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.TextToReplace = args[i + 1];
                    return 1;
                }
            },
        };

      public string CourseFile { get; set; }
      public string CourseFamily { get; set; }
      public string Bib { get; set; }
      public string CourseExport { get; set; }
      public string OverprintSymbol { get; set; }
      public string TextToReplace { get; set; }
      public void Dispose()
      { }

      public string Validate()
      {
        if (!string.IsNullOrWhiteSpace(OverprintSymbol) && !int.TryParse(OverprintSymbol, out int nr))
        {
          return $"Cannot convert overprint symbol {OverprintSymbol} to int.";
        }
        return null;
      }
      /// <summary>
      ///     interpret and verify command line arguments
      /// </summary>
      /// <param name="args"></param>
      /// <returns>null, if not successfull, otherwise interpreted parameters</returns>
      public static ICommand ReadArgs(IList<string> args)
      {
        Parameters result = new Parameters();

        if (Basics.Cmd.Utils.ReadArgs(result, args, _cmds, out string error))
        {
          if (string.IsNullOrWhiteSpace(error))
          {
            error = result.Validate();
          }
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
          Console.WriteLine(error);
          return null;
        }

        return new ValidateCommand(result);
      }
    }

    private readonly Parameters _pars;

    private HashSet<string> _handledConnections;
    public ValidateCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      bool success = true;
      StringBuilder sb = null;
      try
      {
        Iof3.CourseData courseData;
        using (TextReader r = new StreamReader(_pars.CourseExport))
        {
          Serializer.Deserialize(out courseData, r);
        }
        Dictionary<PersonKey, Iof3.PersonCourseAssignment> persons =
          courseData.RaceCourseData.PersonCourseAssignments.ToDictionary(
            x => new PersonKey { Bib = $"{x.BibNumber}", CourseFamily = $"{x.CourseFamily}" }, new PersonKey.Comparer());
        Dictionary<string, Iof3.Course> courseDict = courseData.RaceCourseData.Courses.ToDictionary(x => x.Name);
        Dictionary<PersonKey, Iof3.PersonCourseAssignment>
          unhandledPersons = new Dictionary<PersonKey, Iof3.PersonCourseAssignment>(persons, new PersonKey.Comparer());

        _handledConnections = new HashSet<string>();
        IList<string> parts = _pars.CourseFile.Split(',');
        TemplateUtils tmpl = new TemplateUtils(parts[0]);
        string dir = Path.GetDirectoryName(parts[0]);
        foreach (string course in Directory.GetFiles(dir, tmpl.Template))
        {
          if (!Validate(course, parts, tmpl, out string fileError,
            out List<string> files, out Dictionary<int, Ocad.MapElement> allControls))
          {
            sb = sb ?? new StringBuilder();
            sb.AppendLine($"error validating {course}: {fileError}");
            success = false;
          }

          string bib = tmpl.GetReplaced(course, _pars.Bib);
          string courseFamily = tmpl.GetReplaced(course, _pars.CourseFamily);
          PersonKey personKey = new PersonKey { Bib = bib, CourseFamily = courseFamily };

          if (!unhandledPersons.TryGetValue(personKey, out Iof3.PersonCourseAssignment assignment))
          {
            sb.AppendLine($"PersonCourseAssignment {bib},{courseFamily} not found in {_pars.CourseExport}");
            continue;
          }
          unhandledPersons.Remove(personKey);
          string courseName = assignment.CourseName;
          if (!courseDict.TryGetValue(courseName, out Iof3.Course c))
          {
            sb.AppendLine($"Course {courseName} of {bib},{courseFamily} not found in {_pars.CourseExport}");
            continue;
          }
          ValidateCourse(c, allControls, sb);

          if (!string.IsNullOrWhiteSpace(_pars.TextToReplace))
          {
            foreach (var file in files)
            {
              using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(file))
              {
                w.AdaptElements((e) =>
                {
                  if (e.Text != _pars.TextToReplace)
                  { return null; }
                  e.Text = c.Name;
                  return e;
                });
              }
            }
          }
        }
        if (unhandledPersons.Count > 0)
        {
          foreach (var person in unhandledPersons.Keys)
          {
            sb.AppendLine($"No map files found for {person.CourseFamily},{person.Bib}");
          }
        }
      }
      catch
      {
        _pars.Dispose();
        throw;
      }
      error = sb?.ToString();
      return success;
    }

    private bool ValidateCourse(Iof3.Course course, Dictionary<int, Ocad.MapElement> controlsDict, StringBuilder sb)
    {
      int nCtrs = course.CourseControls.Count;
      if (nCtrs - 2 != controlsDict.Count)
      {
        sb.AppendLine($"Expected {nCtrs - 2} controls from course file, found {controlsDict.Count} in map files");
        return false;
      }
      if ("Start" != course.CourseControls[0].type)
      {
        sb.AppendLine($"Expected Start as first control in course file");
        return false;
      }
      if ("Finish" != course.CourseControls[course.CourseControls.Count - 1].type)
      {
        sb.AppendLine($"Expected Finish as last control in course file");
        return false;
      }

      bool success = true;
      for (int iControl = 1; iControl < nCtrs - 1; iControl++)
      {
        int courseControl = int.Parse(course.CourseControls[iControl].Control);
        int mapControl = int.Parse(controlsDict[iControl].Text.Split('-')[1]);
        if (courseControl != mapControl)
        {
          sb.AppendLine($"Expected courseControl {courseControl} as {iControl}th in map, got {mapControl}");
          success = false;
        }
      }
      return success;
    }

    private bool Validate(string startFile, IList<string> parts, TemplateUtils tmpl, out string error,
      out List<string> files, out Dictionary<int, Ocad.MapElement> allControls)
    {
      allControls = new Dictionary<int, Ocad.MapElement>();
      files = new List<string>();
      files.Add(startFile);
      StringBuilder sb = new StringBuilder();
      if (!Validate(startFile, allControls, parts.Count == 1, out string startError))
      {
        sb.AppendLine(startError);
      }
      for (int iPart = 1; iPart < parts.Count; iPart++)
      {
        string result = tmpl.GetReplaced(startFile, parts[iPart]);
        files.Add(result);
        if (!Validate(result, allControls, iPart == parts.Count - 1, out string partError))
        {
          sb.AppendLine(partError);
        }
      }

      bool success = sb.Length == 0;
      error = !success ? sb.ToString() : null;

      return success;
    }

    private bool Validate(string file, Dictionary<int, Ocad.MapElement> allControls, bool isLast, out string error)
    {
      if (!File.Exists(file))
      {
        error = $"{file} not found";
        return false;
      }

      CourseMap cm = new CourseMap();
      if (!string.IsNullOrWhiteSpace(_pars.OverprintSymbol))
      { cm.ControlNrOverprintSymbol = int.Parse(_pars.OverprintSymbol); }
      cm.ControlNrSymbols.Add(cm.ControlNrOverprintSymbol);
      cm.InitFromFile(file);


      if (cm.StartElems.Count != 1)
      {
        error = $"Expected 1 start in {file}, got {cm.StartElems.Count}";
        return false;
      }
      int nFinish = isLast ? 1 : 0;
      if (cm.FinishElems.Count != nFinish)
      {
        error = $"Expected {nFinish} finish in {file}, got {cm.FinishElems.Count}";
        return false;
      }

      StringBuilder sb = new StringBuilder();      

      CheckConnections(cm, sb);

      Dictionary<int, Ocad.MapElement> controlsDict = CheckControlNrs(cm, sb, out int nHelp);

      int nControls = cm.AllControls.Where(c => cm.ControlSymbols.Contains(c.Symbol)).Count();
      if (cm.ControlNrElems.Count - nHelp != nControls)
      { sb.AppendLine($"Expected {nControls} Controls-Nrs, got {cm.ControlNrElems.Count - nHelp}."); }

      List<int> controlNrs = new List<int>(controlsDict.Keys);
      controlNrs.Sort();
      int max = allControls.Count;
      int nCtr = controlNrs.Count;
      if (controlNrs[0] != max + 1)
      {
        sb.AppendLine($"Expected mimimum control {max + 1}, got {controlNrs[0]}");
      }
      if (controlNrs[nCtr - 1] - controlNrs[0] != nCtr - 1)
      {
        sb.AppendLine($"Expected maximum control {controlNrs[0] + nCtr - 1}, got {controlNrs[nCtr - 1]}");
      }

      foreach (var pair in controlsDict)
      {
        allControls.Add(pair.Key, pair.Value);
      }

      if (sb.Length > 0)
      {
        error = sb.ToString();
        return false;
      }

      error = null;
      return true;
    }

    private void CheckConnections(CourseMap cm, StringBuilder sb)
    {
      foreach (var conn in cm.ConnectionElems)
      {
        Ocad.StringParams.NamedParam p = new Ocad.StringParams.NamedParam(conn.ObjectString);
        string key = $"{p.GetString('f')}-{p.GetString('t')}";
        if (_handledConnections.Add(key))
        {
          Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)conn.GetMapGeometry();
          foreach (var entry in cm.LayoutsTree.Search(line.Extent))
          {
            foreach (var seg in line.BaseGeometry.EnumSegments())
            {
              if (seg.Start is Ocad.Coord.CodePoint coord && (coord.Flags & Ocad.Coord.Flags.noLine) == Ocad.Coord.Flags.noLine)
              {
                continue;
              }

              if (GeometryOperator.Intersects(seg, Box.CastOrWrap(entry.Box)))
              {
                sb.AppendLine($"Line {key} intersects {entry.Value.Elem.Symbol}. See course {p.Name}");
              }
            }
          }
        }
      }
    }

    private Dictionary<int, Ocad.MapElement> CheckControlNrs(CourseMap cm, StringBuilder sb, out int nHelp)
    {
      nHelp = 0;

      Dictionary<int, Ocad.MapElement> controlsDict = new Dictionary<int, Ocad.MapElement>();
      foreach (var controlNr in cm.ControlNrElems)
      {
        string text = controlNr.Text;
        IList<string> parts = text.Split('-');
        if (parts.Count != 2)
        {
          var nrBox = controlNr.GetMapGeometry().Extent;
          double boxExt = BoxOp.GetMaxExtent(nrBox);
          Point2D off = new Point2D(boxExt, boxExt);
          Box search = new Box(PointOp.Sub(nrBox.Min, off), PointOp.Add(nrBox.Max, off));
          bool near = false;
          foreach (var entry in cm.ControlsTree.Search(search))
          {
            near = true;
            break;
          }
          if (!near)
          {
            nHelp++;
            continue;
          }
          sb.AppendLine($"Expected 2 parts in controlNr {text}.Split('-'), got {parts.Count}");
          continue;
        }
        foreach (var nr in parts[0].Split('/'))
        {
          if (!int.TryParse(nr, out int iNr))
          {
            sb.AppendLine($"Invalid control number {text}. Cannot parse control.");
            continue;
          }
          if (controlsDict.ContainsKey(iNr))
          {
            sb.AppendLine($"Control {iNr} already exists");
            continue;
          }
          controlsDict.Add(iNr, controlNr);
        }
        PointCollection geom = (PointCollection)controlNr.GetMapGeometry().GetGeometry();

        bool nearCtr = Near(text, parts[1], geom, cm, sb);
      }
      return controlsDict;
    }
    private bool Near(string text, string ctr, PointCollection geom, CourseMap cm, StringBuilder sb)
    {
      NearInfo controlInfo = null;
      double f = 2;
      bool success = true;
      while (controlInfo == null)
      {
        double cd = f * cm.ControlDistance;
        CheckNear(ctr, geom, cm, f, out controlInfo, out List<NearInfo> others);
        if (controlInfo == null)
        {
          if (others?.Count > 0)
          {
            sb.AppendLine($"Control text {text} not near Control {ctr}");
            return false;
          }
          f = 1.5 * f;
          continue;
        }
        double off = controlInfo.Offset;
        if (off < cm.ControlDistance)
        {
          sb.AppendLine($"Misplaced control nr {text}: Offset {off:N0} < {cm.ControlDistance:N0}");
          break;
        }
        foreach (var other in others)
        {
          if (cm.FinishSymbols.Contains(other.Element.Symbol))
          {
            if (off < 1.2 * cm.ControlDistance)
            {
              sb.AppendLine($"Control nr {text} too close to finish: Offset {off:N0} < {1.2 * cm.ControlDistance:N0}");
              success = false;
            }
            continue;
          }
          if (cm.StartSymbols.Contains(other.Element.Symbol))
          {
            if (off < 1.2 * cm.ControlDistance)
            {
              sb.AppendLine($"Control nr {text} too close to start: Offset {off:N0} < {1.2 * cm.ControlDistance:N0}");
              success = false;
            }
            continue;
          }
          if (1.5 * off > other.Offset)
          {
            Ocad.StringParams.ControlPar p = new Ocad.StringParams.ControlPar(other.Element.ObjectString);
            sb.AppendLine($"Control nr {text} too near to control {p.Id}: Offset {other.Offset:N0} < {1.5 * off:N0}");
            success = false;
          }
        }
      }
      return success;
    }

    private void CheckNear(string ctr, PointCollection geom, OCourse.Commands.CourseMap cm, double f,
      out NearInfo controlInfo, out List<NearInfo> others)
    {
      double cd = f * cm.ControlDistance;
      Point2D maxOff = new Point2D(cd, cd);
      Point2D ll = new Point2D(geom[0].X, geom[0].Y);
      Point2D ur = new Point2D(geom[2].X, geom[0].Y + cm.ControlNrHeight);
      Box textBox = new Box(ll, ur);
      IBox searchBox = new Box(ll - maxOff, ur + maxOff);

      controlInfo = null;
      others = new List<NearInfo>();
      foreach (var entry in cm.ControlsTree.Search(searchBox))
      {
        Point center = Point.CastOrCreate(entry.Value.Elem.GetMapGeometry().EnumPoints().First());
        double off = Math.Sqrt(textBox.Dist2(center));

        NearInfo ni = new NearInfo { Offset = off, Element = entry.Value.Elem };
        Ocad.StringParams.ControlPar p = new Ocad.StringParams.ControlPar(entry.Value.Elem.ObjectString);

        if (p.Id != ctr)
        {
          others.Add(ni);
        }
        else
        {
          controlInfo = ni;
        }
      }
    }

    private class NearInfo
    {
      public double Offset { get; set; }
      public Ocad.MapElement Element { get; set; }
    }
    private class PersonKey
    {
      public class Comparer : IEqualityComparer<PersonKey>
      {
        public bool Equals(PersonKey x, PersonKey y)
        {
          return x.Bib == y.Bib && x.CourseFamily == y.CourseFamily;
        }

        public int GetHashCode(PersonKey obj)
        {
          return obj.Bib.GetHashCode() ^ 37 * obj.CourseFamily.GetHashCode();
        }
      }
      public string Bib { get; set; }
      public string CourseFamily { get; set; }
    }

  }
}
