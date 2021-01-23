using System.Collections.Generic;
using System.Text;
using Ocad;

namespace OCourse.Ext
{
  public class NextControlList
  {
    private List<NextControl> _list = new List<NextControl>();
    private bool _codesAssured;

    public int Count
    { get { return _list.Count; } }

    public List<NextControl> List
    {
      get { return _list; }
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < _list.Count && i < 7; i++)
      {
        sb.Append($"{_list[i]}, ");
      }
      if (_list.Count > 8)
      { sb.Append("..., "); }
      if (_list.Count > 7)
      { sb.Append($"{_list[_list.Count - 1]}"); }
      else
      { sb.Remove(sb.Length - 2, 2); }
      return sb.ToString();
    }

    public void AssureCodes()
    {
      if (_codesAssured)
      { return; }

      Dictionary<NextControl, List<NextControl>> dict =
        new Dictionary<NextControl, List<NextControl>>(new NextControl.EqualityComparer());
      foreach (var next in _list)
      {
        if (!dict.TryGetValue(next, out List<NextControl> nextList))
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
        foreach (var nextList in dict.Values)
        {
          code++;
          foreach (var next in nextList)
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
      if (section is MultiSection m)
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