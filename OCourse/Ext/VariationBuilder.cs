using Basics;
using Basics.Geom;
using Ocad;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public List<SectionList> BuildPermutations(int nPermutations, IList<SectionList> allVariations = null)
    {
      PermutationBuilder builder = new PermutationBuilder(_course);
      builder.VariationAdded += Builder_VariationAdded;
      List<SectionList> permutations = builder.BuildPermutations(nPermutations, allVariations);

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
