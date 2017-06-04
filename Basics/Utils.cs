
using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace Basics
{
  public class NotNullAttribute : Attribute
  { }
  public class CanBeNullAttribute : Attribute
  { }
  public class UsedImplicitlyAttribute : Attribute
  { }
  public static class Assert
  {
    public static T NotNull<T>(T value)
    {
      return value;
    }

    public static void ArgumentCondition(bool condition, string msg, params object[] args)
    {
    }

  }

  public static class Utils
  {
    public static double Round(double value, double interval)
    {
      double rounded = Math.Round(value / interval) * interval;
      return rounded;
    }

    /// <summary>
    /// Greates common divisor
    /// </summary>
    public static int Gcd(int x, int y)
    {
      if (x == 0)
      { return Math.Abs(y); }
      if (y == 0)
      { return Math.Abs(x); }

      int x0 = Math.Abs(x);
      int y0 = Math.Abs(y);

      int t;
      do
      {
        t = x0 % y0;
        x0 = y0;
        y0 = t;
      } while (t != 0);
      return x0;
    }

    public static string GetMsg(Exception exp, string stackIgnore = null)
    {
      StringBuilder sb = new StringBuilder();
      while (exp != null)
      {
        if (sb.Length > 0)
        { sb.AppendLine("[InnerException]"); }
        sb.AppendLine(exp.Message);
        if (stackIgnore == null)
        {
          sb.AppendLine(exp.StackTrace);
        }
        else
        {
          using (System.IO.TextReader r = new System.IO.StringReader(exp.StackTrace))
          {
            string line;
            while ((line = r.ReadLine()) != null)
            {
              if (line.IndexOf(stackIgnore, StringComparison.InvariantCultureIgnoreCase) >= 0)
              { continue; }

              sb.AppendLine(line);
            }
          }
        }
        sb.AppendLine();

        exp = exp.InnerException;
      }
      return sb.ToString();
    }

    public static string FindExecutable(string ext)
    {
      StringBuilder b = new StringBuilder(1024);
      FindExecutable(ext, null, b);
      return b.ToString();
    }

    [DllImport("shell32.dll")]
    private static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);
  }

  public static class Extensions
  {
    public static bool TryParse<T>(string text, out T value)
    {
      if (string.IsNullOrEmpty(text))
      {
        value = default(T);
      }
      else
      {
        try
        {
          object oe = Enum.Parse(typeof(T), text);
          value = (T)oe;
        }
        catch
        {
          value = default(T);
          return false;
        }
      }
      return true;
    }
  }
}
