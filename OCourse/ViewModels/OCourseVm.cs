using Basics.Geom;
using Basics.Views;
using Grid;
using Grid.Lcp;
using Ocad;
using OCourse.Ext;
using OCourse.Route;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using Ocad.StringParams;

namespace OCourse.ViewModels
{
  public enum VelocityType { Auto, GrayScale, Colors }
  public enum VarBuilderType { All, Explicit }
  public enum DisplayType { Course, All }


  public class OCourseVm : NotifyListener
  {
    public delegate void ShowGridInContext(LeastCostPath path, StatusEventArgs args);
    public delegate void DrawCourseHandler();

    private const string LegColumn = "Leg";
    public event ShowGridInContext ShowProgress;
    public event DrawCourseHandler DrawingCourse;

    internal const string MeanPrefix = "_Mean ";

    internal const string IndexName = "Index";
    internal const string StartNrName = "StartNr";
    internal const string PartName = "Part";


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
    private DataView _permutations;

    public OCourseVm()
    {
      _lcpConfig = new ConfigVm();
      _courseNames = new BindingListView<string>();
      CategoryNames = new BindingListView<string>();

      _veloTypes = new List<EnumText<VelocityType>>();
      _veloTypes.Add(new EnumText<VelocityType>("Auto", VelocityType.Auto));
      _veloTypes.Add(new EnumText<VelocityType>("From Gray value", VelocityType.GrayScale));
      _veloTypes.Add(new EnumText<VelocityType>("From Color", VelocityType.Colors));

      _veloType = VelocityType.Auto;

      _varBuilderTypes = new List<EnumText<VarBuilderType>>();
      _varBuilderTypes.Add(new EnumText<VarBuilderType>("All", VarBuilderType.All));
      _varBuilderTypes.Add(new EnumText<VarBuilderType>("Explicits", VarBuilderType.Explicit));

      _varBuilderType = VarBuilderType.All;

      _displayTypes = new List<EnumText<DisplayType>>();
      _displayTypes.Add(new EnumText<DisplayType>("Course", DisplayType.Course));
      _displayTypes.Add(new EnumText<DisplayType>("All", DisplayType.All));

      _displayType = DisplayType.Course;

      _selectedRoute = new BindingListView<ICost>();
      _info = new BindingListView<ICost>();
    }

    protected override void Disposing(bool disposing)
    { }

    public string CourseFile
    {
      get { return _courseFile; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
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
        using (Changing(MethodBase.GetCurrentMethod()))
        {
          _courseName = value;
          SetCourse();
        }
      }
    }

    public Course Course
    {
      get { return _course; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
        {
          _course = value;

          if (_course != null)
          {
            InitHeightVelo();
            VariationBuilder v = new VariationBuilder();
            PermutEstimate = v.EstimatePermutations(_course);
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
        using (Changing(MethodBase.GetCurrentMethod()))
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
        using (Changing(MethodBase.GetCurrentMethod()))
        { _permutEstimate = value; }
      }
    }

    public int StartNrMin
    {
      get { return _startNrMin; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
        { _startNrMin = value; }
      }
    }
    public int StartNrMax
    {
      get { return _startNrMax; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
        { _startNrMax = value; }
      }
    }

    public DataView Permutations
    {
      get { return _permutations; }
      set
      {
        using (Changing(MethodBase.GetCurrentMethod()))
        { _permutations = value; }
      }
    }

    public IList<ICost> Info
    {
      get { return _info; }
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

      using (Changing())
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
        using (Changing(MethodBase.GetCurrentMethod()))
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
        using (Changing(MethodBase.GetCurrentMethod()))
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
        using (Changing(MethodBase.GetCurrentMethod()))
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
        Changed(MethodBase.GetCurrentMethod());
      }
    }

    public void CancelCalc()
    {
      _cancelCalc = true;
      CancelRun();
    }
    public void DrawCourse()
    {
      if (DrawingCourse != null)
      { DrawingCourse(); }
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
        using (Changing(MethodBase.GetCurrentMethod()))
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
      CostSectionlist vRow = selection as CostSectionlist;
      if (vRow == null)
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
        foreach (CostSectionlist sectionsCost in info)
        {
          int leg;
          int.TryParse(sectionsCost.Name.Substring(0, 1), out leg);

          CostSum sum;
          if (!sumList.TryGetValue(leg, out sum))
          {
            sum = new CostSum(0, 0, 0, 0);
            sumList.Add(leg, sum);
            countList.Add(leg, new List<CostSectionlist>());
          }

          sumList[leg] = sum + sectionsCost;
          countList[leg].Add(sectionsCost);
        }
        foreach (KeyValuePair<int, CostSum> pair in sumList)
        {
          int leg = pair.Key;
          CostSum sum = pair.Value;
          sum = (1.0 / countList[leg].Count) * sum;
          CostMean mean = new CostMean(sum, countList[leg]);
          mean.Name = string.Format("{0} {1}", MeanPrefix, leg);
          routes.Add(mean);
        }
      }
      foreach (CostSectionlist pair in info)
      {
        routes.Add(pair);
      }

      try
      {
        _info.RaiseListChangedEvents = false;
        _info.Clear();
        foreach (ICost cost in routes)
        {
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
        foreach (DataRow row in reader)
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

          CostFromTo tVal;
          if (RouteCalculator.RouteCostDict.TryGetValue(routeCost, out tVal) == false)
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
      using (Changing())
      {
        bool oldExists = false;
        try
        {
          _courseNames.RaiseListChangedEvents = false;
          _courseNames.Clear();
          _courseNames.Add("");

          foreach (string course in Utils.GetCourseList(reader, pars))
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
        using (Changing())
        {
          Course course;
          using (OcadReader reader = OcadReader.Open(CourseFile))
          {
            _setup = reader.ReadSetup();
            course = reader.ReadCourse(CourseName);

            IList<string> catNames;
            if (course != null)
            { catNames = Category.GetCourseCategories(reader, course.Name); }
            else
            { catNames = new List<string>(); }

            try
            {
              CategoryNames.RaiseListChangedEvents = false;
              CategoryNames.Clear();
              foreach (string catName in catNames)
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
      List<Gaga> variations = new List<Gaga>();

      List<int> legs = new List<int>(nLegs);
      for (int i = 1; i <= nLegs; i++)
      {
        variations.Add(new Gaga { Leg = "L" + i });
        legs.Add(i);
      }
      SetVariations(course, legs, variations);

      // Permutations = tbl;
    }

    private void SetVariations(SectionCollection sections, IList<int> legs, List<Gaga> variations)
    {
      if (sections == null)
      { return; }

      foreach (ISection section in sections)
      {
        if (section is Variation)
        {
          Variation v = (Variation)section;
          foreach (Variation.Branch b in v.Branches)
          {
            SetVariations(b, b.Legs, variations);
          }
        }
        else if (section is Fork)
        {
          Fork fork = (Fork)section;
          foreach (Gaga row in variations)
          { row.Forks[fork] = "-"; }
          char v = 'A';
          foreach (int leg in legs)
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
      string veloGrid = null;
      IDoubleGrid heightGrid = null;
      bool isNew = false;
      if (_routeCalc != null)
      {
        veloGrid = _routeCalc.VeloGrid;
        heightGrid = _routeCalc.HeightGrid;
        if (LcpConfig.StepsMode.Count != _routeCalc.Step.Count)
        { isNew = true; }
        if (LcpConfig.CostProvider != _routeCalc.CostProvider)
        { isNew = true; }
      }
      if (LcpConfig.VeloPath != veloGrid)
      {
        isNew = true;
        veloGrid = LcpConfig.VeloPath;
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
          _routeCalc.StatusChanged -= routeCalc_Status;
          _routeCalc.VariationAdded -= builder_VariationAdded;
        }

        _routeCalc = new RouteCalculator(LcpConfig.CostProvider,
          heightGrid, veloGrid, LcpConfig.StepsMode);
        _routeCalc.StatusChanged += routeCalc_Status;
        _routeCalc.VariationAdded += builder_VariationAdded;
      }

      return _routeCalc;
    }

    private void SetEventInit()
    {
      InitHeightVelo();
      SetCourseList();

      EventSetter calcEvent = new EventSetter(this, RouteCalculator, _lcpConfig.Resolution, CourseFile);

      Working = true;
      RunAsync(calcEvent);
    }

    private void routeCalc_Status(object sender, StatusEventArgs args)
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
                           args.CurrentStep + " of (max) " + args.TotalStep;
        SetProgressAsync(prog);

        //ShowInContext((GridTest.LeastCostPath)sender, args);
        if (ShowProgress != null)
        { ShowProgress((LeastCostPath)sender, args); }

        if (_cancelCalc)
        {
          args.Cancel = true;
        }
      }
    }

    private string _lastVariation;
    private System.Diagnostics.Stopwatch _watch;
    private long _nextCall;
    void builder_VariationAdded(object sender, SectionList variation)
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
        builder_VariationAdded();
      }
    }

    void builder_VariationAdded()
    {
      SetProgressAsync(_lastVariation);
    }

    internal void PermutationsInit()
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

      VariationBuilder builder = new VariationBuilder();
      builder.VariationAdded += builder_VariationAdded;
      PermutationBuilder pb = new PermutationBuilder(this, builder, course, min, max);

      RunAsync(pb);
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

    private class Gaga
    {
      public Gaga()
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
          foreach (string courseName in courseList)
          {
            Course course = reader.ReadCourse(courseName);

            Course adapted = _parent.AdaptCourse(course, _varBuildType);

            List<CostSectionlist> courseCosts =
              _calc.CalcCourse(adapted, _resol, setup);

            foreach (CostSectionlist pair in courseCosts)
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
      private readonly Course _course;
      private readonly int _min;
      private readonly int _max;

      private IList<SectionList> _permuts;

      public PermutationBuilder(OCourseVm parent, VariationBuilder builder, Course course, int min, int max)
      {
        _parent = parent;
        _builder = builder;
        _course = course;
        _min = min;
        _max = max;
      }

      public bool Start()
      {
        _permuts = _builder.BuildPermutations(_course, _max - _min + 1);
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
        DataTable permutTbl = new DataTable();
        int idx = 0;
        Random r = new Random(_min);

        foreach (SectionList permut in permuts)
        {
          int index = idx;
          idx++;
          int pos = r.Next(startPos.Count);
          int startNr = startPos[pos];
          startPos.RemoveAt(pos);

          IList<SectionList> parts = permut.GetParts();
          int iPart = 1;
          if (permutTbl.Columns.Count == 0)
          {
            permutTbl.Columns.Add("All", typeof(IList<SectionList>));
            permutTbl.Columns.Add(IndexName, typeof(int));
            permutTbl.Columns.Add(StartNrName, typeof(int));
            foreach (SectionList part in parts)
            {
              permutTbl.Columns.Add(string.Format("{0}{1}", PartName, iPart), typeof(string));
              iPart++;
            }
          }

          DataRow row = permutTbl.NewRow();
          row[0] = parts;
          row[IndexName] = index;
          row[StartNrName] = startNr;
          iPart = 3;
          foreach (SectionList part in parts)
          {
            row[iPart] = part.GetName();
            iPart++;
          }
          permutTbl.Rows.Add(row);
        }
        permutTbl.AcceptChanges();
        DataView permutView = new DataView(permutTbl);
        permutView.AllowDelete = false;
        permutView.AllowNew = false;
        permutView.AllowEdit = false;
        permutView.Sort = StartNrName;

        _parent.Permutations = permutView;
      }
    }
  }
}
