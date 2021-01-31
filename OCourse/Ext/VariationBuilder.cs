using Basics;
using Basics.Geom;
using Ocad;
using System;
using System.Collections.Generic;

namespace OCourse.Ext
{
  public partial class VariationBuilder
  {
    public delegate void VariationEventHandler(object sender, SectionList variation);
    public event VariationEventHandler VariationAdded;

    private readonly SectionCollection _origCourse;

    private readonly SectionCollection _course;
    private readonly Control _startWrapper;
    private readonly Control _endWrapper;

    public VariationBuilder(SectionCollection course)
    {
      _origCourse = course;

      SectionCollection init = course.Clone();

      _startWrapper = GetWrapperControl("++", init);
      init.AddFirst(_startWrapper);

      _endWrapper = GetWrapperControl("--", init);
      init.AddLast(_endWrapper);

      _course = init;
    }
    public List<SectionList> BuildPermutations(int nPermutations)
    {
      PermutationBuilder builder = new PermutationBuilder(_course);
      builder.VariationAdded += Builder_VariationAdded;
      List<SectionList> permutations = builder.BuildPermutations(nPermutations);

      permutations = GetReduced(permutations);

      return permutations;
    }

    private List<SectionList> GetReduced(List<SectionList> permutations)
    {
      if (_startWrapper == null && _endWrapper == null)
      { return permutations; }

      List<SectionList> reduceds = new List<SectionList>();
      foreach (var variation in permutations)
      {
        SectionList reduced = RemoveControls(variation, _startWrapper, _endWrapper);
        reduceds.Add(reduced);
      }

      return reduceds;
    }


    public static Control GetWrapperControl(string template, SectionCollection course)
    {
      string key = template;

      List<SimpleSection> sections = course.GetAllSections();
      Dictionary<string, Control> controls = new Dictionary<string, Control>();
      foreach (var section in sections)
      {
        controls[section.From.Name] = section.From;
        controls[section.To.Name] = section.To;
      }

      int i = 0;
      while (controls.ContainsKey(key))
      {
        key = $"{template}_{i}";
        i++;
      }

      return new Control(key, ControlCode.TextBlock);
    }
    public static void Split(Course course,
      Func<int, Dictionary<string, List<Control>>, Dictionary<string, List<Control>>, int> customWeight = null)
    {
      List<Control> controls = new List<Control>();
      Dictionary<string, List<Control>> dict = new Dictionary<string, List<Control>>();
      int iControl =  0;
      int iLastSplit = -1;
      foreach (Control control in course)
      {
        controls.Add(control);
        List<Control> multiControls = dict.GetOrCreateValue(control.Name);
        multiControls.Add(control);
        if (multiControls.Count > 1)
        { iLastSplit = iControl; }
        iControl++;
      }
      Dictionary<string, List<Control>> dictPre = new Dictionary<string, List<Control>>();
      dictPre.GetOrCreateValue(controls[0].Name).Add(controls[0]);
      int minDouble = int.MaxValue;
      int minSplit = -1;
      LinkedListNode<ISection> minNode = null;
      LinkedListNode<ISection> currentNode = course.First;
      for (int iSplit = 0; iSplit < controls.Count; iSplit++)
      {
        Control split = controls[iSplit];
        dictPre.GetOrCreateValue(split.Name);
        currentNode = currentNode.Next;

        bool lastSplit = false;
        if (dict[split.Name].Count <= 1 || 
           (lastSplit = dictPre[split.Name].Count + 1 == dict[split.Name].Count))
        {
          int nDouble = GetNDouble(dict, dictPre, lastSplit, customWeight);
          if (nDouble < minDouble
            || (nDouble == minDouble && Math.Abs(iSplit - iLastSplit / 2) < Math.Abs(minSplit - iLastSplit / 2)))
          {
            minDouble = nDouble;
            minSplit = iSplit;
            minNode = currentNode;
          }
        }
        dictPre[split.Name].Add(split);
      }

      if (minNode != null)
      {
        Control pre = (Control)minNode.Previous.Value;
        Control start = new Control($"S{pre.Name}", ControlCode.Start);
        foreach (IPoint p in pre.Element.Geometry.EnumPoints())
        {
          start.Element = new GeoElement(p);
          start.Element.Type = GeomType.point;
          start.Element.ObjectStringType = ObjectStringType.CsObject;
          start.Element.Symbol = 701000;

          Ocad.StringParams.ControlPar param = new Ocad.StringParams.ControlPar(pre.Element.ObjectString);
          param.Name = $"0S{pre.Name}";
          param.Type = 's';
          param.Id = $"S{pre.Name}";
          start.Element.ObjectString = param.StringPar; // pre.Element.ObjectString;
          break;
        }

        course.AddBefore(minNode, new Control(string.Empty, ControlCode.MapChange));

        course.AddBefore(minNode, start);
      }
    }

    private static int GetNDouble(Dictionary<string, List<Control>> dict,
      Dictionary<string, List<Control>> dictPre, bool lastSplit,
      Func<int, Dictionary<string, List<Control>>, Dictionary<string, List<Control>>, int> custom)
		{
      int nDouble = 0;
      foreach (var pair in dict)
      {
        if (pair.Value.Count == 1)
        { continue; }

        if (!dictPre.TryGetValue(pair.Key, out List<Control> pre))
        { pre = new List<Control>(); }
        int n0 = pre.Count;
        int n1 = pair.Value.Count - n0;
        int d = n1 - n0;

        nDouble += Math.Abs(d);
      }
      nDouble = 2 * nDouble + (lastSplit ? 1 : 0);

      nDouble = custom?.Invoke(nDouble, dict, dictPre) ?? nDouble;
      return nDouble;
    }


    public IReadOnlyList<SimpleSection> GetSimpleSections()
    {
      PermutationBuilder builder = new PermutationBuilder(_course);
      return builder.SimpleSections;
    }

    public List<SectionList> AnalyzeLeg(int leg)
    {
      List<SectionList> permuts;
      DummyControl.InsertDummies(_course, _course);
      try
      {
        LegBuilder builder = new LegBuilder(_course, leg);
        builder.VariationAdded += Builder_VariationAdded;
        permuts = builder.Analyze();
      }
      finally
      { DummyControl.RemoveDummies(_course); }

      permuts = GetReduced(permuts);
      foreach (var permutation in permuts)
      {
        permutation.SetLeg(leg);
      }

      return permuts;
    }
    public List<SectionList> Analyze(SectionCollection course)
    {
      CourseBuilder builder = new CourseBuilder(course);
      builder.VariationAdded += Builder_VariationAdded;

      List<SectionList> permuts = builder.Analyze();
      return permuts;
    }

    public int EstimatePermutations()
    {
      PermutationBuilder builder = new PermutationBuilder(_course);

      int permuts = builder.Estimate();
      return permuts;
    }

    void Builder_VariationAdded(object sender, SectionList variation)
    {
      VariationAdded?.Invoke(this, variation);
    }

    public static void InsertDummies(SectionCollection course)
    {
      DummyControl.InsertDummies(course, course);
    }
    public static void RemoveDummies(SectionCollection course)
    {
      DummyControl.RemoveDummies(course);
    }

    private class DummyControl : Control
    {
      public static void InsertDummies(SectionCollection course, SectionCollection reference)
      {
        foreach (var section in course)
        {
          if (section is Variation var)
          {
            foreach (var branch in var.Branches)
            {
              InsertDummies(branch, reference);
              if (branch.Count == 0)
              {
                Control dd = GetWrapperControl("dd", reference);
                branch.AddLast(new DummyControl { Name = dd.Name, Code = ControlCode.Control });
              }
            }
          }
        }
      }
      public static void RemoveDummies(SectionCollection course)
      {
        foreach (var section in course)
        {
          if (section is Variation var)
          {
            foreach (var branch in var.Branches)
            {
              RemoveDummies(branch);
              while (branch.Last?.Value is DummyControl)
              {
                branch.RemoveLast();
              }
            }
          }
        }
      }
    }

    private static SectionList RemoveControls(SectionList variation, Control startWrapper, Control endWrapper)
    {
      SectionList reduced = null;
      foreach (var control in variation.NextControls)
      {
        if (control.Control == startWrapper)
        {
          reduced?.Add(new NextControl(SectionList.LegStart));
          continue;
        }
        if (control.Control == endWrapper)
        { continue; }

        if (control.Control is DummyControl)
        { continue; }

        if (reduced == null)
        { reduced = new SectionList(control); }
        else
        { reduced.Add(control); }
      }
      return reduced;
    }

    public static Course BuildExplicit(Course course)
    {
      Dictionary<string, List<Control>> controls = new Dictionary<string, List<Control>>();
      Course expl = course.Clone();
      BuildExplicit(expl, controls);
      return expl;
    }

    public static void BuildExplicit(SectionCollection course, Dictionary<string, List<Control>> controls)
    {
      LinkedListNode<ISection> node = course.First;
      while (node != null)
      {
        ISection section = node.Value;
        if (section is Control control)
        {
          if (!controls.TryGetValue(control.Name, out List<Control> equalControls))
          {
            equalControls = new List<Control>();
            controls.Add(control.Name, equalControls);
            equalControls.Add(control);
          }
          else
          {
            Control eq = control.Clone();
            eq.Name = eq.Name + "." + equalControls.Count;
            equalControls.Add(eq);

            node.Value = eq;
          }
        }
        else if (section is SplitSection split)
        {
          foreach (var branch in split.Branches)
          {
            BuildExplicit(branch, controls);
          }
        }

        node = node.Next;
      }
    }
  }
}
