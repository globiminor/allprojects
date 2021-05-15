using Ocad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace OCourse.Ext
{
  partial class VariationBuilder
  {
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
      private const string Lc = "Lc";
      private const string Pc = "Pc";

      private static readonly List<string> _keys = new List<string> { Leg, PartControls, LegControls };
      public static IReadOnlyList<string> Keywords => _keys;

      private string Evaluate(ISectionList sections, DataSet courseDs,
        string tableName, string eval, string filter)
      {
        tableName = tableName.Trim();
        if (tableName.Equals(Lc, StringComparison.InvariantCultureIgnoreCase))
        { tableName = LegControls; }
        if (tableName.Equals(Pc, StringComparison.InvariantCultureIgnoreCase))
        { tableName = PartControls; }

        eval = eval.Trim();
        if (eval == "#")
        { eval = "Count(Control)"; }

        filter = filter.Trim();
        if (filter[0] == '\'' && filter[filter.Length - 1] == '\'')
        { filter = $"Control = {filter}"; }

        if (!courseDs.Tables.Contains(tableName))
        {

          if (tableName.Equals(LegControls, StringComparison.InvariantCultureIgnoreCase))
          {
            DataTable t = new DataTable(LegControls);
            courseDs.Tables.Add(t);

            t.Columns.Add("Position", typeof(int));
            t.Columns.Add("Control", typeof(string));

            GetLegControls(sections, null, out List<Control> legControls);
            int iControl = 0;
            foreach (Control control in legControls)
            {
              t.Rows.Add(iControl, control.Name);
              iControl++;
            }
            t.AcceptChanges();
          }
          else
          { throw new NotImplementedException(); }
          //TODO
        }
        DataTable tbl = courseDs.Tables[tableName];
        return $"{tbl.Compute(eval, filter)}";
      }
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

        using (DataSet courseDs = new DataSet())
        {
          State state = State.Unknown;
          where = Evaluate(where, (string expression) =>
            {
              string[] parts = expression.Split(';');
              if (parts.Length == 3)
              {
                string table = parts[0];
                string aggr = parts[1];
                string filter = parts[2];

                return Evaluate(sections, courseDs, table, aggr, filter);
              }
              if (parts.Length == 1)
              {
                string ctrName = expression;

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
                  state = State.Failed;
                  return null;
                }

                if (ctrIdx <= sections.ControlsCount)
                {
                  Control ctr = sections.GetControl(ctrIdx);
                  return $"'{ctr.Name}'";
                }
                else
                {
                  MaxControl = Math.Max(MaxControl, ctrIdx);
                  return null;
                }

              }
              throw new NotImplementedException();
            });

          if (where == null)
          {
            return state;
          }

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
            if (legControls == null)
            {
              bool containsControl = GetLegControls(sections, _nextControl, out List<Control> legControlList);
              if (!containsControl)
              {
                legControls = null;
                break;
              }
              GetControlsInList(legControlList, out legControls);
            }

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

      private string Evaluate(string where, Func<string, string> eval)
      {
        while (where?.IndexOf("[") >= 0)
        {
          where = EvaluateNext(where, eval, searchEnd: false);
        }
        return where;
      }
      private string EvaluateNext(string expression, Func<string, string> evalFunc, bool searchEnd)
      {
        int iStart = expression.IndexOf('[', 0);
        int iEnd = expression.IndexOf(']');
        if (iStart < iEnd && iStart >= 0)
        {
          string evaluated = EvaluateNext(expression.Substring(iStart + 1), evalFunc, searchEnd: true);
          if (evaluated == null)
          { return null; }
          string result = $"{expression.Substring(0, iStart)}{evaluated}";
          return result;
        }
        if (searchEnd)
        {
          string eval = expression.Substring(0, iEnd);
          string evaluated = evalFunc(eval);
          if (evaluated == null)
          { return null; }
          string result = $"{evaluated}{expression.Substring(iEnd + 1)}";
          return result;
        }
        return expression;
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
      private bool GetLegControls(ISectionList sections, NextControl control, out List<Control> legControls)
      {
        List<Control> currentLeg = null;
        bool isControlInCurrentPart = false;
        foreach (var ctr in sections.NextControls)
        {
          if (_legController.IsLegStart(ctr))
          {
            if (isControlInCurrentPart) { legControls = currentLeg; return true; }
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
            if (isControlInCurrentPart) { legControls = currentLeg; return true; }
            currentLeg = null;
          }
        }
        legControls = currentLeg;
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

    internal class SplitInfo // : IComparable<SplitInfo>
    {
      public ISectionList Sections { get; }
      public int Position { get; }
      public int Split { get; }
      public SplitInfo(ISectionList sections, int position, int split)
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

        // List<SimpleSection> parts = b.GetParts(course, leg);
        // simples = parts;

        List<SimpleSection> sections = new List<SimpleSection>(simples.Count);
        Dictionary<string, Control> controls = new Dictionary<string, Control>();
        foreach (var section in simples)
        {
          //Control from = GetUniqueControl(controls, section.From, section.Where);
          //Control to = GetUniqueControl(controls, section.To, section.Where);

          Control from = GetUniqueControl(controls, section.From, null);
          Control to = GetUniqueControl(controls, section.To, null);

          MultiSection add = new MultiSection(from, to);
          if (section is MultiSection s)
          {
            foreach (var c in s.Inter)
            {
              Control u = GetUniqueControl(controls, c, section.Where);
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
          Control from = GetUniqueControl(controls, section.From, section.Where);
          Control to = GetUniqueControl(controls, section.To, section.Where);

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

      protected static List<SimpleSection> MakeUniqueControls(IList<SimpleSection> sections,
        bool whereDependent)
      {
        Dictionary<string, Control> controls = new Dictionary<string, Control>();
        List<SimpleSection> unique = new List<SimpleSection>(sections.Count);
        foreach (var section in sections)
        {
          string where = whereDependent ? section.Where : null;
          Control from = GetUniqueControl(controls, section.From, where);
          Control to = GetUniqueControl(controls, section.To, where);
          SimpleSection add = new SimpleSection(from, to);
          add.SetWhere(section.Where);
          unique.Add(add);
        }
        return unique;
      }

      protected static Control GetUniqueControl(Dictionary<string, Control> controls, Control control, string where)
      {
        if (control.Code == ControlCode.TextBlock || control.Code == ControlCode.MapChange)
        { return control; }

        string key = $"{where};{control.Name}";
        if (controls.TryGetValue(key, out Control unique) == false)
        {
          unique = control;
          controls.Add(key, unique);
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
  }
}
