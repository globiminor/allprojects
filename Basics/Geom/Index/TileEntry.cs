
namespace Basics.Geom.Index
{
  public abstract class TileEntry
  {
    protected TileEntry(IBox box, object value)
    {
      Box = box;
      BaseValue = value;
    }

    public IBox Box { get; }
    public object BaseValue { get; }
    public override string ToString()
    {
      return $"{BoxOp.ToString(Box, "N3")}: {BaseValue}";
    }
  }
}
