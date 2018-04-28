using System.Linq;

namespace Grid.Lcp
{
  public class BlockField<T> : ICostField, ICell
    where T : class, ICostField
  {
    public BlockField(T baseField)
    { BaseField = baseField; }

    public int X { get { return BaseField.X; } }
    public int Y { get { return BaseField.Y; } }
    public double Cost { get { return BaseField.Cost; } }
    public int IdDir { get { return BaseField.IdDir; } }

    public int Ix { get; internal set; }
    public int Iy { get; internal set; }

    public void SetCost(ICostField fromField, double cost, int idDir)
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
      where T : class, ICostField
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

      public override double MinUnitCost => _baseLcp.MinUnitCost;

      protected override BlockField<T> InitField(IField position)
      {
        BlockField<T> field = position as BlockField<T> ?? new BlockField<T>(_baseLcp.InitField(position));

        if (field != position)
        {
          double tx = X0 + position.X * Dx;
          double ty = Y0 + position.Y * Dy;

          _blockGrid.Extent.GetNearest(X0 + position.X * Dx, Y0 + position.Y * Dy, out int ix, out int iy);
          field.Ix = ix;
          field.Iy = iy;
        }

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

        if (!_blockGrid.HasBlocks(startField, neighbors[iField]))
        {
          return baseCost;
        }

        if (step.Distance > 1)
        {
          return baseCost * 1000;
        }

        throw new System.Exception();
        //double factor;
        //_blockGrid.FindWay(startField, neighbors[iField], out factor);
        //return baseCost * factor;
      }

      protected override bool UpdateCost(BlockField<T> field)
      {
        return false;
      }
    }
  }
}
