
namespace Basics.Num
{
  public class Polynom
  {
    double[] mA;
    public Polynom(double[] a)
    { mA = a; }

    public double Value(double x)
    {
      double d = 0;
      foreach (double a in mA)
      { d = d * x + a; }
      return d;
    }
  }
}
