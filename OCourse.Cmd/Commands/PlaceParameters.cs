using Basics.Cmd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCourse.Cmd.Commands
{
  public class PlaceParameters : IDisposable
  {
    private static readonly List<Command<PlaceParameters>> _cmds = new List<Command<PlaceParameters>>
        {
            new Command<PlaceParameters>
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
      PlaceParameters result = new PlaceParameters();

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
}
