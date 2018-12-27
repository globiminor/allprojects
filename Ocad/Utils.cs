
using System.Collections.Generic;
using Basics.Geom;

namespace Ocad
{
  public static class Utils
  {
    public static IList<Polyline> Split(Polyline line, GeometryCollection symbol,
      IPoint position, double angle)
    {
      GeometryCollection symPrj = SymbolGeometry(symbol, position, angle);

      IList<ParamGeometryRelation> cuts =
        GeometryOperator.CreateRelations(line, symPrj, new TrackOperatorProgress());

      if (cuts == null)
      {
        return null;
      }
      return line.Split(cuts);
    }

    public static GeometryCollection SymbolGeometry(GeometryCollection symbol,
      IPoint position, double angle)
    {
      Setup setup = new Setup();
      setup.PrjTrans.X = position.X;
      setup.PrjTrans.Y = position.Y;
      setup.Scale = 1 / FileParam.OCAD_UNIT;
      setup.PrjRotation = -angle;// +_templateSetup.PrjRotation;

      GeometryCollection prjList = new GeometryCollection();
      foreach (var geometry in symbol)
      {
        IGeometry gprj = geometry.Project(setup.Map2Prj);
        prjList.Add(gprj);
      }
      return prjList;
    }

  }
}
