using iTextSharp.text.pdf;
using Ocad;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OTextSharp.Models
{
  public class OcadPdfWriter
  {
    public class StartGroup
    {
      public string Key { get; set; }
      public List<string> Cats { get; set; }
    }

    private readonly string _ocadFile;

    public OcadPdfWriter(string ocadFile)
    {
      _ocadFile = ocadFile;
    }

    public TimeSpan WriteStartTimes(string fileName, string categories, TimeSpan t0, 
      int nTimes, int nx = 5, int ny = 16, float marginX = 28.3f, float marginY = 34.0f)
    {
      using (StartWriter w = new StartWriter(fileName, categories, nx, ny, marginX, marginY))
      {
        TimeSpan t = t0;
        for (int i = 0; i < nTimes; i++)
        {
          w.Add(t);
          t += new TimeSpan(0, 1, 0);
        }
        return t;
      }
    }

    public void PrintCourseRunners()
    {
      using (OcadReader reader = OcadReader.Open(_ocadFile))
      {
        IList<StringParamIndex> idxs = reader.ReadStringParamIndices();
        IList<CategoryPar> cats = reader.GetCategories(idxs);

        Dictionary<string, List<CategoryPar>> courseDict = new Dictionary<string, List<CategoryPar>>();
        foreach (var cat in cats)
        {
          string course = cat.CourseName;
          if (!courseDict.TryGetValue(course, out List<CategoryPar> courseCats))
          {
            courseCats = new List<CategoryPar>();
            courseDict.Add(course, courseCats);
          }
          courseCats.Add(cat);
        }
        List<string> css = new List<string>(courseDict.Keys);
        css.Sort();
        foreach (var course in css)
        {
          int runners = 0;
          foreach (var cat in courseDict[course])
          {
            runners += cat.NrRunners.Value;
          }
          Console.WriteLine("{0,-14}              {1,2}                  {2,3}", course.Replace("/", "_"), runners, runners / 5 + 4);
        }
      }
    }

    public void WriteStartGroups(List<StartGroup> startGroups, TimeSpan tMin, TimeSpan tMax,
      int nTimes, int nx = 5, int ny = 16, float marginX = 28.3f, float marginY = 34.0f)
    {
      Dictionary<string, CategoryPar> catsDict = ValidateGroups(startGroups);
      Console.WriteLine("-----");
      foreach (var startGroup in startGroups)
      {
        StringBuilder sb = new StringBuilder();
        StringBuilder fb = new StringBuilder();

        List<string> fullNames = new List<string>();
        int runners = 0;
        foreach (var cat in startGroup.Cats)
        {
          runners += catsDict[cat].NrRunners.Value;
          if (sb.Length > 0)
          {
            sb.Append("/");
          }
          if (fb.Length > 0)
          {
            fb.Append(", ");
            if (fb.Length > 24)
            {
              fullNames.Add(fb.ToString());
              fb = new StringBuilder();
            }
          }
          fb.Append(cat);
          string cc = cat;
          string[] parts = cc.Split();
          if (parts.Length > 1)
          {
            cc = string.Empty;
            foreach (var part in parts)
            {
              cc += part[0];
            }
          }
          if (cc.Length > 3)
          {
            cc = cc.Substring(0, 3);
          }
          sb.Append(cc);
        }
        fullNames.Add(fb.ToString());

        string cats = sb.ToString();
        Console.WriteLine("{0} {1}", cats, runners);
        string filePart = cats.Replace("/", "_");
        {
          string name = string.Format("StartTimes_{0}_{1}.pdf", startGroup.Key, filePart);
          string fullName = Path.Combine(Path.GetDirectoryName(_ocadFile), name);
          using (PdfWrite w = new PdfWrite(fullName))
          {
            w.SetFontSize(32);
            w.ShowText(PdfContentByte.ALIGN_CENTER, "Startgruppe", PdfWrite.DefaultWidth / 2, 0.75f * PdfWrite.DefaultHeight, 0);

            w.Font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
            w.SetFontSize(160);
            w.ShowText(PdfContentByte.ALIGN_CENTER, startGroup.Key, PdfWrite.DefaultWidth / 2, PdfWrite.DefaultHeight / 2, 0);
            w.Font = null;

            w.SetFontSize(32);
            int i = 0;
            foreach (var f in fullNames)
            {
              w.ShowText(PdfContentByte.ALIGN_CENTER, f, PdfWrite.DefaultWidth / 2, PdfWrite.DefaultHeight / 3 - i * 32, 0);
              i++;
            }

            w.SetFontSize(24);
            w.ShowText(PdfContentByte.ALIGN_CENTER, cats, PdfWrite.DefaultWidth / 2, PdfWrite.DefaultHeight / 3 - i * 32 - 72, 0);

            w.SetFontSize(24);
            w.ShowText(PdfContentByte.ALIGN_CENTER, "(Etikette ablösen und auf Postenbeschreibung kleben)", PdfWrite.DefaultWidth / 2, 30, 0);
          }
        }
        TimeSpan t = tMin;
        while (t < tMax)
        {
          string name = string.Format("StartTimes_{0}_{1}_{2}_{3}.pdf", startGroup.Key, filePart, t.Hours, t.Minutes);
          string fullName = Path.Combine(Path.GetDirectoryName(_ocadFile), name);
          t = WriteStartTimes(fullName, cats, t, nTimes, 
            nx, ny, marginX, marginY);
        }
      }
      Console.WriteLine("-----");
    }

    private Dictionary<string, CategoryPar> ValidateGroups(List<StartGroup> startGroups)
    {
      using (OcadReader reader = OcadReader.Open(_ocadFile))
      {
        IList<StringParamIndex> idxs = reader.ReadStringParamIndices();
        IList<CategoryPar> cats = reader.GetCategories(idxs);

        Dictionary<string, CategoryPar> catDict = cats.ToDictionary(x => x.Name);
        Dictionary<string, List<CategoryPar>> courseDict = new Dictionary<string, List<CategoryPar>>();
        foreach (var cat in cats)
        {
          string course = cat.CourseName;
          if (!courseDict.TryGetValue(course, out List<CategoryPar> courseCats))
          {
            courseCats = new List<CategoryPar>();
            courseDict.Add(course, courseCats);
          }
          courseCats.Add(cat);
        }

        foreach (var startGroup in startGroups)
        {
          foreach (var catName in startGroup.Cats)
          {
            if (!catDict.ContainsKey(catName))
            {
              throw new InvalidOperationException("Unknown Category " + catName);
            }

            List<CategoryPar> courseCats = courseDict[catDict[catName].CourseName];
            foreach (var catPar in courseCats)
            {
              if (!startGroup.Cats.Contains(catPar.Name))
              {
                string msg = string.Format("{0} not in same group as {1}", catPar.Name, catName);
                throw new InvalidOperationException(msg);
              }
            }
            catDict.Remove(catName);
          }
        }

        if (catDict.Count > 0)
        {
          StringBuilder sb = new StringBuilder("Missing Category");
          foreach (var catName in catDict.Keys)
          {
            sb.AppendFormat("{0}, ", catName);
          }
          throw new InvalidOperationException("Missing categories " + sb);
        }
        return cats.ToDictionary(x => x.Name);
      }
    }

    public void WriteCategories(string title, List<StartGroup> catGroups, List<StartGroup> startGroups)
    {
      using (OcadReader reader = OcadReader.Open(_ocadFile))
      {
        IList<StringParamIndex> idxs = reader.ReadStringParamIndices();
        IList<CategoryPar> cats = reader.GetCategories(idxs);

        Dictionary<string, CategoryPar> catDict = cats.ToDictionary(x => x.Name);
        Dictionary<string, Course> courseDict = reader.ReadCourses().ToDictionary(x => x.Name);

        Dictionary<string, string> groupDict = new Dictionary<string, string>();
        foreach (var startGroup in startGroups)
        {
          foreach (var cat in startGroup.Cats)
          {
            groupDict.Add(cat, startGroup.Key);
          }
        }

        string fullName = Path.ChangeExtension(_ocadFile, ".Categories.pdf");
        using (PdfWrite w = new PdfWrite(fullName))
        {
          float y0 = 710;
          float mx = 30;
          float wi = (PdfWrite.DefaultWidth / 2) - mx;
          float x = mx - wi;
          float fs = 20;
          w.SetFontSize(36);
          w.ShowText(PdfContentByte.ALIGN_LEFT, title, mx, y0 + 80, 0);
          w.ShowText(PdfContentByte.ALIGN_LEFT, "Startzeitgruppen", mx, y0 + 40, 0);
          w.SetFontSize(fs);
          foreach (var catGroup in catGroups)
          {
            float y = y0;
            x += wi;
            w.ShowText(PdfContentByte.ALIGN_LEFT, "Kategorie", x, y0 + 5, 0);
            w.ShowText(PdfContentByte.ALIGN_CENTER, "Gruppe", x + 160, y0 + 5, 0);
            foreach (var cat in catGroup.Cats)
            {
              y -= fs * 1.4f;
              if (string.IsNullOrWhiteSpace(cat))
              {
                continue;
              }
              w.ShowText(PdfContentByte.ALIGN_LEFT, cat, x, y, 0);
              string group = groupDict[cat];
              if (group.StartsWith("_"))
              { continue; }
              w.ShowText(PdfContentByte.ALIGN_CENTER, group, x + 160, y, 0);
            }
          }
        }
      }
    }

    private class StartWriter : PdfWrite
    {
      private readonly string _categories;
      private int _ix;
      private int _iy;

      private readonly int _nX;
      private readonly float _marginX;
      private readonly float _w;

      private readonly int _nY;
      private readonly float _marginY = 10;
      private readonly float _h;

      public StartWriter(string export, string categories,
        int nx = 5, int ny = 13, float marginX = 10, float marginY = 10)
        : base(export)
      {
        _categories = categories;

        _nX = nx;
        _nY = ny;

        _marginX = marginX;
        _marginY = marginY;

        _w = (DefaultWidth - 2 * _marginX) / _nX;
        _h = (DefaultHeight - 2 * _marginY) / _nY;

        _ix = 0;
        _iy = 0;
      }

      public void Add(TimeSpan time)
      {
        if (_ix >= _nX)
        {
          _ix = 0;
          _iy++;
        }

        SetFontSize(9);
        ShowText(PdfContentByte.ALIGN_CENTER, _categories, (_ix + 0.5f) * _w + _marginX, (_nY - 0.3f - _iy) * _h + _marginY, 0);

        SetFontSize(24);
        ShowText(PdfContentByte.ALIGN_CENTER, string.Format("{0:D2}:{1:D2}", time.Hours, time.Minutes), 
          (_ix + 0.5f) * _w + _marginX, (_nY - 0.8f - _iy) * _h + _marginY, 0);

        _ix++;
      }
    }
  }
}