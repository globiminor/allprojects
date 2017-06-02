using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      FrameworkElement.LanguageProperty.OverrideMetadata(
          typeof(FrameworkElement),
          new FrameworkPropertyMetadata(
              XmlLanguage.GetLanguage(
              CultureInfo.CurrentCulture.IetfLanguageTag)));

      Dispatcher.UnhandledException += (s,ex) =>
      {
        MessageBox.Show(Basics.Utils.GetMsg(ex.Exception), "Error");
        ex.Handled = true;
      };

      base.OnStartup(e);
    }
  }
}
