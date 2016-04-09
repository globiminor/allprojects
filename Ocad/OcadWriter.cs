using System;
using System.IO;
using System.Collections.Generic;
using Basics.Geom;
using Ocad.StringParams;
// ReSharper disable RedundantCast

namespace Ocad
{
  /// <summary>
  /// Summary description for OcadWriter.
  /// </summary>
  public abstract class OcadWriter : IDisposable
  {
    public delegate T ElementIndexDlg<T>(ElementIndex element);

    protected EndianWriter Writer { get; set; }
    protected FileParam Param { get; set; }
    protected OcadReader OcdReader { get; set; }
    protected EndianReader _reader;
    protected Setup _setup;

    protected abstract void Init();

    public OcadReader Reader
    {
      get
      {
        if (OcdReader == null)
        { Init(); }
        return OcdReader;
      }
    }

    public Setup Setup
    {
      get
      {
        Init();
        return _setup;
      }
    }

    public void Overwrite(Element element, int index)
    {
      Init();
      IGeometry prj = null;

      if (element.IsGeometryProjected)
      {
        prj = element.Geometry;
        element.Geometry = element.Geometry.Project(_setup.Prj2Map);
        element.IsGeometryProjected = false;
      }

      ElementIndex idx = element.GetIndex();
      idx.Position = (int)_reader.BaseStream.Seek(0, SeekOrigin.End);

      if (!Reader.SeekIndex(index))
      { throw new InvalidOperationException("Invalid Index " + index); }
      Write(idx);

      Writer.BaseStream.Seek(idx.Position, SeekOrigin.Begin);
      Write(element);

      if (prj != null)
      {
        element.Geometry = prj;
        element.IsGeometryProjected = true;
      }
    }
    /// <summary>
    /// assumption : index has all values set except for pos
    /// </summary>
    /// <param name="index"></param>
    private int Append(ElementIndex index)
    {
      Init();
      int i;
      long p = Param.FirstIndexBlock;
      long p0 = p;

      int indexNr = 0;

      while (p > 0)
      {
        p0 = p;
        Writer.BaseStream.Seek(p, SeekOrigin.Begin);
        p = _reader.ReadInt32();

        if (p > 0)
        {
          indexNr += FileParam.N_IN_ELEM_BLOCK;
        }
      }
      if (p0 > 0)
      {
        p = p0 + 4;
        _reader.BaseStream.Seek(p + 16, SeekOrigin.Begin); /* index->pos */
        i = 0;
        while (_reader.ReadInt32() != 0 && i < FileParam.N_IN_ELEM_BLOCK)
        {
          p += Param.INDEX_LENGTH;
          _reader.BaseStream.Seek(p + 16, SeekOrigin.Begin);
          i++;
        }
        indexNr = indexNr + i;
      }
      else
      { // empty ocad file
        i = FileParam.N_IN_ELEM_BLOCK;
        indexNr = 0;
      }
      if (i == FileParam.N_IN_ELEM_BLOCK)
      { /* new index block */
        Writer.BaseStream.Seek(0, SeekOrigin.End);
        p = Writer.BaseStream.Position;
        Writer.Write((int)0);
        int n = FileParam.N_IN_ELEM_BLOCK * Param.INDEX_LENGTH;
        for (int j = 0; j < n; j++)
        { Writer.Write((byte)0); }
        if (p0 > 0)
        {
          Writer.BaseStream.Seek(p0, SeekOrigin.Begin);
        }
        else
        {
          Writer.BaseStream.Seek(12, SeekOrigin.Begin); /* first Index Block position */
          Param.FirstIndexBlock = (int)p;
        }
        Writer.Write((int)p);
        p += 4;

        index.Position = (int)p + n;
      }
      else
      {
        Writer.BaseStream.Seek(0, SeekOrigin.End);
        index.Position = (int)Writer.BaseStream.Position;
      }
      Writer.BaseStream.Seek(p, SeekOrigin.Begin);

      Write(index);

      return indexNr;
    }

    public int Append(Element element)
    {
      if (element == null)
      { return -1; }

      Init();
      ElementIndex index = element.GetIndex();

      int pos = Append(element, index);
      return pos;
    }

    public int Append(Element element, ElementIndex index)
    {
      IGeometry prj = null;
      if (element.IsGeometryProjected)
      {
        prj = element.Geometry;
        element.Geometry = element.Geometry.Project(_setup.Prj2Map);
        index.SetBox(element.Geometry.Extent);
        element.IsGeometryProjected = false;
      }

      int pos = Append(index);

      Writer.BaseStream.Seek(index.Position, SeekOrigin.Begin);
      Write(element);

      if (prj != null)
      {
        element.Geometry = prj;
        index.SetBox(element.Geometry.Extent);
        element.IsGeometryProjected = true;
      }

      return pos;
    }

    protected virtual void Write(ElementIndex index)
    {
      Coord pCoord = Coord.Create(index.Box.Min, 0);
      Writer.Write((int)pCoord.Ox);
      Writer.Write((int)pCoord.Oy);
      pCoord = Coord.Create(index.Box.Max, 0);
      Writer.Write((int)pCoord.Ox);
      Writer.Write((int)pCoord.Oy);
    }

    protected abstract void Write(Element element);
    protected abstract void WriteElementHeader(Element element);
    protected abstract void WriteElementSymbol(int symbol);

    protected void Write(IGeometry geometry)
    {
      Coord pCoord;

      if (geometry is Point)
      {
        Point p = (Point)geometry;
        pCoord = Coord.Create(p, Coord.Flags.none);
        Writer.Write((int)pCoord.Ox);
        Writer.Write((int)pCoord.Oy);
      }
      else if (geometry is PointCollection)
      {
        foreach (Point p in ((PointCollection)geometry))
        {
          pCoord = Coord.Create(p, Coord.Flags.none);
          Writer.Write((int)pCoord.Ox);
          Writer.Write((int)pCoord.Oy);
        }
      }
      else if (geometry is Polyline)
      {
        Write((Polyline)geometry, false);
      }
      else if (geometry is Area)
      {
        Area area = (Area)geometry;
        bool bIsInnerRing = false;
        foreach (Polyline pLine in area.Border)
        {
          Write(pLine, bIsInnerRing);
          bIsInnerRing = true;
        }
      }
      else
      { throw new Exception("Unhandled geometry type " + geometry.GetType()); }
    }

    public void Write(Symbol.SymbolGraphics graphics)
    {
      if (graphics == null)
      { return; }

      Writer.Write((short)graphics.Type);
      ushort iFlags = 0;
      if (graphics.RoundEnds)
      { iFlags |= 1; }
      Writer.Write((short)iFlags);
      Writer.Write((short)graphics.Color);

      Writer.Write((short)graphics.LineWidth);
      Writer.Write((short)graphics.Diameter);

      Writer.Write((short)Element.PointCount(graphics.Geometry));
      Writer.Write((short)0); // reserved
      Writer.Write((short)0); // reserved

      Write(graphics.Geometry);
    }

    private void Write(Polyline polyline, bool isInnerRing)
    {
      Coord pCoord;

      if (polyline.Points.Count < 1)
      { return; }

      if (isInnerRing)
      { pCoord = Coord.Create(polyline.Points.First.Value, Coord.Flags.firstHolePoint); }
      else
      { pCoord = Coord.Create(polyline.Points.First.Value, Coord.Flags.none); }
      Writer.Write((int)pCoord.Ox);
      Writer.Write((int)pCoord.Oy);

      foreach (Curve pSeg in polyline.Segments)
      {
        if (pSeg is Bezier)
        {
          Bezier pBezier = pSeg as Bezier;

          pCoord = Coord.Create(pBezier.P1, Coord.Flags.firstBezierPoint);
          Writer.Write((int)pCoord.Ox);
          Writer.Write((int)pCoord.Oy);

          pCoord = Coord.Create(pBezier.P2, Coord.Flags.secondBezierPoint);
          Writer.Write((int)pCoord.Ox);
          Writer.Write((int)pCoord.Oy);
        }

        pCoord = Coord.Create(pSeg.End, Coord.Flags.none);
        Writer.Write((int)pCoord.Ox);
        Writer.Write((int)pCoord.Oy);
      }

    }


    public void WriteHeader(FileParam data)
    {
      Writer.BaseStream.Seek(0, SeekOrigin.Begin);

      Writer.Write((short)FileParam.OCAD_MARK);
      Writer.Write((short)data.SectionMark);
      Writer.Write((short)data.Version);
      Writer.Write((short)data.SubVersion);
      Writer.Write((int)data.FirstSymbolBlock);
      Writer.Write((int)data.FirstIndexBlock);
      Writer.Write((int)data.SetupPosition);
      Writer.Write((int)data.SetupSize);
      Writer.Write((int)data.InfoPosition);
      Writer.Write((int)data.InfoSize);
      Writer.Write((int)data.FirstStrIdxBlock);
      Writer.Write((int)data.FileNamePosition);
      Writer.Write((int)data.FileNameSize);
      Writer.Write((int)0); // reserved 4
    }

    public void WriteSymData(SymbolData sym)
    {
      Writer.BaseStream.Seek(FileParam.HEADER_LENGTH, SeekOrigin.Begin);

      Writer.Write((short)sym.ColorCount);
      Writer.Write((short)sym.ColorSeparationCount);
      Writer.Write((short)sym.CyanFreq);
      Writer.Write((short)sym.CyanAngle);
      Writer.Write((short)sym.MagentaFreq);
      Writer.Write((short)sym.MagentaAngle);
      Writer.Write((short)sym.YellowFreq);
      Writer.Write((short)sym.YellowAngle);
      Writer.Write((short)sym.BlackFreq);
      Writer.Write((short)sym.BlackAngle);
      Writer.Write((short)0); // reserved 1
      Writer.Write((short)0); // reserved 2
    }

    public void WriteSetup(Setup setup, int length)
    {
      int i;

      int l = 0;
      Writer.Write((int)setup.Offset.X); l += 4;
      Writer.Write((int)setup.Offset.Y); l += 4;

      Writer.Write((double)setup.GridDistance); l += 8;

      Writer.Write((short)setup.WorkMode); l += 2;
      Writer.Write((short)setup.LineMode); l += 2;
      Writer.Write((short)setup.EditMode); l += 2;
      Writer.Write((short)setup.Symbol); l += 2;
      Writer.Write((double)setup.Scale); l += 8;

      Writer.Write((double)setup.PrjTrans.X); l += 8;
      Writer.Write((double)setup.PrjTrans.Y); l += 8;
      Writer.Write((double)setup.PrjRotation); l += 8;
      Writer.Write((double)setup.PrjGrid); l += 8;

      for (i = l; i < length; i++)
      { Writer.Write((char)0); }
    }

    public int WriteDefaultHeaderV7(double scale)
    {
      FileParam data = new FileParamV8();
      SymbolData sym = new SymbolData();
      Setup setup = new Setup();
      int i, j;

      setup.Offset.X = 0;
      setup.Offset.Y = 0;
      setup.GridDistance = 500;
      setup.WorkMode = WorkMode.curve;
      setup.LineMode = LineMode.curve;
      setup.EditMode = EditMode.editPoint;
      setup.Symbol = 0;
      setup.Scale = scale;
      setup.PrjTrans.X = 0;
      setup.PrjTrans.Y = 0;
      setup.PrjRotation = 0;
      setup.PrjGrid = 1000;

      sym.ColorCount = 0;
      sym.ColorSeparationCount = 0;
      sym.CyanFreq = 1500;
      sym.CyanAngle = 150;
      sym.MagentaFreq = 1500;
      sym.MagentaAngle = 750;
      sym.YellowFreq = 1500;
      sym.YellowAngle = 0;
      sym.BlackFreq = 1500;
      sym.BlackAngle = 450;

      data.SectionMark = 7;
      data.Version = 7;
      data.SubVersion = 90;
      data.FirstSymbolBlock = 0;
      data.FirstIndexBlock = 0;

      data.SetupPosition = FileParam.HEADER_LENGTH + FileParam.SYM_BLOCK_LENGTH +
        FileParam.MAX_COLORS * FileParam.COLORINFO_LENGTH +
        FileParam.MAX_SEPS * FileParam.COLORSEP_LENGTH;
      data.SetupSize = 1348;

      data.FirstSymbolBlock = data.SetupPosition + data.SetupSize;
      data.FirstIndexBlock = data.FirstSymbolBlock +
        (FileParam.N_IN_SYMBOL_BLOCK + 1) * 4; // (sizeof(int))

      data.InfoPosition = 0;
      data.InfoSize = 0;
      data.FirstStrIdxBlock = 0;

      WriteHeader(data);

      WriteSymData(sym);

      /* colors */
      for (i = 0; i < FileParam.MAX_COLORS; i++)
      {
        for (j = 0; j < FileParam.COLORINFO_LENGTH; j++)
        { Writer.Write((char)0); }
      }

      /* separations */
      for (i = 0; i < FileParam.MAX_SEPS; i++)
      {
        for (j = 0; j < FileParam.COLORSEP_LENGTH; j++)
        { Writer.Write((char)0); }
      }

      WriteSetup(setup, data.SetupSize);

      /* symbol indices */
      Writer.Write((int)0);
      for (i = 0; i < FileParam.N_IN_SYMBOL_BLOCK; i++)
      {
        Writer.Write((int)0);
      }

      /* elements */
      return 0;
    }

    public void Dispose()
    {
      if (Writer != null)
      { Writer.Close(); }
      Writer = null;
    }

    public void Close()
    {
      Writer.Close();
    }

    public int DeleteElements(ElementIndexDlg<bool> checkDelete)
    {
      Init();

      int n = 0;
      int iIndex = 0;
      ElementIndex pIndex = OcdReader.ReadIndex(iIndex);
      while (pIndex != null)
      {
        long pos = Writer.BaseStream.Position;
        if (checkDelete(pIndex))
        {
          pIndex.Status = ElementIndex.StatusDeleted;
          Writer.BaseStream.Seek(pos - Param.INDEX_LENGTH, SeekOrigin.Begin);
          Write(pIndex);

          n++;
        }

        iIndex++;
        pIndex = OcdReader.ReadIndex(iIndex);
      }
      return n;
    }

    public int ChangeSymbols(ElementIndexDlg<int> changeSymbol)
    {
      Init();

      int n = 0;
      int iIndex = 0;
      ElementIndex pIndex = OcdReader.ReadIndex(iIndex);
      List<ElementIndex> changedElements = new List<ElementIndex>();
      while (pIndex != null)
      {
        long pos = Writer.BaseStream.Position;
        int newSymbol = changeSymbol(pIndex);
        if (newSymbol != pIndex.Symbol)
        {
          pIndex.Symbol = newSymbol;
          Writer.BaseStream.Seek(pos - Param.INDEX_LENGTH, SeekOrigin.Begin);
          Write(pIndex);

          changedElements.Add(pIndex);
          n++;
        }

        iIndex++;
        pIndex = OcdReader.ReadIndex(iIndex);
      }

      foreach (ElementIndex index in changedElements)
      {
        Writer.BaseStream.Seek(index.Position, SeekOrigin.Begin);
        // Assumption: Symbol and ObjectType is at the start position!
        WriteElementSymbol(index.Symbol);
        Writer.Write((byte)index.ObjectType);
      }
      return n;
    }

  }
  #region OCAD8
  public class Ocad8Writer : OcadWriter
  {
    public Ocad8Writer(Stream ocadStream)
    {
      Writer = new EndianWriter(ocadStream);
      Param = new FileParamV8();
    }

    protected override void Init()
    {
      if (OcdReader == null)
      {
        OcdReader = new Ocad8Reader(Writer.BaseStream);
        _reader = new EndianReader(Writer.BaseStream, true);
        Param = OcdReader.FileParam;
        _setup = OcdReader.ReadSetup();
      }
    }

    protected override void Write(ElementIndex index)
    {
      base.Write(index);

      Writer.Write((int)index.Position);
      Writer.Write((short)index.Length);
      Writer.Write((short)index.Symbol);
    }

    protected override void Write(Element element)
    {
      WriteElementHeader(element);

      Write(element.Geometry);

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

    protected override void WriteElementHeader(Element element)
    {
      ElementV8 elem8 = element as ElementV8;
      WriteElementSymbol(element.Symbol);
      Writer.Write((byte)element.Type);
      Writer.Write((byte)(element.UnicodeText ? 1 : 0));
      Writer.Write((short)element.PointCount());
      Writer.Write((short)element.TextCount());
      Writer.Write((short)(element.Angle * 180 / Math.PI)); // 1 Degrees
      Writer.Write((short)0);
      if (elem8 != null)
      { Writer.Write((int)elem8.ReservedHeight); }
      else
      { Writer.Write((int)0); }
      for (int i = 0; i < 16; i++)
      { Writer.Write(' '); }
    }

    protected override void WriteElementSymbol(int symbol)
    {
      Writer.Write((short)symbol);
    }
  }
  #endregion

  #region OCAD9
  public class Ocad9Writer : OcadWriter
  {
    private int _removeNIndex;

    private StringType _removeType;

    private StringType _overwriteType;

    private StringParamIndex _overwriteIndex;

    public static Ocad9Writer AppendTo(string file)
    {
      return AppendTo(new FileStream(file, FileMode.Open,
         FileAccess.ReadWrite));
    }
    public static Ocad9Writer AppendTo(Stream ocadStream)
    {
      Ocad9Writer ocd = new Ocad9Writer();
      ocd.Writer = new EndianWriter(ocadStream);
      ocd.Param = new FileParamV9();
      return ocd;
    }

    protected override void Init()
    {
      if (OcdReader == null)
      {
        _reader = new EndianReader(Writer.BaseStream, System.Text.Encoding.UTF7, true);
        OcdReader = OcadReader.Open(Writer.BaseStream);  //new Ocad9Reader(_writer.BaseStream);
        Param = OcdReader.FileParam;
        _setup = OcdReader.ReadSetup();
      }
    }

    public void DeleteElements(IList<int> symbols)
    {
      Init();
      List<int> symSort = null;
      if (symbols != null)
      {
        symSort = new List<int>(symbols);
        symSort.Sort();
      }

      int iIndex = 0;
      ElementIndex pIndex = OcdReader.ReadIndex(iIndex);
      while (pIndex != null)
      {
        if (symSort == null || symSort.BinarySearch(pIndex.Symbol) >= 0)
        {
          pIndex.Status = ElementIndex.StatusDeleted;
          Writer.BaseStream.Seek(-Param.INDEX_LENGTH, SeekOrigin.Current);
          Write(pIndex);
        }
        iIndex++;
        pIndex = OcdReader.ReadIndex(iIndex);
      }
    }

    public void Remove(StringType type)
    {
      Init();
      _removeNIndex = 0;
      _removeType = type;
      OcdReader.StringIndexRead += Remove_StringIndexRead;
      OcdReader.ReadStringParamIndices();
      OcdReader.StringIndexRead -= Remove_StringIndexRead;
    }

    private void Remove_StringIndexRead(object sender, StringIndexEventArgs args)
    {
      if (args.Index.Type != _removeType)
      { return; }

      Writer.BaseStream.Seek(-8, SeekOrigin.Current);

      Writer.Write((int)-1);

      Writer.BaseStream.Seek(4, SeekOrigin.Current);

      //if (args.Index.Type == _removeType)
      //{
      //  _removeNIndex++;
      //}
      //else if (_removeNIndex > 0)
      //{
      //  long iPos = _reader.BaseStream.Position;
      //  _writer.BaseStream.Seek(-(_removeNIndex + 1) * 16, SeekOrigin.Current);

      //  _writer.Write((int)args.Index.FilePosition);
      //  _writer.Write((int)args.Index.Size);
      //  _writer.Write((int)args.Index.Type);
      //  _writer.Write((int)args.Index.ElemNummer);

      //  _reader.BaseStream.Seek(iPos, SeekOrigin.Begin);
      //}

      //if (_removeNIndex > 0)
      //{
      //  _writer.BaseStream.Seek(-16, SeekOrigin.Current);

      //  _writer.Write((int)0);
      //  _writer.Write((int)0);
      //  _writer.Write((int)0);
      //  _writer.Write((int)0);
      //}
    }

    public void Append(StringType type, int elemNummer, string stringParam)
    {
      Init();
      _reader.BaseStream.Seek(0, SeekOrigin.End);
      int iPos = (int)_reader.BaseStream.Position;

      StringParamIndex overwriteIndex = new StringParamIndex();
      overwriteIndex.Type = type;
      overwriteIndex.ElemNummer = elemNummer;
      overwriteIndex.Size = stringParam.Length / 64 * 64 + 64; // for some funny reason
      Append(overwriteIndex);

      Overwrite(overwriteIndex, stringParam);

      for (int i = 0; i < 64; i++) // same reason as above
      { Writer.Write((byte)0); }
    }

    public void Overwrite(StringParamIndex overwriteIndex, string stringParam)
    {
      if (stringParam.Length > overwriteIndex.Size)
      { }
      _reader.BaseStream.Seek(overwriteIndex.FilePosition, SeekOrigin.Begin);
      int n = stringParam.Length;
      for (int i = 0; i < n; i++)
      { Writer.Write((byte)stringParam[i]); }
    }

    public void Overwrite(StringType type, int elemNummer, string stringParam)
    {
      Init();
      _reader.BaseStream.Seek(0, SeekOrigin.End);
      int iPos = (int)_reader.BaseStream.Position;

      _overwriteType = type;
      _overwriteIndex = null;

      OcdReader.StringIndexRead += Overwrite_StringIndexRead;
      OcdReader.ReadStringParamIndices();
      OcdReader.StringIndexRead -= Overwrite_StringIndexRead;

      if (_overwriteIndex == null)
      {
        _overwriteIndex = new StringParamIndex();
        _overwriteIndex.Type = type;
        _overwriteIndex.ElemNummer = elemNummer;
        _overwriteIndex.Size = stringParam.Length;
        Append(_overwriteIndex);
      }
      else
      {
        _reader.BaseStream.Seek(-16, SeekOrigin.Current);
        _overwriteIndex.Size = stringParam.Length;
        _overwriteIndex.FilePosition = iPos;

        Writer.Write((int)_overwriteIndex.FilePosition);
        Writer.Write((int)_overwriteIndex.Size);
        Writer.Write((int)_overwriteIndex.Type);
        Writer.Write((int)_overwriteIndex.ElemNummer);
      }

      _reader.BaseStream.Seek(_overwriteIndex.FilePosition, SeekOrigin.Begin);
      int n = stringParam.Length;
      for (int i = 0; i < n; i++)
      { Writer.Write((byte)stringParam[i]); }
    }

    private void Overwrite_StringIndexRead(object sender, StringIndexEventArgs args)
    {
      if (args.Index.Type == _overwriteType)
      {
        args.Cancel = true;
        _overwriteIndex = args.Index;
      }
    }

    private void Append(StringParamIndex index)
    {
      Init();
      int i;
      long p = Param.FirstStrIdxBlock;
      long p0 = p;
      while (p > 0)
      {
        p0 = p;
        Writer.BaseStream.Seek(p, SeekOrigin.Begin);
        p = _reader.ReadInt32();
      }
      if (p0 > 0)
      {
        p = p0 + 4;
        _reader.BaseStream.Seek(p, SeekOrigin.Begin); /* index->pos */
        StringParamIndex pIndex = new StringParamIndex();
        pIndex.FilePosition = 1;
        i = 0;
        while (pIndex.FilePosition > 0 && i < FileParam.N_IN_STRINGPARAM_BLOCK)
        {
          pIndex.FilePosition = _reader.ReadInt32();
          pIndex.Size = _reader.ReadInt32();
          pIndex.Type = (StringType)_reader.ReadInt32();
          pIndex.ElemNummer = _reader.ReadInt32();

          p += 16;
          i++;
        }
      }
      else
      { // empty ocad file
        i = FileParam.N_IN_STRINGPARAM_BLOCK;
      }
      if (i == FileParam.N_IN_STRINGPARAM_BLOCK)
      { /* new index block */
        Writer.BaseStream.Seek(0, SeekOrigin.End);
        p = Writer.BaseStream.Position;
        Writer.Write((int)0);
        int n = FileParam.N_IN_STRINGPARAM_BLOCK * 16;
        for (int j = 0; j < n; j++)
        { Writer.Write((byte)0); }
        if (p0 > 0)
        {
          Writer.BaseStream.Seek(p0, SeekOrigin.Begin);
        }
        else
        {
          Writer.BaseStream.Seek(32, SeekOrigin.Begin); /* first String Index Block position */
          Param.FirstStrIdxBlock = (int)p;
        }
        Writer.Write((int)p);
        p += 4;

        index.FilePosition = (int)p + n;
      }
      else
      {
        Writer.BaseStream.Seek(0, SeekOrigin.End);
        index.FilePosition = (int)Writer.BaseStream.Position;
        p -= 16;
      }
      Writer.BaseStream.Seek(p, SeekOrigin.Begin);

      Writer.Write((int)index.FilePosition);
      Writer.Write((int)index.Size);
      Writer.Write((int)index.Type);
      Writer.Write((int)index.ElemNummer);
    }

    protected override void Write(ElementIndex index)
    {
      base.Write(index);

      Writer.Write((int)index.Position);
      Writer.Write((int)index.Length);
      Writer.Write((int)index.Symbol);

      Writer.Write((sbyte)index.ObjectType);
      Writer.Write((sbyte)0); // reserved
      Writer.Write((sbyte)index.Status);
      Writer.Write((sbyte)index.ViewType);
      Writer.Write((short)index.Color);
      Writer.Write((short)0); // reserved
      Writer.Write((short)index.ImportLayer);
      Writer.Write((short)0); // reserved
    }

    public static void Write(Element element, Stream stream)
    {
      Ocad9Writer writer = AppendTo(stream);
      writer.Write(element);
    }
    protected override void Write(Element element)
    {
      WriteElementHeader(element);

      Write(element.Geometry);

      if (element.Text != "")
      {
        Writer.WriteUnicodeString(element.Text);
        int n = ElementV9.TextCount(element.Text);
        for (int i = element.Text.Length * 2; i < n; i++)
        { Writer.BaseStream.WriteByte(0); }
      }
    }

    protected override void WriteElementHeader(Element element)
    {
      ElementV9 elem9 = element as ElementV9;

      WriteElementSymbol(element.Symbol);
      Writer.Write((byte)element.Type);
      Writer.Write((byte)0);

      Writer.Write((short)(element.Angle * 1800 / Math.PI)); // 0.1 Degrees

      Writer.Write((int)element.PointCount());
      int nText = 0;
      if (element.Text.Length > 0)
      { nText = ElementV9.TextCount(element.Text) / 8; }
      Writer.Write((short)nText);
      Writer.Write((short)0); // reserved

      if (elem9 != null)
      {
        Writer.Write((int)elem9.Color);
        Writer.Write((short)elem9.LineWidth);
        Writer.Write((short)elem9.Flags);
      }
      else
      {
        Writer.Write((int)0);
        Writer.Write((short)0);
        Writer.Write((short)0);
      }

      Writer.Write((double)0); // reserved
      // OCAD 10
      Writer.Write((byte)0); // mark
      Writer.Write((byte)0); // reserved
      Writer.Write((short)0); // reserved;
      int heightMm = (int)(element.Height * 1000);
      Writer.Write((int)heightMm); // reserved;
    }

    protected override void WriteElementSymbol(int symbol)
    {
      Writer.Write((int)symbol);
    }

    public void WriteSymbolStatus(Symbol.SymbolStatus status, int symbolStartPosition)
    {
      _reader.BaseStream.Seek(symbolStartPosition + 11, SeekOrigin.Begin);
      Writer.Write((byte)status);
    }

    public void SymbolsSetState(IList<int> symbols, Symbol.SymbolStatus state)
    {
      Init();
      List<int> sortSymbols = null;
      if (symbols != null)
      {
        sortSymbols = new List<int>(symbols);
        sortSymbols.Sort();
      }

      List<int> symPos = new List<int>();

      int iSymbol = 0;
      for (int posIndex = OcdReader.ReadSymbolPosition(iSymbol);
        posIndex > 0; posIndex = OcdReader.ReadSymbolPosition(iSymbol))
      {
        symPos.Add(posIndex);

        iSymbol++;
      }

      foreach (int posIndex in symPos)
      {
        Symbol.BaseSymbol symbol = OcdReader.ReadSymbol(posIndex);
        if (symbols == null || sortSymbols.BinarySearch(symbol.Number) >= 0)
        {
          WriteSymbolStatus(state, posIndex);
        }
      }
    }
  }
  #endregion
}
