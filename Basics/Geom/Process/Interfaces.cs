using System.Collections.Generic;

namespace Basics.Geom.Process
{
  public interface ITable
  {
  }
  public interface ISpatialTable : ITable
  {
    IEnumerable<ISpatialRow> Search(IBox extent, string filter);
  }
  public interface ITable<T> : ITable
    where T : IRow
  {
  }
  public interface ISpatialTable<T> : ITable<T>, ISpatialTable
    where T : ISpatialRow
  {
  }
  public interface IRow
  {
    ITable Table { get; }
  }
  public interface ISpatialRow : IRow
  {
    IBox Extent { get; }
  }
}
