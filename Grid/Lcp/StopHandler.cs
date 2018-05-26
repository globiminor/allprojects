using System;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class StopHandler : ICostOptimizer
  {
    private class StopInfo
    {
      public IField Stop { get; set; }
      public double Cost { get; set; }
      public bool Handled { get; set; }
    }
    private readonly Dictionary<IField, StopInfo> _stopDict;
    private readonly double _minCellCost;

    private IField _startField;
    private StopInfo _maxFullCostField;

    public StopHandler(IList<IField> stops, double minCellCost)
    {
      _stopDict = new Dictionary<IField, StopInfo>(new FieldComparer());
      _minCellCost = minCellCost;

      foreach (IField stop in stops)
      {
        _stopDict[stop] = new StopInfo { Stop = stop, Cost = -1 };
      }
    }

    public double StopFactor { get; set; } = 1;

    public void Init(IField startField)
    {
      _startField = startField;
      _maxFullCostField = FindMaxField(_startField);
    }

    bool ICostOptimizer.Stop<T>(ICostField processField, SortedList<T, T> costList)
    {
      if (_stopDict.TryGetValue(processField, out StopInfo stopInfo))
      {
        stopInfo.Cost = processField.Cost;
      }

      if (_maxFullCostField.Cost < 0)
      { return false; }

      if (processField is IRestCostField restCostField)
      {
        if (restCostField.MinRestCost + processField.Cost < _maxFullCostField.Cost * StopFactor)
        { return false; }
        _maxFullCostField.Handled = true;
      }
      else
      { _maxFullCostField.Handled = true; }

      if (!_maxFullCostField.Handled)
      { return false; }

      bool removed = _stopDict.Remove(_maxFullCostField.Stop);
      if (_stopDict.Count == 0)
      { return true; }

      _maxFullCostField = FindMaxField(_startField);
      RefreshMinCosts(_maxFullCostField.Stop, costList);
      return false;
    }

    bool ICostOptimizer.AdaptCost(ICostField field)
    {
      return SetMinRestCost(field as IRestCostField, _maxFullCostField.Stop);
    }

    private StopInfo FindMaxField(IField startField)
    {
      double maxDist = 0;
      StopInfo maxField = null;
      foreach (var pair in _stopDict)
      {
        StopInfo stopInfo = pair.Value;
        if (stopInfo.Handled)
        { continue; }

        IField stop = stopInfo.Stop;
        int dx = stop.X - startField.X;
        int dy = stop.Y - startField.Y;
        double dist = dx * dx + dy * dy;
        if (dist > maxDist)
        {
          maxDist = dist;
          maxField = stopInfo;
        }
      }

      return maxField;
    }

    private void RefreshMinCosts<T>(IField maxField, SortedList<T, T> costList)
    {
      if (costList != null)
      {
        List<T> toUpdate = new List<T>(costList.Values);

        costList.Clear();
        foreach (T update in toUpdate)
        {
          SetMinRestCost(update as IRestCostField, maxField);
          costList.Add(update, update);
        }
      }
    }


    private bool SetMinRestCost(IRestCostField field, IField stop)
    {
      if (field == null || stop == null)
      { return false; }

      double dx = stop.X - field.X;
      double dy = stop.Y - field.Y;
      double cellDist = Math.Sqrt(dx * dx + dy * dy);

      field.MinRestCost = _minCellCost * cellDist;
      return true;
    }

    public IComparer<ICostField> GetCostComparer()
    {
      return new MinFullCostComparer();
    }
  }
}
