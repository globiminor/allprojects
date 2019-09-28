namespace Ocad.Symbol
{
  public class AreaSymbol : BaseSymbol
  {
    public AreaSymbol(int symbol)
      : base(symbol, SymbolType.Area)
    { }

    public enum EHatchMode
    { None = 0, Single = 1, Cross = 2 }

    public enum EStructMode
    { None = 0, Aligned = 1, Shifted = 2 }
    public enum EStructDraw
    { Clip = 0, FullWithin = 1, CenterWithin = 2, PartlyWithin = 3 }

    public int BorderSym { get; set; }

    public short FillColor { get; set; }

    public EHatchMode HatchMode { get; set; }
    public short HatchColor { get; set; }
    public short HatchLineWidth { get; set; }
    public short HatchLineDist { get; set; }
    public short HatchAngle1 { get; set; }
    public short HatchAngle2 { get; set; }
    public bool FillOn { get; set; }
    public bool BorderOn { get; set; }

    public EStructMode StructMode { get; set; }
    public EStructDraw StructDraw { get; set; }
    public short StructWidth { get; set; }
    public short StructHeight { get; set; }
    public short StructAngle { get; set; }
    public byte StructIrregularVarX { get; set; }
    public byte StructIrregularVarY { get; set; }
    public short StructIrregularMinDist { get; set; }

    protected override int GetMainColorBase()
    {
      if (FillOn)
      { return FillColor; }
      if (HatchMode > 0)
      { return HatchColor; }
      if (StructMode > 0)
      {
        if (Graphics.Count > 0)
        { return Graphics[0].Color; }
      }
      if (BorderOn)
      { return -1; }

      return -1;
    }
  }
}
