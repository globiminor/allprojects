using System.Windows.Media;

namespace OMapScratch
{
  public partial class Map
  {
    private System.Collections.Generic.List<ColorRef> GetDefaultColors()
    {
      return new System.Collections.Generic.List<ColorRef>
      {
        new ColorRef { Id = "Bl", Color = Colors.Black },
        new ColorRef { Id = "Gy", Color = Colors.Gray },
        new ColorRef { Id = "Bw", Color = Colors.Brown },
        new ColorRef { Id = "Y", Color = Colors.Yellow },
        new ColorRef { Id = "G", Color = Colors.Green },
        new ColorRef { Id = "K", Color = Colors.Khaki },
        new ColorRef { Id = "R", Color = Colors.Red },
        new ColorRef { Id = "B", Color = Colors.Blue } };
    }
  }
  public partial class ColorRef
  {
    public Color Color { get; set; }
  }
  public partial class XmlColor
  {
    private void GetEnvColor(ColorRef color)
    {
      color.Color = new Color { R = Red, G = Green, B = Blue };
    }

    private void SetEnvColor(ColorRef color)
    {
      Red = color.Color.R;
      Green = color.Color.G;
      Blue = color.Color.B;
    }
  }
}