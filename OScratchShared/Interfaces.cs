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

  public partial interface IMapView
  {
    MapVm MapVm { get; }
    IPointAction NextPointAction { get; }

    void SetGetSymbolAction(ISymbolAction setSymbol);
    void SetGetColorAction(IColorAction setSymbol);

    void SetNextPointAction(IPointAction actionWithNextPoint);
    void StartCompass(bool hide = false);

    void ShowText(string text, bool success = true);
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

  public partial interface IDrawable
  {
    IBox Extent { get; }
    string ToText();
    IEnumerable<Pnt> GetVertices();
    IDrawable Project(IProjection prj);
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