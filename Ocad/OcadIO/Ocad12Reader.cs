using Ocad.StringParams;
using Ocad.Symbol;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ocad
{
  internal class Ocad12Io : OcadIo //Ocad11Reader
  {
    public Ocad12Io(Stream stream)
      : base(stream)
    { }

    public override Setup ReadSetup() => Ocad9Io.ReadSetup(this);
    public override string ReadDescription() => Ocad11Io.ReadDescription(this);
    public override BaseSymbol ReadSymbol() => Ocad11Io.ReadSymbol(this);
    public override IEnumerable<ColorInfo> ReadColorInfos() => Ocad9Io.ReadColorInfos(this);
    public override ElementIndex ReadIndex(int i) => Ocad9Io.ReadIndex(this, i);
    public override int ReadElementLength() => Ocad9Io.ReadElementLength(this);
    public override int GetElementTextCount(Element element, string text) => Ocad9Io.TextCountV9(text);
    public override ElementIndex GetIndex(Element element) => Ocad9Io.GetIndex(this, element);
    public override void Write(ElementIndex index) => Ocad9Io.Write(this, index);
    public override void WriteElementSymbol(int symbol) => Ocad9Io.WriteElementSymbol(this, symbol);
    public override Element ReadElement()
    {
      Element elem = new Element(false);

      elem.Symbol = Reader.ReadInt32();
      if (elem.Symbol < -4 || elem.Symbol == 0)
      { return null; }

      elem.Type = (GeomType)Reader.ReadByte();
      if (elem.Type == 0)
      { return null; }

      Reader.ReadByte(); // reserved

      elem.Angle = Reader.ReadInt16() * Math.PI / 1800.0; // 0.1 Degrees

      elem.Color = Reader.ReadInt32();
      elem.LineWidth = Reader.ReadInt16();
      elem.Flags = Reader.ReadInt16();
      elem.ServerObjectId = Reader.ReadInt32();
      double height256MM = Reader.ReadInt32();
      elem.Height = height256MM / (256.0 * 1000.0);
      elem.CreationDate = DateTime.FromOADate(Reader.ReadDouble());
      elem.MultirepresenationId = Reader.ReadInt32();
      elem.ModificationDate = DateTime.FromOADate(Reader.ReadDouble());

      int nPoint = Reader.ReadInt32();
      int nText = Reader.ReadInt16();
      int nObjectString = Reader.ReadInt16();
      int nDatabaseString = Reader.ReadInt16();
      byte objectStringType = Reader.ReadByte();

      elem.ObjectStringType = (ObjectStringType)objectStringType;

      Reader.ReadByte();

      List<Coord> coords = ReadCoords(nPoint);
      elem.Geometry = Coord.GetGeometry(elem.Type, coords);
      if (nText > 0)
      { elem.Text = Reader.ReadUnicodeString(); }
      if (nObjectString > 0)
      {
        if (nText > 0)
        {
          int n = nText * 8;
          for (int i = elem.Text.Length * 2 + 2; i < n; i++)
          { Reader.BaseStream.ReadByte(); }
        }

        elem.ObjectString = Reader.ReadUnicodeString();
      }

      return elem;
    }

    protected override FileParam Init(int sectionMark, int version)
    {
      FileParamV12 fileParam = new FileParamV12();

      fileParam.SectionMark = sectionMark;
      fileParam.Version = version;
      fileParam.SubVersion = Reader.ReadInt16();
      fileParam.FirstSymbolBlock = Reader.ReadInt32();
      fileParam.FirstIndexBlock = Reader.ReadInt32();
      fileParam.OfflineSyncSerial = Reader.ReadInt32();
      fileParam.CurrentFileVersion = Reader.ReadInt32();

      Reader.ReadInt32(); // reserved
      Reader.ReadInt32(); // reserved
      fileParam.FirstStrIdxBlock = Reader.ReadInt32();
      fileParam.FileNamePosition = Reader.ReadInt32();
      fileParam.FileNameSize = Reader.ReadInt32();
      Reader.ReadInt32(); // reserved
      Reader.ReadInt32(); // reserved
      Reader.ReadInt32(); // reserved
      fileParam.MrStartBlockPosition = Reader.ReadInt32();

      return fileParam;
    }

    public override void ReadAreaSymbol(AreaSymbol symbol)
    {
      symbol.BorderSym = Reader.ReadInt32();
      symbol.FillColor = Reader.ReadInt16();
      symbol.HatchMode = (AreaSymbol.EHatchMode)Reader.ReadInt16();
      symbol.HatchColor = Reader.ReadInt16();
      symbol.HatchLineWidth = Reader.ReadInt16();
      symbol.HatchLineDist = Reader.ReadInt16();
      symbol.HatchAngle1 = Reader.ReadInt16();
      symbol.HatchAngle2 = Reader.ReadInt16();
      symbol.FillOn = Convert.ToBoolean(Reader.ReadByte());
      symbol.BorderOn = Convert.ToBoolean(Reader.ReadByte());
      symbol.StructMode = (AreaSymbol.EStructMode)Reader.ReadByte();
      symbol.StructDraw = (AreaSymbol.EStructDraw)Reader.ReadByte();
      symbol.StructWidth = Reader.ReadInt16();
      symbol.StructHeight = Reader.ReadInt16();
      symbol.StructAngle = Reader.ReadInt16();
      symbol.StructIrregularVarX = Reader.ReadByte();
      symbol.StructIrregularVarY = Reader.ReadByte();
      symbol.StructIrregularMinDist = Reader.ReadInt16();
      Reader.ReadInt16(); // reserved

      Reader.ReadInt16();
    }

    public override void WriteElementHeader(Element element)
    {
      WriteElementSymbol(element.Symbol);
      Writer.Write((byte)element.Type);
      Writer.Write((byte)0);

      Writer.Write((short)(element.Angle * 1800 / Math.PI)); // 0.1 Degrees

      Writer.Write((int)element.Color);
      Writer.Write((short)element.LineWidth);
      Writer.Write((short)element.Flags);

      Writer.Write((int)element.ServerObjectId);
      Writer.Write((int)(element.Height * 256 * 1000));
      Writer.Write((double)element.CreationDate.ToOADate());
      Writer.Write((int)element.MultirepresenationId);
      Writer.Write((double)element.ModificationDate.ToOADate());

      Writer.Write((int)element.PointCount()); //nItem
      Writer.Write((short)(Ocad9Io.TextCountV9(element.Text) / 8)); // nText

      Writer.Write((short)(Ocad9Io.TextCountV9(element.ObjectString) / 8)); // nObjectString
      Writer.Write((short)0); // nDatabaseString
      Writer.Write((byte)element.ObjectStringType); // ObjectStringType
      Writer.Write((byte)0); // reserved
    }

    public override void WriteElementContent(Element element)
    {
      Ocad9Io.WriteElementContent(this, element);
      WriteUnicodeString(Writer, element.ObjectString);
    }
    public override int CalcElementLength(Element element)
    {
      int length = 56 + 8 * element.PointCount() + GetElementTextCount(element, element.Text) + GetElementTextCount(element, element.ObjectString);
      return length;
    }

    private IList<Element> _courseSettingElements;
    private IList<Element> CourseSettingElements => _courseSettingElements ?? (_courseSettingElements = GetCourseSettingElements());
    private IList<Element> GetCourseSettingElements()
    {
      List<Element> csElements = new List<Element>();
      IEnumerator<Element> elems = new ElementEnumerator(this, null, false, null);
      while (elems.MoveNext())
      {
        Element elem = elems.Current;
        if (elem.ObjectStringType == ObjectStringType.None)
        { continue; }

        csElements.Add(elem);
      }
      return csElements;
    }


    public override Element ReadControlGeometry(Control control, IList<StringParamIndex> settingIndexList)
    {
      Element match = null;
      string search = control.Name;
      while (search.Length < 3) search = $"0{search}";

      foreach (var e in CourseSettingElements)
      {
        if (e.ObjectStringType != ObjectStringType.CsObject)
        { continue; }

        string key = new ControlPar(e.ObjectString).Name;

        if (key == $"1{search}"         // Control
         || key == $"0{search}"         // Start
         || key == $"2{search}"         // Must
         || key == $"3{search}")        // Finish
        {
          match = e;
          break;
        }
      }

      if (match == null)
      { }

      if (control.Code == ControlCode.TextBlock)
      {
        foreach (var idx in settingIndexList)
        {
          if (idx.Type == StringType.TextBlock)
          {
            TextBlockPar par = new TextBlockPar(ReadStringParam(idx));
            if (par.Code == control.Name)
            {
              control.Text = par.TextDesc;
              break;
            }
          }
        }
      }

      control.Element = match;
      return match;
    }
  }
}
