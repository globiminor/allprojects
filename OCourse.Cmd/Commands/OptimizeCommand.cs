using Basics.Cmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OCourse.Cmd.Commands
{
  public class OptimizeCommand : ICommand
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
                Key = "-nw",
                Info = "Do not wait for finish",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.Wait = false;
                    return 0;
                }
            },

        };

      public string CoursePath { get; set; }
      public string ScriptPath { get; set; }
      public bool Wait { get; set; } = true;

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

        return new OptimizeCommand(result);
      }
    }

    private readonly Parameters _pars;
    public OptimizeCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {
        string dir = Path.GetDirectoryName(_pars.CoursePath);
        IList<string> exports = Directory.EnumerateFiles(Path.GetDirectoryName(_pars.CoursePath), Path.GetFileName(_pars.CoursePath))
          .Select(x => Path.GetFullPath(x)).ToList();
        string scriptPath = _pars.ScriptPath ?? Path.Combine(dir, "optimize.xml");
        Ocad.Scripting.Utils.Optimize(exports, scriptPath, wait: _pars.Wait);
      }
      finally
      {
        _pars.Dispose();
      }
      error = null;
      return true;
    }
  }

}
