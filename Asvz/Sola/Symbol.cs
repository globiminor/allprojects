namespace Asvz.Sola
{
  public static class SymT
  {
    public const int Strecke = 101000;
    public const int StreckeKurz = 101001;
    public const int Laufrichtung = 101002;
    public const int VorherNacher = 102000;

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
    public const int TextRahmen = 302005;
    public const int TextSpital = 304000;
    public const int TextSanTitel = 305000;
    public const int TextSanDetail = 306000;
    public const int TextSanBg = 307000;

    public const int KmDist = 310000;
    public const int TextKmDist = 311000;
    public const int KmStartEnd = 312000;
    public const int StreckenInfo = 315000;

    public const int Wald = 501000;
    public const int Siedlung = 502000;
    public const int Teer = 503000;

    public const int Deckweiss = 600000;
    public const int TextKmRaster = 601000;
    public const int KmRasterLinie = 602000;
    public const int Bewilligung = 603000;
  }

  public static class SymS
  {
    public const int Strecke = 101000;
    public const int Verkuerzt = 101001;
    public const int Laufrichtung = 101002;
    public const int VorherNachher = 102000;

    public const int UebergabeTeil = 112001;
    public const int ZielTeil = 113001;

    public const int LinieBreit = 150000;

    public const int Verpflegung = 201000;

    public const int Sanitaet = 205000;
    public const int SanitaetPfeil = 205001;
    public const int SanitaetText = 205002;
    public const int Treffpunkt = 210000;
    public const int TreffpunktText = 210001;
    public const int StreckenHinweis = 211000;
    public const int AchtungPfeil = 211001;
    public const int AchtungZeichen = 211002;

    public const int TextStrecke = 301000;
    public const int TextUebergabe = 302000;
    public const int RahmenText = 302001;
    public const int TextInfo = 304000;
    public const int TextStreckeNr = 305000;
    public const int BoxStreckeNr = 305001;
    public const int RahmenStrecke = 305002;

    public const int KmStrich = 310000;
    public const int TextKm = 311000;

    public const int TextBewilligung = 320000;
    public const int TextBewilligungVoid = 320999;

    public const int Nordpfeil = 715000;

    public const int Ausschnitt = 200004;
  }

  public static class SymD
  {
    public const int Objekt          =   1002;

    public const int UeVorherNachher = 102001;
    public const int DtVorherNachher = 102002;
    public const int DtNeustart      = 102003;
    public const int DtZiel          = 102004;

    public const int Anfahrt         = 115000;
    public const int AnfahrtU        = 115001;
    public const int Abfahrt         = 116000;
    public const int AbfahrtU        = 116001;

    public const int IdxRot          = 117000;
    public const int IdxSchwarz      = 117001;
    public const int UeAusserhalb    = 117003;

    public const int UeNeustart = 121000;
    public const int UeCircle   = 121001;

    public const int LinieBreit = 150000;
    public const int LinieMittel = 150001;
    public const int LinieSchmal = 150002;
    public const int Absperrband = 150003;

    public const int Uebergaberaum = 151000;
    public const int DtAusserhalb  = 167000;

    public const int Uebergabe     = 200000;
    public const int Detail        = 200002;
    public const int DtAusschnitt  = 200003;
    public const int TrAusschnitt  = 200004;

    public const int Sanitaet      = 205000;
    public const int GepBleibt     = 211000;
    public const int GepVon        = 212000;
    public const int GepNach       = 213000;

    public const int Bus = 221000;
    public const int Tram = 222000;
    public const int Bahn = 223000;
    public const int Lsb = 224000;

    public const int WC = 225000;
    public const int Imbiss = 226000;

    public const int Sponser   = 250000;
    public const int Zeitnahme = 251000;
    public const int Speaker = 252000;
    public const int StickAusgabe = 253000;

    public const int UeNordRoh = 270000;
    public const int DtNordRoh = 270001;
    public const int TrNordRoh = 270002;

    public const int TextGross = 301001;
    public const int TextMittel = 301002;
    public const int TextKlein = 301003;
    public const int DtTextList = 301004;
    public const int BoxText = 301010;

    public const int UeLegendeBox = 701000;
    public const int UeTextLegende = 711000;
    public const int UeMassstab = 712000;
    public const int UeNordpfeil = 715000;

    public const int DtLegendeBox = 751000;
    public const int DtTextLegende = 761000;
    public const int DtMassstab = 762000;
    public const int DtNordpfeil = 763000;

    public const int TrLegendeBox = 771000;
    public const int TrTextLegende = 772000;
    public const int TrMassstab = 773000;
    public const int TrNordpfeil = 774000;

  }
}
