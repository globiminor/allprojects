
using Basics.Cmd;
using Basics.Geom;
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
        IList<string> parts = _pars.CourseFile.Split(',');
        TemplateUtils tmpl = new TemplateUtils(parts[0]);
        string dir = Path.GetDirectoryName(parts[0]);
        foreach (string course in Directory.GetFiles(dir, tmpl.Template))
        {
          if (!Validate(course, parts, tmpl, out string fileError))
          {
            sb = sb ?? new StringBuilder();
            sb.AppendLine($"error validating {course}: {fileError}");
            success = false;
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

    private bool Validate(string startFile, IList<string> parts, TemplateUtils tmpl, out string error)
    {
      Dictionary<int, Ocad.MapElement> allControls = new Dictionary<int, Ocad.MapElement>();
      if (!Validate(startFile, allControls, parts.Count == 1, out string e))
      {
        error = e;
        return false;
      }
      for (int iPart = 1; iPart < parts.Count; iPart++)
      {
        string result = tmpl.GetReplaced(startFile, parts[iPart]);
        if (!Validate(result, allControls, iPart == parts.Count - 1, out string pe))
        {
          error = pe;
          return false;
        }
      }
      error = null;
      return true;
    }

    private bool Validate(string file, Dictionary<int, Ocad.MapElement> allControls, bool isLast, out string error)
    {
      if (!File.Exists(file))
      {
        error = $"{file} not found";
        return false;
      }

      OCourse.Commands.CourseMap cm = new OCourse.Commands.CourseMap();
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
      Dictionary<int, Ocad.MapElement> controlsDict = new Dictionary<int, Ocad.MapElement>();
      int nHelp = 0;
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

        // Verify ControlNr is only near corresponding control circle
        double cd = 2 * cm.ControlDistance;
        Point2D maxOff = new Point2D(cd, cd);
        Point2D ll = new Point2D(geom[0].X, geom[0].Y);
        Point2D ur = new Point2D(geom[2].X, geom[0].Y + cm.ControlNrHeight);
        Box textBox = new Box(ll, ur);
        IBox searchBox = new Box(ll - maxOff, ur + maxOff);
        bool nearCtr = false;
        foreach (var entry in cm.ControlsTree.Search(searchBox))
        {
          Point center = Point.CastOrCreate(entry.Value.Elem.GetMapGeometry().EnumPoints().First());
          double off = Math.Sqrt(textBox.Dist2(center));
          Ocad.StringParams.ControlPar p = new Ocad.StringParams.ControlPar(entry.Value.Elem.ObjectString);
          if (p.Id != parts[1])
          {
            if (cm.FinishSymbols.Contains(entry.Value.Elem.Symbol))
            {
              if (off < 1.2 * cm.ControlDistance)
              {
                sb.AppendLine($"Control nr {text} too close to finish: Offset {off:N0} < {1.2 * cm.ControlDistance:N0}");
              }
              continue; 
            }
            if (off < 1.8 * cm.ControlDistance)
            {
              sb.AppendLine($"Control nr {text} too near to control {p.Id}: Offset {off:N0} < {1.8 * cm.ControlDistance:N0}");
            }
            continue;
          }
          if (off < cm.ControlDistance)
          {
            sb.AppendLine($"Misplaced control nr {text}: Offset {off:N0} < {cm.ControlDistance:N0}");
          }
          if (off > 1.6 * cm.ControlDistance)
          {
            sb.AppendLine($"Control nr {text} too far from control: Offset {off:N0} > {1.5 * cm.ControlDistance:N0}");
          }
          nearCtr = true;
        }
        if (!nearCtr)
        { sb.AppendLine($"Control text {text} not near Control {parts[1]}"); }
      }
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

      if (sb.Length > 0)
      {
        error = sb.ToString();
        return false;
      }
      foreach (var pair in controlsDict)
      {
        allControls.Add(pair.Key, pair.Value);
      }

      error = null;
      return true;
    }
  }
}
