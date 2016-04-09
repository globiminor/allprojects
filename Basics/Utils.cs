
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

    public static string GetMsg(Exception exp)
    {
      string msg = "";
      while (exp != null)
      {
        msg = exp.Message + Environment.NewLine +
              exp.StackTrace + Environment.NewLine + msg;
        exp = exp.InnerException;
      }
      return msg;
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
    /// <summary>
    /// Returns the name of the property for the given Linq statement.
    /// </summary>
    /// <typeparam name="T">The type of the object the property is gotten from.</typeparam>
    /// <typeparam name="S">The type of Linq expression.</typeparam>
    /// <param name="obj">The object from which the property is retrieved.</param>
    /// <param name="expr">The expression of the property.</param>
    /// <returns>The name of the property.</returns>
    public static string GetPropertyName<T, S>(this T obj, Expression<Func<T, S>> expr)
    {
      return ((MemberExpression)expr.Body).Member.Name;
    }

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
