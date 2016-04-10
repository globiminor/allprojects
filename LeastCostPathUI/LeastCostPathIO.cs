using System;
using System.Reflection;
using Grid;
using Basics.Geom;
using Shape;
using Grid.Lcp;

namespace LeastCostPathUI
{
  internal class LcpParams
  {
    public double xs = 0, ys = 0, xe = 0, ye = 0, x0 = 0, y0 = 0, x1 = 0, y1 = 0, dx = 0;
    public string sHeight = null;
    public string sVelo = null;

    public int stepType = -1;
    public string sCostAssembley = null;
    public string sCostMethod = null;

    public string ssCostGrd = null;
    public string ssCostTif = null;
    public string ssDirGrd = null;
    public string ssDirTif = null;

    public string seCostGrd = null;
    public string seCostTif = null;
    public string seDirGrd = null;
    public string seDirTif = null;

    public string sCostGrd = null;
    public string sCostTif = null;

    public string sRouteTif = null;
    public string sRouteShp = null;
    public double maxLonger = 0.2;
    public double minOffset = 0.05;

    public bool bStart = false;
    public bool bEnd = false;

    public bool bFullCalc = true;
    public bool bRouteCalc = true;

    public bool ReadArgs(string[] arg)
    {
      bool bValid = true;
      bool bExtent = false;
      bool bResol = false;
      int i = 0;

      while (i < arg.GetLength(0) && bValid)
      {
        if (arg[i] == "-h")
        {
          sHeight = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-v")
        {
          sVelo = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-E")
        {
          x0 = Convert.ToDouble(arg[i + 1]);
          y0 = Convert.ToDouble(arg[i + 2]);
          x1 = Convert.ToDouble(arg[i + 3]);
          y1 = Convert.ToDouble(arg[i + 4]);
          bExtent = true;
          i += 5;
        }
        else if (arg[i] == "-r")
        {
          dx = Convert.ToDouble(arg[i + 1]);
          bResol = true;
          i += 2;
        }
        else if (arg[i] == "-p")
        {
          stepType = Convert.ToInt32(arg[i + 1]);
          if (stepType != 16 && stepType != 8 && stepType != 4)
          { throw new InvalidOperationException("Invalid step type " + stepType); }
          i += 2;
        }
        else if (arg[i] == "-m")
        {
          sCostAssembley = (arg[i + 1]);
          sCostMethod = (arg[i + 2]);
          i += 3;
        }
        else if (arg[i] == "-s")
        {
          xs = Convert.ToDouble(arg[i + 1]);
          ys = Convert.ToDouble(arg[i + 2]);
          bStart = true;
          i += 3;
        }
        else if (arg[i] == "-sc")
        {
          ssCostGrd = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-sC")
        {
          ssCostTif = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-sd")
        {
          ssDirGrd = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-sD")
        {
          ssDirTif = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-e")
        {
          xe = Convert.ToDouble(arg[i + 1]);
          ye = Convert.ToDouble(arg[i + 2]);
          bEnd = true;
          i += 3;
        }
        else if (arg[i] == "-ec")
        {
          seCostGrd = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-eC")
        {
          seCostTif = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-ed")
        {
          seDirGrd = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-eD")
        {
          seDirTif = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-c")
        {
          sCostGrd = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-C")
        {
          sCostTif = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-rg")
        {
          sRouteTif = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-R")
        {
          sRouteShp = arg[i + 1];
          i += 2;
        }
        else if (arg[i] == "-ro")
        {
          minOffset = Convert.ToDouble(arg[i + 1]);
          i += 2;
        }
        else if (arg[i] == "-rm")
        {
          maxLonger = Convert.ToDouble(arg[i + 1]);
          i += 2;
        }
        else
        {
          throw (new InvalidOperationException(string.Format("Invalid option {0}", arg[i])));
        }
      }


      if (bExtent == false) throw (new InvalidOperationException("Extent not defined"));
      if (bResol == false) throw (new InvalidOperationException("Resolution not defined"));
      string fullCalcError = ValidateFullCalc();
      if (!string.IsNullOrEmpty(fullCalcError))
      {
        bFullCalc = false;
        string routeCalcError = ValidateRouteCalc();
        if (routeCalcError != null)
        {
          bRouteCalc = false;
          throw new InvalidOperationException("Command is not correct");
        }
      }
      return true;
    }

    private string ValidateFullCalc()
    {
      if (sHeight == null) return "Height not defined";
      if (sVelo == null) return "Velocity not defined";
      if ((ssCostGrd != null || ssCostTif != null || ssDirGrd != null || ssDirTif != null) && !bStart)
      {
        return "Start not correctly defined";
      }
      if ((seCostGrd != null || seCostTif != null || seDirGrd != null || seDirTif != null) && !bEnd)
      {
        return "End not correctly defined";
      }
      if ((sCostGrd != null || seCostTif != null) && (!bStart || !bEnd))
      {
        return "Cost not correctly defined";
      }
      return null;
    }

    private string ValidateRouteCalc()
    {
      if ((ssCostGrd == null || ssDirGrd == null))
      {
        return "Start not defined";
      }
      if ((seCostGrd == null || seDirGrd == null) && !bEnd)
      {
        return "End not defined";
      }
      return null;
    }

  }
  /// <summary>
  /// LeastCostPathIO
  /// </summary>
  public class IO
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        WdgLeastCostPath.Main(args);
      }

      LcpParams lcp = new LcpParams();
      try
      {
        lcp.ReadArgs(args);
      }
      catch (Exception exp)
      {
        PrintException(exp);
        PrintUsage();
        Console.WriteLine();
        Console.WriteLine("press <enter>");
        Console.Read();
        return;
      }

      try
      {
        ProcessParams(lcp);
      }
      catch (Exception exp)
      {
        PrintException(exp);
        Console.WriteLine();
        Console.WriteLine("press <enter>");
        Console.Read();
        return;
      }
    }

    private static void ProcessParams(LcpParams lcp)
    {
      LeastCostPath costPath;

      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Common.InitColors(r, g, b);

      DoubleGrid startCostGrid = null;
      IntGrid startDirGrid = null;

      DoubleGrid endCostGrid = null;
      IntGrid endDirGrid = null;

      Steps step;
      if (lcp.stepType < 0) step = Steps.Step16;
      else if (lcp.stepType == 16) step = Steps.Step16;
      else if (lcp.stepType == 8) step = Steps.Step8;
      else if (lcp.stepType == 4) step = Steps.Step4;
      else throw new InvalidOperationException("Unhandled step type " + lcp.stepType);

      Box box = new Box(new Point2D(lcp.x0, lcp.y0), new Point2D(lcp.x1, lcp.y1));
      costPath = new LeastCostPath(box, lcp.dx, step);

      DoubleGrid heightGrid = null;
      if (!string.IsNullOrEmpty(lcp.sHeight))
      { heightGrid = DoubleGrid.FromAsciiFile(lcp.sHeight, 0, 0.01, typeof(double)); }

      if (lcp.bFullCalc)
      {
        DoubleGrid velocityGrid = VelocityGrid.FromImage(lcp.sVelo);

        if (lcp.sCostAssembley != null)
        {
          Assembly ass = Assembly.LoadFile(lcp.sCostAssembley);
          int startMethod = lcp.sCostMethod.LastIndexOf('.');
          string typeName = lcp.sCostMethod.Substring(0, startMethod);
          string methodName = lcp.sCostMethod.Substring(startMethod + 1);
          Type type = ass.GetType(typeName);
          MethodInfo method =
            type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

          costPath.StepCost = (StepCostHandler<double>)
            Delegate.CreateDelegate(typeof(StepCostHandler<double>), method);
        }
        else
        {
          costPath.StepCost = VelocityCostProvider.CostPath_StepCost;
        }

        if (lcp.bStart)
        {
          costPath.Status -= CostPath_Status;
          costPath.Status += CostPath_Status;
          costPath.CalcCost(new Point2D(lcp.xs, lcp.ys), heightGrid, velocityGrid, out startCostGrid, out startDirGrid);
        }
        if (lcp.bEnd)
        {
          costPath.Status -= CostPath_Status;
          costPath.Status += CostPath_Status;
          Point2D pe = new Point2D(lcp.xe, lcp.ye);
          costPath.CalcCost(pe, heightGrid, velocityGrid, out endCostGrid, out endDirGrid, true);
        }
      }
      if (lcp.ssCostGrd != null)
      {
        if (startCostGrid != null)
        { startCostGrid.Save(lcp.ssCostGrd); }
        else
        { startCostGrid = DoubleGrid.FromBinaryFile(lcp.ssCostGrd); }
      }
      if (lcp.ssDirGrd != null)
      {
        if (startDirGrid != null)
        { startDirGrid.Save(lcp.ssDirGrd); }
        else
        { startDirGrid = IntGrid.FromBinaryFile(lcp.ssDirGrd); }
      }
      if (lcp.ssCostTif != null && startCostGrid != null)
      {
        ImageGrid.GridToTif(startCostGrid.ToIntGrid() % 256, lcp.ssCostTif, r, g, b);
      }
      if (lcp.ssDirTif != null && startDirGrid != null)
      {
        // make sure that the start point cell returns a valid value for the step angle array
        ImageGrid.GridToTif(((step[(startDirGrid - 1).Abs() % step.Count] / Math.PI + 1.0) * 128).ToIntGrid(),
                            lcp.ssDirTif, r, g, b);
      }
      if (lcp.seCostGrd != null)
      {
        if (endCostGrid != null)
        { endCostGrid.Save(lcp.seCostGrd); }
        else
        { endCostGrid = DoubleGrid.FromBinaryFile(lcp.seCostGrd); }
      }
      if (lcp.seDirGrd != null)
      {
        if (endDirGrid != null)
        { endDirGrid.Save(lcp.seDirGrd); }
        else
        { endDirGrid = IntGrid.FromBinaryFile(lcp.seDirGrd); }
      }
      if (lcp.seCostTif != null && endCostGrid != null)
      {
        ImageGrid.GridToTif(endCostGrid.ToIntGrid() % 256, lcp.seCostTif, r, g, b);
      }
      if (lcp.seDirTif != null && endDirGrid != null)
      {
        // make sure that the end point cell returns a valid value for the step angle array
        ImageGrid.GridToTif(((step[(endDirGrid - 1).Abs() % step.Count] / Math.PI + 1.0) * 128).ToIntGrid(),
                            lcp.seDirTif, r, g, b);
      }

      if (startCostGrid == null || endCostGrid == null)
      { return; }
      DoubleGrid grdSum = null;
      if (lcp.sCostGrd != null)
      {
        if (grdSum == null) grdSum = GetSum(startCostGrid, endCostGrid, costPath.GetGridExtent());
        grdSum.Save(lcp.sCostGrd);
      }
      if (lcp.sCostTif != null)
      {
        if (grdSum == null) grdSum = GetSum(startCostGrid, endCostGrid, costPath.GetGridExtent());
        ImageGrid.GridToTif((grdSum - grdSum.Min()).ToIntGrid() % 256, lcp.sCostTif, r, g, b);
      }
      if (lcp.sRouteShp != null || lcp.sRouteTif != null)
      {
        if (grdSum == null) grdSum = GetSum(startCostGrid, endCostGrid, costPath.GetGridExtent());
        double maxLengthFactor = lcp.maxLonger + 1;
        double minDiffFactor = lcp.minOffset;
        RouteTable routes = LeastCostPath.CalcBestRoutes(grdSum,
          startCostGrid, startDirGrid, null,
          endCostGrid, endDirGrid, null,
          maxLengthFactor, minDiffFactor, Route_Status);
        if (lcp.sRouteTif != null)
        {
          CreateRouteImage(routes, grdSum.Extent, lcp.sRouteTif);
        }
        if (lcp.sRouteShp != null)
        {
          CreateRouteShapes(routes, grdSum, heightGrid, lcp.sRouteShp);
        }

      }
    }

    private static DoubleGrid GetSum(DoubleGrid startCostGrid, DoubleGrid endCostGrid, GridExtent extent)
    {
      if (extent.EqualExtent(startCostGrid.Extent) && extent.EqualExtent(endCostGrid.Extent))
      {
        return startCostGrid + endCostGrid;
      }
      EGridInterpolation inter = EGridInterpolation.bilinear;
      DoubleGrid sum = new DoubleGrid(extent.Nx, extent.Ny, typeof(double), extent.X0, extent.Y0, extent.Dx);
      for (int ix = 0; ix < extent.Nx; ix++)
      {
        for (int iy = 0; iy < extent.Ny; iy++)
        {
          Point2D p = extent.CellLL(ix, iy);
          sum[ix, iy] = startCostGrid.Value(p.X, p.Y, inter) + endCostGrid.Value(p.X, p.Y, inter);
        }
      }
      return sum;
    }

    private static void PrintException(Exception exp)
    {
      string msg = Basics.Utils.GetMsg(exp);
      ConsoleColor orig = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(msg);
      Console.ForegroundColor = orig;
      Console.WriteLine();
    }

    internal static void CreateRouteImage(RouteTable routes, GridExtent e, string routeName)
    {
      DoubleGrid routeGrid = new DoubleGrid(e.Nx, e.Ny, typeof(double), e.X0, e.Y0, e.Dx);
      bool first = true;
      foreach (RouteTable.Row row in routes.Rows)
      {
        Polyline route = row.Route.Clone();
        if (!first)
        {
          double limit = 25;
          Polyline start = new Polyline();
          Polyline end = new Polyline();
          {
            IPoint p0 = route.Points.First.Value;
            double x0 = p0.X;
            double y0 = p0.Y;
            double dist;
            do
            {
              IPoint pt = route.Points.First.Value;
              double dx = pt.X - x0;
              double dy = pt.Y - y0;
              dist = dx * dx + dy * dy;

              if (dist < limit)
              {
                pt.Z = -1;
                start.Add(pt);
                route.Points.RemoveFirst();
              }
            } while (route.Points.Count > 0 && dist < limit);
          }
          if (route.Points.Count > 0)
          {
            IPoint p1 = route.Points.Last.Value;
            double x1 = p1.X;
            double y1 = p1.Y;
            double dist;
            do
            {
              IPoint pt = route.Points.Last.Value;
              double dx = pt.X - x1;
              double dy = pt.Y - y1;
              dist = dx * dx + dy * dy;
              if (dist < limit)
              {
                pt.Z = -1;
                end.Add(pt);
                route.Points.RemoveLast();
              }
            } while (route.Points.Count > 0 && dist < limit);
          }
          LeastCostPath.Assign(start, routeGrid, RouteValue);
          LeastCostPath.Assign(end, routeGrid, RouteValue);
        }
        first = false;
        LeastCostPath.Assign(route, routeGrid, RouteValue);
      }
      byte[] r = new byte[256];
      byte[] g = new byte[256];
      byte[] b = new byte[256];
      Common.InitColors(r, g, b);
      r[0] = 255;
      g[0] = 255;
      b[0] = 255;
      r[1] = 160;
      g[1] = 160;
      b[1] = 160;

      IntGrid img = new IntGrid(e.Nx, e.Ny, typeof(byte), e.X0, e.Y0, e.Dx);
      for (int ix = 0; ix < e.Nx; ix++)
      {
        for (int iy = 0; iy < e.Ny; iy++)
        {
          double v = routeGrid[ix, iy];
          if (v < 0)
          {
            img[ix, iy] = 1;
          }
          if (v > 0)
          {
            img[ix, iy] = ((int)v) % 254 + 2;
          }
        }
      }

      ImageGrid.GridToTif(img, routeName, r, g, b);
    }

    private static double RouteValue(double value)
    {
      return value;
    }

    internal static void CreateRouteShapes(RouteTable routes, DoubleGrid sum, DoubleGrid heightGrd, string fileName)
    {
      RouteTable copy = new RouteTable();
      foreach (RouteTable.Row row in routes.Rows)
      {
        Polyline grdRoute = row.Route;
        Polyline prjRoute = new Polyline();
        foreach (IPoint grdPt in grdRoute.Points)
        {
          IPoint cell = sum.Extent.CellLL((int)grdPt.X, (int)grdPt.Y);
          Vector prjPt = new Vector(4);
          prjPt.X = cell.X;
          prjPt.Y = cell.Y;
          if (heightGrd != null)
          {
            prjPt.Z = heightGrd.Value(cell.X, cell.Y, EGridInterpolation.bilinear);
          }
          prjPt[3] = grdPt.Z;
          prjRoute.Add(prjPt);

          prjRoute.Add(prjPt);
        }
        copy.AddRow(prjRoute, row.TotalCost, row.Delay);
      }
      System.Data.DataTable schema = new System.Data.DataTable();
      schema.Columns.Add("Shape", typeof(Polyline));
      schema.Columns.Add(new DBase.DBaseColumn(RouteTable.TotalCostColumn.Name, DBase.ColumnType.Double, 8, 2));
      schema.Columns.Add(new DBase.DBaseColumn(RouteTable.DelayColumn.Name, DBase.ColumnType.Double, 8, 2));
      using (ShapeWriter writer = new ShapeWriter(fileName, ShapeType.LineZ, schema))
      {
        foreach (RouteTable.Row row in copy.Rows)
        {
          writer.Write(row.Route, new object[] { row.TotalCost, row.Delay });
        }
      }
    }

    private static void PrintUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine(" {0} -h <heightgrid> -v <velocitygrid>",
        AppDomain.CurrentDomain.FriendlyName);
      Console.WriteLine("-E <xmin> <ymin> <xmax> <ymax> -r <resolution>");
      Console.WriteLine("[-p 16|8|4] [-m <stepcost-assembly> <stepcost-type.method>]");
      Console.WriteLine("[-s <xs> <ys> [-sc <costgrid>] [-sC <costtif>] [-sd <dirgrid>] [-sD <dirTif>]]");
      Console.WriteLine("[-e <xe> <ye> [-ec <costgrid>] [-eC <costtif>] [-ed <dirgrid>] [-eD <dirTif>]");
      Console.WriteLine("[-c <costgrid>] [-C <costtif>]");
      Console.WriteLine("[-rg <routegrid>] [-R <routeshape>] [-ro <minoffset>] [-rm <maxlonger>]]");
      Console.WriteLine();
      Console.WriteLine(" {0} -sc <startcostgrid> -sd <startdirgrid>",
        AppDomain.CurrentDomain.FriendlyName);
      Console.WriteLine("-ec <endcostgrid> -sd <enddirgrid>");
      Console.WriteLine("-E <xmin> <ymin> <xmax> <ymax> -r <resolution>");
      Console.WriteLine("[-c <costgrid>] [-C <costtif>]");
      Console.WriteLine("[-rg <routegrid>] [-R <routeshape>] [-ro <minoffset>] [-rm <maxlonger>]]");
      Console.WriteLine();

      return;
    }

    private static void CostPath_Status(object sender, StatusEventArgs args)
    {
      if (args.CurrentStep % 1000 == 0 || args.CurrentStep == args.TotalStep)
      {
        Console.Write("\rprocessed {0:##0.0} % of {1} cells",
          100.0 * args.CurrentStep / args.TotalStep, args.TotalStep);
        Console.Out.Flush();
      }
    }

    private static void Route_Status(object sender, StatusEventArgs args)
    {
      {
        Console.Write("\rprocessed {0:##0} of {1} routes",
          args.CurrentStep, args.TotalStep);
        Console.Out.Flush();
      }
    }

  }
}
