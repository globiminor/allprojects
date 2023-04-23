using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Basics
{
  public static class Logger
  {
    private static string? _configPath;
    private static DateTime _configModDate;
    private static DateTime _nextAccess;

    private static Dictionary<string, TraceSwitch>? _switchDict;
    private static Dictionary<string, TraceSwitch> SwitchDict
    {
      get
      {
        // TODO
        return _switchDict;
      }
    }

    static bool FileChanged(string? path, DateTime modDate)
    {
      if (path == null)
      { return false; }
      if (!System.IO.File.Exists(path))
      { return true; }
      if (System.IO.File.GetLastWriteTime(path) > modDate)
      { return true; }
      return false;
    }

    private static TraceSwitch GetDefaultSwitch()
    {
      TraceSwitch s = new TraceSwitch("Default", string.Empty, "1");
      return s;
    }

    public static void Exception(Exception exp, string switchName = null, string stackIgnore = null)
    {
      if (GetSwitch(switchName).TraceError)
      {
        Trace.Write(exp);
        DateTime t = DateTime.Now;
        StringBuilder msg = new StringBuilder();
        msg.AppendLine();
        msg.AppendFormat("{0} {1}, {2} ", t.ToShortDateString(), t.ToLongTimeString(), Environment.UserName);
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
        Version version = assembly.GetName().Version;
        msg.AppendFormat($"[{assembly.FullName} {version}]");
        msg.AppendLine();
        msg.Append(Utils.GetMsg(exp, stackIgnore));
        msg.AppendLine();
        Trace.WriteLine(msg.ToString());
        Trace.Flush();
      }
    }

    public static void Warn(Func<string> message, string switchName = null)
    {
      if (GetSwitch(switchName).TraceWarning)
      {
        Trace.WriteLine(message());
        Trace.Flush();
      }
    }
    public static void Warn(Exception exp, string switchName = null, string stackIgnore = null)
    {
      if (GetSwitch(switchName).TraceWarning)
      {
        Trace.Write(Utils.GetMsg(exp, stackIgnore));
        Trace.Flush();
      }
    }

    public static void Info(Func<string> message, string switchName = null)
    {
      if (GetSwitch(switchName).TraceInfo)
      {
        Trace.WriteLine(message());
        Trace.Flush();
      }
    }

    public static void Verbose(Func<string> message, string switchName = null)
    {
      TraceSwitch s = GetSwitch(switchName);
      if (s.TraceVerbose)
      {
        Trace.Write(message());
        Trace.Flush();
      }
    }

    private static TraceSwitch GetSwitch(string name)
    {
      if (name == null || !SwitchDict.TryGetValue(name, out TraceSwitch traceSwitch))
      {
        traceSwitch = SwitchDict.Values.FirstOrDefault() ?? GetDefaultSwitch();
      }

      return traceSwitch;
    }
  }
}
