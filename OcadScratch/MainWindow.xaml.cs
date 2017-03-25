using OMapScratch;
using System.Collections.Generic;
using System.IO;
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
  }
}
