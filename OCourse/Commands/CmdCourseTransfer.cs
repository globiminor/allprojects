using System;
using System.Collections.Generic;
using Ocad;
using Ocad.StringParams;
using OCourse.Route;

namespace OCourse.Commands
{
  public class CmdCourseTransfer : IDisposable
  {
    private readonly string _exportFile;
    private readonly string _templateFile;
    private readonly string _origFile;

    private OcadReader _reader;
    private IList<StringParamIndex> _origStrIdxList;
    private Dictionary<StringParamIndex, CourseViewPar> _courseViewDict;
    private readonly Dictionary<int, int> _oldNewElemDict = new Dictionary<int, int>();

    private Ocad9Writer _writer;
    private Setup _setup;

    private bool _disposed;

    public CmdCourseTransfer(string exportFile, string templateFile, string origFile)
    {
      _exportFile = exportFile;
      _templateFile = templateFile;
      _origFile = origFile;
    }

    public void Dispose()
    {
      _disposed = true;
      if (_reader != null)
      {
        _reader.Close();
        _reader.Dispose();
        _reader = null;
      }
      if (_writer != null)
      {
        _writer.Close();
        _writer.Dispose();
        _writer = null;
      }
    }

    private OcadReader Reader
    {
      get
      {
        if (_reader == null)
        {
          if (_disposed) throw new InvalidOperationException("disposed");
          _reader = OcadReader.Open(_origFile);
        }
        return _reader;
      }
    }
    private Setup Setup
    {
      get
      {
        if (_setup == null)
        { _setup = Reader.ReadSetup(); }
        return _setup;
      }
    }
    private IList<StringParamIndex> OrigStrIdxList
    {
      get
      {
        if (_origStrIdxList == null)
        { _origStrIdxList = Reader.ReadStringParamIndices(); }
        return _origStrIdxList;
      }
    }
    private Dictionary<StringParamIndex, CourseViewPar> CourseViewDict
    {
      get
      {
        if (_courseViewDict == null)
        {
          Dictionary<StringParamIndex, CourseViewPar> courseViewDict =
            new Dictionary<StringParamIndex, CourseViewPar>();
          foreach (var idx in OrigStrIdxList)
          {
            if (idx.Type == StringType.CourseView)
            {
              string stringParam = Reader.ReadStringParam(idx);
              CourseViewPar par = new CourseViewPar(stringParam);

              courseViewDict.Add(idx, par);
            }
          }
          _courseViewDict = courseViewDict;
        }
        return _courseViewDict;
      }
    }
    private Ocad9Writer Writer
    {
      get
      {
        if (_writer == null)
        {
          if (_disposed) throw new InvalidOperationException("disposed");

          System.IO.File.Copy(_templateFile, _exportFile, true);
          _writer = Ocad9Writer.AppendTo(_exportFile);

          if (_origFile != _templateFile)
          {
            using (OcadReader origReader = OcadReader.Open(_origFile))
            { TransferCourseSetting(_writer, origReader); }
          }
        }
        return _writer;
      }
    }

    public void Export(string prefix, IEnumerable<CostSectionlist> combs, string customLayoutCourse)
    {
      foreach (var comb in combs)
      {
        Course course = comb.Sections.ToSimpleCourse();
        string name;
        if (string.IsNullOrEmpty(comb.Name))
        { name = prefix; }
        else
        { name = string.Format("{0}{1}", prefix, comb.Name); }
        course.Name = name;
        Export(course, (int)Basics.Utils.Round(comb.Climb, 5), customLayoutCourse);
      }
    }

    public void Export(Course course, int climb, string customLayoutCourse)
    {
      Export(course, climb);

      foreach (var pair in CourseViewDict)
      {
        CourseViewPar par = pair.Value;
        if (par.Name != customLayoutCourse)
        { continue; }

        foreach (var section in course)
        {
          Control control = (Control)section;
          if (control.Name == par.Obj)
          {
            CourseViewPar expPar = new CourseViewPar(course.Name) { Obj = control.Name };
            int elemIdx = GetElemIdx(pair.Key);

            Writer.Append(StringType.CourseView, elemIdx, expPar.StringPar);
          }
        }
      }
    }
    private int GetElemIdx(StringParamIndex oldIdx)
    {
      if (!_oldNewElemDict.TryGetValue(oldIdx.ElemNummer, out int newIdx))
      {
        ElementIndex elemIdx = Reader.ReadIndex(oldIdx.ElemNummer - 1);
        Element elem = Reader.ReadElement(elemIdx);
        newIdx = Writer.Append(elem, elemIdx) + 1;
        //Element t = Writer.Reader.ReadElement(newIdx - 1);
        _oldNewElemDict.Add(oldIdx.ElemNummer, newIdx);
      }
      return newIdx;
    }
    public void Export(Course course, int climb)
    {
      CoursePar coursePar = CoursePar.Create(course);
      coursePar.Climb = climb;
      Writer.Append(StringType.Course, -1, coursePar.StringPar);
    }


    public static void TransferCourseSetting(Ocad9Writer writer, OcadReader reader)
    {
      // TODO do not use when controls exist in writer
      writer.DeleteElements(new int[] { 701000, 70200, 705000, 706000, 709000 });

      List<ControlHelper> controls = new List<ControlHelper>();
      foreach (var index in reader.ReadStringParamIndices())
      {
        if (index.Type == StringType.Control)
        {
          ControlHelper control = new ControlHelper
          { ParIndex = index, Par = reader.ReadStringParam(index) };

          controls.Add(control);
        }
        else if (index.Type == StringType.Class)
        {
          string par = reader.ReadStringParam(index);
          writer.Append(index.Type, -1, par);
        }

      }
      Setup rs = reader.ReadSetup();
      foreach (var control in controls)
      {
        Element elem = reader.ReadElement(control.ParIndex.ElemNummer - 1);
        if (elem != null)
        {
          elem.Geometry = elem.Geometry.Project(rs.Map2Prj);
          elem.IsGeometryProjected = true;
          control.ElementIndex = writer.Append(elem);
        }
      }

      foreach (var control in controls)
      {
        writer.Append(StringType.Control, control.ElementIndex + 1, control.Par);
      }


      IList<int> transferSymbols = new int[] { 709000 };
      foreach (var element in reader.Elements(true, null))
      {
        if (transferSymbols.Contains(element.Symbol))
        {
          writer.Append(element);
        }
      }
    }

    private class ControlHelper
    {
      public string Par;
      public StringParamIndex ParIndex;
      public int ElementIndex;
    }
  }
}

