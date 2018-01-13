using OcadScratch.ViewModels;
using System;
using Macro;

namespace OcadScratch.Commands
{
  class CmdMoveTo : IDisposable
  {
    private MapVm _mapVm;
    private WorkElemVm _elemVm;

    public CmdMoveTo(MapVm mapVm, WorkElemVm elemVm)
    {
      _mapVm = mapVm;
      _elemVm = elemVm;
    }

    public string Error { get; private set; }

    public void Dispose()
    { }

    public bool Execute()
    {
      Error = null;

      if (_mapVm == null)
      {
        Error = "Map not set";
        return false;
      }
      if (_elemVm == null)
      {
        Error = "Elem not set";
        return false;
      }

      Basics.Geom.IBox ext = _elemVm.GetElem()?.Geometry?.GetGeometry().Project(_mapVm.GlobalPrj).Extent;
      if (ext == null)
      {
        Error = "Invalid element geometry";
        return false;
      }
      string x = $"{((ext.Min.X + ext.Max.X) / 2.0):f1}";
      string y = $"{((ext.Min.Y + ext.Max.Y) / 2.0):f1}";


      Processor macro = new Processor();
      if (macro.SetForegroundWindow("OCAD") == System.IntPtr.Zero)
      {
        Error = "Keine OCAD-Instanz gefunden";
        return false;
      }

      macro.SendCommands('n', Ui.VK_ALT);
      macro.SendKey('c');

      System.Threading.Thread.Sleep(500);
      macro.SendKeys($"{x:f0}");

      System.Threading.Thread.Sleep(100);
      macro.SendKey('\t');

      System.Threading.Thread.Sleep(100);
      macro.SendKeys($"{y:f0}");

      System.Threading.Thread.Sleep(100);
      macro.SendKey('\t');

      System.Threading.Thread.Sleep(100);
      macro.SendKeys(Environment.NewLine);

      return true;
    }
  }
}
