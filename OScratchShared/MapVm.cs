
using Basics;
using System;
using System.Collections.Generic;
using System.IO;

namespace OMapScratch
{
  public partial class MapVm
  {
    private readonly Map _map;
    private Pnt _currentLocalLocation;
    private float? _maxSymbolSize;

    public event System.ComponentModel.CancelEventHandler Saving;
    public event EventHandler Saved;
    public event System.ComponentModel.CancelEventHandler Loading;
    public event EventHandler Loaded;

    public MapVm(Map map)
    {
      _map = map;
    }

    public bool HasGlobalLocation()
    {
      return _map.World != null;
    }

    public Pnt GetCurrentLocalLocation()
    {
      return _currentLocalLocation;
    }

    public Pnt SetCurrentLocation(double lat, double lon, double alt, double accuracy)
    {
      XmlWorld w = _map.World;
      if (w == null)
      {
        _currentLocalLocation = null;
        return null;
      }

      double dLat = lat - w.Latitude;
      double dLon = lon - w.Longitude;

      double x = w.GeoMatrix00 * dLon + w.GeoMatrix10 * dLat;
      double y = w.GeoMatrix01 * dLon + w.GeoMatrix11 * dLat;

      _currentLocalLocation = new Pnt((float)x, (float)-y);
      return _currentLocalLocation;
    }

    public float? CurrentOrientation { get; private set; }
    public void SetCurrentOrientation(float? orientation)
    { CurrentOrientation = orientation; }

    public void SetDeclination(float? declination)
    { _map.SetDeclination(declination); }

    private class ActionDistance
    {
      public ContextActions Actions { get; set; }
      public double Distance { get; set; }
      public List<ContextActions> VertexActionsList { get; set; }
    }
    public List<ContextActions> GetContextActions(IMapView view, float x0, float y0, float dx)
    {
      float dd = Math.Max(Math.Abs(dx), _map.MinSearchDistance);
      Box box = new Box(new Pnt(x0 - dd, y0 - dd), new Pnt(x0 + dd, y0 + dd));

      List<ActionDistance> distActions = new List<ActionDistance>();
      foreach (var elem in _map.Elems)
      {
        if (!box.Intersects(elem.Geometry.Extent))
        { continue; }

        Curve curve = elem.Geometry as Curve;

        Pnt elemPnt = null;

        List<ContextActions> vertexActionsList = new List<ContextActions>();
        Pnt down = new Pnt { X = x0, Y = y0 };
        int iVertex = 0;
        float? split = null;
        double elemDist = double.MaxValue;
        foreach (var point in elem.Geometry.GetVertices())
        {
          if (box.Intersects(point))
          {
            double pointDist = down.Dist2(point);
            if (elemPnt == null)
            {
              elemPnt = point;
              elemDist = pointDist;
            }
            else
            {
              if (elemDist > pointDist)
              {
                elemPnt = point;
                elemDist = pointDist;
                if (iVertex > 0 && curve?.Count > iVertex)
                { split = iVertex; }
              }
            }

            if (curve != null)
            {
              List<ContextAction> vertexActions = new List<ContextAction>();
              if (curve.Count > 1)
              { vertexActions.Add(new ContextAction(point, new DeleteVertexAction(_map, elem, iVertex)) { Name = "Delete" }); }

              vertexActions.Add(new ContextAction(point, new MoveVertexAction(view, _map, elem, iVertex)) { Name = "Move" });

              if (iVertex > 0 && iVertex < curve.Count)
              { vertexActions.Add(new ContextAction(point, new SplitAction(_map, elem, iVertex)) { Name = "Split Elem" }); }

              vertexActionsList.Add(new ContextActions($"Vertex #{iVertex}", null, point, vertexActions));
            }
          }
          iVertex++;
        }
        if (curve != null)
        {
          for (int iSeg = 0; iSeg < curve.Count; iSeg++)
          {
            ISegment seg = curve[iSeg];
            Box extent = seg.GetExtent();
            if (box.Intersects(extent))
            {
              float along = seg.GetAlong(down);
              if (along > 0 && along < 1)
              {
                Pnt at = seg.At(along);
                double distAt = down.Dist2(at);
                if (elemPnt == null || elemDist > distAt)
                {
                  elemPnt = at;
                  elemDist = distAt;
                  split = iSeg + along;
                }
              }
            }
          }
        }
        if (elemPnt != null)
        {
          List<ContextAction> elemActions = new List<ContextAction>
          {
            new ContextAction(elemPnt, new DeleteElemAction(_map, elem)) { Name = "Delete" },

            new ContextAction(elemPnt, new MoveElementAction(view, _map, elem, elemPnt)) { Name = "Move", }
          };
          if (curve != null && split != null)
          {
            float at = split.Value - (int)split.Value;
            float limit = 0.01f;
            if (at > limit && at < 1 - limit)
            {
              elemActions.Add(new ContextAction(elemPnt, new InsertVertexAction(view, _map, elem, split.Value)) { Name = "Insert Vertex" });
            }
            elemActions.Add(new ContextAction(elemPnt, new ReshapeAction(view, _map, elem, split.Value)) { Name = "Reshape" });
            elemActions.Add(new ContextAction(elemPnt, new FlipAction(_map, elem)) { Name = "Flip" });
            elemActions.Add(new ContextAction(elemPnt, new SplitAction(_map, elem, split.Value)) { Name = "Split" });
          }
          if (curve == null)
          {
            elemActions.Add(new ContextAction(elemPnt, new RotateElementAction(view, _map, elem, elemPnt)) { Name = "Rotate" });
          }
          elemActions.Add(new ContextAction(elemPnt, new SetSymbolAction_(view, _map, elem)) { Name = "Change Symbol" });
          elemActions.Add(new ContextAction(elemPnt, new EditTextAction(view, _map, elem)) { Name = "Add/Edit Text" });

          distActions.Add(new ActionDistance
          {
            Actions = new ContextActions("Elem", elem, elemPnt, elemActions),
            Distance = elemDist,
            VertexActionsList = vertexActionsList
          });
        }
        else if (vertexActionsList.Count > 0)
        {
          distActions.Add(new ActionDistance
          {
            Distance = elemDist,
            VertexActionsList = vertexActionsList
          });
        }
      }

      distActions.Sort((x, y) => x.Distance.CompareTo(y.Distance));
      List<ContextActions> allActions = new List<ContextActions>();
      foreach (var distAction in distActions)
      {
        if (distAction.Actions != null)
        { allActions.Add(distAction.Actions); }
        allActions.AddRange(distAction.VertexActionsList);
      }
      return allActions;
    }

    public int ImageCount { get { return _map?.Images?.Count ?? 0; } }
    public IReadOnlyList<GeoImageViews> Images => _map.Images ?? new List<GeoImageViews>();
    public IGeoImage CurrentGeoImage => _map.CurrentGeoImage;

    public IEnumerable<Elem> Elems
    { get { return _map.Elems; } }

    public float SymbolScale
    {
      get { return _map.SymbolScale; }
    }

    public ColorRef ConstrColor
    {
      get { return _map.Config?.Data?.ConstrColor?.GetColor(); }
    }
    /// <summary>
    /// Font size of Text Symbol Elements in pt.
    /// </summary>
    /// <returns></returns>
    public float ElemTextSize
    {
      get { return _map.ElemTextSize; }
    }

    /// <summary>
    /// Font size of construction text
    /// </summary>
    /// <returns></returns>
    public float ConstrTextSize
    {
      get { return _map.ConstrTextSize; }
    }

    public float ConstrLineWidth
    {
      get { return _map.ConstrLineWidth; }
    }

    internal void AddPoint(float x, float y, Symbol symbol, ColorRef color)
    { _map.AddPoint(x, y, symbol, color); }
    internal void CommitCurrentOperation()
    { _map.CommitCurrentOperation(); }
    internal void Undo()
    { _map.Undo(); }
    internal void Redo()
    { _map.Redo(); }

    public double[] GetOffset()
    { return _map.GetOffset(); }
    public float? GetDeclination()
    { return _map.GetDeclination(); }
    public double[] GetCurrentWorldMatrix()
    { return _map.GetCurrentWorldMatrix(); }
    public List<Symbol> GetSymbols()
    { return _map.GetSymbols(); }
    public List<ColorRef> GetColors()
    { return _map.GetColors(); }

    public float MaxSymbolSize
    {
      get
      {
        if ((_maxSymbolSize ?? 0) <= 0)
        {
          float maxSymbolSize2 = 0;
          foreach (var sym in GetSymbols())
          {
            if (sym.Curves == null)
            { continue; }

            foreach (var curve in sym.Curves)
            {
              IBox ext = curve.Curve?.Extent;
              if (ext == null)
              { continue; }

              maxSymbolSize2 = Math.Max(maxSymbolSize2, ext.Min.Dist2());
              maxSymbolSize2 = Math.Max(maxSymbolSize2, ext.Max.Dist2());
            }
          }
          _maxSymbolSize = (float)Math.Sqrt(maxSymbolSize2);
        }
        return _maxSymbolSize.Value;
      }
    }

    internal void Save(bool backup = false)
    {
      if (EventUtils.Cancel(this, Saving))
      { return; }
      _map.Save(backup);
      if (!backup)
      { Saved?.Invoke(this, null); }
    }

    public void Load(string configPath)
    {
      Save();
      _maxSymbolSize = null;

      XmlConfig config;
      using (TextReader r = new StreamReader(configPath))
      { Serializer.TryDeserialize(out config, r); }
      if (config != null)
      {
        if (EventUtils.Cancel(this, Loading))
        { return; }
        _map.Load(configPath, config);
        Loaded?.Invoke(this, null);
      }
    }

    public void DrawElems<T>(IGraphics<T> canvas, Box maxExtent,
      float[] elemMatrixValues, float[] dd, IEnumerable<Elem> elems = null) where T: IPaint
    {
      elems = elems ?? Elems;
      if (elems == null)
      { return; }

      using (T p = canvas.CreatePaint())
      {
        p.TextSize = ElemTextSize;
        canvas.Save();
        try
        {
          float[] matrix = elemMatrixValues;

          if (dd != null)
          {
            matrix = (float[])matrix.Clone();
            matrix[2] -= dd[0];
            matrix[5] -= dd[1];
          }

          float symbolScale = SymbolScale;
          MatrixProps matrixProps = new MatrixProps(matrix);

          foreach (var elem in elems)
          {
            if (!maxExtent.Intersects(elem.Geometry.Extent))
            { continue; }

            p.Color = elem.Color;
            elem.Geometry.Draw(canvas, elem.Symbol, matrixProps, symbolScale, p);
          }
        }
        finally
        { canvas.Restore(); }
      }
    }

  }
}
