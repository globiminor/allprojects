namespace Ocad.StringParams
{
  public class ExportPar : MultiParam
  {
    public const char ResolutionKey = 'r';
    public ExportPar()
    { }
    public ExportPar(string stringParam)
      : base(stringParam)
    { }
    public double Resolution
    {
      get { return GetDouble(ResolutionKey); }
      set { SetParam(ResolutionKey, value); }
    }
  }
}
