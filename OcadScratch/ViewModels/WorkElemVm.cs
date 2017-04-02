using OMapScratch;

namespace OcadScratch.ViewModels
{
  public interface IWorkElemView
  { }
  public class WorkElemVm
  {
    private readonly Elem _elem;
    public WorkElemVm()
    { }

    public WorkElemVm(Elem elem)
    {
      _elem = elem;
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

  }
}
