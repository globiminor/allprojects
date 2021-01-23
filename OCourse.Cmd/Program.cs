using Basics.Cmd;
using System;
using System.Collections.Generic;

namespace OCourse.Cmd
{
	class Program
	{
		static void Main(string[] args)
		{
      Run(args);
      while (true)
      {
        Console.Write("OCourse> ");
        string line = Console.ReadLine();
        if (line != null && line.Trim().ToLower() == "exit")
          return;
        Run(Basics.Cmd.Utils.CommandLineToArgs(line));
      }
    }

    private static Dictionary<string, Func<IList<string>, ICommand>> _commands = new Dictionary<string, Func<IList<string>, ICommand>>
    {
      { "build", Commands.BuildParameters.ReadArgs },
    };

    public static bool Run(IList<string> args)
    {
      return Basics.Cmd.Utils.Run(args, _commands);
    }

  }
}
