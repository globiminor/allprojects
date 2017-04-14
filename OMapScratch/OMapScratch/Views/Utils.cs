
using Android.Views;

namespace OMapScratch.Views
{
  public static class Utils
  {
    public static float GetMmPixel(View view)
    {
      return Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Mm, 1, view.Resources.DisplayMetrics);
    }
  }
}