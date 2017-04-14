namespace Ocad.StringParams
{
  public class CoordSystemPar : MultiParam
  {
    public const char KeyDatumId = 'd';
    public const char KeyEllipsoidId = 'e';
    public const char KeyEllipsoidAxis = 'x';
    public const char KeyEllipsoidFlattening = 't';
    public const char KeyProjectionId = 'r';
    public const char KeyFalseEasting = 'm';
    public const char KeyFalseNorthing = 'n';
    public const char KeyScaleFactor = 'f';
    public const char KeyCentralMeridian = 'c';
    public const char KeyOriginLongitude = 'o';
    public const char KeyOriginLatitude = 'a';
    public const char KeyStandardParallel1 = 's';
    public const char KeyStandardParallel2 = 'p';
    public const char KeyLocation = 'l';
    public const char KeyAzimuth = 'z';
    public const char KeyEPSGCode = 'g';

    public CoordSystemPar()
    { }
    public CoordSystemPar(string stringParam)
      : base(stringParam)
    { }

  }
}
