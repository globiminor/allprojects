namespace Ocad
{
  /// <summary>
  /// Summary description for Color.
  /// </summary>
  public class Color
  {
    private byte _cyan;
    private byte _magenta;
    private byte _yellow;
    private byte _black;

    internal Color()
    { }
    public Color(byte cyan, byte magenta, byte yellow, byte black)
    {
      _cyan = cyan;
      _magenta = magenta;
      _yellow = yellow;
      _black = black;
    }
    public byte Black
    {
      get { return _black; }
      set { _black = value; }
    }

    public byte Cyan
    {
      get { return _cyan; }
      set { _cyan = value; }
    }

    public byte Magenta
    {
      get { return _magenta; }
      set { _magenta = value; }
    }

    public byte Yellow
    {
      get { return _yellow; }
      set { _yellow = value; }
    }


    public int ToNumber()
    {
      return GetByte(Cyan) + 256 * (GetByte(Magenta) + 256 * (GetByte(Yellow) + 256 * GetByte(Black)));
    }

    private int GetByte(int percent)
    {
      return (int)(255 * percent / 100.0);
    }
  }

  public class ColorInfo
  {
    private int _nummer;
    private int _res;
    private Color _color;
    private string _name;
    private string msSepPercentage;

    public ColorInfo()
    {
      _color = new Color();
    }

    public ColorInfo(StringParams.ColorPar colorPar)
    {
      _name = colorPar.Name;
      _nummer = colorPar.Number;
      _color = new Color((byte)colorPar.Cyan, (byte)colorPar.Magenta,
        (byte)colorPar.Yellow, (byte)colorPar.Black);

    }

    public Color Color
    {
      get
      { return _color; }
      set
      { _color = value; }
    }

    public string Name
    {
      get
      { return _name; }
      set
      { _name = value; }
    }

    public int Nummer
    {
      get
      { return _nummer; }
      set
      { _nummer = value; }
    }

    public int Resolution
    {
      get
      { return _res; }
      set
      { _res = value; }
    }

    public string SepPercentage
    {
      get
      { return msSepPercentage; }
      set
      { msSepPercentage = value; }
    }

    public override string ToString()
    {
      return string.Format(
        "  num     = {0,5}\n" +
        "  cyan    = {1,3}\n" +
        "  magenta = {2,3}\n" +
        "  yellow  = {3,3}\n" +
        "  black   = {4,3}\n" +
        "  name    = {5}",
        _nummer, _color.Cyan, _color.Magenta,
        _color.Yellow, _color.Black, _name);
    }
  }

  public class ColorSeparation
  {
    private string msName;
    private Color mpColor;
    private int miRasterFreq;
    private int miRasterAng;

    public ColorSeparation()
    {
      mpColor = new Color();
    }

    public Color Color
    {
      get
      { return mpColor; }
      set
      { mpColor = value; }
    }

    public string Name
    {
      get
      { return msName; }
      set
      { msName = value; }
    }

    public int RasterFreq
    {
      get
      { return miRasterFreq; }
      set
      { miRasterFreq = value; }
    }

    public int RasterAngle
    {
      get
      { return miRasterAng; }
      set
      { miRasterAng = value; }
    }

    public override string ToString()
    {
      return string.Format(
        "  name    = {0}\n" +
        "  cyan    = {1,3}\n" +
        "  magenta = {2,3}\n" +
        "  yellow  = {3,3}\n" +
        "  black   = {4,3}\n" +
        "  Raster frequency = {5,5}\n" +
        "  Raster angle     = {6,5}",
        msName, mpColor.Cyan, mpColor.Magenta, mpColor.Yellow, mpColor.Black,
        miRasterFreq, miRasterAng);
    }
  }
}
