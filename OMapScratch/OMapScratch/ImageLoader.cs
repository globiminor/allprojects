
namespace OMapScratch
{
  public static class ImageLoader
  {
    public static int GetResource(DrawOptions drawOption)
    {
      if (drawOption == DrawOptions.DetailUR)
      {
        return Resource.Drawable.DetailUr;
      }
      else if (drawOption == DrawOptions.DetailLL)
      {
        return Resource.Drawable.DetailLl;
      }
      else if (drawOption == DrawOptions.RotL90)
      {
        return Resource.Drawable.RotL90;
      }
      else if (drawOption == DrawOptions.RotNorth)
      {
        return Resource.Drawable.RotNorth;
      }
      else if (drawOption == DrawOptions.RotCompass)
      {
        return Resource.Drawable.RotCompass;
      }

      return Resource.Drawable.DetailUr;
    }
  }
}