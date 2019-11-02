using Basics;
using Basics.Views;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace OCourse.ViewModels
{
  public class VeloModelDto
  {
    public string MapName { get; set; }
    public double DefaultVelocity { get; set; }
    public double DefaultPntSize { get; set; }
    public List<SymbolDto> Symbols { get; set; }

    public void SetValues(VeloModelVm vm)
    {
      vm.MapName = MapName;
      vm.DefaultVelocity = DefaultVelocity;
      vm.DefaultPntSize = DefaultPntSize;
      vm.SetSymbols(Symbols);
    }
    public static VeloModelDto CreateFrom(VeloModelVm vm)
    {
      VeloModelDto dto = new VeloModelDto
      {
        MapName = vm.MapName,
        DefaultVelocity = vm.DefaultVelocity,
        DefaultPntSize = vm.DefaultPntSize,
        Symbols = new List<SymbolDto>(vm.Symbols.Select(x => SymbolDto.CreateFrom(x)))
      };
      return dto;
    }
  }
  public class SymbolDto
  {
    [XmlAttribute]
    public int Id { get; set; }
    [XmlAttribute]
    public string Description { get; set; }
    [XmlAttribute]
    public string Velocity { get; set; }
    [XmlAttribute]
    public string Prio { get; set; }
    [XmlAttribute]
    public string Size { get; set; }
    [XmlAttribute]
    public string Teleport { get; set; }

    public VeloModelVm.SymbolVm ToSymbol()
    {
      VeloModelVm.SymbolVm sym = new VeloModelVm.SymbolVm
      {
        Id = Id,
        Description = Description,
        Velocity = ParseDouble(Velocity),
        Size = ParseDouble(Size),
        Priority = ParseInt(Prio),

        Teleport = ParseDouble(Teleport)
      };
      return sym;
    }
    private double? ParseDouble(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      { return null; }
      if (double.TryParse(value, out double d))
      { return d; }
      return null;
    }
    private int? ParseInt(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      { return null; }
      if (int.TryParse(value, out int i))
      { return i; }
      return null;
    }
    public static SymbolDto CreateFrom(VeloModelVm.SymbolVm vm)
    {
      SymbolDto dto = new SymbolDto
      {
        Id = vm.Id,
        Description = vm.Description,
        Velocity = vm.Velocity?.ToString("0.00"),
        Prio = vm.Priority?.ToString("0"),
        Size = vm.Size?.ToString("0.00")
      };
      return dto;
    }
  }
  public partial class VeloModelVm : BaseVm
  {
    public class SymbolVm : NotifyListener
    {
      public int Id { get; internal set; }
      public string Code => $"{Id / 1000.0:N3}";
      public string Description { get; internal set; }
      public int NObjects { get; set; }
      public Ocad.Symbol.SymbolStatus Status { get; set; }
      public Ocad.Symbol.SymbolType Type { get; set; }

      private double? _velocity;
      public double? Velocity
      {
        get { return _velocity; }
        set
        {
          _velocity = value;
          Changed();
        }
      }

      private double? _teleport;
      public double? Teleport
      {
        get { return _teleport; }
        set
        {
          _teleport = value;
          Changed();
        }
      }

      private int? _prio;
      public int? Priority
      {
        get { return _prio; }
        set
        {
          _prio = value;
          Changed();
        }
      }
      private double? _size;
      public double? Size
      {
        get { return _size; }
        set
        {
          if (value.HasValue && value.Value < 0)
          { return; }
          _size = value;
          Changed();
        }
      }

      protected override void Disposing(bool disposing)
      { }

      public static SymbolVm CreateFrom(Ocad.Symbol.BaseSymbol symbol, Ocad.Setup setup)
      {
        SymbolVm vm = new SymbolVm { Id = symbol.Number, Description = symbol.Description, Status = symbol.Status, Type = symbol.Type };
        if (symbol is Ocad.Symbol.LineSymbol l)
        {
          vm.Size = l.LineWidth * Ocad.FileParam.OCAD_UNIT * setup.Scale;
        }
        return vm;
      }
    }
    private readonly BindingListView<SymbolVm> _symbols;

    public VeloModelVm()
    {
      _symbols = new BindingListView<SymbolVm>();
    }

    private string _mapName;
    public string MapName
    {
      get { return _mapName; }
      set
      {
        _mapName = value;
        Changed();
      }
    }
    private string _settingsPath;
    public string SettingsPath
    {
      get { return _settingsPath; }
      set { SetValue(ref _settingsPath, value, string.Empty); }
    }
    public bool CanSave => !string.IsNullOrWhiteSpace(_settingsPath);

    private double _defaultVelo = 0.9;
    public double DefaultVelocity
    {
      get { return _defaultVelo; }
      set
      {
        if (value > 0)
        { SetStruct(ref _defaultVelo, value); }
      }
    }

    private double _defaultPntSize = 0;
    public double DefaultPntSize
    {
      get { return _defaultPntSize; }
      set
      {
        if (value >= 0)
        { SetStruct(ref _defaultPntSize, value); }
      }
    }

    public IList<SymbolVm> Symbols => _symbols;
    internal void SetSymbols(IEnumerable<SymbolDto> dtos)
    {
      foreach (var dto in dtos)
      { _symbols.Add(dto.ToSymbol()); }
    }

    public bool LoadSettings(string settingsFile)
    {
      if (!File.Exists(settingsFile))
      { return false; }

      try
      {
        _symbols.RaiseListChangedEvents = false;
        _symbols.Clear();
        using (TextReader r = new StreamReader(settingsFile))
        {
          Serializer.Deserialize(out VeloModelDto dto, r);
          dto.SetValues(this);
        }
        SynchMap(MapName);
      }
      finally
      {
        _symbols.RaiseListChangedEvents = true;
        _symbols.ResetBindings();
      }


      SettingsPath = settingsFile;
      return true;
    }

    public void Save(string fileName)
    {
      using (TextWriter w = new StreamWriter(fileName))
      {
        Serializer.Serialize(VeloModelDto.CreateFrom(this), w);
      }
      SettingsPath = fileName;
    }

    public bool SetMap(string mapName)
    {
      MapName = mapName;

      try
      {
        _symbols.RaiseListChangedEvents = false;
        SynchMap(mapName);
      }
      finally
      {
        _symbols.RaiseListChangedEvents = true;
        _symbols.ResetBindings();
      }
      return true;
    }

    private void SynchMap(string mapName)
    {
      Dictionary<int, SymbolVm> origDict = _symbols.ToDictionary(x => x.Id);
      Dictionary<int, SymbolVm> symDict = new Dictionary<int, SymbolVm>(origDict);
      GetDefinedSymbols(mapName, symDict, out double scale);
      UpdateStatistics(mapName, symDict);

      foreach (var symbol in symDict.Values)
      {
        if (origDict.ContainsKey(symbol.Id))
        { continue; }
        if (scale <= 5000)
        { InitIoSprintDefault(symbol); }
        else
        { InitIosDefault(symbol); }
      }
      List<SymbolVm> symbols = new List<SymbolVm>(symDict.Values);
      symbols.Sort((x, y) => x.Id.CompareTo(y.Id));

      _symbols.Clear();
      foreach (var sym in symbols)
      {
        _symbols.Add(sym);
      }
    }

    private void GetDefinedSymbols(string mapName, Dictionary<int, SymbolVm> symDict, out double scale, bool ignoreMissing = false)
    {
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(mapName))
      {
        Ocad.Setup setup = r.ReadSetup();
        scale = setup.Scale;
        foreach (var symbol in r.ReadSymbols())
        {
          if (!symDict.TryGetValue(symbol.Number, out SymbolVm sym))
          {
            if (ignoreMissing)
            { continue; }
            sym = SymbolVm.CreateFrom(symbol, setup);
            symDict.Add(sym.Id, sym);
          }
          else
          {
            sym.Description = symbol.Description;
            sym.Status = symbol.Status;
            sym.Type = symbol.Type;
          }
        }
      }
    }
    private void UpdateStatistics(string mapName, Dictionary<int, SymbolVm> symDict, bool ignoreMissing = false)
    {
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(mapName))
      {
        foreach (var elem in r.EnumGeoElements())
        {
          if (!symDict.TryGetValue(elem.Symbol, out SymbolVm sym))
          {
            if (ignoreMissing)
            { continue; }
            sym = new SymbolVm { Id = elem.Symbol, Type = Ocad.Symbol.SymbolType.unknown };
            symDict.Add(sym.Id, sym);
          }
          sym.NObjects++;
        }
      }
    }

    private void InitIoSprintDefault(SymbolVm symbol)
    {
      InitCommonDefault(symbol);
      int code = symbol.Id / 1000;
      if (code == 410) { symbol.Velocity = 0.0; }
    }
    private void InitIosDefault(SymbolVm symbol)
    {
      InitCommonDefault(symbol);
    }
    private void InitCommonDefault(SymbolVm symbol)
    {
      int code = symbol.Id / 1000;
      if (code == 101) { symbol.Velocity = null; }
      if (code == 102) { symbol.Velocity = null; }
      if (code == 103) { symbol.Velocity = null; }
      if (code == 104) { symbol.Velocity = 0.5; symbol.Priority = -1; }
      if (code == 105) { symbol.Velocity = 0.7; symbol.Priority = -1; }
      if (code == 106) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 107) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 108) { symbol.Velocity = null; }
      if (code == 109) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 110) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 111) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 112) { symbol.Velocity = 0.7; symbol.Priority = -1; }
      if (code == 113) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 114) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 115) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 201) { symbol.Velocity = 0; }
      if (code == 202) { symbol.Velocity = 0.5; }
      if (code == 203) { symbol.Velocity = 0.6; symbol.Priority = -1; }
      if (code == 204) { symbol.Velocity = 0.3; }
      if (code == 205) { symbol.Velocity = 0.3; }
      if (code == 206) { symbol.Velocity = 0.1; }
      if (code == 207) { symbol.Velocity = 0.5; symbol.Priority = -1; }
      if (code == 208) { symbol.Velocity = 0.7; symbol.Priority = -1; }
      if (code == 209) { symbol.Velocity = 0.4; }
      if (code == 210) { symbol.Velocity = 0.7; }
      if (code == 211) { symbol.Velocity = 0.4; }
      if (code == 212) { symbol.Velocity = 0.15; }
      if (code == 213) { symbol.Velocity = 0.8; }
      if (code == 214) { symbol.Velocity = 0.95; }
      if (code == 215) { symbol.Velocity = 0.9; symbol.Priority = -1; }
      if (code == 301) { symbol.Velocity = 0; }
      if (code == 302) { symbol.Velocity = 0.6; }
      if (code == 303) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 304) { symbol.Velocity = 0.5; symbol.Priority = -1; }
      if (code == 305) { symbol.Velocity = 0.7; symbol.Priority = -1; }
      if (code == 306) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 307) { symbol.Velocity = 0; }
      if (code == 308) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 309) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 310) { symbol.Velocity = 0.85; symbol.Priority = -1; }
      if (code == 311) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 312) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 313) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 401) { symbol.Velocity = 0.9; symbol.Priority = -1; }
      if (code == 402) { symbol.Velocity = 0.9; symbol.Priority = -1; }
      if (code == 403) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 404) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 405) { symbol.Velocity = 0.9; }
      if (code == 406) { symbol.Velocity = 0.7; }
      if (code == 407) { symbol.Velocity = 0.7; }
      if (code == 408) { symbol.Velocity = 0.4; }
      if (code == 409) { symbol.Velocity = 0.4; }
      if (code == 410) { symbol.Velocity = 0.2; }
      if (code == 411) { symbol.Velocity = 0; }
      if (code == 412) { symbol.Velocity = 0; }
      if (code == 413) { symbol.Velocity = 0.9; }
      if (code == 414) { symbol.Velocity = 0; }
      if (code == 415) { symbol.Velocity = null; }
      if (code == 416) { symbol.Velocity = null; }
      if (code == 417) { symbol.Velocity = 0.8; }
      if (code == 418) { symbol.Velocity = 0.5; }
      if (code == 419) { symbol.Velocity = 0.8; }
      if (code == 501) { symbol.Velocity = 1.0; }
      if (code == 502) { symbol.Velocity = 1.0; }
      if (code == 503) { symbol.Velocity = 1.0; }
      if (code == 504) { symbol.Velocity = 1.0; }
      if (code == 505) { symbol.Velocity = 1.0; }
      if (code == 506) { symbol.Velocity = 0.95; }
      if (code == 507) { symbol.Velocity = 0.95; }
      if (code == 508) { symbol.Velocity = null; }
      if (code == 509) { symbol.Velocity = 0; }
      if (code == 510) { symbol.Velocity = null; }
      if (code == 511) { symbol.Velocity = null; }
      if (code == 512) { symbol.Velocity = 0; }
      if (code == 513) { symbol.Velocity = 0.5; }
      if (code == 514) { symbol.Velocity = 0.7; }
      if (code == 515) { symbol.Velocity = 0; }
      if (code == 516) { symbol.Velocity = 0.5; }
      if (code == 517) { symbol.Velocity = 0.8; }
      if (code == 518) { symbol.Velocity = 0; }
      if (code == 519) { symbol.Velocity = null; }
      if (code == 520) { symbol.Velocity = 0; }
      if (code == 521) { symbol.Velocity = 0; }
      if (code == 522) { symbol.Velocity = 1.0; }
      if (code == 523) { symbol.Velocity = 0.5; }
      if (code == 524) { symbol.Velocity = 0.5; symbol.Priority = -1; }
      if (code == 525) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 526) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 527) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 528) { symbol.Velocity = 0.8; }
      if (code == 529) { symbol.Velocity = 0; }
      if (code == 530) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 531) { symbol.Velocity = 0.8; symbol.Priority = -1; }
      if (code == 601) { symbol.Velocity = null; }
      if (code == 602) { symbol.Velocity = null; }
      if (code == 603) { symbol.Velocity = null; }
      if (code == 701) { symbol.Velocity = null; }
      if (code == 702) { symbol.Velocity = null; }
      if (code == 703) { symbol.Velocity = null; }
      if (code == 704) { symbol.Velocity = null; }
      if (code == 705) { symbol.Velocity = null; }
      if (code == 706) { symbol.Velocity = null; }
      if (code == 707) { symbol.Velocity = null; }
      if (code == 708) { symbol.Velocity = 0; }
      if (code == 709) { symbol.Velocity = 0; }
      if (code == 710) { symbol.Velocity = null; }
      if (code == 711) { symbol.Velocity = 0; }
      if (code == 712) { symbol.Velocity = null; }
      if (code == 713) { symbol.Velocity = null; }
      if (code == 714) { symbol.Velocity = null; }
      if (code == 900) { symbol.Velocity = null; }
      if (code == 901) { symbol.Velocity = null; }
      if (code == 902) { symbol.Velocity = null; }

      if (code == 800) { symbol.Velocity = null; symbol.Teleport = 1; }
    }
  }
}
