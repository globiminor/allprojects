﻿using Basics;
using Basics.Geom;
using Basics.Views;
using Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TMap;

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
    public string Size { get; set; }

    public VeloModelVm.SymbolVm ToSymbol()
    {
      VeloModelVm.SymbolVm sym = new VeloModelVm.SymbolVm
      {
        Id = Id,
        Description = Description,
        Velocity = Parse(Velocity),
        Size = Parse(Size)
      };
      return sym;
    }
    private double? Parse(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      { return null; }
      if (double.TryParse(value, out double d))
      { return d; }
      return null;
    }
    public static SymbolDto CreateFrom(VeloModelVm.SymbolVm vm)
    {
      SymbolDto dto = new SymbolDto
      {
        Id = vm.Id,
        Description = vm.Description,
        Velocity = vm.Velocity?.ToString("0.00"),
        Size = vm.Size?.ToString("0.00")
      };
      return dto;
    }
  }
  public partial class VeloModelVm : BaseVm
  {
    public class SymbolVm : NotifyListener
    {
      private double? _velocity;
      public int Id { get; internal set; }
      public string Code => $"{Id / 1000.0:N3}";
      public string Description { get; internal set; }
      public int NObjects { get; set; }
      public Ocad.Symbol.SymbolStatus Status { get; set; }
      public Ocad.Symbol.SymbolType Type { get; set; }
      public double? Velocity
      {
        get { return _velocity; }
        set
        {
          _velocity = value;
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
      GetDefinedSymbols(mapName, symDict);
      UpdateStatistics(mapName, symDict);

      foreach (var symbol in symDict.Values)
      {
        if (origDict.ContainsKey(symbol.Id))
        { continue; }
        symbol.Velocity = GetDefaultVelocity(symbol);
      }
      List<SymbolVm> symbols = new List<SymbolVm>(symDict.Values);
      symbols.Sort((x, y) => x.Id.CompareTo(y.Id));

      _symbols.Clear();
      foreach (var sym in symbols)
      {
        _symbols.Add(sym);
      }
    }

    private void GetDefinedSymbols(string mapName, Dictionary<int, SymbolVm> symDict, bool ignoreMissing = false)
    {
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(mapName))
      {
        Ocad.Setup setup = r.ReadSetup();
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

    private double? GetDefaultVelocity(SymbolVm symId)
    {
      int code = symId.Id / 1000;
      if (code == 101) { return null; }
      if (code == 102) { return null; }
      if (code == 103) { return null; }
      if (code == 104) { return 0.5; }
      if (code == 105) { return 0.7; }
      if (code == 106) { return 0.8; }
      if (code == 107) { return 0.8; }
      if (code == 108) { return null; }
      if (code == 109) { return 0.8; }
      if (code == 110) { return 0.8; }
      if (code == 111) { return 0.8; }
      if (code == 112) { return 0.7; }
      if (code == 113) { return 0.8; }
      if (code == 114) { return 0.8; }
      if (code == 115) { return 0.8; }
      if (code == 201) { return 0; }
      if (code == 202) { return 0; }
      if (code == 203) { return 0.6; }
      if (code == 204) { return 0.3; }
      if (code == 205) { return 0.3; }
      if (code == 206) { return 0.1; }
      if (code == 207) { return 0.5; }
      if (code == 208) { return 0.7; }
      if (code == 209) { return 0.4; }
      if (code == 210) { return 0.7; }
      if (code == 211) { return 0.4; }
      if (code == 212) { return 0.15; }
      if (code == 213) { return 0.8; }
      if (code == 214) { return 0.95; }
      if (code == 215) { return 0.9; }
      if (code == 301) { return 0; }
      if (code == 302) { return 0.6; }
      if (code == 303) { return 0.8; }
      if (code == 304) { return 0.5; }
      if (code == 305) { return 0.7; }
      if (code == 306) { return 0.8; }
      if (code == 307) { return 0; }
      if (code == 308) { return 0.8; }
      if (code == 309) { return 0.8; }
      if (code == 310) { return 0.85; }
      if (code == 311) { return 0.8; }
      if (code == 312) { return 0.8; }
      if (code == 313) { return 0.8; }
      if (code == 401) { return 0.9; }
      if (code == 402) { return 0.9; }
      if (code == 403) { return 0.8; }
      if (code == 404) { return 0.8; }
      if (code == 405) { return 0.9; }
      if (code == 406) { return 0.7; }
      if (code == 407) { return 0.7; }
      if (code == 408) { return 0.4; }
      if (code == 409) { return 0.4; }
      if (code == 410) { return 0.2; }
      if (code == 411) { return 0; }
      if (code == 412) { return 0; }
      if (code == 413) { return 0.9; }
      if (code == 414) { return 0; }
      if (code == 415) { return null; }
      if (code == 416) { return null; }
      if (code == 417) { return 0.8; }
      if (code == 418) { return 0.5; }
      if (code == 419) { return 0.8; }
      if (code == 501) { return 1.0; }
      if (code == 502) { return 1.0; }
      if (code == 503) { return 1.0; }
      if (code == 504) { return 1.0; }
      if (code == 505) { return 1.0; }
      if (code == 506) { return 0.95; }
      if (code == 507) { return 0.95; }
      if (code == 508) { return null; }
      if (code == 509) { return 0; }
      if (code == 510) { return null; }
      if (code == 511) { return null; }
      if (code == 512) { return 0; }
      if (code == 513) { return 0.5; }
      if (code == 514) { return 0.7; }
      if (code == 515) { return 0; }
      if (code == 516) { return 0.5; }
      if (code == 517) { return 0.8; }
      if (code == 518) { return 0; }
      if (code == 519) { return null; }
      if (code == 520) { return 0; }
      if (code == 521) { return 0; }
      if (code == 522) { return 1.0; }
      if (code == 523) { return 0.5; }
      if (code == 524) { return 0.5; }
      if (code == 525) { return 0.8; }
      if (code == 526) { return 0.8; }
      if (code == 527) { return 0.8; }
      if (code == 528) { return 0.8; }
      if (code == 529) { return 0; }
      if (code == 530) { return 0.8; }
      if (code == 531) { return 0.8; }
      if (code == 601) { return null; }
      if (code == 602) { return null; }
      if (code == 603) { return null; }
      if (code == 701) { return null; }
      if (code == 702) { return null; }
      if (code == 703) { return null; }
      if (code == 704) { return null; }
      if (code == 705) { return null; }
      if (code == 706) { return null; }
      if (code == 707) { return null; }
      if (code == 708) { return 0; }
      if (code == 709) { return 0; }
      if (code == 710) { return null; }
      if (code == 711) { return 0; }
      if (code == 712) { return null; }
      if (code == 713) { return null; }
      if (code == 900) { return null; }
      if (code == 901) { return null; }
      if (code == 902) { return null; }

      return null;
    }
  }

  public class SymbolVeloModel : Grid.IGrid<double>
  {
    public static SymbolVeloModel FromXml(string veloModelXmlPath, double stepSize)
    {
      VeloModelVm vm = new VeloModelVm();
      vm.LoadSettings(veloModelXmlPath);
      return new SymbolVeloModel(vm, stepSize);

    }

    private readonly VeloModelVm _vm;
    private readonly TMap.MapData _mapData;
    private readonly TiledByteGrid _grid;
    public SymbolVeloModel(VeloModelVm veloModelVm, double stepSize)
    {
      _vm = veloModelVm;
      _mapData = _vm.GetMapData();

      Basics.Geom.IBox allExtent = _mapData.Extent;
      int nx = (int)((allExtent.Max.X - allExtent.Min.X) / stepSize);
      int ny = (int)((allExtent.Max.Y - allExtent.Min.Y) / stepSize);
      _grid = new TiledByteGrid(nx, ny, allExtent.Min.X, allExtent.Max.Y, stepSize, this)
      { DoInitOnRead = true };
    }

    GridExtent IGrid.Extent => _grid.Extent;
    Type IGrid.Type => typeof(double);

    public double MinVelo { get; set; } = VelocityGrid.DefaultMinVelo;
    object IGrid.this[int ix, int iy] { get => GetValue(ix, iy); set { } }
    double IGrid<double>.this[int ix, int iy] { get => GetValue(ix, iy); set { } }

    public double GetValue(int ix, int iy)
    {
      byte b = _grid[ix, iy];
      if (b == 0) return MinVelo;
      return b / 255.0;
    }


    public double Value(double x, double y)
    {
      _grid.Extent.GetNearest(new Basics.Geom.Point2D(x, y), out int ix, out int iy);
      return GetValue(ix, iy);
    }

    private class TiledByteGrid : TiledGrid<byte>
    {
      private readonly SymbolVeloModel _parent;
      public TiledByteGrid(int nx, int ny, double x0, double y0, double dx, SymbolVeloModel parent)
        : base(new GridExtent(nx, ny, x0, y0, dx))
      {
        _parent = parent;
      }

      protected override IGrid<byte> CreateTile(int nx, int ny, double x0, double y0)
      {
        GridExtent tileExt = new GridExtent(nx, ny, x0, y0, Extent.Dx);

        SimpleGrid<byte> tile;
        using (Drawable drawable = new Drawable(tileExt))
        {
          _parent._mapData.Draw(drawable);
          tile = drawable.GetTile();
        }
        //ImageGrid.GridToImage("C:\\temp\\temp.tif", nx, ny, (x, y) => tile[x, y]);

        return tile;
      }
    }

    private class _Projection : IProjection
    {
      private readonly GridExtent _ext;
      public _Projection(GridExtent ext)
      {
        _ext = ext;
      }

      public IPoint Project(IPoint point)
      {
        return new Point2D((point.X - _ext.X0) / _ext.Dx,
                           (_ext.Y0 - point.Y) / _ext.Dx);
      }
    }

    private class Drawable : IDrawable, IDisposable
    {
      private readonly GridExtent _extent;
      private readonly IProjection _prj;
      private readonly System.Drawing.Bitmap _bmp;
      private readonly System.Drawing.Graphics _grp;
      private readonly System.Drawing.SolidBrush _brush;
      private readonly System.Drawing.Pen _pen;

      public Drawable(GridExtent extent)
      {
        _extent = extent;
        _prj = new _Projection(extent);

        _bmp = new System.Drawing.Bitmap(extent.Nx, extent.Ny);
        _grp = System.Drawing.Graphics.FromImage(_bmp);
        _brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
        _pen = new System.Drawing.Pen(System.Drawing.Color.White);
      }

      public SimpleGrid<byte> GetTile()
      {
        _grp.Flush();
        SimpleGrid<byte> tile = new SimpleGrid<byte>(_extent);
        ImageGrid.ImageToGrid(_bmp, (x, y, argb) => tile[x, y] = (byte)argb);
        //_bmp.Save("C:\\temp\\tempBmp.png");

        return tile;
      }

      public void Dispose()
      {
        _pen.Dispose();
        _brush.Dispose();
        _grp.Dispose();
        _bmp.Dispose();
      }
      public bool BreakDraw { get; set; }
      public IProjection Projection => _prj;
      public Box Extent => _extent.Extent;
      void IDrawable.BeginDraw() { }
      void IDrawable.BeginDraw(MapData data) { }
      void IDrawable.BeginDraw(ISymbolPart symbolPart, DataRow dataRow) { }
      void IDrawable.Draw(MapData data) { }

      public void DrawArea(Area area, ISymbolPart symbolPart)
      {
        _brush.Color = symbolPart.Color;
        Basics.Forms.DrawUtils.DrawArea(_grp, area, _brush);
      }
      public void DrawLine(Polyline line, ISymbolPart symbolPart)
      {
        _pen.Color = symbolPart.Color;
        _pen.Width = (float)((symbolPart as SymbolPartLine)?.LineWidth ?? 1);

        Basics.Forms.DrawUtils.DrawLine(_grp, line, _pen);
      }
      void IDrawable.DrawRaster(GridMapData raster) { }
      void IDrawable.EndDraw(ISymbolPart symbolPart) { }
      void IDrawable.EndDraw(MapData data) { }
      void IDrawable.EndDraw() { }
      void IDrawable.Flush() { }
      void IDrawable.SetExtent(IBox proposedExtent) { }
    }
  }
}
