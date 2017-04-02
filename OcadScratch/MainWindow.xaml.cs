using Basics.Views;
using OcadScratch.ViewModels;
using OMapScratch;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
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

    private void btnTransfer_Click(object sender, RoutedEventArgs e)
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

      string ocdFile;
      {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Title = "Load OCAD file";
        dlg.Filter = "*.ocd | *.ocd";
        if (!(dlg.ShowDialog() ?? false))
        { return; }

        ocdFile = dlg.FileName;
      }

      using (CmdTransfer cmd = new CmdTransfer(scratchFile, ocdFile))
      {
        cmd.Execute();
        BindingListView<WorkElemVm> lstElems = new BindingListView<WorkElemVm>();
        lstElems.AllowNew = false;
        lstElems.AllowRemove = false;
        foreach (Elem elem in cmd.Map.Elems)
        { lstElems.Add(new WorkElemVm(elem)); }

        grdElems.ItemsSource = lstElems;
        cntWorkElem.DataContext = lstElems.FirstOrDefault();
      }
    }

    private void btnMoveToOcad_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Topmost = false;
        Macro.Macro macro = new Macro.Macro();
        macro.SetForegroundWindow("OCAD");

        macro.SendCommands('n', Macro.Macro.VK_ALT);
        macro.SendKey('c');

        System.Threading.Thread.Sleep(500);
        macro.SendKeys("677500");

        System.Threading.Thread.Sleep(100);
        macro.SendKey('\t');

        System.Threading.Thread.Sleep(100);
        macro.SendKeys("254200");

        System.Threading.Thread.Sleep(100);
        macro.SendKey('\t');

        System.Threading.Thread.Sleep(100);
        macro.SendKeys(System.Environment.NewLine);
      }
      finally
      {
        Topmost = true;
      }
    }

    private void grdElems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      WorkElemVm currentElem = grdElems.CurrentItem as WorkElemVm;
      if (currentElem != null && currentElem != cntWorkElem.DataContext)
      {
        cntWorkElem.DataContext = currentElem;
      }
    }
  }
}
