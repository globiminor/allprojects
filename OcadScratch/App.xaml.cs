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
      base.OnStartup(e);
    }
  }
}
