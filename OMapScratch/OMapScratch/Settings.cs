
namespace OMapScratch
{
  public enum DrawOptions
  {
    DetailUR, DetailLL, RotL90, RotNorth
  }
  public static class Settings
  {
    public static bool UseLocation { get; set; } = true;
    public static bool UseCompass { get; set; } = false;

    public static DrawOptions DrawOption { get; set; } = DrawOptions.DetailUR;
    public static bool DetailUR { get; set; } = true;
  }
}