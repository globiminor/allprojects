using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using OMapScratch.Views;
using System.Collections.Generic;

namespace OMapScratch
{

  partial class MainActivity
  {
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

        foreach (ColorRef clr in colors)
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
        foreach (Symbol sym in symbols)
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

        foreach (SymbolButton btn in _symBtns)
        {
          btn.Color = btnColor.Color;
          btn.PostInvalidate();
        }
      }
    }
  }
}