using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Asvz.Sola
{
  public static class Ddx
  {
    private static XmlDocument _ddx;
    private static string _ddxPath;
    private const string _nodeLaufdatum = "laufdatum";
    private const string _nodeBewilligung = "bewilligung";
    private const string _nodeDhm = "dhm";

    private const string _nodeUebergabe = "Uebergabe";
    private const string _nodeUeName = "name";
    private const string _nodeUeVon = "von";
    private const string _nodeUeNach = "nach";
    private const string _nodeUeVorlage = "vorlage";
    private const string _nodeUeKarte = "karte";

    private const string _nodeStrecke = "Strecke";
    private const string _nodeStNummer = "nummer";
    private const string _nodeStVorlage = "vorlage";

    private const string _nodeKategorie = "Kategorie";
    private const string _nodeKatTyp = "typ";
    private const string _nodeKatDistanz = "distanz";
    private const string _nodeKatOffsetStart = "offsetStart";
    private const string _nodeKatOffsetEnd = "offsetEnd";
    private const string _nodeKatStufe = "stufe";

    private static string _workDir;

    private static List<Uebergabe.Info> _uebergabe;
    private static List<SolaStrecke> _strecken;

    static Ddx()
    {
      _workDir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

      _ddxPath = _workDir + "SolaDdx.xml";
      _ddx = new XmlDocument();

      _ddx.Load(_ddxPath);

      GetUebergaben();
      GetStrecken();
    }

    private static void GetUebergaben()
    {
      XmlNodeList nodes = _ddx.SelectNodes("//" + _nodeUebergabe);
      _uebergabe = new List<Uebergabe.Info>(nodes.Count);
      foreach (XmlNode node in nodes)
      {
        XmlAttribute attr;

        string name = node.Attributes[_nodeUeName].Value;

        attr = node.Attributes[_nodeUeVon];
        if (attr == null || int.TryParse(attr.Value, out int from) == false)
        {
          int to = int.Parse(node.Attributes[_nodeUeNach].Value);
          from = to - 1;
        }

        string vorlage = node.Attributes[_nodeUeVorlage].Value;
        if (vorlage != Path.GetFullPath(vorlage))
        {
          vorlage = _workDir + "Uebergabe" + Path.DirectorySeparatorChar + vorlage;
        }

        string karte = node.Attributes[_nodeUeKarte].Value;

        Uebergabe.Info uebergabe = new Uebergabe.Info(name, from, vorlage, karte);

        UebergabeTransport.Info trans = GetTransport(node);
        uebergabe.Transport = trans;

        _uebergabe.Add(uebergabe);
      }
      _uebergabe.Sort();
    }

    private static T ParseEnum<T>(string value)
    {
      return (T)Enum.Parse(typeof(T), value, true);
    }
    private static UebergabeTransport.Info GetTransport(XmlNode uebergabe)
    {
      XmlNodeList list = uebergabe.SelectNodes("./Transport");
      if (list == null || list.Count != 1)
      { return null; }

      XmlNode transNode = list[0];

      XmlAttribute attr;

      UebergabeTransport.Info transInfo = new UebergabeTransport.Info();

      attr = transNode.Attributes["vonTyp"];
      if (attr != null)
      { transInfo.VonTyp = ParseEnum<UebergabeTransport.Typ>(attr.Value); }

      attr = transNode.Attributes["hatNach"];
      if (attr != null)
      { transInfo.NachTyp = ParseEnum<UebergabeTransport.Typ>(attr.Value); }

      attr = transNode.Attributes["von"];
      if (attr != null)
      { transInfo.Von = int.Parse(attr.Value); }

      attr = transNode.Attributes["nach"];
      if (attr != null)
      { transInfo.Nach = int.Parse(attr.Value); }

      return transInfo;
    }
    private static void GetStrecken()
    {
      XmlNodeList nodes = _ddx.SelectNodes("//" + _nodeStrecke);
      _strecken = new List<SolaStrecke>(nodes.Count);
      foreach (XmlNode node in nodes)
      {
        int strecke;
        string vorlage;
        XmlAttribute attr;

        attr = node.Attributes[_nodeStNummer];
        strecke = int.Parse(attr.Value);

        vorlage = node.Attributes[_nodeStVorlage].Value;
        if (vorlage != Path.GetFullPath(vorlage))
        {
          vorlage = _workDir + "Strecken" + Path.DirectorySeparatorChar + vorlage;
        }

        SolaStrecke info = new SolaStrecke(strecke, vorlage);

        XmlNodeList cats = node.SelectNodes(_nodeKategorie);
        foreach (XmlNode nodeCat in cats)
        {
          attr = nodeCat.Attributes[_nodeKatTyp];
          Kategorie typ = (Kategorie)int.Parse(attr.Value);

          double? distance = null;
          attr = nodeCat.Attributes[_nodeKatDistanz];
          if (attr != null)
          { distance = double.Parse(attr.Value) * 1000; }

          double offsetStart = 0;
          attr = nodeCat.Attributes[_nodeKatOffsetStart];
          if (attr != null)
          { offsetStart = double.Parse(attr.Value) * 1000; }

          double offsetEnd = 0;
          attr = nodeCat.Attributes[_nodeKatOffsetEnd];
          if (attr != null)
          { offsetEnd = double.Parse(attr.Value) * 1000; }

          attr = nodeCat.Attributes[_nodeKatStufe];
          string stufe = attr.Value;

          info.Categories.Add(new SolaCategorie(typ, distance, offsetStart, offsetEnd, stufe));
        }

        _strecken.Add(info);
      }
      _strecken.Sort();
    }

    public static string WorkDir
    {
      get { return _workDir; }
    }
    public static string Bewilligung
    {
      get
      {
        XmlNode node = _ddx.SelectSingleNode("Sola");
        return node.Attributes[_nodeBewilligung].Value;
      }
    }

    public static string DhmPfad
    {
      get
      {
        XmlNode node = _ddx.SelectSingleNode("Sola");
        return node.Attributes[_nodeDhm].Value;
      }
    }


    public static DateTime Laufdatum
    {
      get
      {
        XmlNode node = _ddx.SelectSingleNode("Sola");
        return DateTime.Parse(node.Attributes[_nodeLaufdatum].Value);
      }
    }

    public static IList<Uebergabe.Info> Uebergabe
    {
      get { return _uebergabe; }
    }

    public static IList<SolaStrecke> Strecken
    {
      get { return _strecken; }
    }

  }
}
