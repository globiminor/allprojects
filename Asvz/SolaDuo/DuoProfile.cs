using System;
using System.IO;
using Ocad;

namespace Asvz.SolaDuo
{
  class DuoProfile : Profile
  {
    private DuoData _data;

    public DuoProfile(DuoData data)
      : base(data)
    {
      _data = data;
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
        writer.DeleteElements( new int[] { 301005, 101000, 103000 } );
        Ocad9Reader pTemplate = (Ocad9Reader)OcadReader.Open(template);
        ReadTemplate(pTemplate);
        pTemplate.Close();

        double sumDist = 0;
        double distStart = 0;
        Random random = new Random(1);
        double nextWald, nextHaus;
        InitHausWald(out nextHaus, out nextWald);

        WriteStart(writer, _data.PostenListe[0].Name, distStart, sumDist);

        int nStrecken = _data.Strecken.Count;
        for (int iStrecke = 0; iStrecke < nStrecken; iStrecke++)
        {
          DuoCategorie cat = (DuoCategorie)_data.Strecken[iStrecke].Categories[0];

          WriteProfile(writer, cat.ProfilNormed, sumDist);

          WriteUmgebung(writer, cat.Strecke, cat.ProfilNormed, sumDist,
            random, ref nextWald, ref nextHaus);

          WriteLayoutStrecke(writer, cat, _data.PostenListe[iStrecke].Id,
            _data.PostenListe[iStrecke + 1].Name, sumDist, distStart);

          sumDist += cat.Laenge();
        }
        WriteEnd(writer, _data.PostenListe[nStrecken].Id, sumDist);

        WriteLayout(writer, sumDist);
        WriteParams(writer, sumDist);
      }
      finally
      { writer.Close(); }
    }

  }

}
