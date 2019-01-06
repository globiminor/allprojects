using Ocad.StringParams;
using Ocad.Symbol;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ocad
{
  internal class Ocad8Io : OcadIo
  {
    public Ocad8Io(Stream ocadStream)
      : base(ocadStream)
    { }

    public override void ReadAreaSymbol(AreaSymbol symbol) => Ocad9Io.ReadAreaSymbol(this, symbol);

    public override void Write(ElementIndex index)
    {
      WriteIndexBox(index);

      Writer.Write((int)index.Position);
      Writer.Write((short)index.Length);
      Writer.Write((short)index.Symbol);
    }


    internal override IEnumerable<StringParamIndex> EnumStringParamIndices()
    {
      if (FileParam.SectionMark != 3)
      {
        yield break;
      }
      foreach (var paramIndex in base.EnumStringParamIndices())
      {
        yield return paramIndex;
      }
    }

    protected override void VerifyExists(Element element, ElementIndex index)
    {
      if (index.Symbol == 0)
      {
        element.DeleteSymbol = element.Symbol;
        element.Symbol = 0;
      }
    }

    protected sealed override FileParam Init(int sectionMark, int version)
    {
      FileParamV8 fileParam = new FileParamV8();

      fileParam.SectionMark = sectionMark;
      fileParam.Version = version;
      fileParam.SubVersion = Reader.ReadInt16();
      fileParam.FirstSymbolBlock = Reader.ReadInt32();
      fileParam.FirstIndexBlock = Reader.ReadInt32();
      fileParam.SetupPosition = Reader.ReadInt32();
      fileParam.SetupSize = Reader.ReadInt32();
      fileParam.InfoPosition = Reader.ReadInt32();
      fileParam.InfoSize = Reader.ReadInt32();
      fileParam.FirstStrIdxBlock = Reader.ReadInt32();

      Reader.ReadInt32(); // reserved 2
      Reader.ReadInt32(); // reserved 3
      Reader.ReadInt32(); // reserved 4

      return fileParam;
    }

    public override Setup ReadSetup()
    {
      Setup setup = new Setup();
      Reader.BaseStream.Seek(FileParam.SetupPosition, SeekOrigin.Begin);

      setup.Offset.X = Coord.GetGeomPart(Reader.ReadInt32());
      setup.Offset.Y = Coord.GetGeomPart(Reader.ReadInt32());

      setup.GridDistance = Reader.ReadDouble();

      setup.WorkMode = (WorkMode)Reader.ReadInt16();
      setup.LineMode = (LineMode)Reader.ReadInt16();
      setup.EditMode = (EditMode)Reader.ReadInt16();
      setup.Symbol = Reader.ReadInt16();
      setup.Scale = Reader.ReadDouble();

      setup.PrjTrans.X = Reader.ReadDouble();
      setup.PrjTrans.Y = Reader.ReadDouble();
      setup.PrjRotation = Reader.ReadDouble();
      setup.PrjGrid = Reader.ReadDouble();

      if (setup.PrjTrans.X == 0 && setup.PrjTrans.Y == 0)
      {
        //try to find real world co-ordinates from template
        Reader.ReadDouble(); // GpsAngle
        for (int iAdjust = 0; iAdjust < 12; iAdjust++)
        { ReadGpsAdjust(); }  // GpsAdjust points
        Reader.ReadInt32();  // nGpsAdjust
        Reader.ReadDouble(); // DraftScaleX
        Reader.ReadDouble(); // DraftScaleY
        ReadCoord();          // TempOffset

        // now we are at template
        string tmplName = Reader.ReadChars(255).ToString();
        if (tmplName != "System.Char[]")
        {
          OcadReader pReader = OcadReader.Open(tmplName);
          Setup ptmplSetup = pReader.ReadSetup();

          setup.Scale = ptmplSetup.Scale;
          setup.PrjTrans.X = ptmplSetup.PrjTrans.X;
          setup.PrjTrans.Y = ptmplSetup.PrjTrans.Y;
          setup.PrjRotation = ptmplSetup.PrjRotation;
        }
      }
      return setup;
    }

    public override IEnumerable<ColorInfo> ReadColorInfos()
    {
      for (int i = 0; i < SymbolData.ColorCount; i++)
      {
        yield return ReadColorInfo(i);
      }
    }
    private ColorInfo ReadColorInfo(int index)
    {
      if (index >= SymbolData.ColorCount)
      {
        return null;
      }

      ColorInfo color = new ColorInfo();

      Reader.BaseStream.Seek(FileParam.HEADER_LENGTH + FileParam.SYM_BLOCK_LENGTH +
        index * FileParam.COLORINFO_LENGTH, SeekOrigin.Begin);

      color.Nummer = Reader.ReadInt16();
      color.Resolution = Reader.ReadInt16();

      color.Color.Cyan = Reader.ReadByte();
      color.Color.Magenta = Reader.ReadByte();
      color.Color.Yellow = Reader.ReadByte();
      color.Color.Black = Reader.ReadByte();

      color.Name = Reader.ReadChars(32).ToString();
      color.SepPercentage = Reader.ReadChars(32).ToString();
      return color;
    }

    public override ElementIndex ReadIndex(int i)
    {
      ElementIndex index = ReadIndexPart(i);

      if (index == null)
      { return null; }

      index.Position = Reader.ReadInt32();
      index.ReadElementLength(this);
      index.Symbol = Reader.ReadInt16();

      return index;
    }

    public override int ReadElementLength()
    {
      return Reader.ReadInt16();
    }

    public override Element ReadElement()
    {
      int nPoint;
      int nText;
      Element elem = new Element(false);
      elem.Symbol = Reader.ReadInt16();
      elem.Type = (GeomType)Reader.ReadByte();
      elem.UnicodeText = Reader.ReadByte() != 0;
      nPoint = Reader.ReadInt16();
      nText = Reader.ReadInt16();
      elem.Angle = Reader.ReadInt16() * Math.PI / 1800.0;
      Reader.ReadInt16(); // reserved
      elem.ReservedHeight = Reader.ReadInt32();
      Reader.ReadChars(16); // reserve ID

      List<Coord> coords = ReadCoords(nPoint);
      elem.Geometry = Coord.GetGeometry(elem.Type, coords);
      if (nText > 0)
      {
        if (elem.UnicodeText)
        { elem.Text = Reader.ReadUnicodeString(); }
        else
        { elem.Text = Reader.ReadChars(nText).ToString(); }
      }

      return elem;
    }

    public override BaseSymbol ReadSymbol()
    {
      throw new NotImplementedException();
    }

    public override string ReadDescription()
    {
      return string.Empty;
    }

    public override ElementIndex GetIndex(Element element)
    {
      ElementIndex index = new ElementIndex(element.Geometry.Extent);
      index.Symbol = element.Symbol;
      index.CalcElementLength(this, element);

      index.ObjectType = (short)element.Type;
      index.Status = 1;
      index.Color = -1;

      return index;
    }

    public override void WriteElementHeader(Element element)
    {
      WriteElementSymbol(element.Symbol);
      Writer.Write((byte)element.Type);
      Writer.Write((byte)(element.UnicodeText ? 1 : 0));
      Writer.Write((short)element.PointCount());
      Writer.Write((short)GetElementTextCount(element, element.Text));
      Writer.Write((short)(element.Angle * 180 / Math.PI)); // 1 Degrees
      Writer.Write((short)0);

      { Writer.Write((int)element.ReservedHeight); }

      for (int i = 0; i < 16; i++)
      { Writer.Write(' '); }
    }

    public override void WriteElementContent(Element element)
    {
      OcadWriter.Write(Writer, element.Geometry);

      if (element.Text != "")
      {
        if (element.UnicodeText)
        { Writer.WriteUnicodeString(element.Text); }
        else
        {
          Writer.Write(element.Text);
          Writer.Write((char)0);
        }
      }

    }

    public override void WriteElementSymbol(int symbol)
    {
      Writer.Write((short)symbol);
    }

    public override int CalcElementLength(Element element)
    {
      int length = element.PointCount() + GetElementTextCount(element, element.Text);
      return length;
    }

    public override int GetElementTextCount(Element element, string text)
    {
      if (text == null || text.Length == 0)
      { return 0; }

      if (element.UnicodeText)
      { return text.Length / 4 + 1; }
      else
      { return text.Length / 8 + 1; }
    }

  }
}
