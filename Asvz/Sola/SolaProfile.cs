using System;
using System.Collections.Generic;
using System.IO;
using Ocad;

namespace Asvz.Sola
{
  public class SolaProfile : Profile
  {
    private SolaData _data_;

    public SolaProfile(SolaData data)
      : base(data)
    {
      _data_ = data;
    }

    public void WriteProfiles(string template, string result, int von, int bis,
      bool sumDistance, Kategorie kategorie)
    {
      string sText;

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

        int iNStrecken = _data_.Strecken.Count;
        IList<SolaStrecke> strecke = _data_.Strecken;

        double sumDist = 0;
        Random random = new Random(1);

        double distStart = 0;

        if (sumDistance)
        {
          for (int iStrecke = 0; iStrecke < von; iStrecke++)
          {
            Categorie cat = _data_.Categorie(iStrecke, kategorie);
            double dDist = cat.Laenge();
            distStart += dDist;
          }
        }

        WriteStart(writer, Ddx.Uebergabe[von].Name, distStart, sumDist);

        double nextWald, nextHaus;
        InitHausWald(out nextHaus, out nextWald);

        for (int iStrecke = von; iStrecke < bis; iStrecke++)
        {
          Categorie cat = strecke[iStrecke].GetCategorie(Kategorie.Default);

          WriteProfile(writer, cat.ProfilNormed, sumDist);

          WriteUmgebung(writer, cat.Strecke, cat.ProfilNormed, sumDist,
            random, ref nextWald, ref nextHaus);

          WriteLayoutStrecke(writer, cat, string.Format("{0}", iStrecke + 1),
            Ddx.Uebergabe[iStrecke + 1].Name,
            sumDist, distStart);

          sumDist += cat.Laenge();
        }

        if (bis + 1 < Ddx.Uebergabe.Count)
        { sText = string.Format("{0}", bis + 1); }
        else
        { sText = "Ziel"; }

        WriteEnd(writer, sText, sumDist);

        WriteLayout(writer, sumDist);
        WriteParams(writer, sumDist);
      }
      finally
      { writer.Close(); }
    }

  }
}
