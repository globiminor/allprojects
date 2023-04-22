using System;
using System.Data;
using Basics.Geom;
using Ocad;
using Shape;

namespace Asvz.SolaDuo
{
  public class DuoMap : IDisposable
  {
    readonly Symbol[] _symbols = new Symbol[]
      {
        new Symbol("Fussweg", 112000),
        new Symbol("Fahrstraes", 111000),
        new Symbol("NebenStr3", 110000, 107010),
        new Symbol("NebenStr6", 109000, 107010),
        new Symbol("NebenStr_Rampe", 105000), // TODO
        new Symbol("VerbindStr4", 108000),
        new Symbol("VerbindStr6", 107000, 107010),
        new Symbol("VerbindStr_Rampe", 105000), // TODO
        new Symbol("HauptStrAB4", 106000),
        new Symbol("HauptStrAB6", 105000, 103010),
        new Symbol("HauptStrAB_Rampe", 105000), // TODO
        new Symbol("Autostr", 103000, 103010),
        new Symbol("Autostr_Rampe", 103000), // TODO
        new Symbol("Autob_Ri", 102000),
        new Symbol("Autob_Ri_Rampe", 102000), // TODO
        new Symbol("Autobahn", 101000, 101010),
        new Symbol("Autobahn_Rampe",102000, 103010), // TODO
        new Symbol("Zugang", 401000),

        new Symbol("NS_Bahn", 204000, 204010),
        new Symbol("SS_Bahn", 206000, 204010),
        new Symbol("MS_Bahn", 203000), 
        new Symbol("Luftseilbahn", 302000),
        new Symbol("Standseilbahn", 303000),

        new Symbol("Gebaeude", 601000),

        new Symbol("Obstanlage", 501000),
        new Symbol("Fels", 502000),
        new Symbol("Geroell", 507000),
        new Symbol("Reben", 516000),
        new Symbol("See", 517000),
        new Symbol("Stausee", 517000),
        new Symbol("Siedl", 518000),
        new Symbol("Stadtzentr", 519000),
        new Symbol("Wald", 527000),

        new Symbol( "Fluss", 401000)
      };

    OcadWriter _writer;

    public DuoMap(string ocdFile)
    {
      _writer = OcadWriter.AppendTo(ocdFile);
    }

    ~DuoMap()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (_writer != null)
      { _writer.Close(); }
      _writer = null;
    }

    private class Symbol
    {
      public readonly string Name;
      private int _symbol;
      private int _dLevel = 0;
      private int _tunnelSym = -1;
      private int _dBreite = 0;

      public Symbol(string name, int symbol)
      {
        Name = name;
        _symbol = symbol;
      }

      public Symbol Clone()
      {
        Symbol clone = new Symbol(Name, _symbol);
        clone._dLevel = _dLevel;
        clone._tunnelSym = _tunnelSym;
        clone._dBreite = _dBreite;

        return clone;
      }
      public Symbol(string name, int symbol, int tunnelSymbol)
        : this(name, symbol)
      {
        _tunnelSym = tunnelSymbol;
      }

      public int GetSymbol()
      {
        return _symbol + _dLevel + _dBreite;
      }

      public void SetBaute(string baute)
      {
        if (baute == "Keine Kunstbaute")
        {
        }
        else if (baute == "Brücke")
        {
        }
        else if (baute == "Tunnel")
        {
          if (_tunnelSym < 0)
          { throw new NotImplementedException(Name + " " + baute); }

          _symbol = _tunnelSym;
        }
        else
        { throw new NotImplementedException(baute); }

      }
      public void SetLevel(int level)
      {
        if (level == 1)
        { }
        else if (level == 2)
        { _dLevel = 1; }
        else if (level == -1)
        { _dLevel = 5; }
        else if (level == -2)
        { _dLevel = 6; }
        else
        { throw new NotImplementedException("Unhandled level " + level); }

      }
      public void SetBreite(int breite)
      {
        if (breite == 2)
        { _dBreite = 2; }
        else if (breite == 4)
        { _dBreite = 4; }
        else if (breite == 5)
        { _dBreite = 5; }
        else if (breite == 6)
        { _dBreite = 6; }
        else if (breite == 7)
        { _dBreite = 7; }
        else if (breite == 8)
        { _dBreite = 8; }
        else if (breite == 9)
        { _dBreite = 9; }
        else if (breite == 10)
        { _dBreite = 10; }
        else if (breite == 15)
        { _dBreite = 7; }
        else if (breite == 17)
        { _dBreite = 5; }
        else if (breite == 18)
        { _dBreite = 7; }
        else if (breite == 19)
        { _dBreite = 9; }
        else if (breite == 20)
        { _dBreite = 15; }
        else if (breite == 89)
        { _dBreite = 7; }
        else
        { throw new NotImplementedException("Unhandled breite " + breite); }

      }

    }


    public void Import(string shpFile)
    {
      ShapeReader reader = new ShapeReader(shpFile);

      string shapeCol = "Shape";
      string objTyp = "ObjVal";
      string baute = "Construct";
      string stufe = "EdgeLevel";
      string breite = "Breite";


      int iStufe = -1;
      int iBaute = -1;
      int iBreite = -1;
      foreach (var row in reader)
      {
        DataTable tbl = row.Table;
        iStufe = tbl.Columns.IndexOf(stufe);
        iBaute = tbl.Columns.IndexOf(baute);
        iBreite = tbl.Columns.IndexOf(breite);
        break;
      }

      foreach (var row in reader)
      {

        GeoElement elem;
        object geom = row[shapeCol];
        {
          if (geom is Surface a)
          { elem = new GeoElement(a); }
          else if (geom is Polyline l)
          { elem = new GeoElement(l); }
          else if (geom is IPoint p)
          { elem = new GeoElement(p); }
          else
          { throw new NotImplementedException("Unhandled type " + geom.GetType()); }
        }
        string val = ((string)row[objTyp]).Trim();

        if (val == "Fluss_U")
        { continue; }
        if (val == "Seeachse")
        { continue; }


        Symbol sym = null;
        foreach (var symbol in _symbols)
        {
          if (symbol.Name == val)
          {
            sym = symbol.Clone();
            break;
          }
        }
        if (sym == null)
        { throw new NotImplementedException(val); }


        if (iBaute >= 0)
        {
          string bau = ((string)row[iBaute]).Trim();
          sym.SetBaute(bau);
        }

        // EdgeLevel
        if (iStufe >= 0)
        {
          int level = (int)row[iStufe];
          sym.SetLevel(level);
        }

        if (iBreite >= 0)
        {
          int breit = (int)row[iBreite];
          sym.SetBreite(breit);
        }
        elem.Symbol = sym.GetSymbol();

        if (elem.Geometry is GeoElement.Surface)
        { elem.Type = GeomType.area; }
        else if (elem.Geometry is GeoElement.Line)
        { elem.Type = GeomType.line; }
        else if (elem.Geometry is GeoElement.Point)
        { elem.Type = GeomType.point; }
        else
        { throw new NotImplementedException("Unhandled type " + elem.Geometry.GetType()); }

        _writer.Append(elem);
      }
    }
  }
}
