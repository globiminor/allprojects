
using Basics.Geom;
using Grid.Lcp;
using System;
using TMap;

namespace LeastCostPathUI
{
  public class LeastCostPathCmd : ICommand
  {
    private WdgLeastCostPath _wdg;

    public LeastCostPathCmd()
    {
      // "AssemblyResolve" is active only here
      // --> load all potentially needed libraries here
      _wdg = new WdgLeastCostPath();
      WdgLeastCostPath.Init();
    }

    public void Execute(IContext context)
    {
      if (_wdg == null)
      {
        _wdg = new WdgLeastCostPath();
        WdgLeastCostPath.Init();
      }
      _wdg.TMapContext = context;
      if (context is System.Windows.Forms.IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
    }

    public static void ShowInTMap(IContext context, LeastCostGrid lcp, ref GridMapData gridData, StatusEventArgs args)
    {
      if (context == null)
      { return; }
      if (lcp == null)
      { return; }

      if (!(args.CurrentStep % 4096 == 0))
      { return; }

      if (gridData == null)
      {
        gridData = GridMapData.FromData(args.DirGrid);
        LookupSymbolisation sym = new LookupSymbolisation();

        sym.Colors.Clear();

        sym.Colors.Add(null);
        for (int iCol = 0; iCol < lcp.Steps.Count; iCol++)
        {
          double angle = lcp.Steps.GetStep(iCol).Angle;

          int r = ColorVal(angle, 0);
          int g = ColorVal(angle, Math.PI * 2.0 / 3.0);
          int b = ColorVal(angle, -Math.PI * 2.0 / 3.0);
          double max = Math.Max(r, Math.Max(g, b));
          r = (int)(255 * r / max);
          g = (int)(255 * g / max);
          b = (int)(255 * b / max);
          sym.Colors.Add(System.Drawing.Color.FromArgb(r, g, b));
        }
        sym.Colors.Add(null);

        gridData.Symbolisation = sym;
      }
      else
      {
        if (gridData.Data.BaseData != args.DirGrid)
        {
          gridData.Data.BaseData = args.DirGrid;
        }
      }

      context.Maps[0].Draw(null);
      context.Maps[0].Draw(gridData);

      context.Maps[0].Flush();
    }

    private static int ColorVal(double angle, double zero)
    {
      angle = angle - zero;
      if (angle > Math.PI)
      { angle -= Math.PI * 2; }
      if (angle < -Math.PI)
      { angle += Math.PI * 2; }
      double c = (1.0 - Math.Abs(angle) / (2.0 * Math.PI / 3.0));
      if (c < 0)
      { c = 0; }
      c = (1 - (1 - c) * (1 - c));
      return (int)(255 * c);
    }

  }
}

namespace LeastCostPathUI
{
  partial class WdgLeastCostPath
  {
    private GridMapData _thisGridData;
    private IContext _context;

    private static int _initTMap = InitTMapStatic();
    private static int InitTMapStatic()
    {
      InitWdg -= InitTMap;
      InitWdg += InitTMap;
      return 1;
    }


    private static void InitTMap(WdgLeastCostPath wdg)
    {
      if (_initTMap != 1)
      { Basics.Logger.Warn(() => "Why am I here"); }

      wdg.cntOutput.StatusChanged += wdg.ShowInTMap;
    }

    public IContext TMapContext
    {
      get { return _context; }
      set { _context = value; }
    }

    private void ShowInTMap(object sender, StatusEventArgs args)
    {
      LeastCostPathCmd.ShowInTMap(_context, sender as LeastCostGrid, ref _thisGridData, args);
    }
  }
}

