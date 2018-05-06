using System;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class StopHandler : ICostOptimizer
  {
    private readonly Dictionary<IField, int[]> _stopDict;
    private readonly double _minCellCost;

    private IField _startField;
    private IField _maxFullCostField;

    public StopHandler(IList<int[]> stops, double minCellCost)
    {
      _stopDict = new Dictionary<IField, int[]>(new FieldComparer());
      _minCellCost = minCellCost;

      foreach (int[] stop in stops)
      {
        _stopDict[new Field { X = stop[0], Y = stop[1] }] = stop;
      }
    }

    public void Init(IField startField)
    {
      _startField = startField;
      _maxFullCostField = FindMaxField(_startField);
    }

    bool ICostOptimizer.Stop<T>(ICostField processField, SortedList<T, T> costList)
    {
      bool removed = _stopDict.Remove(processField);
      if (_stopDict.Count == 0)
      { return true; }

      if (removed && !_stopDict.ContainsKey(_maxFullCostField))
      {
        _maxFullCostField = FindMaxField(_startField);
        RefreshMinCosts(_maxFullCostField, costList);
      }
      return false;
    }

    bool ICostOptimizer.AdaptCost(ICostField field)
    {
      return SetMinRestCost(field as IRestCostField, _maxFullCostField);
    }

    private IField FindMaxField(IField startField)
    {
      double maxDist = 0;
      IField maxField = null;
      foreach (IField stop in _stopDict.Keys)
      {
        int dx = stop.X - startField.X;
        int dy = stop.Y - startField.Y;
        double dist = dx * dx + dy * dy;
        if (dist > maxDist)
        {
          maxDist = dist;
          maxField = stop;
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
