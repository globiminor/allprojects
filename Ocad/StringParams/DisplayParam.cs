
namespace Ocad.StringParams
{
  public class DisplayPar : SingleParam
  {
    public const char GraphicObjectModeKey = 'j';
    public const char ImageObjectModeKey = 'k';

    public DisplayPar() { }
    public DisplayPar(string para) : base(para) { }

    public int? GraphicObjectMode
    {
      get { return GetInt_(GraphicObjectModeKey); }
      set { SetParam(GraphicObjectModeKey, value); }
    }
    public int? ImageObjectMode
    {
      get { return GetInt_(ImageObjectModeKey); }
      set { SetParam(ImageObjectModeKey, value); }
    }
  }
}
