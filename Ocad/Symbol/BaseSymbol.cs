namespace Ocad.Symbol
{
  public abstract class BaseSymbol
  {
    private int _iSymNum;
    private readonly SymbolGraphicsCollection _graphics;

    // private SymbolCourseType _eCourseType;
    // private SymbolColumn _eAllowedColumnes;

    protected BaseSymbol(int symbolNumber, SymbolType symbolType)
    {
      _iSymNum = symbolNumber;
      _graphics = new SymbolGraphicsCollection();

      Type = symbolType;
    }

    public int Number => _iSymNum;

    public SymbolType Type { get; }
    public bool Rotatable { get; set; }
    public bool IsFavorit { get; set; }
    public bool Selected { get; set; }
    public SymbolStatus Status { get; set; }
    public SymbolTool PreferredTool { get; set; }
    public SymbolCourseSetting CourseSettingMode { get; set; }
    public string Description { get; set; }
    public byte[] Icon { get; set; }
    public int Group { get; set; }
    public SymbolGraphicsCollection Graphics => _graphics;

    public int GetMainColor() => GetMainColorBase();
    protected abstract int GetMainColorBase();
  }

  public class AnySymbol : BaseSymbol
  {
    public AnySymbol(int symbolNumber)
      :
        base(symbolNumber, SymbolType.unknown)
    { }
    protected override int GetMainColorBase() => -1;
  }
}