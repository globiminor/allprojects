using Basics.Cmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OCourse.Cmd.Commands
{
  public class ExportCommand : ICommand
  {
    public class Parameters : IDisposable
    {
      private static readonly List<Command<Parameters>> _cmds = new List<Command<Parameters>>
        {
            new Command<Parameters>
            {
                Key = "-c",
                Parameters = "<course path>",
                Read = (p, args, i) =>
                {
                    p.CoursePath = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-s",
                Parameters = "<script path>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.ScriptPath = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-o",
                Info = "overwrite",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.Overwrite = true;
                    return 0;
                }
            },
            new Command<Parameters>
            {
                Key = "-hb",
                Info = "hide background",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.HideBackground = true;
                    return 0;
                }
            },
            new Command<Parameters>
            {
                Key = "-hf",
                Info = "hide foreground",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.HideForeground = true;
                    return 0;
                }
            },
        };

      public string CoursePath { get; set; }
      public string ScriptPath { get; set; }
      public bool Overwrite { get; set; }
      public bool HideBackground { get; set; }
      public bool HideForeground { get; set; }

      public void Dispose()
      { }
      public string Validate()
      {
        StringBuilder sb = new StringBuilder();
        string error = sb.ToString();

        return sb.ToString();
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

        return new ExportCommand(result);
      }
    }

    private readonly Parameters _pars;
    public ExportCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {

        bool success = false;
        bool overwrite = _pars.Overwrite;
        while (success == false)
        {
          success = true;
          IList<string> exports = Directory.EnumerateFiles(Path.GetDirectoryName(_pars.CoursePath), Path.GetFileName(_pars.CoursePath))
            .Select(x => Path.GetFullPath(x)).ToList();
          List<string> maxExports = new List<string>();
          foreach (var export in exports)
          {
            string pdfPath = Path.ChangeExtension(export, ".pdf");
            if (File.Exists(pdfPath))
            {
              if (!overwrite)
              { continue; }

            }
            maxExports.Add(export);
          }

          success &= ExportCourses(maxExports);
          overwrite = false;
        }
      }
      finally
      {
        _pars.Dispose();
      }
      error = null;
      return true;
    }

    private bool ExportCourses(List<string> exports)
    {
      if (exports.Count <= 0)
      { return true; }
      string scriptFile = _pars.ScriptPath ?? Path.Combine(Path.GetDirectoryName(exports.First()), "createPdfs.xml");
      string defaultExe;
      string lastPdf;
      string firstOcd;
      HashSet<string> tempFiles = new HashSet<string>();
      using (Ocad.Scripting.Script s = new Ocad.Scripting.Script(scriptFile))
      {
        defaultExe = CreatePdfScript(exports, s);
        firstOcd = s.ScriptNode.FirstChild.FirstChild.InnerText;
        var lastExportNode = s.ScriptNode.LastChild.PreviousSibling.PreviousSibling;
        lastPdf = lastExportNode.FirstChild.InnerText;

        foreach (System.Xml.XmlNode node in s.ScriptNode.ChildNodes)
        {
          if (node.Name == Ocad.Scripting.Script.ExportBySymbol_Name)
          {
            tempFiles.Add(node.FirstChild.InnerText);
          }
        }
      }

      string procName = new Ocad.Scripting.Utils { Exe = defaultExe }.RunScript(scriptFile, defaultExe, wait: 20000);
      int nPrePdf = 0;
      bool success = true;
      while (!File.Exists(lastPdf))
      {
        System.Threading.Thread.Sleep(4000);
        int nPdf = Directory.GetFiles(Path.GetDirectoryName(lastPdf), "*.pdf").Length;
        if (nPdf == nPrePdf)
        {
          success = false;
          break;
        }
        nPrePdf = nPdf;
      }
      Ocad.Scripting.Utils.CloseOcadProcs(defaultExe, force: true);
      foreach (var tempFile in tempFiles)
      {
        if (File.Exists(tempFile))
        { File.Delete(tempFile); }
      }
      return success;
    }

    private string CreatePdfScript(IEnumerable<string> files, Ocad.Scripting.Script expPdf)
    {
      string exe = null;
      string tempName = null;

      if (_pars.HideForeground)
      {
        tempName = $"{Guid.NewGuid()}.ocd";
      }
      foreach (var ocdFile in files)
      {
        exe = exe ?? Basics.Utils.FindExecutable(ocdFile);
        string pdfPath = Path.ChangeExtension(ocdFile, "pdf");

        using (Ocad.Scripting.Node node = expPdf.FileOpen())
        { node.File(ocdFile); }
        if (_pars.HideForeground)
        {
          string tempFile = Path.Combine(Path.GetDirectoryName(pdfPath), tempName);
          using (Ocad.Scripting.Node node = expPdf.ExportBySymbol())
          {
            node.File(tempFile);
            node.SymbolNumber(100.000); // TODO: verify symbol is not used!
          }

          using (Ocad.Scripting.Node node = expPdf.FileClose())
          { node.Enabled(true); }
          using (Ocad.Scripting.Node node = expPdf.FileOpen())
          { node.File(tempFile); }
        }
        if (_pars.HideBackground)
        {
          using (Ocad.Scripting.Node node = expPdf.BackgroundMapRemove())
          { }
        }
        using (Ocad.Scripting.Node node = expPdf.FileExport())
        {
          node.File(pdfPath);
          node.Format(Ocad.Scripting.Format.Pdf);
        }
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
  }
}
