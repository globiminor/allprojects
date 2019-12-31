using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using OTextSharp.Commands;
using OTextSharp.Models;
using System;
using System.Collections.Generic;

namespace OcadTest.OEvent
{
  [TestClass]
  public class Events
  {
    private static readonly string _courseDuebi2020 = @"C:\daten\felix\kapreolo\karten\dübendorf\Bahnlegung.ocd";
    private static readonly string _imagesDuebi2020 = @"C:\daten\felix\kapreolo\karten\dübendorf\Fotos\P*.jpg";

    [TestMethod]
    public void Duebi2020_CreateImagePoBes()
    {
      CmdCreateImagePoBe cmd = new CmdCreateImagePoBe(_courseDuebi2020, new List<ControlInfo>
      {
        new ControlInfo{Key = "100", Info="Sockel"},
        new ControlInfo{Key = "102", Info="Sockel"},
        new ControlInfo{Key = "111", Info="Sockel"},
        new ControlInfo{Key = "117", Info="!Folie!"},
        new ControlInfo{Key = "123", Info="K-Binder"},
        new ControlInfo{Key = "132", Info="K-Binder"},
        new ControlInfo{Key = "136", Info="K-Binder"},

        new ControlInfo{Key = "M2", Info="Tor:Schnur"},
        new ControlInfo{Key = "M3", Info="Tor:K-Binder"},
        new ControlInfo{Key = "M4", Info="Tor:K-Binder"},
        new ControlInfo{Key = "M5", Info="Tor:K-Binder"},
        new ControlInfo{Key = "M6", Info="Absperren"},
        new ControlInfo{Key = "M7", Info="Tor:K-Binder"},
        new ControlInfo{Key = "M8", Info="Tor:K-Binder"},
        new ControlInfo{Key = "M9", Info="Verbot:Abdecken"},
        new ControlInfo{Key = "M10", Info="Verbot:Abdecken"},

      },
      maxCols: 3);

      //cmd.WriteImages("Setzen 1", _imagesOpfikon2018);
      //cmd.WriteImages("Setzen 2", _imagesOpfikon2018);
      //cmd.WriteImages("Setzen 3", _imagesOpfikon2018);
      //cmd.WriteImages("Setzen 4", _imagesOpfikon2018);
      cmd.WriteImages("setzen1", _imagesDuebi2020);
      cmd.WriteImages("setzen2", _imagesDuebi2020);
      cmd.WriteImages("setzen3", _imagesDuebi2020);
      cmd.WriteImages("setzen4", _imagesDuebi2020);

      List<OcadPdfWriter.StartGroup> startGroups = new List<OcadPdfWriter.StartGroup>
      {
        new OcadPdfWriter.StartGroup { Key="A", Cats  = new List<string> {"D18", "HAL", "H35", "H40", "Offen Lang"}},
        new OcadPdfWriter.StartGroup { Key="B", Cats  = new List<string> {"DAL", "H45", "HAM", "H18" }},
        new OcadPdfWriter.StartGroup { Key="C", Cats  = new List<string> {"D35", "D40", "H50", "H55", "H60"}},
        new OcadPdfWriter.StartGroup { Key="D", Cats  = new List<string> {"DAM", "D55", "HAK", "H65"}},
        new OcadPdfWriter.StartGroup { Key="E", Cats  = new List<string> {"DAK", "D45", "D50", "D60", "D65", "H70"}},
        new OcadPdfWriter.StartGroup { Key="F", Cats  = new List<string> {"D70", "D75", "H75", "H80"}},
        new OcadPdfWriter.StartGroup { Key="G", Cats  = new List<string> {"D14", "D16", "H14", "H16"}},
        new OcadPdfWriter.StartGroup { Key="H", Cats  = new List<string> {"D10", "D12", "H10", "H12", "sCOOL"}},
        new OcadPdfWriter.StartGroup { Key="J", Cats  = new List<string> {"DB", "HB", "Offen Mittel", "Offen Kurz", "Familien"}},
      };

      OcadPdfWriter w = new OcadPdfWriter(_courseDuebi2020);
      w.WriteStartGroups(startGroups, new TimeSpan(0, 9, 30, 0), new TimeSpan(0, 12, 09, 0),
        80, nx: 5, ny: 16, marginX: 28.3f, marginY: 34.0f);

      List<OcadPdfWriter.StartGroup> catGroups = new List<OcadPdfWriter.StartGroup>
      {
        new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"H10", "H12", "H14", "H16", "H18", "HAL", "HAM", "HAK", "HB",
          "H35", "H40", "H45", "H50", "H55", "H60", "H65", "H70", "H75", "H80", "Offen Lang", "Offen Mittel", "Offen Kurz"}},

                new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"D10", "D12", "D14", "D16", "D18", "DAL", "DAM", "DAK", "DB",
          "D35", "D40", "D45", "D50", "D55", "D60", "D65", "D70", "D75", null, "Familien", "sCOOL"}},
      };

      w.WriteCategories("Winter Stadt OL", catGroups, startGroups);
      //int h = 9;
      //int min = 30;
      //string name = string.Format(@"C:\daten\felix\kapreolo\karten\effretikon\2015\StartTimes_{0}_{1}.pdf", h, min);
      //w.WriteStartTimes(name, "D18/DAK/D60/D55/H70", new DateTime(2016, 1, 10, h, min, 0));

      w.PrintCourseRunners();

    }

    private static readonly string _courseOpfikon2018 = @"C:\daten\felix\kapreolo\karten\opfikon\2018\GlattalOL\Glattalol_v2_post.ocd";
    private static readonly string _imagesOpfikon2018 = @"C:\daten\felix\kapreolo\karten\opfikon\2018\GlattalOL\Po\P*.jpg";

    [TestMethod]
    public void Opfikon2018_CreateImagePoBes()
    {
      CmdCreateImagePoBe cmd = new CmdCreateImagePoBe(_courseOpfikon2018, new List<ControlInfo>
      {
        new ControlInfo{Key = "36", Info="Sockel"},
        new ControlInfo{Key = "43", Info="Sockel"},
        new ControlInfo{Key = "57", Info="Sockel"},
        new ControlInfo{Key = "58", Info="Sockel"},
        new ControlInfo{Key = "59", Info="Sockel"},
        new ControlInfo{Key = "67", Info="K-Binder"},
        new ControlInfo{Key = "68", Info="Sockel"},
        new ControlInfo{Key = "86", Info="K-Binder"},
      });

      //cmd.WriteImages("Setzen 1", _imagesOpfikon2018);
      //cmd.WriteImages("Setzen 2", _imagesOpfikon2018);
      //cmd.WriteImages("Setzen 3", _imagesOpfikon2018);
      //cmd.WriteImages("Setzen 4", _imagesOpfikon2018);
      cmd.WriteImages("D4550", _imagesOpfikon2018);
    }

    [TestMethod]
    public void Effi()
    {
      //PdfTextTest.TestEffi(@"C:\daten\felix\kapreolo\karten\effretikon\2015\_test.pdf",
      //    @"C:\daten\felix\kapreolo\karten\effretikon\2015\Ausschreibung_EffiOL16_DE.pdf");

      string courseFile = @"C:\daten\felix\kapreolo\karten\effretikon\2015\OL2016.ocd";

      CmdCreateImagePoBe cmd = new CmdCreateImagePoBe(courseFile,
        new List<ControlInfo>
      {
          new ControlInfo { Key = "142", Info = "Sockel"},
          new ControlInfo { Key = "144", Info = "Sender"},
          new ControlInfo { Key = "186", Info = "Sender"},
          new ControlInfo { Key = "188", Info = "Sockel"},
          new ControlInfo { Key = "181", Info = "Sockel"},
          new ControlInfo { Key = "119", Info = "Sockel"},
          new ControlInfo { Key = "157", Info = "Hinweis 30\""}
        });

      foreach (var course in new[] { "Setzen1", "Setzen2", "Setzen3", "Setzen4", "Setzen5", "SetzenFinal" })
      {
        cmd.WriteImages(course, @"c:\temp\o*.jpg");
      }
      // w.WriteImages("Setzen1", @"c:\temp\o*.jpg");

      List<OcadPdfWriter.StartGroup> startGroups = new List<OcadPdfWriter.StartGroup>
      {
        new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"H20S", "HS"}},
        new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"D20S", "DS"}},
        new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"FinalS"}},
        new OcadPdfWriter.StartGroup { Key="A", Cats  = new List<string> {"D18", "HAL", "H35", "H40", "H45"}},
        new OcadPdfWriter.StartGroup { Key="B", Cats  = new List<string> {"DAL", "D35", "H50", "Offen Lang"}},
        new OcadPdfWriter.StartGroup { Key="C", Cats  = new List<string> {"DAM", "D40", "D45", "HAK", "HAM"}},
        new OcadPdfWriter.StartGroup { Key="D", Cats  = new List<string> {"D50", "H55", "H60", "H65"}},
        new OcadPdfWriter.StartGroup { Key="E", Cats  = new List<string> {"DAK", "D55", "D60", "H18", "H70"}},
        new OcadPdfWriter.StartGroup { Key="F", Cats  = new List<string> {"D65", "D70", "D75", "H75", "H80"}},
        new OcadPdfWriter.StartGroup { Key="G", Cats  = new List<string> {"D14", "D16", "H14", "H16"}},
        new OcadPdfWriter.StartGroup { Key="H", Cats  = new List<string> {"D10", "D12", "H10", "H12", "Kids"}},
        new OcadPdfWriter.StartGroup { Key="J", Cats  = new List<string> {"DB", "HB", "Offen Mittel", "Offen Kurz", "Sie und Er", "Familien"}},
      };

      OcadPdfWriter w = new OcadPdfWriter(courseFile);
      w.WriteStartGroups(startGroups, new TimeSpan(0, 9, 30, 0), new TimeSpan(0, 12, 30, 0), 60,
        nx: 5, ny: 13, marginX: 28.3f, marginY: 34f);

      List<OcadPdfWriter.StartGroup> catGroups = new List<OcadPdfWriter.StartGroup>
      {
        new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"H10", "H12", "H14", "H16", "H18", "H20S", "HS", "HAL", "HAM", "HAK", "HB",
          "H35", "H40", "H45", "H50", "H55", "H60", "H65", "H70", "H75", "H80", "Offen Lang", "Offen Mittel", "Offen Kurz"}},

                new OcadPdfWriter.StartGroup { Key="_1", Cats  = new List<string> {"D10", "D12", "D14", "D16", "D18", "D20S", "DS", "DAL", "DAM", "DAK", "DB",
          "D35", "D40", "D45", "D50", "D55", "D60", "D65", "D70", "D75", null, "Familien", "Sie und Er", "Kids"}},
      };

      w.WriteCategories("20. Effretiker OL", catGroups, startGroups);
      //int h = 9;
      //int min = 30;
      //string name = string.Format(@"C:\daten\felix\kapreolo\karten\effretikon\2015\StartTimes_{0}_{1}.pdf", h, min);
      //w.WriteStartTimes(name, "D18/DAK/D60/D55/H70", new DateTime(2016, 1, 10, h, min, 0));

      w.PrintCourseRunners();
    }

  }
}