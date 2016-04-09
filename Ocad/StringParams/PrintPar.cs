
namespace Ocad.StringParams
{
  /// <summary>
  /// Summary description for PrintPar
  /// </summary>
  public class PrintPar : SingleParam
  {
    public enum RangeType 
    {
      EntireMap = 0,PartialMap = 1, OnePage = 2
    }

    public const char ScaleKey = 'a';
    public const char RangeKey = 'r';
    public const char LeftKey = 'L';
    public const char RightKey = 'R';
    public const char TopKey = 'T';
    public const char BottomKey = 'B';

    public PrintPar()
    {	}
    public PrintPar(string stringParam) : base(stringParam)
    { }
    public double Scale
    {
      get{ return GetDouble(ScaleKey); }
      set{ SetParam(ScaleKey, value); }
    }
    public RangeType Range
    {
      get{ return (RangeType) GetInt(RangeKey); }
      set{ SetParam(RangeKey, (int) value); }
    }

    public double Left
    {
      get{ return GetDouble(LeftKey); }
      set{ SetParam(LeftKey, value); }
    }
    public double Right
    {
      get{ return GetDouble(RightKey); }
      set{ SetParam(RightKey, value); }
    }
    public double Top
    {
      get{ return GetDouble(TopKey); }
      set{ SetParam(TopKey, value); }
    }
    public double Bottom
    {
      get{ return GetDouble(BottomKey); }
      set{ SetParam(BottomKey, value); }
    }
  }
}
