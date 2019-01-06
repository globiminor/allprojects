using System.Globalization;
using System.Threading;

namespace System.IO
{
  public class InvariantCulture : IDisposable
  {
    private readonly CultureInfo _culture;
    private readonly Thread _thread;

    public InvariantCulture()
    {
      _thread = Thread.CurrentThread;
      _culture = _thread.CurrentCulture;

      _thread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public void Dispose()
    {
      _thread.CurrentCulture = _culture;
    }
  }
}
