using System;
using System.Collections.Generic;
using Ocad;

namespace OCourse.Ext
{
  public class SectionsBuilder
  {
    public delegate IList<Control> AddEventHandler(int leg, ISection section,
                                                   IList<Control> fromControls, List<SimpleSection> sections, string where);
    public event AddEventHandler AddingBranch;

    public List<SimpleSection> GetSections(SectionCollection course)
    {
      List<SimpleSection> sections = new List<SimpleSection>();
      IList<Control> lastControls = new List<Control>();

      foreach (ISection section in course)
      {
        lastControls = AppendSections(-1, section, lastControls, sections, null);
      }
      return sections;
    }

    public List<SimpleSection> GetSections(SectionCollection course, int leg)
    {
      List<SimpleSection> sections = new List<SimpleSection>();
      IList<Control> lastControls = new List<Control>();

      foreach (ISection section in course)
      {
        lastControls = AppendSections(leg, section, lastControls, sections, null);
      }
      return sections;
    }

    private IList<Control> AppendSections(int leg, ISection section,
      IList<Control> fromControls, List<SimpleSection> sections, string where)
    {
      IList<Control> toControls;
      if (section is Control)
      {
        Control to = (Control)section;
        foreach (Control from in fromControls)
        {
          SimpleSection simple = new SimpleSection(from, to);
          simple.AndWhere(where);
          sections.Add(simple);
        }
        toControls = new Control[] { to };
      }
      else if (section is Fork)
      {
        Fork fork = (Fork)section;
        List<Control> tos = new List<Control>(fork.Branches.Count);
        foreach (Fork.Branch branch in fork.Branches)
        {
          IList<Control> forksTo = AppendSections(leg, branch, fromControls, sections, where);
          tos.AddRange(forksTo);
        }
        toControls = tos;
      }
      else if (section is Fork.Branch)
      {
        Fork.Branch branch = (Fork.Branch)section;
        foreach (ISection control in branch)
        {
          fromControls = AppendSections(leg, control, fromControls, sections, where);
        }
        toControls = fromControls;
      }
      else if (section is Variation)
      {
        Variation var = (Variation)section;
        List<Control> tos = new List<Control>();
        foreach (Variation.Branch branch in var.Branches)
        {
          if (leg < 0 || branch.Legs.Contains(leg))
          {
            IList<Control> varTos = AppendSections(leg, branch, fromControls, sections, where);
            tos.AddRange(varTos);
          }
        }
        toControls = tos;
      }
      else if (section is Variation.Branch)
      {
        Variation.Branch branch = (Variation.Branch)section;

        if (AddingBranch != null)
        { fromControls = AddingBranch(leg, branch, fromControls, sections, branch.GetWhere()); }

        foreach (ISection control in branch)
        {
          fromControls = AppendSections(leg, control, fromControls, sections, branch.GetWhere());
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
      IList<Control> lastControls = new List<Control>();

      List<Control> inter = new List<Control>();
      Control lastControl = null;
      foreach (ISection section in course)
      {
        if (section is Control)
        {
          lastControl = (Control)section;
          inter.Add(lastControl);
        }
        else
        {
          if (lastControl != null)
          {
            lastControls = AppendParts(leg, lastControl, inter, lastControls, sections, null);
            lastControl = null;
            inter = new List<Control>();
          }
          lastControls = AppendParts(leg, section, inter, lastControls, sections, null);
        }
      }
      if (lastControl != null)
      {
        AppendParts(leg, lastControl, inter, lastControls, sections, null);
      }

      return sections;
    }

    private IList<Control> AppendParts(int leg, ISection section, List<Control> inter,
      IList<Control> fromControls, List<SimpleSection> sections, string where)
    {
      IList<Control> toControls;
      if (section is Control)
      {
        Control to = (Control)section;
        foreach (Control from in fromControls)
        {
          MultiSection simple = new MultiSection(from, to);
          simple.Inter.AddRange(inter);
          simple.AndWhere(where);
          sections.Add(simple);
        }
        toControls = new Control[] { to };
      }
      else if (section is Fork)
      {
        Fork fork = (Fork)section;
        List<Control> tos = new List<Control>(fork.Branches.Count);
        char code = 'A';
        foreach (Fork.Branch branch in fork.Branches)
        {
          string w = string.Format("'{0}' = '{0}'", code);
          if (!string.IsNullOrEmpty(where))
          {
            w = string.Format("({0}) AND {1}", where, w);
          }
          IList<Control> forksTo = AppendParts(leg, branch, null, fromControls, sections, w);
          tos.AddRange(forksTo);

          code++;
        }
        toControls = tos;
      }
      else if (section is Fork.Branch)
      {
        Fork.Branch branch = (Fork.Branch)section;
        List<Control> interBranch = new List<Control>();
        Control lastControl = null;
        foreach (ISection control in branch)
        {
          if (control is Control)
          {
            lastControl = (Control)control;
            interBranch.Add(lastControl);
          }
          else
          {
            if (lastControl != null)
            {
              fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, where);
              lastControl = null;
              interBranch = new List<Control>();
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
      else if (section is Variation)
      {
        Variation var = (Variation)section;
        List<Control> tos = new List<Control>();
        foreach (Variation.Branch branch in var.Branches)
        {
          if (leg < 0 || branch.Legs.Contains(leg))
          {
            IList<Control> varTos = AppendParts(leg, branch, null, fromControls, sections, where);
            tos.AddRange(varTos);
          }
        }
        toControls = tos;
      }
      else if (section is Variation.Branch)
      {
        Variation.Branch branch = (Variation.Branch)section;

        if (AddingBranch != null)
        { fromControls = AddingBranch(leg, branch, fromControls, sections, branch.GetWhere()); }

        List<Control> interBranch = new List<Control>();
        Control lastControl = null;
        foreach (ISection control in branch)
        {
          if (control is Control)
          {
            lastControl = (Control)control;
            interBranch.Add(lastControl);
          }
          else
          {
            if (lastControl != null)
            {
              fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, branch.GetWhere());
              lastControl = null;
              interBranch = new List<Control>();
            }
            fromControls = AppendParts(leg, control, interBranch, fromControls, sections, branch.GetWhere());
          }
        }
        if (lastControl != null)
        {
          fromControls = AppendParts(leg, lastControl, interBranch, fromControls, sections, branch.GetWhere());
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