namespace OMapScratch
{
  public partial class ColorRef
  {
    public string Id { get; set; }
  }

  public class Elem
  {
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