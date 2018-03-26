using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Views;
using OMapScratch.Views;
using Android.Locations;
using OMapScratch.ViewModels;

namespace OMapScratch
{
  [Activity(Label = "O-Scratch", MainLauncher = true, Icon = "@drawable/icon")]
  public partial class MainActivity : Activity, ILocationContext, ICompassContext
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
        Utils.Try(() => { _activity.ShowBrowser(); });
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
        Utils.Try(() =>
        { _activity.MapVm.Save(); });
        return true;
      }
    }
    private class ExportImgListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
      private MainActivity _activity;
      public ExportImgListener(MainActivity activity)
      {
        _activity = activity;
      }
      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        Utils.Try(() =>
        {
          string imgPath = _activity._mapView.ProcessScratchImg((b, m) => _activity.MapVm.SaveImg(b, m));
          if (!string.IsNullOrEmpty(imgPath))
          {
            string defaultPath = Environment.ExternalStorageDirectory.AbsolutePath;
            string displayPath = imgPath;
            if (displayPath.StartsWith(defaultPath, System.StringComparison.InvariantCultureIgnoreCase))
            { displayPath = displayPath.Substring(defaultPath.Length).TrimStart('/'); }
            _activity._mapView.ShowText($"Image saved to {displayPath}");
          }
        });
        return true;
      }
    }

    private class LocationListener : Java.Lang.Object,
      IMenuItemOnMenuItemClickListener
    {
      private MainActivity _activity;
      public LocationListener(MainActivity activity)
      {
        _activity = activity;
      }

      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        Utils.Try(() =>
        {
          if (Settings.UseLocation)
          {
            Settings.UseLocation = false;
            _activity.LocationVm.PauseLocation();
          }
          else
          {
            if (!_activity.MapVm.HasGlobalLocation())
            {
              _activity.MapView.ShowText("Unknown Georeference, please set location (Edit tool -> map click -> set location).", false);
            }
            else
            {
              Settings.UseLocation = true;
              _activity.LocationVm.StartLocation(false);
            }
          }
        });
        return true;
      }
    }

    private class CompassListener : Java.Lang.Object,
      IMenuItemOnMenuItemClickListener
    {
      private MainActivity _activity;
      public CompassListener(MainActivity activity)
      {
        _activity = activity;
      }

      bool IMenuItemOnMenuItemClickListener.OnMenuItemClick(IMenuItem item)
      {
        if (Settings.UseCompass)
        {
          Settings.UseCompass = false;
          _activity.CompassVm.StopCompass();
        }
        else
        {
          //if (_activity.MapVm.GetDeclination() == null)
          //{
          //  _activity.MapView.ShowText("Unknown Declination, please set orientation (Edit tool -> map click -> set orientation).", false);
          //}
          //else
          //{
          Settings.UseCompass = true;
          _activity.CompassVm.StartCompass();
          //          }
        }
        return true;
      }
    }

    private static Map _staticMap;

    private RelativeLayout _parentLayout;
    private MapView _mapView;
    private MapVm _map;
    private ModeButton _btnCurrentMode;
    private SymbolGrid _symbolGrid;
    private RotateModeGrid _rotateGrid;

    private RecentFileBrowser _browser;
    private LinearLayout _imageList;

    private CheckBox chkDrawOnly = null;

    private System.Action<MapButton> _setModeFct;

    public MapView MapView
    { get { return _mapView; } }
    public ModeButton BtnCurrentMode
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
            _mapView.ConstrView.ResetMap();

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
        return metrics.WidthPixels;
        //        return System.Math.Min(metrics.WidthPixels, metrics.HeightPixels);
      }
    }

    private IMenu _optionsMenu;
    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      MenuInflater.Inflate(Resource.Layout.Toolbar, menu);
      _optionsMenu = menu;

      //var shareMenuItem = menu.FindItem(Resource.Id.shareMenuItem);
      //var shareActionProvider =
      //   (ShareActionProvider)shareMenuItem.ActionProvider;
      //shareActionProvider.SetShareIntent(CreateIntent());

      var loadMenu = menu.FindItem(Resource.Id.mniLoad);
      loadMenu.SetOnMenuItemClickListener(new LoadListener(this));

      var saveMenu = menu.FindItem(Resource.Id.mniSave);
      saveMenu.SetOnMenuItemClickListener(new SaveListener(this));

      var exportImgMenu = menu.FindItem(Resource.Id.mniExportImg);
      exportImgMenu.SetOnMenuItemClickListener(new ExportImgListener(this));

      var locationMenu = menu.FindItem(Resource.Id.mniLocation);
      locationMenu.SetOnMenuItemClickListener(new LocationListener(this));

      var orientationMenu = menu.FindItem(Resource.Id.mniOrientation);
      orientationMenu.SetOnMenuItemClickListener(new CompassListener(this));

      return true;
    }

    public override bool OnMenuOpened(int id, IMenu menu)
    {
      if (menu == null && _optionsMenu != null)
      {
        var locationMenu = _optionsMenu.FindItem(Resource.Id.mniLocation);
        locationMenu.SetTitle(Settings.UseLocation ? "Hide Location" : "Show Location");

        var orientationMenu = _optionsMenu.FindItem(Resource.Id.mniOrientation);
        orientationMenu.SetTitle(Settings.UseCompass ? "Hide Compass" : "Show Compass");
      }
      return base.OnMenuOpened(id, menu);
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
        lprams.AddRule(LayoutRules.Below, Resource.Id.lloTools);
        _mapView.LayoutParameters = lprams;
        _mapView.Id = View.GenerateViewId();
      }
      _parentLayout.AddView(_mapView);
      Utils.MapView = _mapView;
      ConstrView constrView = new ConstrView(_mapView);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        lprams.AddRule(LayoutRules.AlignTop, _mapView.Id);
        constrView.LayoutParameters = lprams;
      }
      _parentLayout.AddView(constrView);

      {
        LinearLayout mapCtxMenu = new LinearLayout(this)
        {
          Orientation = Orientation.Vertical,
          Visibility = ViewStates.Invisible
        };

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.AddRule(LayoutRules.Below, Resource.Id.lloTools);
        mapCtxMenu.LayoutParameters = lprams;

        _mapView.ContextMenu = new ContextMenuView(mapCtxMenu, _mapView);
        _parentLayout.AddView(mapCtxMenu);
      }
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

          _mapView.TextInfo = txtInfo;
          scroll.AddView(txtInfo);
        }
      }

      LinearLayout lloTools = FindViewById<LinearLayout>(Resource.Id.lloTools);

      Button imageButton = FindViewById<Button>(Resource.Id.btnImages);
      //ImageButton imageButton = FindViewById<ImageButton>(Resource.Id.btnImages);
      imageButton.Click += (s, e) => Utils.Try(() =>
      {
        _imageList.RemoveAllViews();
        {
          foreach (XmlImage img in MapVm.Images)
          {
            Button btn = new Button(this)
            { Text = img.Name };
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            btn.LayoutParameters = lprams;
            btn.Click += (bs, be) => Utils.Try(() =>
            {
              _imageList.Visibility = ViewStates.Invisible;
              MapVm.LoadLocalImage(img.Path);
            });

            _imageList.AddView(btn);
          }
          _imageList.Visibility = ViewStates.Visible;
        }
      });
      if (!(_mapView?.MapVm?.ImageCount > 1))
      {
        imageButton.Visibility = ViewStates.Gone;
      }

      {
        _btnCurrentMode = new ModeButton(this, new SymbolButton(MapVm.GetSymbols()[0], this));
        _btnCurrentMode.SetMinimumWidth(5);
        //        _btnCurrentMode.SetWidth(SymbolFullWidth / 8);
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        _btnCurrentMode.LayoutParameters = lprams;
        _btnCurrentMode.Id = View.GenerateViewId();

        _btnCurrentMode.Click += (s, e) => Utils.Try(() =>
        {
          _setModeFct = null;
          MapView.ResetContextMenu(clearMenu: true);
          MapVm.CommitCurrentOperation();
          if (_symbolGrid.Visibility == ViewStates.Visible)
          {
            _symbolGrid.Visibility = ViewStates.Invisible;
          }
          else
          {
            ShowSymbols((btn) =>
            {
              _btnCurrentMode.CurrentMode = btn;
              _btnCurrentMode.PostInvalidate();
            });
            _symbolGrid.Visibility = ViewStates.Visible;
          }
        });

        lloTools.AddView(_btnCurrentMode);
      }

      {

        float dScale = 1.5f;
        {
          ImageButton btnZoomIn = new ImageButton(this);
          btnZoomIn.SetBackgroundResource(Resource.Drawable.ZoomIn);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            lprams.MarginStart = (int)(0.2 * Utils.GetMmPixel(btnZoomIn));
            btnZoomIn.LayoutParameters = lprams;
          }
          btnZoomIn.Id = View.GenerateViewId();
          btnZoomIn.Click += (s, a) =>
          {
            _mapView.Scale(dScale);
            _mapView.PostInvalidate();
          };
          lloTools.AddView(btnZoomIn);
        }
        {
          ImageButton btnZoomOut = new ImageButton(this);
          btnZoomOut.SetBackgroundResource(Resource.Drawable.ZoomOut);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            lprams.MarginStart = (int)(0.2 * Utils.GetMmPixel(btnZoomOut));
            btnZoomOut.LayoutParameters = lprams;
          }
          btnZoomOut.Id = View.GenerateViewId();
          btnZoomOut.Click += (s, a) =>
          {
            _mapView.Scale(1 / dScale);
          };
          lloTools.AddView(btnZoomOut);
        }

        {
          ImageButton btnUndo = new ImageButton(this);
          btnUndo.SetBackgroundResource(Resource.Drawable.Undo);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            lprams.MarginStart = (int)(0.2 * Utils.GetMmPixel(btnUndo));
            btnUndo.LayoutParameters = lprams;
          }
          btnUndo.Id = View.GenerateViewId();
          btnUndo.Click += (s, a) => Utils.Try(() =>
          {
            MapVm.Undo();
            _mapView.PostInvalidate();
          });
          lloTools.AddView(btnUndo);
        }

        {
          ImageButton btnRedo = new ImageButton(this);
          btnRedo.SetBackgroundResource(Resource.Drawable.Redo);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            lprams.MarginStart = (int)(0.2 * Utils.GetMmPixel(btnRedo));
            btnRedo.LayoutParameters = lprams;
          }
          btnRedo.Id = View.GenerateViewId();
          btnRedo.Click += (s, a) => Utils.Try(() =>
          {
            MapVm.Redo();
            _mapView.PostInvalidate();
          });
          lloTools.AddView(btnRedo);
        }

        //{
        //  chkDrawOnly = new CheckBox(this);
        //  RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        //  chkDrawOnly.LayoutParameters = lprams;
        //  chkDrawOnly.Text = " ";
        //  chkDrawOnly.SetButtonDrawable(Resource.Drawable.ExtentMove);
        //  chkDrawOnly.Click += (s, e) =>
        //  {
        //    chkDrawOnly.Text = " ";
        //    if (chkDrawOnly.Checked)
        //    {
        //      chkDrawOnly.SetButtonDrawable(Resource.Drawable.ExtentFix);
        //    }
        //    else
        //    {
        //      chkDrawOnly.SetButtonDrawable(Resource.Drawable.ExtentMove);
        //    }
        //  };

        //  chkDrawOnly.Id = View.GenerateViewId();

        //  lloTools.AddView(chkDrawOnly);
        //}

        {
          ImageButton btnRotation = new ImageButton(this);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            lprams.MarginStart = (int)(0.2 * Utils.GetMmPixel(btnRotation));
            btnRotation.LayoutParameters = lprams;
          }
          btnRotation.SetBackgroundResource(ImageLoader.GetResource(Settings.DrawOption));
          btnRotation.Click += (s, e) =>
          {
            if (_rotateGrid.Visibility == ViewStates.Visible)
            {
              _rotateGrid.Visibility = ViewStates.Invisible;
              return;
            }

            if (Settings.DrawOption == DrawOptions.DetailUR)
            {
              Settings.DrawOption = DrawOptions.DetailLL;
              Settings.DetailUR = false;
              btnRotation.SetBackgroundResource(ImageLoader.GetResource(Settings.DrawOption));
            }
            else if (Settings.DrawOption == DrawOptions.DetailLL)
            {
              Settings.DrawOption = DrawOptions.DetailUR;
              Settings.DetailUR = true;
              btnRotation.SetBackgroundResource(ImageLoader.GetResource(Settings.DrawOption));
            }
            else
            {
              _rotateGrid.Apply(Settings.DrawOption);
            }
          };
          btnRotation.LongClick += (s, e) =>
          {
            _rotateGrid.ShowAll(btnRotation);
            _rotateGrid.Visibility = ViewStates.Visible;
            _rotateGrid.PostInvalidate();
          };

          btnRotation.Id = View.GenerateViewId();

          lloTools.AddView(btnRotation);

          {
            _rotateGrid = new RotateModeGrid(this);
            {
              RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
              lprams.AddRule(LayoutRules.Below, Resource.Id.lloTools);
              _rotateGrid.LayoutParameters = lprams;
            }
            _rotateGrid.Init();
            _rotateGrid.Visibility = ViewStates.Invisible;
            _parentLayout.AddView(_rotateGrid);
          }

        }

        {
          View dummy = new View(this);
          {
            LinearLayout.LayoutParams lprams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
            dummy.LayoutParameters = lprams;
          }
          lloTools.AddView(dummy);
        }

        {
          ImageButton btnLoc = new ImageButton(this);
          btnLoc.SetBackgroundResource(Resource.Drawable.Location);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            btnLoc.LayoutParameters = lprams;
          }
          btnLoc.Id = View.GenerateViewId();
          btnLoc.Click += (s, a) => Utils.Try(() =>
          {
            Pnt pnt = MapView.GetCurrentMapLocation();
            if (pnt != null)
            {
              _btnCurrentMode.MapClicked(pnt.X, pnt.Y);
              MapView.PostInvalidate();
            }
            else
            {
              MapView.ShowText("Unknown location. Ensure location is active and set", false);
            }
          });
          lloTools.AddView(btnLoc);
        }

      }

      MotionListener listener = new MotionListener(this);
      _mapView.LongClickable = true;
      _mapView.SetOnTouchListener(listener);
      //_mapView.SetOnGenericMotionListener(listener);
      //_mapView.SetOnHoverListener(listener);
      //_mapView.SetOnLongClickListener(listener);

      {
        _symbolGrid = new SymbolGrid(this);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.AddRule(LayoutRules.Below, Resource.Id.lloTools);
          _symbolGrid.LayoutParameters = lprams;
        }
        _symbolGrid.Init(SymbolFullWidth);
        _symbolGrid.Visibility = ViewStates.Invisible;
        _parentLayout.AddView(_symbolGrid);
      }

      RecentFileBrowser browser = new RecentFileBrowser(this);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        browser.LayoutParameters = lprams;
        browser.Visibility = ViewStates.Invisible;
      }
      _parentLayout.AddView(browser);
      _browser = browser;

      LinearLayout imageList = new LinearLayout(this)
      { Orientation = Orientation.Vertical };
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
      RecentFileBrowser browser = _browser;
      browser.Filter = new[] { ".config" };
      browser.SetDirectory(path);
      browser.Show(MapVm.GetRecents(), (file) =>
      {
        MapVm.Load(file);
        MapVm.SetRecent(file);
        _mapView.Invalidate();
        Button btnImages = FindViewById<Button>(Resource.Id.btnImages);
        btnImages.Visibility = (MapVm.ImageCount > 1) ?
          ViewStates.Visible : ViewStates.Gone;
        OnResume();
      });
    }

    public void ShowSymbols(System.Action<MapButton> setModeFct)
    {
      _setModeFct = setModeFct;
      _symbolGrid.ShowAll();
      _symbolGrid.Visibility = ViewStates.Visible;
      _symbolGrid.PostInvalidate();
    }

    public void ShowColors(System.Action<ColorRef> setColorFct)
    {
      _symbolGrid.ShowColors();
      _symbolGrid.OnceColorFct = setColorFct;
      _symbolGrid.Visibility = ViewStates.Visible;
      _symbolGrid.PostInvalidate();
    }
    private void SetMode(MapButton modeButton)
    {
      _setModeFct?.Invoke(modeButton);
    }

    public bool DrawOnlyMode
    { get { return chkDrawOnly?.Checked ?? false; } }

    public bool DetailUpperRight
    {
      get { return Settings.DetailUR; }
    }

    private LocationVm _locationVm;
    public LocationVm LocationVm
    { get { return _locationVm ?? (_locationVm = new LocationVm(this) { View = MapView }); } }
    private CompassVm _compassVm;
    public CompassVm CompassVm
    { get { return _compassVm ?? (_compassVm = new CompassVm(this) { View = MapView }); } }

    protected override void OnResume()
    {
      base.OnResume();

      //Button imageButton = FindViewById<Button>(Resource.Id.btnImages);
      //int nImages = 0;
      //foreach (XmlImage img in MapVm.Images)
      //{ nImages++; }
      //imageButton.Visibility = (nImages < 2) ? ViewStates.Gone : ViewStates.Visible;
      //imageButton.PostInvalidate();

      if (Settings.UseLocation)
      { LocationVm.StartLocation(false); }
      if (Settings.UseCompass)
      { CompassVm.StartCompass(); }
    }
    protected override void OnPause()
    {
      base.OnPause();
      MapVm.Save(backup: true);
      LocationVm.PauseLocation();
      CompassVm.StopCompass();
    }
    LocationManager ILocationContext.GetLocationManager()
    { return (LocationManager)GetSystemService(LocationService); }
    Android.Hardware.SensorManager ICompassContext.GetSensorManager()
    { return (Android.Hardware.SensorManager)GetSystemService(SensorService); }

  }
}

