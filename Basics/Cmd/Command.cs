
using System;
using System.Collections.Generic;

namespace Basics.Cmd
{
  public interface ICommand
  {
    bool Execute(out string error);
  }

  /// <summary>
  ///     Base class for interpreting command lines in a console application
  ///     <para>Remark: Move to "base assembly" if more console applications are implemented</para>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Command<T>
  {
    /// <summary>
    ///     Key of the command, typically -&lt;letter&gt;, i.e -c
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     Method that read the parameters from the command line args in to the command properties T.
    ///     <para>Args</para>
    ///     <para>T: instance of command properties</para>
    ///     <para>IList&lt;string&gt;: ALL arguments of the command line</para>
    ///     <para>int: position, where the interpretation of the command line arguments starts</para>
    ///     <para>return value: number of arguments used, appart from the key arg</para>
    /// </summary>
    public Func<T, IList<string>, int, int> Read { get; set; }

    /// <summary>
    ///     Description for parameters following Key.
    ///     Used mainly for user info.
    /// </summary>
    public string Parameters { get; set; }

    public bool Optional { get; set; }

    public Func<string[]> Default { get; set; }
    public string Info { get; set; }

    /// <summary>
    /// </summary>
    /// <param name="t">object, where thre arguments are interpreted</param>
    /// <param name="args">All command line arguments</param>
    /// <param name="position">position where to start the interpretation</param>
    /// <returns>
    ///     false: position is not changed and args[position] != Key
    ///     <para>
    ///         true: args[position] == Key, needed arguments are read into t, and position gets changed to the next not
    ///         interpreted argument
    ///     </para>
    /// </returns>
    public bool TryApply(T t, IList<string> args, ref int position)
    {
      if (args.Count <= position)
      { return false; }
      if (args[position] != Key)
      { return false; }

      int nUsed = Read(t, args, position);
      position += nUsed + 1;
      return true;
    }
  }
}
