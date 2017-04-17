
using Basics.Geom.Projection;

namespace OcadScratch.ViewModels
{
  public class ProjectionVm
  {
    private readonly Projection _map;

    private TransferProjection _map2Gg;
    private TransferProjection _gg2map;

    public ProjectionVm(string name, Projection map)
    {
      Name = name;
      _map = map;
    }
    public string Name { get; }

    public override string ToString()
    {
      return Name;
    }

    public int OcadId { get; set; }

    public TransferProjection Map2Geo
    {
      get { return _map2Gg ?? (_map2Gg = new TransferProjection(_map, Geographic.Wgs84())); }
    }
    public TransferProjection Geo2Map
    {
      get { return _gg2map ?? (_gg2map = new TransferProjection(Geographic.Wgs84(), _map)); }
    }
  }
}
