namespace Ocad.StringParams
{
  public enum StringType
  {
    Control = 1,
    Course = 2,
    Class = 3,

    CourseView = 7,
    Template = 8,
    Color = 9,

    LayoutObject = 27,

    DisplayPar = 1024,
    PrintPar = 1026,
    ViewPar = 1030,
    Export = 1035,
    ScalePar = 1039,

    MapNotes = 1061
  }

  /// <summary>
  /// Summary description for CourseSettingIndex.
  /// </summary>
  public class StringParamIndex
  {
    public int FilePosition { get; set; }
    public int Size { get; set; }
    public StringType Type { get; set; }
    public int ElemNummer { get; set; }
    public long IndexPosition { get; set; }
  }
}
