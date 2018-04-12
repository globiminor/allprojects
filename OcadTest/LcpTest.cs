using System;
using System.Collections.Generic;
using Grid;
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using Grid.Lcp;

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
        @"D:\daten\felix\kapreolo\karten\ruemlangerwald\2011\Rümlangerwald_Arnet_110619.ocd"))
      {
        Box all = null;
        foreach (Element element in reader.Elements(true, null))
        {
          if (all == null)
          {
            IBox box = element.Geometry.Extent;
            all = new Box(box.Min, box.Max);
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
        foreach (Ocad.Symbol.BaseSymbol ocadSymbol in reader.ReadSymbols())
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


        foreach (Element element in reader.Elements(true, null))
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

          foreach (int[] cell in cells)
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

      HeightVeloLcp lcp = new HeightVeloLcp(new Box(new Point2D(2685260, 1252920), new Point2D(2685360, 1253010)), 5.0)
      {
        HeightGrid = grdHeight,
        VelocityGrid = grdVelo
      };

      HeightVeloLcp.BlockLcp blcp = new HeightVeloLcp.BlockLcp(lcp, block);
      blcp.CalcCost(new Point2D(2685302, 1252930), out DataDoubleGrid costGrid, out IntGrid dirGrid);
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
