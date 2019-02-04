using OcadScratch.ViewModels;
using System;

namespace OcadScratch.Commands
{
  class CmdNextElem : IDisposable
  {
    private MapVm _mapVm;
    private WorkElemVm _elemVm;

    public CmdNextElem(MapVm mapVm, WorkElemVm elemVm)
    {
      _mapVm = mapVm;
      _elemVm = elemVm;
    }

    public string Error { get; private set; }
    public bool SetHandled { get; set; }
    public WorkElemVm NextElem { get; private set; }

    public void Dispose()
    { }

    public bool Execute()
    {
      Error = null;
      NextElem = null;

      if (_mapVm == null)
      {
        Error = "Map not set";
        return false;
      }
      if (_elemVm == null)
      {
        Error = "Elem not set";
        return false;
      }

      if (SetHandled)
      {
        _elemVm.Handled = true;
      }

      Basics.Geom.IBox ext0 = _elemVm.GetElem()?.Geometry?.GetGeometry().Extent;
      Basics.Geom.Point p0 = 2 * Basics.Geom.PointOp.Add(ext0.Min, ext0.Max);

      WorkElemVm next = null;
      double minD2 = double.MaxValue;
      foreach (var candidate in _mapVm.Elems)
      {
        if (candidate.Handled || candidate == _elemVm)
        { continue; }

        Basics.Geom.IBox ext = candidate.GetElem()?.Geometry?.GetGeometry().Extent;
        Basics.Geom.Point p = 2 * Basics.Geom.PointOp.Add(ext.Min, ext.Max);

        double d2 = p.Dist2(p0);
        if (d2 < minD2)
        {
          next = candidate;
          minD2 = d2;
        }
      }

      if (next == null)
      {
        Error = "Kein nächstes Element gefunden";
        return false;
      }

      NextElem = next;
      return true;
    }
  }
}
