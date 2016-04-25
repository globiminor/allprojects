using System.Collections.Generic;
using System.Runtime.Serialization;
namespace Cards
{
  public interface IHasInfo
  {
    string Info { get; }
  }

  [DataContract]
  public abstract class GameBase
  {
    public abstract void Solve();
  }

  public class StandArgs
  {
    public Stand Stand { get; set; }
    public bool Cancel { get; set; }
  }
  public delegate void GameDlg(object sender, StandArgs standArgs);

  [DataContract]
  public abstract class GameBase<T> : GameBase where T : MoveBase
  {
    public abstract IEnumerable<CardPosition> GetCardPositions(IEnumerable<T> moves);
    public abstract IEnumerable<T> GetPossibleNextMoves(IEnumerable<T> moves);

    public event GameDlg Solving;

    public override void Solve()
    {
      Solve(CreateStandCore());
    }

    public void Solve(Stand<T> stand)
    {
      Solver solver = new Solver(this);
      solver.Solve(stand);
    }

    private class StandCache
    {
      private Dictionary<int, List<SolverStand>> _codeStands;
      private Dictionary<int, List<SolverStand>> CodeStands
      {
        get
        {
          if (_codeStands == null)
          { _codeStands = new Dictionary<int, List<SolverStand>>(); }
          return _codeStands;
        }
      }
      public int Count { get { return _codeStands.Count; } }

      public SolverStand GetAndRemoveFirst()
      {
        SolverStand stand = null;
        while (stand == null && CodeStands.Count > 0)
        {
          int key = 0;
          bool remove = false;
          foreach (var pair in CodeStands)
          {
            key = pair.Key;
            List<SolverStand> stands = pair.Value;
            int n = stands.Count;
            if (n > 0)
            {
              stand = stands[n - 1];
              stands.RemoveAt(n - 1);
            }
            if (n < 2)
            { remove = true; }
            break;
          }
          if (remove)
          { CodeStands.Remove(key); }
        }
        return stand;
      }

      public void Add(SolverStand stand)
      {
        List<SolverStand> stands;
        if (!CodeStands.TryGetValue(stand.Stand.PositionCode, out stands))
        {
          stands = new List<SolverStand>();
          CodeStands.Add(stand.Stand.PositionCode, stands);
        }
        stands.Add(stand);
      }

      internal SolverStand FindStand(Stand<T> stand)
      {
        List<SolverStand> existings;
        if (!CodeStands.TryGetValue(stand.PositionCode, out existings))
        { return null; }

        foreach (SolverStand existing in existings)
        {
          Stand<T> existPos = existing.Stand;
          if (stand.EqualPosition(existPos))
          { return existing; }
        }
        return null;
      }
    }

    private class SolverStand
    {
      private readonly Stand<T> _stand;
      internal bool Completed { get; set; }
      internal bool Success { get; set; }
      internal bool Retry { get; set; }

      private Dictionary<SolverStand, SolverStand> _children;

      public override string ToString()
      {
        return _stand.ToString();
      }
      public SolverStand(Stand<T> stand)
      {
        _stand = stand;
      }
      public Stand<T> Stand { get { return _stand; } }

      public double Potential { get { return _stand.Potential; } }

      public Dictionary<SolverStand, SolverStand> GetPossibleStands(
        GameBase<T> parent, StandCache existings)
      {
        if (_children != null)
        {
          Dictionary<SolverStand, SolverStand> stands =
            new Dictionary<SolverStand, SolverStand>(_children.Count);
          foreach (var pair in _children)
          {
            if (pair.Key._stand.MovesCount > _stand.MovesCount)
            { stands.Add(pair.Key, pair.Value); }
          }
          return stands;
        }

        IEnumerable<T> moves = _stand.GetPossibleMoves();
        if (moves == null)
        {
          Success = true;
          return null;
        }

        Dictionary<SolverStand, SolverStand> nexts =
          new Dictionary<SolverStand, SolverStand>();
        foreach (T move in moves)
        {
          Stand<T> nextPos = parent.CreateStand();
          nextPos.SetMove(_stand, move);
          SolverStand next = new SolverStand(nextPos);

          SolverStand exists = existings.FindStand(nextPos);
          if (exists != null)
          {
            if (exists.Stand.MovesCount > nextPos.MovesCount)
            {
              exists.ReplaceMoves(nextPos);
              next.Completed = exists.Completed;
              next.Retry = true;
            }
          }

          if (exists == null)
          { nexts.Add(next, null); }
          else if (next.Retry)
          { nexts.Add(next, exists); }
        }

        foreach (var pair in nexts)
        {
          if (pair.Value == null)
          { existings.Add(pair.Key); }
        }

        return nexts;
      }

      private void ReplaceMoves(Stand<T> nextPos)
      {
        _stand.ReplaceMoves(nextPos);
        if (_children == null)
        { return; }

        int movesCount = nextPos.MovesCount + 1;
        List<SolverStand> removes = new List<SolverStand>(_children.Keys.Count);
        foreach (SolverStand child in _children.Keys)
        {
          if (child.Completed ||
            child._stand.Pre != _stand)
          {
            removes.Add(child);
            continue;
          }

          if (child._stand.MovesCount > movesCount)
          {
            child._stand.SetMove(nextPos, child._stand.Move);
            child.ReplaceMoves(child._stand);
          }
          else
          { removes.Add(child); }
        }

        foreach (SolverStand remove in removes)
        { _children.Remove(remove); }
      }

      public void AddIncompleted(SolverStand child)
      {
        child.Completed = false;
        if (_children == null)
        { _children = new Dictionary<SolverStand, SolverStand>(); }

        if (!_children.ContainsKey(child))
        { _children.Add(child, child); }
      }

      public void SetCompleted(SolverStand child)
      {
        child.Completed = true;
        if (_children != null)
        {
          _children.Remove(child);
          if (_children.Count == 0)
          { Completed = true; }
        }
      }
    }

    private enum Status { Success, Incompleted, Completed, Cancel };
    private class Solver
    {
      private readonly GameBase<T> _parent;
      private readonly StandCache _existing;
      private readonly SortedDictionary<double, StandCache> _potential;

      public Solver(GameBase<T> parent)
      {
        _parent = parent;
        _existing = new StandCache();
        _potential = new SortedDictionary<double, StandCache>();

        MaxMoves = 300;
      }
      public int MaxMoves { get; set; }

      public void Solve(Stand<T> start)
      {
        SolvePotential(start);
      }

      public void SolvePotential(Stand<T> start)
      {
        SolverStand ss = new SolverStand(start);
        _existing.Add(ss);
        StandCache potCache = new StandCache();
        potCache.Add(ss);
        _potential.Add(-ss.Potential, potCache);

        while (_potential.Count > 0)
        {
          List<double> removes = new List<double>();
          SolverStand stand = null;

          foreach (var pair in _potential)
          {
            double key = pair.Key;
            StandCache cache = pair.Value;
            do
            {
              stand = cache.GetAndRemoveFirst();
            } while (stand.Completed && cache.Count > 0);
            if (cache.Count <= 0)
            { removes.Add(key); }
            if (stand != null)
            { break; }
          }
          foreach (double key in removes)
          { _potential.Remove(key); }

          StandArgs args = new StandArgs { Stand = stand.Stand };
          _parent.OnSolving(args);
          if (args.Cancel)
          { return; }

          Dictionary<SolverStand, SolverStand> newStands = stand.GetPossibleStands(_parent, _existing);

          List<Stand<T>> sorteds = new List<Stand<T>>(newStands.Count);
          Dictionary<Stand<T>, SolverStand> nextDict =
            new Dictionary<Stand<T>, SolverStand>(newStands.Count);
          foreach (SolverStand newStand in newStands.Keys)
          {
            sorteds.Add(newStand.Stand);
            nextDict.Add(newStand.Stand, newStand);
          }
          _parent.Sort(sorteds);


          foreach (Stand<T> sorted in sorteds)
          {
            SolverStand newStand = nextDict[sorted];
            SolverStand preStand = newStands[newStand];
            if (preStand != null)
            { preStand.Completed = true; }
            else
            { _existing.Add(newStand); }

            double pot = -newStand.Potential;
            StandCache cache;
            if (!_potential.TryGetValue(pot, out cache))
            {
              cache = new StandCache();
              _potential.Add(pot, cache);
            }
            cache.Add(newStand);
          }
        }

      }
      public void SolveTree(Stand<T> start)
      {
        SolverStand ss = new SolverStand(start);
        _existing.Add(ss);
        SolveTree(ss);
      }

      private Status SolveTree(SolverStand current)
      {
        if (!current.Retry)
        {
          StandArgs args = new StandArgs { Stand = current.Stand };
          _parent.OnSolving(args);
          if (args.Cancel)
          {
            return Status.Cancel;
          }
        }

        Dictionary<SolverStand, SolverStand> nexts =
          current.GetPossibleStands(_parent, _existing);
        if (nexts == null)
        {
          return Status.Success;
        }

        if (nexts.Count == 0)
        { return Status.Completed; }

        List<Stand<T>> sorted = new List<Stand<T>>(nexts.Count);
        Dictionary<Stand<T>, SolverStand> nextDict =
          new Dictionary<Stand<T>, SolverStand>(nexts.Count);
        foreach (SolverStand stand in nexts.Keys)
        {
          sorted.Add(stand.Stand);
          nextDict.Add(stand.Stand, stand);
        }
        _parent.Sort(sorted);

        Status baseStatus = Status.Completed;
        foreach (Stand<T> nextPos in sorted)
        {
          SolverStand next = nextDict[nextPos];
          if (nextPos.MovesCount > MaxMoves)
          {
            current.AddIncompleted(next);
            baseStatus = Status.Incompleted;
            continue;
          }
          if (next.Completed)
          { continue; }

          Status status = SolveTree(next);
          if (status == Status.Success)
          {
            baseStatus = Status.Success;
            return baseStatus;
          }
          else if (status == Status.Incompleted)
          {
            current.AddIncompleted(next);
            baseStatus = Status.Incompleted;
          }
          else
          {
            current.SetCompleted(next);
            SolverStand exists = nexts[next];
            if (exists != null) { exists.Completed = true; }
          }
        }
        return baseStatus;
      }
    }
    protected void OnSolving(StandArgs standArgs)
    {
      if (Solving == null)
      { return; }

      Solving(this, standArgs);
    }

    protected virtual void Sort(List<Stand<T>> stands)
    {
      return;
    }

    public Stand<T> CreateStand()
    {
      return CreateStandCore();
    }
    protected abstract Stand<T> CreateStandCore();
  }
}
