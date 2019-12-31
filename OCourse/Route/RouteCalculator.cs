
using Basics.Geom;
using Grid;
using Grid.Lcp;
using LeastCostPathUI;
using Ocad;
using OCourse.Ext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCourse.Route
{
  public class RouteCalculator
  {
    public StatusEventHandler StatusChanged;
    public event VariationBuilder.VariationEventHandler VariationAdded;

    private readonly string _veloPath;
    private readonly IGrid<double> _heightGrid;
    private readonly Steps _steps;
    private readonly ITvmCalc _tvmCalc;

    private readonly Dictionary<CostFromTo, CostFromTo> _calcList;

    public RouteCalculator(IGrid<double> heightGrid, string veloPath, Steps steps, ITvmCalc tvmCalc)
    {
      _veloPath = veloPath;
      _heightGrid = heightGrid;
      _steps = steps;
      _tvmCalc = tvmCalc ?? new TvmCalc();

      _calcList = new Dictionary<CostFromTo, CostFromTo>(new CostFromTo.GeometryComparer());
    }

    public string VeloPath => _veloPath;
    public IGrid<double> HeightGrid => _heightGrid;
    public Steps Steps => _steps;
    public ITvmCalc TvmCalc => _tvmCalc;


    private IGrid<double> _veloGrid;
    private IGrid<double> VeloGrid => _veloGrid ?? (System.IO.Path.GetExtension(VeloPath).Equals(".xml", StringComparison.InvariantCultureIgnoreCase)
      ? null
      : (_veloGrid = VelocityGrid.FromImage(VeloPath)));

    private ViewModels.SymbolVeloModel<TvmCell> _symVeloModel;

    internal Dictionary<CostFromTo, CostFromTo> RouteCostDict
    {
      get { return _calcList; }
    }
    public CostFromTo GetCost(string from, string to)
    {
      var pair = _calcList.FirstOrDefault(p => p.Key.From.Name == from && p.Key.To.Name == to);
      return pair.Value;
    }

    public void CalcEvent(string courseFile, double resolution)
    {
      InitRawCourses(courseFile);
      List<CostFromTo> addList = GetRoutes(resolution);

      foreach (var info in addList)
      {
        if (!RouteCostDict.ContainsKey(info))
        {
          RouteCostDict.Add(info, info);
        }
      }
    }

    public void ScriptEvent(string courseFile, string heightGrid, string veloGrid, double resolution, string resultBat)
    {
      InitRawCourses(courseFile);

      using (System.IO.TextWriter w = new System.IO.StreamWriter(resultBat))
      {
        SortedDictionary<IPoint, List<CostFromTo>> starts = new SortedDictionary<IPoint, List<CostFromTo>>(new PointComparer());
        SortedDictionary<IPoint, List<CostFromTo>> ends = new SortedDictionary<IPoint, List<CostFromTo>>(new PointComparer());
        foreach (var info in _calcList.Keys)
        {
          if (starts.TryGetValue(info.Start, out List<CostFromTo> startList) == false)
          {
            startList = new List<CostFromTo>();
            starts.Add(info.Start, startList);
          }
          startList.Add(info);

          if (ends.TryGetValue(info.End, out List<CostFromTo> endList) == false)
          {
            endList = new List<CostFromTo>();
            ends.Add(info.End, endList);
          }
          endList.Add(info);
        }
        string lcpPath = typeof(IO).Assembly.Location;
        w.WriteLine("-- Start files scripts");
        foreach (var pair in starts)
        {
          Box extent = GetBox(pair.Value, new List<IPoint>());
          Round(extent, resolution);
          w.WriteLine("{0} -h {1} -v {2} -E {3} {4} {5} {6} -r {7} -s {8} {9} -sc {10}_c.grd -sd {10}_d.grd",
            lcpPath, heightGrid, veloGrid,
            extent.Min.X, extent.Min.Y, extent.Max.X, extent.Max.Y,
            resolution, pair.Key.X, pair.Key.Y,
            pair.Value[0].From.Name);
        }
        w.WriteLine("-- End files scripts");
        foreach (var pair in ends)
        {
          Box extent = GetBox(pair.Value, new List<IPoint>());
          Round(extent, resolution);
          w.WriteLine("{0} -h {1} -v {2} -E {3} {4} {5} {6} -r {7} -e {8} {9} -ec _{10}c.grd -ed _{10}d.grd",
            lcpPath, heightGrid, veloGrid,
            extent.Min.X, extent.Min.Y, extent.Max.X, extent.Max.Y,
            resolution, pair.Key.X, pair.Key.Y,
            pair.Value[0].To.Name);
        }

        w.WriteLine("-- route files script");
        foreach (var route in _calcList.Keys)
        {
          Box extent = GetBox(new CostFromTo[] { route }, new List<IPoint>());
          Round(extent, resolution);
          w.WriteLine("{0} -E {1} {2} {3} {4} -r {5} -sc {6}_c.grd -sd {6}_d.grd -ec _{7}c.grd -ed _{7}d.grd -C {6}_{7}.tif -rg {6}_{7}r.tif",
            lcpPath,
            extent.Min.X, extent.Min.Y, extent.Max.X, extent.Max.Y,
            resolution,
            route.From.Name, route.To.Name);
        }
      }
    }

    private void Round(Box extent, double resolution)
    {
      extent.Min.X = Math.Floor(extent.Min.X / resolution) * resolution;
      extent.Min.Y = Math.Floor(extent.Min.Y / resolution) * resolution;

      extent.Max.X = Math.Ceiling(extent.Max.X / resolution) * resolution;
      extent.Max.Y = Math.Ceiling(extent.Max.Y / resolution) * resolution;
    }

    public List<CostSectionlist> CalcCourse(SectionCollection course, double resol)
    {
      if (course == null)
      { return null; }
      SectionCollection copy = new SectionCollection();
      copy.AddLast(course);
      int legs = copy.LegCount();
      VariationBuilder builder = new VariationBuilder(course);
      builder.VariationAdded += Builder_VariationAdded;
      List<SectionList> allPermuts = new List<SectionList>();
      if (legs > 1)
      {
        for (int i = 1; i <= legs; i++)
        {
          allPermuts.AddRange(builder.AnalyzeLeg(i));
        }
      }
      else
      {
        IList<SectionList> permuts = builder.Analyze(copy);
        if (permuts.Count > 1)
        {
          int nStarts = 0;
          foreach (var ctr in permuts[0].Controls)
          {
            if (ctr.Code == ControlCode.Start)
            { nStarts++; }
          }
          if (nStarts == 1)
          {
            allPermuts.AddRange(permuts);
          }
          else
          {
            permuts[0].PreName = "Full_";
            allPermuts.Add(permuts[0]);
            IList<SectionList> parts = SectionList.GetSimpleParts(permuts);
            allPermuts.AddRange(parts);
          }
        }
        else if (permuts.Count > 0)
        {
          allPermuts.Add(permuts[0]);
        }
      }

      List<CostSectionlist> courseInfos = GetCourseInfo(allPermuts, resol);
      return courseInfos;
    }

    void Builder_VariationAdded(object sender, SectionList variation)
    {
      VariationAdded?.Invoke(this, variation);
    }

    public List<CostSectionlist> GetCourseInfo(IList<SectionList> permuts, double resol)
    {
      List<CostSectionlist> courseInfo = new List<CostSectionlist>(permuts.Count);
      foreach (var permut in permuts)
      {
        CostSectionlist cost = GetCourseInfo(permut, resol);
        if (cost != null)
        {
          courseInfo.Add(cost);
        }
      }
      return courseInfo;
    }

    public CostSectionlist GetCourseInfo(SectionList permut, double resol)
    {
      List<Control> controls = new List<Control>(permut.Controls);
      Control from = null;
      IPoint toP = null;
      CostSum sum = null;
      foreach (var to in controls)
      {
        IPoint fromP = toP;

        if (to.Code == ControlCode.TextBlock
          || to.Code == ControlCode.MapChange)
        { continue; }

        toP = to.GetPoint();

        if (from != null)
        {
          CostFromTo part = CalcSection(from, to, fromP, toP, resol,
                                         from.Name + "->" + to.Name);
          if (sum == null)
          { sum = new CostSum(part); }
          else
          { sum = sum + part; }
        }
        from = to;
      }
      if (sum == null)
      { return null; }

      CostSectionlist cost = new CostSectionlist(sum, permut)
      { Name = permut.GetName() };

      return cost;
    }

    public Box GetBox(IPoint start, IPoint end, double l)
    {
      Box box = new Box(Point.Create(start), Point.Create(end), true);

      box.Min.X -= l;
      box.Min.Y -= l;
      box.Max.X += l;
      box.Max.Y += l;

      box.Min.X = Math.Max(box.Min.X, _heightGrid.Extent.Extent.Min.X);
      box.Min.Y = Math.Max(box.Min.Y, _heightGrid.Extent.Extent.Min.Y);
      box.Max.X = Math.Min(box.Max.X, _heightGrid.Extent.Extent.Max.X);
      box.Max.Y = Math.Min(box.Max.Y, _heightGrid.Extent.Extent.Max.Y);
      return box;
    }

    internal List<CostFromTo> GetRoutes(double resolution)
    {
      AccessCurrentCalcList((l) => { l.Clear(); });

      List<CostFromTo> fullList = new List<CostFromTo>(_calcList.Count);
      Dictionary<IPoint, List<CostFromTo>> startList =
        new Dictionary<IPoint, List<CostFromTo>>(new PointComparer());

      foreach (var cost in _calcList.Keys)
      {
        if (cost.Resolution > 0)
        { continue; }
        if (_calcList.ContainsKey(new CostFromTo(cost.From, cost.To, cost.Start, cost.End,
                                                resolution, 0, 0, null, 0, 0)))
        { continue; }

        IPoint start = cost.Start;
        if (!startList.TryGetValue(start, out List<CostFromTo> routes))
        {
          routes = new List<CostFromTo>();
          startList.Add(start, routes);
        }
        routes.Add(cost);
      }

      int idx = 0;
      foreach (var routes in startList.Values)
      {
        idx++;
        List<CostFromTo> notCalculated = new List<CostFromTo>(routes.Count);
        foreach (var route in routes)
        {
          if (_calcList.TryGetValue(new CostFromTo(route.From, route.To, route.Start, route.End,
                                                  resolution, 0, 0, null, 0, 0), out CostFromTo existing))
          {
            AccessCurrentCalcList((l) => { l.Add(existing); });
            continue;
          }
          notCalculated.Add(route);
        }
        if (notCalculated.Count == 0)
        { continue; }

        OnStatusChanged(string.Format("{0} of {1}: {2} -> :", idx, startList.Count, notCalculated[0].From.Name), null);
        List<CostFromTo> calcList = CalcRoutes(notCalculated, resolution);

        foreach (var cost in calcList)
        {
          if (!_calcList.ContainsKey(cost))
          { _calcList.Add(cost, cost); }
          AccessCurrentCalcList((l) => { l.Add(cost); });
        }

        fullList.AddRange(calcList);
      }
      return fullList;
    }

    private List<CostFromTo> CalcRoutes(List<CostFromTo> calcList,
      double resolution)
    {
      if (_heightGrid == null || _veloPath == null || calcList.Count == 0)
      {
        return new List<CostFromTo>();
      }


      List<IPoint> endList = new List<IPoint>(calcList.Count);
      foreach (var calc in calcList)
      {
        endList.Add(calc.End);
      }


      //Box box = GetBox(calcList, endList);

      //GridTest.LeastCostPath path = new GridTest.LeastCostPath(new GridTest.Step16(), box, resolution);
      ILcpModel lcpModel = GetLcpModel(resolution, _steps);

      //IDirCostModel<TvmCell> costProvider = new TerrainVeloModel(_heightGrid, VeloGrid)
      //{ TvmCalc = _tvmCalc };

      lcpModel.Status += RouteCalc_Status;
      ILeastCostData lcg = lcpModel.CalcCost(calcList[0].Start, endList); // endlist

      //LeastCostGrid<TvmCell> path = new LeastCostGrid<TvmCell>(costProvider, resolution, steps: Steps); // box: box);
      //path.Status += RouteCalc_Status;
      //ILeastCostData lcg = path.CalcCost(calcList[0].Start, endList);

      List<CostFromTo> result = new List<CostFromTo>();
      foreach (var routeCost in calcList)
      {
        double cost = lcg.GetCost(routeCost.End.X, routeCost.End.Y);
        Polyline route = GetRoute(lcg, routeCost.Start, routeCost.End, out double dh, out double optimal);

        CostFromTo add = new CostFromTo(
          routeCost.From, routeCost.To, routeCost.Start, routeCost.End, resolution,
          Math.Sqrt(PointOp.Dist2(routeCost.Start, routeCost.End)), dh, route, optimal, cost);
        result.Add(add);

        OnStatusChanged(null);
      }
      return result;
    }

    private static Box GetBox(IList<CostFromTo> calcList, List<IPoint> endList)
    {
      Box box = new Box(Point.Create(calcList[0].Start),
                        Point.Create(calcList[0].End), true);
      foreach (var info in calcList)
      {
        endList.Add(info.End);
        double l = Math.Sqrt(PointOp.Dist2(info.Start, info.End)) / 2.0 + 200;
        Box infoBox = new Box(Point.Create(info.Start), Point.Create(info.End), true);

        infoBox.Min.X -= l;
        infoBox.Min.Y -= l;
        infoBox.Max.X += l;
        infoBox.Max.Y += l;

        box.Include(infoBox);
      }
      return box;
    }

    private List<CostFromTo> _currentCalcList;
    private readonly object _lock = new object();
    internal void AccessCurrentCalcList(Action<List<CostFromTo>> action)
    {
      lock (_lock)
      {
        _currentCalcList = _currentCalcList ?? new List<CostFromTo>();
        action(_currentCalcList);
      }
    }

    private CostFromTo CalcSection(Control from, Control to,
      IPoint start, IPoint end, double resol, string section)
    {
      CostFromTo cost = CalcSectionCore(from, to, start, end, resol, section);
      if (resol > 0)
      { AccessCurrentCalcList((l) => { l.Add(cost); }); }
      return cost;
    }

    public ILcpGridModel GetLcpModel(double resol, Steps steps, IBox box = null)
    {
      TerrainVeloModel costProvider = GetTerrainVeloModel();
      if (costProvider != null)
      {
        LeastCostGrid<TvmCell> lcGrid = new LeastCostGrid<TvmCell>(costProvider, resol, steps: steps, box: box);
        return lcGrid;
      }
      ViewModels.SymbolVeloModel<TvmCell> symVeloModel = GetSymbolVeloModel(resol);
      if (symVeloModel != null)
      {
        LeastCostStack<TvmCell, IDirCostModel<TvmCell>> lcStack = LeastCostStack.Create(
          symVeloModel.Layers,
          symVeloModel.Teleports, resol, box);
        return lcStack;
      }

      return null;
    }

    public TerrainVeloModel GetTerrainVeloModel()
    {
      IGrid<double> veloGrid = VeloGrid;
      if (veloGrid == null)
      { return null; }
      return new TerrainVeloModel(_heightGrid, veloGrid) { TvmCalc = _tvmCalc };
    }
    public ViewModels.SymbolVeloModel<TvmCell> GetSymbolVeloModel(double resol)
    {
      if (_symVeloModel?.GridExtent.Dx != resol)
      { _symVeloModel = null; }

      if (_symVeloModel == null)
      {
        ViewModels.SymbolVeloModel<TvmCell> symVeloModel =
          ViewModels.SymbolVeloModel.FromXml<TvmCell>(VeloPath, resol);
        IGrid<double> veloGrd = symVeloModel;

        TerrainVeloModel tvm = new TerrainVeloModel(_heightGrid, veloGrd);
        symVeloModel.InitTeleports(tvm);

        _symVeloModel = symVeloModel;
      }
      return _symVeloModel;
    }

    private CostFromTo CalcSectionCore(Control from, Control to,
      IPoint start, IPoint end, double resol, string section)
    {
      CostFromTo existingInfo;
      if (_calcList != null && _calcList.TryGetValue(new CostFromTo(from, to,
        start, end, resol, 0, 0, null, 0, 0), out CostFromTo routeCost))
      {
        return routeCost;
      }
      if (_heightGrid == null || VeloPath == null || resol < 0)
      {
        routeCost = new CostFromTo(from, to, start, end, resol,
          Math.Sqrt(PointOp.Dist2(start, end)), 0, null, 0, 0);

        if (_calcList != null && _calcList.TryGetValue(routeCost, out existingInfo) == false)
        { _calcList.Add(routeCost, routeCost); }

        return routeCost;
      }

      OnStatusChanged(section + " : ");

      double l = Math.Sqrt(PointOp.Dist2(start, end));
      // Box box = GetBox(start, end, l / 2 + 200);

      ILcpModel lcpModel = GetLcpModel(resol, _steps);

      lcpModel.Status += Path_Status;


      DateTime t0 = DateTime.Now;
      ILeastCostData lcg = lcpModel.CalcCost(start, new[] { end });

      DateTime t1 = DateTime.Now;
      TimeSpan dt = t1 - t0;

      double cost = lcg.GetCost(end.X, end.Y);
      Polyline route = GetRoute(lcg, start, end, out double dh, out double length);

      OnStatusChanged(null);

      routeCost = new CostFromTo(from, to, start, end, resol, l, dh, route, length, cost);
      if (_calcList.TryGetValue(routeCost, out existingInfo) == false)
      { _calcList.Add(routeCost, routeCost); }
      else
      { }
      return routeCost;
    }

    private Polyline GetRoute(ILeastCostData lcg, IPoint start, IPoint end, out double climb, out double optimal)
    {
      Polyline line = lcg.GetPath(end);
      {
        Point s = Point.Create(line.Points[0]);
        s.X = start.X;
        s.Y = start.Y;
        line.Replace(0, s);
      }
      {
        Point e = Point.Create(line.GetPoint(-1));
        e.X = end.X;
        e.Y = end.Y;
        line.Replace(-1, e);
      }
      climb = -1;
      double h0 = 0;
      foreach (var p1 in line.Points)
      {
        double h1 = _heightGrid.Value(p1.X, p1.Y, EGridInterpolation.bilinear);
        if (climb >= 0 && h0 < h1)
        {
          climb += h1 - h0;
        }
        else if (climb < 0)
        { climb = 0; }
        h0 = h1;
      }
      if (line.Dimension != 2)
      { throw new InvalidProgramException(string.Format("Expected dimension 2, got {0}", line.Dimension)); }

      optimal = line.Project(Geometry.ToXY).Length();

      return line;
    }

    public void InitRawCourses(string courseFile)
    {
      IList<string> courseList = Utils.GetCourseList(courseFile);
      using (OcadReader reader = OcadReader.Open(courseFile))
      {
        foreach (var courseName in courseList)
        {
          Course course = reader.ReadCourse(courseName);
          CalcCourse(course, -1);
        }
      }
    }

    private void Path_Status(object sender, StatusEventArgs args)
    {
      OnStatusChanged(sender, args);
    }

    protected void OnStatusChanged(object sender, StatusEventArgs args)
    {
      StatusChanged?.Invoke(sender, args);
    }

    private void OnStatusChanged(string msg)
    {
      StatusChanged?.Invoke(msg, null);
    }

    private void RouteCalc_Status(object sender, StatusEventArgs args)
    {
      OnStatusChanged(sender, args);
    }
  }
}
