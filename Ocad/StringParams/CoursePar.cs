using System;

namespace Ocad.StringParams
{
  public class CoursePar : MultiParam
  {
    public const char ClimbKey = 'C';
    public const char ExtraDistanceKey = 'E';
    public const char FromStartNumberKey = 'F';
    public const char LegCountKey = 'L';
    public const char RunnersCountKey = 'R';
    public const char ToStartNumberKey = 'T';
    public const char CourseTypeKey = 'Y';

    public const char StartKey = 's';
    public const char ControlKey = 'c';
    public const char FinishKey = 'f';
    public const char MarkedRouteKey = 'm';
    public const char TextBlockKey = 't';
    public const char MapChangeKey = 'g';

    public const char ForkKey = 'r';
    public const char ForkStartKey = 'v';
    public const char ForkEndKey = 'q';

    public const char VariationKey = 'l';
    public const char VarStartKey = 'b';
    public const char VarEndKey = 'p';

    public CoursePar(string para)
      : base(para)
    { }

    private CoursePar()
    { }

    public static CoursePar Create(Course course)
    {
      CoursePar par = new CoursePar();

      par.ParaList.Add(course.Name);
      par.Add(ClimbKey, course.Climb);
      par.Add(ExtraDistanceKey, course.ExtraDistance);
      par.Add(FromStartNumberKey, null);
      par.Add(ToStartNumberKey, null);
      par.Add(RunnersCountKey, null);

      //par.Add(LegCountKey, course.LegCount());
      //par.Add(CourseTypeKey, "gaga");
      foreach (ISection section in course)
      {
        par.Add(section);
      }
      return par;
    }

    public int? NrRunners
    {
      get { return GetInt_(RunnersCountKey); }
      set { SetParam(RunnersCountKey, value); }
    }

    public int? MinStartNr
    {
      get { return GetInt_(FromStartNumberKey); }
      set { SetParam(FromStartNumberKey, value); }
    }
    public int? MaxStartNr
    {
      get { return GetInt_(ToStartNumberKey); }
      set { SetParam(ToStartNumberKey, value); }
    }

    public int? Climb
    {
      get { return GetInt_(ClimbKey); }
      set { SetParam(ClimbKey, value); }
    }

    private void Add(ISection section)
    {
      if (section is Control cntr)
      {
        Add(cntr.Code, cntr.Name);
      }
      else
      {
        throw new NotImplementedException();
      }
    }
  }
}
