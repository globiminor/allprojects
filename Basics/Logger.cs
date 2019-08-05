using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Basics
{
  public static class Logger
  {
    private static string _configPath;
    private static DateTime _configModDate;
    private static DateTime _nextAccess;

    private static Dictionary<string, TraceSwitch> _switchDict;
    private static Dictionary<string, TraceSwitch> SwitchDict
    {
      get
      {
        if (_switchDict == null || _nextAccess < DateTime.Now)
        {
          if (_switchDict == null || FileChanged(_configPath, _configModDate))
          {
            _switchDict = AppendSwitchDict(_switchDict);
          }
          _nextAccess = DateTime.Now.AddSeconds(1);
        }
        return _switchDict;
      }
    }

    static bool FileChanged(string path, DateTime modDate)
    {
      if (path == null)
      { return false; }
      if (!System.IO.File.Exists(path))
      { return true; }
      if (System.IO.File.GetLastWriteTime(path) > modDate)
      { return true; }
      return false;
    }

    private static Dictionary<string, TraceSwitch> AppendSwitchDict(Dictionary<string, TraceSwitch> dict = null)
    {
      dict = dict ?? new Dictionary<string, TraceSwitch>();
      Configuration config;
      if (_configPath == null)
      {
        Assembly entry = Assembly.GetEntryAssembly();
        string loc = entry?.Location;
        if (loc == null)
        {
          if (dict.Count == 0)
          {
            TraceSwitch s = GetDefaultSwitch();
            dict.Add(s.DisplayName, s);
          }
          return dict;
        }

        config = ConfigurationManager.OpenExeConfiguration(loc);
        _configPath = config.FilePath;
      }
      else
      {
        ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
        configFile.ExeConfigFilename = _configPath;
        config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
      }
      _configModDate = System.IO.File.GetLastWriteTime(_configPath);

      ConfigurationSection diagnostics = config.GetSection("system.diagnostics");

      return AppendSwitchDict(dict, diagnostics);
    }

    private static TraceSwitch GetDefaultSwitch()
    {
      TraceSwitch s = new TraceSwitch("Default", string.Empty, "1");
      return s;
    }

    private static Dictionary<string, TraceSwitch> AppendSwitchDict(Dictionary<string, TraceSwitch> switchDict, ConfigurationSection diagnostics)
    {
      foreach (var switchElem in GetSwitches(diagnostics))
      {
        TraceSwitch traceSwitch = GetTraceSwitch(switchElem);
        if (traceSwitch != null)
        { switchDict[traceSwitch.DisplayName] = traceSwitch; }
      }
      return switchDict;
    }


    public static bool Refresh(Configuration config)
    {
      try
      {
        ConfigurationSection diagnostics = config.GetSection("system.diagnostics");

        ReadDiagnosticsSection(diagnostics);

        return true;
      }
      catch
      {
        return false;
      }
    }

    private static void ReadDiagnosticsSection(ConfigurationSection diagnostics)
    {
      Dictionary<string, TraceListener> listenerDict = GetTraceListeners(diagnostics);

      AppendSwitchDict(SwitchDict, diagnostics);

      for (int i = 0; i < Trace.Listeners.Count; i++)
      {
        TraceListener oldListener = Trace.Listeners[i];
        if (listenerDict.TryGetValue(oldListener.Name, out TraceListener newListener))
        {
          listenerDict.Remove(newListener.Name);
          oldListener.Dispose();
          Trace.Listeners[i] = newListener;
        }
      }
      foreach (TraceListener newListener in listenerDict.Values)
      { Trace.Listeners.Add(newListener); }
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


    #region read config section
    private static Dictionary<string, TraceListener> GetTraceListeners(ConfigurationSection diagnostics)
    {
      if (diagnostics == null)
      { return null; }

      PropertyInformation prop = diagnostics.ElementInformation.Properties["trace"];
      if (prop == null)
      { return null; }

      return GetTraceListeners(prop.Value as ConfigurationElement);
    }
    private static Dictionary<string, TraceListener> GetTraceListeners(ConfigurationElement trace)
    {
      if (trace == null)
      { return null; }

      PropertyInformation indentProp = trace.ElementInformation.Properties["indentsize"];
      if (indentProp != null && int.TryParse(indentProp.Value.ToString(), out int size))
      { Trace.IndentSize = size; }

      PropertyInformation flushProp = trace.ElementInformation.Properties["autoflush"];
      if (flushProp != null && bool.TryParse(flushProp.Value.ToString(), out bool autoflush))
      { Trace.AutoFlush = autoflush; }


      PropertyInformation lisProp = trace.ElementInformation.Properties["listeners"];
      if (lisProp == null)
      { return null; }
      return GetTraceListeners(lisProp.Value as ConfigurationElementCollection);
    }
    private static Dictionary<string, TraceListener> GetTraceListeners(ConfigurationElementCollection listeneres)
    {
      if (listeneres == null)
      { return null; }

      Dictionary<string, TraceListener> dict = new Dictionary<string, TraceListener>(listeneres.Count);

      foreach (ConfigurationElement listener in listeneres)
      {
        TraceListener traceListener = GetTraceListener(listener);
        if (traceListener != null)
        { dict[traceListener.Name] = traceListener; }
      }
      return dict;
    }
    private static TraceListener GetTraceListener(ConfigurationElement listener)
    {
      PropertyInformation typeProp = listener.ElementInformation.Properties["type"];
      if (typeProp == null) { return null; }
      if (!(typeProp.Value is string typeName))
      { return null; }

      Type type = Type.GetType(typeName);
      if (type == null)
      { type = typeof(TraceListener).Assembly.GetType(typeName); }
      if (type == null)
      { return null; }

      PropertyInformation initProp = listener.ElementInformation.Properties["initializeData"];
      if (initProp == null)
      { return null; }

      if (type == typeof(DefaultTraceListener))
      { return null; }
      TraceListener traceLis = Activator.CreateInstance(type, initProp.Value) as TraceListener;
      PropertyInformation nameProp = listener.ElementInformation.Properties["name"];
      if (nameProp != null)
      { traceLis.Name = nameProp.Value as string; }

      return traceLis;
    }

    private static IEnumerable<ConfigurationElement> GetSwitches(ConfigurationSection diagnostics)
    {
      PropertyInformation prop = diagnostics?.ElementInformation.Properties["switches"];
      return GetSwitches(prop?.Value as ConfigurationElementCollection);
    }
    private static IEnumerable<ConfigurationElement> GetSwitches(ConfigurationElementCollection switches)
    {
      if (switches == null)
      { yield break; }

      foreach (ConfigurationElement switchElem in switches)
      {
        yield return switchElem;
      }
    }
    private static TraceSwitch GetTraceSwitch(ConfigurationElement switchElem)
    {
      PropertyInformation nameProp = switchElem.ElementInformation.Properties["name"];
      string name = nameProp?.Value as string;

      PropertyInformation valueProp = switchElem.ElementInformation.Properties["value"];
      string value = valueProp?.Value as string;

      TraceSwitch traceSwitch = new TraceSwitch(name, "", value);
      traceSwitch.Level = (TraceLevel)int.Parse(value);

      return traceSwitch;
    }
    #endregion

  }
}
