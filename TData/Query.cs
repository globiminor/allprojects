using System;
using System.Collections.Generic;
using System.Data;
using Basics.Geom;

namespace TData
{
  /// <summary>
  /// Geometric Query
  /// </summary>
  public class GeometryQuery
  {
    private DataColumn _column;
    private IGeometry _geom;
    private BoxRelation _rel;
    public GeometryQuery(DataColumn geomColum, IGeometry geometry, BoxRelation relation)
    {
      _column = geomColum;
      _geom = geometry;
      _rel = relation;
    }

    public DataColumn Column
    { get { return _column; } }
    public IGeometry Geometry
    { get { return _geom; } }
    public BoxRelation Relation
    { get { return _rel; } }

    public static IList<string> Parse(string expression)
    {
      return Parse(expression, new char[] { '\'' }, new char[] { },
        new char[] { ',', ':', '(', ')', '=', '<', '>', '+', '-', '*', '/' });
    }

    public static IList<string> Parse(string expression, IList<char> stringDelimiters,
      IList<char> escapeChars, IList<char> specialChars)
    {
      int iStart = 0;
      int nPos = expression.Length;
      int iPos = 0;
      List<string> terms = new List<string>();
      while (iPos < nPos)
      {
        char c = expression[iPos];
        if (stringDelimiters.Contains(c))
        {
          if (iStart != iPos)
          {
            string error = GetError(expression, iPos);
            throw new InvalidExpressionException(error);
          }
          iPos++;
          while (iPos < nPos && c != expression[iPos])
          {
            iPos++;
          }
          if (iPos >= nPos)
          {
            string error = GetError(expression, iStart);
            throw new InvalidExpressionException(error);
          }
          terms.Add(expression.Substring(iStart, iPos + 1 - iStart));
          iStart = iPos + 1;

          if (iStart >= nPos ||
            char.IsWhiteSpace(expression[iStart]) ||
            specialChars.Contains(expression[iStart]))
          { }
          else
          {
            string error = GetError(expression, iPos);
            throw new InvalidExpressionException(error);
          }
        }
        else if (char.IsWhiteSpace(c))
        {
          if (iPos == iStart)
          {
            iStart++;
          }
          else
          {
            terms.Add(expression.Substring(iStart, iPos - iStart));
          }
        }
        else if (specialChars.Contains(c))
        {
          if (iPos > iStart)
          {
            terms.Add(expression.Substring(iStart, iPos - iStart));
          }
          terms.Add(expression.Substring(iPos, 1));
          iStart = iPos + 1;
        }

        iPos++;
      }

      if (iPos > iStart)
      {
        terms.Add(expression.Substring(iStart, iPos - iStart));
      }

      return terms;
    }

    private static string GetError(string expression, int iStart)
    {
      System.Text.StringBuilder e = new System.Text.StringBuilder();
      for (int i = 0; i < Math.Min(3, iStart); i++)
      {
        e.Append(".");
      }
      bool complete = false;
      int n = 10;
      if (expression.Length - iStart < 13)
      {
        n = expression.Length - iStart;
        complete = true;
      }
      for (int i = iStart; i < iStart + n; i++)
      {
        e.Append(expression[i]);
      }
      if (complete == false)
      {
        e.Append("...");
      }
      string error = string.Format("{0}" + Environment.NewLine +
        "Invalid expression near position {1}" + Environment.NewLine +
        "{2}", expression, iStart, e);
      return error;
    }

  }
}
