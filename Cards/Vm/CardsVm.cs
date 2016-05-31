using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
namespace Cards.Vm
{
  public interface ICardsView
  {
    CardsVm Vm { get; set; }
    void Clear();

    void RefreshCards();
  }

  public class CardsVm : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly ICardsView _view;
    private GameBase _game;
    private Stand _cards;
    private Stand _solve;

    public CardsVm(ICardsView view)
    {
      _view = view;
      _view.Vm = this;
    }

    public Stand Cards
    {
      get { return _cards; }
      set { _cards = value; OnPropertyChanged(null); }
    }

    public string Title { get { return Cards.Title; } }
    public int Moves
    {
      get
      {
        if (_solve != null) { return _solve.MovesCount; }
        return Cards.MovesCount;
      }
    }

    public int Points
    {
      get
      {
        if (_solve != null) { return _solve.Points; }
        return Cards.Points;
      }
    }

    public string Info
    {
      get
      {
        IHasInfo hasInfo = _solve as IHasInfo;
        if (hasInfo != null)
        { return hasInfo.Info; }
        else
        { return null; }
      }
    }
    private void OnPropertyChanged(string member)
    {
      if (PropertyChanged == null)
      { return; }

      PropertyChanged(this, new PropertyChangedEventArgs(member));
    }

    private Thread _solveThread;
    private SolveState _solveState;
    private enum SolveState { Idle, Solving, Cancelling }

    public void SetSpider4()
    {
      Spider s = new Spider();
      s.Init();
      SetGame(s);
    }

    public void SetTriPeaks()
    {
      TriPeaks t = new TriPeaks();
      t.Init();
      SetGame(t);
    }

    private void SetGame<T>(GameBase<T> game) where T : MoveBase
    {
      _game = game;
      Cards = game.CreateStand();
    }

    public void Save(string fileName)
    {
      if (_game == null)
      { return; }

      DataContractSerializer ser = new DataContractSerializer(_game.GetType());
      using (Stream writer = new FileStream(fileName, FileMode.Create))
      {
        ser.WriteObject(writer, _game);
      }

    }

    public void Load(string fileName)
    {
      if (_game == null)
      { return; }

      DataContractSerializer ser = new DataContractSerializer(_game.GetType());
      using (Stream reader = new FileStream(fileName, FileMode.Open))
      {
        GameBase game = (GameBase)ser.ReadObject(reader);

        if (game is TriPeaks)
        {
          SetGame((TriPeaks)game);
        }
        else
        {
          throw new NotImplementedException(string.Format( "unhandled type {0}", game.GetType()));
        }
      }
    }

    public void ToggleSolve()
    {
      if (Cards == null)
      { return; }

      if (_solveState == SolveState.Idle)
      {
        _solveState = SolveState.Solving;
        Cards.Solving -= Cards_Solving;
        Cards.Solving += Cards_Solving;
        OnPropertyChanged(null);

        if (System.Diagnostics.Debugger.IsAttached)
        {
          Cards.Solve();
          _solveState = SolveState.Idle;
          Cards = _solve;
          _solve = null;
          OnPropertyChanged(null);
        }
        else
        {
          _solveThread = new Thread(Cards.Solve);
          _solveThread.Start();
        }
      }
      else if (_solveState == SolveState.Solving)
      {
        _solveState = SolveState.Cancelling;
        OnPropertyChanged(null);
        if (_solveThread != null)
        {
          _solveThread.Join();
          _solveThread = null;
          _solveState = SolveState.Idle;
          OnPropertyChanged(null);
        }
        if (_solve != null)
        {
          Cards = _solve;
          _solve = null;
        }
      }
      else if (_solveState == SolveState.Cancelling)
      { return; }
      else
      { throw new NotImplementedException("Unhandled solve state " + _solveState); }
    }

    public string SolveText
    {
      get
      {
        if (_solveState == SolveState.Idle)
        { return "Solve"; }
        else if (_solveState == SolveState.Solving)
        { return "Stop"; }
        else if (_solveState == SolveState.Cancelling)
        { return "Stopping..."; }
        else
        { throw new NotImplementedException("Unhandled solve state " + _solveState); }
      }
    }

    public double DisplaySecs
    {
      get
      {
        double secs = double.Parse(string.Format("{0:e2}", _delta.TotalSeconds));
        return secs;
      }
      set
      {
        double secs = value;
        _delta = new TimeSpan((long)(TimeSpan.TicksPerSecond * secs));
        OnPropertyChanged(null);
      }
    }
    private DateTime _lastStand;
    private TimeSpan _delta = new TimeSpan(0, 0, 0, 1, 0);

    private void Cards_Solving(object sender, StandArgs stand)
    {
      _solve = stand.Stand;
      if (_solveState != SolveState.Solving)
      { stand.Cancel = true; }

      if (_view != null)
      {
        DateTime newStand = DateTime.Now;
        while (newStand - _lastStand < _delta)
        {
          double sleep = Math.Min(100, _delta.TotalMilliseconds);
          Thread.Sleep((int)sleep);
          newStand = DateTime.Now;
        }
        OnPropertyChanged(null);
        _lastStand = newStand;
        _view.RefreshCards();
      }
    }

    public IEnumerable<CardPosition> GetCardPositions()
    {
      Stand stand = _solve;
      if (_solve == null)
      { stand = Cards; }

      if (stand == null)
      { yield break; }

      foreach (CardPosition card in stand.GetCardPositions())
      { yield return card; }
    }

    public bool Move(double fx, double fy, double tx, double ty)
    {
      if (Cards == null)
      { return false; }

      Stand move = Cards.TryMove(fx, fy, tx, ty);
      if (move != null)
      {
        Cards = move;
        return true;
      }
      return false;
    }

    public bool Revert()
    {
      if (Cards == null)
      { return false; }

      Stand reverted = Cards.Revert();
      if (reverted != null)
      {
        Cards = reverted;
        return true;
      }
      return false;
    }
    public void InitView()
    {
      _view.Clear();
    }
  }
}
