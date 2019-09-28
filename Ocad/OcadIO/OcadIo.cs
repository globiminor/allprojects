using Basics.Geom;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ocad
{
  internal abstract class OcadIo : IDisposable
  {
    private readonly EndianReader _reader;
    private readonly Stream _stream;

    private EndianWriter _writer;
    public EndianWriter Writer => _writer ?? (_writer = new EndianWriter(_reader.BaseStream));

    internal Stream BaseStream => _stream;
    internal EndianReader Reader => _reader;
    internal FileParam FileParam { get; private set; }
    internal SymbolData SymbolData { get; private set; }
    internal Setup Setup { get; private set; }
    internal bool SortByColors { get; set; }


    public IEnumerable<GeoElement> EnumGeoElements(IBox extentIntersect, IList<ElementIndex> indexList)
    {
      indexList = indexList ?? GetIndices();
      IBox extent = BoxOp.ProjectRaw(extentIntersect, Setup.Prj2Map)?.Extent;

      return EnumElements<GeoElement>(extent, indexList);
    }
    public IEnumerable<MapElement> EnumMapElements(IBox extent, IList<ElementIndex> indexList)
    {
      indexList = indexList ?? GetIndices();

      return EnumElements<MapElement>(extent, indexList);
    }
    private IEnumerable<T> EnumElements<T>(IBox extent, IList<ElementIndex> indexList) where T : Element, new()
    {
      IEnumerable<ElementIndex> idxs = EnumElementIdxs(extent, indexList);
      if (SortByColors)
      { idxs = GetColorSorted(idxs); }

      foreach (var index in idxs)
      {
        ReadElement(index, out T element);
        if (element == null)
        { continue; }

        yield return element;
      }
    }

    private List<ElementIndex> GetColorSorted(IEnumerable<ElementIndex> indices)
    {
      List<ElementIndex> sort = new List<ElementIndex>(indices);

      int pos = 0;
      Dictionary<int, ColorInfo> colors = new Dictionary<int, ColorInfo>();
      foreach (var color in ReadColorInfos())
      {
        color.Position = pos;
        colors.Add(color.Nummer, color);
        pos++;
      }
      Dictionary<int, int> symColors = new Dictionary<int, int>();
      foreach (var sym in ReadSymbols())
      {
        int color = sym.GetMainColor();
        symColors.Add(sym.Number, color);
      }

      sort.Sort((x, y) => 
      {
        if (!symColors.TryGetValue(x.Symbol, out int xCi))
        { return 1; }
        if (!symColors.TryGetValue(y.Symbol, out int yCi))
        { return -1; }

        if (!colors.TryGetValue(xCi, out ColorInfo xClr))
        { return 1; }
        if (!colors.TryGetValue(yCi, out ColorInfo yClr))
        { return 1; }

        return -xClr.Position.CompareTo(yClr.Position);
      });

      return sort;
    }
    private IEnumerable<ElementIndex> EnumElementIdxs(IBox extent, IList<ElementIndex> indexList)
    {
      foreach (var index in indexList)
      {
        if (index.Status == ElementIndex.StatusDeleted)
        { continue; }
        if (extent == null || BoxOp.Intersects(extent, index.MapBox))
        { yield return index; }
      }
    }


    public static OcadIo GetIo(Stream stream, int ocdVersion = 0, Encoding encoding = null)
    {
      encoding = encoding ?? Encoding.UTF7;

      EndianReader reader = new EndianReader(stream, encoding);
      int iVersion = ocdVersion;
      OcadIo ocadIo;
      if (iVersion == 0)
      {
        Version(reader, out int iSectionMark, out iVersion);
        ocadIo = GetIo(stream, iVersion);
        ocadIo.FileParam = ocadIo.Init(iSectionMark, iVersion);
        ocadIo.SymbolData = ocadIo.ReadSymbolData();
        ocadIo.Setup = ocadIo.ReadSetup();
      }
      else
      {
        ocadIo = GetIo(stream, iVersion);
      }

      return ocadIo;
    }

    private static OcadIo GetIo(Stream stream, int version)
    {
      OcadIo reader;
      if (version == 6 || version == 7 || version == 8)
      { reader = new Ocad8Io(stream); }
      else if (version == 9 || version == 10)
      { reader = new Ocad9Io(stream); }
      else if (version == 11)
      { reader = new Ocad11Io(stream); }
      else if (version == 12)
      { reader = new Ocad12Io(stream); }
      else if (version == 2018)
      { reader = new Ocad12Io(stream); }
      else
      { throw new Exception("Invalid OCAD File"); }

      return reader;
    }

    protected OcadIo(Stream ocadStream, Encoding encoding = null)
    {
      encoding = encoding ?? Encoding.UTF7;
      _stream = ocadStream;
      _reader = new EndianReader(ocadStream, encoding);
    }

    protected static void Version(EndianReader reader, out int sectionMark, out int version)
    {
      reader.BaseStream.Seek(0, SeekOrigin.Begin);

      if (reader.ReadInt16() != FileParam.OCAD_MARK)
      { throw new Exception("Invalid OCAD File"); }

      sectionMark = reader.ReadInt16();
      version = reader.ReadInt16();
    }

    public int ReadSymbolPosition(int i)
    {
      int p = FileParam.FirstSymbolBlock;
      while (i >= FileParam.N_IN_SYMBOL_BLOCK && p > 0)
      {
        _reader.BaseStream.Seek(p, SeekOrigin.Begin);
        p = _reader.ReadInt32();
        i -= FileParam.N_IN_ELEM_BLOCK;
      }
      if (p == 0)
      { return 0; }

      _reader.BaseStream.Seek(4 * i + p + 4, SeekOrigin.Begin);
      return _reader.ReadInt32();
    }

    public IList<ElementIndex> GetIndices()
    {
      ElementIndex idx;
      IList<ElementIndex> idxList = new List<ElementIndex>();
      int iIndex = 0;
      while ((idx = ReadIndex(iIndex)) != null)
      {
        idxList.Add(idx);
        iIndex++;
      }
      return idxList;
    }

    internal virtual IEnumerable<StringParamIndex> EnumStringParamIndices()
    {
      int iNextStrIdxBlk;

      _reader.BaseStream.Seek(FileParam.FirstStrIdxBlock, SeekOrigin.Begin);

      do
      {
        iNextStrIdxBlk = _reader.ReadInt32();
        int j = 0;
        StringParamIndex index;
        do
        {
          index = new StringParamIndex
          {
            IndexPosition = _reader.BaseStream.Position,
            FilePosition = _reader.ReadInt32(),
            Size = _reader.ReadInt32(),
            Type = (StringType)_reader.ReadInt32(),
            ElemNummer = _reader.ReadInt32()
          };

          yield return index;

          j++;
        }
        while (index.FilePosition > 0 && j < FileParam.N_IN_STRINGPARAM_BLOCK);

        if (iNextStrIdxBlk > 0)
        {
          _reader.BaseStream.Seek(iNextStrIdxBlk, SeekOrigin.Begin);
        }
      }
      while (iNextStrIdxBlk > 0);
    }

    public string ReadStringParam(StringParamIndex index, int nTabs = 0)
    {
      int iTab = 0;
      _reader.BaseStream.Seek(index.FilePosition, SeekOrigin.Begin);
      StringBuilder sResult = new StringBuilder(ReadTab(255, out char e1));
      iTab++;
      if (iTab == nTabs) return sResult.ToString();


      int max = index.Size;
      char e0 = e1;
      string sNext = ReadTab(255, out e1);
      while (sNext != "")
      {
        if (sResult.Length + 1 + sNext.Length > max)
        {
          break;
        }
        sResult.Append(e0);
        sResult.Append(sNext);
        iTab++;
        if (iTab == nTabs) return sResult.ToString();

        e0 = e1;
        if (e0 == 0)
        {
          break;
        }
        sNext = ReadTab(255, out e1);
      }
      return sResult.ToString();
    }

    public string ReadTab(int max, out char endChar)
    {
      string sName = "";
      endChar = '\0';
      if (_reader.BaseStream.Position < _reader.BaseStream.Length)
      {
        int i = 0;
        endChar = _reader.ReadChar();
        while (endChar != '\t' && endChar != 0 && endChar != '\n' && i + 1 < max &&
          _reader.BaseStream.Position < _reader.BaseStream.Length)
        {
          sName = sName + endChar;
          endChar = (char)_reader.ReadByte();
          i++;
        }
      }
      return sName;
    }

    public void Dispose()
    {
      Close();
    }
    public void Close()
    {
      _reader?.Close();
    }

    internal static int FLength(Element element)
    {
      return 32 + element.PointCount() * 8 + element.Text.Length;
    }

    List<int> _indexBlockStarts;
    internal bool SeekIndex(int id)
    {
      if (id < 0)
      { return false; }
      if (_indexBlockStarts == null)
      {
        _indexBlockStarts = new List<int> { FileParam.FirstIndexBlock };
      }
      int iBlock = 0;
      int p = _indexBlockStarts[iBlock];
      int blockPos = id;
      while (blockPos >= FileParam.N_IN_ELEM_BLOCK && p > 0)
      {
        iBlock++;
        if (iBlock == _indexBlockStarts.Count)
        {
          _reader.BaseStream.Seek(p, SeekOrigin.Begin);
          p = _reader.ReadInt32();
          _indexBlockStarts.Add(p);
        }
        else
        {
          p = _indexBlockStarts[iBlock];
        }
        blockPos -= FileParam.N_IN_ELEM_BLOCK;
      }
      if (p == 0)
      { return false; }

      _reader.BaseStream.Seek(FileParam.INDEX_LENGTH * blockPos + p + 4, SeekOrigin.Begin);
      return true;
    }

    public string ReadCourseTemplateName()
    {
      if (FileParam.SectionMark != 3)
      {
        return null;
      }
      _reader.BaseStream.Seek(FileParam.FirstStrIdxBlock + 4, SeekOrigin.Begin);
      _reader.BaseStream.Seek(_reader.ReadInt32(), SeekOrigin.Begin);

      return ReadTab(1024, out char e);
    }

    public ElementIndex ReadIndexPart(int id)
    {
      if (SeekIndex(id) == false)
      { return null; }

      ElementIndex index = new ElementIndex(id);

      Box box = new Box(new Point2D(), new Point2D());

      box.Min.X = Coord.GetGeomPart(_reader.ReadInt32());
      box.Min.Y = Coord.GetGeomPart(_reader.ReadInt32());
      box.Max.X = Coord.GetGeomPart(_reader.ReadInt32());
      box.Max.Y = Coord.GetGeomPart(_reader.ReadInt32());

      index.SetMapBox(box);

      return index;
    }

    public virtual GeoElement ReadControlGeometry(Control control,
      IList<StringParamIndex> settingIndexList)
    {
      GeoElement element;
      foreach (var strIdx in settingIndexList)
      {
        if (strIdx.Type == StringType.Control)
        {
          _reader.BaseStream.Seek(strIdx.FilePosition, SeekOrigin.Begin);
          string name = ReadTab(control.Name.Length + 1, out char e);
          if (name == control.Name)
          {
            string stringParam = ReadStringParam(strIdx);
            ControlPar ctrPar = new ControlPar(stringParam);
            control.Text = ctrPar.TextDesc;

            ReadElement(strIdx.ElemNummer - 1, out element);
            control.Element = element;
            return element;
          }
        }
      }
      element = null;
      return element;
    }

    public T ReadElement<T>(int i, out T element) where T : Element, new()
    {
      return ReadElement(ReadIndex(i), out element);
    }
    public T ReadElement<T>(ElementIndex index, out T element) where T : Element, new()
    {
      if (index == null)
      {
        element = null;
        return element;
      }
      if (index.Position == 0)
      {
        element = null;
        return element;
      }

      _reader.BaseStream.Seek(index.Position, SeekOrigin.Begin);

      ReadElement(out element);
      VerifyExists(element, index);

      return element;
    }

    protected virtual void VerifyExists(Element element, ElementIndex index)
    { }

    public List<Coord> ReadCoords(int nPoints)
    {
      List<Coord> coords = new List<Coord>(nPoints);
      for (int iPoint = 0; iPoint < nPoints; iPoint++)
      {
        coords.Add(ReadCoord());
      }
      return coords;
    }

    protected Coord ReadCoord()
    {
      return new Coord(_reader.ReadInt32(), _reader.ReadInt32());
    }

    protected void ReadGpsAdjust()
    {
      ReadCoord();
      _reader.ReadDouble();
      _reader.ReadDouble();
      _reader.ReadChars(16);
    }

    public void WriteIndexBox(ElementIndex index)
    {
      Coord coord = Coord.Create(index.MapBox.Min, 0);
      Writer.Write((int)coord.Ox);
      Writer.Write((int)coord.Oy);
      coord = Coord.Create(index.MapBox.Max, 0);
      Writer.Write((int)coord.Ox);
      Writer.Write((int)coord.Oy);
    }

    public void WriteElementGeometry(Element element)
    {
      WriteCoords(element.GetCoords(Setup));
    }

    internal void WriteGeometry(GeoElement.Geom geometry)
    {
      IEnumerable<Coord> coords = geometry.EnumCoords();
      WriteCoords(coords);
    }

    internal void WriteCoords(IEnumerable<Coord> coords)
    {
      foreach (Coord coord in coords)
      {
        Writer.Write((int)coord.Ox);
        Writer.Write((int)coord.Oy);
      }
    }

    public abstract IEnumerable<ColorInfo> ReadColorInfos();

    public ColorSeparation ReadColorSeparation(int index)
    {
      if (index >= SymbolData.ColorSeparationCount)
      { return null; }

      ColorSeparation sep = new ColorSeparation();

      _reader.BaseStream.Seek(FileParam.HEADER_LENGTH +
        FileParam.SYM_BLOCK_LENGTH +
        FileParam.MAX_COLORS * FileParam.COLORINFO_LENGTH +
        index * FileParam.COLORSEP_LENGTH, SeekOrigin.Begin);

      sep.Name = _reader.ReadChars(16).ToString();

      sep.Color.Cyan = _reader.ReadByte();
      sep.Color.Magenta = _reader.ReadByte();
      sep.Color.Yellow = _reader.ReadByte();
      sep.Color.Black = _reader.ReadByte();

      sep.RasterFreq = _reader.ReadInt16();
      sep.RasterAngle = _reader.ReadInt16();

      return sep;
    }

    public virtual SymbolData ReadSymbolData()
    {
      SymbolData sym = new SymbolData();
      _reader.BaseStream.Seek(FileParam.HEADER_LENGTH, SeekOrigin.Begin);
      sym.ColorCount = _reader.ReadInt16();
      sym.ColorSeparationCount = _reader.ReadInt16();
      sym.CyanFreq = _reader.ReadInt16();
      sym.CyanAngle = _reader.ReadInt16();
      sym.MagentaFreq = _reader.ReadInt16();
      sym.MagentaAngle = _reader.ReadInt16();
      sym.YellowFreq = _reader.ReadInt16();
      sym.YellowAngle = _reader.ReadInt16();
      sym.BlackFreq = _reader.ReadInt16();
      sym.BlackAngle = _reader.ReadInt16();
      _reader.ReadInt16(); // reserve 1
      _reader.ReadInt16(); // reserve 2

      return sym;
    }


    public Symbol.BaseSymbol ReadSymbol(int position)
    {
      _reader.BaseStream.Seek(position, SeekOrigin.Begin);
      return ReadSymbol();
    }
    public abstract Symbol.BaseSymbol ReadSymbol();

    public IEnumerable<Symbol.BaseSymbol> ReadSymbols()
    {
      List<int> symPos = new List<int>();
      int iSymbol = 0;
      for (int posIndex = ReadSymbolPosition(iSymbol);
           posIndex > 0;
           posIndex = ReadSymbolPosition(iSymbol))
      {
        symPos.Add(posIndex);

        iSymbol++;
      }

      foreach (var posIndex in symPos)
      {
        Symbol.BaseSymbol symbol = ReadSymbol(posIndex);
        yield return symbol;
      }
    }

    protected static void WriteUnicodeString(EndianWriter writer, string text)
    {
      if (!string.IsNullOrEmpty(text))
      {
        writer.WriteUnicodeString(text);
        int n = Ocad9Io.TextCountV9(text);
        for (int i = text.Length * 2 + 2; i < n; i++)
        { writer.BaseStream.WriteByte(0); }
      }
    }

    public abstract T ReadElement<T>(out T element) where T : Element, new();

    protected abstract FileParam Init(int sectionMark, int version);

    public abstract Setup ReadSetup();

    public abstract ElementIndex ReadIndex(int i);
    public abstract int ReadElementLength();

    // Symbol description
    public abstract string ReadDescription();
    public abstract void ReadAreaSymbol(Symbol.AreaSymbol symbol);

    public abstract ElementIndex GetIndex(Element element);
    public abstract int GetElementTextCount(Element element, string text);
    public abstract void Write(ElementIndex index);

    public abstract void WriteElementHeader(Element element);
    public abstract void WriteElementContent(Element element);
    public abstract void WriteElementSymbol(int symbol);

    public abstract int CalcElementLength(Element element);
  }
}
