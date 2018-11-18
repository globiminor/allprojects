using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Ocad;
using Ocad.StringParams;

namespace OCourse.Ext
{
  public class VariationBuilder
  {
    public delegate void VariationEventHandler(object sender, SectionList variation);
    public event VariationEventHandler VariationAdded;
    public List<SectionList> BuildPermutations(SectionCollection course, int nPermutations)
    {
      PermutationBuilder builder = new PermutationBuilder(course);
      builder.VariationAdded += Builder_VariationAdded;
      List<SectionList> permutations = builder.BuildPermutations(nPermutations);

      return permutations;
    }

    public IReadOnlyList<SimpleSection> GetSimpleSections(SectionCollection course)
    {
      PermutationBuilder builder = new PermutationBuilder(course);
      return builder.SimpleSections;
    }

    public List<SectionList> AnalyzeLeg(SectionCollection course, int leg)
    {
      LegBuilder builder = new LegBuilder(course, leg);
      builder.VariationAdded += Builder_VariationAdded;

      List<SectionList> permuts = builder.Analyze();
      return permuts;
    }
    public List<SectionList> Analyze(SectionCollection course)
    {
      CourseBuilder builder = new CourseBuilder(course);
      builder.VariationAdded += Builder_VariationAdded;

      List<SectionList> permuts = builder.Analyze();
      return permuts;
    }

    public int EstimatePermutations(SectionCollection course)
    {
      PermutationBuilder builder = new PermutationBuilder(course);

      int permuts = builder.Estimate();
      return permuts;
    }

    void Builder_VariationAdded(object sender, SectionList variation)
    {
      VariationAdded?.Invoke(this, variation);
    }

    public class WhereInfo
    {
      public static WhereInfo Failed = new WhereInfo(State.Failed);
      public static WhereInfo FulFilled = new WhereInfo(State.Fulfilled);

      public enum State { Fulfilled, Unknown, Failed }
      [ThreadStatic]
      private static DataView _whereView;
      [ThreadStatic]
      private static DataRow _exprRow;
      [ThreadStatic]
      private static DataColumn _exprColumn;

      private int _max;
      public int MaxControl { get { return _max; } }

      private readonly State _state;
      private readonly string _parsed;

      public WhereInfo(string where, SectionList permut)
      {
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

      public State GetState(SectionList sections)
      {
        if (_state == State.Failed)
        { return State.Failed; }
        if (_state == State.Fulfilled)
        { return State.Fulfilled; }

        if (sections.ControlsCount <= _max)
        { return State.Unknown; }

        State state = GetState(_parsed, sections, out string parsed);

        return state;
      }

      private State GetState(string where, SectionList sections, out string parse)
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

        if (where.Contains("leg"))
        {
          int leg = 0;
          Control start = sections.Start;
          foreach (Control ctr in sections.Controls)
          {
            if (ctr == start)
            { leg++; }
          }
          where = where.Replace("leg", leg.ToString());
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
            _max = Math.Max(_max, ctrIdx);
          }
        }
        parsed.Append(where);

        parse = parsed.ToString();
        if (_max > sections.ControlsCount)
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
    }

    private class PermutationBuilder : Builder
    {
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
          for (int i = 0; i < Position; i++)
          {
            sb.AppendFormat(" {0}", Sections.GetNextControl(i).Control.Name);
          }
          return sb.ToString();
        }

        private int GetPreviousSplit(int start)
        {
          int i = start - 1;
          while (i >= 0)
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
            NextControl x = Sections.GetNextControl(posX);
            NextControl y = other.Sections.GetNextControl(posY);

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
          return Sections.GetNextControl(Position - 1);
        }
      }
      private class Index
      {
        Dictionary<NextControl, List<SplitInfo>> _dict =
          new Dictionary<NextControl, List<SplitInfo>>(new NextControl.EqualCodeComparer());

        private Dictionary<Control, List<List<SplitInfo>>> _controlDict =
          new Dictionary<Control, List<List<SplitInfo>>>();

        private static Dictionary<Control, List<SplitInfo>> GetControlDict(SectionList sections)
        {
          int i = 0;
          Control pre = sections.GetControl(0);
          Dictionary<Control, List<SplitInfo>> controlDict = new Dictionary<Control, List<SplitInfo>>();
          Dictionary<string, int> splitCount = new Dictionary<string, int>();
          foreach (NextControl v in sections.NextControls)
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
          foreach (List<SplitInfo> splits in controlDict.Values)
          {
            foreach (SplitInfo split in splits)
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

          foreach (KeyValuePair<Control, List<SplitInfo>> pair in controlDict)
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

        private class CommonInfo
        {
          public readonly SectionList Variation;
          public readonly NextWhere Next;
          public readonly int SplitLength;
          public readonly int SplitCount;
          public readonly int VarLength;
          public readonly int VarCount;

          public CommonInfo(SectionList variation, NextWhere next,
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
          private readonly SectionList _pre;
          private readonly Control _split;
          private readonly Dictionary<Control, List<List<SplitInfo>>> _controlDict;

          private Dictionary<char, int> _varCount;

          public CommonCpr(SectionList pre, Control split, Dictionary<Control, List<List<SplitInfo>>> controlDict)
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
                foreach (Control ctr in _pre.Controls)
                {
                  if (pre == split)
                  { pos++; }
                  pre = ctr;
                }
                List<List<SplitInfo>> combs = _controlDict[split];
                Dictionary<char, int> varCount = new Dictionary<char, int>();
                foreach (List<SplitInfo> comb in combs)
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
        public void Sort(SectionList variation, Control split, int iSplit, List<NextWhere> nexts)
        {
          if (_dict.Count == 0)
          { return; }
          if (nexts.Count < 2)
          { return; }

          List<CommonInfo> commons = GetCommons(variation, iSplit, nexts);
          commons.Sort(new CommonCpr(variation, split, _controlDict));
          nexts.Clear();
          foreach (CommonInfo common in commons)
          {
            nexts.Add(common.Next);
          }
        }

        private List<CommonInfo> GetCommons(SectionList variation, int iSplit,
          List<NextWhere> nexts)
        {
          List<CommonInfo> commons = new List<CommonInfo>(nexts.Count);
          foreach (NextWhere next in nexts)
          {
            List<SplitInfo> existing = GetExisting(next.Next);
            int maxCommonVar = 0;
            int maxCountVar = 0;
            int maxCommonSplit = 0;
            int maxCountSplit = 0;
            foreach (SplitInfo split in existing)
            {
              SectionList secExist = split.Sections;
              int posExist = split.Position - 3;
              int posVar = variation.ControlsCount - 3;

              int common = 0;

              while (posVar >= 0 && posExist >= 0)
              {
                NextControl nextVar = variation.GetNextControl(posVar);
                NextControl nextExits = secExist.GetNextControl(posExist);

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
          _sections = new SectionList(parent.StartControl);
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
      private SectionCollection _course;

      private class Stat
      {
        private readonly Dictionary<char, List<NextWhere>> _fullVariations;
        private readonly Dictionary<char, int> _fullCount;
        private readonly Dictionary<char, double> _pseudoCount;
        public Stat(Dictionary<char, List<NextWhere>> fullVariations)
        {
          _fullVariations = fullVariations;

          _fullCount = new Dictionary<char, int>();
          foreach (char key in fullVariations.Keys)
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

      private Dictionary<Control, Stat> _stats;

      public int Estimate()
      {
        if (_course.Count == 0)
        { return 0; }

        SectionList permut = new SectionList(StartControl);
        List<SectionList> validList = new List<SectionList>();

        GetVariations(permut, StartControl, true, true, validList);

        if (validList.Count == 0)
        { return 0; }

        SectionList def = validList[0];

        int nEstimate = 1;
        SectionList estimate = new SectionList(def.GetControl(0));
        int iCtr = 0;
        while (iCtr < def.ControlsCount)
        {
          Control start = estimate.GetControl(iCtr);
          if (NextControls.TryGetValue(start, out NextControlList nexts))
          {
            Dictionary<char, NextWhere> variations = GetPossibleNexts(estimate,
              start, nexts.List, false);

            List<NextWhere> sorted = Sort(estimate, start, variations.Values);
            int vars = 0;
            foreach (NextWhere next in sorted)
            {
              validList = new List<SectionList>();

              SectionList clone = estimate.Clone();
              GetVariations(clone, next.Next, next.Where, true, true, validList);
              if (validList.Count > 0)
              { vars++; }
            }

            nEstimate *= vars;
          }

          if (iCtr < def.ControlsCount - 1)
          { estimate.Add(def.GetNextControl(iCtr)); }
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
          foreach (PermutInfo permutation in permutations)
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

            foreach (PermutInfo permutation in permuts)
            {
              permutation.AddBestNext(last, nexts.List);
            }
          }
        }
        List<SectionList> fullSections = new List<SectionList>(permutations.Count);
        foreach (PermutInfo permutation in permutations)
        { fullSections.Add(permutation.Sections); }
        return fullSections;
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

      protected override Control StartControl { get { return (Control)_course.First.Value; } }
      protected override Control EndControl { get { return (Control)_course.Last.Value; } }

      private Index _index;

      protected override List<SimpleSection> GetSections()
      {
        List<SimpleSection> sections = _course.GetAllSections();
        sections = MakeUniqueControls(sections);

        return sections;
      }

      protected override List<NextWhere> Sort(SectionList variation,
        Control split, IEnumerable<NextWhere> nextEnum)
      {
        List<NextWhere> nexts = base.Sort(variation, split, nextEnum);
        if (nexts.Count < 2)
        { return nexts; }
        if (_index == null)
        { return nexts; }

        string pre = null;
        int iSplit = 0;
        foreach (Control control in variation.Controls)
        {
          if (pre == split.Name)
          { iSplit++; }
          pre = control.Name;
        }
        _index.Sort(variation, split, iSplit, nexts);
        return nexts;
      }

      public List<SectionList> BuildPermutations(int nPermutations)
      {
        _index = new Index();
        List<SectionList> variations = new List<SectionList>(nPermutations);
        for (int i = 0; i < nPermutations; i++)
        {
          SectionList variation = GetVariation();
          variations.Add(variation);
          _index.Add(variation);
        }
        return variations;
      }

      private SectionList GetVariation()
      {
        SectionList permut = new SectionList(StartControl);
        string countExpr = GetCountExpression(_course);
        WhereInfo countWhere = new WhereInfo(countExpr, permut);
        permut.Add(countWhere);
        List<SectionList> variations = new List<SectionList>();

        GetVariations(permut, StartControl, true, true, variations);

        SectionList variation;
        if (variations.Count == 1)
        { variation = variations[0]; }
        else
        { variation = null; }
        return variation;
      }

      [Obsolete("rebuild ++,--")]
      public PermutationBuilder(SectionCollection course)
      {
        SectionCollection init = course;
        if (!(init.First.Value is Control))
        {
          init = course.Clone();
          init.AddFirst(new Control("++", ControlCode.TextBlock));
        }
        if (!(init.Last.Value is Control))
        {
          init = course.Clone();
          init.AddLast(new Control("--", ControlCode.TextBlock));
        }

        _course = init;
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
        List<SectionList> validList = new List<SectionList>();

        SectionList permut = new SectionList(StartControl);
        for (int i = 1; i < _leg; i++)
        {
          permut.Add(StartNext);
        }

        GetVariations(permut, StartControl, false, false, validList);

        foreach (SectionList permutation in validList)
        {
          permutation.SetLeg(_leg);
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
        foreach (SimpleSection section in simples)
        {
          Control from = GetUniqueControl(controls, section.From);
          Control to = GetUniqueControl(controls, section.To);

          MultiSection add = new MultiSection(from, to);
          if (section is MultiSection s)
          {
            foreach (Control c in s.Inter)
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

        SectionList permut = new SectionList(StartControl);
        string countExpr = GetCountExpression(_course);
        WhereInfo countWhere = new WhereInfo(countExpr, permut);
        permut.Add(countWhere);
        List<SectionList> validList = new List<SectionList>();

        GetVariations(permut, StartControl, true, false, validList);

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
        foreach (SimpleSection section in simples)
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

    private abstract class Builder
    {
      public event VariationEventHandler VariationAdded;

      private List<SimpleSection> _sections;
      private Dictionary<Control, NextControlList> _nextControls;
      private Dictionary<SimpleSection, int> _sectionCount;
      private NextControl _startNext;

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
            foreach (NextControlList nextInfo in nextInfos.Values)
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
            foreach (SimpleSection section in Sections)
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
      protected NextControl StartNext
      {
        get
        {
          if (_startNext == null)
          { _startNext = new NextControl(StartControl); }
          return _startNext;
        }
      }

      protected string GetCountExpression(SectionCollection course)
      {
        int nSections = course.LegCount() + Sections.Count - 1;
        string expr = string.Format("[{0}] = '{1}'", nSections, EndControl.Name);
        return expr;
      }

      protected enum InvalidType { WhereFailed, NoVariation }

      protected Dictionary<char, NextWhere> GetPossibleNexts(SectionList pre,
        Control from, IList<NextControl> nexts, bool verifyFull)
      {
        EvaluateNexts(pre, from, nexts, verifyFull, 
          out Dictionary<char, List<NextWhere>> variations, out Dictionary<char, InvalidType> invalids);

        foreach (char code in invalids.Keys)
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

      protected void EvaluateNexts(SectionList pre,
        Control from, IList<NextControl> nexts, bool verifyFull,
        out Dictionary<char, List<NextWhere>> variations,
        out Dictionary<char, InvalidType> invalids)
      {
        variations = new Dictionary<char, List<NextWhere>>();
        invalids = new Dictionary<char, InvalidType>();
        foreach (NextControl next in nexts)
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

      private bool ExistsVariation(SectionList variation, NextControl next, WhereInfo where)
      {
        SectionList candidate = variation.Clone();
        List<SectionList> sectionsList = new List<SectionList>();

        GetVariations(candidate, next, where, true, true, sectionsList);
        bool exists = sectionsList.Count > 0;
        return exists;
      }

      protected void GetVariations(SectionList variation, Control start,
        bool allLegs, bool oneOnly, List<SectionList> valids)
      {
        Dictionary<char, NextWhere> variations = GetPossibleNexts(variation,
          start, NextControls[start].List, false);

        List<NextWhere> sorted = Sort(variation, start, variations.Values);
        foreach (NextWhere next in sorted)
        {
          if (variations.Count > 1)
          {
            SectionList clone = variation.Clone();
            GetVariations(clone, next.Next, next.Where, allLegs, oneOnly, valids);
            if (oneOnly && valids.Count > 0)
            { return; }
          }
          else
          {
            GetVariations(variation, next.Next, next.Where, allLegs, oneOnly, valids);
          }
        }
      }

      protected void GetVariations(SectionList variation, NextControl current, WhereInfo currentWhere,
        bool allLegs, bool oneOnly, List<SectionList> valids)
      {
        do
        {
          variation.Add(current);
          if (currentWhere == null && current.Where != null)
          { currentWhere = new WhereInfo(current.Where, variation); }
          variation.Add(currentWhere);
          WhereInfo.State state = variation.GetState();
          if (state == WhereInfo.State.Failed)
          { return; }

          if (NextControls.TryGetValue(current.Control, out NextControlList nextList) == false) // last Control
          {
            if (allLegs == false)
            {
              if (state == WhereInfo.State.Fulfilled && current.Control == EndControl)
              {
                valids.Add(variation);
                OnVariationAdded(variation);
              }
              return;
            }
            current = null;
            nextList = NextControls[StartControl];
          }

          Control split = current != null ? current.Control : StartControl;
          SectionList nextVariation = variation;
          if (current == null)
          {
            nextVariation = nextVariation.Clone();
            nextVariation.Add(StartNext);
          }
          Dictionary<char, NextWhere> nextsDict = GetPossibleNexts(nextVariation, split, nextList.List, false);

          bool finished = true;
          List<NextWhere> nexts = Sort(variation, split, nextsDict.Values);
          foreach (NextWhere next in nexts)
          {
            if (next == null) // only invalid nexts
            {
              if (current == null)
              { break; }
              return;
            }

            finished = false;

            if (current == null)
            {
              current = StartNext;
              variation.Add(current);
            }

            if (nexts.Count > 1)
            {
              SectionList clone = variation.Clone();
              GetVariations(clone, next.Next, next.Where, allLegs, oneOnly, valids);
              if (oneOnly && valids.Count > 0)
              { return; }
            }
            else
            {
              current = next.Next;
              currentWhere = next.Where;
            }
          }
          if (nexts.Count > 1)
          {
            return;
          }
          if (finished)
          {
            WhereInfo.State finishedState = variation.GetState();
            if (finishedState == WhereInfo.State.Fulfilled)
            {
              valids.Add(variation);
              OnVariationAdded(variation);
            }
            return;
          }
        } while (true);
      }

      private void OnVariationAdded(SectionList variation)
      {
        VariationAdded?.Invoke(this, variation);
      }

      protected virtual List<NextWhere> Sort(SectionList variation, Control split, IEnumerable<NextWhere> nextEnum)
      {
        List<NextWhere> nexts = new List<NextWhere>(nextEnum);
        return nexts;
      }

      private WhereInfo GetWhere(SectionList sections, Control from, NextControl next)
      {
        int nSections = sections.ControlsCount - 1;
        if (nSections == 0)
        {
          return WhereInfo.FulFilled;
        }
        WhereInfo where = WhereInfo.FulFilled;
        if (next.Where != null)
        {
          where = new WhereInfo(next.Where, sections);
          WhereInfo.State state = where.GetState(sections);
          if (state == WhereInfo.State.Failed)
          { return where; }
        }
        if (from == null) from = sections.Start;
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
        foreach (SimpleSection section in sections)
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
        foreach (SimpleSection section in sections)
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
          foreach (SplitSection.Branch branch in split.Branches)
          {
            BuildExplicit(branch, controls);
          }
        }

        node = node.Next;
      }
    }
  }
}
