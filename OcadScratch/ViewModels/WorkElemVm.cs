using Basics.Views;
using OMapScratch;
using System.Linq;
using System.Text;

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

    public string Text
    {
      get { return _elem?.Text; }
      set { }
    }

    public string PicturesText
    {
      get
      {
        if (_elem.Pictures == null)
        { return null; }
        StringBuilder sb = new StringBuilder();
        foreach (var p in _elem.Pictures)
        { sb.Append($"{p};"); }
        return sb.ToString();
      }
      set { }
    }

    public bool Handled
    {
      get { return _handled; }
      set
      {
        _handled = value;
        Changed();
      }
    }

    public Elem GetElem()
    {
      return _elem;
    }
  }
}
