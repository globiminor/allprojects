using System;
using System.Collections.Generic;
using System.IO;
using Ocad;

namespace Asvz.Forchlauf
{
  public class ForchProfile : Profile
  {
    private ForchData _data;

    public ForchProfile(ForchData data)
      : base(data)
    {
      _data = data;

      FormatDist = "N1";
    }

    public void WriteProfile(string template, string result, Kategorie kategorie)
    {
      Ocad9Writer writer;
      string dir = Path.GetDirectoryName(result);
      if (Directory.Exists(dir) == false)
      { Directory.CreateDirectory(dir); }
      File.Copy(template, result, true);

      writer = Ocad9Writer.AppendTo(result);
      try
      {

        Ocad9Reader pTemplate = (Ocad9Reader)OcadReader.Open(template);
        ReadTemplate(pTemplate);
        pTemplate.Close();

        ForchCategorie cat = _data.GetKategorie(kategorie);

        int iNStrecken = _data.Strecken.Count;

        double sumDist = 0;
        Random random = new Random(1);

        double distStart = 0;

        WriteStart(writer, "Fluntern", distStart, sumDist);

        InitHausWald(out double nextHaus, out double nextWald);


        WriteProfile(writer, cat.ProfilNormed, sumDist);

        WriteUmgebung(writer, cat.Strecke, cat.ProfilNormed, sumDist,
          random, ref nextWald, ref nextHaus);

        WriteLayoutStrecke(writer, cat, " ", "Fluntern",
          sumDist, distStart);

        sumDist += cat.DispLength;

        WriteEnd(writer, cat.Name, sumDist);

        WriteLayout(writer, sumDist);
        WriteParams(writer, sumDist);
      }
      finally
      { writer.Close(); }
    }

  }
}
