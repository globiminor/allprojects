using Basics;
using Basics.Geom;
using Basics.Geom.Projection;
using Basics.Views;
using Ocad;
using Ocad.StringParams;
using OcadScratch.Commands;
using OMapScratch;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OcadScratch.ViewModels
{
  public class ConfigVm : NotifyListener
  {
    private class LazySymbols
    {
      public BindingListView<ColorRef> Colors { get; set; }
      public BindingListView<Symbol> Symbols { get; set; }
    }
    private readonly string _configFile;

    private readonly XmlConfig _config;
    private readonly List<ProjectionVm> _projections;
    private ProjectionVm _prj;
    private Dictionary<int, ProjectionVm> _projectionDict;
    private BindingListView<ImageVm> _images;

    //private System.Lazy<LazySymbols> _lazySymbols;
    private LazySymbols _lazySymbols;

    public ConfigVm() :
      this(null, new XmlConfig())
    {
    }

    public ConfigVm(string configFile, XmlConfig config)
    {
      _configFile = configFile;

      _config = config;
      _config.Offset = _config.Offset ?? new XmlOffset();
      _config.Data = _config.Data ?? new XmlData();
      _config.Images = _config.Images ?? new List<XmlImage>();

      _projections = new List<ProjectionVm>
      {
        null,
        new ProjectionVm("CH1903", new Ch1903()) { OcadId = 14001 },
        new ProjectionVm("CH1903 LV95", new Ch1903_LV95()) { OcadId = 14002 },
      };

      //_lazySymbols = new System.Lazy<LazySymbols>(LoadSymbolsCore);
    }

    public string ConfigFile { get { return _configFile; } }
    public XmlConfig BaseConfig { get { return _config; } }

    public double OffsetX
    {
      get { return _config.Offset.X; }
      set { _config.Offset.X = value; }
    }

    public double OffsetY
    {
      get { return _config.Offset.Y; }
      set { _config.Offset.Y = value; }
    }

    public bool IsInit
    { get { return _config != null; } }

    public double? Lat
    {
      get { return _config.Offset.World?.Latitude; }
      set { _config.Offset.GetWorld().Latitude = value ?? 0; }
    }

    public double? Lon
    {
      get { return _config.Offset.World?.Longitude; }
      set { _config.Offset.GetWorld().Longitude = value ?? 0; }
    }

    public double? GeoMatrix00
    {
      get { return _config.Offset.World?.GeoMatrix00; }
      set { _config.Offset.GetWorld().GeoMatrix00 = value ?? 0; }
    }

    public double? GeoMatrix01
    {
      get { return _config.Offset.World?.GeoMatrix01; }
      set { _config.Offset.GetWorld().GeoMatrix01 = value ?? 0; }
    }

    public double? GeoMatrix10
    {
      get { return _config.Offset.World?.GeoMatrix10; }
      set { _config.Offset.GetWorld().GeoMatrix10 = value ?? 0; }
    }

    public double? GeoMatrix11
    {
      get { return _config.Offset.World?.GeoMatrix11; }
      set { _config.Offset.GetWorld().GeoMatrix11 = value ?? 0; }
    }

    public double? Declination
    {
      get { return _config.Offset.Declination; }
      set
      {
        _config.Offset.Declination = value;
        ChangedAll();
      }
    }

    public string Scratch
    {
      get { return _config.Data.Scratch; }
      set
      {
        if (_config.Data.Scratch != value)
        {
          _config.Data.Scratch = value;
          Validate();
          Changed();
        }
      }
    }

    public string ScratchImg
    {
      get { return _config.Data.ScratchImg; }
      set
      {
        if (_config.Data.ScratchImg != value)
        {
          _config.Data.ScratchImg = value;
          Validate();
          Changed();
        }
      }
    }

    public string SymbolPath
    {
      get { return _config.Data.Symbol; }
      set
      {
        if (_config.Data.Symbol != value)
        {
          _config.Data.Symbol = value;
          Validate();
          Changed();
        }
      }
    }

    public void AddImage(ImageVm img)
    {
      ImagesCore.Add(img);
      _config.Images.Add(img.BaseImage);
    }
    public IReadOnlyList<ImageVm> Images
    {
      get { return ImagesCore; }
    }
    private BindingListView<ImageVm> ImagesCore
    {
      get
      {
        if (_images == null)
        {
          BindingListView<ImageVm> imgs = new BindingListView<ImageVm>();
          if (_config.Images != null)
          {
            foreach (var img in _config.Images)
            { imgs.Add(new ImageVm(img)); }
          }
          _images = imgs;
        }
        return _images;
      }
    }

    public IList<ColorRef> Colors
    {
      //      get { return _lazySymbols.Value.Colors; }
      get { return _lazySymbols?.Colors; }
    }

    public IList<Symbol> Symbols
    {
      //    get { return _lazySymbols.Value.Symbols; }
      get { return _lazySymbols?.Symbols; }
    }

    public void LoadSymbols(XmlSymbols xmlSymbols = null)
    {
      _lazySymbols = _lazySymbols ?? InitSymbolsCore();

      _lazySymbols.Symbols.Clear();
      _lazySymbols.Colors.Clear();

      XmlSymbols xmls = xmlSymbols ?? InitXmlSymbols();

      if (xmls != null)
      {
        foreach (var clr in xmls.Colors)
        {
          _lazySymbols.Colors.Add(clr.GetColor());
        }
        foreach (var sym in xmls.Symbols)
        {
          _lazySymbols.Symbols.Add(sym.GetSymbol());
        }
      }
    }

    private LazySymbols InitSymbolsCore()
    {
      BindingListView<Symbol> symbols = new BindingListView<Symbol>();
      BindingListView<ColorRef> colors = new BindingListView<ColorRef>();
      LazySymbols lazy = new LazySymbols { Symbols = symbols, Colors = colors };

      symbols.AllowEdit = false;
      symbols.AllowNew = false;
      symbols.AllowRemove = false;

      colors.AllowEdit = false;
      colors.AllowNew = false;
      colors.AllowRemove = false;

      return lazy;
    }

    private XmlSymbols InitXmlSymbols()
    {
      if (_configFile == null)
      { return null; }
      string symFile = _config?.Data?.Symbol;
      if (symFile == null)
      { return null; }

      string symPath = Path.Combine(Path.GetDirectoryName(_configFile), symFile);

      try
      {
        Basics.Window.Browse.PortableDeviceUtils.Deserialize(symPath, out XmlSymbols xmls);

        return xmls;
      }
      catch (System.Exception e)
      { throw new System.InvalidOperationException($"Error in Symbols file '{symPath}'", e); }
    }

    public float SymbolScale
    {
      get
      {
        float current = _config.Data.SymbolScale ?? 0;
        return current != 0 ? current : Map.DefaultSymbolScale;
      }
      set
      {
        _config.Data.SymbolScale = value;
        Validate();
        Changed();
      }
    }

    public float Search
    {
      get
      {
        float current = _config.Data.Search ?? 0;
        return current > 0 ? current : Map.DefaultMinSearchDist;
      }
      set
      {
        _config.Data.Search = value;
        Validate();
        Changed();
      }
    }

    public float ElemTextSize
    {
      get
      {
        float current = _config.Data.ElemTextSize ?? 0;
        return current > 0 ? current : Map.DefaultElemTextSize;
      }
      set
      {
        _config.Data.ElemTextSize = value;
        Validate();
        Changed();
      }
    }

    public float ConstrTextSize
    {
      get
      {
        float current = _config.Data.ConstrTextSize ?? 0;
        return current > 0 ? current : Map.DefaultConstrTextSize;
      }
      set
      {
        _config.Data.ConstrTextSize = value;
        Validate();
        Changed();
      }
    }

    public float ConstrLineWidth
    {
      get
      {
        float current = _config.Data.ConstrLineWidth ?? 0;
        return current > 0 ? current : Map.DefaultConstrLineWidth;
      }
      set
      {
        _config.Data.ConstrLineWidth = value;
        Validate();
        Changed();
      }
    }

    public System.Windows.Media.Color ConstrColor
    {
      get
      {
        System.Windows.Media.Color clr = _config.Data.ConstrColor?.GetColor().Color ?? System.Windows.Media.Colors.Black;
        return clr;
      }
      set
      {
        System.Windows.Media.Color clr = value;
        _config.Data.ConstrColor = new XmlColor { Red = clr.R, Green = clr.G, Blue = clr.B };
        Changed();
      }
    }

    public ProjectionVm Projection
    {
      get { return _prj; }
      set
      {
        _prj = value;
        ChangedAll();
      }
    }

    public void Validate()
    {
      {
        string value = Scratch;
        string error = null;
        if (System.IO.Path.GetFileName(value) != value)
        { error = $"{Scratch} must be local"; }
        SetError(nameof(Scratch), error);
      }

      {
        string value = SymbolPath;
        string error = null;
        if (System.IO.Path.GetFileName(value) != value)
        { error = $"{SymbolPath} must be local"; }
        SetError(nameof(SymbolPath), error);
      }

      {
        double value = SymbolScale;
        string error = null;
        if (value < 0)
        { error = $"{SymbolScale} must be > 0"; }
        SetError(nameof(SymbolScale), error);
      }

      {
        double value = Search;
        string error = null;
        if (value < 0)
        { error = $"{Search} must be > 0"; }
        SetError(nameof(Search), error);
      }

    }

    public bool CanGeo2Map
    { get { return _prj != null && _config.Offset.World != null; } }

    public bool CanMap2Geo
    { get { return _prj != null; } }

    public bool CanDeclination
    {
      get { return _prj != null || _config.Offset.World != null; }
    }

    private Dictionary<int, ProjectionVm> ProjectionDict
    {
      get
      {
        return _projectionDict ??
          (_projectionDict = _projections.Where(x => x != null).ToDictionary(x => x.OcadId));
      }
    }

    public List<ProjectionVm> Projections
    {
      get { return _projections; }
    }

    protected override void Disposing(bool disposing)
    { }

    public void Init(string ocdFile)
    {
      int? gridAndZone = null;
      Point offset;
      using (OcadReader r = OcadReader.Open(ocdFile))
      {
        Setup s = r.ReadSetup();
        offset = s?.PrjTrans;

        OffsetX = offset?.X ?? 0;
        OffsetY = offset?.Y ?? 0;

        foreach (var idx in r.ReadStringParamIndices())
        {
          if (idx.Type == StringType.ScalePar)
          {
            string p = r.ReadStringParam(idx);
            ScalePar par = new ScalePar(p);
            gridAndZone = par.GridAndZone;
          }
        }
      }

      if (gridAndZone.HasValue && ProjectionDict.TryGetValue(gridAndZone.Value, out ProjectionVm prj))
      {
        Projection = prj;
      }

      CalcWgs84();
      CalcDeclination();

      ChangedAll();
    }

    public void CalcMapCoord()
    {
      ProjectionVm prj = Projection;
      if (prj == null)
      { return; }

      XmlWorld w = _config.Offset.World;
      if (w == null)
      { return; }

      IPoint map = prj.Geo2Map.Project(new Point2D(w.Longitude, w.Latitude));
      OffsetX = map.X;
      OffsetY = map.Y;

      ChangedAll();
    }

    public void CalcWgs84()
    {
      ProjectionVm prj = Projection;
      if (prj == null)
      { return; }

      IPoint offset = new Point2D(_config.Offset.X, _config.Offset.Y);

      IPoint wgs84 = prj.Map2Geo.Project(offset);
      Lon = wgs84.X;
      Lat = wgs84.Y;

      double d = 0.001;
      IPoint dEast = prj.Geo2Map.Project(new Point2D(wgs84.X + d, wgs84.Y));
      IPoint dNorth = prj.Geo2Map.Project(new Point2D(wgs84.X, wgs84.Y + d));

      XmlWorld w = _config.Offset.GetWorld();

      w.GeoMatrix00 = (dEast.X - offset.X) / d;
      w.GeoMatrix01 = (dEast.Y - offset.Y) / d;
      w.GeoMatrix10 = (dNorth.X - offset.X) / d;
      w.GeoMatrix11 = (dNorth.Y - offset.Y) / d;

      //IPoint test = new Point2D(offset.X + 80, offset.Y + 30);
      //IPoint rr = prj.Project(test);

      //double xt = (rr.X - wgs84.X) * w.GeoMatrix00 + (rr.Y - wgs84.Y) * w.GeoMatrix10;
      //double yt = (rr.X - wgs84.X) * w.GeoMatrix01 + (rr.Y - wgs84.Y) * w.GeoMatrix11;

      ChangedAll();
    }
    public void CalcDeclination()
    {
      double lat;
      double lon;
      if (Lat.HasValue && Lon.HasValue)
      {
        lat = Lat.Value;
        lon = Lon.Value;
      }
      else if (_prj != null)
      {
        IPoint wgs = _prj.Map2Geo.Project(new Point2D(OffsetX, OffsetY));
        lat = wgs.Y;
        lon = wgs.X;
      }
      else
      { return; }

      CmdCalcDeclination cmd = new CmdCalcDeclination(lat, lon);
      cmd.Execute();
      Declination = cmd.Declination ?? Declination;
    }
  }
}
