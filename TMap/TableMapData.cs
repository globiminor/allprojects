using Ocad;

namespace TMap
{
  public class TableMapData : GraphicMapData
  {
    TData.TTable _data;
    private VectorSymbolisation _symbolisation;

    public static new TableMapData FromFile(string dataPath)
    {
      TableMapData mapData;
      if (System.IO.Path.GetExtension(dataPath) == ".ocd" &&
          Ocad.OcadReader.Exists(dataPath))
      {
        Ocad.Data.OcadConnection conn = new Ocad.Data.OcadConnection(dataPath) { SorterProvider = (r) => r.ColorComparer.Compare };
        string table = Ocad.Data.OcadConnection.TableElements;
        TData.TTable tTable = new TData.TTable(table, conn);
        mapData = FromOcad(dataPath, tTable);
        return mapData;
      }

      TData.TTable data = TData.TTable.FromFile(dataPath);
      if (data == null)
      { return null; }

      mapData = new TableMapData(System.IO.Path.GetFileNameWithoutExtension(dataPath),
                                 data);

      return mapData;
    }

    /// <summary>
    /// Create MapData with Data from dataPath
    /// </summary>
    public static TableMapData FromData(object data)
    {
      TableMapData mapData = new TableMapData("Table", TData.TTable.FromData(data));
      return mapData;
    }

    public TableMapData(string name, TData.TTable data)
      : base(name, data)
    {
      _data = data;
      _symbolisation = new VectorSymbolisation(_data.DefaultGeometryColumn);
    }

    public TData.TTable Data
    {
      get { return _data; }
    }

    public VectorSymbolisation Symbolisation
    {
      get { return _symbolisation; }
    }

    public override void Draw(IDrawable drawable)
    {
      VectorSymbolisation sym = _symbolisation;

      string fields = sym.NeededFields;

      TData.GeometryQuery query = new TData.GeometryQuery(sym.GeometryColumn,
                                              drawable.Extent, Basics.Geom.Relation.ExtentIntersect);

      using (new TData.OpenConnection(_data.Connection))
      using (System.Data.Common.DbDataReader reader = _data.GetSelectCmd(query, fields).ExecuteReader())
      {
        while (reader.Read() && !drawable.BreakDraw)
        {
          Symbol symbol = sym.GetSymbol(reader, out System.Data.DataRow data);
          if (symbol == null)
          {
            continue;
          }

          Basics.Geom.IGeometry geom = (Basics.Geom.IGeometry)data[sym.GeometryColumn.ColumnName];
          foreach (var part in symbol)
          {
            part.SetProperties(data);
            drawable.BeginDraw(part, data);
            part.Draw(geom, drawable);
            drawable.EndDraw(part);
          }
        }
      }
    }

    private class SymbolInfo
    {
      public Symbol Symbol;
      public int Count;
      public int Position;
      public static int CountCompare(SymbolInfo x, SymbolInfo y)
      { return x.Count.CompareTo(y.Count); }
    }
    private static TableMapData FromOcad(string ocadPath, TData.TTable table)
    {
      using (OcadReader reader = Ocad.OcadReader.Open(ocadPath))
      {
        Setup setup = reader.ReadSetup();
        setup.PrjTrans.X = 0;
        setup.PrjTrans.Y = 0;
        System.Collections.Generic.Dictionary<int, SymbolInfo> symbols = new System.Collections.Generic.Dictionary<int, SymbolInfo>();
        foreach (var idx in reader.GetIndices())
        {
          if (symbols.TryGetValue(idx.Symbol, out SymbolInfo symInfo) == false)
          {
            symInfo = new SymbolInfo();
            symbols.Add(idx.Symbol, symInfo);
          }
          symInfo.Count++;
        }

        System.Collections.Generic.List<SymbolInfo> symbolInfos = new System.Collections.Generic.List<SymbolInfo>(symbols.Values);
        symbolInfos.Sort(SymbolInfo.CountCompare);
        for (int i = symbolInfos.Count - 1; i >= 0; i--)
        { symbolInfos[i].Position = -i - 1; }

        System.Collections.Generic.Dictionary<int, ColorInfo> colors = new System.Collections.Generic.Dictionary<int, ColorInfo>();
        foreach (var color in reader.ReadColorInfos())
        { colors.Add(color.Nummer, color); }

        string name = System.IO.Path.GetFileNameWithoutExtension(ocadPath);
        TableMapData data = new TableMapData(name, table);

        foreach (var ocadSymbol in reader.ReadSymbols())
        {
          if (symbols.ContainsKey(ocadSymbol.Number) == false)
          {
            continue;
          }
          Symbol sym = ToSymbol(ocadSymbol, setup, colors);
          if (sym != null)
          {
            SymbolInfo symInfo = symbols[ocadSymbol.Number];
            symInfo.Symbol = sym;

            data._symbolisation.SymbolList.Table.Rows.Add(
              $"{Ocad.Data.OcadConnection.FieldSymbol} = {ocadSymbol.Number}", symInfo.Position, sym);
          }
        }
        data._symbolisation.SymbolList.Table.AcceptChanges();

        return data;
      }
    }

    private static Symbol ToSymbol(Ocad.Symbol.BaseSymbol ocadSymbol, Setup setup,
                                   System.Collections.Generic.Dictionary<int, ColorInfo> colors)
    {
      Symbol sym = null;
      if (ocadSymbol is Ocad.Symbol.PointSymbol ocadPoint)
      {
        sym = new Symbol(0);
        foreach (var graphic in ocadPoint.Graphics)
        {
          SymbolPartPoint part = new SymbolPartPoint();
          part.Scale = true;
          part.LineWidth = graphic.LineWidth * FileParam.OCAD_UNIT * setup.Scale;
          if (colors.TryGetValue(graphic.Color, out ColorInfo color))
          { part.Color = GetColor(color.Color); }
          if (graphic.MapGeometry is GeoElement.Line l) part.SymbolLine = l.BaseGeometry.Project(setup.Map2Prj);
          else if (graphic.MapGeometry is GeoElement.Area a)
          {
            part.SymbolLine = a.BaseGeometry.Border[0].Project(setup.Map2Prj);
            part.Fill = true;
            part.Stroke = false;
            part.Color = part.Color;
          }
          else throw new System.NotImplementedException();
          part.RotateExpression = Ocad.Data.OcadConnection.FieldAngle;

          part.DrawLevel = 0;
          sym.Add(part);
        }
      }
      else if (ocadSymbol is Ocad.Symbol.LineSymbol ocadLine)
      {
        sym = new Symbol(1);
        SymbolPartLine part = new SymbolPartLine();
        part.Scale = true;
        part.LineWidth = ocadLine.LineWidth * FileParam.OCAD_UNIT * setup.Scale;
        if (!colors.TryGetValue(ocadLine.LineColor, out ColorInfo oc))
        {
          oc = new ColorInfo();
          oc.Color = new Color(0, 0, 0, 128);
        }
        part.Color = GetColor(oc.Color);
        part.DrawLevel = 0; // 3;
        sym.Add(part);
      }
      else if (ocadSymbol is Ocad.Symbol.AreaSymbol ocadArea)
      {
        sym = new Symbol(2);
        SymbolPartArea part = new SymbolPartArea();
        //part.FillColor = GetColor(colors[ocadArea.FillColor].Color);
        if (!colors.TryGetValue(ocadArea.FillColor, out ColorInfo oc))
        {
          oc = new ColorInfo();
          oc.Color = new Color(0, 0, 0, 128);
        }
        part.Color = GetColor(oc.Color);
        part.DrawLevel = 0; // 5;
        sym.Add(part);
      }
      return sym;
    }

    private static System.Drawing.Color GetColor(Color ocadColor)
    {
      System.Drawing.Color color = Ocad.Color.CmykToColor(ocadColor.Cyan / 255.0,
                                                      ocadColor.Magenta / 255.0, ocadColor.Yellow / 255.0, ocadColor.Black / 255.0);
      return color;
    }
  }
}