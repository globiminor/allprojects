
using System;
using Basics.Views;
using OMapScratch;

namespace OcadScratch.ViewModels
{
  public class ImageVm : NotifyListener
  {
    private readonly XmlImage _baseImg;

    public ImageVm(XmlImage baseImg)
    {
      _baseImg = baseImg;
      Validate();
    }

    protected override void Disposing(bool disposing)
    { }

    public string Name
    {
      get { return _baseImg.Name; }
      set
      {
        _baseImg.Name = value;
        Changed();
      }
    }

    public string Path
    {
      get { return _baseImg.Path; }
      set
      {
        _baseImg.Path = value;
        Validate();
        Changed();
      }
    }

    public void Validate()
    {
      {
        string value = Path;
        string error = null;
        if (System.IO.Path.GetFileName(value) != value)
        { error = $"{Path} must be local"; }
        SetError(nameof(Path), error);
      }
    }
  }
}
