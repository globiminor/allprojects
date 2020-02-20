using System;
using System.Collections.Generic;
using System.Linq;
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

    private OcadWriter _writer;
    private Dictionary<string, Control> _targetControls;
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

    private List<Element> _courseElements;
    private List<Element> CourseElements
    {
      get
      {
        if (_courseElements == null)
        {
          List<Element> courseElements = new List<Element>();
          foreach (Element element in Reader.EnumGeoElements(null))
          {
            if (element.ObjectStringType != ObjectStringType.CsPreview)
            { continue; }
            courseElements.Add(element);
          }
          _courseElements = courseElements;
        }
        return _courseElements;
      }
    }

    private OcadWriter Writer
    {
      get
      {
        if (_writer == null)
        {
          if (_disposed) throw new InvalidOperationException("disposed");

          System.IO.File.Copy(_templateFile, _exportFile, true);
          _writer = OcadWriter.AppendTo(_exportFile);

          //if (_origFile != _templateFile)
          //{
          //  using (OcadReader origReader = OcadReader.Open(_origFile))
          //  { TransferCourseSetting(_writer, origReader); }
          //}
          _targetControls = _writer.ReadControls().ToDictionary(x => x.Name);
        }
        return _writer;
      }
    }

    public void Export(IEnumerable<Course> courses, string customLayoutCourse)
    {
      foreach (var course in courses)
      {
        Export(course, (int)course.Climb, customLayoutCourse);
      }
    }

    public void Export(Course course, int climb, string customLayoutCourse)
    {
      Export(course, climb);

      ExportCourseStringParamViews(course, customLayoutCourse);

      ExportCourseElementViews(course, customLayoutCourse);
    }

    /// <summary>
    /// Export -OCAD11
    /// </summary>
    private void ExportCourseStringParamViews(Course course, string customLayoutCourse)
    {
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

    /// <summary>
    /// Export -OCAD11
    /// </summary>
    private void ExportCourseElementViews(Course course, string customLayoutCourse)
    {
      foreach (var elem in CourseElements)
      {
        string origObjectString = elem.ObjectString ?? string.Empty;
        if (!origObjectString.StartsWith(customLayoutCourse))
        { continue; }

        MultiParam par = new ExportPar(origObjectString);
        if (par.Name != customLayoutCourse)
        { continue; }

        par.Name = course.Name;
        try
        {
          elem.ObjectString = par.StringPar.Trim('\t');
          Writer.Append(elem);
        }
        finally
        { elem.ObjectString = origObjectString; }
      }
    }


    private int GetElemIdx(StringParamIndex oldIdx)
    {
      if (!_oldNewElemDict.TryGetValue(oldIdx.ElemNummer, out int newIdx))
      {
        ElementIndex elemIdx = Reader.ReadIndex(oldIdx.ElemNummer - 1);
        Reader.ReadElement(elemIdx, out GeoElement elem);
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

      foreach (SimpleSection sec in course.GetAllSections())
      {
        foreach (Control ctr in new[] { sec.From, sec.To })
        {
          if (!_targetControls.ContainsKey(ctr.Name) && ctr.Code != ControlCode.MapChange)
          {
            Writer.Append(ctr);
            _targetControls.Add(ctr.Name, ctr);
          }
        }
      }
    }


    public static void TransferCourseSetting(OcadWriter writer, OcadReader reader)
    {
      // TODO do not use when controls exist in writer
      writer.DeleteElements(new int[] { 701000, 70200, 705000, 706000, 709000 });

      List<ControlHelper> controls = new List<ControlHelper>();
      foreach (var index in reader.ReadStringParamIndices())
      {
        string sp = reader.ReadStringParam(index);
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
        reader.ReadElement(control.ParIndex.ElemNummer - 1, out GeoElement elem);
        if (elem != null)
        {
          // writer may have different coordinate settings!
          control.ElementIndex = writer.Append(elem);
        }
      }

      foreach (var control in controls)
      {
        writer.Append(StringType.Control, control.ElementIndex + 1, control.Par);
      }


      IList<int> transferSymbols = new int[] { 709000 };
      foreach (var element in reader.EnumGeoElements(null))
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

