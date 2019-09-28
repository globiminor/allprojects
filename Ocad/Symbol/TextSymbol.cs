namespace Ocad.Symbol
{
  public class TextSymbol : BaseSymbol
  {
    public class Underline
    {
      public Underline(int color, int width, int distance)
      {
        Color = color;
        Width = width;
        Distance = distance;
      }
      public int Color { get; private set; }
      public int Width { get; private set; }
      public int Distance { get; private set; }
    }
    public abstract class Framing
    {
      protected Framing(int color)
      { Color = color; }
      public int Color { get; private set; }
    }
    public class ShadowFraming : Framing
    {
      public ShadowFraming(int color, int xOffset, int yOffset)
        : base(color)
      {
        XOffset = xOffset;
        YOffset = yOffset;
      }
      public int XOffset { get; private set; }
      public int YOffset { get; private set; }
    }
    public class LineFraming : Framing
    {
      public LineFraming(int color, int width)
        : base(color)
      {
        Width = width;
      }
      public int Width { get; private set; }
    }
    public class RectFraming : Framing
    {
      public RectFraming(int color, int left, int right,
                         int bottom, int top)
        : base(color)
      {
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
      }
      /// <summary>
      /// Left in 0.01 mm in map units
      /// </summary>
      public double Left { get; private set; }
      /// <summary>
      /// Right in 0.01 mm in map units
      /// </summary>
      public double Right { get; private set; }
      /// <summary>
      /// Bottom in 0.01 mm in map units
      /// </summary>
      public double Bottom { get; private set; }
      /// <summary>
      /// Top in 0.01 mm in map units
      /// </summary>
      public double Top { get; private set; }
    }
    public enum WeightMode
    { Normal = 400, Bold = 700 }
    public enum AlignmentMode
    { Left = 0, Center = 1, Right = 2, Justified = 3 }
    public enum FramingMode
    { No = 0, Shadow = 1, Line = 2, Rectangle = 3 }

    public TextSymbol(int symbolNumber)
      :
        base(symbolNumber, SymbolType.Text)
    { }

    protected override int GetMainColorBase() => Color;

    public Framing Frame { get; set; }
    public AlignmentMode Alignment { get; set; }
    public int CharSpace { get; set; }
    public int Color { get; set; }
    public string FontName { get; set; }
    public int IndentFirst { get; set; }
    public int IndentOther { get; set; }
    public bool Italic { get; set; }
    public Underline LineBelow { get; set; }
    public int LineSpace { get; set; }
    public int ParagraphSpace { get; set; }
    public int Size { get; set; }
    public int[] Tabs { get; set; }
    public WeightMode Weight { get; set; }
    public int WordSpace { get; set; }
  }
}