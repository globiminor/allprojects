using Basics.Geom;

namespace TData
{
  /// <summary>
  /// Summary description for Data
  /// </summary>
  public abstract class Data
  {
    #region nested classes

    #endregion

    private string _path;

    public abstract IBox Extent { get; }
    public string Path
    {
      get
      { return _path; }
      protected set
      { _path = value; }
    }

    public string Name
    {
      get
      {
        if (_path == null)
        { return null; }
        return System.IO.Path.GetFileNameWithoutExtension(_path);
      }
    }
  }
}
