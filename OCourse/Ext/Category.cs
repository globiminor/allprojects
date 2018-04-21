
using System;
using System.Collections.Generic;
using Ocad;
using Ocad.StringParams;

namespace OCourse.Ext
{
  public class Category
  {
    private readonly Course _course;

    public int MinStartNr;
    public int MaxStartNr;

    public readonly List<int> VariationIndexList = new List<int>();

    public Category(Course course)
    {
      _course = course;
    }
    public Course Course
    {
      get { return _course; }
    }
    public bool IsSimple()
    {
      throw new NotImplementedException();
    }

    public SectionList GetIdxPermutation(int index)
    {
      throw new NotImplementedException();
    }

    public SectionList GetStartNrPermutation(int startNr)
    {
      SectionList course = GetIdxPermutation(startNr - MinStartNr);
      return course;
    }
    public static List<Category> CreateCategories(OcadReader reader, IList<string> courseNames, int runnersConst)
    {
      List<Category> categories = new List<Category>();

      IList<StringParamIndex> paramList = reader.ReadStringParamIndices();

      int minNr = 1;

      foreach (string courseName in courseNames)
      {
        Category cat = Create(reader, paramList, courseName, minNr, runnersConst);
        categories.Add(cat);

        minNr = cat.MaxStartNr + 1;
      }

      return categories;
    }

    public static Category Create(OcadReader reader, string courseName, int minNr, int runnersConst)
    {
      IList<StringParamIndex> paramList = reader.ReadStringParamIndices();
      Category cat = Create(reader, paramList, courseName, minNr, runnersConst);
      return cat;
    }

    public static Category Create(OcadReader reader, IList<StringParamIndex> paramList, string courseName,
      int minNr, int runnersConst)
    {
      int min;
      int max;

      if (runnersConst <= 0)
      {
        bool success = GetStartNumbers(courseName, reader, paramList, out int nrRunners, out min, out max);
        if (success == false)
        {
          min = 0;
          max = -1;
        }
        if (max > min)
        {
          nrRunners = max - min + 1;
        }
        if (min <= 1)
        {
          min = minNr;
        }
        max = min + nrRunners - 1;
      }
      else
      {
        min = minNr;
        max = minNr + runnersConst - 1;
      }

      Category cat;
      {
        Course course = reader.ReadCourse(courseName);
        cat = new Category(course);
      }
      cat.MinStartNr = min;
      cat.MaxStartNr = max;

      return cat;
    }


    private static bool GetStartNumbers(string courseName,
      OcadReader reader, IList<StringParamIndex> paramList,
      out int nrRunner, out int min, out int max)
    {
      CategoryPar catPar = null;
      CoursePar coursePar = null;
      foreach (StringParamIndex par in paramList)
      {
        if (par.Type == StringType.Class)
        {
          CategoryPar candidate = new CategoryPar(reader.ReadStringParam(par));
          if (candidate.Name == courseName &&
            (candidate.MinStartNr.HasValue || candidate.NrRunners.HasValue))
          {
            catPar = candidate;
            break;
          }
        }

        if (par.Type == StringType.Course)
        {
          CoursePar candidate = new CoursePar(reader.ReadStringParam(par));
          if (candidate.Name == courseName &&
            (candidate.MinStartNr.HasValue || candidate.NrRunners.HasValue))
          {
            coursePar = candidate;
            break;
          }
        }
      }

      min = 0;
      max = 0;
      nrRunner = 0;

      if (catPar == null && coursePar == null)
      {
        return false;
      }

      if (catPar != null && catPar.MinStartNr.HasValue)
      { min = catPar.MinStartNr.Value; }
      else if (coursePar != null && coursePar.MinStartNr.HasValue)
      { min = coursePar.MinStartNr.Value; }

      if (catPar != null && catPar.MaxStartNr.HasValue)
      { max = catPar.MaxStartNr.Value; }
      else if (coursePar != null && coursePar.MaxStartNr.HasValue)
      { max = coursePar.MaxStartNr.Value; }

      if (catPar != null && catPar.NrRunners.HasValue)
      { nrRunner = catPar.NrRunners.Value; }
      else if (coursePar != null && coursePar.NrRunners.HasValue)
      { nrRunner = coursePar.NrRunners.Value; }

      return true;
    }

    public List<Course> GetMainSections()
    {
      throw new NotImplementedException();
    }
  }
}
