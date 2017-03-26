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
    private static Map _staticMap;

    private MapView _mapView;
    private MapVm _map;
    private SymbolButton _btnCurrentSymbol;
    private SymbolGrid _symbolGrid;
    private FileBrowser _browser;
    private LinearLayout _imageList;

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

    private class LoadListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private MainActivity _activity;
      public LoadListener(MainActivity activity)
      {
        _activity = activity;
      }
      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        Java.IO.File store = Environment.ExternalStorageDirectory;
        string path = store.AbsolutePath;
        FileBrowser browser = _activity._browser;
        browser.Filter = new[] { ".config" };
        browser.SetDirectory(path);
        browser.Show((file) => { _activity.MapVm.Load(file); _activity._mapView.Invalidate(); });
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

    int SymbolFullWidth
    {
      get
      {
        var metrics = Resources.DisplayMetrics;
        return System.Math.Min( metrics.WidthPixels, metrics.HeightPixels);
      }
    }

    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);
      SetContentView(Resource.Layout.Main);

      _mapView = new MapView(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        lprams.AddRule(LayoutRules.Below, Resource.Id.btnImages);
        _mapView.LayoutParameters = lprams;
      }
      _mapView.SetAdjustViewBounds(true);
      RelativeLayout parentLayout = FindViewById<RelativeLayout>(Resource.Id.parentLayout);
      parentLayout.AddView(_mapView);

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
        _btnCurrentSymbol = new SymbolButton(MapVm.GetSymbols()[0], this);
        _btnCurrentSymbol.SetMinimumWidth(5);
        _btnCurrentSymbol.SetWidth(SymbolFullWidth / 8);
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.RightOf, Resource.Id.btnImages);
        _btnCurrentSymbol.LayoutParameters = lprams;
        _btnCurrentSymbol.Id = View.GenerateViewId();

        _btnCurrentSymbol.Click += (s, e) =>
        {
          if (_symbolGrid.Visibility == ViewStates.Visible)
          { _symbolGrid.Visibility = ViewStates.Invisible; }
          else
          { _symbolGrid.Visibility = ViewStates.Visible; }
          MapVm.CommitCurrentCurve();
        };

        parentLayout.AddView(_btnCurrentSymbol);
      }

      {
        float dScale = 1.5f;
        ImageButton btnZoomIn = new ImageButton(this);
        btnZoomIn.SetBackgroundResource(Resource.Drawable.ZoomIn);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.RightOf, _btnCurrentSymbol.Id);
          btnZoomIn.LayoutParameters = lprams;
        }
        btnZoomIn.Id = View.GenerateViewId();
        btnZoomIn.Click += (s, a) =>
        {
          _mapView.Scale(dScale);
          _mapView.PostInvalidate();
        };
        parentLayout.AddView(btnZoomIn);

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
        parentLayout.AddView(btnZoomOut);

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
        parentLayout.AddView(btnUndo);

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
        parentLayout.AddView(btnRedo);

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
        parentLayout.AddView(_symbolGrid);
      }

      FileBrowser browser = new FileBrowser(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        browser.LayoutParameters = lprams;
        browser.Visibility = ViewStates.Invisible;
      }
      parentLayout.AddView(browser);
      _browser = browser;

      LinearLayout imageList = new LinearLayout(this);
      imageList.Orientation = Orientation.Vertical;
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        imageList.LayoutParameters = lprams;
        imageList.Visibility = ViewStates.Invisible;
      }
      parentLayout.AddView(imageList);
      _imageList = imageList;

      {
        if (MapVm.CurrentImage == null)
        { MapVm.LoadDefaultImage(); }
        else
        { MapVm.RefreshImage(); }
      }
    }

    private class MotionListener : Java.Lang.Object, View.IOnGenericMotionListener,
      View.IOnTouchListener
    {
      private readonly MainActivity _parent;
      public MotionListener(MainActivity parent)
      {
        _parent = parent;
      }
      public bool OnGenericMotion(View v, MotionEvent e)
      {
        if (e.Action == MotionEventActions.Pointer1Down)
        { }

        if (e.Action == MotionEventActions.HoverEnter || e.Action == MotionEventActions.HoverExit)
        { return false; }
        if (e.Action == MotionEventActions.HoverMove)
        { return false; }
        return false;
      }

      private class Touch
      {
        public float X { get; set; }
        public float Y { get; set; }
        public long Time { get; set; }
        public MotionEventActions Action { get; set; }
        public float Precision { get; set; }

        public static Touch Create(MotionEvent e, int pointId)
        {
          float x = e.RawX;
          float y = e.RawY;
          if (e.PointerCount > pointId)
          {
            MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
            e.GetPointerCoords(pointId, coords);
            x = coords.X;
            y = coords.Y;
          }
          Touch t = new Touch
          {
            X = x,
            Y = y,
            Time = e.EventTime,
            Action = e.Action,
            Precision = (float)System.Math.Sqrt(e.XPrecision * e.XPrecision + e.YPrecision * e.YPrecision)
          };
          return t;
        }
      }

      private Touch _t1Down;
      private Touch _t2Down;
      private Touch _t1Up;
      private Touch _t2Up;

      private void TouchReset()
      {
        _t1Down = null;
        _t2Down = null;
        _t1Up = null;
        _t2Up = null;
      }

      private bool HandleAction()
      {
        if (_t1Down == null)
        {
          TouchReset();
          return true;
        }
        if (_t1Up == null)
        {
          return true;
        }
        if ((_t2Down == null) != (_t2Up == null))
        {
          if (_t2Up != null)
          { TouchReset(); }
          return true;
        }

        Rect rect = new Rect();
        _parent._mapView.GetGlobalVisibleRect(rect);
        if (_t2Down == null)
        {
          float dx = _t1Up.X - _t1Down.X;
          float dy = _t1Up.Y - _t1Down.Y;
          float prec = System.Math.Min(rect.Height(), rect.Width()) / 100f;

          if (System.Math.Abs(dx) <= prec && System.Math.Abs(dy) <= prec)
          {
            float x = (_t1Down.X + _t1Up.X) / 2.0f;
            float y = (_t1Down.Y + _t1Up.Y) / 2.0f;

            _parent._mapView.AddPoint(_parent._btnCurrentSymbol.Symbol, _parent._btnCurrentSymbol.Color, x, y);
          }
          else
          { _parent._mapView.Translate(dx, dy); }
          _parent._mapView.PostInvalidate();

          TouchReset();
          return true;
        }

        {
          float dxDown = _t2Down.X - _t1Down.X;
          float dyDown = _t2Down.Y - _t1Down.Y;
          float lDown = (float)System.Math.Sqrt(dxDown * dxDown + dyDown * dyDown);

          float dxUp = _t2Up.X - _t1Up.X;
          float dyUp = _t2Up.Y - _t1Up.Y;
          float lUp = (float)System.Math.Sqrt(dxUp * dxUp + dyUp * dyUp);
          float scale = lUp / lDown;

          float centerX = (_t1Down.X + _t1Up.X + _t2Down.X + _t2Up.X) / 4 - rect.Left;
          float centerY = (_t1Down.Y + _t1Up.Y + _t2Down.Y + _t2Up.Y) / 4 - rect.Top;

          _parent._mapView.Scale(scale, centerX, centerY);
        }

        TouchReset();
        return true;
      }

      public bool OnTouch(View v, MotionEvent e)
      {
        if (e.Action == MotionEventActions.Down)
        {
          if (_t1Down == null) _t1Down = Touch.Create(e, 0);
          return HandleAction();
        }
        if (e.Action == MotionEventActions.Move)
        { return true; }
        if (e.Action == MotionEventActions.Up)
        {
          if (_t1Up == null) _t1Up = Touch.Create(e, 0);
          else if (_t2Up == null) _t2Up = Touch.Create(e, 0);
          HandleAction();
          TouchReset();
        }
        if (e.Action == MotionEventActions.Pointer2Down)
        {
          _t2Down = Touch.Create(e, 1);
          return HandleAction();
        }
        if (e.Action == MotionEventActions.Pointer2Up)
        {
          _t2Up = Touch.Create(e, 1);
          return HandleAction();
        }
        if (e.Action == MotionEventActions.Pointer1Up)
        {
          _t1Up = Touch.Create(e, 0);
          return HandleAction();
        }
        if (e.Action == MotionEventActions.Pointer1Down)
        {
          _t1Down = Touch.Create(e, 0);
          return HandleAction();
        }

        return false;
      }
    }

  }
}

