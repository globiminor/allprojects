﻿using Basics;
using Basics.Views;
using OcadScratch.Commands;
using OcadScratch.ViewModels;
using OMapScratch;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private class SymbolPanel : Panel
    {
      public SymbolPanel()
      {
        DataContextChanged += (s, a) => InvalidateVisual();
      }

      protected override void OnRender(DrawingContext dc)
      {
        base.OnRender(dc);
        WorkElemVm wElem = DataContext as WorkElemVm;
        Elem elem = wElem?.Elem;
        if (elem != null)
        {
          Color color = elem.Color?.Color ?? Colors.Red;
          color.A = 255;
          Pen p = new Pen(new SolidColorBrush(color), 1);

          SymbolUtils.DrawSymbol(dc, elem.Symbol, color, (float)ActualWidth, (float)ActualHeight, 1);
        }
      }
    }
    public MainWindow()
    {
      InitializeComponent();

      WorkElemVm workElemVm = null;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Symbol ID", Binding = new Binding(nameof(workElemVm.SymbolId)) };
        col.IsReadOnly = true;
        grdElems.Columns.Add(col);
      }
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Color ID", Binding = new Binding(nameof(workElemVm.ColorId)) };
        col.IsReadOnly = true;
        grdElems.Columns.Add(col);
      }
      {
        DataGridTemplateColumn col = new DataGridTemplateColumn { Header = "Symbol" };
        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(SymbolPanel));
        factory.SetBinding(Panel.TagProperty, new Binding(nameof(workElemVm.Elem)));
        col.CellTemplate = new DataTemplate { VisualTree = factory };
        col.IsReadOnly = true;
        grdElems.Columns.Add(col);
      }
      {
        DataGridCheckBoxColumn col = new DataGridCheckBoxColumn { Header = "Handled", Binding = new Binding(nameof(workElemVm.Handled)) };
        grdElems.Columns.Add(col);
      }

    }

    public new ViewModels.MapVm DataContext
    {
      get { return (ViewModels.MapVm)base.DataContext; }
      set { base.DataContext = value; }
    }

    private void btnSymbols_Click(object sender, RoutedEventArgs e)
    {
      Map map = new Map();
      StringBuilder sb = new StringBuilder();
      using (var w = new StringWriter(sb))
      {
        XmlSymbols syms = XmlSymbols.Create(map.GetSymbols());
        syms.Colors = new List<XmlColor>();
        foreach (ColorRef color in map.GetColors())
        { syms.Colors.Add(XmlColor.Create(color)); }
        Serializer.Serialize(syms, w);
      }

      string first = sb.ToString();
      XmlSymbols des;
      using (var r = new StringReader(first))
      {
        Serializer.Deserialize(out des, r);
      }

      List<Symbol> symbols = new List<Symbol>();
      foreach (XmlSymbol s in des.Symbols)
      { symbols.Add(s.GetSymbol()); }
      sb = new StringBuilder();
      using (var w = new StringWriter(sb))
      {
        Serializer.Serialize(XmlSymbols.Create(symbols), w);
      }
      if (sb.ToString() != first)
      { throw new InvalidDataException(""); }
    }

    private void mniLoad_Click(object sender, RoutedEventArgs e)
    {
      string scratchFile;
      {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Title = "Load Scratch file";
        dlg.Filter = "*.config | *.config";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        scratchFile = dlg.FileName;
      }
      ViewModels.MapVm vm = new ViewModels.MapVm();
      vm.Init(scratchFile);
      DataContext = vm;
    }

    private void btnTransfer_Click(object sender, RoutedEventArgs e)
    {
      string scratchFile;
      {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Title = "Transfer to OCAD";
        dlg.Filter = "*.ocd | *.ocd";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        scratchFile = dlg.FileName;
      }

      List<WorkElemVm> elems = new List<WorkElemVm>();
      foreach (object item in grdElems.Items)
      {
        WorkElemVm elem = item as WorkElemVm;
        if (elem != null)
        { elems.Add(elem); }
      }

      using (CmdTransfer cmd = new CmdTransfer(DataContext?.Map, elems, scratchFile))
      { cmd.Execute(); }
    }


    private void btnMoveToOcad_Click(object sender, RoutedEventArgs e)
    {
      MoveTo();
    }

    private void MoveTo()
    {
      try
      {
        Topmost = false;

        using (CmdMoveTo cmd = new CmdMoveTo(DataContext, cntWorkElem?.DataContext))
        {
          if (!cmd.Execute())
          { MessageBox.Show(this, cmd.Error); }
        }
      }
      finally
      {
        Topmost = true;
      }
    }

    private void grdElems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      WorkElemVm currentElem = grdElems.CurrentItem as WorkElemVm;
      if (currentElem != null && currentElem != cntWorkElem.DataContext)
      {
        cntWorkElem.DataContext = currentElem;
      }
    }

    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
      using (CmdNextElem cmd = new CmdNextElem(DataContext, cntWorkElem.DataContext))
      {
        cmd.SetHandled = true;
        if (!cmd.Execute())
        {
          MessageBox.Show(this, cmd.Error);
          return;
        }
        cntWorkElem.DataContext = cmd.NextElem;
      }
      MoveTo();
    }

    private void mniPrepare_Click(object sender, RoutedEventArgs e)
    {
      FrmConfig frm = new FrmConfig();
      frm.ShowDialog();
    }
  }
}
