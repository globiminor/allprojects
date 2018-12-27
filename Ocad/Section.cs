using System;
using System.Collections.Generic;
using System.Text;

namespace Ocad
{
  /// <summary>
  /// Summary description for Section.
  /// </summary>
  public interface ISection : ICloneable
  {
    new ISection Clone();
  }

  public class SimpleSection
  {
    public class EqualFullComparer : IEqualityComparer<SimpleSection>
    {
      public bool Equals(SimpleSection x, SimpleSection y)
      {
        bool equals = x.From.Name == y.From.Name 
          && x.To?.Name == y.To?.Name 
          && x.Where == y.Where;
        return equals;
      }
      public int GetHashCode(SimpleSection t)
      {
        int code = t.From.Name.GetHashCode();
        if (t.To != null)
        {
          code += 3 * t.To.Name.GetHashCode();
        }
        return code;
      }
    }

    public class EqualControlsComparer : IEqualityComparer<SimpleSection>
    {
      public bool Equals(SimpleSection x, SimpleSection y)
      {
        bool equals = x.From == y.From && x.To == y.To;
        return equals;
      }
      public int GetHashCode(SimpleSection t)
      {
        int code = t.From.Name.GetHashCode();
        if (t.To != null)
        {
          code += 3 * t.To.Name.GetHashCode();
        }
        return code;
      }
    }
    public Control From;
    public Control To;

    private string _where;

    public string Where { get { return _where; } }

    public SimpleSection(Control from, Control to)
    {
      From = from;
      To = to;

      if (to.Text != null && to.Text.StartsWith("where ", StringComparison.InvariantCultureIgnoreCase))
      {
        _where = to.Text.Substring(6);
      }
    }
    public string SetWhere(string where)
    {
      _where = where;
      return _where;
    }

    public string AndWhere(string andCondition)
    {
      if (string.IsNullOrEmpty(andCondition))
      {
        return _where;
      }

      if (string.IsNullOrEmpty(_where))
      {
        _where = andCondition;
      }
      else
      {
        _where = string.Format("({0}) AND {1}", _where, andCondition);
      }
      return _where;
    }

    public override string ToString()
    {
      if (string.IsNullOrEmpty(Where)) return string.Format("{0}-{1}", From.Name, To.Name);
      else
      {
        return string.Format("{0}-{1}; {2}", From.Name, To.Name, Where);
      }
    }
  }

  public abstract class SplitSection : ISection
  {
    #region nested classes
    public abstract class Branch : SectionCollection, ISection
    {
      protected Branch() // for Cloning
      { }
      protected Branch(IEnumerable<ISection> sections)
      {
        foreach (var section in sections)
        { AddLast(section); }
      }

      ISection ISection.Clone()
      { return Clone(); }
      public new Branch Clone()
      {
        Branch clone = (Branch)CloneCore();
        return clone;
      }

      internal abstract void SetPre(Control control);
      internal abstract void SetPost(Control control);
    }
    #endregion

    private List<Branch> _branchList;

    public SplitSection()
    {
      _branchList = new List<Branch>();
    }

    object ICloneable.Clone()
    { return Clone(); }
    ISection ISection.Clone()
    { return Clone(); }
    public SplitSection Clone()
    {
      SplitSection clone = CloneCore();
      return clone;
    }
    protected virtual SplitSection CloneCore()
    {
      SplitSection clone = (SplitSection)Activator.CreateInstance(GetType(), true);
      clone._branchList = new List<Branch>();
      foreach (var branch in _branchList)
      {
        clone._branchList.Add(branch.Clone());
      }
      return clone;
    }

    public List<Branch> Branches
    { get { return _branchList; } }

    public abstract int LegCount();
    public abstract Branch GetExplicitBranch(int index);

    public abstract List<Control> GetExplicitCombination(int index);
  }
  public class Fork : SplitSection
  {
    #region nested classes
    public new class Branch : SplitSection.Branch
    {
      private List<int> _legList;
      private ISection _pre;
      private ISection _post;

      protected Branch() // for cloning
      { }
      public Branch(IEnumerable<ISection> sections)
        : base(sections)
      {
        _legList = new List<int>();
      }
      public new Branch Clone()
      {
        Branch clone = (Branch)CloneCore();
        return clone;
      }
      protected override SectionCollection CloneCore()
      {
        Branch clone = (Branch)base.CloneCore();
        clone._legList = new List<int>();
        clone._legList.AddRange(_legList);
        return clone;
      }

      internal override void SetPre(Control control)
      {
        _pre = control;
      }
      internal void SetPre(SplitSection.Branch branch)
      {
        _pre = branch;
      }

      internal override void SetPost(Control control)
      {
        _post = control;
      }
      internal void SetPost(SplitSection.Branch branch)
      {
        _post = branch;
      }

      public IList<int> Legs
      {
        get { return _legList; }
      }
    }
    #endregion

    public override int LegCount()
    {
      return Branches.Count;
    }
    public override SplitSection.Branch GetExplicitBranch(int index)
    {
      return Branches[index];
    }
    public override List<Control> GetExplicitCombination(int index)
    {
      List<Control> list = new List<Control>(Branches[index].Count);
      foreach (var section in Branches[index])
      {
        Control control = (Control)section;
        list.Add(control);
      }
      return list;
    }
  }

  public class Variation : SplitSection
  {
    #region nested classes
    public new class Branch : SplitSection.Branch
    {
      private List<int> _legList;
      private List<ISection> _preList;
      private List<ISection> _postList;

      protected Branch()  // for cloning
      { }
      public Branch(IEnumerable<int> legs, IEnumerable<ISection> sections)
        : base(sections)
      {
        _legList = new List<int>(legs);
      }
      public new Branch Clone()
      {
        Branch clone = (Branch)CloneCore();
        return clone;
      }
      protected override SectionCollection CloneCore()
      {
        Branch clone = (Branch)base.CloneCore();
        clone._legList = new List<int>();
        clone._legList.AddRange(_legList);
        return clone;
      }

      public IList<int> Legs
      {
        get { return _legList; }
      }

      internal override void SetPre(Control control)
      {
        int n = _legList.Count;
        _preList = new List<ISection>(n);
        for (int i = 0; i < n; i++)
        {
          _preList.Add(control);
        }
      }
      internal void SetPre(SplitSection.Branch section, int index)
      {
        if (_preList == null)
        { _preList = Alloc(_legList.Count); }
        _preList[index] = section;
      }

      internal override void SetPost(Control control)
      {
        int n = _legList.Count;
        _postList = new List<ISection>(n);
        for (int i = 0; i < n; i++)
        {
          _postList.Add(control);
        }
      }

      internal void SetPost(SplitSection.Branch section, int index)
      {
        if (_postList == null)
        { _postList = Alloc(_legList.Count); }
        _postList[index] = section;
      }

      private List<ISection> Alloc(int size)
      {
        List<ISection> list = new List<ISection>(size);
        for (int i = 0; i < size; i++)
        { list.Add(null); }
        return list;
      }


      internal List<string> GetValidLegCombinations(int leg, LinkedListNode<ISection> next, string pre)
      {
        if (Count == 0)
        { return GetValidCombinationStrings(leg, next, pre); }

        List<string> combs = new List<string>();
        foreach (var comb in GetValidCombinationStrings(leg, First, ""))
        {
          combs.AddRange(GetValidCombinationStrings(leg, next, pre + comb));
        }
        return combs;
      }

      public string GetWhere()
      {
        string legsList = GetLegsListToString();
        string legsWhere = string.Format("leg in ({0})", legsList);
        return legsWhere;
      }

      public string GetLegsListToString()
      {
        StringBuilder legsList = new StringBuilder();
        foreach (var i in Legs)
        {
          if (legsList.Length > 0) legsList.Append(",");
          legsList.Append(i);
        }
        return legsList.ToString();
      }
    }
    #endregion

    public override SplitSection.Branch GetExplicitBranch(int index)
    {
      int nLegs = 0;
      foreach (var o in Branches)
      {
        Variation.Branch branch = (Variation.Branch)o;
        nLegs += branch.Legs.Count;
        if (nLegs > index)
        { return branch; }
      }
      return null;
    }

    public override List<Control> GetExplicitCombination(int leg)
    {
      foreach (var o in Branches)
      {
        Variation.Branch branch = (Variation.Branch)o;
        int index = 0;
        foreach (var iLeg in branch.Legs)
        {
          if (iLeg == leg)
          {
            List<Control> list = new List<Control>();
            foreach (var section in branch)
            {
              if (section is Control)
              { list.Add((Control)section); }
              else if (section is Fork)
              { list.AddRange(((Fork)section).GetExplicitCombination(index)); }
              else if (section is Variation)
              { list.AddRange(((Variation)section).GetExplicitCombination(leg)); }
            }
            return list;
          }
          index++;
        }
      }
      return null;
    }

    public override int LegCount()
    {
      int nLegs = 0;
      foreach (var o in Branches)
      {
        Variation.Branch branch = (Variation.Branch)o;
        nLegs += branch.Legs.Count;
      }
      return nLegs;
    }


    public Branch GetBranch(int leg)
    {
      foreach (var o in Branches)
      {
        Variation.Branch branch = (Variation.Branch)o;
        if (branch.Legs.IndexOf(leg) >= 0)
        {
          return branch;
        }
      }
      return null;
    }

    public int GetBranchIndex(int leg)
    {
      int index = 0;
      foreach (var o in Branches)
      {
        Variation.Branch branch = (Variation.Branch)o;
        foreach (var iLeg in branch.Legs)
        {
          if (iLeg == leg)
          {
            return index;
          }
        }
        index++;
      }
      return -1;
    }
  }

}
