
using Basics.Geom;
using System.Collections.Generic;

namespace Grid.Lcp
{
  public class LeastCostStack : LeastCostBase
  {
    public static LeastCostStack<T, TT> Create<T, TT>(IList<IDirCostModel<T>> dirCostModels, IList<Teleport<TT>> teleports, double dx,
      IBox userBox = null, Steps steps = null)
      where TT : class, IDirCostModel<T>
      where T : class
    {
      return new LeastCostStack<T, TT>(dirCostModels, teleports, dx, userBox, steps);
    }
  }

  public class LeastCostStack<T, TT> : LeastCostStack, ILcpModel
  where T : class
  where TT : class, IDirCostModel<T>
  {
    private readonly IList<IDirCostModel<T>> _dirCostModels;
    private readonly IList<Teleport<TT>> _teleports;
    private readonly double _dx;
    private readonly IBox _userBox;
    private readonly Steps _steps;

    private readonly List<TelePoint> _starts;
    private readonly List<TelePoint> _ends;

    public LeastCostStack(IList<IDirCostModel<T>> dirCostModels, IList<Teleport<TT>> teleports, double dx,
      IBox userBox = null, Steps steps = null)
    {
      _dirCostModels = dirCostModels;
      _teleports = teleports;
      _dx = dx;
      _userBox = userBox;
      _steps = steps ?? Steps.Step16;

      _starts = new List<TelePoint>();
      _ends = new List<TelePoint>();
    }

    ILeastCostData ILcpModel.CalcCost(IPoint start, IPoint end)
    {
      AddStart(start, _dirCostModels[0]);
      AddEnd(end, _dirCostModels[0]);

      CalcCost();

      LeastCostStackData costData = GetCostData();
      return costData;
    }
    public void AddStart(IPoint p, IDirCostModel<T> costModel)
    {
      _starts.Add(new TelePoint(p, costModel));
    }
    public void AddEnd(IPoint p, IDirCostModel<T> costModel)
    {
      _ends.Add(new TelePoint(p, costModel));
    }

    private class TelePoint
    {
      public TelePoint(IPoint p, IDirCostModel<T> costModel)
      {
        Point = p;
        CostModel = costModel;
      }

      public IPoint Point { get; set; }
      public IDirCostModel<T> CostModel { get; set; }
    }
    private class Layer
    {
      public LeastCostGrid<T> Lcg { get; set; }
      public Calc Calc { get; set; }
      public Dictionary<IField, IList<Teleport<TT>>> Teleports { get; set; }
    }
    private Dictionary<IDirCostModel<T>, Layer> LayerDict => _layerDict ?? (_layerDict = new Dictionary<IDirCostModel<T>, Layer>());
    private Dictionary<IDirCostModel<T>, Layer> _layerDict;
    private Dictionary<IDirCostModel<T>, Layer> CalcLayerDict => _calcLayerDict ?? (_calcLayerDict = new Dictionary<IDirCostModel<T>, Layer>());
    private Dictionary<IDirCostModel<T>, Layer> _calcLayerDict;

    public object CalcCost(bool invers = false, double stopFactor = 1)
    {
      foreach (var pt in _starts)
      {
        IDirCostModel<T> costModel = pt.CostModel;
        Layer stack = GetCalcLayer(costModel, invers, stopFactor);
        stack.Calc.AddStart(pt.Point);
      }
      if (_ends != null)
      {
        foreach (var pt in _ends)
        {
          IDirCostModel<T> costModel = pt.CostModel;
          Layer stack = GetCalcLayer(costModel, invers, stopFactor);
        }
      }

      int i = 0;
      bool completed = false;
      while (!completed)
      {
        completed = true;

        ICostField minCostField = null;
        Layer minLayer = null;
        IDirCostModel<T> minModel = null;

        bool endsCompleted = (_ends?.Count > 0);
        foreach (var pair in CalcLayerDict)
        {
          Layer layer = pair.Value;
          ICostField costField = layer.Calc.InitNextProcessField(remove: false);
          if (costField != null && (minCostField == null || costField.Cost < minCostField.Cost))
          {
            minCostField = costField;
            minLayer = layer;
            minModel = pair.Key;
          }
          if (endsCompleted && !((layer.Calc.CostOptimizer as StopHandler)?.IsEndsCompleted() ?? false))
          {
            endsCompleted = false;
          }
        }
        if (endsCompleted)
        {
          continue;
        }

        if (minCostField != null)
        {
          completed = false;
          ICostField processField = minLayer.Calc.InitNextProcessField(remove: true);
          bool processed = minLayer.Calc.Process(processField, ref i);

          if (!processed)
          { CalcLayerDict.Remove(minModel); }

          // calc teleport
          if (minLayer.Teleports.TryGetValue(processField, out IList<Teleport<TT>> teleports))
          {
            foreach (var teleport in teleports)
            {
              Layer layer = GetCalcLayer(teleport.ToData, invers, stopFactor);
              layer.Calc.AddStart(teleport.To, processField.Cost + teleport.Cost);
            }
          }
        }
      }

      bool export = false;
      if (export)
      {
        int it = 0;
        foreach (var layer in LayerDict.Values)
        {
          IGrid<double> costGrid = layer.Calc.CostGrid;
          IGrid<double> reduced = BaseGrid.TryReduce(costGrid);
          ImageGrid.GridToImage(reduced.ToInt(), $"C:\\temp\\test_{it}.tif");
          it++;
        }
      }
      return null;
      // Export to image:
    }

    private Layer GetCalcLayer(IDirCostModel<T> costModel, bool invers, double stopFactor)
    {
      if (!CalcLayerDict.TryGetValue(costModel, out Layer layer))
      {
        layer = GetLayer(costModel, invers, stopFactor);
        CalcLayerDict.Add(costModel, layer);
      }
      return layer;
    }

    private Layer GetLayer(IDirCostModel<T> costModel, bool invers, double stopFactor)
    {
      if (!LayerDict.TryGetValue(costModel, out Layer layer))
      {
        LeastCostGrid<T> lcg = new LeastCostGrid<T>(costModel, _dx, _userBox, _steps);
        StopHandler stopHandler = null;
        if (_ends != null)
        {
          Dictionary<IField, int> stopDict = new Dictionary<IField, int>();
          List<IField> stops = new List<IField>();
          foreach (var telePoint in _ends)
          {
            if (telePoint.CostModel != costModel)
            { continue; }
            Field stopField = lcg.GetField(telePoint.Point);
            stops.Add(stopField);
            stopDict.Add(stopField, 0);
          }
          foreach (var telePort in _teleports)
          {
            IPoint port;
            if (!invers && telePort.FromData == costModel)
            { port = telePort.From; }
            else if (invers && telePort.ToData == costModel)
            { port = telePort.To; }
            else
            { continue; }
            Field portField = lcg.GetField(port);
            stops.Add(portField);
            stopDict.Add(portField, 1);
          }

          stopHandler = new StopHandler(stops, costModel.MinUnitCost * _dx, 5 * _dx, (int)(0.7 / _dx))
          {
            StopFactor = stopFactor,
            StopComparer = (x, y) =>
            {
              int ix = stopDict[x];
              int iy = stopDict[y];
              return ix.CompareTo(iy);
            },
            HandleUncompleted = (fields) =>
            {
              foreach (var field in fields)
              {
                if (stopDict[field] == 0)
                { return false; }
              }
              return true;
            }
          };
        }

        Calc calc = new Calc(lcg, invers, 
          costOptimizer: stopHandler, maxCostHandler: stopHandler);
        Dictionary<IField, IList<Teleport<TT>>> layerPorts =
          new Dictionary<IField, IList<Teleport<TT>>>(new FieldComparer());

        foreach (var teleport in _teleports)
        {
          Field port;
          if (!invers && teleport.FromData == costModel)
          { port = lcg.GetField(teleport.From); }
          else if (invers && teleport.ToData == costModel)
          { port = lcg.GetField(teleport.To); }
          else
          { continue; }

          if (!layerPorts.TryGetValue(port, out IList<Teleport<TT>> teleports))
          {
            teleports = new List<Teleport<TT>>();
            layerPorts.Add(port, teleports);
          }
          teleports.Add(teleport);
        }

        layer = new Layer { Lcg = lcg, Calc = calc, Teleports = layerPorts };
        lcg.Status += (s, e) => { OnStatus(e, s); };
        LayerDict.Add(costModel, layer);
      }
      return layer;
    }

    private LeastCostStackData GetCostData()
    {
      Dictionary<Layer, LeastCostData> layerDict = new Dictionary<Layer, LeastCostData>();
      Layer mainLayer = LayerDict[_dirCostModels[0]];
      layerDict.Add(mainLayer, new LeastCostData(mainLayer.Calc.CostGrid, mainLayer.Calc.DirGrid, _steps));

      Dictionary<LeastCostData, List<Teleport<LeastCostData>>> lcdTeleportsDict =
        new Dictionary<LeastCostData, List<Teleport<LeastCostData>>>();

      bool addedAny = true;
      while (addedAny)
      {
        addedAny = false;
        foreach (Layer layer in LayerDict.Values)
        {
          if (!layerDict.TryGetValue(layer, out LeastCostData lcd))
          { continue; }

          if (lcdTeleportsDict.ContainsKey(lcd))
          { continue; }

          List<Teleport<LeastCostData>> lcdTeleports = null;

          foreach (var pair in layer.Teleports)
          {
            IList<Teleport<TT>> teleports = pair.Value;
            foreach (var teleport in teleports)
            {
              IDirCostModel<T> toCostModel = teleport.ToData;
              if (!LayerDict.TryGetValue(toCostModel, out Layer toLayer))
              { continue; }

              if (!layerDict.TryGetValue(toLayer, out LeastCostData toLcd))
              {
                toLcd = new LeastCostData(toLayer.Calc.CostGrid, toLayer.Calc.DirGrid, _steps);
                layerDict.Add(toLayer, toLcd);
                addedAny = true;
              }

              if (lcdTeleports == null)
              {
                lcdTeleports = new List<Teleport<LeastCostData>>();
                lcdTeleportsDict.Add(lcd, lcdTeleports);
              }
              lcdTeleports.Add(Teleport.Create(teleport.From, lcd, teleport.To, toLcd, teleport.Cost));
            }
          }
        }
      }

      LeastCostStackData data = new LeastCostStackData(new List<LeastCostData>(layerDict.Values), lcdTeleportsDict);
      return data;
    }
  }

}
