

using iTextSharp.text.pdf;
using Ocad;
using Ocad.StringParams;
using OTextSharp.Models;
using System.Collections.Generic;
using System.IO;

namespace OTextSharp.Commands
{
  public class CmdCreateImagePoBe
  {
    private readonly string _ocadFile;
    private readonly IList<ControlInfo> _infos;

    public CmdCreateImagePoBe(string ocadFile, IList<ControlInfo> infos)
    {
      _ocadFile = ocadFile;
      _infos = infos;
    }

    public void WriteImages(string courseName, string imageTemplate)
    {
      using (OcadReader reader = OcadReader.Open(_ocadFile))
      {
        IList<StringParamIndex> idxs = reader.ReadStringParamIndices();
        WriteImages(courseName, reader, idxs, imageTemplate);
      }
    }

    private void WriteImages(string courseName, OcadReader reader, IList<StringParamIndex> idxs, string imageTemplate)
    {
      Course course = reader.ReadCourse(courseName, idxs);

      using (ImagePoBeWriter w = new ImagePoBeWriter(reader, _infos,
        Path.ChangeExtension(_ocadFile, "Foto." + courseName + ".pdf"), imageTemplate))
      {
        float x0 = 20;
        float y0 = 810;

        w.SetFontSize(16);
        w.ShowText(PdfContentByte.ALIGN_LEFT, courseName, x0, y0, 0);

        y0 -= w.DescriptionSize;
        float x = x0;
        float y = y0;
        foreach (var section in course)
        {
          Control ctr = (Control)section;
          w.DrawControl(ctr.Name, x, y);
          y -= w.DescriptionSize;

          if (y < 650)
          {
            y = y0;
            x = x + 290;
          }
        }

        w.SetY(1);
        foreach (var section in course)
        {
          Control ctr = (Control)section;
          w.AddImage(ctr);
        }
      }
    }
  }
}