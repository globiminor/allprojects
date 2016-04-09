
namespace Ocad.StringParams
{
  public class ColorPar : MultiParam
  {
    public const char NumberKey = 'n';
    public const char CyanKey = 'c';
    public const char MagentaKey = 'm';
    public const char YellowKey = 'y';
    public const char BlackKey = 'k';
    public const char OverprintKey = 'o';
    public const char TransparencyKey = 't';

    public const char SpotColorSeparationNameKey = 's';
    public const char PercentageSpotColorKey = 'p';

    public ColorPar(string para)
      : base(para)
    { }

    public int Number
    {
      get { return GetInt_(NumberKey).Value; }
      set { SetParam(NumberKey, value); }
    }

    public int Cyan
    {
      get { int? c = GetInt_(CyanKey); return c ?? 0; }
      set { SetParam(CyanKey, value); }
    }

    public int Magenta
    {
      get { int? m = GetInt_(MagentaKey); return m ?? 0; }
      set { SetParam(MagentaKey, value); }
    }
    public int Yellow
    {
      get { int? y = GetInt_(YellowKey); return y ?? 0; }
      set { SetParam(YellowKey, value); }
    }

    public int Black
    {
      get { int? b = GetInt_(BlackKey); return b ?? 0; }
      set { SetParam(BlackKey, value); }
    }
  }
}
