

using Basics.Geom;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ocad
{
  public class Ocad9Reader : OcadReader
  {
    internal Ocad9Reader()
    { }
    private void Init()
    {
      Version(BaseReader, out int iSectionMark, out int iVersion);
      if (iVersion != 9 && iVersion != 10)
      { throw (new Exception(string.Format("Invalid Version {0}", iVersion))); }

      Init(iSectionMark, iVersion);
    }
    public Ocad9Reader(Stream ocadStream, bool incomplete)
      : base(ocadStream)
    {
      if (incomplete)
      { return; }
      Init();
    }
    public Ocad9Reader(Stream ocadStream)
      : base(ocadStream, Encoding.UTF7)
    {
      Init();
    }

    protected override FileParam Init(int sectionMark, int version)
    {
      _fileParam = new FileParamV9
      {
        SectionMark = sectionMark,
        Version = version,
        SubVersion = BaseReader.ReadInt16(),
        FirstSymbolBlock = BaseReader.ReadInt32(),
        FirstIndexBlock = BaseReader.ReadInt32()
      };
      BaseReader.ReadInt32(); // reserved
      BaseReader.ReadInt32(); // reserved
      BaseReader.ReadInt32(); // reserved
      BaseReader.ReadInt32(); // reserved
      _fileParam.FirstStrIdxBlock = BaseReader.ReadInt32();
      _fileParam.FileNamePosition = BaseReader.ReadInt32();
      _fileParam.FileNameSize = BaseReader.ReadInt32();
      BaseReader.ReadInt32(); // reserved 4

      _symbol = ReadSymbolData();

      return _fileParam;
    }

    public override IEnumerable<ColorInfo> ReadColorInfos()
    {
      IList<StringParamIndex> strIdxs = ReadStringParamIndices();
      foreach (StringParamIndex strIdx in strIdxs)
      {
        if (strIdx.Type == StringType.Color)
        {
          string stringParam = ReadStringParam(strIdx);

          ColorPar colorPar = new ColorPar(stringParam);
          ColorInfo color = new ColorInfo(colorPar);
          yield return color;
        }
      }
    }

    public override Setup ReadSetup()
    {
      Setup setup = new Setup();
      IList<StringParamIndex> pIndexList = ReadStringParamIndices();

      using (new InvariantCulture())
      {
        foreach (StringParamIndex pIndex in pIndexList)
        {
          if (pIndex.Type == StringType.ScalePar)
          {
            BaseReader.BaseStream.Seek(pIndex.FilePosition, SeekOrigin.Begin);
            ReadTab(255, out char e1);

            char e0 = e1;
            string name = ReadTab(255, out e1);
            while (name != "" && BaseReader.BaseStream.Position < pIndex.FilePosition + pIndex.Size)
            {
              if (e0 != '\t')
              { }
              else if (name[0] == 'a')
              {
                StringBuilder n = new StringBuilder();
                int i = 1; while (i < name.Length && (char.IsDigit(name[i]) || name[i] == '.' || name[i] == '-'))
                { n.Append(name[i]); i++; }
                if (n.Length > 0)
                { setup.PrjRotation = Math.PI / 180.0 * Convert.ToDouble(n.ToString()); }
              }
              else if (name[0] == 'm')
              { setup.Scale = Convert.ToDouble(name.Substring(1)); }
              else if (name[0] == 'x')
              { setup.PrjTrans.X = Convert.ToDouble(name.Substring(1)); }
              else if (name[0] == 'y')
              { setup.PrjTrans.Y = Convert.ToDouble(name.Substring(1)); }

              e0 = e1;
              name = ReadTab(255, out e1);
            }
            break;
          }
        }
        if (setup.PrjTrans.X == 0 && setup.PrjTrans.Y == 0 && setup.PrjRotation == 0)
        {
          foreach (StringParamIndex pIndex in pIndexList)
          {
            if (pIndex.Type == StringType.Template)
            {
              string sTemp = ReadStringParam(pIndex);
              int tabIdx = sTemp.IndexOf('\t');
              if (tabIdx >= 0)
              { sTemp = sTemp.Substring(0, tabIdx); }
              if (Path.GetExtension(sTemp) == ".ocd")
              {
                if (File.Exists(sTemp) == false)
                {
                  string name = ((FileStream)BaseReader.BaseStream).Name;
                  sTemp = Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
                    sTemp;
                }
                if (File.Exists(sTemp))
                {
                  OcadReader baseMap = Open(sTemp);
                  setup = baseMap.ReadSetup();
                  baseMap.Close();
                  if (setup.PrjTrans.X != 0 || setup.PrjTrans.Y != 0 || setup.PrjRotation != 0)
                  { break; }
                }
              }
            }
          }
        }
      }
      return setup;
    }

    public override ElementIndex ReadIndex(int i)
    {
      ElementIndex index = ReadIndexPart(i);

      if (index == null)
      { return null; }

      index.Position = BaseReader.ReadInt32();
      index.ReadElementLength(this);
      index.Symbol = BaseReader.ReadInt32();

      index.ObjectType = BaseReader.ReadSByte();
      BaseReader.ReadByte(); // reserved
      index.Status = BaseReader.ReadSByte();
      index.ViewType = BaseReader.ReadSByte();
      index.Color = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // reserved
      index.ImportLayer = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // reserved

      return index;
    }

    public override int ReadElementLength()
    {
      return BaseReader.ReadInt32();
    }


    public override Element ReadElement()
    {
      int nPoint;
      int nText;
      ElementV9 elem = new ElementV9(false) { Symbol = BaseReader.ReadInt32() };
      if (elem.Symbol < -4 || elem.Symbol == 0)
      { return null; }

      elem.Type = (GeomType)BaseReader.ReadByte();
      if (elem.Type == 0)
      { return null; }

      BaseReader.ReadByte(); // reserved

      elem.Angle = BaseReader.ReadInt16() * Math.PI / 1800.0; // 0.1 Degrees

      nPoint = BaseReader.ReadInt32();
      nText = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // reserved

      elem.Color = BaseReader.ReadInt32();
      elem.LineWidth = BaseReader.ReadInt16();
      elem.Flags = BaseReader.ReadInt16();

      // OCAD 10
      BaseReader.ReadDouble(); // reserved
      byte mark = BaseReader.ReadByte();
      BaseReader.ReadByte(); // reserved
      BaseReader.ReadInt16(); // reserved;
      int heightMM = BaseReader.ReadInt32();
      elem.Height = heightMM / 1000.0;

      List<Coord> coords = ReadCoords(nPoint);
      elem.Geometry = GetGeometry(elem.Type, coords);
      if (nText > 0)
      { elem.Text = BaseReader.ReadUnicodeString(); }

      return elem;
    }

    public override Symbol.BaseSymbol ReadSymbol()
    {
      Symbol.BaseSymbol symbol = ReadSymbolRoot();

      ReadSymbolSpez(symbol);

      return symbol;
    }

    protected void ReadSymbolSpez(Symbol.BaseSymbol symbol)
    {
      if (symbol is Symbol.PointSymbol)
      { ReadPointSymbol(symbol as Symbol.PointSymbol); }
      else if (symbol is Symbol.LineSymbol)
      { ReadLineSymbol(symbol as Symbol.LineSymbol); }
      else if (symbol is Symbol.AreaSymbol)
      { ReadAreaSymbol(symbol as Symbol.AreaSymbol); }
      else if (symbol is Symbol.TextSymbol)
      { ReadTextSymbol(symbol as Symbol.TextSymbol); }
    }

    protected Symbol.BaseSymbol ReadSymbolRoot()
    {
      Symbol.BaseSymbol symbol;
      int iSize = BaseReader.ReadInt32();
      int iNumber = BaseReader.ReadInt32();
      Symbol.SymbolType eType = (Symbol.SymbolType)BaseReader.ReadByte();
      if (eType == Symbol.SymbolType.Point)
      {
        symbol = new Symbol.PointSymbol(iNumber);
      }
      else if (eType == Symbol.SymbolType.Text)
      {
        symbol = new Symbol.TextSymbol(iNumber);
      }
      else if (eType == Symbol.SymbolType.Line)
      {
        symbol = new Symbol.LineSymbol(iNumber);
      }
      else if (eType == Symbol.SymbolType.Area)
      {
        symbol = new Symbol.AreaSymbol(iNumber);
      }
      else
      {
        symbol = new Symbol.AnySymbol(iNumber);
      }
      byte bFlags = BaseReader.ReadByte();
      if ((bFlags & 1) == 1)
      { symbol.Rotatable = true; }
      if ((bFlags & 4) == 4)
      { symbol.IsFavorit = true; }

      symbol.Selected = BaseReader.ReadByte() > 0;
      symbol.Status = (Symbol.SymbolStatus)BaseReader.ReadByte();
      symbol.PreferredTool = (Symbol.SymbolTool)BaseReader.ReadByte();
      symbol.CourseSettingMode = (Symbol.SymbolCourseSetting)BaseReader.ReadByte();

      BaseReader.ReadByte(); // CsObjType
      BaseReader.ReadByte(); // CsCDFlags

      BaseReader.ReadInt32(); // Extent, can be derived from symbol props !
      int iFilePos = BaseReader.ReadInt32();
      symbol.Group = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // # Colors, can be derived from symbol props !
      for (int iColor = 0; iColor < 14; iColor++)
      { BaseReader.ReadInt16(); } // Color

      symbol.Description = ReadDescription(); // ReadString(32);
      symbol.Icon = BaseReader.ReadBytes(484);
      return symbol;
    }

    protected virtual string ReadDescription()
    {
      string desc = ReadString(32);
      return desc;
    }
    protected string ReadString(int length)
    {
      byte[] pByte = BaseReader.ReadBytes(length);

      StringBuilder sByte = new StringBuilder(pByte[0]);
      for (int iLetter = 1; iLetter <= pByte[0]; iLetter++)
      { sByte.Append((char)pByte[iLetter]); }
      return sByte.ToString();
    }


    private void ReadPointSymbol(Symbol.PointSymbol symbol)
    {
      int iData = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // Reserved

      while (iData > 0)
      {
        Symbol.SymbolGraphics pElem = ReadSymbolGraphics(out int iNData);
        symbol.Graphics.Add(pElem);
        iData -= iNData;
      }
    }

    private void ReadLineSymbol(Symbol.LineSymbol symbol)
    {
      symbol.LineColor = BaseReader.ReadInt16();
      symbol.LineWidth = BaseReader.ReadInt16();
      symbol.LineStyle = (Symbol.LineSymbol.ELineStyle)BaseReader.ReadInt16();
      symbol.DistFromStart = BaseReader.ReadInt16();
      symbol.DistToEnd = BaseReader.ReadInt16();
      symbol.MainLength = BaseReader.ReadInt16();
      symbol.EndLength = BaseReader.ReadInt16();
      symbol.MainGap = BaseReader.ReadInt16();
      symbol.SecondGap = BaseReader.ReadInt16();
      symbol.EndGap = BaseReader.ReadInt16();
      symbol.MinSym = BaseReader.ReadInt16();
      symbol.NPrimSym = BaseReader.ReadInt16();
      symbol.PrimSymDist = BaseReader.ReadInt16();
      symbol.FillColorOn = BaseReader.ReadChar();
      symbol.Flags = BaseReader.ReadChar();
      symbol.FillColor = BaseReader.ReadInt16();
      symbol.LeftColor = BaseReader.ReadInt16();
      symbol.RightColor = BaseReader.ReadInt16();
      symbol.FillWidth = BaseReader.ReadInt16();
      symbol.LeftWidth = BaseReader.ReadInt16();
      symbol.RightWidth = BaseReader.ReadInt16();
      symbol.Gap = BaseReader.ReadInt16();
      BaseReader.ReadInt16();
      BaseReader.ReadInt16();
      BaseReader.ReadInt16();
      symbol.Last = BaseReader.ReadInt16();
      BaseReader.ReadInt16();
      symbol.FrameColor = BaseReader.ReadInt16();
      symbol.FrameWidth = BaseReader.ReadInt16();
      symbol.FrameStyle = (Symbol.LineSymbol.ELineStyle)BaseReader.ReadInt16();
    }

    protected virtual void ReadAreaSymbol(Symbol.AreaSymbol symbol)
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
      symbol.StructMode = (Symbol.AreaSymbol.EStructMode)BaseReader.ReadInt16();
      symbol.StructWidth = BaseReader.ReadInt16();
      symbol.StructHeight = BaseReader.ReadInt16();

      BaseReader.ReadInt16();
    }

    private void ReadTextSymbol(Symbol.TextSymbol symbol)
    {
      symbol.FontName = ReadString(32);
      symbol.Color = BaseReader.ReadInt16();
      symbol.Size = BaseReader.ReadInt16();
      symbol.Weight = (Symbol.TextSymbol.WeightMode)BaseReader.ReadInt16();
      symbol.Italic = (BaseReader.ReadByte() != 0);
      BaseReader.ReadByte(); // reserved
      symbol.CharSpace = BaseReader.ReadInt16();
      symbol.WordSpace = BaseReader.ReadInt16();
      symbol.Alignment = (Symbol.TextSymbol.AlignmentMode)BaseReader.ReadInt16();
      symbol.LineSpace = BaseReader.ReadInt16();
      symbol.ParagraphSpace = BaseReader.ReadInt16();
      symbol.IndentFirst = BaseReader.ReadInt16();
      symbol.IndentOther = BaseReader.ReadInt16();
      int nTabs = BaseReader.ReadInt16();
      int[] iTabs = new int[nTabs];
      for (int iTab = 0; iTab < nTabs; iTab++)
      { iTabs[iTab] = BaseReader.ReadInt32(); }
      for (int iTab = nTabs; iTab < 32; iTab++)
      { BaseReader.ReadInt32(); }
      symbol.Tabs = iTabs;
      // line below
      bool bLineBelow = (BaseReader.ReadInt16() != 0);
      int iLbColor = BaseReader.ReadInt16();
      int iLbWidth = BaseReader.ReadInt16();
      int iLbDist = BaseReader.ReadInt16();
      if (bLineBelow)
      { symbol.LineBelow = new Symbol.TextSymbol.Underline(iLbColor, iLbWidth, iLbDist); }
      BaseReader.ReadInt16(); // reserved
      // Framing
      Symbol.TextSymbol.FramingMode eFrameMode = (Symbol.TextSymbol.FramingMode)BaseReader.ReadByte();
      BaseReader.ReadByte(); // reserved
      ReadString(24); // reserved
      int iFrLeft = BaseReader.ReadInt16();
      int iFrBottom = BaseReader.ReadInt16();
      int iFrRight = BaseReader.ReadInt16();
      int iFrTop = BaseReader.ReadInt16();
      int iFrColor = BaseReader.ReadInt16();
      int iFrWidth = BaseReader.ReadInt16();
      BaseReader.ReadInt16(); // reserved
      BaseReader.ReadInt16(); // reserved;
      int iFrOffX = BaseReader.ReadInt16();
      int iFrOffY = BaseReader.ReadInt16();
      if (eFrameMode == Symbol.TextSymbol.FramingMode.No)
      { }
      else if (eFrameMode == Symbol.TextSymbol.FramingMode.Line)
      { symbol.Frame = new Symbol.TextSymbol.LineFraming(iFrColor, iFrWidth); }
      else if (eFrameMode == Symbol.TextSymbol.FramingMode.Shadow)
      { symbol.Frame = new Symbol.TextSymbol.ShadowFraming(iFrColor, iFrOffX, iFrOffY); }
      else if (eFrameMode == Symbol.TextSymbol.FramingMode.Rectangle)
      {
        symbol.Frame = new Symbol.TextSymbol.RectFraming(iFrColor,
          iFrLeft, iFrRight, iFrBottom, iFrTop);
      }
      else
      { throw new ArgumentException("unhandled mode " + eFrameMode); }
    }

    private Symbol.SymbolGraphics ReadSymbolGraphics(out int nData)
    {
      Symbol.SymbolGraphics elem = new Symbol.SymbolGraphics
      { Type = (Symbol.SymbolGraphicsType)BaseReader.ReadInt16() };
      int flags = BaseReader.ReadInt16();
      elem.RoundEnds = ((flags & 1) == 1);
      elem.Color = BaseReader.ReadInt16();
      elem.LineWidth = BaseReader.ReadInt16();

      int diameter = BaseReader.ReadInt16();
      int nPoints = BaseReader.ReadInt16();

      BaseReader.ReadInt16(); // Reserved
      BaseReader.ReadInt16(); // Reserved

      nData = 2 + nPoints;

      List<Coord> coords = ReadCoords(nPoints);

      if (elem.Type == Symbol.SymbolGraphicsType.Line)
      { elem.Geometry = GetGeometry(GeomType.line, coords); }
      else if (elem.Type == Symbol.SymbolGraphicsType.Area)
      { elem.Geometry = GetGeometry(GeomType.area, coords); }
      else if (elem.Type == Symbol.SymbolGraphicsType.Circle ||
        elem.Type == Symbol.SymbolGraphicsType.Dot)
      {
        Point center = (Point)GetGeometry(GeomType.point, coords);
        Arc arc = new Arc(center, 0.5 * diameter, 0, 2 * Math.PI);
        Polyline circle = new Polyline();
        circle.Add(arc);
        if (elem.Type == Symbol.SymbolGraphicsType.Circle)
        { elem.Geometry = circle; }
        else if (elem.Type == Symbol.SymbolGraphicsType.Dot)
        { elem.Geometry = new Area(circle); }
        else
        { throw new NotImplementedException(elem.Type.ToString()); }
      }
      else
      { throw new NotImplementedException(elem.Type.ToString()); }

      return elem;
    }

    public override void WriteElementSymbol(EndianWriter writer, int symbol)
    {
      writer.Write((int)symbol);
    }
    public override void WriteElementHeader(EndianWriter writer, Element element)
    {
      WriteElementSymbol(writer, element.Symbol);
      writer.Write((byte)element.Type);
      writer.Write((byte)0);

      writer.Write((short)(element.Angle * 1800 / Math.PI)); // 0.1 Degrees

      writer.Write((int)element.PointCount());
      int nText = 0;
      if (element.Text.Length > 0)
      { nText = ElementV9.TextCount(element.Text) / 8; }
      writer.Write((short)nText);
      writer.Write((short)0); // reserved

      if (element is ElementV9 elem9)
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

      writer.Write((double)0); // reserved
      // OCAD 10
      writer.Write((byte)0); // mark
      writer.Write((byte)0); // reserved
      writer.Write((short)0); // reserved;
      int heightMm = (int)(element.Height * 1000);
      writer.Write((int)heightMm); // reserved;
    }

    public override void WriteElementContent(EndianWriter writer, Element element)
    {
      OcadWriter.Write(writer, element.Geometry);

      if (element.Text != "")
      {
        writer.WriteUnicodeString(element.Text);
        int n = ElementV9.TextCount(element.Text);
        for (int i = element.Text.Length * 2; i < n; i++)
        { writer.BaseStream.WriteByte(0); }
      }
    }

    public override int CalcElementLength(Element element)
    {
      int length = 40 + 8 * element.PointCount() + element.TextCount();
      return length;
    }
  }
}
