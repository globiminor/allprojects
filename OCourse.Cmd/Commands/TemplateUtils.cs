using System;
using System.Collections.Generic;
using System.IO;

namespace OCourse.Cmd.Commands
{
  public class TemplateUtils
  {
    private readonly List<string> _tmplParts;
    public TemplateUtils(string path, bool fullPath = false)
    {
      Template = !fullPath ? Path.GetFileName(path) : path;
      _tmplParts = new List<string>(Template.Split(new[] { '*' }, StringSplitOptions.None));

    }
    public string Template { get; }
    public IReadOnlyList<string> Parts => _tmplParts;

    public string GetPart(string path, int index)
    {
      string fileName = Path.GetFileName(path);
      int idx = 0;
      for (int iPart = 0; iPart <= index; iPart++)
      {
        idx = fileName.IndexOf(_tmplParts[iPart], idx);
        idx += _tmplParts[iPart].Length;
      }
      int idxEnd = fileName.IndexOf(_tmplParts[index + 1], idx);
      string part = fileName.Substring(idx, idxEnd - idx);
      return part;
    }

    /// <summary>
    /// replace file name part of source and replace by resultTemplate adapted corresponding to Template
    /// </summary>
    /// <param name="source"></param>
    /// <param name="resultTemplate"></param>
    /// <param name="fullSource">if true: take all of source to replace</param>
    /// <returns></returns>
    public string GetReplaced(string source, string resultTemplate, bool fullSource = false)
    {
      string s = !fullSource ? Path.GetFileName(source) : source;
      string t = resultTemplate;
      int idx = 0;
      for (int iPart = 0; iPart < _tmplParts.Count; iPart++)
      {
        int eIdx = s.IndexOf(_tmplParts[iPart], idx, StringComparison.InvariantCultureIgnoreCase);
        if (eIdx < 0) // could not be fully parsed
        { return s; }
        string rep = s.Substring(idx, eIdx - idx);
        t = t.Replace($"[{iPart - 1}]", rep);
        idx = eIdx + _tmplParts[iPart].Length;
      }
      t = t.Replace($"[{_tmplParts.Count}]", s.Substring(idx));

      return t;
    }
  }
}
