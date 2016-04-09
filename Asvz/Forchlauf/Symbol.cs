using System;
using System.Collections.Generic;
using System.Text;

namespace Asvz.Forchlauf
{
  internal class SymF
  {
    public const int StreckeLang = 101000;
    public const int LaufrichtungLang = 101001;
    public const int StreckeMittel = 102000;
    public const int LaufrichtungMittel = 102001;
    public const int StreckeKurz = 103000;
    public const int LaufrichtungKurz = 103001;

    public const int StreckeLangMittelKurz = 101002;
    public const int StreckeLangMittel = 101003;

    public const int Start = 111000;
    public const int Uebergabe = 112000;
    public const int UebergabeTeil = 112001;
    public const int Ziel = 113000;
    public const int ZielTeil = 113001;

    public const int Transport = 116000;
    public const int TransportUi = 116001;
    public const int TransportHilf = 116005;

    public const int LinieBreit = 150000;

    public const int Verpflegung = 201000;
    public const int Sanitaet = 205000;

    public const int TextStrecke = 301000;
    public const int TextStreckeBox = 301001;

    public const int TextGross = 302000;

    public const int TextKlein = 303000;
    public const int TextLang = 303001;
    public const int TextMittel = 303002;
    public const int TextKurz = 303003;

    public const int TextRahmen = 302005;
    public const int TextSpital = 304000;
    public const int TextSanTitel = 305000;
    public const int TextSanDetail = 306000;
    public const int TextSanBg = 307000;

    public const int KmDist = 310000;
    public const int TextKmDist = 311000;
    public const int TextKmDistL = 311003;
    public const int TextKmDistM = 311002;
    public const int TextKmDistK = 311001;
    public const int KmStartEnd = 312000;

    public const int Wald = 501000;
    public const int Siedlung = 502000;
    public const int Teer = 503000;

    public const int Deckweiss = 600000;
    public const int TextKmRaster = 601000;
    public const int KmRasterLinie = 602000;
    public const int Bewilligung = 603000;
  }
}
