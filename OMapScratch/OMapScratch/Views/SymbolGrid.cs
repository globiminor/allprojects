using Android.Graphics;
using Android.Views;
using Android.Widget;
using Basics.Views;
using OMapScratch.Views;
using System.Collections.Generic;

namespace OMapScratch
{

  partial class MainActivity
  {
    private class RotateModeGrid : GridLayout
    {
      private readonly MainActivity _activity;

      private ImageButton _caller;

      public RotateModeGrid(MainActivity context)
        : base(context)
      {
        _activity = context;
        ColumnCount = 1;
      }

      public void Init()
      {
        RemoveAllViews();

        {
          ImageButton btn = new ImageButton(_activity);
          btn.SetBackgroundResource(Resource.Drawable.DetailUr);
          btn.Click += (s, e) =>
          {
            Settings.DetailUR = true;
            if (Settings.DrawOption == DrawOptions.DetailLL)
            {
              Settings.DrawOption = DrawOptions.DetailUR;
              _caller?.SetBackgroundResource(ImageLoader.GetResource(Settings.DrawOption));
              _caller?.PostInvalidate();
            }
            Visibility = ViewStates.Invisible;
          };
          Add(btn);
        }
        {
          ImageButton btn = new ImageButton(_activity);
          btn.SetBackgroundResource(Resource.Drawable.DetailLl);
          btn.Click += (s, e) =>
          {
            Settings.DetailUR = false;
            if (Settings.DrawOption == DrawOptions.DetailUR)
            {
              Settings.DrawOption = DrawOptions.DetailLL;
              _caller?.SetBackgroundResource(ImageLoader.GetResource(Settings.DrawOption));
              _caller?.PostInvalidate();
            }
            Visibility = ViewStates.Invisible;
          };
          Add(btn);
        }
        {
          ImageButton btn = new ImageButton(_activity);
          btn.SetBackgroundResource(Resource.Drawable.RotL90);
          btn.Click += (s, e) =>
          {
            Apply(DrawOptions.RotL90, setAsDefault: true);
            Visibility = ViewStates.Invisible;
          };
          Add(btn);
        }
        {
          ImageButton btn = new ImageButton(_activity);
          btn.SetBackgroundResource(Resource.Drawable.RotNorth);
          btn.Click += (s, e) =>
          {
            Apply(DrawOptions.RotNorth, setAsDefault: true);
            Visibility = ViewStates.Invisible;
          };
          Add(btn);
        }
        {
          ImageButton btn = new ImageButton(_activity);
          btn.SetBackgroundResource(Resource.Drawable.RotCompass);
          btn.Click += (s, e) =>
          {
            Apply(DrawOptions.RotCompass, setAsDefault: true);
            Visibility = ViewStates.Invisible;
          };
          Add(btn);
        }
      }

      public void Apply(DrawOptions option, bool setAsDefault = false)
      {
        if (setAsDefault)
        {
          Settings.DrawOption = option;
          _caller?.SetBackgroundResource(ImageLoader.GetResource(Settings.DrawOption));
          _caller?.PostInvalidate();
        }

        if (option == DrawOptions.RotL90)
        {
          _activity.MapView.Rotate(-System.Math.PI / 2, _activity.MapView.Width / 2, _activity.MapView.Height / 2);
        }
        else if (option == DrawOptions.RotNorth)
        {
          float[] vals = _activity.MapView.ElemMatrixValues;
          MatrixProps props = new MatrixProps(vals);
          float scale = props.Scale;
          using (Matrix invers = new Matrix())
          {
            float[] pts = new[] { _activity.MapView.Width / 2f, _activity.MapView.Height / 2f };
            _activity.MapView.ElemMatrix.Invert(invers);
            invers.MapPoints(pts);

            Matrix north = new Matrix();
            north.SetValues(new[] { scale, 0, vals[2], 0, scale, vals[5], 0, 0, 1 });
            north.MapPoints(pts);

            _activity.MapView.SetElemMatrix(north, postInvalidate: false);
            _activity.MapView.Translate(_activity.MapView.Width / 2f - pts[0], _activity.MapView.Height / 2f - pts[1]);
          }
        }
        else if (option == DrawOptions.RotCompass && _activity.MapVm.GetDeclination() != null)
        {
          _activity.CompassVm.GetCompass((orient) =>
          {
            float? decl = _activity.MapVm.GetDeclination();
            if (decl == null)
            { return; }
            MatrixProps mat = new MatrixProps(_activity.MapView.ElemMatrixValues);
            float rotate = (float)(mat.Rotate - (orient + decl) / 180 * System.Math.PI);
            _activity.MapView.Rotate(rotate, _activity.MapView.Width / 2, _activity.MapView.Height / 2);
          });
        }
      }

      private void Add(ImageButton btn)
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.TopMargin = (int)(0.2 * Utils.GetMmPixel(btn));
        btn.LayoutParameters = lprams;
        AddView(btn);
      }

      public void ShowAll(ImageButton caller)
      {
        SetX(caller.GetX());
        _caller = caller;

        for (int i = 0; i < ChildCount; i++)
        { GetChildAt(i).Visibility = ViewStates.Visible; }
      }
    }
    private class SymbolGrid : GridLayout
    {
      private List<SymbolButton> _symBtns;
      private readonly MainActivity _activity;
      public System.Action<ColorRef> OnceColorFct { get; set; }

      public SymbolGrid(MainActivity context)
        : base(context)
      {
        _activity = context;
      }

      public void ShowAll()
      {
        OnceColorFct = null;
        for (int i = 0; i < ChildCount; i++)
        { GetChildAt(i).Visibility = ViewStates.Visible; }
      }

      public void ShowColors()
      {
        OnceColorFct = null;

        for (int i = 0; i < ChildCount; i++)
        {
          View btn = GetChildAt(i);
          if (!(btn is ColorButton))
          { btn.Visibility = ViewStates.Gone; }
        }
      }

      public void Init(int fullWidth)
      {
        RemoveAllViews();

        List<ColorRef> colors = _activity.MapVm.GetColors();
        List<Symbol> symbols = _activity.MapVm.GetSymbols();

        int nColumns = colors.Count + 1;

        ColumnCount = nColumns;
        RowCount = 2 + (symbols.Count - 1) / nColumns;
        List<SymbolButton> symbolBtns = new List<SymbolButton>();
        EditButton btnEdit = new EditButton(_activity);
        btnEdit.SetMinimumWidth(5);
        btnEdit.SetWidth(fullWidth / nColumns);
        btnEdit.Click += (s, e) => Utils.Try(() =>
        {
          _activity.SetMode(btnEdit);
          Visibility = ViewStates.Invisible;
        });

        AddView(btnEdit);

        foreach (var clr in colors)
        {
          ColorButton btnColor = new ColorButton(_activity, clr);
          btnColor.SetMinimumWidth(5);
          btnColor.SetWidth(fullWidth / nColumns);
          AddView(btnColor);

          btnColor.Click += (s, a) => Utils.Try(() =>
          {
            ColorClicked(btnColor);
          });
        }
        SymbolButton firstSym = null;
        foreach (var sym in symbols)
        {
          SymbolButton btnSym = new SymbolButton(sym, _activity);
          btnSym.SetMinimumWidth(5);
          btnSym.SetWidth(fullWidth / nColumns);

          firstSym = firstSym ?? btnSym;

          btnSym.Click += (s, e) => Utils.Try(() =>
          {
            _activity.SetMode(btnSym);
            Visibility = ViewStates.Invisible;
          });

          AddView(btnSym);
          symbolBtns.Add(btnSym);
        }

        if (firstSym != null)
        {
          _activity._btnCurrentMode.CurrentMode = firstSym;
          _activity._btnCurrentMode.PostInvalidate();
        }

        _symBtns = symbolBtns;
      }


      private void ColorClicked(ColorButton btnColor)
      {
        System.Action<ColorRef> fct = OnceColorFct;
        OnceColorFct = null;
        if (fct != null)
        {
          fct(btnColor.Color);
          Visibility = ViewStates.Invisible;
          PostInvalidate();
          return;
        }

        if (_symBtns == null)
        { return; }

        foreach (var btn in _symBtns)
        {
          btn.Color = btnColor.Color;
          btn.PostInvalidate();
        }
      }
    }
  }
}