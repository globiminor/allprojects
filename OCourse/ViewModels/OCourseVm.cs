using Basics.Geom;
using Basics.Views;
using Grid;
using Grid.Lcp;
using Ocad;
using Ocad.StringParams;
using OCourse.Commands;
using OCourse.Ext;
using OCourse.Route;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace OCourse.ViewModels
{
  public enum VelocityType { Auto, GrayScale, Colors }
  public enum VarBuilderType { All, Explicit }
  public enum DisplayType { Course, All }


  public class OCourseVm : NotifyListener
  {
    private class DataView { }
    private class DataRow { }
    private class DataTable { }

    public delegate void ShowGridInContext(LeastCostGrid path, StatusEventArgs args);
    public delegate void DrawCourseHandler();

    private const string _legColumn = "Leg";
    public event ShowGridInContext ShowProgress;
    public event DrawCourseHandler DrawingCourse;

    internal const string _meanPrefix = "_Mean ";

    private readonly BindingListView<string> _courseNames;
    private readonly List<EnumText<VelocityType>> _veloTypes;
    private readonly List<EnumText<VarBuilderType>> _varBuilderTypes;
    private readonly List<EnumText<DisplayType>> _displayTypes;

    private readonly ConfigVm _lcpConfig;

    private string _courseFile;
    private string _courseName;
    private string _categoryName;
    private VelocityType _veloType;
    private VarBuilderType _varBuilderType;
    private DisplayType _displayType;
    private Setup _setup;

    private Course _course;
    private int _permutEstimate;
    private int _startNrMin;
    private int _startNrMax;

    private bool _running;
    private bool _cancelCalc;

    private RouteCalculator _routeCalc;
    private List<CostSectionlist> _fullInfo;

    private SectionList _selectedComb;
    private readonly BindingListView<ICost> _selectedRoute;
    private readonly BindingListView<ICost> _info;
    private PermutationVms _permutations;

    public OCourseVm()
    {
      _lcpConfig = new ConfigVm();
      _courseNames = new BindingListView<string>();
      CategoryNames = new BindingListView<string>();

      _veloTypes = new List<EnumText<VelocityType>>
      {
        new EnumText<VelocityType>("Auto", VelocityType.Auto),
        new EnumText<VelocityType>("From Gray value", VelocityType.GrayScale),
        new EnumText<VelocityType>("From Color", VelocityType.Colors)
      };

      _veloType = VelocityType.Auto;

      _varBuilderTypes = new List<EnumText<VarBuilderType>>
      {
        new EnumText<VarBuilderType>("All", VarBuilderType.All),
        new EnumText<VarBuilderType>("Explicits", VarBuilderType.Explicit)
      };

      _varBuilderType = VarBuilderType.All;

      _displayTypes = new List<EnumText<DisplayType>>
      {
        new EnumText<DisplayType>("Course", DisplayType.Course),
        new EnumText<DisplayType>("All", DisplayType.All)
      };

      _displayType = DisplayType.Course;

      _selectedRoute = new BindingListView<ICost>();
      _info = new BindingListView<ICost>();
    }

    protected override void Disposing(bool disposing)
    { }

    private string _settingsName;
    public string SettingsName
    {
      get { return _settingsName; }
      set
      {
        _settingsName = value;
        Changed();
      }
    }
    public string Title { get { return $"OCourse {SettingsName}"; } }
    public bool CanSave { get { return _settingsName != null; } }

    public void LoadSettings(string settingsPath)
    {
      using (TextReader r = new StreamReader(settingsPath))
      {
        Basics.Serializer.Deserialize(out OCourseSettings settings, r);
        using (Changing())
        {
          CourseFile = settings.CourseFile;
          LcpConfig.HeightPath = settings.HeightFile;
          LcpConfig.VeloPath = settings.VeloFile;
          LcpConfig.Resolution = settings.Resolution ?? LcpConfig.Resolution;
        }
        if (settings.PathesFile != null && File.Exists(settings.PathesFile))
        {
          PathesFile = settings.PathesFile;
          ImportRoutes(PathesFile);
        }

        SettingsName = settingsPath;
      }
    }
    public void SaveSettings(string settingsPath = null)
    {
      settingsPath = settingsPath ?? SettingsName;
      using (TextWriter w = new StreamWriter(settingsPath))
      {
        OCourseSettings settings = new OCourseSettings
        {
          CourseFile = CourseFile,
          HeightFile = LcpConfig.HeightPath,
          VeloFile = LcpConfig.VeloPath,
          Resolution = LcpConfig.Resolution,

          PathesFile = PathesFile
        };
        Basics.Serializer.Serialize(settings, w);
        SettingsName = settingsPath;
      }
    }

    public string CourseFile
    {
      get { return _courseFile; }
      set
      {
        using (Changing())
        {
          _courseFile = value;
          SetCourseList();
        }
      }
    }
    public string CourseName
    {
      get { return _courseName; }
      set
      {
        using (Changing())
        {
          _courseName = value;
          SetCourse();
        }
      }
    }
    public string PathesFile { get; set; }

    public Course Course
    {
      get { return _course; }
      set
      {
        using (Changing())
        {
          _course = value;

          if (_course != null)
          {
            InitHeightVelo();
            VariationBuilder v = new VariationBuilder(_course);
            PermutEstimate = v.EstimatePermutations();
          }
          else
          {
            PermutEstimate = 0;
          }
        }
      }
    }
    public BindingListView<string> CourseNames
    {
      get { return _courseNames; }
    }
    public string CategoryName
    {
      get { return _categoryName; }
      set
      {
        using (Changing())
        {
          _categoryName = value;
          InitCategoryName();
        }
      }
    }

    public int PermutEstimate
    {
      get { return _permutEstimate; }
      set
      {
        using (Changing())
        { _permutEstimate = value; }
      }
    }

    public int StartNrMin
    {
      get { return _startNrMin; }
      set
      {
        using (Changing())
        { _startNrMin = value; }
      }
    }
    public int StartNrMax
    {
      get { return _startNrMax; }
      set
      {
        using (Changing())
        { _startNrMax = value; }
      }
    }

    public PermutationVms Permutations
    {
      get { return _permutations; }
      set
      {
        using (Changing())
        { _permutations = value; }
      }
    }

    public IReadOnlyList<ICost> Info
    {
      get
      {
        List<CostFromTo> updated = null;

        RouteCalculator?.AccessCurrentCalcList((calcList) =>
        {
          int d = (calcList?.Count ?? 0) - _info.Count;
          if (d > 0)
          {
            updated = new List<CostFromTo>(calcList.GetRange(_info.Count, d));
          }
        });
        if (updated != null)
        {
          try
          {
            _info.RaiseListChangedEvents = false;
            foreach (var cost in updated)
            { _info.Add(cost); }
          }
          finally
          {
            _info.RaiseListChangedEvents = true;
            _info.ResetBindings();
          }
        }
        return _info;
      }
    }

    public void CalcEventInit()
    {
      InitHeightVelo();
      SetCourseList();

      EventCalculator calcEvent = new EventCalculator(this, RouteCalculator, _lcpConfig.Resolution, CourseFile);

      Working = true;
      RunAsync(calcEvent);
    }

    private void InitCategoryName()
    {
      string catName = CategoryName;

      using (ChangingAll())
      {
        if (string.IsNullOrEmpty(catName))
        {
          StartNrMin = 0;
          StartNrMax = 0;
          return;
        }

        Category cat;
        using (OcadReader reader = OcadReader.Open(CourseFile))
        {
          cat = Category.Create(reader, catName, 1, -1);
        }
        StartNrMin = cat.MinStartNr;
        StartNrMax = cat.MaxStartNr;

        Permutations = null;
      }
    }


    public ConfigVm LcpConfig
    { get { return _lcpConfig; } }
    public VelocityType VelocityType
    {
      get { return _veloType; }
      set
      {
        using (Changing())
        { _veloType = value; }
      }
    }

    public List<EnumText<VelocityType>> VelocityTypes
    {
      get { return _veloTypes; }
    }

    public VarBuilderType VarBuilderType
    {
      get { return _varBuilderType; }
      set
      {
        using (Changing())
        {
          _varBuilderType = value;
          SetCourse();
        }
      }
    }
    public List<EnumText<VarBuilderType>> VarBuilderTypes
    {
      get { return _varBuilderTypes; }
    }

    public Setup Setup
    {
      get { return _setup; }
    }

    public DisplayType DisplayType
    {
      get { return _displayType; }
      set
      {
        using (Changing())
        {
          _displayType = value;
          SetCourse();
        }
      }
    }
    public List<EnumText<DisplayType>> DisplayTypes
    {
      get { return _displayTypes; }
    }

    public BindingListView<string> CategoryNames { get; }

    public bool Working
    {
      get { return _running; }
      private set
      {
        _running = value;
        _cancelCalc = false;
        Changed();
      }
    }

    public void CancelCalc()
    {
      _cancelCalc = true;
      CancelRun();
    }
    public void DrawCourse()
    {
      DrawingCourse?.Invoke();
    }

    public List<CostSectionlist> CalcCourse(IList<SectionList> permuts, Setup setup)
    {
      InitHeightVelo();

      double resol = _lcpConfig.Resolution;
      List<CostSectionlist> courseInfo;
      try
      {
        Working = true;
        courseInfo = RouteCalculator.GetCourseInfo(permuts, resol, setup);
      }
      finally
      {
        Working = false;
      }
      return courseInfo;
    }

    internal void SetCourseList()
    {
      using (OcadReader reader = OcadReader.Open(CourseFile))
      {
        IList<StringParamIndex> pars = reader.ReadStringParamIndices();
        SetCourseList(reader, pars);

        Config config = Utils.GetConfig(reader, pars);
        string dir = Path.GetDirectoryName(CourseFile);
        _lcpConfig.SetConfig(dir, config);
        if (config != null && config.Routes != null)
        {
          ImportRoutes(Config.GetFullPath(dir, config.Routes));
        }
      }
    }

    public SectionList SelectedCombination
    {
      get { return _selectedComb; }
      set
      {
        using (Changing())
        { _selectedComb = value; }
      }
    }

    internal RouteCalculator RouteCalculator
    {
      get
      {
        InitHeightVelo();
        return _routeCalc;
      }
    }
    internal bool IsRouteCalculatorNull
    {
      get { return _routeCalc == null; }
    }
    internal void SetRouteCalculator(RouteCalculator calc)
    {
      _routeCalc = calc;
    }

    public bool SetSelectionedComb(object selection)
    {
      if (!(selection is CostSectionlist vRow))
      { return false; }

      SectionList comb = vRow.Sections;

      SelectedCombination = comb;
      return true;
    }

    public void SetInfo(IList<ICost> routes, IList<CostSectionlist> info)
    {
      if (info == null)
      { return; }
      if (info.Count > 1)
      {
        Dictionary<int, CostSum> sumList = new Dictionary<int, CostSum>();
        Dictionary<int, List<CostSectionlist>> countList = new Dictionary<int, List<CostSectionlist>>();
        foreach (var sectionsCost in info)
        {
          int.TryParse(sectionsCost.Name.Substring(0, 1), out int leg);

          if (!sumList.TryGetValue(leg, out CostSum sum))
          {
            sum = new CostSum(0, 0, 0, 0);
            sumList.Add(leg, sum);
            countList.Add(leg, new List<CostSectionlist>());
          }

          sumList[leg] = sum + sectionsCost;
          countList[leg].Add(sectionsCost);
        }
        foreach (var pair in sumList)
        {
          int leg = pair.Key;
          CostSum sum = pair.Value;
          sum = (1.0 / countList[leg].Count) * sum;
          CostMean mean = new CostMean(sum, countList[leg])
          { Name = string.Format("{0} {1}", _meanPrefix, leg) };
          routes.Add(mean);
        }
      }
      foreach (var pair in info)
      {
        routes.Add(pair);
      }

      Dictionary<ICost, ICost> singleCosts = new Dictionary<ICost, ICost>();
      foreach (var costSection in info)
      {
        if (costSection.Parts != null)
        {
          foreach (var cost in costSection.Parts)
          { singleCosts[cost] = cost; }
        }
      }
      if (singleCosts.Count > 1)
      {
        foreach (var singleCost in singleCosts.Keys)
        {
          routes.Add(singleCost);
        }
      }

      RouteCalculator.AccessCurrentCalcList((l) => l.Clear());
      _info.Clear();
      try
      {
        _info.RaiseListChangedEvents = false;
        Dictionary<CostFromTo, CostFromTo> costDict =
          new Dictionary<CostFromTo, CostFromTo>(new CostFromTo.SectionComparer());
        foreach (var cost in routes)
        {
          if (cost is CostFromTo costFromTo)
          {
            if (costDict.ContainsKey(costFromTo))
            { continue; }
            costDict.Add(costFromTo, costFromTo);
          }
          _info.Add(cost);
        }
      }
      finally
      {
        _info.RaiseListChangedEvents = true;
        _info.ResetBindings();
      }
    }

    public void ImportRoutes(string shapeName)
    {
      InitHeightVelo();

      if (!File.Exists(shapeName))
      { return; }

      using (Shape.ShapeReader reader = new Shape.ShapeReader(shapeName))
      {
        foreach (var row in reader)
        {
          Polyline route = (Polyline)row["Shape"];
          string from = (string)row["From"];
          string to = (string)row["To"];
          double resol = (double)row["Resol"];
          double climb = (double)row["Climb"];
          double optimal = (double)row["Optimal"];
          double cost = (double)row["Cost"];

          Control ctrl0 = new Control();
          Control ctrl1 = new Control();

          ctrl0.Name = from.Trim();
          ctrl1.Name = to.Trim();
          IPoint start = new Point2D(route.Points.First.Value);
          IPoint end = new Point2D(route.Points.Last.Value);
          double direct = Math.Sqrt(PointOperator.Dist2(start, end));
          CostFromTo routeCost = new CostFromTo(ctrl0, ctrl1, start, end,
            resol, direct, climb, route, optimal, cost);

          if (RouteCalculator.RouteCostDict.TryGetValue(routeCost, out CostFromTo tVal) == false)
          { RouteCalculator.RouteCostDict.Add(routeCost, routeCost); }
        }
        reader.Close();
      }
    }

    public void SetCourseList(string courseFile)
    {
      using (OcadReader reader = OcadReader.Open(courseFile))
      {
        IList<StringParamIndex> pars = reader.ReadStringParamIndices();
        SetCourseList(reader, pars);
      }
    }
    public void SetCourseList(OcadReader reader, IList<StringParamIndex> pars)
    {
      string oldCourse = CourseName;
      using (ChangingAll())
      {
        bool oldExists = false;
        try
        {
          _courseNames.RaiseListChangedEvents = false;
          _courseNames.Clear();
          _courseNames.Add("");

          foreach (var course in Utils.GetCourseList(reader, pars))
          {
            _courseNames.Add(course);
            if (course == oldCourse)
            { oldExists = true; }
          }
        }
        finally
        {
          _courseNames.RaiseListChangedEvents = true;
          _courseNames.ResetBindings();
        }
        if (oldExists)
        { CourseName = oldCourse; }
        else
        { CourseName = ""; }
      }
    }

    private bool _settingCourse;
    private void SetCourse()
    {
      if (!File.Exists(CourseFile))
      { return; }

      if (_settingCourse)
      { return; }

      try
      {
        _settingCourse = true;
        using (ChangingAll())
        {
          Course course;
          using (OcadReader reader = OcadReader.Open(CourseFile))
          {
            _setup = reader.ReadSetup();
            course = reader.ReadCourse(CourseName);

            IList<string> catNames;
            if (course != null)
            { catNames = reader.GetCourseCategories(course.Name); }
            else
            { catNames = new List<string>(); }

            try
            {
              CategoryNames.RaiseListChangedEvents = false;
              CategoryNames.Clear();
              foreach (var catName in catNames)
              { CategoryNames.Add(catName); }
            }
            finally
            {
              CategoryNames.RaiseListChangedEvents = true;
              CategoryNames.ResetBindings();
            }
          }

          if (course == null)
          {
            DisplayType dspType = DisplayType;
            Course = null;
            if (dspType == DisplayType.All)
            {
              _selectedRoute.Clear();
              SetEventInit();
            }
            else
            {
              _info.Clear();
            }
            return;
          }

          course = AdaptCourse(course, VarBuilderType);
          SetVariations(course);
          Course = course;
        }

        _selectedRoute.Clear();
        _info.Clear();
        RouteCalculator.AccessCurrentCalcList((l) => l.Clear());
        CalcCourseInit(Course, _setup);
      }
      finally
      { _settingCourse = false; }
    }

    private Course AdaptCourse(Course course, VarBuilderType type)
    {
      Course result;
      if (type == VarBuilderType.All)
      {
        result = course;
      }
      else if (type == VarBuilderType.Explicit)
      {
        result = VariationBuilder.BuildExplicit(course);
      }
      else
      {
        throw new NotFiniteNumberException("Unhandled type " + type);
      }
      return result;
    }

    private void SetVariations(Course course)
    {
      int nLegs = course != null ? course.LegCount() : 0;
      List<LegForks> variations = new List<LegForks>();

      List<int> legs = new List<int>(nLegs);
      for (int i = 1; i <= nLegs; i++)
      {
        variations.Add(new LegForks { Leg = "L" + i });
        legs.Add(i);
      }
      SetVariations(course, legs, variations);

      // Permutations = tbl;
    }

    private void SetVariations(SectionCollection sections, IList<int> legs, List<LegForks> variations)
    {
      if (sections == null)
      { return; }

      foreach (var section in sections)
      {
        if (section is Variation)
        {
          Variation v = (Variation)section;
          foreach (Variation.Branch b in v.Branches)
          {
            SetVariations(b, b.Legs, variations);
          }
        }
        else if (section is Fork fork)
        {
          foreach (var row in variations)
          { row.Forks[fork] = "-"; }
          char v = 'A';
          foreach (var leg in legs)
          {
            variations[leg - 1].Forks[fork] = v.ToString();
            v++;
          }
        }
      }
    }

    private void CalcCourseInit(Course course, Setup setup)
    {
      InitHeightVelo();

      double resol = LcpConfig.Resolution;

      Working = true;
      CourseCalculator courseCalc = new CourseCalculator(this, RouteCalculator, course, resol, setup);
      RunAsync(courseCalc);
    }

    internal RouteCalculator InitHeightVelo()
    {
      string veloPath = null;
      IDoubleGrid heightGrid = null;
      bool isNew = false;
      if (_routeCalc != null)
      {
        veloPath = _routeCalc.VeloPath;
        heightGrid = _routeCalc.HeightGrid;
        if (LcpConfig.StepsMode.Count != _routeCalc.Steps.Count)
        { isNew = true; }
        if (LcpConfig.TvmCalc != _routeCalc.TvmCalc)
        { isNew = true; }
      }
      if (LcpConfig.VeloPath != veloPath)
      {
        isNew = true;
        veloPath = LcpConfig.VeloPath;
      }
      if (LcpConfig.HeightGrid != heightGrid)
      {
        isNew = true;
        heightGrid = LcpConfig.HeightGrid;
      }
      if (isNew || _routeCalc == null)
      {
        if (_routeCalc != null)
        {
          _routeCalc.StatusChanged -= RouteCalc_Status;
          _routeCalc.VariationAdded -= Builder_VariationAdded;
        }

        _routeCalc = new RouteCalculator(heightGrid, veloPath, LcpConfig.StepsMode, LcpConfig.TvmCalc);
        _routeCalc.StatusChanged += RouteCalc_Status;
        _routeCalc.VariationAdded += Builder_VariationAdded;
      }

      return _routeCalc;
    }

    private void SetEventInit()
    {
      InitHeightVelo();
      SetCourseList();

      EventSetter calcEvent = new EventSetter(this, RouteCalculator, _lcpConfig.Resolution, CourseFile);

      _info.Clear();
      Working = true;
      RunAsync(calcEvent);
    }

    private void RouteCalc_Status(object sender, StatusEventArgs args)
    {
      if (sender is string)
      {
        SetProgressAsync((string)sender);
      }
      if (args != null && (args.CurrentStep & 1023) == 0)
      {
        string prog = Progress;
        int l = prog.LastIndexOf(':');
        prog = prog.Substring(0, l + 1) +
                          $"{args.CurrentStep:N0} of (max) {args.TotalStep:N0}";
        SetProgressAsync(prog);

        //ShowInContext((GridTest.LeastCostPath)sender, args);
        ShowProgress?.Invoke((LeastCostGrid)sender, args);
        if (_cancelCalc)
        {
          args.Cancel = true;
        }
      }
    }

    private string _lastVariation;
    private System.Diagnostics.Stopwatch _watch;
    private long _nextCall;
    void Builder_VariationAdded(object sender, SectionList variation)
    {
      if (_watch == null)
      {
        _watch = new System.Diagnostics.Stopwatch();
        _nextCall = 0;
        _watch.Start();
      }
      long currentCall = _watch.ElapsedMilliseconds;
      if (currentCall > _nextCall)
      {
        _nextCall = currentCall + 100;
        _lastVariation = variation.ToString();
        Builder_VariationAdded();
      }
    }

    void Builder_VariationAdded()
    {
      SetProgressAsync(_lastVariation);
    }

    internal void PermutationsInit()
    {
      PermutationBuilder pb;
      try
      {
        Working = true;

        Course course;
        using (OcadReader reader = OcadReader.Open(CourseFile))
        {
          _setup = reader.ReadSetup();
          course = reader.ReadCourse(CourseName);
        }

        course = AdaptCourse(course, _varBuilderType);

        int min = StartNrMin;
        int max = StartNrMax;
        if (min > max) throw new ArgumentException("Min.StartNr > Max.StartNr");

        VariationBuilder builder = new VariationBuilder(course);
        builder.VariationAdded += Builder_VariationAdded;
        pb = new PermutationBuilder(this, builder, min, max);
      }
      catch
      {
        Working = false;
        throw;
      }
      RunAsync(pb);
    }

    internal void PermutationsExport(System.Collections.IEnumerable selectedRows, string file)
    {
      List<PermutationVm> permutations = new List<PermutationVm>();

      foreach (var o in selectedRows)
      {
        PermutationVm permutation = (PermutationVm)o;
        permutations.Add(permutation);
      }

      using (TextWriter writer = new StreamWriter(file, append: true))
      {
        CmdExportCourseV8 cmd = new CmdExportCourseV8(_course, permutations, _setup, writer);
        cmd.Execute();
      }
    }

    private class CourseCalculator : IWorker
    {
      private readonly OCourseVm _parent;
      private readonly RouteCalculator _routeCalculator;
      private readonly Course _course;
      private readonly double _resol;
      private readonly Setup _setup;
      private List<CostSectionlist> _courseInfo;

      public CourseCalculator(OCourseVm parent, RouteCalculator routeCalculator, Course course,
        double resol, Setup setup)
      {
        _parent = parent;
        _routeCalculator = routeCalculator;
        _course = course;
        _resol = resol;
        _setup = setup;
      }

      public bool Start()
      {
        _courseInfo = _routeCalculator.CalcCourse(_course, _resol, _setup);
        return true;
      }

      public void Finish()
      {
        _parent.Progress = "";
        _parent.SetInfo(_parent._selectedRoute, _courseInfo);

        _parent.Course = _course;

        _parent._fullInfo = _courseInfo;
      }
      public void Finally()
      {
        _parent.Working = false;
      }
    }

    private class LegForks
    {
      public LegForks()
      {
        Forks = new Dictionary<Fork, string>();
      }
      public string Leg { get; set; }
      public Dictionary<Fork, string> Forks { get; private set; }
    }

    private class EventCalculator : IWorker
    {
      private readonly OCourseVm _parent;
      private readonly RouteCalculator _calc;
      private readonly double _resol;
      private readonly string _courseFile;
      public EventCalculator(OCourseVm parent, RouteCalculator calc, double resol, string courseFile)
      {
        _parent = parent;
        _calc = calc;
        _resol = resol;
        _courseFile = courseFile;
      }
      public bool Start()
      {
        _calc.CalcEvent(_courseFile, _resol);
        return true;
      }

      public void Finish()
      {
        _parent.Progress = "";
      }

      public void Finally()
      {
        _parent.Working = false;
      }
    }

    private class EventSetter : IWorker
    {
      private readonly OCourseVm _parent;
      private readonly RouteCalculator _calc;
      private readonly double _resol;
      private readonly string _courseFile;
      private readonly VarBuilderType _varBuildType;

      private List<CostSectionlist> _allCosts;

      public EventSetter(OCourseVm parent, RouteCalculator calc, double resol, string courseFile)
      {
        _parent = parent;
        _calc = calc;
        _resol = resol;
        _courseFile = courseFile;

        _varBuildType = _parent.VarBuilderType;
      }
      public bool Start()
      {
        _calc.CalcEvent(_courseFile, _resol);

        _parent.SetProgressAsync("");

        List<CostSectionlist> allCosts = new List<CostSectionlist>();
        using (OcadReader reader = OcadReader.Open(_courseFile))
        {
          Setup setup = reader.ReadSetup();
          IList<StringParamIndex> idxList = reader.ReadStringParamIndices();

          IList<string> courseList = Utils.GetCourseList(reader, idxList);
          foreach (var courseName in courseList)
          {
            Course course = reader.ReadCourse(courseName);

            Course adapted = _parent.AdaptCourse(course, _varBuildType);

            List<CostSectionlist> courseCosts =
              _calc.CalcCourse(adapted, _resol, setup);

            foreach (var pair in courseCosts)
            {
              pair.Name = string.Format("{0} {1}", courseName, pair.Name);
              allCosts.Add(pair);
            }
          }
        }

        _allCosts = allCosts;
        return true;
      }

      public void Finish()
      {
        _parent.Progress = "";
        _parent.SetInfo(_parent._selectedRoute, _allCosts);
      }

      public void Finally()
      {
        _parent.Working = false;
      }
    }

    private class PermutationBuilder : IWorker
    {
      private readonly OCourseVm _parent;
      private readonly VariationBuilder _builder;
      private readonly int _min;
      private readonly int _max;

      private IList<SectionList> _permuts;

      public PermutationBuilder(OCourseVm parent, VariationBuilder builder, int min, int max)
      {
        _parent = parent;
        _builder = builder;
        _min = min;
        _max = max;
      }

      public bool Start()
      {
        _permuts = _builder.BuildPermutations(_max - _min + 1);
        return true;
      }

      public void Finally()
      {
        _parent.Working = false;
      }
      public void Finish()
      {
        _parent.Progress = "";
        IList<SectionList> permuts = _permuts;

        List<int> startPos = new List<int>(_max - _min + 1);
        for (int i = _min; i <= _max; i++)
        {
          startPos.Add(i);
        }
        PermutationVms permutations = new PermutationVms();
        int idx = 0;
        Random r = new Random(_min);

        foreach (var permut in permuts)
        {
          int index = idx;
          idx++;
          int pos = r.Next(startPos.Count);
          int startNr = startPos[pos];
          startPos.RemoveAt(pos);

          PermutationVm p = new PermutationVm(permut);
          p.Index = index;
          p.StartNr = startNr;
          permutations.Add(p);
        }

        permutations.AllowEdit = false;
        permutations.AllowNew = false;
        permutations.AllowRemove = false;
        permutations.ApplySort(nameof(PermutationVm.StartNr), ListSortDirection.Ascending);

        _parent.Permutations = permutations;
      }
    }
  }
}
