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
      return Cyan + 256 * (Magenta + 256 * (Yellow + 256 * Black));
      //return GetByte(Cyan) + 256 * (GetByte(Magenta) + 256 * (GetByte(Yellow) + 256 * GetByte(Black)));
    }

    private int GetByte(int percent)
    {
      return (int)(255 * percent / 100.0);
    }

    public static Color FromRgb(byte r, byte g, byte b)
    {
      RgbToCmyk(r / 255.0, g / 255.0, b / 255.0, out double c, out double m, out double y, out double k);
      Color col = new Color { Cyan = (byte)(255 * c), Magenta = (byte)(255 * m), Yellow = (byte)(255 * y), Black = (byte)(255 * k) };
      return col;
    }
    public static Color ColorToCmyk(System.Drawing.Color color)
    {
      ColorToCmyk(color, out double c, out double m, out double y, out double k);
      Color col = new Color { Cyan = (byte)(255 * c), Magenta = (byte)(255 * m), Yellow = (byte)(255 * y), Black = (byte)(255 * k) };
      return col;
    }

    public static void ColorToCmyk(System.Drawing.Color color, out double c, out double m, out double y, out double k)
    {
      double red = color.R / 255.0;
      double green = color.G / 255.0;
      double blue = color.B / 255.0;
      RgbToCmyk(red, green, blue, out c, out m, out y, out k);
    }

    public static void RgbToCmyk(double red, double green, double blue, out double c, out double m, out double y, out double k)
    {
      k = System.Math.Min(System.Math.Min(1 - red, 1 - green), 1 - blue);
      c = (1 - red - k) / (1 - k);
      m = (1 - green - k) / (1 - k);
      y = (1 - blue - k) / (1 - k);
    }

    public static System.Drawing.Color CmykToColor(double c, double m, double y, double k)
    {
      int red = (int)((1 - c) * (1 - k) * 255);
      int green = (int)((1 - m) * (1 - k) * 255);
      int blue = (int)((1 - y) * (1 - k) * 255);

      return System.Drawing.Color.FromArgb(red, green, blue);
    }

  }

  public class ColorInfo
  {
    private int _nummer;
    private int _res;
    private Color _color;
    private string _name;
    private string _sepPercentage;

    public int Position { get; set; }

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
      { return _sepPercentage; }
      set
      { _sepPercentage = value; }
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
    private string _name;
    private Color _color;
    private int _rasterFreq;
    private int _rasterAng;

    public ColorSeparation()
    {
      _color = new Color();
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

    public int RasterFreq
    {
      get
      { return _rasterFreq; }
      set
      { _rasterFreq = value; }
    }

    public int RasterAngle
    {
      get
      { return _rasterAng; }
      set
      { _rasterAng = value; }
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
        _name, _color.Cyan, _color.Magenta, _color.Yellow, _color.Black,
        _rasterFreq, _rasterAng);
    }
  }
}
