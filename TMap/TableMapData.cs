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
        Basics.Data.DbBaseConnection conn = new Ocad.Data.OcadConnection(dataPath);
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
                                              drawable.Extent, Basics.Geom.Relation.Intersect);

      using (new TData.OpenConnection(_data.Connection))
      using (System.Data.Common.DbDataReader reader = _data.GetSelectCmd(query, fields).ExecuteReader())
      {
        while (reader.Read() && !drawable.BreakDraw)
        {
          Symbol symbol;
          System.Data.DataRow data;
          symbol = sym.GetSymbol(reader, out data);
          if (symbol == null)
          {
            continue;
          }

          Basics.Geom.IGeometry geom = (Basics.Geom.IGeometry)data[sym.GeometryColumn.ColumnName];
          foreach (ISymbolPart part in symbol)
          {
            drawable.BeginDraw(part);
            part.Draw(geom, data, drawable);
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
      using (Ocad.OcadReader reader = Ocad.OcadReader.Open(ocadPath))
      {
        Ocad.Setup setup = reader.ReadSetup();
        setup.PrjTrans.X = 0;
        setup.PrjTrans.Y = 0;
        System.Collections.Generic.Dictionary<int, SymbolInfo> symbols = new System.Collections.Generic.Dictionary<int, SymbolInfo>();
        foreach (Ocad.ElementIndex idx in reader.GetIndices())
        {
          SymbolInfo symInfo;
          if (symbols.TryGetValue(idx.Symbol, out symInfo) == false)
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

        System.Collections.Generic.Dictionary<int, Ocad.ColorInfo> colors = new System.Collections.Generic.Dictionary<int, Ocad.ColorInfo>();
        foreach (Ocad.ColorInfo color in reader.ReadColorInfos())
        { colors.Add(color.Nummer, color); }

        string name = System.IO.Path.GetFileNameWithoutExtension(ocadPath);
        TableMapData data = new TableMapData(name, table);

        foreach (Ocad.Symbol.BaseSymbol ocadSymbol in reader.ReadSymbols())
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
              string.Format("Symbol = {0}", ocadSymbol.Number), symInfo.Position, sym);
          }
        }
        data._symbolisation.SymbolList.Table.AcceptChanges();

        return data;
      }
    }

    private static Symbol ToSymbol(Ocad.Symbol.BaseSymbol ocadSymbol, Ocad.Setup setup,
                                   System.Collections.Generic.Dictionary<int, Ocad.ColorInfo> colors)
    {
      Symbol sym = null;
      if (ocadSymbol is Ocad.Symbol.PointSymbol)
      {
        Ocad.Symbol.PointSymbol ocadPoint = (Ocad.Symbol.PointSymbol)ocadSymbol;
        sym = new Symbol(0);
        foreach (Ocad.Symbol.SymbolGraphics graphic in ocadPoint.Graphics)
        {
          SymbolPartPoint part = new SymbolPartPoint(null);
          part.Scale = true;
          part.LineWidth = graphic.LineWidth * Ocad.FileParam.OCAD_UNIT * setup.Scale;
          Ocad.ColorInfo color;
          if (colors.TryGetValue(graphic.Color, out color))
          { part.LineColor = GetColor(color.Color); }
          if (graphic.Geometry is Basics.Geom.Polyline) part.SymbolLine = (Basics.Geom.Polyline)graphic.Geometry.Project(setup.Map2Prj);
          else if (graphic.Geometry is Basics.Geom.Area) part.SymbolLine = ((Basics.Geom.Area)graphic.Geometry).Border[0].Project(setup.Map2Prj);
          else throw new System.NotImplementedException();

          part.DrawLevel = 0;
          sym.Add(part);
        }
      }
      else if (ocadSymbol is Ocad.Symbol.LineSymbol)
      {
        Ocad.Symbol.LineSymbol ocadLine = (Ocad.Symbol.LineSymbol)ocadSymbol;
        sym = new Symbol(1);
        SymbolPartLine part = new SymbolPartLine(null);
        part.Scale = true;
        part.LineWidth = ocadLine.LineWidth * Ocad.FileParam.OCAD_UNIT * setup.Scale;
        Ocad.ColorInfo oc;
        if (!colors.TryGetValue(ocadLine.LineColor, out oc))
        {
          oc = new Ocad.ColorInfo();
          oc.Color = new Ocad.Color(0, 0, 0, 128);
        }
        part.LineColor = GetColor(oc.Color);
        part.DrawLevel = 3;
        sym.Add(part);
      }
      else if (ocadSymbol is Ocad.Symbol.AreaSymbol)
      {
        Ocad.Symbol.AreaSymbol ocadArea = (Ocad.Symbol.AreaSymbol)ocadSymbol;
        sym = new Symbol(2);
        SymbolPartArea part = new SymbolPartArea(null);
        //part.FillColor = GetColor(colors[ocadArea.FillColor].Color);
        Ocad.ColorInfo oc;
        if (!colors.TryGetValue(ocadArea.FillColor, out oc))
        {
          oc = new Ocad.ColorInfo();
          oc.Color = new Ocad.Color(0, 0, 0, 128);
        }
        part.LineColor = GetColor(oc.Color);
        part.DrawLevel = 5;
        sym.Add(part);
      }
      return sym;
    }

    private static System.Drawing.Color GetColor(Ocad.Color ocadColor)
    {
      System.Drawing.Color color = Symbol.CmykToColor(ocadColor.Cyan / 255.0,
                                                      ocadColor.Magenta / 255.0, ocadColor.Yellow / 255.0, ocadColor.Black / 255.0);
      return color;
    }
  }
}