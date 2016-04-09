namespace Ocad.Symbol
{
  public class LineSymbol : BaseSymbol
  {
    private short _lineColor;
    private short _lineWidth;
    private ELineStyle _lineStyle;
    private short _distFromStart;
    private short _distToEnd;
    private short _mainLength;
    private short _mainGap;
    private short _endLength;
    private short _endGap;
    private short _secondGap;
    private short _nPrimSym;
    private short _primSymDist;
    private short _minSym;
    private char _fillColorOn;
    private char _flags;
    private short _leftColor;
    private short _fillColor;
    private short _rightColor;
    private short _fillWidth;
    private short _leftWidth;
    private short _rightWidth;
    private short _gap;
    private short _frameColor;
    private short _last;
    private short _frameWidth;
    private ELineStyle _frameStyle;

    public enum ELineStyle
    { Bevel = 0, Round = 1, Miter = 4 }

    public LineSymbol(int symbolNumber)
      : base(symbolNumber)
    { }

    public short LineColor
    {
      get { return _lineColor; }
      set { _lineColor = value; }
    }

    public short LineWidth
    {
      get { return _lineWidth; }
      set { _lineWidth = value; }
    }

    public ELineStyle LineStyle
    {
      get { return _lineStyle; }
      set { _lineStyle = value; }
    }

    public short DistFromStart
    {
      get { return _distFromStart; }
      set { _distFromStart = value; }
    }

    public short DistToEnd
    {
      get { return _distToEnd; }
      set { _distToEnd = value; }
    }

    public short MainLength
    {
      get { return _mainLength; }
      set { _mainLength = value; }
    }

    public short MainGap
    {
      get { return _mainGap; }
      set { _mainGap = value; }
    }

    public short EndLength
    {
      get { return _endLength; }
      set { _endLength = value; }
    }

    public short EndGap
    {
      get { return _endGap; }
      set { _endGap = value; }
    }

    public short SecondGap
    {
      get { return _secondGap; }
      set { _secondGap = value; }
    }

    public short NPrimSym
    {
      get { return _nPrimSym; }
      set { _nPrimSym = value; }
    }

    public short PrimSymDist
    {
      get { return _primSymDist; }
      set { _primSymDist = value; }
    }

    public short MinSym
    {
      get { return _minSym; }
      set { _minSym = value; }
    }

    public char FillColorOn
    {
      get { return _fillColorOn; }
      set { _fillColorOn = value; }
    }

    public char Flags
    {
      get { return _flags; }
      set { _flags = value; }
    }

    public short LeftColor
    {
      get { return _leftColor; }
      set { _leftColor = value; }
    }

    public short FillColor
    {
      get { return _fillColor; }
      set { _fillColor = value; }
    }

    public short RightColor
    {
      get { return _rightColor; }
      set { _rightColor = value; }
    }

    public short FillWidth
    {
      get { return _fillWidth; }
      set { _fillWidth = value; }
    }

    public short LeftWidth
    {
      get { return _leftWidth; }
      set { _leftWidth = value; }
    }

    public short RightWidth
    {
      get { return _rightWidth; }
      set { _rightWidth = value; }
    }

    public short Gap
    {
      get { return _gap; }
      set { _gap = value; }
    }

    public short FrameColor
    {
      get { return _frameColor; }
      set { _frameColor = value; }
    }

    public short Last
    {
      get { return _last; }
      set { _last = value; }
    }

    public short FrameWidth
    {
      get { return _frameWidth; }
      set { _frameWidth = value; }
    }

    public ELineStyle FrameStyle
    {
      get { return _frameStyle; }
      set { _frameStyle = value; }
    }
  }
}