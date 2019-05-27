using Ocad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCourse.Ext
{
  partial class VariationBuilder
  {
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
      private class CountInfo
      {
        public Dictionary<char, double> Expected { get; set; }

        public Dictionary<int, Dictionary<char, int>> Actual { get; set; }

        public override string ToString()
        {
          StringBuilder sb = new StringBuilder();
          sb.Append("Exp: ");
          foreach (var pair in Expected)
          { sb.Append($"{pair.Key}:{pair.Value};"); }
          sb.Append("  ");

          foreach (var splitPair in Actual)
          {
            sb.Append($"(S{splitPair.Key})");
            foreach (var key in Expected.Keys)
            {
              if (splitPair.Value.TryGetValue(key, out int count))
              { sb.Append($"{key}:{count};"); }
              else
              { sb.Append($"{key}:-;"); }
            }
            sb.Append("  ");
          }
          return sb.ToString();
        }

        public void Add(SplitInfo split)
        {
          NextControl v = split.GetNextControl();
          if (!Actual.TryGetValue(split.Split, out Dictionary<char, int> splitCount))
          {
            splitCount = new Dictionary<char, int>();
            Actual.Add(split.Split, splitCount);
          }
          if (!splitCount.TryGetValue(v.Code, out int count))
          {
            splitCount[v.Code] = 1;
          }
          else
          {
            splitCount[v.Code]++;
          }
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
        Dictionary<NextControl, List<SplitInfo>> _nextSplits =
          new Dictionary<NextControl, List<SplitInfo>>(new NextControl.EqualCodeComparer());

        private Dictionary<Control, List<List<SplitInfo>>> _controlDict =
          new Dictionary<Control, List<List<SplitInfo>>>();

        private Dictionary<Control, CountInfo> _controlCounts;
        private Dictionary<Control, CountInfo> ControlCounts => _controlCounts ?? (_controlCounts = InitCountInfo(_builder, _nextSplits.Values));

        public bool UseCountInfo { get; set; }

        public Index(PermutationBuilder builder)
        {
          _builder = builder;
        }

        private static Dictionary<Control, CountInfo> InitCountInfo(PermutationBuilder builder,
          IEnumerable<List<SplitInfo>> splitLists)
        {
          Dictionary<Control, CountInfo> dict = new Dictionary<Control, CountInfo>();
          foreach (var control in builder.NextControls)
          {
            foreach (var next in control.Value.List)
            {
              if (next.Code == 0)
              { continue; }

              if (!dict.TryGetValue(control.Key, out CountInfo countInfo))
              {
                countInfo = new CountInfo()
                {
                  Expected = new Dictionary<char, double>(),
                  Actual = new Dictionary<int, Dictionary<char, int>>()
                };
                dict.Add(control.Key, countInfo);
              }
              if (!countInfo.Expected.TryGetValue(next.Code, out double count))
              {
                countInfo.Expected[next.Code] = 1;
              }
              else
              {
                countInfo.Expected[next.Code]++;
              }
            }
          }

          foreach (var splits in splitLists)
          {
            foreach (var split in splits)
            {
              Control key = split.Sections.GetControl(split.Position - 1);
              CountInfo countInfo = dict[key];
              countInfo.Add(split);
            }
          }

          return dict;
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
          foreach (var pair in controlDict)
          {
            List<SplitInfo> splits = pair.Value;

            foreach (var split in splits)
            {
              NextControl nextControl = split.GetNextControl();
              if (!_nextSplits.TryGetValue(nextControl, out List<SplitInfo> existing))
              {
                existing = new List<SplitInfo>();
                _nextSplits.Add(nextControl, existing);
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

          if (_controlCounts != null)
          {
            foreach (var pair in controlDict)
            {
              Control key = pair.Key;
              CountInfo countInfo = _controlCounts[key];

              List<SplitInfo> splits = pair.Value;
              foreach (var split in splits)
              {
                countInfo.Add(split);
              }
            }
          }
        }

        public void Sort(ISectionList variation, Control split, int iSplit, List<NextWhere> nexts)
        {
          if (_nextSplits.Count == 0)
          { return; }
          if (nexts.Count < 2)
          { return; }

          List<NextWhere> sorted = (!UseCountInfo)
            ? sorted = new List<NextWhere>(EnumSorted(variation, split, iSplit, nexts))
            : sorted = GetCountSorted(variation, split, iSplit, nexts);
          nexts.Clear();
          nexts.AddRange(sorted);
        }
        private List<NextWhere> GetCountSorted(ISectionList variation, Control split, int iSplit, List<NextWhere> allNexts)
        {
          CountInfo countInfo = ControlCounts[split];
          Dictionary<int, double[]> diff = new Dictionary<int, double[]>();
          Dictionary<int, Dictionary<double, List<char>>> counts = new Dictionary<int, Dictionary<double, List<char>>>();
          foreach (var pair in countInfo.Actual)
          {
            if (pair.Key < iSplit)
            { continue; }

            Dictionary<double, List<char>> exp = new Dictionary<double, List<char>>();
            double min = double.MaxValue;
            double max = 0;
            foreach (var countPair in pair.Value)
            {
              double normed = countPair.Value / countInfo.Expected[countPair.Key];
              if (!exp.TryGetValue(normed, out List<char> vars))
              {
                vars = new List<char>();
                exp.Add(normed, vars);
              }
              vars.Add(countPair.Key);
              min = Math.Min(min, normed);
              max = Math.Max(max, normed);
            }
            diff.Add(pair.Key, new[] { min, max });
            counts.Add(pair.Key, exp);
          }
          int iMax = -1;
          double dMax = 0;
          foreach (var pair in diff)
          {
            double d = pair.Value[1] - pair.Value[0];
            if (d > dMax)
            {
              dMax = d;
              iMax = pair.Key;
            }
          }
          List<NextWhere> sortedNexts = new List<NextWhere>();
          List<NextWhere> processNexts = new List<NextWhere>(allNexts);
          if (dMax > 1)
          {
            List<char> prefers = (iMax == iSplit)
              ? counts[iMax][diff[iMax][0]]
              : counts[iMax][diff[iMax][1]];
            List<NextWhere> preferredNexts = new List<NextWhere>();
            List<NextWhere> remainNexts = new List<NextWhere>();
            foreach (var next in processNexts)
            {
              if (prefers.Contains(next.Next.Code))
              { preferredNexts.Add(next); }
              else
              { remainNexts.Add(next); }
            }
            processNexts = remainNexts;

            sortedNexts.AddRange(EnumSorted(variation, split, iSplit, preferredNexts));
          }

          sortedNexts.AddRange(EnumSorted(variation, split, iSplit, processNexts));

          return sortedNexts;
        }
        private IEnumerable<NextWhere> EnumSorted(ISectionList variation, Control split, int iSplit, List<NextWhere> nexts)
        {
          List<CommonInfo> commons = GetCommons(variation, split, iSplit, nexts);
          commons.Sort(new CommonCpr(variation, split, _controlDict));
          return commons.Select(x => x.Next);
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
          if (_nextSplits.TryGetValue(next, out List<SplitInfo> existing))
          { return existing; }

          // Fuer mehrfache identische Teilstrecken in unterschiedlichen Splitsections
          NextControl nextA = new NextControl(next.Control) { Code = 'A' };
          return _nextSplits[nextA];
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
        sections = MakeUniqueControls(sections, whereDependent: false);

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

        bool orderedLegs = false;
        _index.UseCountInfo = true;

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

  }
}
