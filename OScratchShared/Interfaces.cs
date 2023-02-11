using System.Collections.Generic;
using System.Text;

namespace OMapScratch
{
  public interface IAction
  {
    void Action();
  }
  public interface IPointAction
  {
    string Description { get; }
    void Action(Pnt pnt);
  }
  public interface IEditAction : IPointAction
  {
    bool ShowDetail { get; }
  }

  public interface ISymbolAction
  {
    string Description { get; }
    bool Action(Symbol symbol, ColorRef color, out string message);
  }
  public interface IColorAction
  {
    string Description { get; }
    bool Action(ColorRef color);
  }

  public interface ITextAction
  {
    string Description { get; }
    bool Action(string text);
  }

  public interface IPictureAction
  {
    string Description { get; }
    bool Action(string picturePath);
  }


  public partial interface IMapView
  {
    MapVm MapVm { get; }
    IPointAction NextPointAction { get; }

    void SetGetSymbolAction(ISymbolAction setSymbol);
    void SetGetColorAction(IColorAction setSymbol);

    void SetNextPointAction(IPointAction actionWithNextPoint);
    void StartCompass(bool hide = false);

    void ShowText(string text, bool success = true);
    void EditText(string text, ITextAction setText);
    void AddPicture(IPictureAction addPicture);
  }

  public partial interface ISegment
  {
    ISegment Clone();
    Pnt From { get; set; }
    Pnt To { get; set; }

    ISegment Project(IProjection prj);
    ISegment Flip();
    Box GetExtent();
    float GetAlong(Pnt p);
    Pnt At(float t);
    IList<ISegment> Split(float t);

    void InitToText(StringBuilder sb);
    void AppendToText(StringBuilder sb);
  }

  public interface IPathMeasure : System.IDisposable
  {
    float Length { get; }
    Lin GetTangentAt(float dist);
  }

  public interface IGraphics<T>
    where T :IPaint
  {
    T CreatePaint();

    void Save();
    void Restore();

    void Translate(float dx, float dy);
    void Scale(float fx, float fy);
    void Rotate(float rad);

    void DrawPath(Curve path, IProjection toLocal, T p);
    void DrawText(string text, float x0, float y0, T p);

    IPathMeasure GetPathMeasure(Curve path);
  }
  public interface IPaint : System.IDisposable
  {
    ColorRef Color { set; }

    float StrokeWidth { get; set; }
    void SetStyle(bool fill, bool stroke);
    void SetDashEffect(float[] dash, float offset);
    void ResetPathEffect();

    float TextSize { get; set; }
    void TextAlignSetCenter();
  }

  public partial interface IDrawable
  {
    IBox Extent { get; }
    string ToText();
    IEnumerable<Pnt> GetVertices();
    IDrawable Project(IProjection prj);

    void Draw<T>(IGraphics<T> canvas, Symbol symbol, MatrixProps matrix, float symbolScale, T paint)
      where T : IPaint;
  }

  public interface IProjection
  {
    Pnt Project(Pnt pnt);
  }

  public interface IBox
  {
    Pnt Min { get; }
    Pnt Max { get; }
  }

}