namespace Ocad.StringParams
{
  public class CategoryPar : MultiParam
  {
    public const char CourseKey = 'c';
    public const char NrRunnersKey = 'r';
    public const char MinStartNrKey = 'f';
    public const char MaxStartNrKey = 't';

    public CategoryPar()
    { }
    public CategoryPar(string stringParam)
      : base(stringParam)
    { }
    public string CourseName
    {
      get { return GetString(CourseKey); }
      set { SetParam(CourseKey, value); }
    }
    public int? NrRunners
    {
      get { return GetInt_(NrRunnersKey); }
      set { SetParam(NrRunnersKey, value); }
    }

    public int? MinStartNr
    {
      get { return GetInt_(MinStartNrKey); }
      set { SetParam(MinStartNrKey, value); }
    }
    public int? MaxStartNr
    {
      get { return GetInt_(MaxStartNrKey); }
      set { SetParam(MaxStartNrKey, value); }
    }
  }
}
