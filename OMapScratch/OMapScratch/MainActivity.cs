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
  public class MainActivity : Activity
  {
    private MapView _mapView;
    private static Map _map;
    private SymbolButton _btnCurrentSymbol;
    private FileBrowser _browser;
    private LinearLayout _imageList;

    public Map Map
    {
      get
      {
        if (_map == null)
        {
          Map map = new Map();
          map.OnImageChanged += (s, a) =>
          {
            _mapView.SetScaleType(ImageView.ScaleType.Matrix);
            Matrix m = new Matrix();
            _mapView.ImageMatrix = m;

            _mapView.SetImageBitmap(Map.CurrentImage);
            _mapView.ImageMatrix.SetScale(2, 2);

            m.SetTranslate(10, 10);
            _mapView.ImageMatrix = m;
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
        browser.Show((file) => { _activity.Map.Load(file); _activity._mapView.Invalidate(); });
        return true;
      }

    }
    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      MenuInflater.Inflate(Resource.Layout.Toolbar, menu);

      var shareMenuItem = menu.FindItem(Resource.Id.shareMenuItem);
      var shareActionProvider =
         (ShareActionProvider)shareMenuItem.ActionProvider;
      shareActionProvider.SetShareIntent(CreateIntent());

      var loadMenu = menu.FindItem(Resource.Id.mniLoad);

      var loadListener = new LoadListener(this);
      loadMenu.SetOnMenuItemClickListener(loadListener);
      return true;
    }
    Android.Content.Intent CreateIntent()
    {
      var sendPictureIntent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
      sendPictureIntent.SetType("image/*");
      var uri = Android.Net.Uri.FromFile(GetFileStreamPath("monkey.png"));
      sendPictureIntent.PutExtra(Android.Content.Intent.ExtraStream, uri);
      return sendPictureIntent;
    }
    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      //SetContentView(Resource.Layout.FileBrowser);
      //return;

      // Set our view from the "main" layout resource
      SetContentView(Resource.Layout.Main);

      //Toolbar actionbar = FindViewById<Toolbar>(Resource.Id.toolbar);
      //SetActionBar(actionbar);
      //ActionBar.Title = "Gugus";
      ActionBar x = ActionBar;

      var metrics = Resources.DisplayMetrics;

      _mapView = new MapView(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        lprams.AddRule(LayoutRules.Below, Resource.Id.btnImages);
        _mapView.LayoutParameters = lprams;
      }
      _mapView.SetAdjustViewBounds(true);
      {
        Bitmap img = Map.LoadDefaultImage();
      }

      RelativeLayout parentLayout = FindViewById<RelativeLayout>(Resource.Id.parentLayout);
      parentLayout.AddView(_mapView);

      List<Color> colors = GetColors();
      List<Symbol> symbols = Map.GetSymbols();

      GridLayout commands = new GridLayout(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.Below, Resource.Id.btnImages);
        commands.LayoutParameters = lprams;
      }
      commands.ColumnCount = colors.Count;
      commands.RowCount = 2 + (symbols.Count - 1) / colors.Count;
      List<SymbolButton> symBtns = new List<SymbolButton>();
      foreach (Color color in colors)
      {
        Button btnColor = new Button(this);
        btnColor.SetMinimumWidth(5);
        btnColor.SetWidth(metrics.WidthPixels / colors.Count);
        btnColor.SetBackgroundColor(color);
        commands.AddView(btnColor);

        btnColor.Click += (s, a) =>
        {
          foreach (SymbolButton btn in symBtns)
          {
            btn.Color = new ColorRef { Color = color };
            btn.PostInvalidate();
          }
        };
      }
      foreach (Symbol sym in symbols)
      {
        SymbolButton btnSym = new SymbolButton(sym, this);
        btnSym.SetMinimumWidth(5);
        btnSym.SetWidth(metrics.WidthPixels / colors.Count);

        btnSym.Click += (s, e) =>
        {
          _btnCurrentSymbol.Symbol = btnSym.Symbol;
          _btnCurrentSymbol.Color = btnSym.Color;
          _btnCurrentSymbol.PostInvalidate();
          commands.Visibility = ViewStates.Invisible;
        };

        commands.AddView(btnSym);
        symBtns.Add(btnSym);
      }
      commands.Visibility = ViewStates.Invisible;
      parentLayout.AddView(commands);

      Button imageButton = FindViewById<Button>(Resource.Id.btnImages);
      imageButton.Click += (s, e) =>
      {
        _imageList.RemoveAllViews();
        if (Map.Images?.Count > 0)
        {
          foreach (XmlImage img in Map.Images)
          {
            Button btn = new Button(this);
            btn.Text = img.Name;
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            btn.LayoutParameters = lprams;
            btn.Click += (bs, be) =>
            {
              Map.LoadLocalImage(img.Path);
              _imageList.Visibility = ViewStates.Invisible;
            };

            _imageList.AddView(btn);
          }
          _imageList.Visibility = ViewStates.Visible;
        }
      };

      {
        _btnCurrentSymbol = new SymbolButton(symbols[0], this);
        _btnCurrentSymbol.SetMinimumWidth(5);
        _btnCurrentSymbol.SetWidth(metrics.WidthPixels / colors.Count);
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.RightOf, Resource.Id.btnImages);
        _btnCurrentSymbol.LayoutParameters = lprams;
        _btnCurrentSymbol.Id = View.GenerateViewId();

        _btnCurrentSymbol.Click += (s, e) =>
        {
          if (commands.Visibility == ViewStates.Visible)
          { commands.Visibility = ViewStates.Invisible; }
          else
          { commands.Visibility = ViewStates.Visible; }
          Map.CommitCurrentCurve();
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
          Map.Undo();
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
          Map.Redo();
          _mapView.PostInvalidate();
        };
        parentLayout.AddView(btnRedo);

      }

      MotionListener listener = new MotionListener(this);
      _mapView.SetOnTouchListener(listener);
      _mapView.SetOnGenericMotionListener(listener);

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
    }

    private List<Color> GetColors()
    {
      return new List<Color> { Color.Black, Color.Gray, Color.Brown, Color.Yellow, Color.Green, Color.Khaki, Color.Red, Color.Blue };
    }

    private class MotionListener : Java.Lang.Object, Android.Views.View.IOnGenericMotionListener,
      Android.Views.View.IOnTouchListener
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
        public MotionEventActions Action {get; set; }
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
            Matrix inverse = new Matrix();
            _parent._mapView.ImageMatrix.Invert(inverse);

            float x = (_t1Down.X + _t1Up.X) / 2.0f;
            float y = (_t1Down.Y + _t1Up.Y) / 2.0f;
//            float[] inverted = { x - rect.Left, y - rect.Top };
            float[] inverted = { x - rect.Left, y };
            inverse.MapPoints(inverted);

            _parent.Map.AddPoint(inverted[0], inverted[1],
              _parent._btnCurrentSymbol.Symbol, _parent._btnCurrentSymbol.Color);
          }
          else
          { _parent._mapView.ImageMatrix.PostTranslate(dx, dy); }
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

