namespace Ocad.Symbol
{
  public class AreaSymbol : BaseSymbol
  {
    private int _borderSym;
    private short _fillColor;
    private EHatchMode _hatchMode;
    private short _hatchColor;
    private short _hatchLineWidth;
    private short _hatchLineDist;
    private short _hatchAngle1;
    private short _hatchAngle2;
    private bool _fillOn;
    private bool _borderOn;
    private EStructMode _structMode;
    private short _structWidth;
    private short _structHeight;

    public AreaSymbol(int symbol)
      : base(symbol)
    { }

    public enum EHatchMode
    { None = 0, Single = 1, Cross = 2 }

    public enum EStructMode
    { None = 0, Aligned = 1, Shifted = 2 }

    public int BorderSym
    {
      get { return _borderSym; }
      set { _borderSym = value; }
    }

    public short FillColor
    {
      get { return _fillColor; }
      set { _fillColor = value; }
    }

    public EHatchMode HatchMode
    {
      get { return _hatchMode; }
      set { _hatchMode = value; }
    }

    public short HatchColor
    {
      get { return _hatchColor; }
      set { _hatchColor = value; }
    }

    public short HatchLineWidth
    {
      get { return _hatchLineWidth; }
      set { _hatchLineWidth = value; }
    }

    public short HatchLineDist
    {
      get { return _hatchLineDist; }
      set { _hatchLineDist = value; }
    }

    public short HatchAngle1
    {
      get { return _hatchAngle1; }
      set { _hatchAngle1 = value; }
    }

    public short HatchAngle2
    {
      get { return _hatchAngle2; }
      set { _hatchAngle2 = value; }
    }

    public bool FillOn
    {
      get { return _fillOn; }
      set { _fillOn = value; }
    }

    public bool BorderOn
    {
      get { return _borderOn; }
      set { _borderOn = value; }
    }

    public EStructMode StructMode
    {
      get { return _structMode; }
      set { _structMode = value; }
    }

    public short StructWidth
    {
      get { return _structWidth; }
      set { _structWidth = value; }
    }

    public short StructHeight
    {
      get { return _structHeight; }
      set { _structHeight = value; }
    }
  }
}
