using System.Collections.Generic;
using Ocad;

namespace OCourse.Ext
{
  internal class NextControlList
  {
    private List<NextControl> _list = new List<NextControl>();
    private bool _codesAssured;

    public int Count
    { get { return _list.Count; } }

    public List<NextControl> List
    {
      get { return _list; }
    }

    public void AssureCodes()
    {
      if (_codesAssured)
      { return; }

      Dictionary<NextControl, List<NextControl>> dict =
        new Dictionary<NextControl, List<NextControl>>(new NextControl.EqualityComparer());
      foreach (NextControl next in _list)
      {
        List<NextControl> nextList;
        if (!dict.TryGetValue(next, out nextList))
        {
          nextList = new List<NextControl>();
          dict.Add(next, nextList);
        }
        nextList.Add(next);
      }
      if (dict.Count > 1)
      {
        char code = 'A';
        code--;
        foreach (List<NextControl> nextList in dict.Values)
        {
          code++;
          foreach (NextControl next in nextList)
          {
            next.Code = code;
          }
        }
      }
      _codesAssured = true;
    }
    public NextControl Add(SimpleSection section)
    {
      NextControl newNext = new NextControl(section.To);
      newNext.Where = section.Where;
      MultiSection m = section as MultiSection;
      if (m != null)
      { newNext.Inter.AddRange(m.Inter); }
      _list.Add(newNext);

      return newNext;
    }

    public NextControlList Clone()
    {
      NextControlList clone = new NextControlList();
      clone._codesAssured = _codesAssured;
      clone._list = new List<NextControl>(_list);

      return clone;
    }
  }
}