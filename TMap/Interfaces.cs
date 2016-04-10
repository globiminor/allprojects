using System.Collections.Generic;
using Basics.Geom;

namespace TMap
{
	/// <summary>
	/// Summary description for ITMapDrawable.
	/// </summary>
  public interface IDrawable
  {
    void BeginDraw();
    void BeginDraw(MapData data);
    void BeginDraw(ISymbolPart symbolPart);
    bool BreakDraw { get; set; }
    IProjection Projection { get; }
    void DrawLine(Polyline line, ISymbolPart symbolPart);
    void DrawArea(Area area, ISymbolPart symbolPart);
    void DrawRaster(GridMapData raster);
    void Draw(MapData data);
    void Flush();
    Box Extent { get; }
    void SetExtent(IBox proposedExtent);
    void EndDraw(ISymbolPart symbolPart);
    void EndDraw(MapData data);
    void EndDraw();
  }

  public interface ILevelDrawable : IDrawable
  {
    int DrawLevels { get; }
  }

  public interface IContext
  {
    IList<IDrawable> Maps { get; }
    GroupMapData VisibleList();
    GroupMapData Data { get; }
    void Refresh();
    void Draw();
    void SetExtent(IBox extent);
  }
  public interface ICommand
  {
    void Execute(IContext context);
  }
  public interface ITool : ICommand
  {
    void ToolMove(Point pos, int mouse, Point start, Point end);
    void ToolEnd(Point start, Point end);
  }
}
