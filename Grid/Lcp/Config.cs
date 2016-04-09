using System.IO;

namespace Grid.Lcp
{
  public class Config
  {
    private string _velo;
    private string _dhm;
    private double _resolution;

    private string _routes;

    public static string GetFullPath(string path, string value)
    {
      string fullPath;
      if (Path.IsPathRooted(value))
      { fullPath = value; }
      else
      { fullPath = Path.Combine(path, value); }
      return fullPath;
    }

    public string Velo
    {
      get { return _velo; }
      set { _velo = value; }
    }

    public string Dhm
    {
      get { return _dhm; }
      set { _dhm = value; }
    }

    public double Resolution
    {
      get { return _resolution; }
      set { _resolution = value; }
    }

    public string Routes
    {
      get { return _routes; }
      set { _routes = value; }
    }
  }
}
