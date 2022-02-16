using Basics;
using Basics.Cmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OCourse.Cmd.Commands
{
  /// <summary>
  /// Validate Controls in OCAD against IO3-CourseExport
  /// </summary>
  public class JoinCommand : ICommand
  {
    public class Parameters : IDisposable
    {
      private static readonly List<Command<Parameters>> _cmds = new List<Command<Parameters>>
        {
            new Command<Parameters>
            {
                Key = "-f",
                Parameters = "<foreground>",
                Info ="<foreground> can have wildcards",
                Read = (p, args, i) =>
                {
                    p.Foreground = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-b",
                Parameters = "<background>",
                Read = (p, args, i) =>
                {
                    p.Background = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-f2",
                Parameters = "<Foreground reverse side>",
                Read = (p, args, i) =>
                {
                    p.ForegroundReverse = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-b2",
                Parameters = "<Background reverse side>",
                Read = (p, args, i) =>
                {
                    p.BackgroundReverse = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-e",
                Parameters = "<joined file>",
                Read = (p, args, i) =>
                {
                    p.JoinedFile = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-fk",
                Parameters = "<front key>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.FrontKey = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-rk",
                Parameters = "<reverse key>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.ReverseKey = args[i + 1];
                    return 1;
                }
            },
        };

      public string Foreground { get; set; }
      public string Background { get; set; }
      public string ForegroundReverse { get; set; }
      public string BackgroundReverse { get; set; }
      public string JoinedFile { get; set; }
      public string FrontKey { get; set; }
      public string ReverseKey { get; set; }
      public void Dispose()
      { }

      public string Validate()
      {
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

        return new JoinCommand(result);
      }
    }

    private readonly Parameters _pars;
    public JoinCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      bool success = true;
      StringBuilder sb = null;
      Dictionary<string, List<Comb>> combsDict = new Dictionary<string, List<Comb>>();
      try
      {
        TemplateUtils tmpl = new TemplateUtils(_pars.Foreground);
        string dir = Path.GetDirectoryName(_pars.Foreground);
        foreach (string foreground in Directory.GetFiles(dir, tmpl.Template))
        {
          string background = tmpl.GetReplaced(foreground, _pars.Background);
          if (!File.Exists(background))
          {
            Console.WriteLine($"Background {background} not found for {foreground}, skipping..");
            continue;
          }

          string foreReverse = tmpl.GetReplaced(foreground, _pars.ForegroundReverse);
          if (!File.Exists(foreReverse))
          {
            Console.WriteLine($"Foreground on reverse side {foreReverse} not found for {foreground}, skipping..");
            continue;
          }

          string backReverse = tmpl.GetReplaced(foreground, _pars.BackgroundReverse);
          if (!File.Exists(backReverse))
          {
            Console.WriteLine($"Background on reverse side {foreReverse} not found for {foreground}, skipping..");
            continue;
          }

          string joined = tmpl.GetReplaced(foreground, _pars.JoinedFile);

          List<Comb> combs = combsDict.GetOrCreateValue(joined);
          combs.Add(new Comb(foreground, background, foreReverse, backReverse, joined));
        }

        foreach (var pair in combsDict)
        {
          string joined = pair.Key;

          if (File.Exists(joined))
          { File.Delete(joined); }

          List<Comb> combs = pair.Value;
          List<string> files = new List<string>();
          string background = null;
          string backReverse = null;
          bool joinSuccess = true;
          foreach (var comb in combs)
          {
            if (background == null)
            { background = comb.Background ?? string.Empty; }
            else if (background != comb.Background)
            {
              Console.WriteLine($"Found different backgrounds ({background}, {comb.Background}) for {joined}, skipping ...");
              joinSuccess = false;
              break;
            }
            if (backReverse == null)
            { backReverse = comb.BackReverse ?? string.Empty; }
            else if (backReverse != comb.BackReverse)
            {
              Console.WriteLine($"Found different reverse side backgrounds ({backReverse}, {comb.BackReverse}) for {joined}, skipping ... ");
              joinSuccess = false;
              break;
            }

            files.Add(comb.Foreground);
            files.Add(comb.ForeReverse);
          }

          if (!joinSuccess)
          { continue; }

          OTextSharp.Models.PdfText.OverprintFrontBack(joined, files, background, backReverse,
            frontKey: _pars.FrontKey, reverseKey: _pars.ReverseKey);
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

    private class Comb
    {
      public Comb(string foreground, string background, string foreReverse, string backReverse, string joined)
      {
        Foreground = foreground;
        Background = background;
        ForeReverse = foreReverse;
        BackReverse = backReverse;
        Joined = joined;
      }

      public string Foreground { get; }
      public string Background { get; }
      public string ForeReverse { get; }
      public string BackReverse { get; }
      public string Joined { get; }
    }
  }
}
