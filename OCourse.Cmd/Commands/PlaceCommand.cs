﻿using Basics.Cmd;
using OCourse.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OCourse.Cmd.Commands
{
  public class PlaceCommand : ICommand
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

        };

      public string CoursePath { get; set; }
      public int ControlNrOverprintSymbol { get; set; }

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

        return new PlaceCommand(result);
      }
    }

    private readonly Parameters _pars;
    public PlaceCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {
        foreach (var mapFile in Directory.EnumerateFiles(Path.GetDirectoryName(_pars.CoursePath), Path.GetFileName(_pars.CoursePath)))
				{
          CourseMap cm = new CourseMap();
          cm.InitFromFile(mapFile);
          cm.ControlNrOverprintSymbol = _pars.ControlNrOverprintSymbol;

          string ext = Path.GetExtension(mapFile);
          string dir = Path.GetDirectoryName(mapFile);
          string name = Path.GetFileNameWithoutExtension(mapFile);
          string result = Path.Combine(dir, $"{name}.p{ext}");

          File.Copy(mapFile, result, overwrite: true);

          using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(result))
          {
            CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
            cmd.Execute();
          }
        }
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
