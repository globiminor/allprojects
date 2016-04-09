using System;
using System.Collections.Generic;
using System.Text;

namespace Ocad
{
  /// <summary>
  /// Summary description for Course.
  /// </summary>
  public class Course : SectionCollection
  {
    public class NameComparer : IComparer<Course>
    {
      public int Compare(Course x, Course y)
      {
        int cmp = x._name.CompareTo(y._name);
        return cmp;
      }
    }

    private string _name;
    private double _climb;
    private double _extraDistance;

    public Course(string name)
    {
      _name = name;
    }
    protected Course() // for cloning
    { }
    public new Course Clone()
    {
      Course clone = (Course)CloneCore();
      return clone;
    }
    protected override SectionCollection CloneCore()
    {
      Course clone = (Course)base.CloneCore();
      clone._name = _name;
      return clone;
    }


    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    public double Climb
    {
      get { return _climb; }
      set { _climb = value; }
    }

    public double ExtraDistance
    {
      get { return _extraDistance; }
      set { _extraDistance = value; }
    }

    public override string ToString()
    {
      return string.Format(
        "Name  : {0}\n" +
        "{1}\n" +
        _name, "__");
    }

    internal void AssignLegs()
    {
      int nLegs = 0;

      for (LinkedListNode<ISection> currentNode = First;
        currentNode != null; currentNode = currentNode.Next)
      {
        ISection current = currentNode.Value;
        if (nLegs == 0 && current is Fork)
        { nLegs = ((Fork)current).Branches.Count; }
        else if (nLegs == 0 && current is Variation)
        {
          foreach (Variation.Branch branch in ((Variation)current).Branches)
          { nLegs += branch.Legs.Count; }
        }
      }
    }

    public string ToShortString()
    {
      StringBuilder txt = new StringBuilder();
      int nLeg = this.LegCount();
      for (int i = 0; i < nLeg; i++)
      {
        txt.AppendFormat("{0,6}  ", Name + "." + (i + 1));
        txt.Append(ToShortString(i + 1) + Environment.NewLine);
      }
      return txt.ToString();
    }

    //public List<SimpleSection> GetAllSections()
    //{
    //  List<List<Control>> explicitLegList = GetExplicitCombinations();
    //  List<SimpleSection> allSections = new List<SimpleSection>();
    //  foreach (List<Control> list in explicitLegList)
    //  {
    //    Control pre = null;
    //    foreach (Control post in list)
    //    {
    //      int symbol = post.Element.Symbol;
    //      if (symbol == 701000 || symbol == 702000 || symbol == 706000)
    //      { }
    //      else if (symbol == 705000)
    //      { continue; }
    //      else
    //      { throw new InvalidProgramException("Unhandled symbol " + symbol); }
    //      if (pre != null)
    //      { allSections.Add(new SimpleSection(pre, post)); }
    //      pre = post;
    //    }
    //  }
    //  return allSections;
    //}
  }
}
