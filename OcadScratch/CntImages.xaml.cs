using Microsoft.Win32;
using OcadScratch.ViewModels;
using OMapScratch;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OcadScratch
{
  /// <summary>
  /// Interaktionslogik für CntImages.xaml
  /// </summary>
  public partial class CntImages : UserControl
  {
    public CntImages()
    {
      InitializeComponent();

      StyleImages(grdImages);

      grdImages.SetBinding(DataGrid.ItemsSourceProperty, new Binding(nameof(DataContext.Images)) { Mode = BindingMode.OneWay });
    }

    public new ConfigVm DataContext
    {
      get { return (ConfigVm)base.DataContext; }
      set { base.DataContext = value; }
    }

    private void StyleImages(DataGrid grd)
    {
      grd.AutoGenerateColumns = false;
      ImageVm img;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Image Name", Binding = new Binding(nameof(img.Name)) };
        Basics.Window.FilterPanel.CreateHeader(col);

        grd.Columns.Add(col);
      }
      {
        DataGridTextColumn col = new DataGridTextColumn
        {
          Header = "Image Path",
        };
        Basics.Window.Utils.SetErrorBinding(col, nameof(img.Path));
        Basics.Window.FilterPanel.CreateHeader(col);

        grd.Columns.Add(col);
      }
    }

    private void BtnAddClick(object sender, RoutedEventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter = "*.jpg | *.jpg";
      dlg.FileOk += (s, ca) =>
      {
        string fileName = dlg.FileName;
        string jgw = Path.ChangeExtension(fileName, ".jgw");
        if (!File.Exists(jgw))
        {
          MessageBox.Show($"World-file '{jgw}' is missing!.", "Missing world file", MessageBoxButton.OK, MessageBoxImage.Error);
          ca.Cancel = true;
        }
      };
      if (dlg.ShowDialog() != true)
      { return; }

      string jpg = dlg.FileName;

      XmlImage add = new XmlImage { Name = Path.GetFileNameWithoutExtension(jpg), Path = Path.GetFileName(jpg) };
      ImageVm vm = new ImageVm(add) { CopyFromPath = jpg };

      DataContext.AddImage(vm);
    }
  }
}
