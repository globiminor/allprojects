using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using ToDos.Shared;
using ToDos.Views;

namespace ToDos
{
  [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
  public class MainActivity : AppCompatActivity, IToDosView
  {
    private RelativeLayout _parentLayout;
    private ToDosView _toDos;

    protected override void OnCreate(Bundle savedInstanceState)
    {
      base.OnCreate(savedInstanceState);
      SetContentView(Resource.Layout.activity_main);

      Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
      SetSupportActionBar(toolbar);

      FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
      fab.Click += FabOnClick;

      _parentLayout = FindViewById<RelativeLayout>(Resource.Id.parentLayout);

      ScrollView scroll = new ScrollView(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        scroll.LayoutParameters = lprams;
      }
      _parentLayout.AddView(scroll);


      _toDos = ToDosView.Create(this);
      _toDos.Orientation = Orientation.Vertical;
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        _toDos.LayoutParameters = lprams;
      }
      _toDos.Vm = ToDosVm;

      scroll.AddView(_toDos);
    }

    void IToDosView.InvalidateOrder()
    {
      _toDos.InvalidateOrder();
    }

    private static ToDosVm _toDosVm;
    public ToDosVm ToDosVm
    {
      get
      {
        if (_toDosVm == null)
        {
          _toDosVm = ToDosVm.Test(); 
        }
        return _toDosVm;
      }
    }

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      MenuInflater.Inflate(Resource.Menu.menu_main, menu);
      return true;
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
      int id = item.ItemId;
      if (id == Resource.Id.action_settings)
      {
        return true;
      }

      return base.OnOptionsItemSelected(item);
    }

    private void FabOnClick(object sender, EventArgs eventArgs)
    {
      View view = (View)sender;
      Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
          .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
    }
  }
}

