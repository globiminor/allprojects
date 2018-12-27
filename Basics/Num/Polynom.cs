
namespace Basics.Num
{
  public class Polynom
  {
    readonly double[] _a;
    public Polynom(double[] a)
    { _a = a; }

    public double Value(double x)
    {
      double d = 0;
      foreach (var a in _a)
      { d = d * x + a; }
      return d;
    }
  }
}
