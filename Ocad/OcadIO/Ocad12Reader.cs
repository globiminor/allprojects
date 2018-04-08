using System;
using System.Collections.Generic;
using System.IO;
using Ocad.StringParams;

namespace Ocad
{
  public class Ocad12Reader : Ocad11Reader
  {
    public override Element ReadElement()
    {
      ElementV9 elem = new ElementV9(false) { Symbol = BaseReader.ReadInt32() };
      if (elem.Symbol < -4 || elem.Symbol == 0)
      { return null; }

      elem.Type = (GeomType)BaseReader.ReadByte();
      if (elem.Type == 0)
      { return null; }

      BaseReader.ReadByte(); // reserved

      elem.Angle = BaseReader.ReadInt16() * Math.PI / 1800.0; // 0.1 Degrees

      elem.Color = BaseReader.ReadInt32();
      elem.LineWidth = BaseReader.ReadInt16();
      elem.Flags = BaseReader.ReadInt16();
      elem.ServerObjectId = BaseReader.ReadInt32();
      double height256MM = BaseReader.ReadInt32();
      elem.Height = height256MM / (256.0 * 1000.0);
      elem.CreationDate = DateTime.FromOADate(BaseReader.ReadDouble());
      elem.MultirepresenationId = BaseReader.ReadInt32();
      elem.ModificationDate = DateTime.FromOADate(BaseReader.ReadDouble());

      int nPoint = BaseReader.ReadInt32();
      int nText = BaseReader.ReadInt16();
      int nObjectString = BaseReader.ReadInt16();
      int nDatabaseString = BaseReader.ReadInt16();
      byte objectStringType = BaseReader.ReadByte();

      elem.ObjectStringType = (ObjectStringType)objectStringType;

      BaseReader.ReadByte();

      elem.Geometry = ReadGeometry(elem.Type, nPoint);
      if (nText > 0)
      { elem.Text = BaseReader.ReadUnicodeString(); }
      if (nObjectString > 0)
      {
        byte[] objectText = BaseReader.ReadBytes(nObjectString);
        System.Text.UnicodeEncoding unicode = new System.Text.UnicodeEncoding();
        elem.ObjectString = unicode.GetString(objectText);
      }

      return elem;
    }

    protected override FileParam Init(int sectionMark, int version)
    {
      FileParamV12 fileParam = new FileParamV12
      {
        SectionMark = sectionMark,
        Version = version,
        SubVersion = BaseReader.ReadInt16(),
        FirstSymbolBlock = BaseReader.ReadInt32(),
        FirstIndexBlock = BaseReader.ReadInt32(),
        OfflineSyncSerial = BaseReader.ReadInt32(),
        CurrentFileVersion = BaseReader.ReadInt32()
      };
      BaseReader.ReadInt32(); // reserved
      BaseReader.ReadInt32(); // reserved
      fileParam.FirstStrIdxBlock = BaseReader.ReadInt32();
      fileParam.FileNamePosition = BaseReader.ReadInt32();
      fileParam.FileNameSize = BaseReader.ReadInt32();
      BaseReader.ReadInt32(); // reserved
      BaseReader.ReadInt32(); // reserved
      BaseReader.ReadInt32(); // reserved
      fileParam.MrStartBlockPosition = BaseReader.ReadInt32();

      _fileParam = fileParam;
      _symbol = ReadSymbolData();

      return fileParam;
    }

    protected override void ReadAreaSymbol(Symbol.AreaSymbol symbol)
    {
      symbol.BorderSym = BaseReader.ReadInt32();
      symbol.FillColor = BaseReader.ReadInt16();
      symbol.HatchMode = (Symbol.AreaSymbol.EHatchMode)BaseReader.ReadInt16();
      symbol.HatchColor = BaseReader.ReadInt16();
      symbol.HatchLineWidth = BaseReader.ReadInt16();
      symbol.HatchLineDist = BaseReader.ReadInt16();
      symbol.HatchAngle1 = BaseReader.ReadInt16();
      symbol.HatchAngle2 = BaseReader.ReadInt16();
      symbol.FillOn = Convert.ToBoolean(BaseReader.ReadByte());
      symbol.BorderOn = Convert.ToBoolean(BaseReader.ReadByte());
      symbol.StructMode = (Symbol.AreaSymbol.EStructMode)BaseReader.ReadByte();
      symbol.StructDraw = (Symbol.AreaSymbol.EStructDraw)BaseReader.ReadByte();
      symbol.StructWidth = BaseReader.ReadInt16();
      symbol.StructHeight = BaseReader.ReadInt16();
      symbol.StructAngle = BaseReader.ReadInt16();
      symbol.StructIrregularVarX = BaseReader.ReadByte();
      symbol.StructIrregularVarY = BaseReader.ReadByte();
      symbol.StructIrregularMinDist = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // reserved

      BaseReader.ReadInt16();
    }

    public override void WriteElementHeader(EndianWriter writer, Element element)
    {
      ElementV9 elem9 = element as ElementV9;

      WriteElementSymbol(writer, element.Symbol);
      writer.Write((byte)element.Type);
      writer.Write((byte)0);

      writer.Write((short)(element.Angle * 1800 / Math.PI)); // 0.1 Degrees

      if (elem9 != null)
      {
        writer.Write((int)elem9.Color);
        writer.Write((short)elem9.LineWidth);
        writer.Write((short)elem9.Flags);
      }
      else
      {
        writer.Write((int)0);
        writer.Write((short)0);
        writer.Write((short)0);
      }

      writer.Write((int)element.ServerObjectId);
      writer.Write((int)(element.Height * 256 * 1000));
      writer.Write((double)element.CreationDate.ToOADate());
      writer.Write((int)element.MultirepresenationId);
      writer.Write((double)element.ModificationDate.ToOADate());

      writer.Write((int)element.PointCount()); //nItem
      int nText = 0;
      if (element.Text.Length > 0)
      { nText = ElementV9.TextCount(element.Text) / 8; }
      writer.Write((short)nText); // nText

      writer.Write(element.ObjectStringCount()); // nObjectString
      writer.Write((short)0); // nDatabaseString
      writer.Write((byte)elem9.ObjectStringType); // ObjectStringType
      writer.Write((byte)0); // reserved
    }

    public override void WriteElementContent(EndianWriter writer, Element element)
    {
      base.WriteElementContent(writer, element);

      if (element.ObjectString?.Length > 0)
      {
        System.Text.UnicodeEncoding unicode = new System.Text.UnicodeEncoding();
        byte[] bytes = unicode.GetBytes(element.ObjectString);
        writer.Write(bytes);
      }
    }
    public override int CalcElementLength(Element element)
    {
      int length = 56 + 8 * element.PointCount() + element.TextCount() + element.ObjectStringCount();
      return length;
    }

    private IList<Element> _controlElements;
    public override Element ReadControlGeometry(Control control, IList<StringParamIndex> settingIndexList)
    {
      if (_controlElements == null)
      {
        List<Element> controlElements = new List<Element>();
        foreach (Element elem in Elements(false, null))
        {
          if (elem.ObjectStringType == ObjectStringType.None)
          { continue; }

          controlElements.Add(elem);
        }
        _controlElements = controlElements;
      }

      Element match = null;
      foreach (Element e in _controlElements)
      {
        if (e.ObjectStringType != ObjectStringType.CsObject)
        { continue; }

        string key = new ControlPar(e.ObjectString).Name;

        if (key == $"10{control.Name}"         // Control
         || key == $"00{control.Name}"        // Start
         || key == $"30{control.Name}")        // Finish
        {
          match = e;
          break;
        }
      }

      control.Element = match;
      return match;
    }
  }
}
