using System;
using System.Collections.Generic;

namespace Basics.Geom.Index
{
  public abstract class BoxTile
  {
    private BoxTile _parent;
    private BoxTile _child0;
    private BoxTile _child1;
    private int _splitDimension;
    private int _count;

    private double _minSplitDim;
    private double _maxSplitDim;
    private double _minParentDim;
    private double _maxParentDim;

    //private static int _currentId;
    //private readonly int _id;
    //protected BoxTile()
    //{
    //  _currentId++;
    //  _id = _currentId;
    //}

    public double MinInParentSplitDim
    {
      get { return _minParentDim; }
      set { _minParentDim = value; }
    }

    public double MaxInParentSplitDim
    {
      get { return _maxParentDim; }
      set { _maxParentDim = value; }
    }

    public int Count
    {
      get { return _count; }
      internal set { _count = value; }
    }

    public double MinInSplitDim
    {
      get { return _minSplitDim; }
      private set { _minSplitDim = value; }
    }

    public double MaxInSplitDim
    {
      get { return _maxSplitDim; }
      private set { _maxSplitDim = value; }
    }

    public BoxTile Parent
    {
      get { return _parent; }
      private set { _parent = value; }
    }

    public BoxTile Child0
    {
      get { return _child0; }
    }

    public BoxTile Child1
    {
      get { return _child1; }
    }

    public int SplitDimension
    {
      get { return _splitDimension; }
      set { _splitDimension = value; }
    }

    internal abstract BoxTile CreateEmptyTile();

    protected abstract IEnumerable<TileEntry> InitSplit();

    public abstract IEnumerable<TileEntry> EnumElems();

    internal abstract int ElemsCount { get; }

    internal void InitChildren(BoxTile child0, BoxTile child1, double u0, double u,
             int counter0, int size, int denominator)
    {
      _child0 = child0;
      _child0.Parent = this;
      _child0.MinInParentSplitDim = Position(u0, u, counter0, denominator);
      //== _minSplitDim;
      _child0.MaxInParentSplitDim = Position(u0, u, counter0 + 2 * size, denominator);
      //(_maxSplitDim + _minSplitDim) / 2.0;

      // double d;
      _child1 = child1;
      _child1.Parent = this;
      // d = (_maxSplitDim - _minSplitDim) / 4.0;
      _child1.MinInParentSplitDim = Position(u0, u, counter0 + size, denominator);
      //_minSplitDim + d;
      _child1.MaxInParentSplitDim = Position(u0, u, counter0 + 3 * size, denominator);
      //_maxSplitDim - d;
    }

    /// <summary>
    /// Split the tile along the largest dimesion of box
    /// Assumption: extent represents the actual extent of the tile (see method BoxExtent in BoxTree)
    /// </summary>
    public void Split(Box unit, IList<int> counter0, IList<int> denominator,
          IList<int> mainCounter, IList<int> mainSize)
    {
      int dim = counter0.Count;

      if (_child0 != null)
      {
        throw new InvalidProgramException("child0 already initialized");
      }
      if (_child1 != null)
      {
        throw new InvalidProgramException("child1 already initialized");
      }

      double d0 = (unit.Max[0] - unit.Min[0]) * mainSize[0] / denominator[0];
      int splitDim = 0;
      for (int i = 1; i < dim; i++)
      {
        double d = (unit.Max[i] - unit.Min[i]) * mainSize[i] / denominator[i];
        if (d0 < d)
        {
          d0 = d;
          splitDim = i;
        }
      }
      double u0 = unit.Min[splitDim];
      double u = unit.Max[splitDim] - unit.Min[splitDim];

      //TODO: verify
      _splitDimension = splitDim;
      int c0 = 2 * mainSize[splitDim] * counter0[splitDim] +
         mainCounter[splitDim] * denominator[splitDim];
      MinInSplitDim = Position(u0, u, c0, denominator[splitDim]);
      MaxInSplitDim = Position(u0, u, c0 + 4 * mainSize[splitDim], denominator[splitDim]);

      InitChildren(CreateEmptyTile(), CreateEmptyTile(),
           u0, u, c0, mainSize[splitDim], denominator[splitDim]);

      if (_child0 == null)
      {
        throw new InvalidProgramException("child0 not initiatlized");
      }
      if (_child1 == null)
      {
        throw new InvalidProgramException("child1 not initiatlized");
      }

      double tx1 = _child0.MaxInParentSplitDim;

      double ux0 = _child1.MinInParentSplitDim;
      double ux1 = _child1.MaxInParentSplitDim;

      int origCount = Count;

      _count = 0;

      foreach (var entry in InitSplit())
      {
        double dMin = entry.Box.Min[splitDim];
        double dMax = entry.Box.Max[splitDim];
        if (dMin < ux0)
        {
          if (dMax < tx1)
          {
            _child0.Add(entry, true);
          }
          else
          {
            Add(entry, true);
          } // TODO: do not add directly to mElemList (sorting in future);
        }
        else
        {
          if ((dMin <= tx1) == false)
          {
            BoxTile child = this;
            BoxTile parent = _parent;
            while (parent != null)
            {
              if (parent.SplitDimension == splitDim && parent.Child0 == child)
              {
                throw new InvalidProgramException("dMin <= tx1");
              }
              child = parent;
              parent = parent.Parent;
            }
          }
          if (dMax < ux1)
          {
            _child1.Add(entry, true);
          }
          else
          {
            Add(entry, true);
          }
        }
      }

      _child0.Count = _child0.ElemsCount;
      _child1.Count = _child1.ElemsCount;
      _count = _child0.Count + _child1.Count + ElemsCount;
    }

    protected abstract void AddCore(TileEntry entry);

    protected abstract void AddRange(BoxTile child);

    protected void Add(TileEntry entry, bool splitting)
    {
      // TODO: implement sorting
      AddCore(entry);

      if (splitting)
      {
        return;
      }
      _count++;

      BoxTile parent = Parent;
      while (parent != null)
      {
        parent.Count++;
        parent = parent.Parent;
      }
    }

    public void Collapse()
    {
      if ((_child0.Child0 == null) == false)
      {
        throw new InvalidProgramException("_child0.child0 == null");
      }
      if ((_child1.Child0 == null) == false)
      {
        throw new InvalidProgramException("_child1.child0 == null");
      }

      AddRange(_child0);
      AddRange(_child1);

      _child0 = null;
      _child1 = null;
    }

    internal bool VerifyPointTile(Box mainBox)
    {
      IBox extent = TileExtent(mainBox);
      foreach (var entry in EnumElems())
      {
        if (BoxOp.Contains(extent, entry.Box) == false)
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Get the Extent of Tile
    /// </summary>
    public Box TileExtent(Box mainBox)
    {
      Box box = mainBox;
      BoxTile child = this;
      BoxTile parent = child.Parent;
      double d = -1;
      double dMax = 0;

      // proceed up  until it is verified that all dimensions of the 
      // box are set to the correct extent
      while (parent != null && d <= dMax)
      {
        int splitDim = parent.SplitDimension;

        d = child.MaxInParentSplitDim - child.MinInParentSplitDim;
        if (box.Min[splitDim] < child.MinInParentSplitDim)
        {
          box.Min[splitDim] = child.MinInParentSplitDim;
        }
        if (box.Max[splitDim] > child.MaxInParentSplitDim)
        {
          box.Max[splitDim] = child.MaxInParentSplitDim;
        }

        if (dMax == 0)
        {
          dMax = parent.MaxInSplitDim - parent.MinInSplitDim;
          // dMax was the largest extent when splitting the parent of tile,
          // maybe together with others.
          // So if d > dMax, it must be dimension, that was split already
        }

        child = parent;
        parent = parent.Parent;
      }
      return box;
    }

    public static double Position(double u0, double u, int counter, int denominator)
    {
      while (denominator > 1 && (counter & 1) == 0)
      {
        denominator /= 2;
        counter /= 2;
      }
      double pos = u0 + counter * u / denominator;
      return pos;
    }

  }

  internal sealed class BoxTile<T> : BoxTile
  {
    private List<TileEntry<T>> _elemList = new List<TileEntry<T>>();

    public new BoxTile<T> Parent
    {
      get { return (BoxTile<T>)base.Parent; }
    }

    public new BoxTile<T> Child0
    {
      get { return (BoxTile<T>)base.Child0; }
    }

    public new BoxTile<T> Child1
    {
      get { return (BoxTile<T>)base.Child1; }
    }

    internal override BoxTile CreateEmptyTile()
    {
      return new BoxTile<T>();
    }

    protected override void AddCore(TileEntry entry)
    {
      _elemList.Add((TileEntry<T>)entry);
    }

    protected override IEnumerable<TileEntry> InitSplit()
    {
      List<TileEntry<T>> toSplit = _elemList;
      _elemList = new List<TileEntry<T>>();
      foreach (var splitEntry in toSplit)
      {
        yield return splitEntry;
      }
    }

    public override IEnumerable<Index.TileEntry> EnumElems()
    {
      foreach (var entry in _elemList)
      {
        yield return entry;
      }
    }

    public List<TileEntry<T>> ElemList
    {
      get { return _elemList; }
    }

    internal override int ElemsCount
    {
      get { return _elemList.Count; }
    }

    public TileEntry this[int index]
    {
      get
      {
        if (index < _elemList.Count)
        {
          return _elemList[index];
        }
        index -= _elemList.Count;
        if (index < Child0.Count)
        {
          return Child0[index];
        }

        index -= Child0.Count;
        return Child1[index];
      }
    }

    public void Add(IBox box, T value)
    {
      Add(new TileEntry<T>(box, value), false);
    }

    protected override void AddRange(BoxTile child)
    {
      BoxTile<T> tile = (BoxTile<T>)child;
      AddRange(tile._elemList);
    }

    private void AddRange(List<TileEntry<T>> elemList)
    {
      // TODO: implement sorting
      _elemList.AddRange(elemList);
    }

    public void Insert(int index, TileEntry<T> entry)
    {
      Count++;
      _elemList.Insert(index, entry);

      Index.BoxTile parent = Parent;
      while (parent != null)
      {
        parent.Count++;
        parent = parent.Parent;
      }
    }

    public void Remove(TileEntry<T> entry)
    {
      _elemList.Remove(entry);

      Count--;
      Index.BoxTile parent = Parent;
      while (parent != null)
      {
        parent.Count--;
        parent = parent.Parent;
      }
    }
  }

}
