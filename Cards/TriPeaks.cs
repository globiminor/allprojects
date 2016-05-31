
using Basics.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace Cards
{
  [DataContract]
  public class TriPeaks : GameBase<TriPeaks.Move>
  {
    public class Move : MoveBase
    {
      public Move(int from)
      {
        From = from;
      }
      public int From;

      public override string ToString()
      {
        return string.Format("{0}", From);
      }
    }

    public class TriPeaksStand : Stand<TriPeaks, Move>, IHasInfo
    {
      private int _positionCode;

      public TriPeaksStand(TriPeaks start)
        : base(start)
      {
        Title = start.Title;
      }

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

      public new TriPeaksStand Pre { get { return (TriPeaksStand)base.Pre; } }
      internal TriPeaksStand InitStatistics()
      {
        return this;
      }

      public override int PositionCode
      {
        get
        {
          if (_positionCode == 0)
          {
            TriPeaks stand = GetCurrent(this);
            foreach (Card card in stand.Completed)
            {
              _positionCode = (41 * _positionCode) ^ card.GetCardCode();
            }
          }
          return _positionCode;
        }
      }
      public override double Potential
      {
        get
        {
          return Points + GetCurrent(this).Stack.Count;
        }
      }
      public override bool EqualPosition(Stand<Move> other)
      {
        TriPeaksStand so = other as TriPeaksStand;
        if (so != null)
        {
          if (PositionCode != so.PositionCode)
          { return false; }
        }
        return base.EqualPosition(other);
      }

      public override IEnumerable<Move> GetPossibleMoves()
      {
        int freeCols = 0;
        TriPeaks stand = GetCurrent(this);
        List<Move> moves = new List<Move>();

        if (stand.Stack.Count > 0)
        { moves.Add(new Move(-1)); }

        if (stand.Completed.Count == 0)
        { return moves; }

        Card top = stand.Completed[stand.Completed.Count - 1];

        int index = -1;
        foreach (var pair in stand.Positions)
        {
          index++;
          Position p = pair.Key;
          Card c = pair.Value;
          if (c == null)
          { continue; }
          bool visible = true;
          foreach (Position pre in p.Pres)
          {
            if (stand.Positions[pre] != null)
            { visible = false; }
          }
          if (visible == false)
          { continue; }

          int diff = Math.Abs(c.Height.H - top.Height.H);
          if (diff == 1 || diff == 12)
          { moves.Add(new Move(index)); }
        }

        return moves;
      }

      public override IEnumerable<CardPosition> GetCardPositions()
      {
        TriPeaks start = Game;
        TriPeaks stand = GetCurrent(this);
        return stand.GetCardPositions();
      }

      public override Stand TryMove(double fx, double fy, double tx, double ty)
      {
        TriPeaks start = Game;
        TriPeaks stand = GetCurrent(this);
        Move move = stand.TryGetMove(fx, fy, tx, ty);
        if (move == null)
        { return null; }
        bool success = false;
        foreach (Move possible in GetPossibleMoves())
        {
          if (move.From == possible.From)
          {
            success = true;
            break;
          }
        }
        if (!success)
        { return null; }

        Stand<Move> moved = start.CreateStand();
        moved.SetMove(this, move);
        return moved;
      }

      internal TriPeaks GetCurrent(Stand<Move> stand)
      {
        TriPeaks start = Game;
        TriPeaks cache;

        if (start._cache.TryGetValue(stand, out cache))
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

        TriPeaks pre = GetCurrent(stand.Pre);
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
        foreach (Card card in cards)
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

        TriPeaksStand x = sx._ts;
        TriPeaksStand y = sy._ts;
        if (x == null)
        {
          if (y == null) { return 0; }
          return 1;
        }
        if (y == null) { return -1; }

        int d = x.Points.CompareTo(y.Points);

        if (d != 0)
        { return d; }

        return d;
      }

      private readonly Stand<Move> _stand;
      private readonly TriPeaksStand _ts;
      private readonly Move _lastMove;
      private TriPeaks _triPeaks;

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
        _ts = stand as TriPeaksStand;
        if (_stand.MovesCount == 0)
        { _lastMove = new Move(int.MinValue); }
        else
        { _lastMove = _stand.Move; }
      }
      public Stand<Move> Stand { get { return _stand; } }

      private TriPeaks TriPeaks
      {
        get
        {
          if (_triPeaks == null && _ts != null)
          { _triPeaks = _ts.GetCurrent(_ts); }
          return _triPeaks;
        }
      }

      public override string ToString()
      {
        return _lastMove.ToString();
      }

    }

    [DataContract]
    private class Position
    {
      [Obsolete("Needed for serialization")]
      public Position()
      { }

      public Position(double col, double row)
      {
        Pres = new List<Position>(2);
        Col = col;
        Row = row;
      }

      [DataMember]
      public double Col { get; set; }
      [DataMember]
      public double Row { get; set; }

      public List<Position> Pres { get; set; }

      public override bool Equals(object obj)
      {
        if (obj == this)
        { return true; }

        Position o = obj as Position;
        if (o == null)
        { return false; }

        return Col == o.Col && Row == o.Row;
      }

      public override int GetHashCode()
      {
        return Col.GetHashCode() + 29 * Row.GetHashCode();
      }
    }

    private List<Card> _completed;

    private Dictionary<Position, Card> Positions { get; set; }

    [DataMember]
    private List<CardPosition> RawPositions
    {
      get
      {
        List<CardPosition> posList = new List<CardPosition>();
        foreach (var pair in Positions)
        {
          posList.Add(new CardPosition { Left = pair.Key.Col, Top = pair.Key.Row, Card = pair.Value });
        }
        return posList;
      }
      set
      {
        List<CardPosition> posList = value;
        Dictionary<Position, Position> startDict = new Dictionary<Position, Position>();
        foreach (Position p in StartPositions)
        {
          startDict.Add(p, p);
        }

        Dictionary<Position, Card> positions = new Dictionary<Position, Card>();
        foreach (CardPosition cardPos in posList)
        {
          Position p = new Position(cardPos.Left, cardPos.Top);
          positions.Add(startDict[p], cardPos.Card);
        }

        Positions = positions;
      }
    }

    [DataMember]
    public List<Card> Stack { get; private set; }
    [DataMember]
    public List<Card> Completed
    {
      get
      {
        if (_completed == null) { _completed = new List<Card>(); }
        return _completed;
      }
      private set { _completed = value; }
    }

    public int ColumnCount { get; private set; }
    public string Title { get; private set; }

    private Cache<Stand<Move>, TriPeaks> _cache;

    public TriPeaks()
    {
      InitMembers();
    }

    [OnDeserializing]
    private void OnDeserializing(StreamingContext c)
    {
      InitMembers();
    }
    private void InitMembers()
    {
      _x0 = 0.5;
      _dx = 1.2;
      _y0 = 2;
      _dy = 0.5;
      _yStack = 6;
      _cache = new Cache<Stand<Move>, TriPeaks>();
    }

    public void Init()
    {
      ColumnCount = 10;
      Init(Suite.Suites, (int)DateTime.Now.Ticks);
    }

    public void Init(IEnumerable<Suite> suites, int seed)
    {
      if (System.Diagnostics.Debugger.IsAttached)
      { seed = -1061797517; }

      Title = string.Format("Tri Peaks # {0}", seed);
      List<Card> cards = new List<Card>();

      foreach (Suite suite in suites)
      {
        cards.AddRange(suite.CreateCards());
      }

      Random r = new Random(seed);
      List<Card> shuffled = Card.Shuffle(cards, r);

      Init(shuffled);
    }

    private static List<Position> _startPos;
    private static List<Position> StartPositions
    {
      get
      {
        if (_startPos == null)
        {
          double[,] posArray = new double[,] {
            { 1.5, 0 }, {4.5, 0}, {7.5, 0},
            {1, 1}, {2, 1}, {4, 1}, {5, 1}, {7,1}, {8,1},
            {0.5, 2}, {1.5, 2}, {2.5, 2}, {3.5, 2}, {4.5,2}, {5.5,2}, {6.5,2}, {7.5,2}, {8.5,2},
            {0, 3}, {1, 3}, {2, 3}, {3, 3}, {4,3}, {5,3}, {6,3}, {7,3}, {8,3}, {9,3}
          };
          List<Position> pos = new List<Position>();
          for (int i = 0; i < posArray.GetLength(0); i++)
          {
            pos.Add(new Position(posArray[i, 0], posArray[i, 1]));
          }
          for (int i = 0; i < pos.Count; i++)
          {
            Position pi = pos[i];
            for (int j = i + 1; j < pos.Count; j++)
            {
              Position pj = pos[j];
              if (pi.Row + 1 == pj.Row && Math.Abs(pi.Col - pj.Col) < 1)
              {
                pi.Pres.Add(pj);
              }
            }
          }
          _startPos = pos;
        }
        return _startPos;
      }
    }
    public void Init(List<Card> cards)
    {
      Positions = new Dictionary<Position, Card>();
      Stack = new List<Card>();

      for (int t = 0; t < StartPositions.Count; t++)
      {
        Positions.Add(StartPositions[t], cards[t]);
      }

      for (int t = Positions.Count; t < cards.Count; t++)
      {
        Stack.Add(cards[t]);
      }
    }

    protected override void Sort(List<Stand<Move>> stands)
    {
      List<MoveQuality> mqs = new List<MoveQuality>(stands.Count);
      foreach (Stand<Move> stand in stands)
      { mqs.Add(new MoveQuality(stand)); }
      mqs.Sort(MoveQuality.CompareQuality);
      stands.Clear();
      foreach (MoveQuality mq in mqs)
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

    protected override Stand<TriPeaks.Move> CreateStandCore()
    {
      return new TriPeaksStand(this);
    }

    private TriPeaks CreateNew(Move move)
    {
      TriPeaks clone = Clone();

      if (move.From < 0)
      {
        int iStack = clone.Stack.Count - 1;
        Card stack = clone.Stack[iStack];
        clone.Stack.RemoveAt(iStack);

        clone.Completed.Add(stack);
      }
      else
      {
        Position p = StartPositions[move.From];
        Card card = clone.Positions[p];
        clone.Completed.Add(card);
        clone.Positions[p] = null;
      }

      return clone;
    }

    private TriPeaks Clone()
    {
      TriPeaks clone = new TriPeaks();
      clone.ColumnCount = ColumnCount;
      clone.Stack = new List<Card>(Stack);
      clone.Positions = new Dictionary<Position, Card>(Positions);
      clone.Completed = new List<Card>(Completed);
      return clone;
    }

    private double _x0 = 0.5;
    private double _dx = 1.2;
    private double _y0 = 2;
    private double _dy = 0.5;
    private double _yStack = 6;

    private Move TryGetMove(double fx, double fy, double tx, double ty)
    {
      if (fx != tx || fy != ty)
      { return null; }

      foreach (MyCardPosition pos in GetCardPositions())
      {
        if (pos.Index >= 0 && pos.Visible == false)
        { continue; }
        if (pos.Index < -1)
        { continue; }
        if (pos.Left < fx && pos.Left + 1 > fx && pos.Top < fy && pos.Top + 1 > fy)
        { return new Move(pos.Index); }
      }
      return null;
    }
    private class MyCardPosition : CardPosition
    {
      public int Index { get; set; }
    }
    private IEnumerable<MyCardPosition> GetCardPositions()
    {
      List<Card> covereds = Stack;
      foreach (Card stack in Stack)
      {
        MyCardPosition pos = new MyCardPosition { Index = -1, Card = stack, Visible = false, Left = _x0, Top = _yStack };
        yield return pos;
      }

      int index = -1;
      foreach (var pair in Positions)
      {
        index++;
        Position p = pair.Key;
        Card card = pair.Value;
        if (card == null)
        { continue; }

        bool visible = true;
        foreach (Position pre in p.Pres)
        {
          if (Positions[pre] != null)
          {
            visible = false;
            break;
          }
        }
        MyCardPosition pos = new MyCardPosition
        {
          Index = index,
          Card = card,
          Visible = visible,
          Left = _x0 + p.Col * _dx,
          Top = _y0 + p.Row * _dy
        };
        yield return pos;
      }

      foreach (Card completed in Completed)
      {
        MyCardPosition pos = new MyCardPosition { Index = -2, Card = completed, Visible = true, Left = _x0 + 2, Top = _yStack };
        yield return pos;
      }
    }
  }
}
