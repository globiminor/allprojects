using System.Linq;

namespace Grid.Lcp
{
  public class BlockField : ICostField, ICell
  {
    public BlockField(ICostField baseField)
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

    public ICostField BaseField { get; }

    public override string ToString()
    {
      return $"B[{BaseField}]";
    }
  }

  partial class LeastCostGrid
  {
    public class BlockLcp : LeastCostGrid
    {
      private readonly LeastCostGrid _baseLcp;
      private readonly BlockGrid _blockGrid;
      public BlockLcp(LeastCostGrid baseLcp, BlockGrid blockGrid)
        : base(baseLcp.DirCostModel, baseLcp.Dx, baseLcp.GetGridExtent().Extent, baseLcp.Steps)
      {
        _baseLcp = baseLcp;
        _blockGrid = blockGrid;
      }

      private BlockField _startField;
      private ICostField[] _baseFields;
      protected double CalcCost(BlockField startField, Step step, int iField,
        BlockField[] neighbors, bool invers)
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

      protected bool UpdateCost(BlockField field)
      {
        return false;
      }
    }
  }
}
