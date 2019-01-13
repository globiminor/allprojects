using Basics.Geom;
using Grid;
using Grid.Lcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using System;
using System.Collections.Generic;

namespace OcadTest
{
  [TestClass]
  public class LcpTest
  {
    [TestMethod]
    public void TestStart()
    {
      LeastCostPathUI.IO.Main(new string[]
                                {
                                 "-h"
    });
    }
    [TestMethod]
    public void TestSymbolGrid()
    {
      using (OcadReader reader = OcadReader.Open(
        @"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\R�mlangerwald_Arnet_110619.ocd"))
      {
        Box all = null;
        foreach (var element in reader.EnumGeoElements(null))
        {
          if (all == null)
          {
            all = new Box(element.Geometry.Extent);
          }
          else
          { all.Include(element.Geometry.Extent); }
        }
        if (all == null)
        { return; }
        GridExtent ext = new GridExtent(
          (int)(all.Max.X - all.Min.X + 2), (int)(all.Max.Y - all.Min.Y + 2),
          all.Min.X - 1, all.Max.Y + 1, 1);
        BaseGrid<List<int>> symGrid = new SymbolGrid(ext);

        Dictionary<int, double> symWidths = new Dictionary<int, double>();
        foreach (var ocadSymbol in reader.ReadSymbols())
        {
          if (symWidths.ContainsKey(ocadSymbol.Number) == false)
          {
            continue;
          }
          if (ocadSymbol is Ocad.Symbol.LineSymbol linSym)
          {
            symWidths.Add(ocadSymbol.Number, linSym.LineWidth);
          }
        }


        foreach (var element in reader.EnumGeoElements(null))
        {
          IEnumerable<int[]> cells = null;
          if (element.Geometry is Area area)
          {
            area.CloseBorders();
            cells = symGrid.EnumerateCells(area);
          }
          else if (element.Geometry is Polyline line)
          {
            if (symWidths.TryGetValue(element.Symbol, out double symWidth)
              && symWidth > 0)
            {
              cells = symGrid.EnumerateCells(line, symWidth, false);
            }
          }

          if (cells == null)
          { continue; }

          foreach (var cell in cells)
          {
            int ix = cell[0];
            int iy = cell[1];
            List<int> syms = symGrid[ix, iy];
            if (syms == null)
            {
              syms = new List<int>();
              symGrid[ix, iy] = syms;
            }
            if (!syms.Contains(element.Symbol))
            {
              syms.Add(element.Symbol);
            }
          }
        }
      }
    }

    [TestMethod]
    public void TestBlockGrid()
    {
      DoubleGrid grdHeight = DataDoubleGrid.FromAsciiFile(@"C:\daten\felix\kapreolo\karten\opfikon\2013\opfikon.asc", 0, 0.01, typeof(double));
      VelocityGrid grdVelo = VelocityGrid.FromImage(@"C:\daten\felix\kapreolo\karten\opfikon\2018\Opfikon_2018_velo.tif");
      BlockGrid block = BlockGrid.Create(grdVelo);

      grdVelo.MinVelo = 0.3;

      LeastCostGrid<TvmCell> lcp = new LeastCostGrid<TvmCell>(new TerrainVeloModel(grdHeight, grdVelo), 5.0
        //, new Box(new Point2D(2685260, 1252920), new Point2D(2685360, 1253010))
        );

      LeastCostGrid.BlockLcp blcp = new LeastCostGrid.BlockLcp(lcp, block);
      blcp.CalcCost(new Point2D(2685302, 1252930));
    }

    [TestMethod]
    public void TestBlockLcp()
    {
      DoubleGrid grdHeight = DataDoubleGrid.FromAsciiFile(@"C:\daten\felix\kapreolo\karten\opfikon\2013\opfikon.asc", 0, 0.01, typeof(double));
      VelocityGrid grdVelo = VelocityGrid.FromImage(@"C:\daten\felix\kapreolo\karten\opfikon\2018\Opfikon_2018_velo.tif");
      BlockGrid block = BlockGrid.Create(grdVelo);

      grdVelo.MinVelo = 0.3;

      LeastCostGrid<BlockCell> lcp = new LeastCostGrid<BlockCell>(new BlockModell(block), block.Extent.Dx
        //, new Box(new Point2D(2685260, 1252920), new Point2D(2685360, 1253010))
        );

      lcp.CalcCost(new Point2D(2685302, 1252930));
    }

    private class SymbolGrid : BaseGrid<List<int>>
    {
      private Array _value;

      public SymbolGrid(GridExtent extent)
        : base(extent)
      {
        _value = Array.CreateInstance(typeof(List<int>), extent.Nx, extent.Ny);
      }

      public override List<int> this[int ix, int iy]
      {
        get { return (List<int>)_value.GetValue(ix, iy); }
        set { _value.SetValue(value, ix, iy); }
      }
    }
  }
}
