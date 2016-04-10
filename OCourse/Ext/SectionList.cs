using System;
using System.Collections.Generic;
using System.Text;
using Ocad;
using Ocad.StringParams;

namespace OCourse.Ext
{
  public class SectionList
  {
    public class NameComparer : IComparer<SectionList>
    {
      public int Compare(SectionList x, SectionList y)
      {
        return x.GetName().CompareTo(y.GetName());
      }
    }

    public class EqualControlNamesComparer : IEqualityComparer<SectionList>
    {
      public bool Equals(SectionList x, SectionList y)
      {
        int n = x._nextControls.Count;
        if (n != y._nextControls.Count)
        {
          return false;
        }
        if (x._start.Name != y._start.Name)
        {
          return false;
        }
        for (int i = 0; i < n; i++)
        {
          if (x._nextControls[i].Control.Name != y._nextControls[i].Control.Name)
          {
            return false;
          }
        }
        return true;
      }

      public int GetHashCode(SectionList obj)
      {
        return obj._start.Name.GetHashCode();
      }
    }

    private Control _start;
    private readonly List<VariationBuilder.WhereInfo> Wheres = new List<VariationBuilder.WhereInfo>();
    private readonly List<NextControl> _nextControls = new List<NextControl>();

    private string _preName;

    public SectionList(Control start)
    {
      _start = start;
    }
    public Control Start
    { get { return _start; } }

    public int ControlsCount
    {
      get { return 1 + _nextControls.Count; }
    }

    public IEnumerable<Control> Controls
    {
      get
      {
        yield return _start;
        {
          foreach (NextControl next in _nextControls)
          {
            int nInter = next.Inter.Count - 1;
            for (int i = 0; i < nInter; i++)
            { yield return next.Inter[i]; }
            yield return next.Control;
          }
        }
      }
    }

    public IEnumerable<NextControl> NextControls
    {
      get
      {
        yield return new NextControl(_start);
        {
          foreach (NextControl next in _nextControls)
          { yield return next; }
        }
      }
    }

    public string PreName
    {
      get { return _preName; }
      set { _preName = value; }
    }

    public IList<Course> Parts
    {
      get { throw new NotImplementedException(); }
    }

    public string GetName()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("{0}", _preName);
      foreach (NextControl next in _nextControls)
      {
        if (next.Code != 0)
        { sb.Append(next.Code); }
      }
      return sb.ToString();
    }
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      foreach (Control control in Controls)
      {
        sb.AppendFormat("{0} ", control.Name);
      }
      string full = sb.ToString();
      if (full.Length > 200)
      {
        full = full.Substring(0, 100) + "..." + full.Substring(full.Length - 100);
      }
      return full;
    }

    public IList<SectionList> GetParts()
    {
      List<SectionList> parts = new List<SectionList>();
      SectionList current = null;
      foreach (NextControl next in NextControls)
      {
        if (next.Control.Code == ControlPar.TypeStart)
        {
          if (current != null)
          {
            parts.Add(current);
          }
          current = new SectionList(next.Control);
        }
        else
        {
          if (current != null)
          {
            current.Add(next);
          }
          else
          { // funny course !?!
            current = new SectionList(next.Control);
          }
        }
      }
      parts.Add(current);
      return parts;
    }

    public SectionList Clone()
    {
      SectionList clone = new SectionList(_start);
      clone._nextControls.AddRange(_nextControls);
      clone.Wheres.AddRange(Wheres);
      return clone;
    }

    public VariationBuilder.WhereInfo.State GetState()
    {
      if (Wheres.Count == 0)
      { return VariationBuilder.WhereInfo.State.Fulfilled; }

      List<VariationBuilder.WhereInfo> remove = new List<VariationBuilder.WhereInfo>();
      foreach (VariationBuilder.WhereInfo where in Wheres)
      {
        VariationBuilder.WhereInfo.State state = where.GetState(this);
        if (state == VariationBuilder.WhereInfo.State.Failed)
        { return VariationBuilder.WhereInfo.State.Failed; }

        if (state == VariationBuilder.WhereInfo.State.Unknown)
        { continue; }

        if (state == VariationBuilder.WhereInfo.State.Fulfilled)
        { remove.Add(where); }
      }
      foreach (VariationBuilder.WhereInfo rem in remove)
      { Wheres.Remove(rem); }

      if (Wheres.Count == 0)
      { return VariationBuilder.WhereInfo.State.Fulfilled; }

      return VariationBuilder.WhereInfo.State.Unknown;
    }

    public Course ToSimpleCourse()
    {
      Course course = new Course(GetName());
      foreach (Control control in Controls)
      {
        Control add;
        IList<string> nameParts = control.Name.Split('.');
        if (nameParts.Count == 1)
        { add = control; }
        else
        {
          add = control.Clone();
          add.Name = nameParts[0];
        }
        if (course.Last == null || ((Control)course.Last.Value).Name != add.Name)
        {
          course.AddLast(add);          
        }
      }
      return course;
    }

    public NextControl GetNextControl(int i)
    {
      return _nextControls[i];
    }

    public Control GetControl(int i)
    {
      if (i < 0)
      { i = _nextControls.Count + 1 + i; }

      if (i == 0) return Start;
      return _nextControls[i - 1].Control;
    }
    public NextControl Add(NextControl nextControl)
    {
      _nextControls.Add(nextControl);
      return nextControl;
    }

    public VariationBuilder.WhereInfo Add(VariationBuilder.WhereInfo where)
    {
      if (where == null)
      { return null; }
      Wheres.Add(where);
      return where;
    }

    public int IndexOf(NextControl next)
    {
      return _nextControls.IndexOf(next);
    }

    public int CountExisting(Control from, Control to)
    {
      int count = 0;
      Control c1 = Start;
      foreach (NextControl nextControl in _nextControls)
      {
        Control c0 = c1;
        c1 = nextControl.Control;
        if (c0 == from && c1 == to)
        {
          count++;
        }
      }
      return count;
    }

    public void SetLeg(int leg)
    {
      _preName = leg.ToString();
    }

    public static IList<SectionList> GetSimpleParts(IList<SectionList> combined)
    {
      EqualControlNamesComparer cmp = new EqualControlNamesComparer();
      Dictionary<SectionList, SectionList> partsDict = new Dictionary<SectionList, SectionList>(cmp);

      foreach (SectionList list in combined)
      {
        IList<SectionList> parts = list.GetParts();
        foreach (SectionList part in parts)
        {
          TryAdd(partsDict, part);
        }
      }

      List<SectionList> allParts = new List<SectionList>(partsDict.Keys);
      allParts.Sort(new NameComparer());

      return allParts;
    }

    private static void TryAdd(Dictionary<SectionList, SectionList> parts, SectionList part)
    {
      if (!parts.ContainsKey(part))
      {
        parts.Add(part, part);
      }
    }
  }
  public class MultiSection : SimpleSection
  {
    public readonly List<Control> Inter = new List<Control>();
    public MultiSection(Control from, Control to)
      : base(from, to)
    { }
  }
}