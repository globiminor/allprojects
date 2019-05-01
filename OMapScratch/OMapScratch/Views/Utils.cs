using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Basics.ViewModels;

namespace Basics.Views
{
  public static partial class Utils
  {
    public static TextView TextInfo { get; set; }
    public static System.Action<bool> TextInfoSuccessAction { get; set; }
    public static string AssemblyId { get; set; }
    public static HorizontalScrollView CreateTextInfo(RelativeLayout parentLayout, string assemblyId)
    {
      HorizontalScrollView scroll = new HorizontalScrollView(parentLayout.Context);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.AlignParentBottom);
        scroll.LayoutParameters = lprams;
      }
      parentLayout.AddView(scroll);

      {
        TextView txtInfo = new TextView(parentLayout.Context);

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        txtInfo.LayoutParameters = lprams;
        txtInfo.Visibility = ViewStates.Invisible;
        txtInfo.SetBackgroundColor(Color.LightGray);
        txtInfo.SetTextColor(Color.Black);
        txtInfo.SetHorizontallyScrolling(true);

        TextInfo = txtInfo;
        scroll.AddView(txtInfo);
      }
      AssemblyId = assemblyId;
      return scroll;
    }

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
              if (!line.Contains(AssemblyId))
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
          c = c.InnerException;
          if (c != null)
          {
            sb.AppendLine();
            sb.AppendLine("[Inner Exception]");
          }
        }
        ShowText(sb.ToString(), false);
      }
    }
    public static void ShowText(string text, bool success = true)
    {
      if (TextInfo == null)
      {
        return;
      }
      if (string.IsNullOrWhiteSpace(text))
      {
        TextInfo.Visibility = ViewStates.Gone;
      }
      else
      {
        TextInfo.Text = text;
        TextInfo.SetTextColor(success ? Color.Black : Color.Red);
        TextInfo.Visibility = ViewStates.Visible;
        TextInfoSuccessAction?.Invoke(success);
      }
      TextInfo.PostInvalidate();
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

  public static class DepProps
  {
    public static Binding<int> BindToProgress(this SeekBar seekBar, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<int> b = new Binding<int>(vm, prop, (t) => seekBar.Progress = t, () => seekBar.Progress);
      seekBar.ProgressChanged += b.ValueChanged;
      b.DisposeFct = () => seekBar.ProgressChanged -= b.ValueChanged;

      return b;
    }

    public static Binding<bool> BindToChecked(this CheckBox checkBox, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<bool> b = new Binding<bool>(vm, prop, (t) => checkBox.Checked = t, () => checkBox.Checked);
      checkBox.CheckedChange += b.ValueChanged;
      b.DisposeFct = () => checkBox.CheckedChange -= b.ValueChanged;

      return b;
    }

    public static Binding<string> BindToText(this TextView textView, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<string> b = new Binding<string>(vm, prop, (t) => textView.Text = t, () => textView.Text);
      textView.TextChanged += b.ValueChanged;
      b.DisposeFct = () => textView.TextChanged -= b.ValueChanged;

      return b;
    }

    public static Binding<bool> BindToEnabled(this View view, BaseVm vm, string prop, BindingMode mode = BindingMode.TwoWay)
    {
      Binding<bool> b = new Binding<bool>(vm, prop, (t) => view.Enabled = t, () =>  view.Enabled);
      return b;
    }

  }

}