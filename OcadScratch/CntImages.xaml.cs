using Microsoft.Win32;
using OcadScratch.ViewModels;
using OMapScratch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
      dlg.Filter = "*.jpg|*.jpg|*.png|*.png";
      dlg.Multiselect = true;
      string ext;
      dlg.FileOk += (s, ca) =>
      {
        HashSet<string> msgs = new HashSet<string>();
        foreach (var fileName in dlg.FileNames)
        {
          string iext = Path.GetExtension(fileName).ToLower();
          if (iext == ".jpg")
          {
            ext = ".jgw";
          }
          else if (iext == ".png")
          {
            ext = ".pgw";
          }
          else
          {
            msgs.Add($"Unhandled file format {iext}");
            continue;
          }
          string worldFile = Path.ChangeExtension(fileName, ext);
          if (!File.Exists(worldFile))
          {
            msgs.Add($"World-file '{worldFile}' is missing!");
          }
        }
        if (msgs.Count > 0)
				{
          MessageBox.Show(string.Concat(msgs.Select(x=> x + Environment.NewLine)), "Unhandled", MessageBoxButton.OK, MessageBoxImage.Error);
          ca.Cancel = true;
        }
      };
      if (dlg.ShowDialog() != true)
      { return; }

      foreach (var img in dlg.FileNames)
      {
        XmlImage add = new XmlImage { Name = Path.GetFileNameWithoutExtension(img), Path = Path.GetFileName(img) };
        ImageVm vm = new ImageVm(add) { CopyFromPath = img };

        DataContext.AddImage(vm);
      }
    }
  }
}
