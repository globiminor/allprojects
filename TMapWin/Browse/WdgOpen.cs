
using System.IO;
using System.Windows.Forms;
using TMapWin.Browse;

namespace TMapWin.Browse
{
  public partial class WdgOpen : WdgBrowse
  {
    private string _openName;

    public WdgOpen()
    {
      InitializeComponent();
    }

    public string FileName
    {
      get { return _openName; }
    }

    protected override void OnOpen(string text)
    {
      _openName = Path.Combine(CurrentDirectory, text);
      DialogResult = DialogResult.OK;
      Close();
    }

  }
}