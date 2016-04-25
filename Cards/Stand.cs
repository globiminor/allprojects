using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cards
{
  public abstract class Stand
  {
    public event GameDlg Solving;

    public abstract IEnumerable<CardPosition> GetCardPositions();
    public abstract Stand TryMove(double fx, double fy, double tx, double ty);
    public abstract Stand Revert();
    public abstract void Solve();
    public string Title { get; set; }
    public int MovesCount { get; protected set; }

    public int Points { get; protected set; }

    protected void OnSolving(StandArgs stand)
    {
      if (Solving == null)
      { return; }

      Solving(this, stand);
    }
  }

  public abstract class Stand<M> : Stand where M : MoveBase
  {
    private Stand<M> _pre;
    private M _move;

    protected Stand()
    { }
    // public List<M> Moves { get { return _moves; } }

    public Stand<M> Pre { get { return _pre; } }
    public M Move { get { return _move; } }
    public abstract int PositionCode { get; }
    public abstract double Potential { get; }

    public abstract IEnumerable<M> GetPossibleMoves();
    public void ReplaceMoves(Stand<M> stand)
    {
      _pre = stand._pre;
      _move = stand._move;
      MovesCount = stand.MovesCount;
    }

    public override string ToString()
    {
      if (Move == null)
      { return "Initial"; }
      return Move.ToString();
    }
    public void SetMove(Stand<M> pre, M move)
    {
      _pre = pre;
      _move = move;
      if (pre != null)
      { MovesCount = pre.MovesCount + 1; }
      else
      { MovesCount = 1; }
    }

    public virtual bool EqualPosition(Stand<M> other)
    {
      using (IEnumerator<CardPosition> thisPos = GetCardPositions().GetEnumerator())
      using (IEnumerator<CardPosition> otherPos = other.GetCardPositions().GetEnumerator())
      {
        while (thisPos.MoveNext())
        {
          if (!otherPos.MoveNext())
          {
            return false;
          }
          CardPosition thisCard = thisPos.Current;
          CardPosition otherCard = otherPos.Current;
          if (!thisCard.EqualsCardAndPos(otherCard))
          {
            return false;
          }
        }
        if (otherPos.MoveNext())
        {
          return false;
        }
      }
      return true;
    }

  }

  public abstract class Stand<G, M> : Stand<M>
    where G : GameBase<M>
    where M : MoveBase
  {
    private readonly G _game;
    public Stand(G game)
    {
      _game = game;
    }

    public override void Solve()
    {
      Game.Solving -= Game_Solving;
      Game.Solving += Game_Solving;
      Game.Solve(this);
      Game.Solving -= Game_Solving;
    }

    private void Game_Solving(object sender, StandArgs stand)
    {
      OnSolving(stand);
    }

    public G Game
    {
      get { return _game; }
    }
  }
}
