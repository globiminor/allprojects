using OcadScratch.ViewModels;
using OMapScratch;
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
      XmlImage img;
      {
        DataGridTextColumn col = new DataGridTextColumn { Header = "Image Name", Binding = new Binding(nameof(img.Name)) };
        Basics.Window.FilterPanel.CreateHeader(col);

        grd.Columns.Add(col);
      }
      {
        DataGridTextColumn col = new DataGridTextColumn
        {
          Header = "Image Name",
        };
        Basics.Window.Utils.SetErrorBinding(col, nameof(img.Path));
        Basics.Window.FilterPanel.CreateHeader(col);

        grd.Columns.Add(col);
      }
    }

  }
}
