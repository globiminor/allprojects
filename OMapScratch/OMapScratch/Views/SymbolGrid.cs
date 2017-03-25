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

        List<ColorRef> colors = _activity.Map.GetColors();
        List<Symbol> symbols = _activity.Map.GetSymbols();

        ColumnCount = colors.Count;
        RowCount = 2 + (symbols.Count - 1) / colors.Count;
        List<SymbolButton> symBtns = new List<SymbolButton>();
        foreach (ColorRef color in colors)
        {
          Button btnColor = new Button(Context);
          btnColor.SetMinimumWidth(5);
          btnColor.SetWidth(fullWidth / colors.Count);
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
          SymbolButton btnSym = new SymbolButton(sym, Context);
          btnSym.SetMinimumWidth(5);
          btnSym.SetWidth(fullWidth / colors.Count);

          btnSym.Click += (s, e) =>
          {
            SymbolButton current = _activity._btnCurrentSymbol;
            current.Symbol = btnSym.Symbol;
            current.Color = btnSym.Color;
            current.PostInvalidate();
            Visibility = ViewStates.Invisible;
          };

          AddView(btnSym);
          symBtns.Add(btnSym);
        }
      }
    }
  }
}