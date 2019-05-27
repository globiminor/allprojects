using System;
using System.Collections.Generic;
using System.Linq;
using Ocad;

namespace OCourse.Ext
{
  public class SectionsBuilder
  {
    public interface IDisplayControl
    {
      Control Control { get; }
      Control Display { get; }
    }
    private class FromControl : IDisplayControl
    {
      public FromControl(Control control)
      { Control = control; }
      public Control Control { get; }
      Control IDisplayControl.Display => Control;

      public override string ToString() => $"FromControl: {Control}";
    }

    public delegate IList<IDisplayControl> AddEventHandler(int leg, ISection section,
      IList<IDisplayControl> fromControls, List<SimpleSection> sections, string where);

    public event AddEventHandler AddingBranch;

    public List<SimpleSection> GetSections(SectionCollection course)
    {
      List<SimpleSection> sections = new List<SimpleSection>();
      IList<IDisplayControl> lastControls = new List<IDisplayControl>();

      Dictionary<SimpleSection, List<SimpleSection>> uniqueSections = GetUniqueSections(course);

      foreach (var section in course)
      {
        lastControls = AppendSections(-1, section, lastControls, sections, null, uniqueSections);
      }
      return sections;
    }

    private Dictionary<SimpleSection, List<SimpleSection>> GetUniqueSections(SectionCollection course)
    {
      VariationBuilder v = new VariationBuilder(course);
      IReadOnlyList<SimpleSection> allSections = v.GetSimpleSections();
      return GetUniqueSections(allSections);
    }
    private Dictionary<SimpleSection, List<SimpleSection>> GetUniqueSections(IEnumerable<SimpleSection> sections)
    {
      Dictionary<SimpleSection, List<SimpleSection>> unique = new Dictionary<SimpleSection, List<SimpleSection>>(new SimpleSection.EqualFullComparer());
      foreach (var section in sections)
      {
        if (!unique.TryGetValue(section, out List<SimpleSection> equals))
        {
          equals = new List<SimpleSection>();
          unique.Add(section, equals);
        }
        equals.Add(section);
      }
      return unique;
    }

    public List<SimpleSection> GetSections(SectionCollection course, int leg)
    {
      List<SimpleSection> sections = new List<SimpleSection>();
      IList<IDisplayControl> lastControls = new List<IDisplayControl>();
      Dictionary<SimpleSection, List<SimpleSection>> uniqueSections = GetUniqueSections(course);

      foreach (var section in course)
      {
        lastControls = AppendSections(leg, section, lastControls, sections, null, uniqueSections);
      }
      return sections;
    }

    private IList<IDisplayControl> AppendSections(int leg, ISection section,
      IList<IDisplayControl> fromControls, List<SimpleSection> sections, string where,
      Dictionary<SimpleSection, List<SimpleSection>> uniqueAllSections)
    {
      IList<IDisplayControl> toControls;
      if (section is Control to)
      {
        foreach (var from in fromControls)
        {
          SimpleSection simple = new SimpleSection(from.Control, to);
          simple.AndWhere(where);
          if (uniqueAllSections.ContainsKey(simple))
          {
            simple.From = from.Display;
            sections.Add(simple);
          }
        }
        toControls = new IDisplayControl[] { new FromControl(to) };
      }
      else if (section is Fork fork)
      {
        int nBranches = fork.Branches.Count;
        List<IDisplayControl> tos = new List<IDisplayControl>(nBranches);
        for (int iBranch = 0; iBranch < nBranches; iBranch++)
        {
          Fork.Branch branch = (Fork.Branch)fork.Branches[iBranch];
          IDisplayControl fromControl = fromControls.Count > 1 ? fromControls[iBranch] : fromControls[0];

          IList<IDisplayControl> forksTo = AppendSections(leg, branch, new[] { fromControl }, sections, where, uniqueAllSections);
          tos.AddRange(forksTo);
        }
        toControls = tos;
      }
      else if (section is Fork.Branch forkBranch)
      {
        foreach (var control in forkBranch)
        {
          fromControls = AppendSections(leg, control, fromControls, sections, where, uniqueAllSections);
        }
        toControls = fromControls;
      }
      else if (section is Variation var)
      {
        List<IDisplayControl> tos = new List<IDisplayControl>();
        int iVar = 0;
        foreach (Variation.Branch branch in var.Branches)
        {
          IList<IDisplayControl> branchFroms;
          if (fromControls.Count > 1)
          {
            branchFroms = new List<IDisplayControl>();
            foreach (var iLeg in branch.Legs)
            {
              branchFroms.Add(fromControls[iVar]);
              iVar++;
            }
          }
          else
          {
            branchFroms = fromControls;
          }
          List<SimpleSection> addSections = (leg < 0 || branch.Legs.Contains(leg)) ?
            sections : new List<SimpleSection>();

          IList<IDisplayControl> varTos = AppendSections(leg, branch, branchFroms, addSections, where, uniqueAllSections);
          if (varTos.Count < branch.Legs.Count)
          {
            if (varTos.Count != 1) throw new ArgumentException($"expected 1, got {varTos.Count}");
            for (int iLeg = 0; iLeg < branch.Legs.Count; iLeg++)
            { tos.Add(varTos[0]); }
          }
          else
          { tos.AddRange(varTos); }
        }
        toControls = tos;
      }
      else if (section is Variation.Branch varBranch)
      {
        if (AddingBranch != null)
        {
          fromControls = AddingBranch(leg, varBranch, fromControls, sections, varBranch.GetWhere());
        }

        foreach (var control in varBranch)
        {
          fromControls = AppendSections(leg, control, fromControls, sections, varBranch.GetWhere(), uniqueAllSections);
        }
        toControls = fromControls;
      }

      else
      {
        throw new NotImplementedException("Unhandled ISection type " + section.GetType());
      }
      return toControls;
    }

    public List<SimpleSection> GetParts(SectionCollection course, int leg)
    {
      List<SimpleSection> sections = new List<SimpleSection>();
      IList<IDisplayControl> lastControls = new List<IDisplayControl>();

      List<IDisplayControl> inter = new List<IDisplayControl>();
      IDisplayControl lastControl = null;
      foreach (var section in course)
      {
        if (section is Control)
        {
          lastControl = new FromControl((Control)section);
          inter.Add(lastControl);
        }
        else
        {
          if (lastControl != null)
          {
            lastControls = AppendParts(leg, lastControl.Control, inter, lastControls, sections, null);
            lastControl = null;
            inter = new List<IDisplayControl>();
          }
          lastControls = AppendParts(leg, section, inter, lastControls, sections, null);
        }
      }
      if (lastControl != null)
      {
        AppendParts(leg, lastControl.Control, inter, lastControls, sections, null);
      }

      return sections;
    }

    private IList<IDisplayControl> AppendParts(int leg, ISection section, List<IDisplayControl> inter,
      IList<IDisplayControl> fromControls, List<SimpleSection> sections, string where)
    {
      IList<IDisplayControl> toControls;
      if (section is Control to)
      {
        foreach (var from in fromControls)
        {
          MultiSection simple = new MultiSection(from.Control, to);
          simple.Inter.AddRange(inter.Select(x => x.Control));
          simple.AndWhere(where);
          sections.Add(simple);
        }
        toControls = new IDisplayControl[] { new FromControl(to) };
      }
      else if (section is Fork fork)
      {
        List<IDisplayControl> tos = new List<IDisplayControl>(fork.Branches.Count);
        char code = 'A';
        foreach (var o in fork.Branches)
        {
          Fork.Branch branch = (Fork.Branch)o;
          string w = string.Format("'{0}' = '{0}'", code);
          if (!string.IsNullOrEmpty(where))
          {
            w = string.Format("({0}) AND {1}", where, w);
          }
          IList<IDisplayControl> forksTo = AppendParts(leg, branch, null, fromControls, sections, w);
          tos.AddRange(forksTo);

          code++;
        }
        toControls = tos;
      }
      else if (section is Fork.Branch forkBranch)
      {
        List<IDisplayControl> interBranch = new List<IDisplayControl>();
        Control lastControl = null;
        foreach (var control in forkBranch)
        {
          if (control is Control)
          {
            lastControl = (Control)control;
            interBranch.Add(new FromControl(lastControl));
          }
          else
          {
            if (lastControl != null)
            {
              fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, where);
              lastControl = null;
              interBranch = new List<IDisplayControl>();
            }
            fromControls = AppendParts(leg, control, interBranch, fromControls, sections, where);
          }
        }
        if (lastControl != null)
        {
          fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, where);
        }
        toControls = fromControls;
      }
      else if (section is Variation var)
      {
        List<IDisplayControl> tos = new List<IDisplayControl>();
        foreach (Variation.Branch branch in var.Branches)
        {
          if (leg < 0 || branch.Legs.Contains(leg))
          {
            IList<IDisplayControl> varTos = AppendParts(leg, branch, null, fromControls, sections, where);
            tos.AddRange(varTos);
          }
        }
        toControls = tos;
      }
      else if (section is Variation.Branch varBranch)
      {
        if (AddingBranch != null)
        { fromControls = AddingBranch(leg, varBranch, fromControls, sections, varBranch.GetWhere()); }

        List<IDisplayControl> interBranch = new List<IDisplayControl>();
        Control lastControl = null;
        foreach (var control in varBranch)
        {
          if (control is Control)
          {
            lastControl = (Control)control;
            interBranch.Add(new FromControl(lastControl));
          }
          else
          {
            if (lastControl != null)
            {
              fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, varBranch.GetWhere());
              lastControl = null;
              interBranch = new List<IDisplayControl>();
            }
            fromControls = AppendParts(leg, control, interBranch, fromControls, sections, varBranch.GetWhere());
          }
        }
        if (lastControl != null)
        {
          fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, varBranch.GetWhere());
        }
        toControls = fromControls;
      }

      else
      {
        throw new NotImplementedException("Unhandled ISection type " + section.GetType());
      }
      return toControls;
    }

  }
}