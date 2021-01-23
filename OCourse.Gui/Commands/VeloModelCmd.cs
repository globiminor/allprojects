using Basics.Data;
using Basics.Forms;
using Basics.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using TMap;

namespace OCourse.Gui.Plugins
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
      Programm.Init();
    }

    public void Execute(IContext context)
    {
      if (_wdg == null || _wdg.IsDisposed)
      {
        _wdg = new WdgVeloModel();
        _wdg.Vm = new ViewModels.VeloModelVm();
        Programm.Init();
      }
      _wdg.TMapContext = context;
      _wdg.Vm.TMapContext = context;
      if (context is System.Windows.Forms.IWin32Window parent)
      { _wdg.Show(parent); }
      else
      { _wdg.Show(); }
      _wdg.InitMapContext();
    }
  }
}

namespace OCourse.Gui
{
  partial class WdgVeloModel
  {
    private System.Windows.Forms.Button _addToMap;
    internal IContext TMapContext { get; set; }
    internal void InitMapContext()
    {
      if (_addToMap == null)
      {
        System.Windows.Forms.Button addToMap = new System.Windows.Forms.Button();
        addToMap.Text = "Add To Map";
        addToMap.Size = new System.Drawing.Size { Width = 100, Height = btnMap.Height };
        addToMap.Location = new System.Drawing.Point { X = grdSymbols.Right - addToMap.Size.Width, Y = lblDefaultVelo.Location.Y };
        addToMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        addToMap.Click += (s, e) =>
        {
          if (TMapContext == null || Vm == null)
          { return; }

          TableMapData mapData = Vm.GetMapData();
          TMapContext.Data.Subparts.Add(mapData);
          TMapContext.Refresh();
          TMapContext.Draw();
        };
        Controls.Add(addToMap);

        addToMap.Bind(x => x.Enabled, _bindingSource, nameof(Vm.CanAddToMap), false, System.Windows.Forms.DataSourceUpdateMode.Never);
        _addToMap = addToMap;
      }
    }
  }
}