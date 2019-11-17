using System.Collections.Generic;
using System.Linq;

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

    private Dictionary<IField, List<StopInfo>> _startFields;
    private StopInfo _maxFullCostField;

    public StopHandler(IList<IField> stops, double minCellCost)
    {
      _stopDict = new Dictionary<IField, StopInfo>(new FieldComparer());
      _minCellCost = minCellCost;

      foreach (var stop in stops)
      {
        _stopDict[stop] = new StopInfo { Stop = stop, Cost = -1 };
      }
      _startFields = new Dictionary<IField, List<StopInfo>>();
    }

    public double StopFactor { get; set; } = 1;
    public System.Func<IField, IField, int> StopComparer { get; set; }
    public System.Func<IEnumerable<IField>, bool> HandleUncompleted { get; set; }

    public void Init<T>(IField startField, SortedList<T, T> costList)
    {
      _startFields.Add(startField, GetSortedStops(startField));

      StopInfo maxFullCostField = GetMaxFullCostField();
      if (maxFullCostField != _maxFullCostField)
      {
        _maxFullCostField = maxFullCostField;
        RefreshMinCosts(_maxFullCostField.Stop, costList);
      }
    }

    public bool IsEndsCompleted()
    {
      return HandleUncompleted?.Invoke(_stopDict.Values.Where(x => x.Handled == false).Select(x => x.Stop)) ?? false;
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

      _maxFullCostField = GetMaxFullCostField();
      RefreshMinCosts(_maxFullCostField.Stop, costList);
      return false;
    }

    bool ICostOptimizer.AdaptCost(ICostField field)
    {
      return SetMinRestCost(field as IRestCostField, _maxFullCostField.Stop);
    }

    private StopInfo GetMaxFullCostField()
    {
      StopInfo maxFullCostField = null;
      double maxDist = -1;
      foreach (var pair in _startFields)
      {
        List<StopInfo> sortedStops = pair.Value;
        foreach (StopInfo stop in sortedStops)
        {
          if (stop.Handled)
          { continue; }

          double dist = FieldOp.GetDist2(stop.Stop, pair.Key);
          if (dist > maxDist)
          {
            maxDist = dist;
            maxFullCostField = stop;
          }
          break;
        }
      }
      return maxFullCostField;
    }
    private List<StopInfo> GetSortedStops(IField startField)
    {
      List<StopInfo> stops = new List<StopInfo>(_stopDict.Values);
      stops.Sort((x, y) =>
      {
        int d = StopComparer?.Invoke(x.Stop, y.Stop) ?? 0;
        if (d != 0)
        { return d; }

        double distX = FieldOp.GetDist2(x.Stop, startField);
        double distY = FieldOp.GetDist2(y.Stop, startField);

        return -distX.CompareTo(distY);
      });
      return stops;
    }

    private void RefreshMinCosts<T>(IField maxField, SortedList<T, T> costList)
    {
      if (costList != null)
      {
        List<T> toUpdate = new List<T>(costList.Values);

        costList.Clear();
        foreach (var update in toUpdate)
        {
          SetMinRestCost(update as IRestCostField, maxField);
          costList.Add(update, update);
        }
      }
    }


    private bool SetMinRestCost(IRestCostField field, IField stop)
    {
      return RestCostFieldUtils.SetMinRestCost(field, stop, _minCellCost);
    }

    public IComparer<ICostField> GetCostComparer()
    {
      return new MinFullCostComparer();
    }
  }
}
