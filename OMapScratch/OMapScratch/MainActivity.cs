using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Views;
using System.Collections.Generic;
using OMapScratch.Views;

namespace OMapScratch
{
  [Activity(Label = "O-Scratch", MainLauncher = true, Icon = "@drawable/icon")]
  public partial class MainActivity : Activity
  {
    private class LoadListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private MainActivity _activity;
      public LoadListener(MainActivity activity)
      {
        _activity = activity;
      }
      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        _activity.ShowBrowser();
        return true;
      }
    }
    private class SaveListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private MainActivity _activity;
      public SaveListener(MainActivity activity)
      {
        _activity = activity;
      }
      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        _activity.MapVm.Save();
        return true;
      }

    }

    private static Map _staticMap;

    private RelativeLayout _parentLayout;
    private MapView _mapView;
    private MapVm _map;
    private ModeButton _btnCurrentMode;
    private SymbolGrid _symbolGrid;
    private FileBrowser _browser;
    private LinearLayout _imageList;

    private System.Action<MapButton> _setModeFct;

    public MapView MapView
    { get { return _mapView; } }
    public MapButton BtnCurrentMode
    { get { return _btnCurrentMode; } }

    public RelativeLayout ParentLayout { get { return _parentLayout; } }

    public MapVm MapVm
    {
      get
      {
        if (_map == null)
        {
          _staticMap = _staticMap ?? new Map();
          MapVm map = new MapVm(_staticMap);

          map.ImageChanging += (s, a) =>
          {
            _mapView.PrepareUpdateMapImage();
          };
          map.ImageChanged += (s, a) =>
          {
            _mapView.UpdateMapImage();
            _mapView.PostInvalidate();
          };

          map.Loaded += (s, a) =>
          {
            _symbolGrid.Init(SymbolFullWidth);
            _mapView.ResetMap();

            map.LoadLocalImage(0);
          };
          _map = map;
        }
        return _map;
      }
    }

    private int SymbolFullWidth
    {
      get
      {
        var metrics = Resources.DisplayMetrics;
        return System.Math.Min(metrics.WidthPixels, metrics.HeightPixels);
      }
    }

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      MenuInflater.Inflate(Resource.Layout.Toolbar, menu);

      //var shareMenuItem = menu.FindItem(Resource.Id.shareMenuItem);
      //var shareActionProvider =
      //   (ShareActionProvider)shareMenuItem.ActionProvider;
      //shareActionProvider.SetShareIntent(CreateIntent());

      var loadMenu = menu.FindItem(Resource.Id.mniLoad);
      loadMenu.SetOnMenuItemClickListener(new LoadListener(this));

      var saveMenu = menu.FindItem(Resource.Id.mniSave);
      saveMenu.SetOnMenuItemClickListener(new SaveListener(this));

      return true;
    }
    //Android.Content.Intent CreateIntent()
    //{
    //  var sendPictureIntent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
    //  sendPictureIntent.SetType("image/*");
    //  var uri = Android.Net.Uri.FromFile(GetFileStreamPath("monkey.png"));
    //  sendPictureIntent.PutExtra(Android.Content.Intent.ExtraStream, uri);
    //  return sendPictureIntent;
    //}

    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);
      SetContentView(Resource.Layout.Main);

      _parentLayout = FindViewById<RelativeLayout>(Resource.Id.parentLayout);
      _mapView = new MapView(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        lprams.AddRule(LayoutRules.Below, Resource.Id.btnImages);
        _mapView.LayoutParameters = lprams;
      }
      _parentLayout.AddView(_mapView);
      {
        LinearLayout mapCtxMenu = new LinearLayout(this);
        mapCtxMenu.Orientation = Orientation.Vertical;
        mapCtxMenu.Visibility = ViewStates.Invisible;

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.Below, Resource.Id.btnImages);
        mapCtxMenu.LayoutParameters = lprams;

        _mapView.ContextMenu = mapCtxMenu;
        _parentLayout.AddView(mapCtxMenu);
      }
      {
        TextView txtInfo = new TextView(this);

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.AlignParentBottom);
        txtInfo.LayoutParameters = lprams;
        txtInfo.Visibility = ViewStates.Invisible;
        txtInfo.SetBackgroundColor(Color.LightGray);
        txtInfo.SetTextColor(Color.Black);

        _mapView.TextInfo = txtInfo;
        _parentLayout.AddView(txtInfo);
      }




      Button imageButton = FindViewById<Button>(Resource.Id.btnImages);
      imageButton.Click += (s, e) =>
      {
        _imageList.RemoveAllViews();
        {
          foreach (XmlImage img in MapVm.Images)
          {
            Button btn = new Button(this);
            btn.Text = img.Name;
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            btn.LayoutParameters = lprams;
            btn.Click += (bs, be) =>
            {
              MapVm.LoadLocalImage(img.Path);
              _imageList.Visibility = ViewStates.Invisible;
            };

            _imageList.AddView(btn);
          }
          _imageList.Visibility = ViewStates.Visible;
        }
      };

      {
        _btnCurrentMode = new ModeButton(this, new SymbolButton(MapVm.GetSymbols()[0], this));
        _btnCurrentMode.SetMinimumWidth(5);
        _btnCurrentMode.SetWidth(SymbolFullWidth / 8);
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.RightOf, Resource.Id.btnImages);
        _btnCurrentMode.LayoutParameters = lprams;
        _btnCurrentMode.Id = View.GenerateViewId();

        _btnCurrentMode.Click += (s, e) =>
        {
          _setModeFct = null;
          MapVm.CommitCurrentCurve();
          if (_symbolGrid.Visibility == ViewStates.Visible)
          {
            _symbolGrid.Visibility = ViewStates.Invisible;
          }
          else
          {
            _setModeFct = (btn) =>
            {
              _btnCurrentMode.CurrentMode = btn;
              _btnCurrentMode.PostInvalidate();
            };
            _symbolGrid.Visibility = ViewStates.Visible;
          }
        };

        _parentLayout.AddView(_btnCurrentMode);
      }

      {
        float dScale = 1.5f;
        ImageButton btnZoomIn = new ImageButton(this);
        btnZoomIn.SetBackgroundResource(Resource.Drawable.ZoomIn);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.RightOf, _btnCurrentMode.Id);
          btnZoomIn.LayoutParameters = lprams;
        }
        btnZoomIn.Id = View.GenerateViewId();
        btnZoomIn.Click += (s, a) =>
        {
          _mapView.Scale(dScale);
          _mapView.PostInvalidate();
        };
        _parentLayout.AddView(btnZoomIn);

        ImageButton btnZoomOut = new ImageButton(this);
        btnZoomOut.SetBackgroundResource(Resource.Drawable.ZoomOut);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.RightOf, btnZoomIn.Id);
          btnZoomOut.LayoutParameters = lprams;
        }
        btnZoomOut.Id = View.GenerateViewId();
        btnZoomOut.Click += (s, a) =>
        {
          _mapView.Scale(1 / dScale);
        };
        _parentLayout.AddView(btnZoomOut);

        ImageButton btnUndo = new ImageButton(this);
        btnUndo.SetBackgroundResource(Resource.Drawable.Undo);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.RightOf, btnZoomOut.Id);
          btnUndo.LayoutParameters = lprams;
        }
        btnUndo.Id = View.GenerateViewId();
        btnUndo.Click += (s, a) =>
        {
          MapVm.Undo();
          _mapView.PostInvalidate();
        };
        _parentLayout.AddView(btnUndo);

        ImageButton btnRedo = new ImageButton(this);
        btnRedo.SetBackgroundResource(Resource.Drawable.Redo);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.RightOf, btnUndo.Id);
          btnRedo.LayoutParameters = lprams;
        }
        btnRedo.Id = View.GenerateViewId();
        btnRedo.Click += (s, a) =>
        {
          MapVm.Redo();
          _mapView.PostInvalidate();
        };
        _parentLayout.AddView(btnRedo);

      }

      MotionListener listener = new MotionListener(this);
      _mapView.SetOnTouchListener(listener);
      _mapView.SetOnGenericMotionListener(listener);

      {
        _symbolGrid = new SymbolGrid(this);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.Below, Resource.Id.btnImages);
          _symbolGrid.LayoutParameters = lprams;
        }
        _symbolGrid.Init(SymbolFullWidth);
        _symbolGrid.Visibility = ViewStates.Invisible;
        _parentLayout.AddView(_symbolGrid);
      }

      FileBrowser browser = new FileBrowser(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        browser.LayoutParameters = lprams;
        browser.Visibility = ViewStates.Invisible;
      }
      _parentLayout.AddView(browser);
      _browser = browser;

      LinearLayout imageList = new LinearLayout(this);
      imageList.Orientation = Orientation.Vertical;
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        imageList.LayoutParameters = lprams;
        imageList.Visibility = ViewStates.Invisible;
      }
      _parentLayout.AddView(imageList);
      _imageList = imageList;

      {
        if (MapVm.CurrentImage == null)
        { ShowBrowser(); }
        else
        { MapVm.RefreshImage(); }
      }
    }

    private void ShowBrowser()
    {
      Java.IO.File store = Environment.ExternalStorageDirectory;
      string path = store.AbsolutePath;
      FileBrowser browser = _browser;
      browser.Filter = new[] { ".config" };
      browser.SetDirectory(path);
      browser.Show((file) => { MapVm.Load(file); _mapView.Invalidate(); });
    }

    public void ShowSymbols(System.Action<MapButton> setModeFct)
    {
      _setModeFct = setModeFct;
      _symbolGrid.Visibility = ViewStates.Visible;
      _symbolGrid.PostInvalidate();
    }
    private void SetMode(MapButton modeButton)
    {
      _setModeFct?.Invoke(modeButton);
    }
  }
}

