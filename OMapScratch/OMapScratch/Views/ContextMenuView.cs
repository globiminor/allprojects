
using Android.Widget;
using Android.Views;

namespace OMapScratch.Views
{
  public class ContextMenuView
  {
    private class OnGlobalLayoutListener : Java.Lang.Object, Android.Views.ViewTreeObserver.IOnGlobalLayoutListener
    {
      private readonly MapView _parent;
      private readonly ContextMenuView _menu;

      public OnGlobalLayoutListener(MapView parent, ContextMenuView menu)
      {
        _menu = menu;
        _parent = parent;
      }
      public void OnGlobalLayout()
      {
        //_parent.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

        int width = _menu._frame.MeasuredWidth;
        int height = _menu._frame.MeasuredHeight;

        float x = _menu._frame.GetX();
        if (width + x > _parent.Width)
        {
          _menu._frame.SetX(x - width);
        }
        float y = _menu._frame.GetY();
        if (height + y > _parent.Height)
        {
          _menu._frame.SetY(_parent.GetY() + _parent.Height - height);
        }
      }
    }

    private readonly LinearLayout _frame;
    private readonly LinearLayout _commands;

    public ContextMenuView(LinearLayout frame, MapView mapView)
    {
      _frame = frame;

      {
        ScrollView scroll = new ScrollView(frame.Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          scroll.LayoutParameters = lprams;
        }
        frame.AddView(scroll);

        {
          LinearLayout commands = new LinearLayout(frame.Context);
          commands.Orientation = Orientation.Vertical;
          commands.ScrollBarStyle = ScrollbarStyles.InsideOverlay;
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            commands.LayoutParameters = lprams;
          }
          scroll.AddView(commands);
          _commands = commands;
        }
      }

      ViewTreeObserver vto = _frame.ViewTreeObserver;
      vto.AddOnGlobalLayoutListener(new OnGlobalLayoutListener(mapView, this));
    }

    public ViewStates Visibility
    {
      get { return _frame.Visibility; }
      set { _frame.Visibility = value; }
    }

    public void RemoveAllViews()
    { _commands.RemoveAllViews(); }

    public void PostInvalidate()
    { _frame.PostInvalidate(); }

    public void AddView(View view)
    { _commands.AddView(view); }

    public void SetX(float X)
    { _frame.SetX(X); }
    public void SetY(float Y)
    { _frame.SetY(Y); }

    public Pnt ToggleMenu(LinearLayout objMenus)
    {
      Pnt editPnt = null;
      int nViews = _commands.ChildCount;
      for (int iView = 0; iView < nViews; iView++)
      {
        LinearLayout view = _commands.GetChildAt(iView) as LinearLayout;
        if (view == null)
        { continue; }
        if (view == objMenus)
        {
          if (view.Visibility == ViewStates.Visible)
          { view.Visibility = ViewStates.Gone; }
          else
          {
            int nMenus = objMenus.ChildCount;
            for (int iMenu = 0; iMenu < nMenus; iMenu++)
            {
              ActionButton btn = objMenus.GetChildAt(iMenu) as ActionButton;
              if (btn != null)
              {
                editPnt = btn.Action.Position;
                break;
              }
            }
            view.Visibility = ViewStates.Visible;
          }
          view.PostInvalidate();
        }
        else if (view.Visibility != ViewStates.Gone)
        {
          view.Visibility = ViewStates.Gone;
          view.PostInvalidate();
        }
      }

      return editPnt;
    }

  }
}