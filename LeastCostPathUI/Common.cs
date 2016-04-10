
using System.Reflection;

namespace LeastCostPathUI
{
	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class Common
	{
    public static int InitColors(byte[] r,byte[] g,byte[] b)
    {
      int i,k;
      for (i = 0; i <= 42; i++) 
      {
        k = i;
        r[i] = 255;
        g[i] = (byte) (6 * k);
        b[i] = 0;
      }
      for (i = 43; i <= 85; i++) 
      {
        k = (byte) (i - 43);
        r[i] = (byte) (255 - 6 * k);
        g[i] = 255;
        b[i] = 0;
      }

      for (i = 86; i <= 128; i++) 
      {
        k = (byte) (i - 86);
        g[i] = 255;
        b[i] = (byte) (6 * k);
        r[i] = 0;
      }
      for (i = 129; i <= 170; i++) 
      {
        k = (byte) (i - 129);
        g[i] = (byte) (255 - 6 * k);
        b[i] = 255;
        r[i] = 0;
      }
  
      for (i = 171; i <= 212; i++) 
      {
        k = (byte) (i - 171);
        b[i] = 255;
        r[i] = (byte) (6 * k);
        g[i] = 0;
      }
      for (i = 213; i <= 255; i++) 
      {
        k = (byte) (i - 213);
        b[i] = (byte) (255 - 6 * k);
        r[i] = 255;
        g[i] = 0;
      }
      return 0;
    }

	  public static string GetMethodName(MethodInfo methodInfo)
	  {
      return string.Format("{0}.{1}", methodInfo.ReflectedType.Name, methodInfo.Name);
	  }
	}
}
