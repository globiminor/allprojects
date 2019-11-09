﻿
using Basics.Geom;
using Grid;
using System;
using System.Collections.Generic;
using System.Data;
using TMap;

namespace OCourse.ViewModels
{
  public class SymbolVeloModel : IGrid<double>
  {
    public static SymbolVeloModel FromXml(string veloModelXmlPath, double stepSize)
    {
      VeloModelVm vm = new VeloModelVm();
      vm.LoadSettings(veloModelXmlPath);
      return new SymbolVeloModel(vm, stepSize);

    }

    public static SymbolVeloModel<T> FromXml<T>(string veloModelXmlPath, double stepSize)
    {
      VeloModelVm vm = new VeloModelVm();
      vm.LoadSettings(veloModelXmlPath);
      return new SymbolVeloModel<T>(vm, stepSize);
    }

    private readonly VeloModelVm _vm;
    private readonly MapData _mapData;
    private readonly TiledByteGrid _grid;
    private readonly double _maxSymbolSize;
    public SymbolVeloModel(VeloModelVm veloModelVm, double stepSize)
    {
      _vm = veloModelVm;
      _mapData = _vm.GetMapData();

      IBox allExtent = _mapData.Extent;
      int nx = (int)((allExtent.Max.X - allExtent.Min.X) / stepSize);
      int ny = (int)((allExtent.Max.Y - allExtent.Min.Y) / stepSize);
      _grid = new TiledByteGrid(nx, ny, allExtent.Min.X, allExtent.Max.Y, stepSize, this)
      { DoInitOnRead = true, TileSize = 256 };

      double maxSymbolSize = 0;
      foreach (var sym in _vm.Symbols)
      {
        if (sym.Velocity == null || sym.Size == null)
        { continue; }
        maxSymbolSize = Math.Max(maxSymbolSize, sym.Size.Value);
      }
      _maxSymbolSize = maxSymbolSize;
    }

    GridExtent IGrid.Extent => _grid.Extent;
    Type IGrid.Type => typeof(double);

    public double MinVelo { get; set; } = VelocityGrid.DefaultMinVelo;
    object IGrid.this[int ix, int iy] => GetValue(ix, iy); 
    double IGrid<double>.this[int ix, int iy] => GetValue(ix, iy);

    public double GetValue(int ix, int iy)
    {
      byte b = _grid[ix, iy];
      if (b == 0) return MinVelo;
      return b / 255.0;
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

        using (Drawable drawable = new Drawable(tileExt, _parent._maxSymbolSize))
        {
          _parent._mapData.Draw(drawable);
          SimpleGrid<byte> tile = drawable.GetTile();
          return tile;
        }
      }
    }

    private class MyProjection : IProjection
    {
      private readonly GridExtent _ext;
      public MyProjection(GridExtent ext)
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
      private readonly GridExtent _gridExtent;
      private readonly Box _geoExtent;
      private readonly IProjection _prj;
      private readonly System.Drawing.Bitmap _bmp;
      private readonly System.Drawing.Graphics _grp;
      private readonly System.Drawing.SolidBrush _brush;
      private readonly System.Drawing.Pen _pen;

      public Drawable(GridExtent extent, double expand)
      {
        _gridExtent = extent;
        Point2D ex = new Point2D(expand, expand);
        _geoExtent = new Box(extent.Extent.Min - ex, extent.Extent.Max + ex);
        _prj = new MyProjection(extent);

        _bmp = new System.Drawing.Bitmap(extent.Nx, extent.Ny);
        _grp = System.Drawing.Graphics.FromImage(_bmp);
        _brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
        _pen = new System.Drawing.Pen(System.Drawing.Color.White);
      }

      public SimpleGrid<byte> GetTile()
      {
        _grp.Flush();
        SimpleGrid<byte> tile = new SimpleGrid<byte>(_gridExtent);
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
      public Box Extent => _geoExtent;
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

  public class SymbolVeloModel<T> : SymbolVeloModel
  {
    public SymbolVeloModel(VeloModelVm veloModelVm, double stepSize)
      : base(veloModelVm, stepSize)
    {
      Layers = new List<Grid.Lcp.IDirCostModel<T>>();
      Teleports = new List<Grid.Lcp.Teleport<T>>();
    }

    public List<Grid.Lcp.IDirCostModel<T>> Layers { get; }
    public List<Grid.Lcp.Teleport<T>> Teleports { get; }
  }
}