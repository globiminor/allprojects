using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Basics.Views
{
  public static class ListViewUtils
  {
    public static Type GetElementType(object list)
    {
      if (list == null)
      { return null; }

      Type elemType = null;
      Type e = typeof(IEnumerable<>);
      foreach (Type type in list.GetType().GetInterfaces())
      {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == e)
        {
          elemType = type.GetGenericArguments()[0];
          break;
        }
      }
      return elemType;
    }

    public static Type GetPropertyType(object list, string property)
    {
      Type elemType = GetElementType(list);
      if (elemType == null)
      { return null; }

      PropertyInfo prop = elemType.GetProperty(property);
      if (prop == null)
      { return null; }

      Type propertyType = prop.PropertyType;
      if (propertyType.IsGenericType && typeof(Nullable<>) == propertyType.GetGenericTypeDefinition())
      {
        propertyType = propertyType.GetGenericArguments()[0];
      }
      return propertyType;
    }

    public static ListSortDescriptionCollection AdaptSort(IBindingListView view,
      string property, bool add)
    {
      if (view == null)
      { return null; }

      Type listType = null;
      Type e = typeof(IEnumerable<>);
      foreach (Type type in view.GetType().GetInterfaces())
      {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == e)
        {
          listType = type.GetGenericArguments()[0];
          break;
        }
      }
      if (listType == null)
      { return null; }

      ListSortDescriptionCollection oldSorts = view.SortDescriptions;
      List<ListSortDescription> newSorts = new List<ListSortDescription>();
      bool propSortExists = false;
      if (oldSorts != null)
      {
        foreach (ListSortDescription sort in oldSorts)
        {
          ListSortDescription newSort;
          if (sort.PropertyDescriptor.Name == property)
          {
            propSortExists = true;
            if (sort.SortDirection == ListSortDirection.Descending)
            { continue; }

            newSort = new ListSortDescription(sort.PropertyDescriptor, ListSortDirection.Descending);
          }
          else
          {
            if (!add)
            { continue; }

            newSort = new ListSortDescription(sort.PropertyDescriptor, sort.SortDirection);
          }
          newSorts.Add(newSort);
        }
      }
      if (!propSortExists)
      {
        PropertyDescriptorCollection properties =
          TypeDescriptor.GetProperties(listType);
        PropertyDescriptor prop = properties.Find(property, true);

        newSorts.Add(new ListSortDescription(prop, ListSortDirection.Ascending));
      }

      ListSortDescriptionCollection sortDescs = new ListSortDescriptionCollection(newSorts.ToArray());

      view.ApplySort(sortDescs);
      return view.SortDescriptions;
    }

    public static string GetTextFilterStatement(string text, string property, object list, bool caseSensitive)
    {
      text = text ?? string.Empty;
      if (text.StartsWith("\\"))
      { return text.Substring(1); }

      Type propertyType = GetPropertyType(list, property);

      StringBuilder statement = new StringBuilder();
      StringBuilder expression = null;
      bool inText = false;
      foreach (char c in text)
      {
        if (c == '\'')
        {
          inText = !inText;
        }

        if (inText)
        {
        }
        else if (char.IsWhiteSpace(c))
        {
          continue;
        }
        else if (c == '(' || c == ')')
        {
          AppendExpression(statement, property, propertyType, expression, caseSensitive);
          expression = null;
          statement.Append(c);
          continue;
        }
        else if (c == '&' || c == '|')
        {
          AppendExpression(statement, property, propertyType, expression, caseSensitive);
          expression = null;
          if (c == '&')
          {
            statement.Append(" AND ");
          }
          if (c == '|')
          {
            statement.Append(" OR ");
          }
          continue;
        }

        if (expression == null)
        {
          expression = new StringBuilder();
        }
        expression.Append(c);
      }

      AppendExpression(statement, property, propertyType, expression, caseSensitive);

      return statement.ToString();
    }

    private static void AppendExpression(StringBuilder statement, string propertyName, Type propertyType, StringBuilder expression, bool caseSensitive)
    {
      if (expression == null)
      { return; }

      string filterText;
      string fullOp;
      GetFilterText(expression.ToString(), out filterText, out fullOp);

      string filter;
      if (propertyType == typeof(string))
      {
        bool useUpper = false;
        if (!caseSensitive && !filterText.StartsWith("'"))
        {
          filterText = filterText.ToUpper();
          useUpper = true;
        }
        if (string.IsNullOrEmpty(fullOp))
        {
          fullOp = "LIKE";
          if (!filterText.StartsWith("'"))
          { filterText = string.Format("'*{0}*'", filterText); }
        }
        else if (!filterText.StartsWith("'"))
        { filterText = string.Format("'{0}'", filterText); }

        if (useUpper)
        { filter = string.Format("UPPER({0}) {1} {2}", propertyName, fullOp, filterText); }
        else
        { filter = string.Format("{0} {1} {2}", propertyName, fullOp, filterText); }
      }
      else if (propertyType == typeof(DateTime))
      {
        DateTime dt;
        if (DateTime.TryParse(filterText, out dt))
        {
          if (string.IsNullOrEmpty(fullOp))
          {
            dt = dt.Date;
            filter = string.Format("{0} >= '{1}' AND {0} < '{2}'", propertyName, dt, dt.AddDays(1));
          }
          else
          {
            filter = string.Format("{0} {1} '{2}'", propertyName, fullOp, dt);
          }
        }
        else
        {
          filter = null;
        }
      }
      else
      {
        if (string.IsNullOrEmpty(fullOp))
        { fullOp = "="; }
        filter = string.Format("{0} {1} {2}", propertyName, fullOp, filterText);
      }
      statement.Append(filter);
    }

    private static void GetFilterText(string text, out string filter, out string filterOp)
    {
      string filterText = text;
      filterOp = null;
      if (string.IsNullOrEmpty(filterText))
      {
        filter = null;
        return;
      }
      filterText = filterText.Trim();
      if (string.IsNullOrEmpty(filterText))
      {
        filter = null;
        return;
      }

      filterOp = "";

      char op = filterText[0];
      if (op == '\\')
      {
        filter = filterText.Substring(1);
        filterOp = "\\";
        return;
      }

      while (op == '=' || op == '>' || op == '<')
      {
        filterOp += op;
        filterText = filterText.Substring(1);
        if (filterText.Length > 0)
        { op = filterText[0]; }
        else
        { op = ' '; }
      }

      filter = filterText;
    }

  }
}
