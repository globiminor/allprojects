using Basics.Geom;
using OCourse.Route;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OCourse.ViewModels;

namespace OCourse.Commands
{
  public class CmdShapeExport : IDisposable
  {
    private OCourseVm _vm;

    public CmdShapeExport(OCourseVm vm)
    {
      _vm = vm;
    }

    public void Dispose()
    { }

    public void Export(string shapeFile)
    {
      DataTable schema = new DataTable();
      schema.Columns.Add("Shape", typeof(Polyline));
      schema.Columns.Add(new DBase.DBaseColumn("From", DBase.ColumnType.String, 5, 0));
      schema.Columns.Add(new DBase.DBaseColumn("To", DBase.ColumnType.String, 5, 0));
      schema.Columns.Add(new DBase.DBaseColumn("Resol", DBase.ColumnType.Double, 8, 2));
      schema.Columns.Add(new DBase.DBaseColumn("Climb", DBase.ColumnType.Double, 8, 2));
      schema.Columns.Add(new DBase.DBaseColumn("Optimal", DBase.ColumnType.Double, 10, 2));
      schema.Columns.Add(new DBase.DBaseColumn("Cost", DBase.ColumnType.Double, 10, 2));

      using (Shape.ShapeWriter writer = new Shape.ShapeWriter(shapeFile, Shape.ShapeType.LineZ, schema))
      {
        RouteCalculator origCalcList = _vm.RouteCalculator;
        Dictionary<CostFromTo, CostFromTo> rawList;
        try
        {
          _vm.SetRouteCalculator(null);
          _vm.RouteCalculator.InitRawCourses(_vm.CourseFile);
          rawList = _vm.RouteCalculator.RouteCostDict;
        }
        finally
        {
          _vm.SetRouteCalculator(origCalcList);
        }

        double resolution = _vm.LcpConfig.Resolution;
        foreach (CostFromTo route in rawList.Keys)
        {
          route.Resolution = resolution;
          CostFromTo calcRoute;
          if (_vm.RouteCalculator.RouteCostDict.TryGetValue(route, out calcRoute))
          {
            writer.Write(calcRoute.Route,
              new object[] {
                route.From.Name, route.To.Name, route.Resolution,
                calcRoute.Climb, calcRoute.OptimalLength, calcRoute.OptimalCost
              });
          }
        }
        writer.Close();
      }
    }
  }
}
