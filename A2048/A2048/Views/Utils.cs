using Android.Runtime;
using Android.Views;


namespace A2048.Views
{
  public static class Utils
  {
    public static IMainView MainView { get; set; }
    public static void Try(System.Action action)
    {
      try
      {
        action();
      }
      catch (System.Exception e)
      {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Exception:");
        System.Exception c = e;
        while (c != null)
        {
          sb.AppendLine(c.Message);
          //sb.AppendLine(c.StackTrace);
          string pre = null;
          using (System.IO.TextReader r = new System.IO.StringReader(c.StackTrace))
          {
            for (string line = r.ReadLine(); line != null; line = r.ReadLine())
            {
              if (string.IsNullOrWhiteSpace(line))
              { continue; }
              if (!line.Contains("A2048"))
              {
                pre = line;
                continue;
              }
              if (pre != null)
              { sb.AppendLine(pre); }
              sb.AppendLine(line);
              pre = null;
            }
          }
          c = e.InnerException;
          if (c != null)
          {
            sb.AppendLine();
            sb.AppendLine("[Inner Exception]");
          }
        }
        MainView?.ShowText(sb.ToString(), false);
      }
    }
    public static float GetMmPixel(View view)
    {
      return GetMmPixel(view.Resources);
    }
    public static float GetMmPixel(Android.Content.Res.Resources res)
    {
      return Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Mm, 1, res.DisplayMetrics);
    }

    public static float GetSurfaceOrientation()
    {
      IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
      SurfaceOrientation o = windowManager.DefaultDisplay.Rotation;

      float angle;
      if (o == SurfaceOrientation.Rotation0)
      { angle = 90; }
      else if (o == SurfaceOrientation.Rotation90)
      { angle = 180; }
      else if (o == SurfaceOrientation.Rotation180)
      { angle = 270; }
      else if (o == SurfaceOrientation.Rotation270)
      { angle = 0; }
      else
      { angle = 90; }
      return angle;
    }
  }
}