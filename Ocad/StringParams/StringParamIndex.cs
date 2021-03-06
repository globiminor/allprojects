namespace Ocad.StringParams
{
  public enum StringType
  {
    Control = 1,
    Course = 2,
    Class = 3,
    // TextBlock = 31,

    CourseView = 7,
    Template = 8,
    Color = 9,

    LayoutObject = 27,

    TextBlock = 31,

    DisplayPar = 1024,
    PrintPar = 1026,
    ViewPar = 1030,
    Export = 1035,
    ScalePar = 1039,
    CoordSystemPar = 1053,

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

    public override string ToString()
    {
      return $"T:{Type}, Elem#:{ElemNummer}";
    }
  }
}
