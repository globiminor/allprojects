namespace Ocad.Symbol
{
  public abstract class BaseSymbol
  {
    private int _iSymNum;
    private bool _bRotatable;
    private bool _bFavorites;
    private bool _bSelected;
    private SymbolStatus _eStatus;
    private SymbolTool _eTool;
    private SymbolCourseSetting _eCourse;
    // private SymbolCourseType _eCourseType;
    // private SymbolColumn _eAllowedColumnes;
    private int _iGroup;
    private string _sDescription;
    private byte[] _icon;
    private SymbolGraphicsCollection _graphics;

    protected BaseSymbol(int symbolNumber)
    {
      _iSymNum = symbolNumber;
      _graphics = new SymbolGraphicsCollection();
    }

    public int Number
    {
      get
      { return _iSymNum; }
    }

    public bool Rotatable
    {
      get
      { return _bRotatable; }
      set
      { _bRotatable = value; }
    }
    public bool IsFavorit
    {
      get
      { return _bFavorites; }
      set
      { _bFavorites = value; }
    }
    public bool Selected
    {
      get
      { return _bSelected; }
      set
      { _bSelected = value; }
    }
    public SymbolStatus Status
    {
      get
      { return _eStatus; }
      set
      { _eStatus = value; }
    }
    public SymbolTool PreferredTool
    {
      get
      { return _eTool; }
      set
      { _eTool = value; }
    }
    public SymbolCourseSetting CourseSettingMode
    {
      get
      { return _eCourse; }
      set
      { _eCourse = value; }
    }
    public string Description
    {
      get
      { return _sDescription; }
      set
      { _sDescription = value; }
    }
    public byte[] Icon
    {
      get
      { return _icon; }
      set
      { _icon = value; }
    }
    public int Group
    {
      get
      { return _iGroup; }
      set
      { _iGroup = value; }
    }
    public SymbolGraphicsCollection Graphics
    {
      get
      { return _graphics; }
    }
  }

  public class AnySymbol : BaseSymbol
  {
    public AnySymbol(int symbolNumber)
      :
        base(symbolNumber)
    { }
  }
}