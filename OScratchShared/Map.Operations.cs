using System.Collections.Generic;

namespace OMapScratch
{
  partial class Map
  {
    private interface IMultiOperation
    {
      Operation GetPackedOperation();
    }

    private abstract class Operation
    {
      public abstract void Undo();
      public void Redo(Map map, bool isFirst)
      {
        if (!(this is IMultiOperation))
        {
          Operation packedOperation = null;
          while (map._undoOps?.Count > 0 && map._undoOps.Peek() is IMultiOperation multiOp)
          {
            packedOperation = packedOperation ?? multiOp.GetPackedOperation();
            map._undoOps.Pop();
          }
          if (packedOperation != null)
          { map._undoOps.Push(packedOperation); }
        }
        if (Redo())
        { map.UndoOps.Push(this); }
        if (isFirst)
        { map._redoOps?.Clear(); }
      }
      protected abstract bool Redo();
    }

    private class MoveVertexOperation : Operation
    {
      private readonly Elem _elem;
      private readonly int _vertexIndex;
      private readonly Pnt _newPosition;

      private ISegment _seg0;
      private ISegment _seg1;

      public MoveVertexOperation(Elem elem, int vertexIndex, Pnt newPosition)
      {
        _elem = elem;
        _vertexIndex = vertexIndex;
        _newPosition = newPosition;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          curve[0] = _seg0;
        }
        else if (_vertexIndex == curve.Count)
        {
          curve[_vertexIndex - 1] = _seg1;
        }
        else
        {
          curve[_vertexIndex - 1] = _seg0;
          curve[_vertexIndex] = _seg1;
        }
      }
      protected override bool Redo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          _seg0 = curve[0];
          ISegment clone = _seg0.Clone();
          clone.From = _newPosition;
          curve[0] = clone;
        }
        else if (_vertexIndex == curve.Count)
        {
          _seg1 = curve[_vertexIndex - 1];
          ISegment clone = _seg1.Clone();
          clone.To = _newPosition;
          curve[_vertexIndex - 1] = clone;
        }
        else
        {
          _seg0 = curve[_vertexIndex - 1];
          _seg1 = curve[_vertexIndex];

          ISegment clone0 = _seg0.Clone();
          clone0.To = _newPosition;

          ISegment clone1 = _seg1.Clone();
          clone1.From = _newPosition;

          curve[_vertexIndex - 1] = clone0;
          curve[_vertexIndex] = clone1;
        }

        return true;
      }
    }

    private class RemoveVertexOperation : Operation
    {
      private readonly Elem _elem;
      private readonly int _vertexIndex;

      private ISegment _seg0;
      private ISegment _seg1;

      public RemoveVertexOperation(Elem elem, int vertexIndex)
      {
        _elem = elem;
        _vertexIndex = vertexIndex;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          curve.Insert(0, _seg0);
        }
        else if (_vertexIndex == curve.Count + 1)
        {
          curve.Add(_seg1);
        }
        else
        {
          curve.RemoveAt(_vertexIndex - 1);
          curve.Insert(_vertexIndex - 1, _seg1);
          curve.Insert(_vertexIndex - 1, _seg0);
        }
      }
      protected override bool Redo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_vertexIndex == 0)
        {
          _seg0 = curve[0];
          curve.RemoveAt(0);
        }
        else if (_vertexIndex == curve.Count)
        {
          _seg1 = curve[_vertexIndex - 1];
          curve.RemoveAt(_vertexIndex - 1);
        }
        else
        {
          _seg0 = curve[_vertexIndex - 1];
          _seg1 = curve[_vertexIndex];
          Lin add = new Lin { From = _seg0.From, To = _seg1.To };
          curve.RemoveAt(_vertexIndex - 1);
          curve.RemoveAt(_vertexIndex - 1);
          curve.Insert(_vertexIndex - 1, add);
        }
        return true;
      }
    }

    private class InsertVertexOperation : Operation
    {
      private readonly Elem _elem;
      private readonly float _position;

      private ISegment _orig;
      private int _nSplit;

      public InsertVertexOperation(Elem elem, float position)
      {
        _elem = elem;
        _position = position;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        int pos = (int)_position;

        Curve curve = (Curve)_elem.Geometry;
        for (int i = 0; i < _nSplit; i++)
        { curve.RemoveAt(pos); }
        curve.Insert(pos, _orig);
      }
      protected override bool Redo()
      {
        LastSuccess = false;
        int pos = (int)_position;
        Curve curve = (Curve)_elem.Geometry;
        _orig = curve[pos];

        IList<ISegment> segs = _orig.Split(_position - pos);
        _nSplit = segs.Count;

        curve.RemoveAt(pos);
        for (int i = _nSplit - 1; i >= 0; i--)
        { curve.Insert(pos, segs[i]); }
        LastSuccess = true;
        return true;
      }
    }

    private class FlipOperation : Operation
    {
      private readonly Elem _elem;
      private readonly Curve _orig;

      public FlipOperation(Elem elem)
      {
        _elem = elem;
        _orig = (Curve)elem.Geometry;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Geometry = _orig;
      }
      protected override bool Redo()
      {
        _elem.Geometry = _orig.Flip();
        return true;
      }
    }

    private class SplitOperation : Operation
    {
      private readonly IList<Elem> _elems;
      private readonly Elem _elem;
      private readonly float _position;

      private ISegment _orig;

      public SplitOperation(IList<Elem> elems, Elem elem, float position)
      {
        _elems = elems;
        _elem = elem;
        _position = position;
      }
      public float InsertLimit { get; set; } = 0.01f;
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        int pos = (int)_position;
        float d = _position - pos;

        Curve curve = (Curve)_elem.Geometry;
        Curve newCurve = (Curve)_elems[_elems.Count - 1].Geometry;
        _elems.RemoveAt(_elems.Count - 1);

        if (_orig != null)
        {
          curve.RemoveAt(curve.Count - 1);
          curve.Add(_orig);

          newCurve.RemoveAt(0);
        }
        foreach (var seg in newCurve.Segments)
        { curve.Add(seg); }
      }
      protected override bool Redo()
      {
        Curve curve = (Curve)_elem.Geometry;
        if (_position <= InsertLimit || _position >= curve.Count - InsertLimit)
        { return false; }

        int pos = (int)_position;
        float d = _position - pos;

        Curve newCurve = new Curve();
        ISegment preSplit = null;
        if (d > InsertLimit && d < 1 - InsertLimit)
        {
          _orig = curve[pos];
          IList<ISegment> segs = _orig.Split(_position - pos);
          if (segs.Count == 2)
          {
            preSplit = (segs[0]);
            newCurve.Add(segs[1]);
          }
        }

        if (newCurve.Count == 0)
        {
          newCurve.Add(curve[pos]);
          pos++;
        }

        for (int i = pos + 1; i < curve.Count; i++)
        { newCurve.Add(curve[i]); }

        while (curve.Count > _position)
        { curve.RemoveAt(curve.Count - 1); }

        if (preSplit != null)
        { curve.Add(preSplit); }

        _elems.Add(new Elem(_elem.Symbol, _elem.Color, newCurve));
        return true;
      }
    }

    private class MoveElementOperation : Operation
    {
      private readonly Elem _elem;
      private readonly Pnt _offset;

      private IDrawable _origGeom;

      public MoveElementOperation(Elem elem, Pnt offset)
      {
        _elem = elem;
        _offset = offset;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Geometry = _origGeom;
      }
      protected override bool Redo()
      {
        _origGeom = _elem.Geometry;
        Translation translate = new Translation(_offset);
        _elem.Geometry = _elem.Geometry.Project(translate);
        return true;
      }
    }

    private class SetRotationElementOperation : Operation
    {
      private readonly Elem _elem;
      private readonly float _newAzimuth;

      private Pnt _preGeom;

      public SetRotationElementOperation(Elem elem, float azimuth)
      {
        _elem = elem;
        _newAzimuth = azimuth;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Geometry = _preGeom;
      }
      protected override bool Redo()
      {
        _preGeom = _elem.Geometry as Pnt;
        if (_preGeom == null)
        { return false; }

        _elem.Geometry = DirectedPnt.Create(_preGeom, _newAzimuth);
        return true;
      }
    }

    private class Translation : IProjection
    {
      private readonly Pnt _offset;
      public Translation(Pnt offset)
      { _offset = offset; }
      public Pnt Project(Pnt p)
      { return new Pnt(p.X + _offset.X, p.Y + _offset.Y); }
    }

    private class RemoveElementOperation : Operation
    {
      private readonly Elem _remove;
      private readonly IList<Elem> _elems;
      private int _idx;
      public RemoveElementOperation(IList<Elem> elems, Elem remove)
      {
        _remove = remove;
        _elems = elems;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        if (_idx < 0)
        { return; }
        _elems.Insert(_idx, _remove);
      }
      protected override bool Redo()
      {
        _idx = _elems.IndexOf(_remove);
        LastSuccess = (_idx >= 0);

        if (_idx < 0)
        { return false; }

        _elems.RemoveAt(_idx);
        return true;
      }
    }

    private class EditTextOperation : Operation
    {
      private readonly Elem _elem;
      private readonly string _newText;

      private string _oldText;
      public EditTextOperation(Elem elem, string text)
      {
        _elem = elem;
        _newText = text;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Text = _oldText;
      }
      protected override bool Redo()
      {
        LastSuccess = false;
        _oldText = _elem.Text;

        _elem.Text = _newText;
        LastSuccess = true;
        return true;
      }
    }

    [System.Obsolete("rename")]
    private class SetSymbolOperation_ : Operation
    {
      private readonly Elem _elem;
      private readonly Symbol _newSymbol;
      private readonly ColorRef _newColor;

      private Symbol _oldSymbol;
      private ColorRef _oldColor;

      public SetSymbolOperation_(Elem elem, Symbol symbol, ColorRef color)
      {
        _elem = elem;
        _newSymbol = symbol;
        _newColor = color;
      }
      public bool LastSuccess { get; private set; }
      public override void Undo()
      {
        _elem.Symbol = _oldSymbol;
        _elem.Color = _oldColor;
      }
      protected override bool Redo()
      {
        LastSuccess = false;
        _oldSymbol = _elem.Symbol;
        _oldColor = _elem.Color;

        _elem.Symbol = _newSymbol;
        _elem.Color = _newColor;
        LastSuccess = true;
        return true;
      }
    }

    private class ReshapePackedOperation : Operation
    {
      private readonly Elem _elem;
      private readonly Curve _origGeometry;
      private readonly Curve _reshaped;

      public ReshapePackedOperation(Elem elem, Curve origGeometry, Curve reshaped)
      {
        _elem = elem;
        _origGeometry = origGeometry;
        _reshaped = reshaped;
      }
      protected override bool Redo()
      {
        _elem.Geometry = _reshaped;
        return true;
      }

      public override void Undo()
      { _elem.Geometry = _origGeometry; }
    }

    private class CommitOperation : Operation
    {
      protected override bool Redo()
      { return false; }
      public override void Undo()
      { }
    }

    private class ReshapeOperation : Operation, IMultiOperation
    {
      private readonly Elem _elem;
      private readonly Curve _baseGeometry;
      private readonly Curve _reshapeGeometry;
      private readonly Pnt _add;
      private readonly bool _first;

      public ReshapeOperation(Elem elem, Curve baseGeometry, Curve reshapeGeometry, Pnt add, bool first)
      {
        _elem = elem;
        _baseGeometry = baseGeometry;
        _reshapeGeometry = reshapeGeometry;
        _add = add;
        _first = first;
      }

      protected override bool Redo()
      {
        if (_first)
        { _reshapeGeometry.MoveTo(_add.X, _add.Y); }
        else
        { _reshapeGeometry.LineTo(_add.X, _add.Y); }
        _elem.Geometry = GetReshaped(_baseGeometry, _reshapeGeometry);
        return true;
      }

      public override void Undo()
      {
        if (_first)
        { _elem.Geometry = _baseGeometry; }
        else
        {
          _reshapeGeometry.RemoveLast();
          _elem.Geometry = GetReshaped(_baseGeometry, _reshapeGeometry);
        }
      }

      Operation IMultiOperation.GetPackedOperation()
      { return GetPackedOperation(); }
      public ReshapePackedOperation GetPackedOperation()
      {
        return new ReshapePackedOperation(_elem, _baseGeometry, GetReshaped(_baseGeometry, _reshapeGeometry));
      }

      private Curve GetReshaped(Curve baseGeometry, Curve reshapeGeometry)
      {
        Pnt s = reshapeGeometry.From;

        float alongStart = GetAlong(baseGeometry, s);
        float alongEnd = alongStart;
        if (reshapeGeometry.Count > 0)
        {
          Pnt e = reshapeGeometry[reshapeGeometry.Count - 1].To;
          alongEnd = GetAlong(baseGeometry, e);
          if (alongEnd < alongStart)
          {
            reshapeGeometry = reshapeGeometry.Flip();
            float t = alongStart;
            alongStart = alongEnd;
            alongEnd = t;
          }
        }

        Curve reshaped = new Curve();
        for (int iSeg = 0; iSeg < (int)alongStart; iSeg++)
        {
          reshaped.Add(baseGeometry[iSeg].Clone());
        }
        if (alongStart <= 0)
        { }
        else
        {
          if (alongStart <= 1)
          {
            Pnt p = baseGeometry.From;
            reshaped.MoveTo(p.X, p.Y);
          }
          {
            Pnt p = reshapeGeometry.From;
            reshaped.LineTo(p.X, p.Y);
          }
        }
        foreach (var seg in reshapeGeometry.Segments)
        { reshaped.Add(seg.Clone()); }

        if (alongEnd >= baseGeometry.Count)
        { }
        else
        {
          Pnt p = baseGeometry[(int)alongEnd].To;
          reshaped.LineTo(p.X, p.Y);
          for (int iSeg = (int)alongEnd + 1; iSeg < baseGeometry.Count; iSeg++)
          {
            reshaped.Add(baseGeometry[iSeg].Clone());
          }
        }

        return reshaped;
      }

      private float GetAlong(Curve curve, Pnt p)
      {
        float alongMin = 0;
        float minDist = p.Dist2(curve.From);
        int iSeg = 0;
        foreach (var seg in curve.Segments)
        {
          float along = seg.GetAlong(p);
          if (along >= 0 && along <= 1)
          {
            Pnt at = seg.At(along);
            float distAt = p.Dist2(at);
            if (distAt < minDist)
            {
              alongMin = iSeg + along;
              minDist = distAt;
            }
          }
          else
          {
            float distAt = p.Dist2(seg.To);
            if (distAt < minDist)
            {
              alongMin = iSeg + 1;
              minDist = distAt;
            }
          }
          iSeg++;
        }

        return alongMin;
      }
    }

    private class AddPointOperation : Operation, IMultiOperation
    {
      private readonly float _x;
      private readonly float _y;
      private Elem _currentCurve;
      private readonly bool _first;
      private Map _map;
      public AddPointOperation(Map map, float x, float y)
      {
        _x = x;
        _y = y;
        _map = map;

        _currentCurve = _map._currentCurve;
        _first = (_map._elems.Count == 0 || _map._elems[_map._elems.Count - 1] != _currentCurve);
      }
      protected override bool Redo()
      {
        if (_first)
        {
          ((Curve)_currentCurve.Geometry).MoveTo(_x, _y);
          _map._elems.Add(_currentCurve);
        }
        else
        { ((Curve)_currentCurve.Geometry).LineTo(_x, _y); }
        return true;
      }
      public override void Undo()
      {
        if (_first)
        {
          if (_map._elems.Count > 0 && _map._elems[_map._elems.Count - 1] == _currentCurve)
          {
            _map._elems.RemoveAt(_map._elems.Count - 1);
            _map._currentCurve = null;
          }
          else
          {
            // ERROR!!!
          }
        }
        else
        { ((Curve)_currentCurve.Geometry).RemoveLast(); }
      }

      Operation IMultiOperation.GetPackedOperation()
      { return GetPackedOperation(); }
      public AddElementOperation GetPackedOperation()
      {
        return new AddElementOperation(_currentCurve, _map._elems, null);
      }
    }
    private class AddElementOperation : Operation
    {
      private readonly Elem _elem;
      private IList<Elem> _elems;
      private readonly Elem _currentCurve;
      public AddElementOperation(Elem elem, IList<Elem> elems, Elem currentCurve)
      {
        _elem = elem;
        _elems = elems;
        _currentCurve = currentCurve;
      }

      public override void Undo()
      {
        if (_elems.Count > 0 && _elems[_elems.Count - 1] == _elem)
        {
          _elems.RemoveAt(_elems.Count - 1);
        }
        else
        {
          // ERROR!!!
        }
      }
      protected override bool Redo()
      {
        if (_elem == null)
        { return false; }

        if (_elems.Count == 0 || _elem != _elems[_elems.Count - 1])
        { _elems.Add(_elem); }
        return true;
      }
    }

  }
}
