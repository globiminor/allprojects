
using Android.Widget;

namespace OMapScratch.Views
{
  internal class ActionButton : Button
  {
    public ContextAction Action { get; }
    public ActionButton(MainActivity context, ContextAction action)
      : base(context)
    {
      Action = action;
      SetAllCaps(false);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(
          Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.WrapContent);
        lprams.Height = (int)(5 * Utils.GetMmPixel(this));
        LayoutParameters = lprams;
      }
      SetPadding(0, 0, 0, 0);
      SetTextSize(Android.Util.ComplexUnitType.Mm, 2);
      SetTextColor(Android.Graphics.Color.AntiqueWhite);
    }

    protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
    {
      base.OnLayout(changed, left, top, right, bottom);
      Gravity = Android.Views.GravityFlags.Left;
      float mm = Utils.GetMmPixel(this);
      SetPadding((int)(1.0f * mm), (int)(0.5f * mm), 0, 0);
    }
  }
}