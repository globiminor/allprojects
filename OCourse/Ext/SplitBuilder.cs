using Basics;
using Basics.Geom;
using Ocad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCourse.Ext
{
  public class SplitBuilder
  {
    public class Parameters
    {
      public bool Squared { get; set; } = true;
      public bool Centered { get; set; } = true;
      public Func<int, Control, Dictionary<string, List<Control>>, Dictionary<string, List<Control>>, int> CustomWeight { get; set; }
    }
    public Parameters Pars { get; }

    public SplitBuilder(Parameters pars)
    {
      Pars = pars;
    }

    public void Split(Course course)
    {
      List<Control> controls = new List<Control>();
      Dictionary<string, List<Control>> dict = new Dictionary<string, List<Control>>();
      int iControl = 0;
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
      int nMean = (iLastSplit + 1) / 2;
      Dictionary<string, List<Control>> dictPre = new Dictionary<string, List<Control>>();
      //dictPre.GetOrCreateValue(controls[0].Name).Add(controls[0]);
      int minDouble = int.MaxValue;
      int minSplit = -1;
      LinkedListNode<ISection> minNode = null;
      LinkedListNode<ISection> currentNode = course.First;

      for (int iSplit = 0; iSplit < controls.Count; iSplit++)
      {
        Control split = controls[iSplit];
        List<Control> pres = dictPre.GetOrCreateValue(split.Name);
        List<Control> totals = dict[split.Name];
        currentNode = currentNode.Next;

        bool lastSplit = false;
        pres.Add(split);
        if (dict[split.Name].Count <= 1 ||
           (lastSplit = (pres.Count == totals.Count)))
        {
          int nDouble = GetNDouble(split, dict, dictPre, lastSplit);
          if (Pars.Centered)
          { nDouble += Math.Abs(nMean - iSplit); }
          if (nDouble < minDouble
            || (nDouble == minDouble && Math.Abs(iSplit - iLastSplit / 2) < Math.Abs(minSplit - iLastSplit / 2)))
          {
            minDouble = nDouble;
            minSplit = iSplit;
            minNode = currentNode;
          }
        }
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


    private int GetNDouble(Control split, Dictionary<string, List<Control>> dict,
      Dictionary<string, List<Control>> dictPre, bool lastSplit)
    {
      int nTotal;
      if (Pars.Squared)
      {
        int nPre = GetNDouble(dictPre.Values.Select(x => x.Count));
        int nPost = GetNDouble(dict.Select(x =>
        {
          dictPre.TryGetValue(x.Key, out List<Control> pre);
          return x.Value.Count - (pre?.Count ?? 0);
        }));

        nTotal = nPre * nPre + nPost * nPost;
      }
      else
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

          if (d != 0)
          { nDouble += Math.Abs(d); }
        }
        nTotal = 2 * nDouble + (lastSplit ? 1 : 0);
      }
      nTotal = Pars.CustomWeight?.Invoke(nTotal, split, dict, dictPre) ?? nTotal;
      return nTotal;
    }

    private int GetNDouble(IEnumerable<int> frequencies)
    {
      int n = 0;
      foreach (int freq in frequencies)
      {
        if (freq < 2)
        { continue; }
        int w = freq - 1;
        n += w * w;
      }
      return n;
    }

  }
}
