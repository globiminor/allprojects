using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Basics.Geom;
using Ocad.StringParams;

namespace Ocad
{
  public abstract class FileParam
  {
    public const int OCAD_MARK = 3245;
    public const int HEADER_LENGTH = 48;
    public const int SYM_BLOCK_LENGTH = 24;
    public const int COLORINFO_LENGTH = 72;
    public const int MAX_COLORS = 256;
    public const int COLORSEP_LENGTH = 24;
    public const int MAX_SEPS = 32;
    public const int N_IN_SYMBOL_BLOCK = 256;
    public abstract int INDEX_LENGTH { get; }
    public const int N_IN_ELEM_BLOCK = 256;
    public const int N_IN_STRINGPARAM_BLOCK = 256;
    public const double OCAD_UNIT = 0.00001;

    private int _iSectionMark;
    private int _iVersion;
    private int _iSubVersion;
    private int _iFirstSymbolBlock;
    private int _iFirstIndexBlock;
    private int _iSetupPosition;
    private int _iSetupSize;
    private int _iInfoPosition;
    private int _iInfoSize;
    private int _iFirstStrIdxBlock;
    private int _iFileNamePosition;
    private int _iFileNameSize;

    public int SectionMark
    {
      get
      { return _iSectionMark; }
      set
      { _iSectionMark = value; }
    }

    public int Version
    {
      get
      { return _iVersion; }
      set
      { _iVersion = value; }
    }

    public int SubVersion
    {
      get
      { return _iSubVersion; }
      set
      { _iSubVersion = value; }
    }

    public int FirstSymbolBlock
    {
      get
      { return _iFirstSymbolBlock; }
      set
      { _iFirstSymbolBlock = value; }
    }

    public int FirstIndexBlock
    {
      get
      { return _iFirstIndexBlock; }
      set
      { _iFirstIndexBlock = value; }
    }

    public int SetupPosition
    {
      get
      { return _iSetupPosition; }
      set
      { _iSetupPosition = value; }
    }

    public int SetupSize
    {
      get { return _iSetupSize; }
      set { _iSetupSize = value; }
    }

    public int InfoPosition
    {
      get
      { return _iInfoPosition; }
      set
      { _iInfoPosition = value; }
    }

    public int InfoSize
    {
      get
      { return _iInfoSize; }
      set
      { _iInfoSize = value; }
    }

    public int FirstStrIdxBlock
    {
      get
      { return _iFirstStrIdxBlock; }
      set
      { _iFirstStrIdxBlock = value; }
    }

    public int FileNamePosition
    {
      get
      { return _iFileNamePosition; }
      set
      { _iFileNamePosition = value; }
    }

    public int FileNameSize
    {
      get
      { return _iFileNameSize; }
      set
      { _iFileNameSize = value; }
    }

    public override string ToString()
    {
      return string.Format(
        "OCAD Mark    : {0,5}\n" +
        "Section Mark : {1,5}\n" +
        "Version      : {2,5}\n" +
        "Subversion   : {3,5}\n" +
        "Start Symbol Block : {4,9}\n" +
        "Start Index Block  : {5,9}\n" +
        "Setup Position     : {6,9}\n" +
        "Setup Size         : {7,9}\n" +
        "Info Position      : {8,9}\n" +
        "Info Size          : {9,9}\n" +
        "Start String Index : {10,9}", OCAD_MARK, _iSectionMark, _iVersion,
        _iSubVersion, _iFirstSymbolBlock, _iFirstIndexBlock, _iSetupPosition,
        _iSetupSize, _iInfoPosition, _iInfoSize, _iFirstStrIdxBlock);
    }
  }


  public class FileParamV8 : FileParam
  {
    public override int INDEX_LENGTH
    {
      get
      { return 24; }
    }
  }

  public class FileParamV9 : FileParam
  {
    public override int INDEX_LENGTH
    {
      get
      { return 40; }
    }
  }

  public class SymbolData
  {
    private int _iColorCount;
    private int _iColorSepCount;
    private int _iCyanFreq;
    private int _iCyanAngle;
    private int _iMagentaFreq;
    private int _iMagentaAngle;
    private int _iYellowFreq;
    private int _iYellowAngle;
    private int _iBlackFreq;
    private int _iBlackAngle;

    public int ColorCount
    {
      get
      { return _iColorCount; }
      set
      { _iColorCount = value; }
    }

    public int ColorSeparationCount
    {
      get
      { return _iColorSepCount; }
      set
      { _iColorSepCount = value; }
    }

    public int CyanFreq
    {
      get
      { return _iCyanFreq; }
      set
      { _iCyanFreq = value; }
    }

    public int CyanAngle
    {
      get
      { return _iCyanAngle; }
      set
      { _iCyanAngle = value; }
    }

    public int MagentaFreq
    {
      get
      { return _iMagentaFreq; }
      set
      { _iMagentaFreq = value; }
    }

    public int MagentaAngle
    {
      get
      { return _iMagentaAngle; }
      set
      { _iMagentaAngle = value; }
    }

    public int YellowFreq
    {
      get
      { return _iYellowFreq; }
      set
      { _iYellowFreq = value; }
    }

    public int YellowAngle
    {
      get
      { return _iYellowAngle; }
      set
      { _iYellowAngle = value; }
    }

    public int BlackFreq
    {
      get
      { return _iBlackFreq; }
      set
      { _iBlackFreq = value; }
    }

    public int BlackAngle
    {
      get
      { return _iBlackAngle; }
      set
      { _iBlackAngle = value; }
    }

    public override string ToString()
    {
      return string.Format(
        "Number of Colors      : {0,5}\n" +
        "Number of Separations : {1,5}\n\n" +
        " Cyan Frequncy        : {2,5}\n" +
        " Cyan Angle           : {3,5}\n" +
        " Magenta Frequency    : {4,5}\n" +
        " Magenta Angle        : {5,5}\n" +
        " Yellow Frequency     : {6,5}\n" +
        " Yellow Angle         : {7,5}\n" +
        " Black Frequency      : {8,5}\n" +
        " Black Angle          : {9,5}", _iColorCount, _iColorSepCount,
        _iCyanFreq, _iCyanAngle, _iMagentaFreq, _iMagentaAngle,
        _iYellowFreq, _iYellowAngle, _iBlackFreq, _iBlackAngle);
    }
  }

  internal class StringIndexEventArgs
  {
    StringParamIndex _index;
    bool _cancel;

    public StringIndexEventArgs(StringParamIndex index)
    {
      _index = index;
      _cancel = false;
    }
    public StringParamIndex Index
    {
      get
      { return _index; }
    }
    public bool Cancel
    {
      get
      { return _cancel; }
      set
      { _cancel = value; }
    }
  }
  internal delegate void StringIndexEventHandler(object sender, StringIndexEventArgs args);
  /// <summary>
  /// Summary description for OcadReader.
  /// </summary>
  public abstract class OcadReader : IDisposable
  {
    #region nested classes
    public class ElementEnumerator : IEnumerator<Element>
    {
      private OcadReader _reader;
      private bool _disposable;
      private Setup _setup;
      private Element _current;

      private IList<ElementIndex> _indexList; // List of ElementIndex

      private IBox _extent;
      private bool _projected;
      private int _iRecord;

      public ElementEnumerator(OcadElemsInfo ocad, IBox extentIntersect, bool projected)
      {
        _reader = ocad.CreateReader();
        _disposable = true;
        _projected = projected;
        _extent = extentIntersect;

        _indexList = ocad.GetIndexList();
        _setup = ocad.GetSetup();

        Init();
      }

      public ElementEnumerator(OcadReader ocad, IBox extentIntersect,
        bool projected, IList<ElementIndex> indexList)
      {
        _reader = ocad;
        _projected = projected;
        _extent = extentIntersect;

        _indexList = indexList;

        _setup = _reader.ReadSetup();

        Init();
      }
      #region IEnumerator Members

      public void Dispose()
      {
        if (_disposable && _reader != null)
        {
          _reader.Close();
          _reader.Dispose();
          _reader = null;
        }
      }

      public void Reset()
      {
        _iRecord = 0;
      }

      public Element Current
      {
        get
        { return _current; }
      }
      object System.Collections.IEnumerator.Current
      { get { return _current; } }

      public bool MoveNext()
      {
        do
        {
          ElementIndex elemIndex = null;

          if (_indexList == null)
          { elemIndex = _reader.ReadIndex(_iRecord); }
          else if (_iRecord < _indexList.Count)
          { elemIndex = _indexList[_iRecord]; }

          if (elemIndex == null)
          { return false; }

          _current = null;
          if (elemIndex.Status != ElementIndex.StatusDeleted &&
            (_extent == null || elemIndex.Box.Intersects(_extent)))
          {
            _current = _reader.ReadElement(elemIndex);
            if (_current == null)
            { return false; }
            _current.Index = elemIndex.Index;
            if (_current.Geometry == null) { }
            Debug.Assert(_current.IsGeometryProjected == false);
          }
          _iRecord++;
        } while (_current == null || _current.Symbol == 0); // Deleted Element

        if (_projected && _current.Geometry != null)
        {
          _current.Geometry = _current.Geometry.Project(_setup.Map2Prj);
          _current.IsGeometryProjected = true;
        }

        return true;
      }

      #endregion

      private void Init()
      {
        if (_projected && _extent != null)
        {
          _extent = _extent.Project(_setup.Prj2Map).Extent;
        }

        Reset();
      }
    }
    #endregion

    internal event StringIndexEventHandler StringIndexRead;

    private EndianReader _reader;
    protected FileParam _fileParam;
    protected SymbolData _symbol;

    internal EndianReader BaseReader
    {
      get { return _reader; }
    }
    public static OcadReader Open(Stream stream)
    {
      int iSectionMark;
      int iVersion;
      EndianReader reader = new EndianReader(stream, Encoding.UTF7);
      Version(reader, out iSectionMark, out iVersion);
      OcadReader ocdReader = Reader(iVersion);
      ocdReader._reader = reader;
      ocdReader.Init(iSectionMark, iVersion);
      return ocdReader;
    }
    public static OcadReader Open(string name)
    {
      string ocdName = OcdName(name);
      EncodingInfo[] codings = Encoding.GetEncodings();
      EndianReader reader = new EndianReader(
        new FileStream(ocdName, FileMode.Open, FileAccess.Read), Encoding.UTF7);

      int iSectionMark;
      int iVersion;
      Version(reader, out iSectionMark, out iVersion);
      OcadReader ocdReader = Reader(iVersion);
      ocdReader._reader = reader;
      ocdReader.Init(iSectionMark, iVersion);
      ocdReader.ReadSetup();
      return ocdReader;
    }

    protected OcadReader(Stream ocadStream)
    {
      _reader = new EndianReader(ocadStream);
    }
    protected OcadReader(Stream ocadStream, Encoding encoding)
    {
      _reader = new EndianReader(ocadStream, encoding);
    }

    internal protected OcadReader()
    { }
    public void Dispose()
    {
      Close();
      _reader = null;
    }
    protected static void Version(EndianReader pReader, out int sectionMark, out int version)
    {
      pReader.BaseStream.Seek(0, SeekOrigin.Begin);

      if (pReader.ReadInt16() != FileParam.OCAD_MARK)
      { throw new Exception("Invalid OCAD File"); }

      sectionMark = pReader.ReadInt16();
      version = pReader.ReadInt16();
    }
    private static OcadReader Reader(int version)
    {
      OcadReader reader;
      if (version == 6 || version == 7 || version == 8)
      { reader = new Ocad8Reader(); }
      else if (version == 9 || version == 10)
      { reader = new Ocad9Reader(); }
      else if (version == 11)
      { reader = new Ocad11Reader(); }
      else
      { throw new Exception("Invalid OCAD File"); }

      return reader;
    }

    protected abstract FileParam Init(int sectionMark, int version);
    public abstract Setup ReadSetup();

    protected static string OcdName(string name)
    {
      return Path.GetDirectoryName(Path.GetFullPath(name)) +
        Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".ocd";
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

    public static bool Exists(string name)
    {
      string ext = Path.GetExtension(name);
      if (ext != "" && ext != ".ocd")
      { return false; }

      if (File.Exists(OcdName(name)) == false)
      { return false; }

      return true;
    }

    public FileParam FileParam
    {
      get
      { return _fileParam; }
    }

    public IEnumerable<Element> Elements(IBox extentIntersect, bool projected,
      IList<ElementIndex> indexList)
    {
      ElementEnumerator elements = new ElementEnumerator(this,
        extentIntersect, projected, indexList);

      while (elements.MoveNext())
      {
        yield return elements.Current;
      }
    }

    public IEnumerable<Element> Elements(bool projected, IList<ElementIndex> indexList)
    {
      return Elements(null, projected, indexList);
    }

    public void Close()
    {
      if (_reader != null)
      {
        _reader.Close();
      }
    }

    internal static int FLength(Element element)
    {
      return 32 + element.PointCount() * 8 + element.Text.Length;
    }

    public int ReadSymbolPosition(int i)
    {
      int p = _fileParam.FirstSymbolBlock;
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

    public abstract ElementIndex ReadIndex(int i);

    List<int> _indexBlockStarts;

    internal bool SeekIndex(int id)
    {
      if (id < 0)
      { return false; }
      if (_indexBlockStarts == null)
      {
        _indexBlockStarts = new List<int>();
        _indexBlockStarts.Add(_fileParam.FirstIndexBlock);
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

      _reader.BaseStream.Seek(_fileParam.INDEX_LENGTH * blockPos + p + 4, SeekOrigin.Begin);
      return true;
    }

    protected ElementIndex ReadIndexPart(int id)
    {
      if (SeekIndex(id) == false)
      { return null; }

      ElementIndex index = new ElementIndex(id);

      index.Box.Min.X = Coord.GetGeomPart(_reader.ReadInt32());
      index.Box.Min.Y = Coord.GetGeomPart(_reader.ReadInt32());
      index.Box.Max.X = Coord.GetGeomPart(_reader.ReadInt32());
      index.Box.Max.Y = Coord.GetGeomPart(_reader.ReadInt32());

      return index;
    }

    public Element ReadElement(int i)
    {
      return ReadElement(ReadIndex(i));
    }

    public Element ReadElement(ElementIndex index)
    {
      if (index == null)
      { return null; }
      if (index.Position == 0)
      { return null; }

      _reader.BaseStream.Seek(index.Position, SeekOrigin.Begin);
      Element elem = ReadElement();
      if (this is Ocad8Reader && index.Symbol == 0)
      {
        elem.DeleteSymbol = elem.Symbol;
        elem.Symbol = 0;
      }
      return elem;
    }

    public abstract Element ReadElement();

    protected IGeometry ReadGeometry(GeomType type, int nPoint)
    {
      Coord pCoord = ReadCoord();

      if (type == GeomType.point || type == GeomType.unformattedText ||
        type == GeomType.formattedText)
      {
        if (nPoint == 1)
        { return pCoord.GetPoint(); }
        else
        {
          PointCollection pPoints = new PointCollection();
          pPoints.Add(pCoord.GetPoint());
          for (int iPoint = 1; iPoint < nPoint; iPoint++)
          { pPoints.Add(ReadCoord().GetPoint()); }
          return pPoints;
        }
      }
      else if (type == GeomType.line || type == GeomType.lineText)
      {
        Polyline pLine = ReadPolyline(ref pCoord, ref nPoint);
        Debug.Assert(nPoint == 0);
        return pLine;
      }
      else if (type == GeomType.rectangle)
      {
        Polyline pLine = ReadPolyline(ref pCoord, ref nPoint);
        Debug.Assert(nPoint == 0);
        return pLine;
      }
      else if (type == GeomType.area)
      {
        Area pArea = new Area(ReadPolyline(ref pCoord, ref nPoint));
        while (nPoint > 0) // read holes / inner rings
        { pArea.Border.Add(ReadPolyline(ref pCoord, ref nPoint)); }
        return pArea;
      }
      else
      { // unknown geometry type
        for (int iPoint = 1; iPoint < nPoint; iPoint++)
        { ReadCoord(); }
        return null;
      }
    }

    protected Polyline ReadPolyline(ref Coord pCoord, ref int nPoint)
    {
      int iPoint = 1;
      Point p0;
      Polyline pLine = new Polyline();

      p0 = pCoord.GetPoint();
      while (iPoint < nPoint)
      {
        Curve pSeg;
        Point p1;

        Coord pCoord1 = ReadCoord();
        if (pCoord1.IsNewRing)
        {
          pCoord = pCoord1;
          break;
        }

        iPoint++;
        if (pCoord1.IsBezier)
        {
          Coord pCoord2 = ReadCoord();
          iPoint++;
          Coord pCoord3 = ReadCoord();
          iPoint++;
          p1 = pCoord3.GetPoint();
          pSeg = new Bezier(p0, pCoord1.GetPoint(),
            pCoord2.GetPoint(), p1);

          pCoord = pCoord3;
        }
        else
        {
          p1 = pCoord1.GetPoint();
          pSeg = new Line(p0, p1);

          pCoord = pCoord1;
        }
        pLine.Add(pSeg);

        p0 = p1;
      }

      nPoint -= iPoint;
      return pLine;
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


    public abstract IEnumerable<ColorInfo> ReadColorInfos();

    public ColorSeparation ReadColorSeparation(int index)
    {
      if (index >= _symbol.ColorSeparationCount)
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

    protected virtual SymbolData ReadSymbolData()
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

      foreach (int posIndex in symPos)
      {
        Symbol.BaseSymbol symbol = ReadSymbol(posIndex);
        yield return symbol;
      }
    }

    #region course setting

    protected string ReadTab(int max, out char endChar)
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

    public string ReadStringParam(StringParamIndex index)
    {
      _reader.BaseStream.Seek(index.FilePosition, SeekOrigin.Begin);
      char e1;
      string sResult = ReadTab(255, out e1);

      int max = index.Size;
      char e0 = e1;
      string sNext = ReadTab(255, out e1);
      while (sNext != "")
      {
        if (sResult.Length + 1 + sNext.Length > max)
        {
          break;
        }
        sResult = sResult + e0 + sNext;
        e0 = e1;
        if (e0 == 0)
        {
          break;
        }
        sNext = ReadTab(255, out e1);
      }
      return sResult;
    }

    public IList<StringParamIndex> ReadStringParamIndices()
    {
      int iNextStrIdxBlk;
      List<StringParamIndex> indexList;

      if (this is Ocad8Reader && _fileParam.SectionMark != 3)
      {
        return null;
      }
      _reader.BaseStream.Seek(_fileParam.FirstStrIdxBlock, SeekOrigin.Begin);
      indexList = new List<StringParamIndex>();

      do
      {
        iNextStrIdxBlk = _reader.ReadInt32();
        int j = 0;
        StringParamIndex index;
        do
        {
          index = new StringParamIndex();
          index.IndexPosition = _reader.BaseStream.Position;
          index.FilePosition = _reader.ReadInt32();
          index.Size = _reader.ReadInt32();
          index.Type = (StringType)_reader.ReadInt32();
          index.ElemNummer = _reader.ReadInt32();

          indexList.Add(index);

          if (StringIndexRead != null)
          {
            StringIndexEventArgs args = new StringIndexEventArgs(index);
            StringIndexRead(this, args);
            if (args.Cancel)
            { return indexList; }
          }

          j++;
        }
        while (index.FilePosition > 0 && j < FileParam.N_IN_STRINGPARAM_BLOCK);

        if (iNextStrIdxBlk > 0)
        {
          _reader.BaseStream.Seek(iNextStrIdxBlk, SeekOrigin.Begin);
        }
      }
      while (iNextStrIdxBlk > 0);

      return indexList;
    }

    public string ReadCourseTemplateName()
    {
      if (_fileParam.SectionMark != 3)
      {
        return null;
      }
      _reader.BaseStream.Seek(_fileParam.FirstStrIdxBlock + 4, SeekOrigin.Begin);
      _reader.BaseStream.Seek(_reader.ReadInt32(), SeekOrigin.Begin);

      char e;
      return ReadTab(1024, out e);
    }

    public Element ReadControlGeometry(Control control,
      IList<StringParamIndex> settingIndexList)
    {
      foreach (StringParamIndex strIdx in settingIndexList)
      {
        if (strIdx.Type == StringType.Control)
        {
          _reader.BaseStream.Seek(strIdx.FilePosition, SeekOrigin.Begin);
          char e;
          string name = ReadTab(control.Name.Length + 1, out e);
          if (name == control.Name)
          {
            string stringParam = ReadStringParam(strIdx);
            ControlPar ctrPar = new ControlPar(stringParam);
            control.Text = ctrPar.TextDesc;

            control.Element = ReadElement(strIdx.ElemNummer - 1);
            return control.Element;
          }
        }
      }
      return null;
    }

    public IList<Control> ReadControls()
    {
      IList<StringParamIndex> indexList = ReadStringParamIndices();
      return ReadControls(indexList);
    }
    public IList<Control> ReadControls(IList<StringParamIndex> indexList)
    {
      Dictionary<Control, StringParamIndex> controls =
        new Dictionary<Control, StringParamIndex>();
      foreach (StringParamIndex index in indexList)
      {
        if (index.Type != StringType.Control)
        { continue; }

        string stringParam = ReadStringParam(index);

        Control control = Control.FromStringParam(stringParam);

        controls.Add(control, index);
      }

      foreach (KeyValuePair<Control, StringParamIndex> pair in controls)
      {
        pair.Key.Element = ReadElement(pair.Value.ElemNummer - 1);
      }
      IList<Control> result = new List<Control>(controls.Keys);
      return result;
    }

    public string ReadCourseName(int course)
    {
      int iCourse;
      IList<StringParamIndex> pList = ReadStringParamIndices();
      if (pList == null)
      { return null; }

      iCourse = 0;
      foreach (StringParamIndex pIndex in pList)
      {
        if (pIndex.Type == StringType.Course)
        {
          if (iCourse == course)
          {
            return ReadCourseName(pIndex);
          }
          iCourse++;
        }
      }
      return null;
    }

    public string ReadCourseName(StringParamIndex courseIndex)
    {
      if (courseIndex.Type != StringType.Course)
      {
        throw new ArgumentException("Invalid stringparameter type " + courseIndex.Type +
        ". Expected " + StringType.Course);
      }

      _reader.BaseStream.Seek(courseIndex.FilePosition, SeekOrigin.Begin);
      char e;
      return ReadTab(256, out e);
    }

    protected static SectionCollection ReadSectionList(IList<string> parts, ref int partIdx)
    {
      SectionCollection sectionList;
      string part;

      part = parts[partIdx];
      partIdx++;

      sectionList = new SectionCollection();
      while (string.IsNullOrEmpty(part) == false &&
        part[0] != CoursePar.ForkStartKey && part[0] != CoursePar.VarStartKey &&
        part[0] != CoursePar.ForkEndKey && part[0] != CoursePar.VarEndKey)
      {
        if (part[0] == CoursePar.StartKey)
        { // start
          Control start = new Control();
          sectionList.AddLast(start);
          start.Name = part.Substring(1);
          start.Code = part[0];
        }
        else if (part[0] == CoursePar.ControlKey)
        { // control
          Control control = new Control();
          sectionList.AddLast(control);
          control.Name = part.Substring(1);
          control.Code = part[0];
        }
        else if (part[0] == CoursePar.FinishKey)
        { // finish
          Control finish = new Control();
          sectionList.AddLast(finish);
          finish.Name = part.Substring(1);
          finish.Code = part[0];
        }
        else if (part[0] == CoursePar.VariationKey)
        { // variations
          Variation variation = new Variation();
          sectionList.AddLast(variation);

          part = parts[partIdx];
          partIdx++;
          while (string.IsNullOrEmpty(part) == false && part[0] == CoursePar.VarStartKey)
          {
            string sVariations = part.Substring(1);
            List<int> legs = new List<int>();
            foreach (string sLeg in sVariations.Split('-'))
            {
              legs.Add(Convert.ToInt32(sLeg));
            }
            Variation.Branch branch =
              new Variation.Branch(legs, ReadSectionList(parts, ref partIdx));
            part = (partIdx - 1 < parts.Count) ? parts[partIdx - 1] : null;

            variation.Branches.Add(branch);
          }
          if (string.IsNullOrEmpty(part) || part[0] != CoursePar.VarEndKey)
          { throw new Exception("No variation end " + part); }
        }
        else if (part[0] == CoursePar.ForkKey)
        { // forks
          int nForks;
          if (Int32.TryParse(part.Substring(1), out nForks) == false)
          {
            nForks = -1;
            Debug.Assert(false, "Expected Tab r<number of forks>, got " + part);
          }
          part = parts[partIdx];
          partIdx++;
          Fork fork = new Fork();
          sectionList.AddLast(fork);
          while (string.IsNullOrEmpty(part) == false && part[0] == CoursePar.ForkStartKey)
          {
            fork.Branches.Add(new Fork.Branch(ReadSectionList(parts, ref partIdx)));
            part = (partIdx - 1 < parts.Count) ? parts[partIdx - 1] : null;
          }
          if (string.IsNullOrEmpty(part) || part[0] != CoursePar.ForkEndKey)
          { throw new Exception("No fork end " + part); }
          Debug.Assert(nForks < 0 || (nForks == fork.Branches.Count),
            string.Format("Expected {0} branches, got {1}", nForks, fork.Branches.Count));
        }
        else if (part[0] == CoursePar.MarkedRouteKey) // Pflichtstrecke
        {
          Control pflicht = new Control();
          sectionList.AddLast(pflicht);
          pflicht.Name = part.Substring(1);
          pflicht.Code = part[0];
        }
        else if (part[0] == CoursePar.TextBlockKey)
        {
          Control textBlock = new Control();
          sectionList.AddLast(textBlock);
          textBlock.Name = part.Substring(1);
          textBlock.Code = part[0];
        }
        else if (part[0] == CoursePar.MapChangeKey)
        {
          Control mapExc = new Control();
          sectionList.AddLast(mapExc);
          mapExc.Name = part.Substring(1);
          mapExc.Code = part[0];
        }
        else if (part[0] == CoursePar.ClimbKey)
        { }
        else if (part[0] == CoursePar.ExtraDistanceKey)
        { }
        else if (part[0] == CoursePar.FromStartNumberKey)
        { }
        else if (part[0] == CoursePar.LegCountKey)
        { }
        else if (part[0] == CoursePar.RunnersCountKey)
        { }
        else if (part[0] == CoursePar.ToStartNumberKey)
        { }
        else if (part[0] == CoursePar.CourseTypeKey) // course type
        { }
        else
        { // something unexpected between start and finish
          throw new Exception("Unexpected Tab " + part +
            " (Last successfull Tab: " + parts[partIdx - 2] + ")");
        }
        if (partIdx < parts.Count)
        {
          part = parts[partIdx];
          partIdx++;
        }
        else
        {
          part = null;
        }
      }
      return sectionList;
    }

    private void ReadSectionGeometry(SectionCollection sectionList,
      IList<StringParamIndex> indexList)
    {
      foreach (ISection section in sectionList)
      {
        if (section is Control)
        {
          ReadControlGeometry((Control)section, indexList);
        }
        else if (section is SplitSection)
        {
          SplitSection fork = (SplitSection)section;
          foreach (SplitSection.Branch branch in fork.Branches)
          { ReadSectionGeometry(branch, indexList); }
        }
        else
        {
          throw new ArgumentException("Unhandled section type " + section.GetType());
        }
      }
    }

    public IEnumerable<Course> ReadCourses()
    {
      IList<StringParamIndex> idxList = ReadStringParamIndices();

      foreach (StringParamIndex index in idxList)
      {
        if (index.Type == StringType.Course)
        {
          string courseName = ReadCourseName(index);
          Course course = ReadCourse(courseName);
          yield return course;
        }
      }
    }

    public Course ReadCourse(string courseName)
    {
      if (courseName == null)
      { return null; }

      IList<StringParamIndex> pIndexList = ReadStringParamIndices();

      return ReadCourse(courseName, pIndexList);
    }

    public Course ReadCourse(string courseName, IList<StringParamIndex> indexList)
    {
      foreach (StringParamIndex idx in indexList)
      {
        if (idx.Type == StringType.Course)
        {
          _reader.BaseStream.Seek(idx.FilePosition, SeekOrigin.Begin);
          char e;
          string sName = ReadTab(courseName.Length + 2, out e);

          if (sName == courseName)
          {
            return ReadCourse(idx, indexList);
          }
        }
      }

      return null;

    }

    public Course ReadCourse(StringParamIndex courseIndex, IList<StringParamIndex> indexList)
    {
      string courseParam = ReadStringParam(courseIndex);
      IList<string> parts = courseParam.Split('\t');

      Course course = ReadCourse(parts);

      ReadSectionGeometry(course, indexList);

      return course;
    }

    private static Course ReadCourse(IList<string> parts)
    {
      string name;

      int nParts = parts.Count;
      name = parts[0];

      Course course = new Course(name);

      // look for meta data
      for (int iPart = 1; iPart < nParts; iPart++)
      {
        name = parts[iPart];

        if (name[0] == CoursePar.ClimbKey)
        { course.Climb = Convert.ToDouble(name.Substring(1)); }
        else if (name[0] == CoursePar.ExtraDistanceKey)
        { course.ExtraDistance = Convert.ToDouble(name.Substring(1)); }
      }


      // read course
      {
        int iPart = 1;
        course.AddLast(ReadSectionList(parts, ref iPart));
      }

      course.AssignLegs();

      return course;
    }

    #endregion
  }

  #region OCAD8
  public class Ocad8Reader : OcadReader
  {
    internal Ocad8Reader()
    { }
    public Ocad8Reader(Stream ocadStream)
      : base(ocadStream)
    {
      int iSectionMark;
      int iVersion;
      Version(BaseReader, out iSectionMark, out iVersion);
      if (iVersion != 6 && iVersion != 7 && iVersion != 8)
      { throw (new Exception(string.Format("Invalid Version {0}", iVersion))); }

      Init(iSectionMark, iVersion);
    }

    protected sealed override FileParam Init(int sectionMark, int version)
    {
      _fileParam = new FileParamV8();

      _fileParam.SectionMark = sectionMark;
      _fileParam.Version = version;
      _fileParam.SubVersion = BaseReader.ReadInt16();
      _fileParam.FirstSymbolBlock = BaseReader.ReadInt32();
      _fileParam.FirstIndexBlock = BaseReader.ReadInt32();
      _fileParam.SetupPosition = BaseReader.ReadInt32();
      _fileParam.SetupSize = BaseReader.ReadInt32();
      _fileParam.InfoPosition = BaseReader.ReadInt32();
      _fileParam.InfoSize = BaseReader.ReadInt32();
      _fileParam.FirstStrIdxBlock = BaseReader.ReadInt32();
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
      index.Length = BaseReader.ReadInt16();
      index.Symbol = BaseReader.ReadInt16();

      return index;
    }


    public override Element ReadElement()
    {
      int nPoint;
      int nText;
      ElementV8 elem = new ElementV8(false);

      elem.Symbol = BaseReader.ReadInt16();
      elem.Type = (GeomType)BaseReader.ReadByte();
      elem.UnicodeText = BaseReader.ReadByte() != 0;
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
  }
  #endregion


  #region OCAD9
  public class Ocad9Reader : OcadReader
  {
    internal Ocad9Reader()
    { }
    private void Init()
    {
      int iSectionMark;
      int iVersion;
      Version(BaseReader, out iSectionMark, out iVersion);
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
      _fileParam = new FileParamV9();

      _fileParam.SectionMark = sectionMark;
      _fileParam.Version = version;
      _fileParam.SubVersion = BaseReader.ReadInt16();
      _fileParam.FirstSymbolBlock = BaseReader.ReadInt32();
      _fileParam.FirstIndexBlock = BaseReader.ReadInt32();
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
            char e1;
            ReadTab(255, out e1);

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
      index.Length = BaseReader.ReadInt32();
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

    public override Element ReadElement()
    {
      int nPoint;
      int nText;
      ElementV9 elem = new ElementV9(false);
      elem.Symbol = BaseReader.ReadInt32();
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

      elem.Geometry = ReadGeometry(elem.Type, nPoint);
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
        int iNData;
        Symbol.SymbolGraphics pElem = ReadSymbolGraphics(out iNData);
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

    private void ReadAreaSymbol(Symbol.AreaSymbol symbol)
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
      Symbol.SymbolGraphics elem = new Symbol.SymbolGraphics();
      elem.Type = (Symbol.SymbolGraphicsType)BaseReader.ReadInt16();
      int iFlags = BaseReader.ReadInt16();
      elem.RoundEnds = ((iFlags & 1) == 1);
      elem.Color = BaseReader.ReadInt16();
      elem.LineWidth = BaseReader.ReadInt16();

      int iDiameter = BaseReader.ReadInt16();
      int iNPoints = BaseReader.ReadInt16();

      BaseReader.ReadInt16(); // Reserved
      BaseReader.ReadInt16(); // Reserved

      nData = 2 + iNPoints;

      if (elem.Type == Symbol.SymbolGraphicsType.Line)
      { elem.Geometry = ReadGeometry(GeomType.line, iNPoints); }
      else if (elem.Type == Symbol.SymbolGraphicsType.Area)
      { elem.Geometry = ReadGeometry(GeomType.area, iNPoints); }
      else if (elem.Type == Symbol.SymbolGraphicsType.Circle ||
        elem.Type == Symbol.SymbolGraphicsType.Dot)
      {
        Debug.Assert(iNPoints == 1);
        Point pCenter = (Point)ReadGeometry(GeomType.point, iNPoints);
        Arc pArc = new Arc(pCenter, 0.5 * iDiameter, 0, 2 * Math.PI);
        Polyline pCircle = new Polyline();
        pCircle.Add(pArc);
        if (elem.Type == Symbol.SymbolGraphicsType.Circle)
        { elem.Geometry = pCircle; }
        else if (elem.Type == Symbol.SymbolGraphicsType.Dot)
        { elem.Geometry = new Area(pCircle); }
        else
        { throw new NotImplementedException(elem.Type.ToString()); }
      }
      else
      { throw new NotImplementedException(elem.Type.ToString()); }

      return elem;
    }
  }
  #endregion

  #region OCAD11
  public class Ocad11Reader : Ocad9Reader
  {
    public override Symbol.BaseSymbol ReadSymbol()
    {
      Symbol.BaseSymbol symbol = ReadSymbolRoot();

      //SymbolTreeGroup
      for (int i = 0; i < 64; i++)
      {
        BaseReader.ReadInt16();
      }

      ReadSymbolSpez(symbol);

      return symbol;
    }
    protected override string ReadDescription()
    {
      byte[] pByte = BaseReader.ReadBytes(128);

      // OCAD 11
      Encoding enc = new UnicodeEncoding();
      string read = enc.GetString(pByte).Trim('\0');
      return read;
    }
  }

  #endregion
}
