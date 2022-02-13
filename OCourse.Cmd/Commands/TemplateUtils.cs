using System;
using System.Collections.Generic;
using System.IO;

namespace OCourse.Cmd.Commands
{
  public class TemplateUtils
  {
    private readonly List<string> _tmplParts;
    public TemplateUtils(string path)
    {
      Template = Path.GetFileName(path);
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

    public string GetReplaced(string path, string resultTemplate)
    {
      string mapFileName = Path.GetFileName(path);
      string t = resultTemplate;
      int idx = 0;
      for (int iPart = 0; iPart < _tmplParts.Count; iPart++)
      {
        int eIdx = mapFileName.IndexOf(_tmplParts[iPart], idx, StringComparison.InvariantCultureIgnoreCase);
        string rep = mapFileName.Substring(idx, eIdx - idx);
        t = t.Replace($"[{iPart - 1}]", rep);
        idx = eIdx + _tmplParts[iPart].Length;
      }
      t = t.Replace($"[{_tmplParts.Count}]", mapFileName.Substring(idx));

      return t;
    }
  }
}
