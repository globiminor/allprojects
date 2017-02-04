using Ocad.StringParams;

namespace Ocad
{
  internal class StringIndexEventArgs
  {
    StringParamIndex _index;
    bool _cancel;

    public StringIndexEventArgs(StringParamIndex index)
    {
      _index = index;
      _cancel = false;
    }
    public StringParamIndex Index
    {
      get
      { return _index; }
    }
    public bool Cancel
    {
      get
      { return _cancel; }
      set
      { _cancel = value; }
    }
  }
  internal delegate void StringIndexEventHandler(object sender, StringIndexEventArgs args);
}
