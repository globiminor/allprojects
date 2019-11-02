using Basics.Geom;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ocad
{
  public class OcadReader : IDisposable
  {
    private OcadIo _io;

    internal OcadReader(OcadIo io)
    {
      _io = io;
    }
    public IComparer<ElementIndex> ColorComparer => _io.ColorComparer;

    internal OcadIo Io => _io;
    internal Stream BaseStream => _io.BaseStream;

    public static OcadReader Open(Stream stream, int ocadVersion)
    {
      return new OcadReader(OcadIo.GetIo(stream, ocadVersion));
    }
    public static OcadReader Open(string name)
    {
      string ocdName = OcdName(name);
      return Open(new FileStream(ocdName, FileMode.Open, FileAccess.Read), 0);
    }

    public void Dispose()
    {
      Close();
      _io = null;
    }

    public Setup ReadSetup()
    {
      return _io.ReadSetup();
    }

    public Setup Setup => _io.Setup;

    protected static string OcdName(string name)
    {
      return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(name)),
        Path.GetFileNameWithoutExtension(name) + ".ocd");
    }

    public List<ElementIndex> GetIndices()
    {
      return _io.GetIndices();
    }

    public ElementIndex ReadIndex(int i)
    {
      return _io.ReadIndex(i);
    }

    public T ReadElement<T>(ElementIndex index, out T element) where T : Element, new()
    {
      return _io.ReadElement(index, out element);
    }
    public T ReadElement<T>(int index, out T element) where T : Element, new()
    {
      return _io.ReadElement(index, out element);
    }
    public T ReadElement<T>(out T element) where T : Element, new()
    {
      return _io.ReadElement(out element);
    }

    public IEnumerable<ColorInfo> ReadColorInfos()
    {
      return _io.ReadColorInfos();
    }
    public IEnumerable<Symbol.BaseSymbol> ReadSymbols()
    {
      return _io.ReadSymbols();
    }
    public int ReadSymbolPosition(int idx)
    {
      return _io.ReadSymbolPosition(idx);
    }
    public Symbol.BaseSymbol ReadSymbol(int position)
    {
      return _io.ReadSymbol(position);
    }

    public string ReadStringParam(StringParamIndex idx)
    {
      return _io.ReadStringParam(idx);
    }
    public IList<CategoryPar> GetCategories(IList<StringParamIndex> indexList = null)
    {
      IList<StringParamIndex> indexEnum = indexList ?? _io.EnumStringParamIndices().ToList();

      List<CategoryPar> catNames = new List<CategoryPar>();
      foreach (var index in indexEnum)
      {
        if (index.Type == StringType.Class)
        {
          CategoryPar candidate = new CategoryPar(_io.ReadStringParam(index));
          catNames.Add(candidate);
        }
      }
      return catNames;
    }

    public IList<string> GetCourseCategories(string courseName, IList<StringParamIndex> indexList = null)
    {
      IEnumerable<StringParamIndex> indexEnum = indexList ?? ReadStringParamIndices();
      List<string> catNames = new List<string>();
      foreach (var index in indexEnum)
      {
        if (index.Type == StringType.Class)
        {
          CategoryPar candidate = new CategoryPar(_io.ReadStringParam(index));
          if (candidate.CourseName == courseName)
          {
            catNames.Add(candidate.Name);
          }
        }

        if (index.Type == StringType.Course)
        {
          CoursePar candidate = new CoursePar(_io.ReadStringParam(index));
          if (candidate.Name == courseName)
          {
            catNames.Add(candidate.Name);
          }
        }
      }
      return catNames;
    }

    public static bool Exists(string name)
    {
      string ext = Path.GetExtension(name);
      if (ext != "" && ext != ".ocd")
      { return false; }

      if (File.Exists(OcdName(name)) == false)
      { return false; }

      return true;
    }

    public IEnumerable<ElementIndex> EnumIndexesFromGeo(IBox extentIntersect = null, IList<ElementIndex> indexList = null)
    {
      return _io.EnumIndexesFromGeo(extentIntersect, indexList);
    }

    public IEnumerable<GeoElement> EnumGeoElements(IBox extentIntersect = null, IList<ElementIndex> indexList = null)
    {
      return _io.EnumGeoElements(extentIntersect, indexList);
    }
    public IEnumerable<GeoElement> EnumGeoElements(IList<ElementIndex> indexList)
    {
      return _io.EnumGeoElements(null, indexList);
    }

    public IEnumerable<MapElement> EnumMapElements(IBox extentIntersect, IList<ElementIndex> indexList)
    {
      return _io.EnumMapElements(extentIntersect, indexList);
    }

    public IEnumerable<MapElement> EnumMapElements(IList<ElementIndex> indexList)
    {
      return EnumMapElements(null, indexList);
    }

    public void Close()
    {
      _io?.Close();
    }

    #region course setting

    public List<StringParamIndex> ReadStringParamIndices()
    {
      return new List<StringParamIndex>(_io.EnumStringParamIndices());
    }

    public IList<Control> ReadControls(IList<StringParamIndex> indexList = null)
    {
      IList<StringParamIndex> indexEnum = indexList ?? ReadStringParamIndices();

      Dictionary<Control, StringParamIndex> controls =
        new Dictionary<Control, StringParamIndex>();
      foreach (var index in indexEnum)
      {
        if (index.Type != StringType.Control)
        { continue; }

        string stringParam = _io.ReadStringParam(index);

        Control control = Control.FromStringParam(stringParam);

        controls.Add(control, index);
      }

      foreach (var pair in controls)
      {
        pair.Key.Element = _io.ReadElement(pair.Value.ElemNummer - 1, out GeoElement element);
      }
      IList<Control> result = new List<Control>(controls.Keys);
      return result;
    }

    public string ReadCourseName(int course, IList<StringParamIndex> indexList = null)
    {
      int iCourse;
      IList<StringParamIndex> indexEnum = indexList ?? ReadStringParamIndices();

      iCourse = 0;
      foreach (var index in indexList)
      {
        if (index.Type == StringType.Course)
        {
          if (iCourse == course)
          {
            return ReadCourseName(index);
          }
          iCourse++;
        }
      }
      return null;
    }

    public string ReadCourseName(StringParamIndex courseIndex)
    {
      if (courseIndex.Type != StringType.Course)
      {
        throw new ArgumentException("Invalid stringparameter type " + courseIndex.Type +
        ". Expected " + StringType.Course);
      }

      return _io.ReadStringParam(courseIndex, nTabs: 1);
    }

    protected static SectionCollection ReadSectionList(IList<string> parts, ref int partIdx)
    {
      SectionCollection sectionList;
      string part;

      if (parts.Count <= partIdx)
      { return new SectionCollection(); }

      part = parts[partIdx];
      partIdx++;

      sectionList = new SectionCollection();
      while (string.IsNullOrEmpty(part) == false &&
        part[0] != CoursePar.ForkStartKey && part[0] != CoursePar.VarStartKey &&
        part[0] != CoursePar.ForkEndKey && part[0] != CoursePar.VarEndKey)
      {
        if (part[0] == (char)ControlCode.Start)
        { // start
          Control start = new Control();
          sectionList.AddLast(start);
          start.Name = part.Substring(1);
          start.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.Control)
        { // control
          Control control = new Control();
          sectionList.AddLast(control);
          control.Name = part.Substring(1);
          control.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.Finish)
        { // finish
          Control finish = new Control();
          sectionList.AddLast(finish);
          finish.Name = part.Substring(1);
          finish.Code = (ControlCode)part[0];
        }
        else if (part[0] == CoursePar.VariationKey)
        { // variations
          Variation variation = new Variation();
          sectionList.AddLast(variation);

          part = parts[partIdx];
          partIdx++;
          while (string.IsNullOrEmpty(part) == false && part[0] == CoursePar.VarStartKey)
          {
            string sVariations = part.Substring(1);
            List<int> legs = new List<int>();
            foreach (var sLeg in sVariations.Split('-'))
            {
              legs.Add(Convert.ToInt32(sLeg));
            }
            Variation.Branch branch =
              new Variation.Branch(legs, ReadSectionList(parts, ref partIdx));
            part = (partIdx - 1 < parts.Count) ? parts[partIdx - 1] : null;

            variation.Branches.Add(branch);
          }
          if (string.IsNullOrEmpty(part) || part[0] != CoursePar.VarEndKey)
          { throw new Exception("No variation end " + part); }
        }
        else if (part[0] == CoursePar.ForkKey)
        { // forks
          if (Int32.TryParse(part.Substring(1), out int nForks) == false)
          {
            nForks = -1;
            Debug.Assert(false, "Expected Tab r<number of forks>, got " + part);
          }
          part = parts[partIdx];
          partIdx++;
          Fork fork = new Fork();
          sectionList.AddLast(fork);
          while (string.IsNullOrEmpty(part) == false && part[0] == CoursePar.ForkStartKey)
          {
            fork.Branches.Add(new Fork.Branch(ReadSectionList(parts, ref partIdx)));
            part = (partIdx - 1 < parts.Count) ? parts[partIdx - 1] : null;
          }
          if (string.IsNullOrEmpty(part) || part[0] != CoursePar.ForkEndKey)
          { throw new Exception("No fork end " + part); }
          Debug.Assert(nForks < 0 || (nForks == fork.Branches.Count),
            string.Format("Expected {0} branches, got {1}", nForks, fork.Branches.Count));
        }
        else if (part[0] == (char)ControlCode.MarkedRoute) // Pflichtstrecke
        {
          Control pflicht = new Control();
          sectionList.AddLast(pflicht);
          pflicht.Name = part.Substring(1);
          pflicht.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.TextBlock)
        {
          Control textBlock = new Control();
          sectionList.AddLast(textBlock);
          textBlock.Name = part.Substring(1);
          textBlock.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.MapChange)
        {
          Control mapExc = new Control();
          sectionList.AddLast(mapExc);
          mapExc.Name = part.Substring(1);
          mapExc.Code = (ControlCode)part[0];
        }
        else if (part[0] == CoursePar.ClimbKey)
        { }
        else if (part[0] == CoursePar.ExtraDistanceKey)
        { }
        else if (part[0] == CoursePar.FromStartNumberKey)
        { }
        else if (part[0] == CoursePar.LegCountKey)
        { }
        else if (part[0] == CoursePar.RunnersCountKey)
        { }
        else if (part[0] == CoursePar.ToStartNumberKey)
        { }
        else if (part[0] == CoursePar.CourseTypeKey) // course type
        { }
        else if (part[0] == CoursePar.StartNrObject)
        { }
        else if (part[0] == CoursePar.CourseNameObject)
        { }
        else
        { // something unexpected between start and finish
          throw new Exception("Unexpected Tab " + part +
            " (Last successfull Tab: " + parts[partIdx - 2] + ")");
        }
        if (partIdx < parts.Count)
        {
          part = parts[partIdx];
          partIdx++;
        }
        else
        {
          part = null;
        }
      }
      return sectionList;
    }

    private void ReadSectionGeometry(SectionCollection sectionList,
      IList<StringParamIndex> indexList)
    {
      foreach (var section in sectionList)
      {
        if (section is Control)
        {
          _io.ReadControlGeometry((Control)section, indexList);
        }
        else if (section is SplitSection fork)
        {
          foreach (var branch in fork.Branches)
          { ReadSectionGeometry(branch, indexList); }
        }
        else
        {
          throw new ArgumentException("Unhandled section type " + section.GetType());
        }
      }
    }

    public IEnumerable<Course> ReadCourses()
    {
      IList<StringParamIndex> idxList = ReadStringParamIndices();

      foreach (var index in idxList)
      {
        if (index.Type == StringType.Course)
        {
          string courseName = ReadCourseName(index);
          Course course = ReadCourse(courseName, idxList);
          yield return course;
        }
      }
    }

    public Course ReadCourse(string courseName, IList<StringParamIndex> indexList = null)
    {
      if (courseName == null)
      { return null; }

      indexList = indexList ?? ReadStringParamIndices();
      foreach (var idx in indexList)
      {
        if (idx.Type == StringType.Course)
        {
          string sName = _io.ReadStringParam(idx, nTabs: 1);

          if (sName == courseName)
          {
            return ReadCourse(idx, indexList);
          }
        }
      }

      return null;

    }

    public Course ReadCourse(StringParamIndex courseIndex, IList<StringParamIndex> indexList)
    {
      string courseParam = _io.ReadStringParam(courseIndex);
      IList<string> parts = courseParam.Split('\t');

      Course course = ReadCourse(parts);

      ReadSectionGeometry(course, indexList);

      return course;
    }

    private static Course ReadCourse(IList<string> parts)
    {
      string name;

      int nParts = parts.Count;
      name = parts[0];

      Course course = new Course(name);

      // look for meta data
      for (int iPart = 1; iPart < nParts; iPart++)
      {
        name = parts[iPart];

        if (name[0] == CoursePar.ClimbKey)
        { course.Climb = Convert.ToDouble(name.Substring(1)); }
        else if (name[0] == CoursePar.ExtraDistanceKey)
        { course.ExtraDistance = Convert.ToDouble(name.Substring(1)); }
      }


      // read course
      {
        int iPart = 1;
        course.AddLast(ReadSectionList(parts, ref iPart));
      }

      course.AssignLegs();

      return course;
    }

    #endregion
  }
}
