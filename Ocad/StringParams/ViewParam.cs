
namespace Ocad.StringParams
{
  /// <summary>
  /// Summary description for ViewParam.
  /// </summary>
  public class ViewPar : SingleParam
  {
    public const char ViewModeKey = 'v';
    public const char XOffsetKey = 'x';
    public const char YOffsetKey = 'y';

    public ViewPar()
    { }
    public ViewPar(string stringParam)
      : base(stringParam)
    { }

    public int ViewMode
    {
      get { return GetInt(ViewModeKey); }
      set { SetParam(ViewModeKey, value); }
    }
    public double XOffset
    {
      get { return GetDouble(XOffsetKey); }
      set { SetParam(XOffsetKey, value); }
    }
    public double YOffset
    {
      get { return GetDouble(YOffsetKey); }
      set { SetParam(YOffsetKey, value); }
    }
  }
}
