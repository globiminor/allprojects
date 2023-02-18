using System;
using System.Collections;
using System.Collections.Generic;

namespace Basics.Geom.Index
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

    protected const int DefaultMaxElementCountPerTile = 16; // TODO revise
    protected const bool DefaultDynamic = true;

    internal BoxTree(int dimension, int nElem, bool dynamic, BoxTile mainTile)
    {
      _dimension = dimension;
      _mainTile = mainTile;
      _maxElemPerTile = nElem;
      _dynamic = dynamic;
    }

    private class MyBoxComparer : IEqualityComparer<IBox>
    {
      public bool Equals(IBox x, IBox y)
      {
        bool a = (x == y);
        return a;
      }

      public int GetHashCode(IBox box)
      {
        return box.GetHashCode();
      }
    }
    public static BoxTree<T> Create<T>(IEnumerable<T> elems, Func<T, IBox> getBox, int nElem = DefaultMaxElementCountPerTile, bool dynamic = DefaultDynamic)
    {
      Dictionary<IBox, T> adds = new Dictionary<IBox, T>(new MyBoxComparer());
      Box allBox = null;
      foreach (var elem in elems)
      {
        IBox box = getBox(elem);
        adds.Add(box, elem);
        if (allBox == null)
        { allBox = BoxOp.Clone(box); }
        else
        { allBox.Include(box); }
      }
      int dim = allBox?.Dimension ?? 2;

      BoxTree<T> bt = new BoxTree<T>(dim, nElem, dynamic);
      bt.Init(dim, allBox, nElem, nElem / 2);
      foreach (var pair in adds)
      {
        bt.Add(pair.Key, pair.Value);
      }
      return bt;
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

    public IEnumerable<TileEntry> Search(IBox search = null, IBoxTreeSearch searcher = null)
    {
      IBoxTreeSearch s = searcher ?? ((search != null) ? (IBoxTreeSearch)new BoxTreeBoxSearch(search) : new BoxTreeFullSearch());
      s.Init(search);

      foreach (var entry in s.EnumEntries(MainTile, MainBox))
      {
        yield return entry;
      }
    }

    internal IEnumerable<TileEntry> Search(BoxTile startTile, Box startBox, IBox search)
    {
      foreach (var tile in EnumTiles(startTile, startBox, search))
      {
        foreach (var entry in tile.EnumElems())
        {
          if (search == null || BoxOp.Intersects(entry.Box, search))
          {
            yield return entry;
          }
        }
      }
    }

    internal IEnumerable<BoxTile> EnumTiles(BoxTile startTile, Box startBox, IBox search)
    {
      if (search != null && !BoxOp.Intersects(startBox, search))
      {
        yield break;
      }
      yield return startTile;

      foreach (var child in new[] { startTile.Child0, startTile.Child1 })
      {
        if (child == null)
        { continue; }

        Box childBox = startBox.Clone();
        int splitDim = startTile.SplitDimension;
        childBox.Min[splitDim] = child.MinInParentSplitDim;
        childBox.Max[splitDim] = child.MaxInParentSplitDim;

        foreach (var tile in EnumTiles(child, childBox, search))
        { yield return tile; }
      }
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

      return tile.VerifyPointTile(MainBox);
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
        _mainBox.Max[dimension] = BoxTile.Position(u0, u, _mainCounter[dimension] + 2 * size, 1);

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

        _mainBox.Min[dimension] = BoxTile.Position(u0, u, _mainCounter[dimension], 1);
        _mainBox.Max[dimension] = BoxTile.Position(u0, u, _mainCounter[dimension] + 2 * size, 1);

        newTile.InitChildren(_mainTile.CreateEmptyTile(), _mainTile,
               u0, u, _mainCounter[dimension], size / 2, 1);
      }
      newTile.SplitDimension = dimension;
      _mainTile = newTile;

      return newTile;
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
  }

  public partial class BoxTree<T> : BoxTree
  {
    public BoxTree(int dimension, int nElem = DefaultMaxElementCountPerTile, bool dynamic = DefaultDynamic)
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

    public new IEnumerable<TileEntry> Search(IBox search = null, IBoxTreeSearch searcher = null)
    {
      foreach (var baseEntry in base.Search(search, searcher))
      {
        yield return (TileEntry)baseEntry;
      }
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


    public class TileEntry : Index.TileEntry
    {
      public TileEntry(IBox box, T value)
        : base(box, value)
      { }

      [NotNull]
      public T Value
      {
        get { return (T)BaseValue; }
      }
    }

    internal sealed class BoxTile : Index.BoxTile
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

      internal override Index.BoxTile CreateEmptyTile()
      {
        return new BoxTile();
      }

      protected override void AddCore(Index.TileEntry entry)
      {
        _elemList.Add((TileEntry)entry);
      }

      protected override IEnumerable<Index.TileEntry> InitSplit()
      {
        List<TileEntry> toSplit = _elemList;
        _elemList = new List<TileEntry>();
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

      protected override void AddRange(Index.BoxTile child)
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

        Index.BoxTile parent = Parent;
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
        Index.BoxTile parent = Parent;
        while (parent != null)
        {
          parent.Count--;
          parent = parent.Parent;
        }
      }
    }
  }
}
