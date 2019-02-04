using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Basics.Geom
{
  partial class BoxTree
  {
    public class Neighborhood
    {
      public TileEntry Entry { get; set; }
      public IEnumerable Neighbours { get; set; }
    }

    internal class NeighborhoodEnumerator : IEnumerator<Neighborhood>
    {
      private class TilesHandler
      {
        private interface INeighbourEntryEnumerable
        {
          IEnumerable<TileEntry> GetEntries(IBox box);
        }

        private class ElemsEnumerable : INeighbourEntryEnumerable
        {
          private readonly BoxTile _tile;
          private readonly IBox _tileBox;

          public ElemsEnumerable(BoxTree tree, BoxTile tile)
          {
            _tile = tile;
            _tileBox = tree.GetBox(tile);
          }

          public IEnumerable<TileEntry> GetEntries(IBox box)
          {
            if (!BoxOp.Intersects(box, _tileBox))
            { yield break; }
            foreach (var entry in _tile.EnumElems())
            {
              if (BoxOp.Intersects(entry.Box, box))
              { yield return entry; }
            }
          }
        }

        private class EntryEnumerable : INeighbourEntryEnumerable
        {
          private readonly BoxTree _tree;
          private readonly BoxTile _tile;
          private readonly Box _tileBox;

          public EntryEnumerable(BoxTree tree, BoxTile tile)
          {
            _tree = tree;
            _tile = tile;
            _tileBox = tree.GetBox(tile);
          }

          public IEnumerable<TileEntry> GetEntries(IBox search)
          {
            foreach (var entry in new TileEntryEnumerable(
              _tree, _tile, _tileBox, search))
            {
              yield return entry;
            }
          }
        }

        private readonly NeighborhoodEnumerator _master;
        private TilesHandler _parent;
        private BoxTile _searchingTile;
        private LinkedList<BoxTile> _neighbourTiles;
        private List<BoxTile> _neighbourTilesWithElems;

        private double[] _searchExtent;
        private int _searchDim;

        public static TilesHandler Create(NeighborhoodEnumerator master)
        {
          TilesHandler created = new TilesHandler(master);
          created._searchDim = -1;
          return created;
        }

        [UsedImplicitly]
        public static string ToStringFile { get; set; }

        public override string ToString()
        {
          if (!string.IsNullOrEmpty(ToStringFile))
          {
            ToCanvas(ToStringFile);
          }
          StringBuilder sb = new StringBuilder();
          IBox x0 = _master._searchingTree._mainBox;
          if (_searchingTile != null)
          {
            sb.Append("X: ");
            TileToString(sb, x0, _master._searchingTree.GetBox(_searchingTile));
          }
          else
          {
            sb.Append("X-Tile not initialized.");
          }
          if (_neighbourTiles != null)
          {
            int iTile = 0;
            foreach (var yTile in _neighbourTiles)
            {
              sb.AppendFormat(" Y[{0}]: ", iTile);
              TileToString(sb, _master._neighbourTree._mainBox, _master._neighbourTree.GetBox(yTile));
              iTile++;
            }
          }
          else
          {
            sb.Append("  Y-Tiles not initialized.");
          }
          return sb.ToString();
        }

        private void ToCanvas(string fileName)
        {
          const string canvas = "cnv";
          const string ctx = "ctx";
          const double w = 500;
          const double h = w;

          Box x = _master._searchingTree._mainBox.Clone();
          x.Include(_master._neighbourTree._mainBox.Clone());
          double f = x.GetMaxExtent() / w;

          StringBuilder s = new StringBuilder();
          s.AppendLine("<!DOCTYPE html>");
          s.AppendLine("<html>");
          s.AppendLine("<body>");
          s.AppendLine("");

          s.AppendFormat("    <canvas id=\"{0}\" width=\"{1}\" height=\"{2}\"",
                         canvas, w, h);
          s.AppendLine();
          s.AppendLine("      style=\"border:1px solid #000000;\">");
          s.AppendLine("    </canvas>");
          s.AppendLine("");
          s.AppendLine("  <script>");
          s.AppendFormat("var c = document.getElementById(\"{0}\");", canvas);
          s.AppendLine();
          s.AppendFormat("var {0} = c.getContext(\"2d\");", ctx);
          s.AppendLine();
          s.AppendFormat("{0}.transform(1, 0, 0, -1, 0, {1});", ctx, h);
          s.AppendLine();
          WriteBox(s, ctx, _master._searchingTree._mainBox, x, f, 4, "#000000");
          WriteBox(s, ctx, _master._neighbourTree._mainBox, x, f, 4, "#000000");
          WriteBox(s, ctx, _master._common, x, f, 6, "#808080");
          foreach (var b in _master._searchingTree.GetLeaves(true))
          {
            WriteBox(s, ctx, b, x, f, 0.5, "#00ff00");
          }
          foreach (var b in _master._neighbourTree.GetLeaves(true))
          {
            WriteBox(s, ctx, b, x, f, 0.5, "#ff00ff");
          }
          if (_searchingTile != null)
          {
            WriteBox(s, ctx, _master._searchingTree.GetBox(_searchingTile), x, f, 2, "#ff0000");
          }
          if (_neighbourTiles != null)
          {
            foreach (var yTile in _neighbourTiles)
            {
              WriteBox(s, ctx, _master._neighbourTree.GetBox(yTile), x, f, 1, "#0000ff");
            }
          }
          foreach (var yTile in GetNeighbourTilesWithElems())
          {
            WriteBox(s, ctx, _master._neighbourTree.GetBox(yTile), x, f, 1, "#00ffff");
          }
          s.AppendLine("  </script>");
          s.AppendLine("</body>");
          s.AppendLine("</html>");

          using (TextWriter wr = new StreamWriter(fileName))
          {
            wr.Write(s.ToString());
          }
        }

        private void WriteBox(StringBuilder s, string ctx, Box box, Box x, double f,
                              double lineWidth = 1, string color = null)
        {
          Point min = (1.0 / f) * (Point.Create(box.Min) - x.Min);
          Point max = (1.0 / f) * (Point.Create(box.Max) - x.Min);

          s.AppendFormat("{0}.save();", ctx);
          s.AppendLine();
          s.AppendFormat("{0}.lineWidth = {1:N0};", ctx, lineWidth);
          s.AppendLine();
          if (!string.IsNullOrEmpty(color))
          {
            s.AppendFormat("{0}.strokeStyle = '{1}';", ctx, color);
            s.AppendLine();
          }
          s.AppendFormat("{0}.beginPath();", ctx);
          s.AppendLine();
          s.AppendFormat("{0}.moveTo({1:N1},{2:N1});", ctx, min.X, min.Y);
          s.AppendLine();
          s.AppendFormat("{0}.lineTo({1:N1},{2:N1});", ctx, min.X, max.Y);
          s.AppendLine();
          s.AppendFormat("{0}.lineTo({1:N1},{2:N1});", ctx, max.X, max.Y);
          s.AppendLine();
          s.AppendFormat("{0}.lineTo({1:N1},{2:N1});", ctx, max.X, min.Y);
          s.AppendLine();
          s.AppendFormat("{0}.lineTo({1:N1},{2:N1});", ctx, min.X, min.Y);
          s.AppendLine();
          s.AppendFormat("{0}.stroke();", ctx);
          s.AppendLine();
          s.AppendFormat("{0}.restore();", ctx);
          s.AppendLine();
        }

        private void TileToString(StringBuilder sb, IBox b0, IBox bt)
        {
          Point dx0 = Point.Create(b0.Max) - b0.Min;
          Point dxt = Point.Create(bt.Max) - bt.Min;
          Point ds = Point.Create(bt.Min) - b0.Min;
          for (int i = 0; i < bt.Dimension; i++)
          {
            int nx = (int)Math.Round(dx0[i] / dxt[i]);
            int ix = (int)Math.Round(2 * ds[i] / dxt[i]);
            sb.AppendFormat("{0}:{1}/{2}; ", (char)('x' + i), ix, nx);
          }
        }

        private TilesHandler(NeighborhoodEnumerator master)
        {
          _master = master;
        }

        internal IEnumerable<Neighborhood> GetNeighborhoods(double searchDistance)
        {
          if (_searchingTile.ElemsCount <= 0)
          { yield break; }

          if (!HasNeighbourTiles())
          { yield break; }

          List<INeighbourEntryEnumerable> neighbourEntryEnumerators = GetNeighbourEntryEnumerators();
          if (neighbourEntryEnumerators == null)
          { yield break; }

          foreach (var searchEntry in _searchingTile.EnumElems())
          {
            IBox search = GetSearchBox(searchEntry.Box, searchDistance, _master._common);
            if (search == null)
            { continue; }

            yield return new Neighborhood
            {
              Entry = searchEntry,
              Neighbours =
                GetNeighbours(search, neighbourEntryEnumerators)
            };
          }
        }

        private bool HasNeighbourTiles()
        {
          if (_neighbourTiles != null && _neighbourTiles.Count > 0)
          { return true; }

          if (_neighbourTilesWithElems != null)
          { return true; }

          return false;
        }

        private List<INeighbourEntryEnumerable> GetNeighbourEntryEnumerators()
        {
          int n = _neighbourTiles != null ? _neighbourTiles.Count : 0;
          n += _neighbourTilesWithElems != null ? _neighbourTilesWithElems.Count : 0;
          if (n == 0)
          { return null; }

          List<INeighbourEntryEnumerable> enums = new List<INeighbourEntryEnumerable>(n);
          if (_neighbourTilesWithElems != null)
          {
            foreach (var tile in _neighbourTilesWithElems)
            { enums.Add(new ElemsEnumerable(_master._neighbourTree, tile)); }
          }

          if (_neighbourTiles != null)
          {
            foreach (var tile in _neighbourTiles)
            { enums.Add(new EntryEnumerable(_master._neighbourTree, tile)); }
          }

          return enums;
        }

        private IEnumerable<BoxTile> GetNeighbourTilesWithElems()
        {
          if (_neighbourTilesWithElems != null)
          {
            foreach (var neighbourTile in _neighbourTilesWithElems)
            { yield return neighbourTile; }
          }
        }

        private IEnumerable<TileEntry> GetNeighbours(IBox search,
          List<INeighbourEntryEnumerable> neighbourEntryEnumerators)
        {
          foreach (var neighbourEntryEnumerator in neighbourEntryEnumerators)
          {
            foreach (var neighbourEntry in neighbourEntryEnumerator.GetEntries(search))
            {
              yield return neighbourEntry;
            }
          }
        }

        private IEnumerable<TileEntry> GetNeighbours(IBox search)
        {
          foreach (var neighbourTile in GetNeighbourTilesWithElems())
          {
            if (!BoxOp.Intersects(_master._neighbourTree.GetBox(neighbourTile), search))
            { continue; }

            foreach (var neighbour in neighbourTile.EnumElems())
            {
              if (BoxOp.Intersects(neighbour.Box, search))
              { yield return neighbour; }
            }
          }
          if (_neighbourTiles != null)
          {
            foreach (var neighbourTile in _neighbourTiles)
            {
              foreach (var neighbour in new TileEntryEnumerable(
                _master._neighbourTree, neighbourTile,
                _master._neighbourTree.GetBox(neighbourTile), search))
              {
                yield return neighbour;
              }
            }
          }
        }

        private double[] GetSearchExtent([NotNull] BoxTree tree, [NotNull] BoxTile tile,
          double search, [NotNull] IBox commonBox, out int splitDim)
        {
          if (tile.Parent == null)
          {
            splitDim = -1;
            return null;
          }

          splitDim = tile.Parent.SplitDimension;
          double[] extent = tree.GetExtent(tile, splitDim);
          extent[0] = Math.Max(extent[0] - search, commonBox.Min[splitDim]);
          extent[1] = Math.Min(extent[1] + search, commonBox.Max[splitDim]);

          return extent;
        }

        private double[] GetNeighbourSearchDimExtent(BoxTile neighbourTile)
        {
          double[] neighbourExtent = _master._neighbourTree.GetExtent(neighbourTile, _searchDim);
          if (_searchExtent[1] >= neighbourExtent[0] && _searchExtent[0] <= neighbourExtent[1])
          { return neighbourExtent; }
          return null;
        }

        private bool Intersects(BoxTile yTile, double[] searchExtent)
        {
          return (yTile.MinInParentSplitDim <= searchExtent[1] &&
           yTile.MaxInParentSplitDim >= searchExtent[0]);
        }

        private double[] GetSearchExtent(int dim)
        {
          TilesHandler t = this;
          while (t != null)
          {
            if (t._searchDim == dim)
            { return t._searchExtent; }
            t = t._parent;
          }
          Box b = _master._common;
          return new double[] { b.Min[dim], b.Max[dim] };
        }

        private void Init(LinkedList<BoxTile> initNeighbourTiles)
        {
          _searchExtent = GetSearchExtent(_master._searchingTree, _searchingTile,
            _master._searchDistance, _master._common, out _searchDim);

          if (_parent._neighbourTilesWithElems != null)
          {
            foreach (var neighbourTile in _parent._neighbourTilesWithElems)
            {
              if (GetNeighbourSearchDimExtent(neighbourTile) != null)
              { AddNeighbourTileWithElements(neighbourTile); }
            }
          }


          double searchSize = double.MaxValue;
          if (_searchExtent != null)
          { searchSize = _searchExtent[1] - _searchExtent[0]; }

          if (searchSize < 0 || initNeighbourTiles == null)
          { return; }

          LinkedList<BoxTile> neighbourTiles = new LinkedList<BoxTile>(initNeighbourTiles);

          if (_searchExtent != null)
          {
            List<LinkedListNode<BoxTile>> neighbourNodes =
              new List<LinkedListNode<BoxTile>>(initNeighbourTiles.Count);
            {
              LinkedListNode<BoxTile> neighbourNode = neighbourTiles.First;
              while (neighbourNode != null)
              {
                neighbourNodes.Add(neighbourNode);
                neighbourNode = neighbourNode.Next;
              }
            }

            // neighbourNodes was created because neighbourTiles changes during iteration
            foreach (var neighbourNode in neighbourNodes)
            {
              BoxTile neighbourTile = neighbourNode.Value;
              double[] neighbourExtent = GetNeighbourSearchDimExtent(neighbourTile);
              if (neighbourExtent == null)
              {
                neighbourNode.List.Remove(neighbourNode);
                continue;
              }

              double neighbourSearchDimSize = neighbourExtent[1] - neighbourExtent[0];
              if (neighbourSearchDimSize > searchSize)
              { AddChildren(neighbourNode, searchSize); }
            }
          }
          _neighbourTiles = neighbourTiles;
        }

        private void AddChildren(LinkedListNode<BoxTile> neighbourNode, double searchSize)
        {
          BoxTile neighbourTile = neighbourNode.Value;
          double[] searchExtent = GetSearchExtent(neighbourTile.SplitDimension);

          BoxTile c0 = neighbourTile.Child0;
          if (c0 != null && Intersects(c0, searchExtent))
          { AddChild(neighbourNode, c0, searchSize); }

          BoxTile c1 = neighbourTile.Child1;
          if (c1 != null && Intersects(c1, searchExtent))
          { AddChild(neighbourNode, c1, searchSize); }

          neighbourNode.List.Remove(neighbourNode);
          if (neighbourTile.ElemsCount > 0)
          {
            AddNeighbourTileWithElements(neighbourTile);
          }
        }

        private void AddNeighbourTileWithElements(BoxTile neighbourTile)
        {
          if (_neighbourTilesWithElems == null)
          { _neighbourTilesWithElems = new List<BoxTile>(); }
          _neighbourTilesWithElems.Add(neighbourTile);
        }

        private void AddChild(LinkedListNode<BoxTile> parentNode, BoxTile child,
          double searchSize)
        {
          LinkedListNode<BoxTile> childNode = parentNode.List.AddAfter(parentNode, child);
          if (parentNode.Value.SplitDimension != _searchDim)
          {
            // searchDim-Extent 'e' of neighbour tile did not change with split
            // -->  'e' > searchSize --> add children
            AddChildren(childNode, searchSize);
          }
          else
          {
            double childYSize = child.MaxInParentSplitDim - child.MinInParentSplitDim;
            if (childYSize > searchSize)
            { AddChildren(childNode, searchSize); }
          }
        }

        public TilesHandler GetNext()
        {
          TilesHandler next = new TilesHandler(_master);
          if (_searchingTile == null)
          {
            IBox search = GetSearchBox(_master._searchingTree.UnitBox, _master._searchDistance,
                                       _master._common);
            if (search == null || !BoxOp.Intersects(search, _master._neighbourTree.UnitBox))
            {
              return null;
            }

            BoxTile searchingTile = _master._searchingTree.MainTile;
            BoxTile neighbourTile = _master._neighbourTree.MainTile;
            LinkedList<BoxTile> neighbourTiles = new LinkedList<BoxTile>();
            neighbourTiles.AddFirst(neighbourTile);

            next._searchingTile = searchingTile;
            next._parent = this;
            next.Init(neighbourTiles);
          }
          else
          {
            BoxTile nextSearchTile;
            TilesHandler nextParent = this;

            if (!nextParent.HasNeighbourTiles() || nextParent._searchingTile.Child0 == null)
            {
              // MoveUp;
              while (nextParent != null && nextParent._parent != null &&
                     nextParent._parent._searchingTile != null &&
                     nextParent._parent._searchingTile.Child1 == nextParent._searchingTile)
              {
                nextParent = nextParent._parent;
              }

              if (nextParent == null || nextParent._parent == null ||
                  nextParent._parent._searchingTile == null)
              {
                return null;
              }
              nextParent = nextParent._parent;
              nextSearchTile = nextParent._searchingTile.Child1;
            }
            else
            {
              nextSearchTile = nextParent._searchingTile.Child0;
            }

            next._searchingTile = nextSearchTile;
            next._parent = nextParent;
            next.Init(nextParent._neighbourTiles);
          }

          return next;
        }
      }

      private readonly BoxTree _searchingTree;
      private readonly BoxTree _neighbourTree;
      private readonly Box _common;
      private readonly double _searchDistance;

      private TilesHandler _tilesHandler;
      private IEnumerator<Neighborhood> _tileNeighborhoods;
      private Neighborhood _current;

      public NeighborhoodEnumerator([NotNull] BoxTree searchingTree,
        [NotNull] BoxTree neighbourTree, double searchDistance, [CanBeNull] IBox common)
      {
        _searchingTree = searchingTree;
        _neighbourTree = neighbourTree;
        _searchDistance = searchDistance;

        Point min = Point.Create(_searchingTree._unitBox.Min);
        Point max = Point.Create(_searchingTree._unitBox.Max);
        for (int i = 0; i < min.Dimension; i++)
        {
          min[i] = Math.Max(min[i], _neighbourTree._unitBox.Min[i]);
          if (common != null)
          { min[i] = Math.Max(min[i], common.Min[i]); }
          min[i] -= searchDistance;

          max[i] = Math.Min(max[i], _neighbourTree._unitBox.Max[i]);
          if (common != null)
          { max[i] = Math.Min(max[i], common.Max[i]); }
          max[i] += searchDistance;
        }
        _common = new Box(min, max);

        Reset();
      }

      public void Reset()
      {
        _tileNeighborhoods = null;
        _tilesHandler = TilesHandler.Create(this);
      }

      public Neighborhood Current
      {
        get { return _current; }
        protected set { _current = value; }
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      protected virtual void SetCurrent(Neighborhood pair)
      {
        _current = pair;
      }

      public void Dispose() { }

      public bool MoveNext()
      {
        while (true)
        {
          if (_tileNeighborhoods != null && _tileNeighborhoods.MoveNext())
          {
            SetCurrent(_tileNeighborhoods.Current);
            return true;
          }

          _tilesHandler = _tilesHandler.GetNext();
          if (_tilesHandler == null)
          {
            return false;
          }

          _tileNeighborhoods = GetTileNeighborhoods().GetEnumerator();
        }
      }

      private IEnumerable<Neighborhood> GetTileNeighborhoods()
      {
        foreach (var neighborhood in _tilesHandler.GetNeighborhoods(_searchDistance))
        {
          yield return neighborhood;
        }
      }
    }

    [CanBeNull]
    private static Box GetSearchBox([NotNull] IBox rawBox, double search,
                                     [NotNull] IBox commonBox)
    {
      int dim = commonBox.Dimension;
      Point min = Point.Create_0(dim);
      Point max = Point.Create_0(dim);
      for (int i = 0; i < dim; i++)
      {
        min[i] = Math.Max(rawBox.Min[i] - search, commonBox.Min[i]);
        max[i] = Math.Min(rawBox.Max[i] + search, commonBox.Max[i]);

        if (min[i] > max[i])
        { return null; }
      }

      return new Box(min, max);
    }
  }

  partial class BoxTree<T>
  {
    public class Neighborhood<U> : BoxTree.Neighborhood
    {
      public new BoxTree<T>.TileEntry Entry
      {
        get { return (BoxTree<T>.TileEntry)base.Entry; }
        set { base.Entry = value; }
      }

      public new IEnumerable<BoxTree<U>.TileEntry> Neighbours
      {
        get { return (IEnumerable<BoxTree<U>.TileEntry>)base.Neighbours; }
        set { base.Neighbours = value; }
      }
    }

    public NeighborhoodEnumerable<U> EnumerateNeighborhoods<U>([NotNull] BoxTree<U> neighbourTree,
      double searchDistance, [CanBeNull] IBox common = null)
    {
      return new NeighborhoodEnumerable<U>(this, neighbourTree, searchDistance, common);
    }

    internal sealed class NeighborhoodEnumerator<U> : NeighborhoodEnumerator, IEnumerator<Neighborhood<U>>
    {
      public NeighborhoodEnumerator([NotNull] BoxTree<T> searchingTree,
        [NotNull] BoxTree<U> neighbourTree, double searchDistance, [CanBeNull] IBox common)
        : base(searchingTree, neighbourTree, searchDistance, common) { }

      Neighborhood<U> IEnumerator<Neighborhood<U>>.Current
      {
        get { return (Neighborhood<U>)Current; }
      }

      protected override void SetCurrent(Neighborhood neighborhood)
      {
        Current = new Neighborhood<U>
        {
          Entry = (TileEntry)neighborhood.Entry,
          Neighbours = GetEntries(neighborhood.Neighbours)
        };
      }

      private IEnumerable<BoxTree<U>.TileEntry> GetEntries(IEnumerable neighbourTileEntries)
      {
        foreach (var neighbourEntry in neighbourTileEntries)
        {
          yield return (BoxTree<U>.TileEntry)neighbourEntry;
        }
      }
    }

    public class NeighborhoodEnumerable<U> : IEnumerable<Neighborhood<U>>
    {
      private readonly BoxTree<T> _searchingTree;
      private readonly BoxTree<U> _neighbourTree;
      private readonly double _searchDistance;
      private readonly IBox _common;

      public NeighborhoodEnumerable([NotNull] BoxTree<T> searchingTree,
        [NotNull] BoxTree<U> neighbourTree, double searchDistance, [CanBeNull] IBox common)
      {
        _searchingTree = searchingTree;
        _neighbourTree = neighbourTree;
        _searchDistance = searchDistance;
        _common = common;
      }

      #region IEnumerable Members

      internal NeighborhoodEnumerator<U> GetEnumerator()
      {
        return new NeighborhoodEnumerator<U>(_searchingTree, _neighbourTree, _searchDistance, _common);
      }

      IEnumerator<Neighborhood<U>> IEnumerable<Neighborhood<U>>.GetEnumerator()
      {
        return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }
  }
}
