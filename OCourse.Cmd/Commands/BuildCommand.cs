
using Basics.Cmd;
using OCourse.Commands;
using OCourse.Route;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OCourse.Cmd.Commands
{
  public class BuildCommand : ICommand
  {
    public class Parameters : IDisposable
    {
      private static readonly List<Command<Parameters>> _cmds = new List<Command<Parameters>>
        {
            new Command<Parameters>
            {
                Key = "-c",
                Parameters = "<config path>",
                Read = (p, args, i) =>
                {
                    p.ConfigPath = args[i + 1];
                    return 1;
                }
            },


            new Command<Parameters>
            {
                Key = "-o",
                Parameters = "<output path>",
                Optional = false,
                Read = (p, args, i) =>
                {
                    p.OutputPath = args[i + 1];
                    return 1;
                }
            },


            new Command<Parameters>
            {
                Key = "-b",
                Parameters = "<Bahn>[,<Bahn2...]",
                Optional = false,
                //Default = ()=> new [] {SamaxContext.Instance.DefaultDb },
                Read = (p, args, i) =>
                {
                    p.Course = args[i + 1];
                    return 1;
                }
            },

            new Command<Parameters>
            {
                Key = "-t",
                Parameters = "<template path>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.TemplatePath = args[i + 1];
                    return 1;
                }
            },

            new Command<Parameters>
            {
                Key = "-sf",
                Parameters = "<runners.xml>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.RunnersFile = args[i + 1];
                    return 1;
                }
            },

            new Command<Parameters>
            {
                Key = "-s",
                Parameters = "<begin startNr>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.BeginStartNr = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-e",
                Parameters = "<end startNr>",
                Optional = true,
                Read = (p, args, i) =>
                {
                  p.EndStartNr = args[i + 1];
                  return 1;
                }
            },

            new Command<Parameters>
            {
                Key = "-x",
                Parameters = "io3-course export xml",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.CourseExport = args[i + 1];
                    return 1;
                }
            },

            new Command<Parameters>
            {
                Key = "-ts",
                Parameters = "<transferSymbol>[,transferSymbol...]>",
                Optional = true,
                Read = (p, args, i) =>
                {
                  p.TransferSymbols = args[i + 1];
                  return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-p",
                Info = "(split courses)",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.SplitParameters = new Ext.SplitBuilder.Parameters();
                    return 0;
                }
            },
        };

      public string ConfigPath
      {
        get { return _configPath; }
        set { _configPath = value; _oCourseVm = null; }
      }

      private string _configPath;
      public string OutputPath { get; set; }
      public string TemplatePath { get; set; }
      public string Course { get; set; }
      public string RunnersFile { get; set; }
      public string BeginStartNr { get; set; }
      public string EndStartNr { get; set; }
      public string CourseExport { get; set; }
      public Ext.SplitBuilder.Parameters SplitParameters { get; set; }
      public bool UseAllPermutations { get; set; } = true;
      public string TransferSymbols { get; set; }

      private ViewModels.OCourseVm _oCourseVm;
      public ViewModels.OCourseVm OCourseVm => _oCourseVm;
      public Dictionary<string, List<int>> CatRunners {get; set;}

      public ViewModels.OCourseVm EnsureOCourseVm()
      {
        if (_oCourseVm == null)
        {
          if (_oCourseVm == null)
          {
            ViewModels.OCourseVm oCourseVm = new ViewModels.OCourseVm();
            oCourseVm.LoadSettings(ConfigPath);
            oCourseVm.RunInSynch = true;

            _oCourseVm = oCourseVm;
          }
        }
        return _oCourseVm;
      }
      public void Dispose()
      {
        _oCourseVm?.Dispose();
        _oCourseVm = null;
      }
      public string Validate()
      {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(RunnersFile))
        {
          if (!File.Exists(RunnersFile))
          { sb.AppendLine($"Cannot find runners file {RunnersFile}"); }
          if (!string.IsNullOrEmpty(BeginStartNr))
          { sb.AppendLine($"-s must not be specified if -sf is specified"); }
          if (!string.IsNullOrEmpty(BeginStartNr))
          { sb.AppendLine($"-e must not be specified if -sf is specified"); }

          if (!string.IsNullOrEmpty(RunnersFile))
          {
            if (File.Exists(RunnersFile))
            {
              using (TextReader reader = new StreamReader(RunnersFile))
              {
                Basics.Serializer.Deserialize(out Categories runners, reader);
                CatRunners = InitCategories(runners);
              }
            }
          }
        }

        int beginStartNr = -1;
        if (!string.IsNullOrEmpty(BeginStartNr) && !int.TryParse(BeginStartNr, out beginStartNr))
        { sb.AppendLine($"Invalid BeginStartNr '{BeginStartNr}'"); }

        int endStartNr = -1;
        if (!string.IsNullOrEmpty(EndStartNr) && !int.TryParse(EndStartNr, out endStartNr))
        { sb.AppendLine($"Invalid EndStartNr '{EndStartNr}'"); }

        if (!string.IsNullOrEmpty(TransferSymbols))
        {
          foreach (var symbol in TransferSymbols.Split(','))
          {
            if (!int.TryParse(symbol, out _))
            { sb.AppendLine($"Invalid transfersymbol '{symbol}'"); }
          }
        }

        if (!File.Exists(ConfigPath))
        { sb.AppendLine($"Unknown config File '{ConfigPath}'"); }
        else
        {
          try
          {
            EnsureOCourseVm();
            //_oCourseVm?.Dispose();
            ViewModels.OCourseVm vm = _oCourseVm;

            int vmMinNr = vm.StartNrMin;
            int vmMaxNr = vm.StartNrMax;
            bool courseKnown = true;
            foreach (var course in Course.Split(','))
            {
              if (!vm.CourseNames.Contains(course))
              {
                sb.AppendLine($"Unknown course '{course}'");
                courseKnown = false;
              }

              vm.VarBuilderType = ViewModels.VarBuilderType.All;
              vm.CourseName = course;
              while (vm.Working)
              { System.Threading.Thread.Sleep(100); }


              List<int> runners = null;
              CatRunners?.TryGetValue(course, out runners);
              if (vm.StartNrMin <= 0) vm.StartNrMin = runners?.First() ?? beginStartNr;
              if (vm.StartNrMax <= 0) vm.StartNrMax = runners?.Last() ?? endStartNr;

              if (vm.StartNrMin <= 0)
              { sb.AppendLine($"{course}: Invalid start nr Min '{vm.StartNrMin}'"); }
              if (vm.StartNrMax < vm.StartNrMin)
              { sb.AppendLine($"{course}: Invalid start nr range '[{vm.StartNrMin} - {vm.StartNrMax}]'"); }

              vm.StartNrMin = vmMinNr;
              vm.StartNrMax = vmMaxNr;
            }
            if (!courseKnown)
            { sb.AppendLine($"Available courses: {string.Concat(vm.CourseNames.Select(x => $"{x},"))}"); }
          }
          catch (Exception e)
          {
            sb.AppendLine($"Invalid config File '{ConfigPath}' : {Basics.Utils.GetMsg(e)}");
            _oCourseVm.Dispose();
            _oCourseVm = null;
          }
        }

        string error = sb.ToString();
        return error;
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

        return new BuildCommand(result);
      }
    }

    private readonly Parameters _pars;
    public BuildCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {
        var vm = _pars.OCourseVm;
        string templatePath = _pars.TemplatePath ?? vm.CourseFile;

        if (Path.GetDirectoryName(Path.GetFullPath(templatePath)) != Path.GetDirectoryName(Path.GetFullPath(_pars.OutputPath)))
        {
          TransferTemplates(templatePath);
        }
        Iof3.OCourse2Io3 io3Exporter = null;
        if (!string.IsNullOrWhiteSpace(_pars.CourseExport))
        {
          if (File.Exists(_pars.CourseExport))
          {
            using (TextReader reader = new StreamReader(_pars.CourseExport))
            {
              Basics.Serializer.Deserialize(out Iof3.CourseData courseData, reader);
              io3Exporter = Iof3.OCourse2Io3.Init(courseData, vm.CourseFile);
            }
          }
          else
          { io3Exporter = Iof3.OCourse2Io3.Init("Event", vm.CourseFile); }
        }

        foreach (var course in _pars.Course.Split(','))
        {
          vm.CourseName = course;
          while (vm.Working)
          { System.Threading.Thread.Sleep(100); }

          List<int> runners = null;
          _pars.CatRunners?.TryGetValue(course, out runners);
          if (vm.StartNrMin <= 0) vm.StartNrMin = runners?.First() ?? int.Parse(_pars.BeginStartNr);
          if (vm.StartNrMax <= 0) vm.StartNrMax = runners?.Last() ?? int.Parse(_pars.EndStartNr);

          vm.PermutationsInit(_pars.UseAllPermutations);
          while (vm.Working)
          { System.Threading.Thread.Sleep(100); }

          vm.StartNrMin = 0;
          vm.StartNrMax = 0;
          if (string.IsNullOrWhiteSpace(_pars.OutputPath))
          {
            error = null;
            return true;
          }

          IEnumerable<CostSectionlist> selectedCombs =
            CostSectionlist.GetCostSectionLists(vm.Permutations, vm.RouteCalculator, vm.LcpConfig.Resolution);

          string courseName = Ext.PermutationUtils.GetCoreCourseName(vm.Course.Name);

          using (CmdCourseTransfer cmd = new CmdCourseTransfer(_pars.OutputPath, templatePath, vm.CourseFile))
          {
            cmd.Export(selectedCombs.Select(comb => Ext.PermutationUtils.GetCourse(courseName, comb, _pars.SplitParameters)), courseName);
          }

          io3Exporter?.AddCurrentPermutations(vm);

          templatePath = _pars.OutputPath;
        }


        if (!string.IsNullOrEmpty(_pars.TransferSymbols))
        {
          TransferSymbols();
        }

        if (!string.IsNullOrWhiteSpace(_pars.CourseExport))
        {
          io3Exporter.InitMapInfo();
          io3Exporter.Export(_pars.CourseExport);
        }
      }
      catch
      {
        _pars.Dispose();
        throw;
      }
      error = null;
      return true;
    }

    private void TransferTemplates(string templatePath)
    {

      using (Ocad.OcadReader r = Ocad.OcadReader.Open(templatePath))
      {
        foreach (var idx in r.ReadStringParamIndices())
        {
          if (idx.Type != Ocad.StringParams.StringType.Template)
          { continue; }
          Ocad.StringParams.TemplatePar par = new Ocad.StringParams.TemplatePar(r.ReadStringParam(idx));
          if (Path.IsPathRooted(par.Name))
          { continue; }
          string sourcePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(templatePath)), par.Name);
          string targetPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_pars.OutputPath)), par.Name);

          FileInfo sourceInfo = new FileInfo(sourcePath);
          if (!sourceInfo.Exists)
          { continue; }
          FileInfo targetInfo = new FileInfo(targetPath);
          if (targetInfo.Exists && targetInfo.LastWriteTime == sourceInfo.LastWriteTime)
          { continue; }

          File.Copy(sourcePath, targetPath, overwrite: true);
        }
      }
    }
    private void TransferSymbols()
    {
      if (string.IsNullOrWhiteSpace(_pars.TransferSymbols))
      { return; }
      HashSet<int> symbols = new HashSet<int>(_pars.TransferSymbols.Split(',').Select(int.Parse));
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(_pars.OCourseVm.CourseFile))
      using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(_pars.OutputPath))
      {
        foreach (var elem in r.EnumMapElements())
        {
          if (!symbols.Contains(elem.Symbol))
          { continue; }
          w.Append(elem);
        }
      }
    }

    public static Dictionary<string, List<int>> InitCategories(Categories cats)
    {
      int minNr = cats.minStartNr;
      Dictionary<string, List<int>> catRunners = new Dictionary<string,List<int>>();
      foreach (var cat in cats.Categorie)
      {
        int minStartNr = minNr;
        int maxStartNr = minNr + cat.runners - 1;

        List<int> runners = new List<int>();
        for (int i = minStartNr; i <= maxStartNr; i++)
        { runners.Add(i); }
        try
        {
          catRunners.Add(cat.name, runners);
        }
        catch (Exception ex)
        { throw new InvalidOperationException($"Error adding {cat.name} {cat.startTime}", ex); }

        minNr = maxStartNr + 1;
      }
      return catRunners;
    }

    [XmlRoot]
    public class Categories
    {
      [XmlAttribute]
      public int minStartNr { get; set; }
      public string TimeFormat { get; set; }
      [XmlElement]
      public List<Categorie> Categorie { get; set; }
    }
    public class Categorie
    {
      [XmlAttribute]
      public string name { get; set; }
      [XmlAttribute]
      public int runners { get; set; }
      [XmlAttribute]
      public string startTime { get; set; }
    }
  }
}
