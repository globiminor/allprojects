using System;
using System.Collections;
using System.Collections.Generic;

namespace Basics.Geom
{
  public abstract partial class BoxTree
  {
    private int _dimension;
    private Box _unitBox;
    private Box _mainBox;
    private int[] _mainCounter;
    private int[] _mainSize;

    // private Box mSearchBox;
    private int _maxElemPerTile;
    private int _nElemJoin;
    private readonly bool _dynamic;

    private BoxTile _mainTile;

    private const int _maximumAllowedTileLevels = 30;
    private int _maxDenominator = 1 << _maximumAllowedTileLevels;

    private const int _defaultMaxElementCountPerTile = 64; // TODO revise
    private const bool _defaultDynamic = true;

    internal BoxTree(int dimension, BoxTile mainTile)
      : this(dimension, _defaultMaxElementCountPerTile, _defaultDynamic, mainTile) { }

    internal BoxTree(int dimension, int nElem, bool dynamic, BoxTile mainTile)
    {
      _dimension = dimension;
      _mainTile = mainTile;
      _maxElemPerTile = nElem;
      _dynamic = dynamic;
    }

    public void Init(int dimension, Box box, int maxElemPerTile, int nElemJoin)
    {
      _dimension = dimension;
      _maxElemPerTile = maxElemPerTile;
      _nElemJoin = nElemJoin;

      _mainCounter = new int[dimension];
      _mainSize = new int[dimension];

      if (box != null)
      {
        _unitBox = box.Clone();
        _mainBox = _unitBox.Clone();
        for (int i = 0; i < dimension; i++)
        {
          _mainBox.Max[i] += _mainBox.Max[i] - _mainBox.Min[i];
          _mainSize[i] = 2;
        }
      }
      else
      {
        _unitBox = null;
        _mainBox = null;
      }
    }

    public void Init(int dimension, int maxElemPerTile, int nElemJoin)
    {
      Init(dimension, null, maxElemPerTile, nElemJoin);
    }

    public void Init(int dimension, int maxElemPerTile)
    {
      Init(dimension, null, maxElemPerTile, maxElemPerTile / 2);
    }

    public void Init(Box box, int maxElemPerTile, int nElemJoin)
    {
      Init(box.Dimension, box, maxElemPerTile, nElemJoin);
    }

    public void Init(Box box, int maxElemPerTile)
    {
      Init(box.Dimension, box, maxElemPerTile, maxElemPerTile / 2);
    }

    public void InitSize(IEnumerable<IGeometry> data)
    {
      InitSize((IEnumerable)data);
    }

    private void InitSize()
    {
      InitSize(_mainTile.EnumElems());
    }

    private void InitSize([NotNull] IEnumerable data)
    {
      _unitBox = null;
      foreach (var row in data)
      {
        IBox box;
        if (row is IGeometry)
        {
          box = ((IGeometry)row).Extent;
        }
        else
        {
          //Debug.Assert(pRow is TileEntry);
          box = ((TileEntry)row).Box;
        }
        if (_unitBox == null)
        {
          Point min = Point.Create_0(_dimension);
          Point max = Point.Create_0(_dimension);
          for (int i = 0; i < _dimension; i++)
          {
            min[i] = box.Min[i];
            max[i] = box.Max[i];
          }
          _unitBox = new Box(min, max);
          continue;
        }

        for (int i = 0; i < _dimension; i++)
        {
          if (box.Min[i] < _unitBox.Min[i])
          { _unitBox.Min[i] = box.Min[i]; }
          if (box.Max[i] > _unitBox.Max[i])
          { _unitBox.Max[i] = box.Max[i]; }
        }
      }
      if (_unitBox == null)
      {
        throw new InvalidOperationException("data containes no values");
      }

      // double the size of each dimension
      _mainBox = _unitBox.Clone();
      _mainCounter = new int[_dimension];
      _mainSize = new int[_dimension];
      for (int i = 0; i < _dimension; i++)
      {
        _mainBox.Max[i] = 2 * _unitBox.Max[i] - _unitBox.Min[i];
        _mainCounter[i] = 0;
        _mainSize[i] = 2;
      }
    }

    public IBox UnitBox
    {
      get { return _unitBox; }
    }

    internal BoxTile MainTile
    {
      get { return _mainTile; }
    }

    internal Box MainBox
    {
      get { return _mainBox; }
    }

    public bool Dynamic
    {
      get { return _dynamic; }
    }

    public int NElemJoin
    {
      get { return _nElemJoin; }
    }

    public int MaxTileLevels
    {
      get
      {
        int levels = 0;
        int denominator = _maxDenominator;
        while (denominator > 1)
        {
          levels++;
          denominator /= 2;
        }
        return levels;
      }
      set
      {
        Assert.ArgumentCondition(value <= _maximumAllowedTileLevels,
               "Maximum allowed tile level: {0}",
               _maximumAllowedTileLevels);
        Assert.ArgumentCondition(value > 0, "Maximum tile level must be > 0");

        _maxDenominator = 1 << value;
      }
    }

    public Box GetTile(IBox box, double maxSize)
    {
      if (_mainBox == null)
      {
        throw new InvalidOperationException("main box not initialized");
      }
      VerifyExtent(box);

      IList<int> counter0 = null;
      IList<int> denominator = null;

      if (_unitBox != null)
      {
        GetPositions(out counter0, out denominator);
      }

      BoxTile addTile = FindAddTile(_mainTile, box, true, counter0, denominator);

      while (addTile.MaxInParentSplitDim - addTile.MinInParentSplitDim > maxSize)
      {
        addTile.Split(_unitBox, counter0, denominator, _mainCounter, _mainSize);
        BoxTile childTile = FindAddTile(addTile, box, false, counter0, denominator);
        if (childTile == addTile)
        {
          break;
        }
        addTile = childTile;
      }
      Box tileBox = GetBox(addTile);
      return tileBox;
    }

    public List<Box> GetLeaves(bool nonEmpty = false)
    {
      List<BoxTile> leaves = new List<BoxTile>();
      AddLeaves(_mainTile, leaves, nonEmpty);
      List<Box> leafBoxs = new List<Box>(leaves.Count);
      foreach (var leaf in leaves)
      {
        leafBoxs.Add(GetBox(leaf));
      }
      return leafBoxs;
    }

    private void AddLeaves(BoxTile parent, List<BoxTile> leaves, bool nonEmpty)
    {
      if (parent.Child0 == null)
      {
        if (!nonEmpty || parent.ElemsCount > 0)
        {
          leaves.Add(parent);
        }
        return;
      }
      AddLeaves(parent.Child0, leaves, nonEmpty);
      AddLeaves(parent.Child1, leaves, nonEmpty);
    }

    internal bool VerifyPointTree()
    {
      return VerifyTile(_mainTile);
    }

    protected virtual IEnumerator<TileEntry> GetTileEnumerator(
      TileEntryEnumerator enumerator, IEnumerable<TileEntry> list)
    {
      return list?.GetEnumerator();
    }

    private bool VerifyTile(BoxTile tile)
    {
      if (tile == null)
      {
        return true;
      }

      if (VerifyTile(tile.Child0) == false)
      {
        return false;
      }
      if (VerifyTile(tile.Child1) == false)
      {
        return false;
      }

      return tile.VerifyPointTile(this);
    }

    internal BoxTile StartFindAddTile(IBox geomExtent, bool dynamic)
    {
      IList<int> counter0 = null;
      IList<int> denominator = null;

      if (dynamic && _unitBox != null)
      {
        GetPositions(out counter0, out denominator);
      }

      return FindAddTile(_mainTile, geomExtent, dynamic, counter0, denominator);
    }

    private void GetPositions(out IList<int> counter0, out IList<int> denominator)
    {
      counter0 = new int[_dimension];
      denominator = new List<int>(_dimension);
      for (int i = 0; i < _dimension; i++)
      {
        denominator.Add(4);
      }
    }

    /// <summary>
    /// find tile where geomExtent fits inside
    /// </summary>
    private BoxTile FindAddTile(BoxTile parent, IBox geomExtent, bool dynamic,
            IList<int> counter0, IList<int> denominator)
    {
      BoxTile child0 = parent.Child0;
      if (child0 != null)
      {
        BoxTile child1 = parent.Child1;
        int divDim = parent.SplitDimension;

        if (geomExtent.Min[divDim] < child1.MinInParentSplitDim)
        {
          if (geomExtent.Max[divDim] < child0.MaxInParentSplitDim)
          {
            if (dynamic)
            {
              counter0[divDim] *= 2;
              denominator[divDim] *= 2;
            }

            bool addDynamic = dynamic;
            if (denominator[divDim] >= _maxDenominator)
            {
              addDynamic = false;
            }

            return FindAddTile(child0, geomExtent, addDynamic, counter0, denominator);
          }
          return parent;
        }
        if (geomExtent.Max[divDim] < child1.MaxInParentSplitDim)
        {
          if (dynamic)
          {
            counter0[divDim] += counter0[divDim] + 1;
            denominator[divDim] *= 2;
          }

          bool addDynamic = dynamic;
          if (denominator[divDim] >= _maxDenominator)
          {
            addDynamic = false;
          }

          return FindAddTile(child1, geomExtent, addDynamic, counter0, denominator);
        }
        return parent;
      }
      // tile has no children
      if (!dynamic || parent.ElemsCount < _maxElemPerTile)
      {
        return parent;
      }
      // split tile and try again
      if (_unitBox == null)
      {
        InitSize();
        VerifyExtent(geomExtent);

        return StartFindAddTile(geomExtent, true);
      }
      parent.Split(_unitBox, counter0, denominator, _mainCounter, _mainSize);
      return FindAddTile(parent, geomExtent, false, counter0, denominator);
    }

    protected void VerifyExtent(IBox box)
    {
      if (_mainBox != null)
      {
        int dim = _mainBox.ExceedDimension(box);
        while (dim != 0)
        {
          Enlarge(dim);
          dim = _mainBox.ExceedDimension(box);
        }
      }
    }

    /// <summary>
    /// Enlarge Region. Remark: when data is load, numerical problems may occure
    /// </summary>
    /// <param name="directedDimension">directed dimension to enlarge: (-dim -1) : add negativ (dim + 1) : add positiv </param>
    /// <returns>new parent tile</returns>
    private BoxTile Enlarge(int directedDimension)
    {
      int dimension = Math.Abs(directedDimension) - 1;

      BoxTile newTile = _mainTile.CreateEmptyTile();

      int size = _mainSize[dimension];

      _mainSize[dimension] *= 2;

      double u0 = _unitBox.Min[dimension];
      double u = _unitBox.Max[dimension] - _unitBox.Min[dimension];
      if (directedDimension > 0)
      {
        _mainBox.Max[dimension] = Position(u0, u, _mainCounter[dimension] + 2 * size, 1);

        newTile.InitChildren(_mainTile, _mainTile.CreateEmptyTile(),
               u0, u, _mainCounter[dimension], size / 2, 1);
      }
      else
      {
        if ((size & 1) != 0)
        {
          throw new InvalidProgramException(
            string.Format(
            "Error in software design assumption size {0} must be divisible by 2", size));
        }
        _mainCounter[dimension] -= size / 2;

        _mainBox.Min[dimension] = Position(u0, u, _mainCounter[dimension], 1);
        _mainBox.Max[dimension] = Position(u0, u, _mainCounter[dimension] + 2 * size, 1);

        newTile.InitChildren(_mainTile.CreateEmptyTile(), _mainTile,
               u0, u, _mainCounter[dimension], size / 2, 1);
      }
      newTile.SplitDimension = dimension;
      _mainTile = newTile;

      return newTile;
    }

    /// <summary>
    /// Get the Extent of Tile
    /// </summary>
    private Box TileExtent(BoxTile tile)
    {
      Box box = _mainBox.Clone();
      BoxTile child = tile;
      BoxTile parent = child.Parent;
      double d = -1;
      double dMax = 0;

      // proceed up so long until it is verified that all dimensions of the 
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

    private double[] GetExtent(BoxTile tile, int dimension)
    {
      BoxTile t = tile;
      while (t.Parent != null)
      {
        if (dimension == t.Parent.SplitDimension)
        { return new double[] { t.MinInParentSplitDim, t.MaxInParentSplitDim }; }
        t = t.Parent;
      }
      return new double[] { _mainBox.Min[dimension], _mainBox.Max[dimension] };
    }

    private Box GetBox(BoxTile tile)
    {
      Point min = Point.Create_0(_dimension);
      Point max = Point.Create_0(_dimension);
      bool[] handled = new bool[_dimension];
      int nHandled = 0;
      BoxTile t = tile;
      while (t.Parent != null && nHandled < _dimension)
      {
        if (!handled[t.Parent.SplitDimension])
        {
          min[t.Parent.SplitDimension] = t.MinInParentSplitDim;
          max[t.Parent.SplitDimension] = t.MaxInParentSplitDim;
          handled[t.Parent.SplitDimension] = true;
          nHandled++;
        }
        t = t.Parent;
      }
      if (nHandled < _dimension)
      {
        for (int iHandled = 0; iHandled < _dimension; iHandled++)
        {
          if (!handled[iHandled])
          {
            min[iHandled] = _mainBox.Min[iHandled];
            max[iHandled] = _mainBox.Max[iHandled];
          }
        }
      }
      return new Box(min, max);
    }

    protected static double Position(double u0, double u, int counter, int denominator)
    {
      while (denominator > 1 && (counter & 1) == 0)
      {
        denominator /= 2;
        counter /= 2;
      }
      double pos = u0 + counter * u / denominator;
      return pos;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentTile"></param>
    /// <param name="currentBox">begin: box of current tile; end: box of return tile</param>
    /// <param name="extent">extent which must intersect or touch next tile</param>
    /// <param name="stopTile"></param>
    /// <returns></returns>
    internal static BoxTile NextSelTile([NotNull] BoxTile currentTile,
                                        [NotNull] Box currentBox, IBox extent,
              BoxTile stopTile)
    {
      int splitDim = currentTile.SplitDimension;
      BoxTile child = currentTile.Child0;
      if (child != null)
      {
        if ((currentBox.Min[splitDim] == child.MinInParentSplitDim) == false)
        {
          throw new InvalidProgramException("Error in software design assumption: SplitDim");
        }
        currentBox.Max[splitDim] = child.MaxInParentSplitDim;
        if (extent == null || BoxOp.Intersects(currentBox, extent))
        {
          return child;
        }
      }

      while (currentTile != stopTile && currentTile != null)
      {
        if (child != currentTile.Child1)
        {
          child = currentTile.Child1;
          currentBox.Min[splitDim] = child.MinInParentSplitDim;
          currentBox.Max[splitDim] = child.MaxInParentSplitDim;
          if (extent == null || BoxOp.Intersects(currentBox, extent))
          {
            return child;
          }
        }

        // reset box to current extent
        if (child != null)
        {
          currentBox.Min[splitDim] = currentTile.MinInSplitDim;
          currentBox.Max[splitDim] = currentTile.MaxInSplitDim;
        }

        // one step up
        child = currentTile;
        currentTile = currentTile.Parent;
        if (currentTile != null)
        {
          splitDim = currentTile.SplitDimension;
        }
      }
      // für genuegend unterteilte Tiles (sonst ev. Min.X == 0 o.ae.):
      // Debug.Assert(currentBox.Equals(mMainBox)); 
      return null;
    }

    #region IGeometry Members

    public int Dimension
    {
      get { return _dimension; }
    }

    public int Topology
    {
      get { return _dimension; }
    }

    public Box Extent
    {
      get { return _mainBox; }
    }

    public bool Intersects(IBox box)
    {
      return BoxOp.Intersects(Extent, box);
    }

    #endregion

    #region IList Members

    public void Clear()
    {
      _mainTile = _mainTile.CreateEmptyTile();
    }

    public bool IsFixedSize
    {
      get { return false; }
    }

    #endregion

    #region ICollection Members

    public int Count
    {
      get { return _mainTile.Count; }
    }

    #endregion

    public abstract class TileEntry
    {
      private readonly IBox _box;

      protected TileEntry(IBox box)
      {
        _box = box;
      }

      public IBox Box
      {
        get { return _box; }
      }
    }

    internal abstract class BoxTile
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

      internal abstract IEnumerable<TileEntry> EnumElems();

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

      internal bool VerifyPointTile(BoxTree tree)
      {
        IBox extent = tree.TileExtent(this);
        foreach (var entry in EnumElems())
        {
          if (BoxOp.Contains(extent, entry.Box) == false)
          {
            return false;
          }
        }
        return true;
      }
    }
  }

  public partial class BoxTree<T> : BoxTree
  {
    public BoxTree(int dimension)
      : base(dimension, new BoxTile()) { }

    public BoxTree(int dimension, int nElem, bool dynamic)
      : base(dimension, nElem, dynamic, new BoxTile()) { }

    internal new BoxTile MainTile
    {
      get { return (BoxTile)base.MainTile; }
    }

    public TileEntry this[int index]
    {
      get { return MainTile[index]; }
    }

    public int Add(IBox box, T value)
    {
      VerifyExtent(box);

      BoxTile addTile = (BoxTile)StartFindAddTile(box, Dynamic);
      addTile.Add(box, value);

      return MainTile.Count - 1;
    }

    /// <summary>
    /// Inserts a geometry at position.
    /// </summary>
    public void Insert(int index, TileEntry entry)
    {
      // TODO , only valid if just one tail exists
      MainTile.Insert(index, entry);
    }

    public void Remove(TileEntry entry)
    {
      BoxTile tile = (BoxTile)StartFindAddTile(entry.Box, false);
      tile.Remove(entry);

      if (Dynamic && tile.Child0 == null && tile.Parent != null &&
        tile.Parent.Child0.ElemList.Count + tile.Parent.Child1.ElemList.Count +
        tile.Parent.ElemList.Count < NElemJoin)
      {
        tile.Parent.Collapse();
      }
    }

    public TileEntryEnumerable Search(IBox box)
    {
      return new TileEntryEnumerable(this, box);
    }

    public TileEntry Near(IBox box)
    {
      if (MainBox == null)
      {
        if (MainTile.ElemList.Count == 0)
        { return null; }
      }
      return Near(box, MainTile);
    }

    private TileEntry Near(IBox box, BoxTile parent)
    {
      if (parent.ElemList.Count > 0)
      { return parent.ElemList[0]; }

      int divDim;

      BoxTile child0 = parent.Child0;
      if (child0 == null)
      { return null; }

      BoxTile child1 = parent.Child1;
      divDim = parent.SplitDimension;

      if (box.Min[divDim] < child1.MinInParentSplitDim)
      {
        TileEntry entry = Near(box, child0);
        if (entry != null)
        { return entry; }
        else
        { return Near(box, child1); }
      }
      else
      {
        TileEntry entry = Near(box, child1);
        if (entry != null)
        { return entry; }
        else
        { return Near(box, child0); }
      }
    }

    public bool Contains(TileEntry value)
    {
      BoxTile tile = (BoxTile)StartFindAddTile(value.Box, false);
      if (tile == null)
      {
        return false;
      }
      return tile.ElemList.Contains(value);
    }

    public void Sort(IComparer<TileEntry> comparer)
    {
      Sort(MainTile, comparer);
    }

    private void Sort(BoxTile tile, IComparer<TileEntry> comparer)
    {
      if (tile == null)
      {
        return;
      }

      tile.ElemList.Sort(comparer);

      Sort(tile.Child0, comparer);
      Sort(tile.Child1, comparer);
    }


    public new class TileEntry : BoxTree.TileEntry
    {
      private readonly T _value;

      public TileEntry(IBox box, T value)
        : base(box)
      {
        _value = value;
      }

      [NotNull]
      public T Value
      {
        get { return _value; }
      }
    }

    internal new sealed class BoxTile : BoxTree.BoxTile
    {
      private List<TileEntry> _elemList = new List<TileEntry>();

      public new BoxTile Parent
      {
        get { return (BoxTile)base.Parent; }
      }

      public new BoxTile Child0
      {
        get { return (BoxTile)base.Child0; }
      }

      public new BoxTile Child1
      {
        get { return (BoxTile)base.Child1; }
      }

      internal override BoxTree.BoxTile CreateEmptyTile()
      {
        return new BoxTile();
      }

      protected override void AddCore(BoxTree.TileEntry entry)
      {
        _elemList.Add((TileEntry)entry);
      }

      protected override IEnumerable<BoxTree.TileEntry> InitSplit()
      {
        List<TileEntry> toSplit = _elemList;
        _elemList = new List<TileEntry>();
        foreach (var splitEntry in toSplit)
        {
          yield return splitEntry;
        }
      }

      internal override IEnumerable<BoxTree.TileEntry> EnumElems()
      {
        foreach (var entry in _elemList)
        {
          yield return entry;
        }
      }

      public List<TileEntry> ElemList
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
        Add(new TileEntry(box, value), false);
      }

      protected override void AddRange(BoxTree.BoxTile child)
      {
        BoxTile tile = (BoxTile)child;
        AddRange(tile._elemList);
      }

      private void AddRange(List<TileEntry> elemList)
      {
        // TODO: implement sorting
        _elemList.AddRange(elemList);
      }

      public void Insert(int index, TileEntry entry)
      {
        Count++;
        _elemList.Insert(index, entry);

        BoxTree.BoxTile parent = Parent;
        while (parent != null)
        {
          parent.Count++;
          parent = parent.Parent;
        }
      }

      public void Remove(TileEntry entry)
      {
        _elemList.Remove(entry);

        Count--;
        BoxTree.BoxTile parent = Parent;
        while (parent != null)
        {
          parent.Count--;
          parent = parent.Parent;
        }
      }
    }
  }
}
