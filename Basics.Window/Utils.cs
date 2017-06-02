﻿

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Basics.Window
{
  public interface IDataContext<T>
  {
    T DataContext { get; }
  }

  public static class Utils
  {
    public static void SetBinding<T>(DataGridComboBoxColumn col, IDataContext<T> parent, Func<T, string> getProperty)
    {
      Type ancestorType = parent.GetType();
      string prop = getProperty(parent.DataContext);

      col.ElementStyle = new Style { TargetType = typeof(ComboBox) };
      col.ElementStyle.Setters.Add(new Setter(ComboBox.ItemsSourceProperty,
        new Binding
        {
          Path = new PropertyPath($"{nameof(parent.DataContext)}.{prop}"),
          RelativeSource = new RelativeSource { AncestorType = ancestorType }
        }));

      col.EditingElementStyle = new Style { TargetType = typeof(ComboBox) };
      col.EditingElementStyle.Setters.Add(new Setter(ComboBox.ItemsSourceProperty,
        new Binding
        {
          Path = new PropertyPath($"{nameof(parent.DataContext)}.{prop}"),
          RelativeSource = new RelativeSource { AncestorType = ancestorType }
        }));
    }

    public static void SetErrorBinding(DataGridTextColumn col, string property)
    {
      col.Binding = new Binding(property) { ValidatesOnDataErrors = true };

      ToolTip tooltip = new ToolTip();
      tooltip.IsVisibleChanged += (s, e) =>
      {
        string ttp = null;
        IDataErrorInfo errInfo = tooltip.DataContext as IDataErrorInfo;
        if (errInfo != null)
        {
          ttp = errInfo[property];
        }
        if (string.IsNullOrWhiteSpace(ttp))
        { ttp = "No Error"; }
        tooltip.Content = ttp;
      };
      col.CellStyle = new Style() { TargetType = typeof(DataGridCell) };
      col.CellStyle.Setters.Add(new Setter(DataGridCell.ToolTipProperty, tooltip));
    }

    public static T GetParent<T>(DependencyObject obj) where T : Visual
    {
      DependencyObject parent = obj;
      while (parent != null && !(parent is T))
      {
        parent = VisualTreeHelper.GetParent(parent);
      }
      return (T)parent;
    }

    public static IEnumerable<T> GetChildren<T>(DependencyObject parent) where T : Visual
    {
      if (parent == null)
      { yield break; }

      if (parent is T)
      { yield return (T)parent; }

      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < count; i++)
      {
        foreach (T child in GetChildren<T>(VisualTreeHelper.GetChild(parent, i)))
        {
          yield return child;
        }
      }
    }
  }
}
