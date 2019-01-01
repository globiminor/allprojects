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
      ElementV9 elem = new ElementV9(false);

      elem.Symbol = BaseReader.ReadInt32();
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

      List<Coord> coords = ReadCoords(nPoint);
      elem.Geometry = GetGeometry(elem.Type, coords);
      if (nText > 0)
      { elem.Text = BaseReader.ReadUnicodeString(); }
      if (nObjectString > 0)
      {
        if (nText > 0)
        {
          int n = nText * 8;
          for (int i = elem.Text.Length * 2 + 2; i < n; i++)
          { BaseReader.BaseStream.ReadByte(); }
        }

        elem.ObjectString = BaseReader.ReadUnicodeString();
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
      writer.Write((short)(ElementV9.TextCountV9(element.Text) / 8)); // nText

      writer.Write((short)(ElementV9.TextCountV9(element.ObjectString) / 8)); // nObjectString
      writer.Write((short)0); // nDatabaseString
      writer.Write((byte)elem9.ObjectStringType); // ObjectStringType
      writer.Write((byte)0); // reserved
    }

    public override void WriteElementContent(EndianWriter writer, Element element)
    {
      base.WriteElementContent(writer, element);
      WriteUnicodeString(writer, element.ObjectString);
    }
    public override int CalcElementLength(Element element)
    {
      int length = 56 + 8 * element.PointCount() + element.TextCount(element.Text) + element.TextCount(element.ObjectString);
      return length;
    }

    private IList<Element> _courseSettingElements;
    private IList<Element> CourseSettingElements => _courseSettingElements ?? (_courseSettingElements = GetCourseSettingElements());
    private IList<Element> GetCourseSettingElements()
    {
      List<Element> csElements = new List<Element>();
      foreach (var elem in Elements(false, null))
      {
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
