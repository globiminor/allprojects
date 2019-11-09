using Basics.Data;
using Basics.Forms;
using Basics.Geom;
using OCourse.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using TMap;

namespace OCourse.Plugins
{
  public class VeloModelCmd : ICommand
  {
    private WdgVeloModel _wdg;

    public VeloModelCmd()
    {
      // "AssemblyResolve" is active only here
      // --> load all potentially needed libraries here
      _wdg = new WdgVeloModel();
      _wdg.Vm = new ViewModels.VeloModelVm();
      Common.Init();
    }

    public void Execute(IContext context)
    {
      if (_wdg == null || _wdg.IsDisposed)
      {
        _wdg = new WdgVeloModel();
        _wdg.Vm = new ViewModels.VeloModelVm();
        Common.Init();
      }
      _wdg.TMapContext = context;
      _wdg.Vm.TMapContext = context;
      if (context is System.Windows.Forms.IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
      _wdg.InitMapContext();
    }
  }
}

namespace OCourse.Gui
{
  partial class WdgVeloModel
  {
    private System.Windows.Forms.Button _addToMap;
    internal IContext TMapContext { get; set; }
    internal void InitMapContext()
    {
      if (_addToMap == null)
      {
        System.Windows.Forms.Button addToMap = new System.Windows.Forms.Button();
        addToMap.Text = "Add To Map";
        addToMap.Size = new System.Drawing.Size { Width = 100, Height = btnMap.Height };
        addToMap.Location = new System.Drawing.Point { X = grdSymbols.Right - addToMap.Size.Width, Y = lblDefaultVelo.Location.Y };
        addToMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        addToMap.Click += (s, e) =>
        {
          if (TMapContext == null || Vm == null)
          { return; }

          TableMapData mapData = Vm.GetMapData();
          TMapContext.Data.Subparts.Add(mapData);
          TMapContext.Refresh();
          TMapContext.Draw();
        };
        Controls.Add(addToMap);

        addToMap.Bind(x => x.Enabled, _bindingSource, nameof(Vm.CanAddToMap), false, System.Windows.Forms.DataSourceUpdateMode.Never);
        _addToMap = addToMap;
      }
    }
  }
}
namespace OCourse.ViewModels
{
  partial class VeloModelVm
  {
    private IContext _tmapContext;
    internal IContext TMapContext
    {
      get { return _tmapContext; }
      set { SetValue(ref _tmapContext, value); }
    }

    public bool CanAddToMap => TMapContext != null;

    private TableMapData _mapData;
    internal TableMapData GetMapData()
    {
      //if (TMapContext == null)
      //{ return null; }

      if (_mapData == null)
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(MapName);
        // Ocad.Data.OcadConnection conn = new Ocad.Data.OcadConnection(MapName) { SortByColors = true };
        SimpleConnection<VelocityRecord> conn = new SimpleConnection<VelocityRecord>(new VelocityData(this));
        string table = Ocad.Data.OcadConnection.TableElements;
        TData.TTable tTable = new TData.TTable(table, conn);
        TableMapData mapData = new TableMapData(name, tTable);

        mapData.Symbolisation.SymbolList.Table.Clear();

        Polyline circle = new Polyline();
        circle.Add(new Arc(new Point2D(0, 0), 1, 0, 2 * Math.PI));
        Symbol pointSym = new Symbol(new SymbolPartPoint
        {
          ColorExpression = $"{VelocityData.FieldColor}",
          SymbolLine = circle,
          ScaleExpression = $"{VelocityData.FieldSize}",
          Fill = true,
          Stroke = false,
          Scale = true
        });
        mapData.Symbolisation.Add(pointSym, $"{VelocityData.FieldVelocity} Is Not NULL AND {VelocityData.FieldGeometry} is Point");

        Symbol lineSym = new Symbol(new SymbolPartLine
        {
          ColorExpression = $"{VelocityData.FieldColor}",
          LineWidthExpression = $"{VelocityData.FieldSize}",
          DrawLevel = 0,
          Scale = true
        });
        mapData.Symbolisation.Add(lineSym, $"{VelocityData.FieldVelocity} Is Not NULL AND {VelocityData.FieldSize} Is Not NULL AND {VelocityData.FieldGeometry} is Polyline");

        Symbol areaSym = new Symbol(new SymbolPartArea { ColorExpression = $"{VelocityData.FieldColor}", DrawLevel = 0 });
        mapData.Symbolisation.Add(areaSym, $"{VelocityData.FieldVelocity} Is Not NULL AND {VelocityData.FieldGeometry} is Area");

        _mapData = mapData;
      }
      return _mapData;
    }

    private class VelocityRecord
    {
      public IGeometry Geometry { get; set; }
      public int Symbol { get; set; }
      public double? Velocity { get; set; }
      public double? Size { get; set; }
    }
    private class VelocityData : ISimpleData<VelocityRecord>
    {
      private readonly VeloModelVm _vm;

      public VelocityData(VeloModelVm vm)
      {
        _vm = vm;
      }
      private int Compare(Dictionary<int, SymbolVm> symbols, IComparer<Ocad.ElementIndex> defaultComparer, Ocad.ElementIndex x, Ocad.ElementIndex y)
      {
        if (x.Symbol == y.Symbol)
        { return 0; }

        bool xExists = symbols.TryGetValue(x.Symbol, out SymbolVm xSym);
        bool yExists = symbols.TryGetValue(y.Symbol, out SymbolVm ySym);
        int d = xExists.CompareTo(yExists);
        if (d != 0) return d;
        if (!xExists) return 0;

        int xPrio = xSym.Priority ?? 0;
        int yPrio = ySym.Priority ?? 0;
        d = xPrio.CompareTo(yPrio);
        if (d != 0)
        { return d; }

        d = defaultComparer.Compare(x, y);
        return d;
      }

      private string _mapName;
      private DateTime _lastWriteTime;
      private BoxTree<Ocad.ElementIndex> _idxs;
      private IComparer<Ocad.ElementIndex> _cmp;
      public IEnumerator<VelocityRecord> GetEnumerator(IBox geom)
      {
        if (_mapName != _vm.MapName)
        {
          _mapName = _vm.MapName;
          _lastWriteTime = DateTime.MinValue;
        }
        DateTime lastWriteTime = new System.IO.FileInfo(_vm.MapName).LastWriteTime;
        if (lastWriteTime > _lastWriteTime)
        {
          _lastWriteTime = lastWriteTime;
          _idxs = null;
          _cmp = null;
        }
        Dictionary<int, SymbolVm> symbols = _vm.Symbols.ToDictionary(x => x.Id);
        using (Ocad.OcadReader r = Ocad.OcadReader.Open(_vm.MapName))
        {
          _idxs = _idxs ?? BoxTree.Create(r.GetIndices(), (e) => BoxOp.Clone(e.MapBox));
          _cmp = _cmp ?? r.ColorComparer;

          Box b = new Box(geom);
          yield return new VelocityRecord
          {
            Geometry = new Area(Polyline.Create(new[] { b.Min, new Point2D(b.Min.X, b.Max.Y), b.Max, new Point2D(b.Max.X, b.Min.Y), b.Min })),
            Velocity = _vm.DefaultVelocity
          };

          IBox mapExtent = BoxOp.ProjectRaw(geom, r.Setup.Prj2Map)?.Extent;
          IEnumerable<BoxTree<Ocad.ElementIndex>.TileEntry> search = _idxs.Search(mapExtent);
          List<Ocad.ElementIndex> selIndexes = search.Select(x => x.Value).ToList();
          selIndexes.Sort((x, y) => Compare(symbols, _cmp, x, y));
          foreach (var elem in r.EnumGeoElements(selIndexes))
          {
            if (!symbols.TryGetValue(elem.Symbol, out SymbolVm sym))
            { continue; }

            double? velo = sym?.Velocity;
            if (velo == null)
            { continue; }
            double? size = sym?.Size;
            yield return new VelocityRecord { Geometry = elem.Geometry.GetGeometry(), Symbol = elem.Symbol, Velocity = velo, Size = size };
          }
        }
      }

      public IBox GetExtent()
      {
        Box extent = null;
        using (Ocad.OcadReader r = Ocad.OcadReader.Open(_vm.MapName))
        {
          foreach (var idx in r.GetIndices())
          {
            IBox box = idx.MapBox;
            extent = extent?.Include(box) ?? new Box(box);
          }
          if (extent != null)
          {
            return BoxOp.ProjectRaw(extent, r.Setup.Map2Prj).Extent;
          }
          return null;
        }
      }

      public SchemaColumnsTable GetTableSchema()
      {
        SchemaColumnsTable schema = new SchemaColumnsTable();

        schema.AddSchemaColumn(FieldGeometry, typeof(IGeometry));
        schema.AddSchemaColumn(FieldSymbol, typeof(int));
        schema.AddSchemaColumn(FieldVelocity, typeof(double));
        schema.AddSchemaColumn(FieldSize, typeof(double));

        schema.AddSchemaColumn(FieldColor, typeof(int));

        return schema;
      }

      public const string FieldGeometry = "Geometry";
      public const string FieldSymbol = "Symbol";
      public const string FieldVelocity = "Velocity";
      public const string FieldSize = "Size";
      public const string FieldColor = "Color";

      public object GetValue(VelocityRecord element, string fieldName)
      {
        if (FieldGeometry.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)) return element.Geometry;
        if (FieldSymbol.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)) return (object)element.Symbol ?? DBNull.Value;
        if (FieldVelocity.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)) return (object)element.Velocity ?? DBNull.Value;
        if (FieldSize.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
          return element.Size ?? ((!(element.Geometry is IPoint)) ? DBNull.Value : (object)_vm.DefaultPntSize);
        if (FieldColor.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
        {
          double grey = element.Velocity ?? -1;
          if (grey < 0) return DBNull.Value;
          int gi = (int)(255 * grey);
          // return     a           r          g           b
          int argb = ((255 * 256 + gi) * 256 + gi) * 256 + gi;
          return argb;
        }

        throw new InvalidOperationException($"Unknown field {fieldName}");
      }
    }
  }
}