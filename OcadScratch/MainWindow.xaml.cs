﻿using Macro;
using OcadScratch.Commands;
using OcadScratch.ViewModels;
using OMapScratch;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Basics.Window.Browse;
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Linq;

namespace OcadScratch
{

  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, Basics.Window.IDataContext<ViewModels.MapVm>
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

          SymbolUtils.DrawSymbol(dc, elem.Symbol, color, (float)ActualWidth, (float)ActualHeight, 1);
        }
      }
    }

    public MainWindow()
    {
      InitializeComponent();
      mniSave.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(DataContext.CanSave)));

      WorkElemVm workElemVm;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Symbol ID", Binding = new Binding(nameof(workElemVm.SymbolId)) };

        //Basics.Window.FilterPanel.CreateHeader(col);

        col.IsReadOnly = true;
        grdElems.Columns.Add(col);
      }
      //{
      //  DataGridComboBoxColumn col = new DataGridComboBoxColumn { Header = "Symbol Test" };
      //  Basics.Window.Utils.SetBinding(col, this, dc => nameof(dc.SymbolIds));

      //  col.SelectedItemBinding = new Binding(nameof(workElemVm.SymbolId));

      //  Basics.Window.FilterPanel.CreateHeader(col);

      //  col.IsReadOnly = true;
      //  grdElems.Columns.Add(col);
      //}
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Color ID", Binding = new Binding(nameof(workElemVm.ColorId)) };
        col.IsReadOnly = true;
        grdElems.Columns.Add(col);
      }
      {
        DataGridTemplateColumn col = new DataGridTemplateColumn { Header = "Symbol" };
        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(SymbolPanel));
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
        Basics.Serializer.Serialize(syms, w);
      }

      string first = sb.ToString();
      XmlSymbols des;
      using (var r = new StringReader(first))
      {
        Basics.Serializer.Deserialize(out des, r);
      }

      List<Symbol> symbols = new List<Symbol>();
      foreach (XmlSymbol s in des.Symbols)
      { symbols.Add(s.GetSymbol()); }
      sb = new StringBuilder();
      using (var w = new StringWriter(sb))
      {
        Basics.Serializer.Serialize(XmlSymbols.Create(symbols), w);
      }
      if (sb.ToString() != first)
      { throw new InvalidDataException(""); }
    }

    private void mniLoadTest_Click(object sender, RoutedEventArgs e)
    {
      string fullPath;
      {
        FrmBrowser frm = new FrmBrowser();
        BrowserVm browserVm = BrowserVm.Create(Directory.GetCurrentDirectory(), ".config");
        frm.DataContext = browserVm;
        if (frm.ShowDialog() == true)
        { }
        else
        { return; }
        fullPath = browserVm.Selected.FullPath;
      }
      LoadConfig(fullPath);
    }

    private void mniLoad_Click(object sender, RoutedEventArgs e)
    {
      Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
      dlg.Title = "Load Scratch file";
      dlg.Filter = "*.config | *.config";

      string fullPath = null;
      dlg.FileOk += (s, arg) =>
      {
        string deviceName = PortableDeviceUtils.GetDevicePathName(dlg.FileName);
        if (deviceName != null)
        {
          string path = Wm.GetFileDialogFolder(dlg.Title);
          string dir = path.Substring(path.IndexOf(Path.DirectorySeparatorChar) + 1);
          string name = deviceName.Substring(deviceName.IndexOf(":") + 1);

          fullPath = $"{dir}{Path.DirectorySeparatorChar}{name}";
        }
      };


      if (!(dlg.ShowDialog(this) ?? false))
      { return; }

      LoadConfig(fullPath ?? dlg.FileName);
    }

    private void LoadConfig(string configFile)
    {
      ViewModels.MapVm vm = new ViewModels.MapVm();
      vm.Init(configFile);
      DataContext = vm;

      XmlConfig config;
      PortableDeviceUtils.Deserialize(configFile, out config);
      ConfigVm configVm = new ConfigVm(configFile, config);

      SetConfigVm(configVm);
    }

    private void mniInit_Click(object sender, RoutedEventArgs e)
    {
      string ocdFile;
      {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Title = "OCAD file";
        dlg.Filter = "*.ocd | *.ocd";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        ocdFile = dlg.FileName;
      }

      DataContext = new ViewModels.MapVm();
      ConfigVm configVm = new ConfigVm();
      configVm.Init(ocdFile);
      SetConfigVm(configVm);
    }

    private void mniSave_Click(object sender, RoutedEventArgs e)
    {
      if (cntGeoref?.DataContext == null)
      { return; }

      string configFile;
      {
        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
        dlg.Title = "O-Scratch Config";
        dlg.Filter = "*.config | *.config";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        configFile = dlg.FileName;
      }
      cntGeoref.DataContext.Save(configFile);
    }

    private void SetConfigVm(ConfigVm configVm)
    {
      cntGeoref.DataContext = configVm;
      cntSettings.DataContext = configVm;
      cntImages.DataContext = configVm;
      try
      {
        configVm.LoadSymbols();
      }
      finally
      { cntSymbols.DataContext = configVm; }
      DataContext.ConfigVm = configVm;
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
  }
}
