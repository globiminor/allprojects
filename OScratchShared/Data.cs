using System.Collections.Generic;

namespace OMapScratch
{
  public partial class ColorRef
  {
    public string Id { get; set; }
  }

  public class Elem
  {
    private List<string> _pictures;
    public Elem()
    { }
    public Elem(Symbol symbol, ColorRef color, IDrawable geometry)
    {
      Symbol = symbol;
      Color = color;
      Geometry = geometry;
    }
    public IDrawable Geometry { get; set; }
    public Symbol Symbol { get; set; }
    public ColorRef Color { get; set; }
    public string Text { get; set; }
    public IReadOnlyList<string> Pictures => _pictures;
    public void AddPicture(string picture)
    {
      _pictures = _pictures ?? new List<string>();
      _pictures.Add(picture);
    }
  }

  public static class EventUtils
  {
    public static bool Cancel(object sender, System.ComponentModel.CancelEventHandler cancelEvent)
    {
      if (cancelEvent == null)
      { return false; }
      System.ComponentModel.CancelEventArgs args = new System.ComponentModel.CancelEventArgs();
      cancelEvent.Invoke(sender, args);
      return args.Cancel;
    }
  }
}