using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using A2048.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2048
{
  [Activity(Label = "A2048", MainLauncher = true, Icon = "@drawable/icon")]
  public class MainActivity : Activity
  {
    private static Grid _grid;
    private RelativeLayout _parentLayout;
    private MainView _mainView;
    private IEnumerator<int> _replay;

    public MainView MainView { get { return _mainView; } }

    public Grid Grid { get { return _grid ?? (_grid = new Grid()); } }

    private class StartListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private readonly MainActivity _activity;
      public StartListener(MainActivity activity)
      { _activity = activity; }

      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        _activity.Grid.Reset();
        _activity._mainView?.PostInvalidate();
        return true;
      }
    }

    private class UndoListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private readonly MainActivity _activity;
      public UndoListener(MainActivity activity)
      { _activity = activity; }

      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        _activity.Grid.Undo();
        _activity._mainView?.PostInvalidate();
        return true;
      }
    }

    private class ReplayListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private readonly MainActivity _activity;
      public ReplayListener(MainActivity activity)
      { _activity = activity; }

      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        _activity._replay = _activity.Grid.Replay().GetEnumerator();
        _activity.ShowReplay();
        return true;
      }
    }

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      MenuInflater.Inflate(Resource.Layout.Options, menu);

      IMenuItem mniStart = menu.FindItem(Resource.Id.mniStart);
      mniStart.SetOnMenuItemClickListener(new StartListener(this));

      IMenuItem mniReplay = menu.FindItem(Resource.Id.mniReplay);
      mniReplay.SetOnMenuItemClickListener(new ReplayListener(this));

      IMenuItem mniUndo = menu.FindItem(Resource.Id.mniUndo);
      mniUndo.SetOnMenuItemClickListener(new UndoListener(this));

      return true;
    }

    public async void ShowReplay()
    {
      while (_replay != null && _replay.MoveNext())
      {
        _mainView?.PostInvalidate();
        await Task.Delay(100);
      }
      _replay = null;
    }

    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);
      SetContentView(Resource.Layout.Main);

      _parentLayout = FindViewById<RelativeLayout>(Resource.Id.parentLayout);
      {
        _mainView = new MainView(this);

        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
          _mainView.Id = View.GenerateViewId();
        }
        _parentLayout.AddView(_mainView);
        Utils.MainView = _mainView;
      }

      MotionListener listener = new MotionListener(this);
      _mainView.SetOnTouchListener(listener);

      {
        HorizontalScrollView scroll = new HorizontalScrollView(this);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.AlignParentBottom);
          scroll.LayoutParameters = lprams;
        }
        _parentLayout.AddView(scroll);

        {
          TextView txtInfo = new TextView(this);

          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          txtInfo.LayoutParameters = lprams;
          txtInfo.Visibility = ViewStates.Invisible;
          txtInfo.SetBackgroundColor(Color.LightGray);
          txtInfo.SetTextColor(Color.Black);
          txtInfo.SetHorizontallyScrolling(true);

          _mainView.TextInfo = txtInfo;
          scroll.AddView(txtInfo);
        }
      }
    }
  }
}

