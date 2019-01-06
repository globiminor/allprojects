using Basics.Geom;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Ocad
{
  internal abstract class OcadIo : IDisposable
  {
    #region nested classes
    public class ElementEnumerator : IEnumerator<Element>
    {
      private OcadIo _reader;
      private readonly bool _disposable;
      private Setup _setup;
      private Element _current;

      private IList<ElementIndex> _indexList; // List of ElementIndex

      private IBox _extent;
      private readonly bool _projected;
      private int _iRecord;

      public ElementEnumerator(OcadElemsInfo ocad, IBox extentIntersect, bool projected)
      {
        _reader = ocad.CreateReader().Io;
        _disposable = true;
        _projected = projected;
        _extent = extentIntersect;

        _indexList = ocad.GetIndexList();
        _setup = ocad.GetSetup();

        Init();
      }

      public ElementEnumerator(OcadIo ocad, IBox extentIntersect,
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
            if (!(_current.IsGeometryProjected == false)) throw new InvalidOperationException("Geometry is projected");
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

    private readonly EndianReader _reader;
    private readonly Stream _stream;

    private EndianWriter _writer;
    public EndianWriter Writer => _writer ?? (_writer = new EndianWriter(_reader.BaseStream));

    internal Stream BaseStream => _stream;
    internal EndianReader Reader => _reader;
    internal FileParam FileParam { get; private set; }
    internal SymbolData SymbolData { get; private set; }
    internal Setup Setup { get; private set; }


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

      index.Box.Min.X = Coord.GetGeomPart(_reader.ReadInt32());
      index.Box.Min.Y = Coord.GetGeomPart(_reader.ReadInt32());
      index.Box.Max.X = Coord.GetGeomPart(_reader.ReadInt32());
      index.Box.Max.Y = Coord.GetGeomPart(_reader.ReadInt32());

      return index;
    }

    public virtual Element ReadControlGeometry(Control control,
      IList<StringParamIndex> settingIndexList)
    {
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

            control.Element = ReadElement(strIdx.ElemNummer - 1);
            return control.Element;
          }
        }
      }
      return null;
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
      VerifyExists(elem, index);

      return elem;
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
      Coord coord = Coord.Create(index.Box.Min, 0);
      Writer.Write((int)coord.Ox);
      Writer.Write((int)coord.Oy);
      coord = Coord.Create(index.Box.Max, 0);
      Writer.Write((int)coord.Ox);
      Writer.Write((int)coord.Oy);
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

    public abstract Element ReadElement();

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
  public class OcadReader : IDisposable
  {
    private OcadIo _io;

    internal OcadReader(OcadIo io)
    {
      _io = io;
    }

    internal OcadIo Io => _io;
    internal Stream BaseStream => _io.BaseStream;

    public static OcadReader Open(Stream stream, int ocadVersion)
    {
      return new OcadReader(OcadIo.GetIo(stream, ocadVersion));
    }
    public static OcadReader Open(string name)
    {
      string ocdName = OcdName(name);
      return Open(new FileStream(ocdName, FileMode.Open, FileAccess.Read), 0);
    }

    public void Dispose()
    {
      Close();
      _io = null;
    }

    public Setup ReadSetup()
    {
      return _io.ReadSetup();
    }

    public Setup Setup => _io.Setup;

    protected static string OcdName(string name)
    {
      return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(name)),
        Path.GetFileNameWithoutExtension(name) + ".ocd");
    }

    public IList<ElementIndex> GetIndices()
    {
      return _io.GetIndices();
    }

    public ElementIndex ReadIndex(int i)
    {
      return _io.ReadIndex(i);
    }

    public Element ReadElement(ElementIndex index)
    {
      return _io.ReadElement(index);
    }
    public Element ReadElement(int index)
    {
      return _io.ReadElement(index);
    }
    public Element ReadElement()
    {
      return _io.ReadElement();
    }

    public IEnumerable<ColorInfo> ReadColorInfos()
    {
      return _io.ReadColorInfos();
    }
    public IEnumerable<Symbol.BaseSymbol> ReadSymbols()
    {
      return _io.ReadSymbols();
    }
    public int ReadSymbolPosition(int idx)
    {
      return _io.ReadSymbolPosition(idx);
    }
    public Symbol.BaseSymbol ReadSymbol(int position)
    {
      return _io.ReadSymbol(position);
    }

    public string ReadStringParam(StringParamIndex idx)
    {
      return _io.ReadStringParam(idx);
    }
    public IList<CategoryPar> GetCategories(IList<StringParamIndex> indexList = null)
    {
      IList<StringParamIndex> indexEnum = indexList ?? _io.EnumStringParamIndices().ToList();

      List<CategoryPar> catNames = new List<CategoryPar>();
      foreach (var index in indexEnum)
      {
        if (index.Type == StringType.Class)
        {
          CategoryPar candidate = new CategoryPar(_io.ReadStringParam(index));
          catNames.Add(candidate);
        }
      }
      return catNames;
    }

    public IList<string> GetCourseCategories(string courseName, IList<StringParamIndex> indexList = null)
    {
      IEnumerable<StringParamIndex> indexEnum = indexList ?? ReadStringParamIndices();
      List<string> catNames = new List<string>();
      foreach (var index in indexEnum)
      {
        if (index.Type == StringType.Class)
        {
          CategoryPar candidate = new CategoryPar(_io.ReadStringParam(index));
          if (candidate.CourseName == courseName)
          {
            catNames.Add(candidate.Name);
          }
        }

        if (index.Type == StringType.Course)
        {
          CoursePar candidate = new CoursePar(_io.ReadStringParam(index));
          if (candidate.Name == courseName)
          {
            catNames.Add(candidate.Name);
          }
        }
      }
      return catNames;
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

    public IEnumerable<Element> Elements(IBox extentIntersect, bool projected,
      IList<ElementIndex> indexList)
    {
      OcadIo.ElementEnumerator elements = new OcadIo.ElementEnumerator(_io,
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
      _io?.Close();
    }

    #region course setting

    public List<StringParamIndex> ReadStringParamIndices()
    {
      return new List<StringParamIndex>(_io.EnumStringParamIndices());
    }

    public IList<Control> ReadControls(IList<StringParamIndex> indexList = null)
    {
      IList<StringParamIndex> indexEnum = indexList ?? ReadStringParamIndices();

      Dictionary<Control, StringParamIndex> controls =
        new Dictionary<Control, StringParamIndex>();
      foreach (var index in indexEnum)
      {
        if (index.Type != StringType.Control)
        { continue; }

        string stringParam = _io.ReadStringParam(index);

        Control control = Control.FromStringParam(stringParam);

        controls.Add(control, index);
      }

      foreach (var pair in controls)
      {
        pair.Key.Element = _io.ReadElement(pair.Value.ElemNummer - 1);
      }
      IList<Control> result = new List<Control>(controls.Keys);
      return result;
    }

    public string ReadCourseName(int course, IList<StringParamIndex> indexList = null)
    {
      int iCourse;
      IList<StringParamIndex> indexEnum = indexList ?? ReadStringParamIndices();

      iCourse = 0;
      foreach (var index in indexList)
      {
        if (index.Type == StringType.Course)
        {
          if (iCourse == course)
          {
            return ReadCourseName(index);
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

      return _io.ReadStringParam(courseIndex, nTabs: 1);
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
        if (part[0] == (char)ControlCode.Start)
        { // start
          Control start = new Control();
          sectionList.AddLast(start);
          start.Name = part.Substring(1);
          start.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.Control)
        { // control
          Control control = new Control();
          sectionList.AddLast(control);
          control.Name = part.Substring(1);
          control.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.Finish)
        { // finish
          Control finish = new Control();
          sectionList.AddLast(finish);
          finish.Name = part.Substring(1);
          finish.Code = (ControlCode)part[0];
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
            foreach (var sLeg in sVariations.Split('-'))
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
        else if (part[0] == (char)ControlCode.MarkedRoute) // Pflichtstrecke
        {
          Control pflicht = new Control();
          sectionList.AddLast(pflicht);
          pflicht.Name = part.Substring(1);
          pflicht.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.TextBlock)
        {
          Control textBlock = new Control();
          sectionList.AddLast(textBlock);
          textBlock.Name = part.Substring(1);
          textBlock.Code = (ControlCode)part[0];
        }
        else if (part[0] == (char)ControlCode.MapChange)
        {
          Control mapExc = new Control();
          sectionList.AddLast(mapExc);
          mapExc.Name = part.Substring(1);
          mapExc.Code = (ControlCode)part[0];
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
      foreach (var section in sectionList)
      {
        if (section is Control)
        {
          _io.ReadControlGeometry((Control)section, indexList);
        }
        else if (section is SplitSection fork)
        {
          foreach (var branch in fork.Branches)
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

      foreach (var index in idxList)
      {
        if (index.Type == StringType.Course)
        {
          string courseName = ReadCourseName(index);
          Course course = ReadCourse(courseName, idxList);
          yield return course;
        }
      }
    }

    public Course ReadCourse(string courseName, IList<StringParamIndex> indexList = null)
    {
      if (courseName == null)
      { return null; }

      indexList = indexList ?? ReadStringParamIndices();
      foreach (var idx in indexList)
      {
        if (idx.Type == StringType.Course)
        {
          string sName = _io.ReadStringParam(idx, nTabs: 1);

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
      string courseParam = _io.ReadStringParam(courseIndex);
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
