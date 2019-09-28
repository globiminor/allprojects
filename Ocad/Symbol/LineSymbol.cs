namespace Ocad.Symbol
{
  public class LineSymbol : BaseSymbol
  {
    public enum ELineStyle
    { Bevel = 0, Round = 1, Miter = 4 }
    public enum EDecreaseMode
    { None = 0, ToEnd = 1, ToStart = 2, Both = 3 }
    public enum ESymbolFlags
    { End = 1, Start = 2, Corner = 4, Second = 8 }

    public LineSymbol(int symbolNumber)
      : base(symbolNumber, SymbolType.Line)
    { }

    protected override int GetMainColorBase()
    {
      if (LineWidth > 0)
      { return LineColor; }
      if (LeftWidth > 0)
      { return LeftColor; }
      if (RightWidth > 0)
      { return RightColor; }
      if (FillColorOn > 0 && FillWidth > 0)
      { return FillColor; }

      if (NPrimSym > 0 && PrimSymbol?.Count > 0)
      { return PrimSymbol[0].Color; }
      if ((SymbolFlags & ESymbolFlags.Second) == ESymbolFlags.Second && SecSymbol?.Count > 0)
      { return SecSymbol[0].Color; }
      if ((SymbolFlags & ESymbolFlags.Corner) == ESymbolFlags.Corner && CornerSymbol?.Count > 0)
      { return CornerSymbol[0].Color; }
      if ((SymbolFlags & ESymbolFlags.Start) == ESymbolFlags.Start && StartSymbol?.Count > 0)
      { return StartSymbol[0].Color; }
      if ((SymbolFlags & ESymbolFlags.End) == ESymbolFlags.End && EndSymbol?.Count > 0)
      { return EndSymbol[0].Color; }

      return -1;
    }

    public short LineColor { get; set; }
    public short LineWidth { get; set; }
    public ELineStyle LineStyle { get; set; }
    public short DistFromStart { get; set; }
    public short DistToEnd { get; set; }
    public short MainLength { get; set; }
    public short MainGap { get; set; }
    public short EndLength { get; set; }
    public short EndGap { get; set; }
    public short SecondGap { get; set; }
    public short NPrimSym { get; set; }
    public short PrimSymDist { get; set; }
    public short MinSym { get; set; }
    public short FillColorOn { get; set; }
    public short Flags { get; set; }
    public short LeftColor { get; set; }
    public short FillColor { get; set; }
    public short RightColor { get; set; }
    public short FillWidth { get; set; }
    public short LeftWidth { get; set; }
    public short RightWidth { get; set; }
    public short LRDash { get; set; }
    public short LRGap { get; set; }
    public short FrameColor { get; set; }
    public EDecreaseMode DecreaseMode { get; set; }
    public short DecreaseLast { get; set; }
    public byte DisableDecreaseDistance {get; set; }
    public byte DisableDecreaseWidth { get; set; }
    public short FrameWidth { get; set; }
    public ELineStyle FrameStyle { get; set; }

    public ESymbolFlags SymbolFlags { get; set; }
    public SymbolGraphicsCollection PrimSymbol { get; set; }
    public SymbolGraphicsCollection SecSymbol { get; set; }
    public SymbolGraphicsCollection CornerSymbol { get; set; }
    public SymbolGraphicsCollection StartSymbol { get; set; }
    public SymbolGraphicsCollection EndSymbol { get; set; }
  }
}