namespace Ocad.Symbol
{
  public class TextSymbol : BaseSymbol
  {
    public class Underline
    {
      private int _color;
      private int _width;
      private int _distance;
      public Underline(int color, int width, int distance)
      {
        _color = color;
        _width = width;
        _distance = distance;
      }
      public int Color
      { get { return _color; } }
      public int Width
      { get { return _width; } }
      public int Distance
      { get { return _distance; } }
    }
    public abstract class Framing
    {
      private int _color;
      protected Framing(int color)
      { _color = color; }
      public int Color
      { get { return _color; } }
    }
    public class ShadowFraming : Framing
    {
      private int _xOffset;
      private int _yOffset;
      public ShadowFraming(int color, int xOffset, int yOffset)
        : base(color)
      {
        _xOffset = xOffset;
        _yOffset = yOffset;
      }
      public int XOffset
      { get { return _xOffset; } }
      public int YOffset
      { get { return _yOffset; } }
    }
    public class LineFraming : Framing
    {
      private int _width;
      public LineFraming(int color, int width)
        : base(color)
      {
        _width = width;
      }
      public int Width
      { get { return _width; } }
    }
    public class RectFraming : Framing
    {
      private int _left;
      private int _right;
      private int _bottom;
      private int _top;
      public RectFraming(int color, int left, int right,
                         int bottom, int top)
        : base(color)
      {
        _left = left;
        _right = right;
        _bottom = bottom;
        _top = top;
      }
      /// <summary>
      /// Left in 0.01 mm in map units
      /// </summary>
      public double Left
      {
        get
        { return _left; }
      }
      /// <summary>
      /// Right in 0.01 mm in map units
      /// </summary>
      public double Right
      {
        get
        { return _right; }
      }
      /// <summary>
      /// Bottom in 0.01 mm in map units
      /// </summary>
      public double Bottom
      {
        get
        { return _bottom; }
      }
      /// <summary>
      /// Top in 0.01 mm in map units
      /// </summary>
      public double Top
      {
        get
        { return _top; }
      }

    }
    public enum WeightMode
    { Normal = 400, Bold = 700 }
    public enum AlignmentMode
    { Left = 0, Center = 1, Right = 2, Justified = 3 }
    public enum FramingMode
    { No = 0, Shadow = 1, Line = 2, Rectangle = 3 }

    private string _fontName;
    private int _fontColor;
    private int _size;
    private WeightMode _weight;
    private bool _italic;
    private int _charSpace;
    private int _wordSpace;
    private AlignmentMode _alignment;
    private int _lineSpace;
    private int _paragraphSpace;
    private int _indentFirst;
    private int _indentOther;
    private int[] _tabs;
    private Underline _lineBelow;
    private Framing _framing;

    public TextSymbol(int symbolNumber)
      :
        base(symbolNumber)
    { }

    public Framing Frame
    {
      get
      { return _framing; }
      set
      { _framing = value; }
    }
    public AlignmentMode Alignment
    {
      get
      { return _alignment; }
      set
      { _alignment = value; }
    }
    public int CharSpace
    {
      get
      { return _charSpace; }
      set
      { _charSpace = value; }
    }
    public int Color
    {
      get
      { return _fontColor; }
      set
      { _fontColor = value; }
    }
    public string FontName
    {
      get
      { return _fontName; }
      set
      { _fontName = value; }
    }
    public int IndentFirst
    {
      get
      { return _indentFirst; }
      set
      { _indentFirst = value; }
    }
    public int IndentOther
    {
      get
      { return _indentOther; }
      set
      { _indentOther = value; }
    }
    public bool Italic
    {
      get
      { return _italic; }
      set
      { _italic = value; }
    }
    public Underline LineBelow
    {
      get
      { return _lineBelow; }
      set
      { _lineBelow = value; }
    }
    public int LineSpace
    {
      get
      { return _lineSpace; }
      set
      { _lineSpace = value; }
    }
    public int ParagraphSpace
    {
      get
      { return _paragraphSpace; }
      set
      { _paragraphSpace = value; }
    }
    public int Size
    {
      get
      { return _size; }
      set
      { _size = value; }
    }
    public int[] Tabs
    {
      get
      { return _tabs; }
      set
      { _tabs = value; }
    }
    public WeightMode Weight
    {
      get
      { return _weight; }
      set
      { _weight = value; }
    }
    public int WordSpace
    {
      get
      { return _wordSpace; }
      set
      { _wordSpace = value; }
    }

  }
}