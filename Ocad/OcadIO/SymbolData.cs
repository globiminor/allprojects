namespace Ocad
{
  public class SymbolData
  {
    private int _iColorCount;
    private int _iColorSepCount;
    private int _iCyanFreq;
    private int _iCyanAngle;
    private int _iMagentaFreq;
    private int _iMagentaAngle;
    private int _iYellowFreq;
    private int _iYellowAngle;
    private int _iBlackFreq;
    private int _iBlackAngle;

    public int ColorCount
    {
      get
      { return _iColorCount; }
      set
      { _iColorCount = value; }
    }

    public int ColorSeparationCount
    {
      get
      { return _iColorSepCount; }
      set
      { _iColorSepCount = value; }
    }

    public int CyanFreq
    {
      get
      { return _iCyanFreq; }
      set
      { _iCyanFreq = value; }
    }

    public int CyanAngle
    {
      get
      { return _iCyanAngle; }
      set
      { _iCyanAngle = value; }
    }

    public int MagentaFreq
    {
      get
      { return _iMagentaFreq; }
      set
      { _iMagentaFreq = value; }
    }

    public int MagentaAngle
    {
      get
      { return _iMagentaAngle; }
      set
      { _iMagentaAngle = value; }
    }

    public int YellowFreq
    {
      get
      { return _iYellowFreq; }
      set
      { _iYellowFreq = value; }
    }

    public int YellowAngle
    {
      get
      { return _iYellowAngle; }
      set
      { _iYellowAngle = value; }
    }

    public int BlackFreq
    {
      get
      { return _iBlackFreq; }
      set
      { _iBlackFreq = value; }
    }

    public int BlackAngle
    {
      get
      { return _iBlackAngle; }
      set
      { _iBlackAngle = value; }
    }

    public override string ToString()
    {
      return string.Format(
        "Number of Colors      : {0,5}\n" +
        "Number of Separations : {1,5}\n\n" +
        " Cyan Frequncy        : {2,5}\n" +
        " Cyan Angle           : {3,5}\n" +
        " Magenta Frequency    : {4,5}\n" +
        " Magenta Angle        : {5,5}\n" +
        " Yellow Frequency     : {6,5}\n" +
        " Yellow Angle         : {7,5}\n" +
        " Black Frequency      : {8,5}\n" +
        " Black Angle          : {9,5}", _iColorCount, _iColorSepCount,
        _iCyanFreq, _iCyanAngle, _iMagentaFreq, _iMagentaAngle,
        _iYellowFreq, _iYellowAngle, _iBlackFreq, _iBlackAngle);
    }
  }
}
