using OCourse.Gui;
using TMap;

namespace OCourse.Plugins
{
  public class VeloModelCmd : ICommand
  {
    private WdgVeloModel _wdg;

    public VeloModelCmd()
    {
      // "AssemblyResolve" is active only here
      // --> load all potentially needed libraries here
      _wdg = new WdgVeloModel();
      _wdg.Vm = new ViewModels.VeloModelVm();
      Common.Init();
    }

    public void Execute(IContext context)
    {
      if (_wdg == null || _wdg.IsDisposed)
      {
        _wdg = new WdgVeloModel();
        _wdg.Vm = new ViewModels.VeloModelVm();
        Common.Init();
      }
      _wdg.TMapContext = context;
      if (context is System.Windows.Forms.IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
    }
  }
}

namespace OCourse.Gui
{
  partial class WdgVeloModel
  {
    internal IContext TMapContext { get; set; }
  }
}