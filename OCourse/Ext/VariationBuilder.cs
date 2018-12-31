using Ocad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OCourse.Ext
{
  public class VariationBuilder
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

    public IReadOnlyList<SimpleSection> GetSimpleSections()
    {
      PermutationBuilder builder = new PermutationBuilder(_course);
      return builder.SimpleSections;
    }

    public List<SectionList> AnalyzeLeg(int leg)
    {
      LegBuilder builder = new LegBuilder(_course, leg);
      builder.VariationAdded += Builder_VariationAdded;

      List<SectionList> permuts = builder.Analyze();

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

        if (reduced == null)
        { reduced = new SectionList(control); }
        else
        { reduced.Add(control); }
      }
      return reduced;
    }

    internal interface ILegController
    {
      bool IsPartStart(NextControl ctr);
      bool IsPartEnd(NextControl ctr);

      bool IsLegEnd(NextControl ctr);
      bool IsLegStart(NextControl ctr);
    }

    internal class WhereInfo
    {
      public enum State { Fulfilled, Unknown, Failed }

      public static WhereInfo Failed = new WhereInfo(State.Failed);
      public static WhereInfo FulFilled = new WhereInfo(State.Fulfilled);

      [ThreadStatic]
      private static DataView _whereView;
      [ThreadStatic]
      private static DataRow _exprRow;
      [ThreadStatic]
      private static DataColumn _exprColumn;

      private int _max;
      public int MaxControl { get => _max; set => _max = value; }

      private readonly NextControl _nextControl;
      private readonly State _state;
      private readonly string _parsed;
      private readonly ILegController _legController;

      public WhereInfo(ILegController legController, string where, ISectionList permut)
      {
        _legController = legController;
        _nextControl = permut.GetNextControl(permut.ControlsCount - 1);
        _state = GetState(where, permut, out string parsed);

        _parsed = parsed;
      }
      private WhereInfo(State state)
      {
        _state = state;
      }
      public override string ToString()
      {
        return $"{_parsed} : {_state}";
      }

      public State GetState(ISectionList sections)
      {
        if (_state == State.Failed)
        { return State.Failed; }
        if (_state == State.Fulfilled)
        { return State.Fulfilled; }

        if (sections.ControlsCount <= MaxControl)
        { return State.Unknown; }

        State state = GetState(_parsed, sections, out string parsed);

        return state;
      }

      private string Replace(string s, Match match, string replace)
      {
        return $"{s.Substring(0, match.Index)}{replace}{s.Substring(match.Index + match.Length)}";
      }

      public const string Leg = "Leg";
      public const string PartControls = "PartControls";
      public const string LegControls = "LegControls";

      private static readonly List<string> _keys = new List<string> { Leg, PartControls, LegControls };
      public static IReadOnlyList<string> Keywords => _keys;

      private State GetState(string where, ISectionList sections, out string parse)
      {
        parse = where;
        if (string.IsNullOrEmpty(where))
        {
          return State.Fulfilled;
        }

        if (_whereView == null)
        {
          DataTable whereTbl = new DataTable();
          whereTbl.Rows.Add();
          whereTbl.AcceptChanges();
          _whereView = new DataView(whereTbl);
        }

        RegexOptions opts = RegexOptions.IgnoreCase;

        {
          string sLeg = null;
          while (true)
          {
            Match match = Regex.Match(where, $@"\b({Leg})\b", opts);
            if (!match.Success) break;
            if (sLeg == null)
            {
              int leg = 0;
              Control start = sections.GetControl(0);
              foreach (var ctr in sections.Controls)
              {
                if (ctr == start)
                { leg++; }
              }
              sLeg = leg.ToString();
            }
            where = Replace(where, match, sLeg);
          }
        }
        {
          string partControls = null;
          while (true)
          {
            Match match = Regex.Match(where, $@"\b({PartControls})\b", opts);
            if (!match.Success) break;
            if (partControls == null && !GetPartControls(sections, _nextControl, out partControls)) break;

            where = Replace(where, match, partControls);
          }
        }
        {
          string legControls = null;
          while (true)
          {
            Match match = Regex.Match(where, $@"\b({LegControls})\b", opts);
            if (!match.Success) break;
            if (legControls == null && !GetLegControls(sections, _nextControl, out legControls)) break;

            where = Replace(where, match, legControls);
          }
        }

        StringBuilder parsed = new StringBuilder();
        for (int idx = where.IndexOf("["); idx >= 0; idx = where.IndexOf("["))
        {
          int end = where.IndexOf("]");
          parsed.Append(where.Substring(0, end + 1));
          string ctrName = where.Substring(idx + 1, end - idx - 1);

          where = where.Substring(end + 1);

          if (_exprRow == null)
          {
            DataTable exprTbl = new DataTable();
            _exprColumn = exprTbl.Columns.Add("Expr", typeof(int), "1");
            _exprRow = exprTbl.NewRow();
            exprTbl.Rows.Add(_exprRow);
            _exprRow.AcceptChanges();
          }

          string ctrExpr = ctrName.Replace("#", sections.ControlsCount.ToString());
          _exprColumn.Expression = ctrExpr;

          int ctrIdx = (int)_exprRow[_exprColumn];
          if (ctrIdx < 0)
          {
            parse = parsed.ToString();
            return State.Failed;
          }

          if (ctrIdx <= sections.ControlsCount)
          {
            Control ctr = sections.GetControl(ctrIdx);
            parsed.Replace(string.Format("[{0}]", ctrName), string.Format("'{0}'", ctr.Name));
          }
          else
          {
            parsed.Replace(string.Format("[{0}]", ctrName), string.Format("[{0}]", ctrIdx));
            MaxControl = Math.Max(MaxControl, ctrIdx);
          }
        }
        parsed.Append(where);

        parse = parsed.ToString();
        if (MaxControl > sections.ControlsCount)
        {
          return State.Unknown;
        }
        if (Regex.Match(where, $@"\b({PartControls})\b", opts).Success)
        {
          return State.Unknown;
        }
        if (Regex.Match(where, $@"\b({LegControls})\b", opts).Success)
        {
          return State.Unknown;
        }
        _whereView.RowFilter = parse;
        if (_whereView.Count == 1)
        {
          return State.Fulfilled;
        }
        else
        {
          return State.Failed;
        }
      }

      private bool GetPartControls(ISectionList sections, NextControl control, out string partControls)
      {
        List<Control> currentPart = null;
        bool isControlInCurrentPart = false;
        foreach (var ctr in sections.NextControls)
        {
          if (_legController.IsPartStart(ctr))
          {
            if (isControlInCurrentPart) { return GetControlsInList(currentPart, out partControls); }
            currentPart = null;
          }
          currentPart = currentPart ?? new List<Control>();
          currentPart.Add(ctr.Control);
          if (ctr == control)
          {
            isControlInCurrentPart = true;
          }

          if (_legController.IsPartEnd(ctr))
          {
            if (isControlInCurrentPart) { return GetControlsInList(currentPart, out partControls); }
            currentPart = null;
          }
        }
        partControls = null;
        return false;
      }
      private bool GetLegControls(ISectionList sections, NextControl control, out string legControls)
      {
        List<Control> currentLeg = null;
        bool isControlInCurrentPart = false;
        foreach (var ctr in sections.NextControls)
        {
          if (_legController.IsLegStart(ctr))
          {
            if (isControlInCurrentPart) { return GetControlsInList(currentLeg, out legControls); }
            currentLeg = null;
          }
          currentLeg = currentLeg ?? new List<Control>();
          currentLeg.Add(ctr.Control);
          if (ctr == control)
          {
            isControlInCurrentPart = true;
          }

          if (_legController.IsLegEnd(ctr))
          {
            if (isControlInCurrentPart) { return GetControlsInList(currentLeg, out legControls); }
            currentLeg = null;
          }
        }
        legControls = null;
        return false;
      }
      private bool GetControlsInList(List<Control> controls, out string partControls)
      {
        StringBuilder sb = new StringBuilder("(");
        foreach (var control in controls)
        { sb.Append($"'{control.Name}',"); }
        sb.Remove(sb.Length - 1, 1);
        sb.Append(")");
        partControls = sb.ToString();
        return true;
      }
    }

    private class PermutationBuilder : Builder
    {
      private sealed class LegSections : ISectionList, ICloneable<LegSections>
      {
        private readonly PermutationBuilder _builder;
        private readonly List<SectionList> _legs;

        private int _currentLeg;

        public IReadOnlyList<int> EvalOrder { get; }
        public LegSections(PermutationBuilder builder, IReadOnlyList<int> evalOrder)
        {
          _builder = builder;
          EvalOrder = evalOrder;
          _legs = new List<SectionList>(evalOrder.Select(x => new SectionList(new NextControl(_builder.StartControl))));
        }

        public override string ToString()
        {
          StringBuilder sb = new StringBuilder(string.Concat(EvalOrder.Select(x => $"{x},")));
          sb.Remove(sb.Length - 1, 1);
          sb.Append($":{GetSectionList()}");
          return sb.ToString();
        }

        public void InitLeg(int iLeg)
        {
          _currentLeg = iLeg;
        }

        IEnumerable<Control> ISectionList.Controls => NextControls.Select(x => x.Control);
        IEnumerable<NextControl> ISectionList.NextControls => NextControls;
        private IEnumerable<NextControl> NextControls
        {
          get
          {
            foreach (var leg in CurrentLegs)
            {
              foreach (var control in leg.NextControls)
              {
                yield return control;
              }
            }
          }
        }

        int ISectionList.ControlsCount => ControlsCount;
        private int ControlsCount
        {
          get
          {
            int count = CurrentLegs.Sum(x => x.ControlsCount);
            return count;
          }
        }

        private IEnumerable<SectionList> CurrentLegs
        {
          get
          {
            for (int iLeg = 0; iLeg < _currentLeg; iLeg++)
            { yield return _legs[iLeg]; }
          }
        }

        private SectionList CurrentLeg => _legs[_currentLeg - 1];
        NextControl ISectionList.Add(NextControl current)
        {
          return CurrentLeg.Add(current);
        }

        WhereInfo ISectionList.Add(WhereInfo where)
        {
          if (_currentLeg == 0)
          { throw new NotImplementedException(); }
          return CurrentLeg.Add(where);
        }

        public LegSections Clone()
        {
          LegSections clone = new LegSections(_builder, EvalOrder);
          clone._legs.Clear();
          foreach (var leg in _legs)
          { clone._legs.Add(leg.Clone()); }

          clone._currentLeg = _currentLeg;

          return clone;
        }

        int ISectionList.CountExisting(Control from, Control to)
        {
          int count = _legs.Sum(x => x.CountExisting(from, to));
          return count;
        }

        Control ISectionList.GetControl(int index)
        {
          return GetNextControl(index)?.Control;
        }

        NextControl ISectionList.GetNextControl(int index)
        { return GetNextControl(index); }
        private NextControl GetNextControl(int index)
        {
          int i = index;
          if (i < 0)
          { i = ControlsCount + i; }

          int iLeg = 0;
          while (_legs[iLeg].ControlsCount <= i)
          {
            i -= _legs[iLeg].ControlsCount;
            iLeg++;
          }
          return _legs[iLeg].GetNextControl(i);
        }

        public SectionList GetSectionList()
        {
          SectionList sections = null;
          foreach (var leg in _legs)
          {
            if (sections == null)
            { sections = leg.Clone(); }
            else
            {
              foreach (var control in leg.NextControls)
              { sections.Add(control); }
            }
          }
          return sections;
        }

        int ISectionList.GetSplitIndex(Control split)
        {
          int iSplit = 0;
          for (int iLeg = 0; iLeg < _currentLeg; iLeg++)
          {
            string pre = null;
            SectionList leg = _legs[iLeg];
            if (leg.ControlsCount < 2 && iLeg < _currentLeg - 1)
            {
              leg = _builder._firstLegs._legs[iLeg];
            }

            foreach (var control in leg.Controls)
            {
              if (pre == split.Name)
              { iSplit++; }
              pre = control.Name;
            }
          }

          return iSplit;
        }


        WhereInfo.State ISectionList.GetState()
        {
          WhereInfo.State allState = WhereInfo.State.Fulfilled;
          foreach (var leg in _legs)
          {
            WhereInfo.State state = leg.GetState();
            if (state == WhereInfo.State.Failed)
            { return state; }

            if (state == WhereInfo.State.Unknown)
            { allState = state; }
          }
          return allState;
        }
      }
      private class SplitInfo : IComparable<SplitInfo>
      {
        public SectionList Sections;
        public int Position;
        public int Split;
        public SplitInfo(SectionList sections, int position, int split)
        {
          Sections = sections;
          Position = position;
          Split = split;
        }
        public override string ToString()
        {
          StringBuilder sb = new StringBuilder();

          sb.Append(Sections.GetControl(0).Name);
          for (int i = 1; i <= Position; i++)
          {
            sb.AppendFormat(" {0}", Sections.GetNextControl(i).Control.Name);
          }
          return sb.ToString();
        }

        private int GetPreviousSplit(int start)
        {
          int i = start;
          while (i > 0)
          {
            NextControl next = Sections.GetNextControl(i);
            if (next.Code != 0)
            {
              return i;
            }
            i--;
          }
          return i;
        }

        public int CompareTo(SplitInfo other)
        {
          int posX = Position;
          int posY = other.Position;

          while (posX >= 0 && posY >= 0)
          {
            NextControl x = Sections.GetNextControl(posX + 1);
            NextControl y = other.Sections.GetNextControl(posY + 1);

            int d = x.Control.Name.CompareTo(y.Control.Name);
            if (d != 0)
            {
              return d;
            }
            posX = GetPreviousSplit(posX);
            posY = other.GetPreviousSplit(posY);
          }

          int i = posX.CompareTo(posY);
          return i;
        }

        public NextControl GetNextControl()
        {
          return Sections.GetNextControl(Position);
        }
      }
      private class Index
      {
        private class CommonInfo
        {
          public readonly ISectionList Variation;
          public readonly NextWhere Next;
          public readonly int SplitLength;
          public readonly int SplitCount;
          public readonly int VarLength;
          public readonly int VarCount;

          public CommonInfo(ISectionList variation, NextWhere next,
            int splitLength, int splitCount, int varLength, int varCount)
          {
            Variation = variation;
            Next = next;
            SplitLength = splitLength;
            SplitCount = splitCount;
            VarLength = varLength;
            VarCount = varCount;
          }

          public override string ToString()
          {
            string txt = string.Format("({0},{1}) ({2},{3}) {4}; {5}",
              SplitLength, SplitCount, VarLength, VarCount, Next, Variation);
            return txt;
          }
        }
        private class CommonCpr : IComparer<CommonInfo>
        {
          private readonly ISectionList _pre;
          private readonly Control _split;
          private readonly Dictionary<Control, List<List<SplitInfo>>> _controlDict;

          private Dictionary<char, int> _varCount;

          public CommonCpr(ISectionList pre, Control split, Dictionary<Control, List<List<SplitInfo>>> controlDict)
          {
            _pre = pre;
            _split = split;
            _controlDict = controlDict;
          }

          private int GetCount(char code)
          {
            if (!VarCountDict.TryGetValue(code, out int count))
            { count = 0; }
            return count;
          }

          private Dictionary<char, int> VarCountDict
          {
            get
            {
              if (_varCount == null)
              {
                Control split = _split;
                int pos = 0;
                Control pre = null;
                foreach (var ctr in _pre.Controls)
                {
                  if (pre == split)
                  { pos++; }
                  pre = ctr;
                }
                List<List<SplitInfo>> combs = _controlDict[split];
                Dictionary<char, int> varCount = new Dictionary<char, int>();
                foreach (var comb in combs)
                {
                  char code = comb[pos].GetNextControl().Code;
                  if (!varCount.ContainsKey(code))
                  { varCount.Add(code, 1); }
                  else
                  { varCount[code]++; }
                }

                _varCount = varCount;
              }
              return _varCount;
            }
          }

          public int Compare(CommonInfo x, CommonInfo y)
          {
            int d;

            d = x.SplitLength.CompareTo(y.SplitLength);
            if (d != 0)
            { return d; }

            d = x.SplitCount.CompareTo(y.SplitCount);
            if (d != 0)
            { return d; }

            int countX = GetCount(x.Next.Next.Code);
            int countY = GetCount(y.Next.Next.Code);
            d = countX.CompareTo(countY);
            if (d != 0)
            { return d; }

            d = x.VarLength.CompareTo(y.VarLength);
            if (d != 0)
            { return d; }

            d = x.VarCount.CompareTo(y.VarCount);
            if (d != 0)
            { return d; }

            //Dictionary<Control, List<SplitInfo>> xDict = Index.GetControlDict(x.Variation);
            d = x.Next.Next.Code.CompareTo(y.Next.Next.Code);
            return d;
          }
        }

        private readonly PermutationBuilder _builder;
        Dictionary<NextControl, List<SplitInfo>> _dict =
          new Dictionary<NextControl, List<SplitInfo>>(new NextControl.EqualCodeComparer());

        private Dictionary<Control, List<List<SplitInfo>>> _controlDict =
          new Dictionary<Control, List<List<SplitInfo>>>();

        public Index(PermutationBuilder builder)
        {
          _builder = builder;
        }

        private static Dictionary<Control, List<SplitInfo>> GetControlDict(SectionList sections)
        {
          int i = 0;
          Control pre = sections.GetControl(0);
          Dictionary<Control, List<SplitInfo>> controlDict = new Dictionary<Control, List<SplitInfo>>();
          Dictionary<string, int> splitCount = new Dictionary<string, int>();
          foreach (var v in sections.NextControls)
          {
            if (v.Code != 0)
            {
              if (!splitCount.ContainsKey(pre.Name))
              { splitCount.Add(pre.Name, 0); }
              else
              { splitCount[pre.Name]++; }

              SplitInfo split = new SplitInfo(sections, i, splitCount[pre.Name]);

              if (!controlDict.TryGetValue(pre, out List<SplitInfo> splits))
              {
                splits = new List<SplitInfo>();
                controlDict.Add(pre, splits);
              }
              splits.Add(split);
            }
            pre = v.Control;
            i++;
          }

          return controlDict;
        }

        public void Add(SectionList sections)
        {
          Dictionary<Control, List<SplitInfo>> controlDict = GetControlDict(sections);
          foreach (var splits in controlDict.Values)
          {
            foreach (var split in splits)
            {
              NextControl v = split.GetNextControl();
              if (!_dict.TryGetValue(v, out List<SplitInfo> existing))
              {
                existing = new List<SplitInfo>();
                _dict.Add(v, existing);
              }
              existing.Add(split);
            }
          }

          foreach (var pair in controlDict)
          {
            Control c = pair.Key;
            if (!_controlDict.TryGetValue(c, out List<List<SplitInfo>> controlSplits))
            {
              controlSplits = new List<List<SplitInfo>>();
              _controlDict.Add(c, controlSplits);
            }
            controlSplits.Add(pair.Value);
          }
        }

        public void Sort(ISectionList variation, Control split, int iSplit, List<NextWhere> nexts)
        {
          if (_dict.Count == 0)
          { return; }
          if (nexts.Count < 2)
          { return; }

          List<CommonInfo> commons = GetCommons(variation, split, iSplit, nexts);
          commons.Sort(new CommonCpr(variation, split, _controlDict));
          nexts.Clear();
          nexts.AddRange(commons.Select(x => x.Next));
        }

        private List<CommonInfo> GetCommons(ISectionList variation, Control from, int iSplit,
          IEnumerable<NextWhere> nexts)
        {
          List<CommonInfo> commons = new List<CommonInfo>();
          foreach (var next in nexts)
          {
            NextControlList nextControls = _builder.NextControls[from];
            List<SplitInfo> existing = GetExisting(next.Next);

            int maxCommonVar = 0;
            int maxCountVar = 0;
            int maxCommonSplit = 0;
            int maxCountSplit = 0;
            foreach (var split in existing)
            {
              SectionList secExist = split.Sections;
              int posExist = split.Position - 3;
              int posVar = variation.ControlsCount - 3;

              int common = 0;

              while (posVar >= 0 && posExist >= 0)
              {
                NextControl nextVar = variation.GetNextControl(posVar + 1);
                NextControl nextExits = secExist.GetNextControl(posExist + 1);

                if (nextVar.Control != nextExits.Control)
                { break; }

                if (nextVar.Code != 0)
                { common++; }

                posVar--;
                posExist--;
              }

              if (split.Split == iSplit)
              {
                if (maxCommonSplit < common)
                {
                  maxCommonSplit = common;
                  maxCountSplit = 1;
                }
                else if (maxCommonSplit == common)
                {
                  maxCountSplit++;
                }
              }
              if (maxCommonVar < common)
              {
                maxCommonVar = common;
                maxCountVar = 1;
              }
              else if (maxCommonVar == common)
              {
                maxCountVar++;
              }
            }
            commons.Add(new CommonInfo(variation, next, maxCommonSplit, maxCountSplit,
              maxCommonVar, maxCountVar));
          }
          return commons;
        }

        private List<SplitInfo> GetExisting(NextControl next)
        {
          if (_dict.TryGetValue(next, out List<SplitInfo> existing))
          { return existing; }

          // Fuer mehrfache identische Teilstrecken in unterschiedlichen Splitsections
          NextControl nextA = new NextControl(next.Control) { Code = 'A' };
          return _dict[nextA];
        }
      }
      private class PermutInfo
      {
        private readonly SectionList _sections;
        private readonly PermutationBuilder _parent;
        public PermutInfo(PermutationBuilder parent)
        {
          _parent = parent;
          _sections = new SectionList(new NextControl(parent.StartControl));
        }

        public SectionList Sections { get { return _sections; } }
        public int ControlsCount { get { return _sections.ControlsCount; } }

        public Control GetLastControl()
        {
          return _sections.GetControl(_sections.ControlsCount - 1);
        }

        public void Add(NextControl next)
        {
          _sections.Add(next);
        }

        Dictionary<Control, Dictionary<char, List<NextWhere>>> _remainingVariations;

        public NextControl AddBestNext(Control last, IList<NextControl> nexts)
        {
          _parent.EvaluateNexts(_sections, last, nexts, false,
            out Dictionary<char, List<NextWhere>> variations, out Dictionary<char, InvalidType> invalids);

          if (_parent._stats == null)
          { _parent._stats = new Dictionary<Control, Stat>(); }

          if (!_parent._stats.TryGetValue(last, out Stat stat))
          {
            stat = new Stat(variations);
            _parent._stats.Add(last, stat);
          }

          if (_remainingVariations == null)
          { _remainingVariations = new Dictionary<Control, Dictionary<char, List<NextWhere>>>(); }
          if (!_remainingVariations.TryGetValue(last, out Dictionary<char, List<NextWhere>> remaining))
          {
            remaining = new Dictionary<char, List<NextWhere>>();
            foreach (var pair in variations)
            { remaining.Add(pair.Key, new List<NextWhere>(pair.Value)); }
          }

          char best = stat.AddBest(remaining, invalids);
          NextWhere nextWhere = remaining[best][0];
          remaining[best].RemoveAt(0);
          NextControl next = nextWhere.Next;
          Add(next);
          return next;
        }
      }
      private class Stat
      {
        private readonly Dictionary<char, List<NextWhere>> _fullVariations;
        private readonly Dictionary<char, int> _fullCount;
        private readonly Dictionary<char, double> _pseudoCount;
        public Stat(Dictionary<char, List<NextWhere>> fullVariations)
        {
          _fullVariations = fullVariations;

          _fullCount = new Dictionary<char, int>();
          foreach (var key in fullVariations.Keys)
          { _fullCount.Add(key, 0); }

          _pseudoCount = new Dictionary<char, double>();

          ResetPseudo();
        }

        public void ResetPseudo()
        {
          _pseudoCount.Clear();
          foreach (var pair in _fullCount)
          { _pseudoCount.Add(pair.Key, pair.Value); }
        }

        public char AddBest(Dictionary<char, List<NextWhere>> remaining, Dictionary<char, InvalidType> invalids)
        {
          foreach (var pair in _fullVariations)
          {
            if (!invalids.TryGetValue(pair.Key, out InvalidType invalid))
            {
              return pair.Key;
            }
          }
          throw new InvalidOperationException("No valid variation found");
        }
      }
      private class Statistics
      {
        private List<SectionList> _current;
        private object _currentStat;
        public void AddNext(SectionList permutation, NextWhere nextWhere, List<SectionList> permuts)
        {
          Init(permuts);
          permutation.Add(nextWhere.Next);
        }

        public List<NextWhere> Sort(List<SectionList> permuts, Control last, IEnumerable<NextWhere> values)
        {
          Init(permuts);

          List<NextWhere> nexts = new List<NextWhere>(values);
          if (nexts.Count <= 1)
          { return nexts; }

          return nexts;
        }

        private void Init(List<SectionList> permuts)
        {
          if (_current != permuts)
          {
            _currentStat = new object();
            _current = permuts;
          }
        }
      }

      private SectionCollection _course;
      private Dictionary<Control, Stat> _stats;
      private Index _index;
      private LegSections _firstLegs;

      protected override Control StartControl { get { return (Control)_course.First.Value; } }
      protected override Control EndControl { get { return (Control)_course.Last.Value; } }

      public int Estimate()
      {
        if (_course.Count == 0)
        { return 0; }

        SectionList permut = new SectionList(new NextControl(StartControl));
        SectionList first = EnumVariations(permut, allLegs: true).FirstOrDefault();

        if (first == null)
        { return 0; }

        int nEstimate = 1;
        SectionList estimate = new SectionList(new NextControl(first.GetControl(0)));
        int iCtr = 0;
        while (iCtr < first.ControlsCount)
        {
          Control start = estimate.GetControl(iCtr);
          if (NextControls.TryGetValue(start, out NextControlList nexts))
          {
            Dictionary<char, NextWhere> variations = GetPossibleNexts(estimate,
              start, nexts.List, false);

            List<NextWhere> sorted = Sort(estimate, start, variations.Values);
            int vars = 0;
            foreach (var next in sorted)
            {
              SectionList clone = estimate.Clone();
              if (EnumVariations(clone, next.Next, next.Where, allLegs: true).FirstOrDefault() != null)
              { vars++; }
            }

            nEstimate *= vars;
          }

          if (iCtr < first.ControlsCount - 1)
          { estimate.Add(first.GetNextControl(iCtr + 1)); }
          iCtr++;
        }

        return nEstimate;
      }

      public List<SectionList> BuildPermutationsSimple(int nPermutations)
      {
        List<PermutInfo> permutations = new List<PermutInfo>(nPermutations);
        for (int i = 0; i < nPermutations; i++)
        {
          PermutInfo permutation = new PermutInfo(this);
          permutations.Add(permutation);
        }

        Statistics stat = new Statistics();
        while (true)
        {
          Dictionary<Control, List<PermutInfo>> equalVars = new Dictionary<Control, List<PermutInfo>>();
          bool completed = true;
          foreach (var permutation in permutations)
          {
            if (permutation.ControlsCount == _course.Count)
            { continue; }

            completed = false;

            Control last = permutation.GetLastControl();
            NextControlList nexts = NextControls[last];
            while (nexts != null && nexts.Count == 1)
            {
              NextControl next = nexts.List[0];
              permutation.Add(next);
              last = next.Control;
              nexts = NextControls[last];
            }

            if (nexts == null)
            { continue; }

            if (!equalVars.TryGetValue(last, out List<PermutInfo> equalVariation))
            {
              equalVariation = new List<PermutInfo>();
              equalVars.Add(last, equalVariation);
            }
            equalVariation.Add(permutation);
          }

          if (completed)
          { break; }

          foreach (var pair in equalVars)
          {
            Control last = pair.Key;
            List<PermutInfo> permuts = pair.Value;
            NextControlList nexts = NextControls[last];

            foreach (var permutation in permuts)
            {
              permutation.AddBestNext(last, nexts.List);
            }
          }
        }
        List<SectionList> fullSections = new List<SectionList>(permutations.Count);
        foreach (var permutation in permutations)
        { fullSections.Add(permutation.Sections); }
        return fullSections;
      }

      protected override List<SimpleSection> GetSections()
      {
        List<SimpleSection> sections = _course.GetAllSections();
        sections = MakeUniqueControls(sections);

        return sections;
      }

      protected override List<NextWhere> Sort(ISectionList variation,
        Control split, IEnumerable<NextWhere> nextEnum)
      {
        List<NextWhere> nexts = base.Sort(variation, split, nextEnum);
        if (nexts.Count < 2)
        { return nexts; }
        if (_index == null)
        { return nexts; }

        int iSplit = variation.GetSplitIndex(split);
        _index.Sort(variation, split, iSplit, nexts);
        return nexts;
      }

      public List<SectionList> BuildPermutations(int nPermutations)
      {
        _index = new Index(this);
        _firstLegs = null;
        int nLegs = _course.LegCount();
        bool orderedLegs = true;
        List<SectionList> variations = new List<SectionList>(nPermutations);
        for (int iPermutation = 0; iPermutation < nPermutations; iPermutation++)
        {
          SectionList variation = orderedLegs
            ? GetVariation(iPermutation, nLegs)
            : GetVariation();

          variations.Add(variation);
          OnVariationAdded(variation);
          _index.Add(variation);
        }

        return variations;
      }

      private SectionList GetVariation(int iPermutation, int nLegs)
      {
        List<int> evalOrder = GetEvalOrder(nLegs, iPermutation);
        evalOrder.Reverse();
        LegSections permut = new LegSections(this, evalOrder);

        IEnumerable<LegSections> legVariations = new List<LegSections> { permut };
        foreach (var iLeg in evalOrder)
        {
          legVariations = EnumLegVariations(legVariations, iLeg);
        }

        foreach (var candidate in legVariations)
        {
          SectionList variation = candidate.GetSectionList();

          string countExpr = GetCountExpression(_course);
          WhereInfo countWhere = new WhereInfo(this, countExpr, permut);
          variation.Add(countWhere);

          if (variation.GetState() == WhereInfo.State.Fulfilled)
          {
            _firstLegs = _firstLegs ?? candidate;
            return variation;
          }
        }
        return null;
      }

      private IEnumerable<LegSections> EnumLegVariations(IEnumerable<LegSections> preLeg, int iLeg)
      {
        foreach (var valid in preLeg)
        {
          valid.InitLeg(iLeg);
          foreach (var nextLeg in EnumVariations(valid, allLegs: false))
          {
            yield return nextLeg;
          }
        }
      }

      private static List<int> GetEvalOrder(int splitSize, int iPermut)
      {
        int x = iPermut / splitSize;
        int y = iPermut % splitSize;

        if (splitSize > 1)
        {
          List<int> inner = GetEvalOrder(splitSize - 1, x);
          for (int i = 0; i < inner.Count; i++)
          {
            inner[i] = (inner[i] + y) % splitSize + 1;
          }
          inner.Add(y + 1);
          return inner;
        }
        else
        {
          return new List<int> { 1 };
        }
      }


      private SectionList GetVariation()
      {
        SectionList permut = new SectionList(new NextControl(StartControl));
        string countExpr = GetCountExpression(_course);
        WhereInfo countWhere = new WhereInfo(this, countExpr, permut);
        permut.Add(countWhere);


        SectionList variation = EnumVariations(permut, allLegs: true).FirstOrDefault();
        return variation;
      }

      public PermutationBuilder(SectionCollection course)
      {
        _course = course;
      }
    }

    private class LegBuilder : Builder
    {
      private SectionCollection _course;
      private readonly int _leg;

      protected override Control StartControl { get { return (Control)_course.First.Value; } }
      protected override Control EndControl { get { return (Control)_course.Last.Value; } }

      public LegBuilder(SectionCollection course, int leg)
      {
        _course = course;
        _leg = leg;
      }

      public List<SectionList> Analyze()
      {
        SectionList permut = new SectionList(new NextControl(StartControl));
        for (int i = 1; i < _leg; i++)
        {
          permut.Add(new NextControl(StartControl));
        }

        List<SectionList> validList = new List<SectionList>();
        foreach (var valid in EnumVariations(permut, allLegs: false))
        {
          validList.Add(valid);
          OnVariationAdded(valid);
        }

        return validList;
      }

      protected override List<SimpleSection> GetSections()
      {
        List<SimpleSection> sections = GetSections(_course, _leg);
        return sections;
      }
      private static List<SimpleSection> GetSections(SectionCollection course, int leg)
      {
        SectionsBuilder b = new SectionsBuilder();
        List<SimpleSection> simples = b.GetSections(course, leg);

        List<SimpleSection> parts = b.GetParts(course, leg);
        //simples = parts;


        List<SimpleSection> sections = new List<SimpleSection>(simples.Count);
        Dictionary<string, Control> controls = new Dictionary<string, Control>();
        foreach (var section in simples)
        {
          Control from = GetUniqueControl(controls, section.From);
          Control to = GetUniqueControl(controls, section.To);

          MultiSection add = new MultiSection(from, to);
          if (section is MultiSection s)
          {
            foreach (var c in s.Inter)
            {
              Control u = GetUniqueControl(controls, c);
              add.Inter.Add(u);
            }
          }
          add.SetWhere(section.Where);
          sections.Add(add);
        }
        return sections;
      }
    }

    private class CourseBuilder : Builder
    {
      private SectionCollection _course;

      protected override Control StartControl { get { return (Control)_course.First.Value; } }
      protected override Control EndControl { get { return (Control)_course.Last.Value; } }

      public CourseBuilder(SectionCollection course)
      {
        _course = course;
      }

      public List<SectionList> Analyze()
      {
        if (_course.Count == 0)
        { return new List<SectionList>(); }

        SectionList permut = new SectionList(new NextControl(StartControl));
        string countExpr = GetCountExpression(_course);
        WhereInfo countWhere = new WhereInfo(this, countExpr, permut);
        permut.Add(countWhere);
        List<SectionList> validList = new List<SectionList>();

        foreach (var valid in EnumVariations(permut, allLegs: true))
        {
          validList.Add(valid);
          OnVariationAdded(valid);
        }

        return validList;

      }

      protected override List<SimpleSection> GetSections()
      {
        List<SimpleSection> sections = GetSections(_course);
        return sections;
      }
      private static List<SimpleSection> GetSections(SectionCollection course)
      {
        List<SimpleSection> simples = course.GetAllSections();

        List<SimpleSection> sections = new List<SimpleSection>(simples.Count);
        Dictionary<string, Control> controls = new Dictionary<string, Control>();
        foreach (var section in simples)
        {
          Control from = GetUniqueControl(controls, section.From);
          Control to = GetUniqueControl(controls, section.To);

          SimpleSection add = new SimpleSection(from, to);
          add.SetWhere(section.Where);
          sections.Add(add);
        }
        return sections;
      }
    }

    private abstract class Builder : ILegController
    {
      public event VariationEventHandler VariationAdded;

      private List<SimpleSection> _sections;
      private Dictionary<Control, NextControlList> _nextControls;
      private Dictionary<SimpleSection, int> _sectionCount;

      protected abstract Control StartControl { get; }
      protected abstract Control EndControl { get; }
      protected abstract List<SimpleSection> GetSections();

      public IReadOnlyList<SimpleSection> SimpleSections { get { return Sections; } }
      protected IReadOnlyList<SimpleSection> Sections
      {
        get
        {
          if (_sections == null)
          {
            _sections = GetSections();
          }
          return _sections;
        }
      }
      protected Dictionary<Control, NextControlList> NextControls
      {
        get
        {
          if (_nextControls == null)
          {
            IReadOnlyList<SimpleSection> sections = Sections;
            Dictionary<Control, NextControlList> nextInfos = AssembleSections(sections);
            foreach (var nextInfo in nextInfos.Values)
            { nextInfo.AssureCodes(); }

            _nextControls = nextInfos;
          }
          return _nextControls;
        }
      }
      private Dictionary<SimpleSection, int> SectionCount
      {
        get
        {
          if (_sectionCount == null)
          {
            Dictionary<SimpleSection, int> dict = new Dictionary<SimpleSection, int>(new SimpleSection.EqualControlsComparer());
            foreach (var section in Sections)
            {
              if (!dict.ContainsKey(section))
              {
                dict.Add(section, 1);
              }
              else
              {
                dict[section]++;
              }
            }
            _sectionCount = dict;
          }
          return _sectionCount;
        }
      }

      bool ILegController.IsPartStart(NextControl ctr) { return ctr.Control.Code == ControlCode.Start; }
      bool ILegController.IsPartEnd(NextControl ctr) { return ctr.Control.Code == ControlCode.Finish || ctr.Control.Code == ControlCode.MapChange; }

      bool ILegController.IsLegStart(NextControl ctr) { return ctr.Control == StartControl; }
      bool ILegController.IsLegEnd(NextControl ctr) { return ctr.Control == EndControl; }

      protected string GetCountExpression(SectionCollection course)
      {
        int nSections = course.LegCount() + Sections.Count - 1;
        string expr = string.Format("[{0}] = '{1}'", nSections, EndControl.Name);
        return expr;
      }

      protected enum InvalidType { WhereFailed, NoVariation }

      protected Dictionary<char, NextWhere> GetPossibleNexts<T>(T pre,
        Control from, IList<NextControl> nexts, bool verifyFull)
        where T : ISectionList, ICloneable<T>
      {
        EvaluateNexts(pre, from, nexts, verifyFull,
          out Dictionary<char, List<NextWhere>> variations, out Dictionary<char, InvalidType> invalids);

        foreach (var code in invalids.Keys)
        {
          variations.Remove(code);
        }
        Dictionary<char, NextWhere> possibles = new Dictionary<char, NextWhere>();
        foreach (var pair in variations)
        {
          char code = pair.Key;
          List<NextWhere> codeNexts = pair.Value;
          possibles.Add(code, codeNexts[0]);
        }
        return possibles;
      }

      protected void EvaluateNexts<T>(T pre,
        Control from, IList<NextControl> nexts, bool verifyFull,
        out Dictionary<char, List<NextWhere>> variations,
        out Dictionary<char, InvalidType> invalids)
        where T : ISectionList, ICloneable<T>
      {
        variations = new Dictionary<char, List<NextWhere>>();
        invalids = new Dictionary<char, InvalidType>();
        foreach (var next in nexts)
        {
          if (variations.TryGetValue(next.Code, out List<NextWhere> nextWheres))
          {
            nextWheres.Add(new NextWhere(next, null));
            continue;
          }

          nextWheres = new List<NextWhere>();
          WhereInfo where = GetWhere(pre, from, next);
          WhereInfo.State state = where.GetState(pre);
          if (state == WhereInfo.State.Failed)
          {
            nextWheres.Add(new NextWhere(next, where));
            variations.Add(next.Code, nextWheres);
            invalids.Add(next.Code, InvalidType.WhereFailed);
            continue;
          }
          else if (state == WhereInfo.State.Fulfilled)
          { where = null; }

          nextWheres.Add(new NextWhere(next, where));
          if (verifyFull)
          {
            if (!ExistsVariation(pre, next, where))
            {
              variations.Add(next.Code, nextWheres);
              invalids.Add(next.Code, InvalidType.NoVariation);
              continue;
            }
          }
          variations.Add(next.Code, nextWheres);
          if (next.Code == 0)
          {
            break;
          }
        }
      }

      private bool ExistsVariation<T>(T variation, NextControl next, WhereInfo where)
        where T : ISectionList, ICloneable<T>
      {
        T candidate = variation.Clone();
        bool exists = false;
        foreach (var valid in EnumVariations(candidate, next, where, allLegs: true))
        {
          exists = true;
          break;
        }
        return exists;
      }

      protected IEnumerable<T> EnumVariations<T>(T variation, bool allLegs)
        where T : ISectionList, ICloneable<T>
      {
        Control start = variation.GetControl(variation.ControlsCount - 1);
        Dictionary<char, NextWhere> variations = GetPossibleNexts(variation,
          start, NextControls[start].List, verifyFull: false);

        List<NextWhere> sorted = Sort(variation, start, variations.Values);
        foreach (var next in sorted)
        {
          T init = variations.Count > 1
            ? variation.Clone()
            : variation;

          foreach (var valid in EnumVariations(init, next.Next, next.Where, allLegs))
          {
            yield return valid;
          }
        }
      }

      protected IEnumerable<T> EnumVariations<T>(T variation,
        NextControl current, WhereInfo currentWhere, bool allLegs)
        where T : ISectionList, ICloneable<T>
      {
        do
        {
          variation.Add(current);
          if (currentWhere == null && current.Where != null)
          { currentWhere = new WhereInfo(this, current.Where, variation); }
          variation.Add(currentWhere);
          WhereInfo.State state = variation.GetState();
          if (state == WhereInfo.State.Failed)
          { yield break; }

          if (NextControls.TryGetValue(current.Control, out NextControlList nextList) == false) // last Control
          {
            if (allLegs == false)
            {
              if (state == WhereInfo.State.Fulfilled && current.Control == EndControl)
              {
                yield return variation;
              }
              yield break;
            }
            current = null;
            nextList = NextControls[StartControl];
          }

          Control split = current != null ? current.Control : StartControl;
          T nextVariation = variation;
          if (current == null)
          {
            nextVariation = nextVariation.Clone();
            nextVariation.Add(new NextControl(StartControl));
          }
          Dictionary<char, NextWhere> nextsDict = GetPossibleNexts(nextVariation, split, nextList.List, false);

          bool finished = true;
          List<NextWhere> nexts = Sort(variation, split, nextsDict.Values);
          foreach (var next in nexts)
          {
            if (next == null) // only invalid nexts
            {
              if (current == null)
              { break; }
              yield break;
            }

            finished = false;

            if (current == null)
            {
              current = new NextControl(StartControl);
              variation.Add(current);
            }

            if (nexts.Count > 1)
            {
              T clone = variation.Clone();
              foreach (var validVariation in EnumVariations(clone, next.Next, next.Where, allLegs))
              {
                yield return validVariation;
              }
            }
            else
            {
              current = next.Next;
              currentWhere = next.Where;
            }
          }
          if (nexts.Count > 1)
          {
            yield break;
          }
          if (finished)
          {
            WhereInfo.State finishedState = variation.GetState();
            if (finishedState == WhereInfo.State.Fulfilled)
            {
              yield return variation;
            }
            yield break;
          }
        } while (true);
      }

      protected void OnVariationAdded(SectionList variation)
      {
        VariationAdded?.Invoke(this, variation);
      }

      protected virtual List<NextWhere> Sort(ISectionList variation, Control split, IEnumerable<NextWhere> nextEnum)
      {
        List<NextWhere> nexts = new List<NextWhere>(nextEnum);
        return nexts;
      }

      private WhereInfo GetWhere(ISectionList sections, Control from, NextControl next)
      {
        int nSections = sections.ControlsCount - 1;
        if (nSections == 0)
        {
          return WhereInfo.FulFilled;
        }
        WhereInfo where = WhereInfo.FulFilled;
        if (next.Where != null)
        {
          where = new WhereInfo(this, next.Where, sections);
          WhereInfo.State state = where.GetState(sections);
          if (state == WhereInfo.State.Failed)
          { return where; }
        }
        if (from == null) from = sections.GetControl(0);
        SimpleSection newSection = new SimpleSection(from, next.Control);
        if (sections.CountExisting(from, next.Control) >= SectionCount[newSection])
        {
          return WhereInfo.Failed;
        }
        return where;
      }

      private static Dictionary<Control, NextControlList> AssembleSections(IReadOnlyList<SimpleSection> sections)
      {
        Dictionary<Control, NextControlList> dict = new Dictionary<Control, NextControlList>();
        foreach (var section in sections)
        {
          Control pre = section.From;

          if (dict.TryGetValue(pre, out NextControlList nextList) == false)
          {
            nextList = new NextControlList();
            dict.Add(pre, nextList);
          }
          nextList.Add(section);
        }
        return dict;
      }

      protected static List<SimpleSection> MakeUniqueControls(IList<SimpleSection> sections)
      {
        Dictionary<string, Control> controls = new Dictionary<string, Control>();
        List<SimpleSection> unique = new List<SimpleSection>(sections.Count);
        foreach (var section in sections)
        {
          Control from = GetUniqueControl(controls, section.From);
          Control to = GetUniqueControl(controls, section.To);
          SimpleSection add = new SimpleSection(from, to);
          add.SetWhere(section.Where);
          unique.Add(add);
        }
        return unique;
      }

      protected static Control GetUniqueControl(Dictionary<string, Control> controls, Control control)
      {
        if (control.Code == ControlCode.TextBlock || control.Code == ControlCode.MapChange)
        { return control; }

        if (controls.TryGetValue(control.Name, out Control unique) == false)
        {
          unique = control;
          controls.Add(unique.Name, unique);
        }
        return unique;
      }

      protected class NextWhere
      {
        public NextControl Next { get; private set; }
        public WhereInfo Where { get; private set; }

        public NextWhere(NextControl next, WhereInfo where)
        {
          Next = next;
          Where = where;
        }

        public override string ToString()
        {
          return string.Format("{0} {1} {2}", Next.Control, Next.Code, Next.Where);
        }
      }
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
