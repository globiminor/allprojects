using System;
using System.Collections.Generic;
using System.IO;

namespace Ocad
{
  public class Ocad8Reader : OcadReader
  {
    internal Ocad8Reader()
    { }
    public Ocad8Reader(Stream ocadStream)
      : base(ocadStream)
    {
      Version(BaseReader, out int iSectionMark, out int iVersion);
      if (iVersion != 6 && iVersion != 7 && iVersion != 8)
      { throw (new Exception(string.Format("Invalid Version {0}", iVersion))); }

      Init(iSectionMark, iVersion);
    }

    protected sealed override FileParam Init(int sectionMark, int version)
    {
      _fileParam = new FileParamV8
      {
        SectionMark = sectionMark,
        Version = version,
        SubVersion = BaseReader.ReadInt16(),
        FirstSymbolBlock = BaseReader.ReadInt32(),
        FirstIndexBlock = BaseReader.ReadInt32(),
        SetupPosition = BaseReader.ReadInt32(),
        SetupSize = BaseReader.ReadInt32(),
        InfoPosition = BaseReader.ReadInt32(),
        InfoSize = BaseReader.ReadInt32(),
        FirstStrIdxBlock = BaseReader.ReadInt32()
      };
      BaseReader.ReadInt32(); // reserved 2
      BaseReader.ReadInt32(); // reserved 3
      BaseReader.ReadInt32(); // reserved 4

      _symbol = ReadSymbolData();

      return _fileParam;
    }

    public override Setup ReadSetup()
    {
      Setup pSetup = new Setup();
      BaseReader.BaseStream.Seek(_fileParam.SetupPosition, SeekOrigin.Begin);

      pSetup.Offset.X = Coord.GetGeomPart(BaseReader.ReadInt32());
      pSetup.Offset.Y = Coord.GetGeomPart(BaseReader.ReadInt32());

      pSetup.GridDistance = BaseReader.ReadDouble();

      pSetup.WorkMode = (WorkMode)BaseReader.ReadInt16();
      pSetup.LineMode = (LineMode)BaseReader.ReadInt16();
      pSetup.EditMode = (EditMode)BaseReader.ReadInt16();
      pSetup.Symbol = BaseReader.ReadInt16();
      pSetup.Scale = BaseReader.ReadDouble();

      pSetup.PrjTrans.X = BaseReader.ReadDouble();
      pSetup.PrjTrans.Y = BaseReader.ReadDouble();
      pSetup.PrjRotation = BaseReader.ReadDouble();
      pSetup.PrjGrid = BaseReader.ReadDouble();

      if (pSetup.PrjTrans.X == 0 && pSetup.PrjTrans.Y == 0)
      {
        //try to find real world co-ordinates from template
        BaseReader.ReadDouble(); // GpsAngle
        for (int iAdjust = 0; iAdjust < 12; iAdjust++)
        { ReadGpsAdjust(); }  // GpsAdjust points
        BaseReader.ReadInt32();  // nGpsAdjust
        BaseReader.ReadDouble(); // DraftScaleX
        BaseReader.ReadDouble(); // DraftScaleY
        ReadCoord();          // TempOffset

        // now we are at template
        string tmplName = BaseReader.ReadChars(255).ToString();
        if (tmplName != "System.Char[]")
        {
          OcadReader pReader = Open(tmplName);
          Setup ptmplSetup = pReader.ReadSetup();

          pSetup.Scale = ptmplSetup.Scale;
          pSetup.PrjTrans.X = ptmplSetup.PrjTrans.X;
          pSetup.PrjTrans.Y = ptmplSetup.PrjTrans.Y;
          pSetup.PrjRotation = ptmplSetup.PrjRotation;
        }
      }
      return pSetup;
    }

    public override IEnumerable<ColorInfo> ReadColorInfos()
    {
      for (int i = 0; i < _symbol.ColorCount; i++)
      {
        yield return ReadColorInfo(i);
      }
    }
    private ColorInfo ReadColorInfo(int index)
    {
      if (index >= _symbol.ColorCount)
      {
        return null;
      }

      ColorInfo color = new ColorInfo();

      BaseReader.BaseStream.Seek(FileParam.HEADER_LENGTH + FileParam.SYM_BLOCK_LENGTH +
        index * FileParam.COLORINFO_LENGTH, SeekOrigin.Begin);

      color.Nummer = BaseReader.ReadInt16();
      color.Resolution = BaseReader.ReadInt16();

      color.Color.Cyan = BaseReader.ReadByte();
      color.Color.Magenta = BaseReader.ReadByte();
      color.Color.Yellow = BaseReader.ReadByte();
      color.Color.Black = BaseReader.ReadByte();

      color.Name = BaseReader.ReadChars(32).ToString();
      color.SepPercentage = BaseReader.ReadChars(32).ToString();
      return color;
    }

    public override ElementIndex ReadIndex(int i)
    {
      ElementIndex index = ReadIndexPart(i);

      if (index == null)
      { return null; }

      index.Position = BaseReader.ReadInt32();
      index.ReadElementLength(this);
      index.Symbol = BaseReader.ReadInt16();

      return index;
    }

    public override int ReadElementLength()
    {
      return BaseReader.ReadInt16();
    }

    public override Element ReadElement()
    {
      int nPoint;
      int nText;
      ElementV8 elem = new ElementV8(false)
      {
        Symbol = BaseReader.ReadInt16(),
        Type = (GeomType)BaseReader.ReadByte(),
        UnicodeText = BaseReader.ReadByte() != 0
      };
      nPoint = BaseReader.ReadInt16();
      nText = BaseReader.ReadInt16();
      elem.Angle = BaseReader.ReadInt16() * Math.PI / 1800.0;
      BaseReader.ReadInt16(); // reserved
      elem.ReservedHeight = BaseReader.ReadInt32();
      BaseReader.ReadChars(16); // reserve ID

      elem.Geometry = ReadGeometry(elem.Type, nPoint);
      if (nText > 0)
      {
        if (elem.UnicodeText)
        { elem.Text = BaseReader.ReadUnicodeString(); }
        else
        { elem.Text = BaseReader.ReadChars(nText).ToString(); }
      }

      return elem;
    }

    public override Symbol.BaseSymbol ReadSymbol()
    {
      throw new NotImplementedException();
    }

    public override void WriteElementHeader(EndianWriter writer, Element element)
    {
      WriteElementSymbol(writer, element.Symbol);
      writer.Write((byte)element.Type);
      writer.Write((byte)(element.UnicodeText ? 1 : 0));
      writer.Write((short)element.PointCount());
      writer.Write((short)element.TextCount());
      writer.Write((short)(element.Angle * 180 / Math.PI)); // 1 Degrees
      writer.Write((short)0);
      if (element is ElementV8 elem8)
      { writer.Write((int)elem8.ReservedHeight); }
      else
      { writer.Write((int)0); }
      for (int i = 0; i < 16; i++)
      { writer.Write(' '); }
    }

    public override void WriteElementContent(EndianWriter writer, Element element)
    {
      OcadWriter.Write(writer, element.Geometry);

      if (element.Text != "")
      {
        if (element.UnicodeText)
        { writer.WriteUnicodeString(element.Text); }
        else
        {
          writer.Write(element.Text);
          writer.Write((char)0);
        }
      }

    }

    public override void WriteElementSymbol(EndianWriter writer, int symbol)
    {
      writer.Write((short)symbol);
    }

    public override int CalcElementLength(Element element)
    {
      int length = element.PointCount() + element.TextCount();
      return length;
    }
  }
}
