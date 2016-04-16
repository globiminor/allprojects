using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace TMapWin
{
  public class Settings
  {
    private static Settings _default;
    public static Settings Default
    {
      get 
      { 
        if (_default == null)
        {
          string file = DefaultFilePath(typeof(Settings));
          _default = Create(file);
        }
        return _default; 
      }
    }
    public static Settings Create(string file)
    {
      Settings settings = null;

      if (File.Exists(file))
      {
        XmlSerializer xml = new XmlSerializer(typeof(Settings));
        try
        {
          TextReader txt = new StreamReader(file);
          settings = (Settings)xml.Deserialize(txt);
          txt.Close();
        }
        catch { }
      }
      if (settings == null)
      {
        settings = new Settings();
      }
      settings._pathName = file;

      return settings;
    }


    private string _pathName;

    private Attribute _plugins;
    private Point _wdgMainLocation;
    private Size _wdgMainSize;
    private string _browsePath;

    public Attribute Plugins
    {
      get { return _plugins; }
      set { _plugins = value; }
    }
    public Point WdgMain_Location
    {
      get { return _wdgMainLocation; }
      set { _wdgMainLocation = value; }
    }
    public Size WdgMain_Size
    {
      get { return _wdgMainSize; }
      set { _wdgMainSize = value; }
    }
    public string BrowsePath
    {
      get { return _browsePath; }
      set { _browsePath = value; }
    }
    public void Save()
    {
      string dir = Path.GetDirectoryName(_pathName);

      if (Directory.Exists(dir) == false)
      {
        Directory.CreateDirectory(dir);
      }

      TextWriter txt = new StreamWriter(_pathName);

      XmlSerializer xml = new XmlSerializer(GetType());

      xml.Serialize(txt, this);
      txt.Close();
    }

    private static string DefaultFilePath(Type type)
    {
      string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string assembly = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0];
      string file = type.Name; 
      dir = Path.Combine(dir, assembly);
      file = Path.Combine(dir, file + ".config");

      return file;
    }
  }
}
