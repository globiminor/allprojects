using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Basics.Geom;
using Ocad.StringParams;

namespace Ocad
{
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
      EndianReader reader = new EndianReader(stream, Encoding.UTF7);
      Version(reader, out int iSectionMark, out int iVersion);
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

      Version(reader, out int iSectionMark, out int iVersion);
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
      else if (version == 12)
      { reader = new Ocad12Reader(); }
      else
      { throw new Exception("Invalid OCAD File"); }

      return reader;
    }

    protected abstract FileParam Init(int sectionMark, int version);
    public abstract Setup ReadSetup();

    public abstract void WriteElementHeader(EndianWriter writer, Element element);
    public abstract void WriteElementContent(EndianWriter writer, Element element);
    public abstract void WriteElementSymbol(EndianWriter writer, int symbol);
    public abstract int ReadElementLength();
    public abstract int CalcElementLength(Element element);

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
        _indexBlockStarts = new List<int>
        {          _fileParam.FirstIndexBlock        };
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
          PointCollection pPoints = new PointCollection
          { pCoord.GetPoint() };
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
      string sResult = ReadTab(255, out char e1);

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
          index = new StringParamIndex
          {
            IndexPosition = _reader.BaseStream.Position,
            FilePosition = _reader.ReadInt32(),
            Size = _reader.ReadInt32(),
            Type = (StringType)_reader.ReadInt32(),
            ElemNummer = _reader.ReadInt32()
          };

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

      return ReadTab(1024, out char e);
    }

    public virtual Element ReadControlGeometry(Control control,
      IList<StringParamIndex> settingIndexList)
    {
      foreach (StringParamIndex strIdx in settingIndexList)
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
      return ReadTab(256, out char e);
    }

    protected static SectionCollection ReadSectionList(IList<string> parts, ref int partIdx)
    {
      SectionCollection sectionList;
      string part;

      if (parts.Count <= partIdx)
      { return new SectionCollection(); }

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
          if (Int32.TryParse(part.Substring(1), out int nForks) == false)
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
        else if (section is SplitSection fork)
        {
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
          string sName = ReadTab(courseName.Length + 2, out char e);

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
}
