namespace Ocad.StringParams
{
  public class CourseViewPar : MultiParam
  {
    public const char ObjName = 'd'; // = description (eg object name)
    public const char From = 'f'; //= from, startpoint of line
    public const char To = 't'; // = to, end point of line

    public CourseViewPar(string para)
      : base(para)
    { }
    private CourseViewPar()
    { }

    public static CourseViewPar Create(string name, string obj)
    {
      CourseViewPar par = new CourseViewPar();
      par.ParaList.Add(name);
      par.Add(ObjName, obj);

      return par;
    }

    public static CourseViewPar Create(string name, string from, string to)
    {
      CourseViewPar par = new CourseViewPar();
      par.ParaList.Add(name);
      par.Add(From, from);
      par.Add(To, to);

      return par;
    }

    public string Obj
    {
      get { return GetString(ObjName); }
      set { SetParam(ObjName, value); }
    }

    protected override bool AppendEndTabs
    {
      get { return false; }
    }
  }
}
