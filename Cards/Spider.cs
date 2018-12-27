using Basics.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cards
{
  [DataContract]
  public class Spider : GameBase<Spider.Move>
  {
    [Flags]
    internal enum MoveType
    {
      Unknown = 0,
      Simple = 1,
      CardFlip = 2,
      FullSuite = 4
    }
    public class Move : MoveBase
    {
      public Move(int fromCol, int fromCard, int toCol)
      {
        FromColumn = fromCol;
        FromCard = fromCard;
        ToColumn = toCol;
      }
      public int FromColumn;
      public int FromCard;
      public int ToColumn;

      internal MoveType MoveType { get; set; }

      public override string ToString()
      {
        return string.Format("{0}.{1}->{2}", FromColumn, FromCard, ToColumn);
      }
    }

    public class SpiderStand : Stand<Spider, Move>, IHasInfo
    {
      private bool _init;
      private int _nCompleted;
      private int _nCovered;
      private int _nFree;
      private int _minFree;
      private int _positionCode;

      private double _potential;
      private int _lastSpecialMove;
      private int _nCombMoves;

      public SpiderStand(Spider start)
        : base(start)
      {
        Title = start.Title;
        _potential = double.NaN;
        Points = 500;
      }

      public int NCompleted { get { return _nCompleted; } }
      public int NFree { get { return _nFree; } }
      public int NCovered { get { return _nCovered; } }

      public string Info
      {
        get { return string.Format("Potential: {0}", Potential); }
      }

      public override Stand Revert()
      {
        if (Pre == null)
        { return this; }
        return Pre;
      }

      public new SpiderStand Pre { get { return (SpiderStand)base.Pre; } }
      internal SpiderStand InitStatistics()
      {
        if (_init)
        { return this; }

        Spider spider = GetCurrent(this);
        _nCompleted = spider.Completed.Count;
        _nCovered = 0;
        _nFree = 0;
        _minFree = int.MaxValue;
        foreach (var covered in spider.ColumnsCovered)
        {
          _nCovered += covered.Count;

          if (covered.Count == 0)
          { _nFree++; }
          else if (_minFree > covered.Count)
          { _minFree = covered.Count; }
        }

        double potential = 0;
        for (int i = 0; i < spider.ColumnCount; i++)
        {
          List<Card> covered = spider.ColumnsCovered[i];
          List<Card> open = spider.ColumnsOpen[i];

          potential -= covered.Count;
          potential += GetPotential(open);
        }
        foreach (var completed in spider.Completed)
        {
          potential += GetPotential(completed);
        }
        foreach (var stack in spider.Stack)
        {
          potential -= stack.Count;
        }
        _potential = potential;

        _lastSpecialMove = 0;
        Stand<Move> x = this;
        while (x.Pre != null)
        {
          if (x.Move.MoveType == MoveType.Simple)
          {
            _lastSpecialMove++;
            x = x.Pre;
          }
          else
          { break; }
        }

        _nCombMoves = 0;
        x = this;
        while (x.Pre != null && x.Pre.Move != null)
        {
          Move pre = x.Pre.Move;
          if (pre.FromColumn >= 0 && x.Move.FromColumn >= 0
            && pre.MoveType == MoveType.Simple
            && x.Move.MoveType == MoveType.Simple)
          {
            if (pre.FromColumn == x.Move.FromColumn
              || pre.ToColumn == x.Move.ToColumn
              || pre.FromColumn == x.Move.ToColumn)
            {
              _nCombMoves++;
            }
          }
          x = x.Pre;
        }

        _init = true;

        return this;
      }

      public override int PositionCode
      {
        get
        {
          if (_positionCode == 0)
          {
            Spider stand = GetCurrent(this);
            foreach (var open in stand.ColumnsOpen)
            {
              _positionCode = _positionCode + 29;
              foreach (var card in open)
              { _positionCode = (41 * _positionCode) ^ card.GetCardCode(); }
            }
          }
          return _positionCode;
        }
      }
      public override double Potential
      {
        get
        {
          if (double.IsNaN(_potential))
          {
            InitStatistics();
          }
          return _potential - 3 * MovesCount + 3 * _nCombMoves - 3 * _lastSpecialMove;
        }
      }
      public override bool EqualPosition(Stand<Move> other)
      {
        if (other is SpiderStand so)
        {
          if (PositionCode != so.PositionCode)
          { return false; }
        }
        return base.EqualPosition(other);
      }

      public override IEnumerable<Move> GetPossibleMoves()
      {
        int freeCols = 0;
        Spider stand = GetCurrent(this);
        List<Move> moves = new List<Move>();
        for (int iCol = 0; iCol < stand.ColumnCount; iCol++)
        {
          List<Card> opens = stand.ColumnsOpen[iCol];
          if (opens == null || opens.Count == 0)
          {
            freeCols++;
            continue;
          }

          Card sub = null;
          for (int i = opens.Count - 1; i >= 0; i--)
          {
            Card open = opens[i];
            int h = open.Height.H;
            if (sub != null &&
              (sub.Suite.Code != open.Suite.Code ||
              sub.Height.H + 1 != h))
            { break; }

            for (int iMove = 0; iMove < stand.ColumnCount; iMove++)
            {
              List<Card> to = stand.ColumnsOpen[iMove];
              if (to == null || to.Count == 0 || to[to.Count - 1].Height.H == h + 1)
              {
                moves.Add(new Move(iCol, opens.Count - i, iMove));
              }
            }
            sub = open;
          }
        }
        if (freeCols == 0 && stand.Stack != null && stand.Stack.Count > 0)
        {
          moves.Add(new Move(-1, -1, -1));
        }

        if (stand.Completed.Count == 8)
        { return null; }

        return moves;
      }

      public override IEnumerable<CardPosition> GetCardPositions()
      {
        Spider start = Game;
        Spider stand = GetCurrent(this);
        return stand.GetCardPositions();
      }

      public override Stand TryMove(double fx, double fy, double tx, double ty)
      {
        Spider start = Game;
        Spider stand = GetCurrent(this);
        Move move = stand.TryGetMove(fx, fy, tx, ty);
        if (move == null)
        { return null; }
        bool success = false;
        foreach (var possible in GetPossibleMoves())
        {
          if (move.FromCard == possible.FromCard &&
            move.FromColumn == possible.FromColumn &&
            move.ToColumn == possible.ToColumn)
          {
            success = true;
            break;
          }
        }
        if (!success)
        { return null; }

        SpiderStand moved = (SpiderStand)start.CreateStand();
        moved.SetMove(this, move);
        if ((move.MoveType & MoveType.FullSuite) == MoveType.FullSuite)
        { moved.Points = Points + 100; }
        else
        { moved.Points = Points - 1; }
        return moved;
      }

      internal Spider GetCurrent(Stand<Move> stand)
      {
        Spider start = Game;

        if (start._cache.TryGetValue(stand, out Spider cache))
        {
          return cache;
        }

        int nMoves = stand.MovesCount;
        if (stand.Pre == null)
        {
          if (nMoves == 0)
          {
            return start;
          }
          throw new InvalidProgramException("Pre not defined");
        }

        Spider pre = GetCurrent(stand.Pre);
        cache = pre.CreateNew(stand.Move);
        start._cache.Add(stand, cache);
        return cache;
      }

      private double GetPotential(List<Card> cards)
      {
        if (cards.Count == 0)
        { return 3; }

        double pot = 0;
        Card pre = null;
        double g = 1.3;
        double groups = g;
        foreach (var card in cards)
        {
          if (pre != null)
          {
            if (pre.Height.H == card.Height.H + 1)
            {
              pot++;
              if (pre.Suite.Code == card.Suite.Code)
              {
                pot += 2;
              }
            }
            else
            {
              pot -= groups;
              groups += g;
            }
          }
          pre = card;
        }
        return pot;
      }
    }

    private class MoveQuality
    {
      public static int CompareQuality(MoveQuality sx, MoveQuality sy)
      {
        int cmp = CompareQualityCore(sx, sy);
        return cmp;
      }
      private static int CompareQualityCore(MoveQuality sx, MoveQuality sy)
      {
        if (sx == sy)
        { return 0; }

        SpiderStand x = sx._ss;
        SpiderStand y = sy._ss;
        if (x == null)
        {
          if (y == null) { return 0; }
          return 1;
        }
        if (y == null) { return -1; }

        x.InitStatistics();
        y.InitStatistics();

        int d;

        d = sx.StraitInterrupt.CompareTo(sy.StraitInterrupt);
        if (d != 0)
        { return d; }

        d = sx.EqualSuite.CompareTo(sy.EqualSuite);
        if (d != 0)
        { return -d; }

        d = sx.EnlargeStrait.CompareTo(sy.EnlargeStrait);
        if (d != 0)
        { return -d; }

        d = sx.NearInvariant.CompareTo(sy.NearInvariant);
        if (d != 0)
        { return d; }

        d = sx.ToCard.Height.H.CompareTo(sy.ToCard.Height.H);
        if (d != 0)
        { return -d; }

        d = x.NCompleted.CompareTo(y.NCompleted);
        if (d != 0) { return -d; }

        d = x.NFree.CompareTo(y.NFree);
        if (d != 0) { return -d; }

        return d;
      }

      private static readonly Card _voidCard = new Card(new Suite("X", "void"), new Height(-1, "X"));
      private static readonly Card _newRowCard = new Card(new Suite("Row", "Row"), new Height(-2, "Row"));
      private static readonly Card _emptyColCard = new Card(new Suite("Empty", "Empty"), new Height(-3, "Empty"));

      private readonly Stand<Move> _stand;
      private readonly SpiderStand _ss;
      private readonly Move _lastMove;
      private Spider _spider;

      private bool? _straitInterrupt;
      private bool? _equalSuite;
      private bool? _enlargeStrait;
      private bool? _nearInvariant;

      private Card _fromCard;
      private Card _toCard;
      private Card _remainCard;

      public MoveQuality(Stand<Move> stand)
      {
        _stand = stand;
        _ss = stand as SpiderStand;
        if (_stand.MovesCount == 0)
        { _lastMove = new Move(int.MinValue, int.MinValue, int.MinValue); }
        else
        { _lastMove = _stand.Move; }
      }
      public Stand<Move> Stand { get { return _stand; } }

      public bool StraitInterrupt
      {
        get
        {
          if (!_straitInterrupt.HasValue)
          {
            _straitInterrupt =
              FromCard.Suite.Code == RemainCard.Suite.Code &&
              FromCard.Height.H + 1 == RemainCard.Height.H &&
              ToCard.Suite.Code != FromCard.Suite.Code;
          }
          return _straitInterrupt.Value;
        }
      }

      public bool NearInvariant
      {
        get
        {
          if (!_nearInvariant.HasValue)
          {
            _nearInvariant =
              FromCard.Height.H + 1 == ToCard.Height.H &&
              FromCard.Height.H + 1 == RemainCard.Height.H &&
              _ss.Pre.InitStatistics().NCovered == _ss.NCovered;
          }
          return _nearInvariant.Value;
        }
      }

      public bool EqualSuite
      {
        get
        {
          if (!_equalSuite.HasValue)
          {
            _equalSuite =
              FromCard.Suite.Code == ToCard.Suite.Code &&
              FromCard.Height.H + 1 == ToCard.Height.H &&
              (_ss.Pre.InitStatistics().NCovered < _ss.NCovered ||
               RemainCard.Suite.Code != FromCard.Suite.Code ||
               RemainCard.Height.H != FromCard.Height.H + 1);
          }
          return _equalSuite.Value;
        }
      }

      public bool EnlargeStrait
      {
        get
        {
          if (!_enlargeStrait.HasValue)
          { _enlargeStrait = InitEnlargeStrait(); }
          return _enlargeStrait.Value;
        }
      }

      private Spider Spider
      {
        get
        {
          if (_spider == null && _ss != null)
          { _spider = _ss.GetCurrent(_ss); }
          return _spider;
        }
      }

      private Card FromCard
      {
        get
        {
          if (_fromCard == null)
          { _fromCard = InitFromCard(); }
          return _fromCard;
        }
      }

      private Card ToCard
      {
        get
        {
          if (_toCard == null)
          { _toCard = InitToCard(); }
          return _toCard;
        }
      }

      private Card RemainCard
      {
        get
        {
          if (_remainCard == null)
          { _remainCard = InitRemainCard(); }
          return _remainCard;
        }
      }

      public override string ToString()
      {
        return _lastMove.ToString();
      }

      private Card InitFromCard()
      {
        if (_ss == null)
        { return _voidCard; }
        if (_lastMove.FromCard < 0)
        { return _newRowCard; }
        List<Card> toCol = Spider.ColumnsOpen[_lastMove.ToColumn];
        int fromIdx = toCol.Count - _lastMove.FromCard;
        Card fromCard = toCol[fromIdx];
        return fromCard;
      }

      private Card InitRemainCard()
      {
        if (_ss == null)
        { return _voidCard; }
        if (_lastMove.FromCard < 0)
        { return _newRowCard; }
        List<Card> fromCol = Spider.ColumnsOpen[_lastMove.FromColumn];
        if (fromCol.Count == 0)
        { return _emptyColCard; }

        Card remainCard = fromCol[fromCol.Count - 1];
        return remainCard;
      }

      private Card InitToCard()
      {
        if (_ss == null)
        { return _voidCard; }
        if (_lastMove.FromCard < 0)
        { return _newRowCard; }
        List<Card> toCol = Spider.ColumnsOpen[_lastMove.ToColumn];
        int fromIdx = toCol.Count - _lastMove.FromCard;
        if (fromIdx == 0)
        { return _emptyColCard; }
        Card toCard = toCol[fromIdx - 1];
        return toCard;
      }

      private bool InitEnlargeStrait()
      {
        bool equalSuite =
          FromCard.Suite.Code == ToCard.Suite.Code &&
          FromCard.Height.H + 1 == ToCard.Height.H &&
          _ss.Pre.InitStatistics().NCovered == _ss.NCovered &&
          RemainCard.Suite.Code == FromCard.Suite.Code &&
          RemainCard.Height.H == FromCard.Height.H + 1;

        if (!equalSuite)
        { return false; }

        List<Card> fromCol = Spider.ColumnsOpen[_lastMove.FromColumn];
        int nFrom = StraitCount(fromCol, 1);
        List<Card> toCol = Spider.ColumnsOpen[_lastMove.ToColumn];
        int nTo = StraitCount(toCol, _lastMove.FromCard + 1);
        return nTo > nFrom;
      }

      private int StraitCount(List<Card> cards, int startIndex)
      {
        int iStart = cards.Count - startIndex;
        Card sub = cards[iStart];
        int strait = 1;
        for (int iCard = iStart - 1; iCard >= 0; iCard--)
        {
          Card current = cards[iCard];
          if (sub.Suite.Code != current.Suite.Code ||
            sub.Height.H != current.Height.H - 1)
          { return strait; }
          sub = current;
          strait++;
        }

        return strait;
      }
    }

    private class Pos
    {
      public Pos(CardPosition pos, int col, int card)
      {
        Position = pos;
        Column = col;
        Card = card;
      }

      public CardPosition Position;
      public int Column;
      public int Card;
    }

    private List<List<Card>> _completed;
    [DataMember]
    public List<List<Card>> ColumnsCovered { get; private set; }
    [DataMember]
    public List<List<Card>> ColumnsOpen { get; private set; }
    [DataMember]
    public List<List<Card>> Stack { get; private set; }
    [DataMember]
    public List<List<Card>> Completed
    {
      get
      {
        if (_completed == null) { _completed = new List<List<Card>>(); }
        return _completed;
      }
      private set { _completed = value; }
    }

    public int ColumnCount { get; private set; }
    public string Title { get; private set; }

    private readonly Cache<Stand<Move>, Spider> _cache =
      new Cache<Stand<Move>, Spider>();

    public override List<Card> GetDeck()
    {
      List<Card> cards = new List<Card>();
      foreach (var suite in Suite.Suites)
      {
        cards.AddRange(suite.CreateCards());
        cards.AddRange(suite.CreateCards());
      }
      return cards;
    }

    public override void Init(List<Card> cards, string name = null)
    {
      ColumnCount = 10;
      Title = string.Format("Spider {0}", name);

      int iCard = 0;
      for (int i = 0; i < 44; i++)
      {
        GetCoveredColumn(i % ColumnCount).Add(cards[iCard]);
        iCard++;
      }
      for (int i = 0; i < 10; i++)
      {
        GetOpenColumn(i).Add(cards[iCard]);
        iCard++;
      }

      Stack = new List<List<Card>>();
      for (int iStack = 0; iStack < 5; iStack++)
      {
        List<Card> stack = new List<Card>();
        Stack.Add(stack);
        for (int iCol = 0; iCol < ColumnCount; iCol++)
        {
          stack.Add(cards[iCard]);
          iCard++;
        }
      }
    }

    protected override void Sort(List<Stand<Move>> stands)
    {
      List<MoveQuality> mqs = new List<MoveQuality>(stands.Count);
      foreach (var stand in stands)
      { mqs.Add(new MoveQuality(stand)); }
      mqs.Sort(MoveQuality.CompareQuality);
      stands.Clear();
      foreach (var mq in mqs)
      { stands.Add(mq.Stand); }
    }
    public override IEnumerable<CardPosition> GetCardPositions(IEnumerable<Move> moves)
    {
      throw new NotImplementedException();
    }

    public override IEnumerable<Move> GetPossibleNextMoves(IEnumerable<Move> moves)
    {
      throw new NotImplementedException();
    }

    protected override Stand<Spider.Move> CreateStandCore()
    {
      return new SpiderStand(this);
    }

    private Spider CreateNew(Move move)
    {
      Spider clone = Clone();
      move.MoveType = MoveType.Simple;

      if (move.FromColumn < 0)
      {
        int iStack = clone.Stack.Count - 1;
        List<Card> stack = clone.Stack[iStack];
        clone.Stack.RemoveAt(iStack);

        for (int iCol = 0; iCol < clone.ColumnCount; iCol++)
        {
          clone.ColumnsOpen[iCol].Add(stack[iCol]);
        }
      }
      else
      {
        List<Card> from = clone.ColumnsOpen[move.FromColumn];
        List<Card> to = clone.ColumnsOpen[move.ToColumn];
        int iStart = from.Count - move.FromCard;

        to.AddRange(from.GetRange(iStart, from.Count - iStart));
        from.RemoveRange(iStart, from.Count - iStart);
      }
      // extra moves
      for (int iCol = 0; iCol < ColumnCount; iCol++)
      {
        List<Card> open = clone.ColumnsOpen[iCol];
        bool complete = true;
        int nComplete = 13;
        for (int iH = 1; iH <= nComplete; iH++)
        {
          int iCard = open.Count - iH;
          if (iCard < 0)
          {
            complete = false;
            break;
          }
          if (open[iCard].Height.H != iH)
          {
            complete = false;
            break;
          }
          if (open[iCard].Suite.Code != open[open.Count - 1].Suite.Code)
          {
            complete = false;
            break;
          }
        }

        if (complete)
        {
          move.MoveType |= MoveType.FullSuite;
          clone.Completed.Add(
            new List<Card>(open.GetRange(open.Count - nComplete, nComplete)));
          open.RemoveRange(open.Count - nComplete, nComplete);
        }

        if (open.Count == 0)
        {
          List<Card> covered = clone.ColumnsCovered[iCol];
          int iCovered = covered.Count - 1;
          if (iCovered >= 0)
          {
            move.MoveType |= MoveType.CardFlip;
            open.Add(covered[iCovered]);
            covered.RemoveAt(iCovered);
          }
        }

      }

      return clone;
    }

    protected override GameBase CloneRaw()
    { return Clone(); }
    private new Spider Clone()
    {
      Spider clone = new Spider();
      clone.ColumnCount = ColumnCount;
      clone.ColumnsCovered = CloneList(ColumnsCovered);
      clone.ColumnsOpen = CloneList(ColumnsOpen);
      clone.Stack = CloneList(Stack);
      clone.Completed = CloneList(Completed);
      return clone;
    }

    private List<List<Card>> CloneList(List<List<Card>> orig)
    {
      List<List<Card>> clone = new List<List<Card>>(orig.Count);
      foreach (var cards in orig)
      {
        clone.Add(new List<Card>(cards));
      }
      return clone;
    }

    private double _x0 = 0.5;
    private readonly double _dx = 1.2;
    private double _y0 = 2;
    private readonly double _dy = 0.5;
    private double _yStack = 0.2;

    private Move TryGetMove(double fx, double fy, double tx, double ty)
    {
      int fromCol = int.MinValue;
      int fromCard = int.MinValue;
      int toCol = int.MinValue;

      foreach (var pos in GetPositions())
      {
        if (pos.Position.Left < tx && pos.Position.Left + 1 > tx && pos.Column >= 0)
        { toCol = pos.Column; }

        if (pos.Position == null)
        { continue; }

        if (pos.Position.Left < fx && pos.Position.Left + 1 > fx &&
          pos.Position.Top < fy && pos.Position.Top + 1 > fy)
        {
          fromCard = pos.Card;
          fromCol = pos.Column;
        }
      }

      if (fromCol < -1)
      { return null; }
      if (fromCol == -1)
      { return new Move(-1, -1, -1); }
      if (toCol < -1 || fromCard < 0)
      { return null; }

      return new Move(fromCol, fromCard, toCol);
    }
    private IEnumerable<CardPosition> GetCardPositions()
    {
      foreach (var pos in GetPositions())
      {
        if (pos.Position.Card == null)
        { continue; }
        yield return pos.Position;
      }
    }

    private IEnumerable<Pos> GetPositions()
    {
      for (int iCol = 0; iCol < ColumnCount; iCol++)
      {
        double x = iCol * _dx + _x0;
        int iy = 0;

        List<Card> covereds = ColumnsCovered[iCol];
        foreach (var covered in covereds)
        {
          CardPosition pos = new CardPosition { Card = covered, Visible = false, Left = x, Top = _y0 + iy * _dy };
          yield return new Pos(pos, iCol, -1);
          iy++;
        }

        List<Card> opens = ColumnsOpen[iCol];
        int nOpen = opens.Count;
        int iOpen = 0;
        foreach (var open in opens)
        {
          CardPosition pos = new CardPosition { Card = open, Visible = true, Left = x, Top = _y0 + iy * _dy };
          yield return new Pos(pos, iCol, nOpen - iOpen);
          iOpen++;
          iy++;
        }
        if (nOpen == 0)
        { yield return new Pos(new CardPosition { Left = x, Top = _y0 }, iCol, -1); }
      }

      foreach (var stacks in Stack)
      {
        foreach (var stack in stacks)
        {
          CardPosition pos = new CardPosition { Card = stack, Visible = false, Left = _x0, Top = _yStack };
          yield return new Pos(pos, -1, -1);
        }
      }

      int ix = 2;
      foreach (var comps in Completed)
      {
        double x = _x0 + ix * _dx;
        foreach (var comp in comps)
        {
          CardPosition pos = new CardPosition { Card = comp, Visible = true, Left = x, Top = _yStack };
          yield return new Pos(pos, int.MinValue, -1);
        }
        ix++;
      }

    }

    private List<Card> GetCoveredColumn(int columnIndex)
    {
      if (ColumnsCovered == null)
      {
        ColumnsCovered = new List<List<Card>>();
        for (int i = 0; i < ColumnCount; i++)
        {
          ColumnsCovered.Add(new List<Card>());
        }
      }
      return ColumnsCovered[columnIndex];
    }
    private List<Card> GetOpenColumn(int columnIndex)
    {
      if (ColumnsOpen == null)
      {
        ColumnsOpen = new List<List<Card>>();
        for (int i = 0; i < ColumnCount; i++)
        {
          ColumnsOpen.Add(new List<Card>());
        }
      }
      return ColumnsOpen[columnIndex];
    }
  }
}
