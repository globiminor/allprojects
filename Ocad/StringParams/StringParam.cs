using System.Collections.Generic;
using System.Text;

namespace Ocad.StringParams
{
  public class NamedParam: MultiParam
  {
    public NamedParam(string para)
      :base(para)
    { }
  }
  public abstract class MultiParam : StringParam
  {
    protected MultiParam(string para)
      : base(para)
    { }

    protected MultiParam()
    { }

    public string Name
    {
      get { return ParaList[0]; }
      set { ParaList[0] = value; }
    }

    protected override int Index(char t)
    {
      int n = ParaList.Count;
      for (int i = 1; i < n; i++) // 0 Index = Name!
      {
        string para = ParaList[i];
        if (!(para?.Length > 0))
        { continue; }
        if (para[0] == t)
        { return i; }
      }
      return -1;
    }
  }

  public abstract class SingleParam : StringParam
  {
    protected SingleParam()
    { }

    protected SingleParam(string para)
      : base(para)
    { }

    protected override int Index(char t)
    {
      int n = ParaList.Count;
      for (int i = 0; i < n; i++)
      {
        string p = ParaList[i];

        if (p.Length > 0 && p[0] == t)
        { return i; }
      }
      return -1;
    }
  }

  public abstract class StringParam
  {
    private readonly IList<string> _paraList;

    protected StringParam(string para)
    {
      para = para ?? string.Empty;
      _paraList = new List<string>(para.Split('\t'));
    }

    protected StringParam()
    {
      _paraList = new List<string>();
    }

    protected IList<string> ParaList
    {
      get { return _paraList; }
    }

    public string StringPar
    {
      get
      {
        StringBuilder s = new StringBuilder(_paraList[0]);
        int n = _paraList.Count;
        for (int i = 1; i < n; i++)
        {
          s.AppendFormat("\t{0}", _paraList[i]);
        }
        if (AppendEndTabs)
        {
          s.Append("\t\t");
        }
        return s.ToString();
      }
    }
    protected virtual bool AppendEndTabs
    {
      get { return true; }
    }

    protected void Add(char key, object value)
    {
      string par = string.Format("{0}{1}", key, value);
      _paraList.Add(par);
    }

    protected abstract int Index(char t);

    public string GetString(char key)
    {
      int index = Index(key);

      if (index < 0 || index > ParaList.Count)
      { return null; }

      string para = ParaList[index];

      if (para.Length < 2)
      { return null; }
      return para.Substring(1);
    }

    protected double? GetDouble_(char key)
    {
      string s = GetString(key);
      if (string.IsNullOrEmpty(s))
      { return null; }
      s = s.Split('\0')[0];
      if (double.TryParse(s, out double d) == false)
      { return null; }
      return d;
    }

    protected double GetDouble(char key)
    { return GetDouble_(key).Value; }

    protected int GetInt(char key)
    { return GetInt_(key).Value; }

    protected int? GetInt_(char key)
    {
      double? d = GetDouble_(key);
      if (d == null)
      { return null; }
      return (int)d;
    }

    protected void SetParam(char key, object value)
    {
      int idx = Index(key);
      if (idx < 0)
      {
        if (value == null)
        { return; }

        _paraList.Add(key.ToString());
        idx = Index(key);
      }
      if (value == null)
      {
        _paraList.RemoveAt(idx);
        return;
      }
      _paraList[idx] = string.Format("{0}{1}", _paraList[idx][0], value);
    }
  }
}
