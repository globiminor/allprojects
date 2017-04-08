using Basics.Views;
using OMapScratch;

namespace OcadScratch.ViewModels
{
  public interface IWorkElemView
  { }
  public class WorkElemVm : NotifyListener
  {
    private readonly Elem _elem;
    private bool _handled;

    public WorkElemVm()
    { }
    protected override void Disposing(bool disposing)
    { }

    public WorkElemVm(Elem elem)
    {
      _elem = elem;
    }

    public Elem Elem
    {
      get { return _elem; }
      set { }
    }
    public string SymbolId
    {
      get { return _elem?.Symbol.Id; }
      set { }
    }

    public string ColorId
    {
      get { return _elem?.Color?.Id; }
      set { }
    }

    public bool Handled
    {
      get { return _handled; }
      set
      {
        _handled = value;
        Changed("Handled");
      }
    }

    public Elem GetElem()
    {
      return _elem;
    }
  }
}
