using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Basics.Geom
{
  public partial class Mesh
  {
    #region nested classes

    #region LineString enumeration
    private class EnumerableLineString : IEnumerable<LinkedList<MeshLine>>
    {
      private Mesh _mesh;
      private IComparable<MeshLine> _selectComparable;
      private IComparer<MeshLine> _connectComparer;
      public EnumerableLineString(
        Mesh mesh,
        IComparable<MeshLine> selectComparable,
        IComparer<MeshLine> connectComparer)
      {
        _mesh = mesh;
        _selectComparable = selectComparable;
        _connectComparer = connectComparer;
      }

      public IEnumerator<LinkedList<MeshLine>> GetEnumerator()
      {
        return new EnumeratorLineString(_mesh, _selectComparable, _connectComparer);
      }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      { return GetEnumerator(); }
    }

    private class EnumeratorLineString : IEnumerator<LinkedList<MeshLine>>
    {
      private Mesh _mesh;
      private IComparable<MeshLine> _selectComparable;
      private IComparer<MeshLine> _connectComparer;

      private IEnumerator<MeshLine> _lineEnum;
      private LinkedList<MeshLine> _current;

      private uint _processId;

      public EnumeratorLineString(
        Mesh mesh,
        IComparable<MeshLine> selectComparable,
        IComparer<MeshLine> connectComparer)
      {
        _mesh = mesh;
        _selectComparable = selectComparable;
        _connectComparer = connectComparer;

        _processId = _mesh.CaptureLineProcessId();
        Reset();
      }

      public void Dispose()
      {
        while (_lineEnum.MoveNext())
        {
          MeshLineEx line = (MeshLineEx)_lineEnum.Current;
          line.BaseLine.SetProcessed(_processId, false);
        }
        _mesh.ReleaseLineProcessId(_processId);
      }

      public void Reset()
      {
        _lineEnum = _mesh.Lines(null).GetEnumerator();
      }

      object System.Collections.IEnumerator.Current
      { get { return Current; } }
      public LinkedList<MeshLine> Current
      { get { return _current; } }

      public bool MoveNext()
      {
        while (_lineEnum.MoveNext())
        {
          MeshLineEx line = (MeshLineEx)_lineEnum.Current;
          bool processed = line.BaseLine.GetProcessed(_processId);
          if (processed)
          { line.BaseLine.SetProcessed(_processId, false); }

          if (processed == false && _selectComparable.CompareTo(line) == 0)
          {
            _current = LineString(line, _selectComparable, _connectComparer);
            return true;
          }
        }

        _mesh.ReleaseLineProcessId(_processId);
        _processId = 0; // Do not release again in Dispose(), 
                        // may be already captured elsewhere when Dispose is called!
        return false;
      }

      private LinkedList<MeshLine> LineString(MeshLineEx l0,
        IComparable<MeshLine> selectComparable, IComparer<MeshLine> connectComparer)
      {
        LinkedList<MeshLine> lineString = new LinkedList<MeshLine>();

        lineString.AddLast(l0);

        MeshLineEx next = NextSelected(-l0, selectComparable, connectComparer);
        while (next != null && next.BaseLine != l0.BaseLine)
        {
          lineString.AddLast(next);

          next = NextSelected(-next, selectComparable, connectComparer);
        }

        if (next == null)
        {
          next = NextSelected(l0, selectComparable, connectComparer);
          while (next != null)
          {
            Debug.Assert(next.BaseLine != l0.BaseLine, "Error in software design assumption");

            lineString.AddFirst(-next);

            next = NextSelected(-next, selectComparable, connectComparer);
          }
        }
        else
        {
          Debug.Assert(next.BaseLine == l0.BaseLine,
            "Error in software design assumption");
        }

        return lineString;
      }

      private MeshLineEx NextSelected(MeshLineEx l0,
        IComparable<MeshLine> selectComparable, IComparer<MeshLine> connectComparer)
      {
        MeshLineEx l = l0._GetNextPointLine();
        MeshLineEx connect = null;

        do
        {
          if (selectComparable.CompareTo(l) == 0)
          {
            if (connect == null)
            { connect = l; }
            else
            { // multiple selections
              connect = null;
              break;
            }
          }

          l = l._GetNextPointLine();
        }
        while (l.BaseLine != l0.BaseLine);

        if (connect != null && 
          (connectComparer == null || connectComparer.Compare(l0, connect) == 0))
        {
          connect.BaseLine.SetProcessed(_processId, true);
          return connect;
        }
        else
        { return null; }

      }
    }
    #endregion

    #region TriList enumeration

    private class EnumerableTriList : IEnumerable<IList<Tri>>, 
      IEnumerable<IList<MeshLine>>
    {
      private Mesh _mesh;
      private IComparable<Tri> _selectComparable;
      private IComparer<Tri> _connectComparer;
      private bool _border;

      public EnumerableTriList(
        Mesh mesh,
        IComparable<Tri> selectComparable, IComparer<Tri> connectComparer,
        bool border)
      {
        _mesh = mesh;
        _selectComparable = selectComparable;
        _connectComparer = connectComparer;
        _border = border;
      }

      IEnumerator<IList<Tri>> IEnumerable<IList<Tri>>.GetEnumerator()
      {
        Debug.Assert(_border == false, "Error in software design assumption");
        return new EnumeratorTriList(_mesh, _selectComparable, _connectComparer, false);
      }
      IEnumerator<IList<MeshLine>> IEnumerable<IList<MeshLine>>.GetEnumerator()
      {
        Debug.Assert(_border == true, "Error in software design assumption");
        return new EnumeratorTriList(_mesh, _selectComparable, _connectComparer, true);
      }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new EnumeratorTriList(_mesh, _selectComparable, _connectComparer, _border);
      }
    }

    private class EnumeratorTriList : IEnumerator<IList<Tri>>, IEnumerator<IList<MeshLine>>
    {
      private Mesh _mesh;
      private IComparable<Tri> _selectComparable;
      private IComparer<Tri> _connectComparer;
      private bool _border;

      private IEnumerator<Tri> _triEnum;
      private List<Tri> _currentTris;
      private List<MeshLine> _currentBorder;

      private uint _triProcessId;
      private uint _borderProcessId;

      public EnumeratorTriList(
        Mesh mesh,
        IComparable<Tri> selectComparable, IComparer<Tri> connectComparer,
        bool border)
      {
        _mesh = mesh;
        _selectComparable = selectComparable;
        _connectComparer = connectComparer;
        _border = border;

        _triProcessId = _mesh.CaptureTriProcessId();
        Reset();
      }

      public void Dispose()
      {
        GC.SuppressFinalize(this);

        while (_triEnum.MoveNext())
        {
          Tri tri = _triEnum.Current;
          tri.SetProcessed(_triProcessId, false);
        }
        _mesh.ReleaseTriProcessId(_triProcessId);
      }

      public void Reset()
      {
        _triEnum = _mesh.Tris(null).GetEnumerator();
      }

      object System.Collections.IEnumerator.Current
      { 
        get 
        {
          if (_border)
          { return _currentBorder; }
          else
          { return _currentTris; }
        } 
      }
      IList<Tri> IEnumerator<IList<Tri>>.Current
      { get { return _currentTris; } }
      IList<MeshLine> IEnumerator<IList<MeshLine>>.Current
      { get { return _currentBorder; } }

      public bool MoveNext()
      {
        while (_triEnum.MoveNext())
        {
          Tri tri = _triEnum.Current;
          bool processed = tri.GetProcessed(_triProcessId);
          bool bReturn = false;

          if (processed == false && 
            (_selectComparable == null || _selectComparable.CompareTo(tri) == 0))
          {
            _currentTris = new List<Tri>();
            if (_border)
            {
              _currentBorder = new List<MeshLine>();
              _borderProcessId = _mesh.CaptureLineProcessId();
            }

            AddNeighbours((TriEx) tri);

            if (_border)
            {
              BuildBorder();
              _mesh.ReleaseLineProcessId(_borderProcessId);
            }

            bReturn = true;
          }
          tri.SetProcessed(_triProcessId, false);
          if (bReturn)
          { return true; }
        }

        _mesh.ReleaseTriProcessId(_triProcessId);
        return false;
      }

      private void AddNeighbours(TriEx tri0)
      {
        tri0.SetProcessed(_triProcessId, true);
        _currentTris.Add(tri0);

        for (int i = 0; i < 3; i++)
        {
          MeshLineEx border = tri0._GetLine(i);
          TriEx tri = border.RightTriEx;
          if (tri == null)
          {
            if (_border)
            { AddBorder(border); }
            continue; 
          }
          if (tri.GetProcessed(_triProcessId))
          { 
            if (_border && _connectComparer != null &&
              _connectComparer.Compare(tri0, tri) != 0)
            { AddBorder(border); }
            continue; 
          }

          if ((_selectComparable == null || _selectComparable.CompareTo(tri) == 0) &&
            (_connectComparer == null || _connectComparer.Compare(tri0, tri) == 0))
          {
            AddNeighbours(tri);
          }
          else if (_border)
          { AddBorder(border); }
        }
      }

      private void AddBorder(MeshLineEx border)
      {
        border.BaseLine.SetProcessed(_borderProcessId, true);
        _currentBorder.Add(border);
      }

      private void BuildBorder()
      {
        List<MeshLine> sorted = new List<MeshLine>();

        foreach (MeshLineEx border in _currentBorder)
        {
          if (border.BaseLine.GetProcessed(_borderProcessId) == false)
          { continue; }
          sorted.AddRange(Ring(border));
          sorted.Add(null); // mark ring
        }
      }
      private List<MeshLine> Ring(MeshLineEx start)
      {
        List<MeshLine> ring = new List<MeshLine>();
        MeshLineEx l0 = start;

        do
        {
          ring.Add(l0);

          l0.BaseLine.SetProcessed(_borderProcessId, false);
          MeshLineEx l = -l0._GetNextPointLine();
          while (l.BaseLine.GetProcessed(_borderProcessId) == false)
          {
            Debug.Assert(l.HasEqualBaseLine(l0) == false, "Error in software design assumption");
            Debug.Assert(l.LeftTri != null, "Error in software design assumption");

            l = l._GetNextPointLine();
          }
          l0 = l;
        }
        while (l0.HasEqualBaseLine(start) == false);

        return ring;
      }
    }

    #endregion

    #endregion
    private uint _lineProcessId;
    private uint _triProcessId;

    public IEnumerable<LinkedList<MeshLine>> LineStrings(
      IComparable<MeshLine> selectComparable)
    {
      return new EnumerableLineString(this, selectComparable, null);
    }

    public IEnumerable<LinkedList<MeshLine>> LineStrings(
      IComparable<MeshLine> selectComparable,
      IComparer<MeshLine> connectComparer)
    {
      return new EnumerableLineString(this, selectComparable, connectComparer);
    }

    public IEnumerable<IList<Tri>> TriLists(
      IComparable<Tri> selectComparable,
      IComparer<Tri> connectComparer)
    {
      return new EnumerableTriList(this, selectComparable, connectComparer, false);
    }

    public IEnumerable<IList<Tri>> TriLists(
      IComparable<Tri> selectComparable)
    {
      return new EnumerableTriList(this, selectComparable, null, false);
    }

    public IEnumerable<IList<Tri>> TriLists(
      IComparer<Tri> connectComparer)
    {
      return new EnumerableTriList(this, null, connectComparer, false);
    }

    /// <summary>
    /// Get borders of TriLists
    /// </summary>
    /// <param name="selectComparable"></param>
    /// <param name="connectComparer"></param>
    /// <returns></returns>
    public IEnumerable<IList<MeshLine>> TriBorders(
      IComparable<Tri> selectComparable,
      IComparer<Tri> connectComparer)
    {
      return new EnumerableTriList(this, selectComparable, connectComparer, true);
    }



    private uint CaptureLineProcessId()
    {
      return CaptureProcessId(ref _lineProcessId);
    }
    private void ReleaseLineProcessId(uint processId)
    {
      ReleaseProcessId(ref _lineProcessId, processId);
    }

    private uint CaptureTriProcessId()
    {
      return CaptureProcessId(ref _triProcessId);
    }
    private void ReleaseTriProcessId(uint processId)
    {
      ReleaseProcessId(ref _triProcessId, processId);
    }

    private static uint CaptureProcessId(ref uint processId)
    {
      uint id = 1;
      while ((id & processId) != 0)
      { id = id << 1; }

      processId |= id;

      return id;
    }

    private static void ReleaseProcessId(ref uint processId, uint releaseId)
    {
      processId = (processId & releaseId) ^ processId;
    }
  }
}
