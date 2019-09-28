

using Basics.Geom;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ocad
{
  internal class Ocad9Io : OcadIo
  {
    public Ocad9Io(Stream ocadStream)
      : base(ocadStream, Encoding.UTF7)
    { }

    protected override FileParam Init(int sectionMark, int version) => Init(this, sectionMark, version);
    public static FileParam Init(OcadIo io, int sectionMark, int version)
    {
      FileParam fileParam = new FileParamV9();
      fileParam.SectionMark = sectionMark;
      fileParam.Version = version;
      fileParam.SubVersion = io.Reader.ReadInt16();
      fileParam.FirstSymbolBlock = io.Reader.ReadInt32();
      fileParam.FirstIndexBlock = io.Reader.ReadInt32();
      io.Reader.ReadInt32(); // reserved
      io.Reader.ReadInt32(); // reserved
      io.Reader.ReadInt32(); // reserved
      io.Reader.ReadInt32(); // reserved
      fileParam.FirstStrIdxBlock = io.Reader.ReadInt32();
      fileParam.FileNamePosition = io.Reader.ReadInt32();
      fileParam.FileNameSize = io.Reader.ReadInt32();
      io.Reader.ReadInt32(); // reserved 4

      return fileParam;
    }

    public override IEnumerable<ColorInfo> ReadColorInfos() => ReadColorInfos(this);
    public static IEnumerable<ColorInfo> ReadColorInfos(OcadIo io)
    {
      IList<StringParamIndex> indexEnum = new List<StringParamIndex>(io.EnumStringParamIndices());
      foreach (var strIdx in indexEnum)
      {
        if (strIdx.Type == StringType.Color)
        {
          string stringParam = io.ReadStringParam(strIdx);

          ColorPar colorPar = new ColorPar(stringParam);
          ColorInfo color = new ColorInfo(colorPar);
          yield return color;
        }
      }
    }

    public override Setup ReadSetup() => ReadSetup(this);
    public static Setup ReadSetup(OcadIo io)
    {
      Setup setup = new Setup();
      List<StringParamIndex> indexList = new List<StringParamIndex>(io.EnumStringParamIndices());

      using (new InvariantCulture())
      {
        foreach (var index in indexList)
        {
          if (index.Type == StringType.ScalePar)
          {
            io.Reader.BaseStream.Seek(index.FilePosition, SeekOrigin.Begin);
            io.ReadTab(255, out char e1);

            char e0 = e1;
            string name = io.ReadTab(255, out e1);
            while (name != "" && io.Reader.BaseStream.Position < index.FilePosition + index.Size)
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
              name = io.ReadTab(255, out e1);
            }
            break;
          }
        }
        if (setup.PrjTrans.X == 0 && setup.PrjTrans.Y == 0 && setup.PrjRotation == 0)
        {
          foreach (var index in indexList)
          {
            if (index.Type == StringType.Template)
            {
              string sTemp = io.ReadStringParam(index);
              int tabIdx = sTemp.IndexOf('\t');
              if (tabIdx >= 0)
              { sTemp = sTemp.Substring(0, tabIdx); }
              if (Path.GetExtension(sTemp) == ".ocd")
              {
                if (File.Exists(sTemp) == false)
                {
                  string name = ((FileStream)io.Reader.BaseStream).Name;
                  sTemp = Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
                    sTemp;
                }
                if (File.Exists(sTemp))
                {
                  OcadReader baseMap = OcadReader.Open(sTemp);
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

    public override ElementIndex ReadIndex(int i) => ReadIndex(this, i);
    public static ElementIndex ReadIndex(OcadIo io, int i)
    {
      ElementIndex index = io.ReadIndexPart(i);

      if (index == null)
      { return null; }

      index.Position = io.Reader.ReadInt32();
      index.ReadElementLength(io);
      index.Symbol = io.Reader.ReadInt32();

      index.ObjectType = io.Reader.ReadSByte();
      io.Reader.ReadByte(); // reserved
      index.Status = io.Reader.ReadSByte();
      index.ViewType = io.Reader.ReadSByte();
      index.Color = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // reserved
      index.ImportLayer = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // reserved

      return index;
    }

    public override int ReadElementLength() => ReadElementLength(this);
    public static int ReadElementLength(OcadIo io)
    {
      return io.Reader.ReadInt32();
    }


    public override T ReadElement<T>(out T element) => ReadElement(this, out element);
    public static T ReadElement<T>(OcadIo io, out T element) where T : Element, new()
    {
      int nPoint;
      int nText;
      T elem = new T();

      elem.Symbol = io.Reader.ReadInt32();
      if (elem.Symbol < -4 || elem.Symbol == 0)
      {
        element = null;
        return element;
      }

      elem.Type = (GeomType)io.Reader.ReadByte();
      if (elem.Type == 0)
      {
        element = null;
        return element;
      }

      io.Reader.ReadByte(); // reserved

      elem.Angle = io.Reader.ReadInt16() * Math.PI / 1800.0; // 0.1 Degrees

      nPoint = io.Reader.ReadInt32();
      nText = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // reserved

      elem.Color = io.Reader.ReadInt32();
      elem.LineWidth = io.Reader.ReadInt16();
      elem.Flags = io.Reader.ReadInt16();

      // OCAD 10
      io.Reader.ReadDouble(); // reserved
      byte mark = io.Reader.ReadByte();
      io.Reader.ReadByte(); // reserved
      io.Reader.ReadInt16(); // reserved;
      int heightMM = io.Reader.ReadInt32();
      elem.Height = heightMM / 1000.0;

      List<Coord> coords = io.ReadCoords(nPoint);
      elem.InitGeometry(coords, elem.NeedsSetup ? io.Setup : null);
      if (nText > 0)
      { elem.Text = io.Reader.ReadUnicodeString(); }

      element = elem;
      return element;
    }

    public override Symbol.BaseSymbol ReadSymbol()
    {
      Symbol.BaseSymbol symbol = ReadSymbolRoot(this);

      ReadSymbolSpez(this, symbol);

      return symbol;
    }

    public static void ReadSymbolSpez(OcadIo io, Symbol.BaseSymbol symbol)
    {
      if (symbol is Symbol.PointSymbol pt)
      { ReadPointSymbol(io, pt); }
      else if (symbol is Symbol.LineSymbol li)
      { ReadLineSymbol(io, li); }
      else if (symbol is Symbol.AreaSymbol areaSym)
      { io.ReadAreaSymbol(areaSym); }
      else if (symbol is Symbol.TextSymbol)
      { ReadTextSymbol(io, symbol as Symbol.TextSymbol); }
    }

    public static Symbol.BaseSymbol ReadSymbolRoot(OcadIo io)
    {
      Symbol.BaseSymbol symbol;
      int iSize = io.Reader.ReadInt32();
      int iNumber = io.Reader.ReadInt32();
      Symbol.SymbolType eType = (Symbol.SymbolType)io.Reader.ReadByte();
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
      byte bFlags = io.Reader.ReadByte();
      if ((bFlags & 1) == 1)
      { symbol.Rotatable = true; }
      if ((bFlags & 4) == 4)
      { symbol.IsFavorit = true; }

      symbol.Selected = io.Reader.ReadByte() > 0;
      symbol.Status = (Symbol.SymbolStatus)io.Reader.ReadByte();
      symbol.PreferredTool = (Symbol.SymbolTool)io.Reader.ReadByte();
      symbol.CourseSettingMode = (Symbol.SymbolCourseSetting)io.Reader.ReadByte();

      io.Reader.ReadByte(); // CsObjType
      io.Reader.ReadByte(); // CsCDFlags

      io.Reader.ReadInt32(); // Extent, can be derived from symbol props !
      int iFilePos = io.Reader.ReadInt32();
      symbol.Group = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // # Colors, can be derived from symbol props !
      for (int iColor = 0; iColor < 14; iColor++)
      { io.Reader.ReadInt16(); } // Color

      symbol.Description = io.ReadDescription(); // ReadString(32);
      symbol.Icon = io.Reader.ReadBytes(484);
      return symbol;
    }

    public override string ReadDescription()
    {
      string desc = ReadString(this, 32);
      return desc;
    }
    public static string ReadString(OcadIo io, int length)
    {
      byte[] bytes = io.Reader.ReadBytes(length);

      StringBuilder sByte = new StringBuilder(bytes[0]);
      for (int iLetter = 1; iLetter <= bytes[0]; iLetter++)
      { sByte.Append((char)bytes[iLetter]); }
      return sByte.ToString();
    }


    private static void ReadPointSymbol(OcadIo io, Symbol.PointSymbol symbol)
    {
      int iData = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // Reserved

      while (iData > 0)
      {
        Symbol.SymbolGraphics pElem = ReadSymbolGraphics(io, out int iNData);
        symbol.Graphics.Add(pElem);
        iData -= iNData;
      }
    }

    private static void ReadLineSymbol(OcadIo io, Symbol.LineSymbol symbol)
    {
      symbol.LineColor = io.Reader.ReadInt16();
      symbol.LineWidth = io.Reader.ReadInt16();
      symbol.LineStyle = (Symbol.LineSymbol.ELineStyle)io.Reader.ReadInt16();
      symbol.DistFromStart = io.Reader.ReadInt16();
      symbol.DistToEnd = io.Reader.ReadInt16();
      symbol.MainLength = io.Reader.ReadInt16();
      symbol.EndLength = io.Reader.ReadInt16();
      symbol.MainGap = io.Reader.ReadInt16();
      symbol.SecondGap = io.Reader.ReadInt16();
      symbol.EndGap = io.Reader.ReadInt16();
      symbol.MinSym = io.Reader.ReadInt16();
      symbol.NPrimSym = io.Reader.ReadInt16();
      symbol.PrimSymDist = io.Reader.ReadInt16();
      symbol.FillColorOn = io.Reader.ReadInt16(); // <--
      symbol.Flags = io.Reader.ReadInt16(); // <--
      symbol.FillColor = io.Reader.ReadInt16();
      symbol.LeftColor = io.Reader.ReadInt16();
      symbol.RightColor = io.Reader.ReadInt16();
      symbol.FillWidth = io.Reader.ReadInt16();
      symbol.LeftWidth = io.Reader.ReadInt16();
      symbol.RightWidth = io.Reader.ReadInt16();
      symbol.LRDash = io.Reader.ReadInt16();
      symbol.LRGap = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // < --symbol.BorderBackColor; reserved
      io.Reader.ReadInt16(); // reserved
      io.Reader.ReadInt16(); // reserved
      symbol.DecreaseMode = (Symbol.LineSymbol.EDecreaseMode)io.Reader.ReadInt16();
      symbol.DecreaseLast = io.Reader.ReadInt16();
      symbol.DisableDecreaseDistance = io.Reader.ReadByte();
      symbol.DisableDecreaseWidth = io.Reader.ReadByte();
      symbol.FrameColor = io.Reader.ReadInt16();
      symbol.FrameWidth = io.Reader.ReadInt16();
      symbol.FrameStyle = (Symbol.LineSymbol.ELineStyle)io.Reader.ReadInt16();

      int primSize = io.Reader.ReadInt16();
      int secSize = io.Reader.ReadInt16();
      int cornerSize = io.Reader.ReadInt16();
      int startSize = io.Reader.ReadInt16();
      int endSize = io.Reader.ReadInt16();
      symbol.SymbolFlags = (Symbol.LineSymbol.ESymbolFlags)io.Reader.ReadByte();
      io.Reader.ReadByte();
      if (primSize > 0)
      { symbol.PrimSymbol = ReadSymbolGraphics(io, primSize); }
      if (secSize > 0)
      { symbol.SecSymbol = ReadSymbolGraphics(io, secSize); }
      if (cornerSize > 0)
      { symbol.CornerSymbol = ReadSymbolGraphics(io, cornerSize); }
      if (startSize > 0)
      { symbol.StartSymbol = ReadSymbolGraphics(io, startSize); }
      if (endSize > 0)
      { symbol.EndSymbol = ReadSymbolGraphics(io, endSize); }
    }

    public override void ReadAreaSymbol(Symbol.AreaSymbol symbol) => ReadAreaSymbol(this, symbol);
    public static void ReadAreaSymbol(OcadIo io, Symbol.AreaSymbol symbol)
    {
      symbol.BorderSym = io.Reader.ReadInt32();
      symbol.FillColor = io.Reader.ReadInt16();
      symbol.HatchMode = (Symbol.AreaSymbol.EHatchMode)io.Reader.ReadInt16();
      symbol.HatchColor = io.Reader.ReadInt16();
      symbol.HatchLineWidth = io.Reader.ReadInt16();
      symbol.HatchLineDist = io.Reader.ReadInt16();
      symbol.HatchAngle1 = io.Reader.ReadInt16();
      symbol.HatchAngle2 = io.Reader.ReadInt16();
      symbol.FillOn = Convert.ToBoolean(io.Reader.ReadByte());
      symbol.BorderOn = Convert.ToBoolean(io.Reader.ReadByte());
      symbol.StructMode = (Symbol.AreaSymbol.EStructMode)io.Reader.ReadInt16();
      symbol.StructWidth = io.Reader.ReadInt16();
      symbol.StructHeight = io.Reader.ReadInt16();

      io.Reader.ReadInt16();
    }

    private static void ReadTextSymbol(OcadIo io, Symbol.TextSymbol symbol)
    {
      symbol.FontName = ReadString(io, 32);
      symbol.Color = io.Reader.ReadInt16();
      symbol.Size = io.Reader.ReadInt16();
      symbol.Weight = (Symbol.TextSymbol.WeightMode)io.Reader.ReadInt16();
      symbol.Italic = (io.Reader.ReadByte() != 0);
      io.Reader.ReadByte(); // reserved
      symbol.CharSpace = io.Reader.ReadInt16();
      symbol.WordSpace = io.Reader.ReadInt16();
      symbol.Alignment = (Symbol.TextSymbol.AlignmentMode)io.Reader.ReadInt16();
      symbol.LineSpace = io.Reader.ReadInt16();
      symbol.ParagraphSpace = io.Reader.ReadInt16();
      symbol.IndentFirst = io.Reader.ReadInt16();
      symbol.IndentOther = io.Reader.ReadInt16();
      int nTabs = io.Reader.ReadInt16();
      int[] iTabs = new int[nTabs];
      for (int iTab = 0; iTab < nTabs; iTab++)
      { iTabs[iTab] = io.Reader.ReadInt32(); }
      for (int iTab = nTabs; iTab < 32; iTab++)
      { io.Reader.ReadInt32(); }
      symbol.Tabs = iTabs;
      // line below
      bool bLineBelow = (io.Reader.ReadInt16() != 0);
      int iLbColor = io.Reader.ReadInt16();
      int iLbWidth = io.Reader.ReadInt16();
      int iLbDist = io.Reader.ReadInt16();
      if (bLineBelow)
      { symbol.LineBelow = new Symbol.TextSymbol.Underline(iLbColor, iLbWidth, iLbDist); }
      io.Reader.ReadInt16(); // reserved
      // Framing
      Symbol.TextSymbol.FramingMode eFrameMode = (Symbol.TextSymbol.FramingMode)io.Reader.ReadByte();
      io.Reader.ReadByte(); // reserved
      ReadString(io, 24); // reserved
      int iFrLeft = io.Reader.ReadInt16();
      int iFrBottom = io.Reader.ReadInt16();
      int iFrRight = io.Reader.ReadInt16();
      int iFrTop = io.Reader.ReadInt16();
      int iFrColor = io.Reader.ReadInt16();
      int iFrWidth = io.Reader.ReadInt16();
      io.Reader.ReadInt16(); // reserved
      io.Reader.ReadInt16(); // reserved;
      int iFrOffX = io.Reader.ReadInt16();
      int iFrOffY = io.Reader.ReadInt16();
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

    internal static Symbol.SymbolGraphicsCollection ReadSymbolGraphics(OcadIo io, int size)
    {
      Symbol.SymbolGraphicsCollection graphics = null;
      int iData = size;
      while (iData > 0)
      {
        graphics = graphics ?? new Symbol.SymbolGraphicsCollection();
        Symbol.SymbolGraphics pElem = ReadSymbolGraphics(io, out int iNData);
        graphics.Add(pElem);
        iData -= iNData;
      }
      return graphics;
    }
    private static Symbol.SymbolGraphics ReadSymbolGraphics(OcadIo io, out int nData)
    {
      Symbol.SymbolGraphics elem = new Symbol.SymbolGraphics
      { Type = (Symbol.SymbolGraphicsType)io.Reader.ReadInt16() };
      int flags = io.Reader.ReadInt16();
      elem.RoundEnds = ((flags & 1) == 1);
      elem.Color = io.Reader.ReadInt16();
      elem.LineWidth = io.Reader.ReadInt16();

      int diameter = io.Reader.ReadInt16();
      int nPoints = io.Reader.ReadInt16();

      io.Reader.ReadInt16(); // Reserved
      io.Reader.ReadInt16(); // Reserved

      nData = 2 + nPoints;

      List<Coord> coords = io.ReadCoords(nPoints);

      if (elem.Type == Symbol.SymbolGraphicsType.Line)
      { elem.MapGeometry = Coord.GetGeometry(GeomType.line, coords); }
      else if (elem.Type == Symbol.SymbolGraphicsType.Area)
      { elem.MapGeometry = Coord.GetGeometry(GeomType.area, coords); }
      else if (elem.Type == Symbol.SymbolGraphicsType.Circle ||
        elem.Type == Symbol.SymbolGraphicsType.Dot)
      {
        IPoint center = ((GeoElement.Point)Coord.GetGeometry(GeomType.point, coords)).BaseGeometry;
        Arc arc = new Arc(center, 0.5 * diameter, 0, 2 * Math.PI);
        Polyline circle = new Polyline();
        circle.Add(arc);
        if (elem.Type == Symbol.SymbolGraphicsType.Circle)
        { elem.MapGeometry = new GeoElement.Line(circle); }
        else if (elem.Type == Symbol.SymbolGraphicsType.Dot)
        { elem.MapGeometry = new GeoElement.Area(new Area(circle)); }
        else
        { throw new NotImplementedException(elem.Type.ToString()); }
      }
      else
      { throw new NotImplementedException(elem.Type.ToString()); }

      return elem;
    }

    public override ElementIndex GetIndex(Element element) => GetIndex(this, element);
    public static ElementIndex GetIndex(OcadIo io, Element element)
    {
      ElementIndex index = new ElementIndex(-1);
      index.SetMapBox(element.GetExent(io.Setup));
      index.Symbol = element.Symbol;

      index.CalcElementLength(io, element);

      index.ObjectType = (short)element.Type;
      index.Status = 1;
      if (element.Symbol != -2)
      { index.Color = -1; }
      else
      { index.Color = (short)element.Color; }

      index.ViewType = (byte)element.ObjectStringType;
      if (element.ObjectStringType == ObjectStringType.CsPreview)
      { index.Status = 2; }

      return index;
    }

    public override void Write(ElementIndex index) => Write(this, index);
    public static void Write(OcadIo io, ElementIndex index)
    {
      io.WriteIndexBox(index);

      io.Writer.Write((int)index.Position);
      io.Writer.Write((int)index.Length);
      io.Writer.Write((int)index.Symbol);

      io.Writer.Write((sbyte)index.ObjectType);
      io.Writer.Write((sbyte)0); // reserved
      io.Writer.Write((sbyte)index.Status);
      io.Writer.Write((sbyte)index.ViewType);
      io.Writer.Write((short)index.Color);
      io.Writer.Write((short)0); // reserved
      io.Writer.Write((short)index.ImportLayer);
      io.Writer.Write((short)0); // reserved
    }

    public override void WriteElementSymbol(int symbol) => WriteElementSymbol(this, symbol);
    public static void WriteElementSymbol(OcadIo io, int symbol)
    {
      io.Writer.Write((int)symbol);
    }
    public override void WriteElementHeader(Element element) => WriteElementHeader(this, element);
    public static void WriteElementHeader(OcadIo io, Element element)
    {
      io.WriteElementSymbol(element.Symbol);
      io.Writer.Write((byte)element.Type);
      io.Writer.Write((byte)0);

      io.Writer.Write((short)(element.Angle * 1800 / Math.PI)); // 0.1 Degrees

      io.Writer.Write((int)element.PointCount());
      io.Writer.Write((short)(TextCountV9(element.Text) / 8));
      io.Writer.Write((short)0); // reserved

      io.Writer.Write((int)element.Color);
      io.Writer.Write((short)element.LineWidth);
      io.Writer.Write((short)element.Flags);

      io.Writer.Write((double)0); // reserved
      // OCAD 10
      io.Writer.Write((byte)0); // mark
      io.Writer.Write((byte)0); // reserved
      io.Writer.Write((short)0); // reserved;
      int heightMm = (int)(element.Height * 1000);
      io.Writer.Write((int)heightMm); // reserved;
    }

    public override void WriteElementContent(Element element) => WriteElementContent(this, element);
    public static void WriteElementContent(OcadIo io, Element element)
    {
      io.WriteElementGeometry(element);
      WriteUnicodeString(io.Writer, element.Text);
    }

    public override int CalcElementLength(Element element) => CalcElementLength(this, element);
    public static int CalcElementLength(OcadIo io, Element element)
    {
      int length = 40 + 8 * element.PointCount() + io.GetElementTextCount(element, element.Text);
      return length;
    }

    public override int GetElementTextCount(Element element, string text) => TextCountV9(text);
    public static int TextCountV9(string text)
    {
      if (text?.Length > 0)
      {
        int nText = ((text.Length * 2 / 64) + 1) * 64;
        return nText;
      }
      return 0;
    }

  }
}
