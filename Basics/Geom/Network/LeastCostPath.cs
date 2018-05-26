using System.Collections.Generic;

namespace Basics.Geom.Network
{

  public abstract class LeastCostPath<T> where T: LeastCostPath<T>.NodeInfoBase
  {
    public abstract class NodeInfoBase
    {
      private double _cost;
      private T _from;

      public abstract IEnumerable<T> RawNeighbors { get; }

      public double Cost
      {
        get { return _cost; }
        set { _cost = value; }
      }
      public virtual T FromNode
      {
        get { return _from; }
        set { _from = value; }
      }

      public abstract int CompareWithoutCostTo(T other);
    }

    public class NodeComparer : IComparer<T>
    {
      public int Compare(T field0, T field1)
      { return field0.CompareWithoutCostTo(field1); }
    }

    public class CostComparer : IComparer<T>
    {
      #region IComparer Members

      public int Compare(T x, T y)
      {
        if (x.Cost < y.Cost)
        { return -1; }
        else if (x.Cost > y.Cost)
        { return 1; }
        //int i;
        //i = field0.Cost.CompareTo(field1.Cost);
        //if (i != 0)
        //{ return i; }

        return x.CompareWithoutCostTo(y);
      }

      #endregion
    }

    protected abstract bool PreAssign(T cost);
    protected abstract double Cost(T from, T to);

    protected void BuildTree(T startNode)
    {
      SortedDictionary<T, T> nodes = 
        new SortedDictionary<T, T>(new NodeComparer());
      SortedDictionary<T, double> costs =
        new SortedDictionary<T, double>(new CostComparer());

      startNode.Cost = 0;

      nodes.Add(startNode, startNode);
      costs.Add(startNode, 0);

      T minCost;
      minCost = startNode;

      while (minCost != null)
      {
        costs.Remove(minCost);
        nodes.Remove(minCost);

        if (PreAssign(minCost) == false)
        {
          break;
        }
        AssignNewCosts(minCost, nodes, costs);

        minCost = null;
        foreach (T cost in costs.Keys)
        {
          minCost = cost;
          break;
        }
      }
      nodes.Clear();
      costs.Clear();
    }

    public abstract bool IsFixed(T node);

    private void AssignNewCosts(T startNode,
      SortedDictionary<T, T> nodes,
      SortedDictionary<T, double> costs)
    {
      foreach (T rawNeighbor in startNode.RawNeighbors)
      {
        if (IsFixed(rawNeighbor))
        { continue; }

        if (nodes.TryGetValue(rawNeighbor, out T neighbor) == false)
        {
          neighbor = rawNeighbor;
          nodes.Add(neighbor, neighbor);
        }

        double cost = Cost(startNode, neighbor);
        double costTotal = startNode.Cost + cost;
        if (neighbor == rawNeighbor || costTotal < neighbor.Cost)
        {
          if (neighbor != rawNeighbor)
          {
            costs.Remove(neighbor);
          }
          neighbor.Cost = costTotal;
          neighbor.FromNode = startNode;
          costs.Add(neighbor, costTotal);
        }
      }
    }

  }
}
