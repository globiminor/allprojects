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
      private MainActivity _activity;
      public SymbolGrid(MainActivity context)
        : base(context)
      {
        _activity = context;
      }

      public void Init(int fullWidth)
      {
        RemoveAllViews();

        List<ColorRef> colors = _activity.MapVm.GetColors();
        List<Symbol> symbols = _activity.MapVm.GetSymbols();

        int nColumns = colors.Count + 1;

        ColumnCount = nColumns;
        RowCount = 2 + (symbols.Count - 1) / nColumns;
        List<SymbolButton> symBtns = new List<SymbolButton>();
        EditButton btnEdit = new EditButton(_activity);
        btnEdit.SetMinimumWidth(5);
        btnEdit.SetWidth(fullWidth / nColumns);
        btnEdit.Click += (s, e) =>
        {
          _activity.SetMode(btnEdit);
          Visibility = ViewStates.Invisible;
        };

        AddView(btnEdit);

        foreach (ColorRef color in colors)
        {
          Button btnColor = new Button(Context);
          btnColor.SetMinimumWidth(5);
          btnColor.SetWidth(fullWidth / nColumns);
          btnColor.SetBackgroundColor(color.Color);
          AddView(btnColor);

          btnColor.Click += (s, a) =>
          {
            foreach (SymbolButton btn in symBtns)
            {
              btn.Color = color;
              btn.PostInvalidate();
            }
          };
        }
        foreach (Symbol sym in symbols)
        {
          SymbolButton btnSym = new SymbolButton(sym, _activity);
          btnSym.SetMinimumWidth(5);
          btnSym.SetWidth(fullWidth / nColumns);

          btnSym.Click += (s, e) =>
          {
            _activity.SetMode(btnSym);
            Visibility = ViewStates.Invisible;
          };

          AddView(btnSym);
          symBtns.Add(btnSym);
        }
      }
    }
  }
}