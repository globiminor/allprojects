
using Basics.Geom;
using System.Linq;

namespace Grid.Lcp
{
  public class BlockField<T> : IField
    where T : class, IField
  {
    public BlockField(T baseField)
    { BaseField = baseField; }

    public int X { get { return BaseField.X; } }
    public int Y { get { return BaseField.Y; } }
    public double Cost { get { return BaseField.Cost; } }
    public int IdDir { get { return BaseField.IdDir; } }

    public void SetCost(IField fromField, double cost, int idDir)
    {
      BaseField.SetCost(fromField, cost, idDir);
    }

    public T BaseField { get; }

    public override string ToString()
    {
      return $"B[{BaseField}]";
    }
  }

  partial class LeastCostPath<T>
      where T : class, IField
  {
    public class BlockLcp : LeastCostPath<BlockField<T>>
    {
      private readonly LeastCostPath<T> _baseLcp;
      private readonly BlockGrid _blockGrid;
      public BlockLcp(LeastCostPath<T> baseLcp, BlockGrid blockGrid)
        : base(baseLcp.GetGridExtent().Extent, baseLcp.Dx, baseLcp.Steps)
      {
        _baseLcp = baseLcp;
        _blockGrid = blockGrid;
      }

      protected override BlockField<T> InitField(IField position)
      {
        BlockField<T> field = position as BlockField<T> ?? new BlockField<T>(_baseLcp.InitField(position));
        return field;
      }

      private BlockField<T> _startField;
      private T[] _baseFields;
      protected override double CalcCost(BlockField<T> startField, Step step, int iField,
        BlockField<T>[] neighbors, bool invers)
      {
        if (_startField != startField)
        {
          _baseFields = null;
          _startField = startField;
        }

        _baseFields = _baseFields ?? neighbors.Select(x => x?.BaseField).ToArray();
        double baseCost = _baseLcp.CalcCost(startField.BaseField, step, iField, _baseFields, invers);

        return baseCost;
      }

      protected override bool UpdateCost(BlockField<T> field)
      {
        return false;
      }
    }
  }
}
