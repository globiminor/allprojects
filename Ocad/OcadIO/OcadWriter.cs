using Basics.Geom;
using Ocad.StringParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// ReSharper disable RedundantCast

namespace Ocad
{
  /// <summary>
  /// Summary description for OcadWriter.
  /// </summary>
  public sealed class OcadWriter : OcadReader
  {
    public delegate T ElementIndexDlg<T>(ElementIndex element);

    private OcadWriter(Stream ocadStream, int ocadVersion = 0)
      : base(OcadIo.GetIo(ocadStream, ocadVersion))
    { }

    public void Overwrite(Element element, int index)
    {
      ElementIndex idx = Io.GetIndex(element);
      idx.Position = (int)BaseStream.Seek(0, SeekOrigin.End);

      if (!Io.SeekIndex(index))
      { throw new InvalidOperationException("Invalid Index " + index); }
      Io.Write(idx);

      BaseStream.Seek(idx.Position, SeekOrigin.Begin);
      WriteElement(element);
    }
    /// <summary>
    /// assumption : index has all values set except for pos
    /// </summary>
    /// <param name="index"></param>
    private int Append(ElementIndex index)
    {
      int i;
      long p = Io.FileParam.FirstIndexBlock;
      long p0 = p;

      int indexNr = 0;

      while (p > 0)
      {
        p0 = p;
        BaseStream.Seek(p, SeekOrigin.Begin);
        p = Io.Reader.ReadInt32();

        if (p > 0)
        {
          indexNr += FileParam.N_IN_ELEM_BLOCK;
        }
      }
      if (p0 > 0)
      {
        p = p0 + 4;
        BaseStream.Seek(p + 16, SeekOrigin.Begin); /* index->pos */
        i = 0;
        while (Io.Reader.ReadInt32() != 0 && i < FileParam.N_IN_ELEM_BLOCK)
        {
          p += Io.FileParam.INDEX_LENGTH;
          BaseStream.Seek(p + 16, SeekOrigin.Begin);
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
        BaseStream.Seek(0, SeekOrigin.End);
        p = BaseStream.Position;
        Io.Writer.Write((int)0);
        int n = FileParam.N_IN_ELEM_BLOCK * Io.FileParam.INDEX_LENGTH;
        for (int j = 0; j < n; j++)
        { Io.Writer.Write((byte)0); }
        if (p0 > 0)
        {
          BaseStream.Seek(p0, SeekOrigin.Begin);
        }
        else
        {
          BaseStream.Seek(12, SeekOrigin.Begin); /* first Index Block position */
          Io.FileParam.FirstIndexBlock = (int)p;
        }
        Io.Writer.Write((int)p);
        p += 4;

        index.Position = (int)p + n;
      }
      else
      {
        Io.Writer.BaseStream.Seek(0, SeekOrigin.End);
        index.Position = (int)BaseStream.Position;
      }
      Io.Writer.BaseStream.Seek(p, SeekOrigin.Begin);

      Io.Write(index);

      return indexNr;
    }

    public int Append(Element element, ElementIndex index = null)
    {
      if (element == null)
      { return -1; }

      index = index ?? Io.GetIndex(element);

      PointCollection coords = new PointCollection();
      coords.AddRange(element.GetCoords(Io.Setup).Select(x => x.GetPoint()));
      index.SetMapBox(coords.Extent);

      int pos = Append(index);

      Io.Writer.BaseStream.Seek(index.Position, SeekOrigin.Begin);
      WriteElement(element);

      return pos;
    }

    public void Append(Control control)
    {
      if (control?.Element == null)
      { return; }

      int elementPosition = Append(control.Element);
      Io.AppendContolPar(this, control, elementPosition);
    }

    public void Write(Symbol.SymbolGraphics graphics)
    {
      if (graphics == null)
      { return; }

      Io.Writer.Write((short)graphics.Type);
      ushort iFlags = 0;
      if (graphics.RoundEnds)
      { iFlags |= 1; }
      Io.Writer.Write((short)iFlags);
      Io.Writer.Write((short)graphics.Color);

      Io.Writer.Write((short)graphics.LineWidth);
      Io.Writer.Write((short)graphics.Diameter);

      Io.Writer.Write((short)graphics.MapGeometry.EnumCoords().Count());
      Io.Writer.Write((short)0); // reserved
      Io.Writer.Write((short)0); // reserved

      Io.WriteGeometry(graphics.MapGeometry);
    }

    public void WriteHeader(FileParam data)
    {
      Io.Writer.BaseStream.Seek(0, SeekOrigin.Begin);

      Io.Writer.Write((short)FileParam.OCAD_MARK);
      Io.Writer.Write((short)data.SectionMark);
      Io.Writer.Write((short)data.Version);
      Io.Writer.Write((short)data.SubVersion);
      Io.Writer.Write((int)data.FirstSymbolBlock);
      Io.Writer.Write((int)data.FirstIndexBlock);
      Io.Writer.Write((int)data.SetupPosition);
      Io.Writer.Write((int)data.SetupSize);
      Io.Writer.Write((int)data.InfoPosition);
      Io.Writer.Write((int)data.InfoSize);
      Io.Writer.Write((int)data.FirstStrIdxBlock);
      Io.Writer.Write((int)data.FileNamePosition);
      Io.Writer.Write((int)data.FileNameSize);
      Io.Writer.Write((int)0); // reserved 4
    }

    public void WriteSymData(SymbolData sym, FileParam fileParam)
    {
      BaseStream.Seek(fileParam.HEADER_LENGTH, SeekOrigin.Begin);

      Io.Writer.Write((short)sym.ColorCount);
      Io.Writer.Write((short)sym.ColorSeparationCount);
      Io.Writer.Write((short)sym.CyanFreq);
      Io.Writer.Write((short)sym.CyanAngle);
      Io.Writer.Write((short)sym.MagentaFreq);
      Io.Writer.Write((short)sym.MagentaAngle);
      Io.Writer.Write((short)sym.YellowFreq);
      Io.Writer.Write((short)sym.YellowAngle);
      Io.Writer.Write((short)sym.BlackFreq);
      Io.Writer.Write((short)sym.BlackAngle);
      Io.Writer.Write((short)0); // reserved 1
      Io.Writer.Write((short)0); // reserved 2
    }

    public void WriteSetup(Setup setup, int length)
    {
      int i;

      int l = 0;
      Io.Writer.Write((int)setup.Offset.X); l += 4;
      Io.Writer.Write((int)setup.Offset.Y); l += 4;

      Io.Writer.Write((double)setup.GridDistance); l += 8;

      Io.Writer.Write((short)setup.WorkMode); l += 2;
      Io.Writer.Write((short)setup.LineMode); l += 2;
      Io.Writer.Write((short)setup.EditMode); l += 2;
      Io.Writer.Write((short)setup.Symbol); l += 2;
      Io.Writer.Write((double)setup.Scale); l += 8;

      Io.Writer.Write((double)setup.PrjTrans.X); l += 8;
      Io.Writer.Write((double)setup.PrjTrans.Y); l += 8;
      Io.Writer.Write((double)setup.PrjRotation); l += 8;
      Io.Writer.Write((double)setup.PrjGrid); l += 8;

      for (i = l; i < length; i++)
      { Io.Writer.Write((char)0); }
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

      data.SetupPosition = data.HEADER_LENGTH + FileParam.SYM_BLOCK_LENGTH +
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

      WriteSymData(sym, data);

      /* colors */
      for (i = 0; i < FileParam.MAX_COLORS; i++)
      {
        for (j = 0; j < FileParam.COLORINFO_LENGTH; j++)
        { Io.Writer.Write((char)0); }
      }

      /* separations */
      for (i = 0; i < FileParam.MAX_SEPS; i++)
      {
        for (j = 0; j < FileParam.COLORSEP_LENGTH; j++)
        { Io.Writer.Write((char)0); }
      }

      WriteSetup(setup, data.SetupSize);

      /* symbol indices */
      Io.Writer.Write((int)0);
      for (i = 0; i < FileParam.N_IN_SYMBOL_BLOCK; i++)
      {
        Io.Writer.Write((int)0);
      }

      /* elements */
      return 0;
    }

    public int DeleteElements(Func<ElementIndex, bool> checkDelete)
    {
      int n = 0;
      int iIndex = 0;
      ElementIndex idx = Io.ReadIndex(iIndex);
      while (idx != null)
      {
        long pos = BaseStream.Position;
        if (checkDelete(idx))
        {
          idx.Status = ElementIndex.StatusDeleted;
          BaseStream.Seek(pos - Io.FileParam.INDEX_LENGTH, SeekOrigin.Begin);
          Io.Write(idx);

          n++;
        }

        iIndex++;
        idx = Io.ReadIndex(iIndex);
      }
      return n;
    }

    public int ChangeSymbols(ElementIndexDlg<int> changeSymbol)
    {
      int n = 0;
      int iIndex = 0;
      ElementIndex pIndex = Io.ReadIndex(iIndex);
      List<ElementIndex> changedElements = new List<ElementIndex>();
      while (pIndex != null)
      {
        long pos = Io.Writer.BaseStream.Position;
        int newSymbol = changeSymbol(pIndex);
        if (newSymbol != pIndex.Symbol)
        {
          pIndex.Symbol = newSymbol;
          Io.Writer.BaseStream.Seek(pos - Io.FileParam.INDEX_LENGTH, SeekOrigin.Begin);
          Io.Write(pIndex);

          changedElements.Add(pIndex);
          n++;
        }

        iIndex++;
        pIndex = Io.ReadIndex(iIndex);
      }

      foreach (var index in changedElements)
      {
        BaseStream.Seek(index.Position, SeekOrigin.Begin);
        // Assumption: Symbol and ObjectType is at the start position!
        Io.WriteElementSymbol(index.Symbol);
        Io.Writer.Write((byte)index.ObjectType);
      }
      return n;
    }

    #region OCAD8

    private void Write(ElementIndex index)
    {
      Io.Write(index);
    }

    public void WriteElement(Element element)
    {
      Io.WriteElementHeader(element);

      Io.WriteElementContent(element);
    }

    private void WriteElementHeader(Element element)
    {
      Io.WriteElementHeader(element);
    }

    private void WriteElementSymbol(int symbol)
    {
      Io.WriteElementSymbol(symbol);
    }
    #endregion

    public static OcadWriter AppendTo(string file)
    {
      return AppendTo(new FileStream(file, FileMode.Open,
         FileAccess.ReadWrite));
    }
    public static OcadWriter AppendTo(Stream ocadStream, int ocadVersion = 0)
    {
      OcadWriter ocd = new OcadWriter(ocadStream, ocadVersion);
      return ocd;
    }

    public void DeleteElements(IList<int> symbols)
    {
      List<int> symSort = null;
      if (symbols != null)
      {
        symSort = new List<int>(symbols);
        symSort.Sort();
      }

      int iIndex = 0;
      ElementIndex pIndex = Io.ReadIndex(iIndex);
      while (pIndex != null)
      {
        if (symSort == null || symSort.BinarySearch(pIndex.Symbol) >= 0)
        {
          pIndex.Status = ElementIndex.StatusDeleted;
          BaseStream.Seek(-Io.FileParam.INDEX_LENGTH, SeekOrigin.Current);
          Io.Write(pIndex);
        }
        iIndex++;
        pIndex = Io.ReadIndex(iIndex);
      }
    }

    public void Remove(StringType type)
    {
      foreach (StringParamIndex index in Io.EnumStringParamIndices())
      {
        if (index.Type != type)
        { continue; }

        BaseStream.Seek(-8, SeekOrigin.Current);
        Io.Writer.Write((int)-1);
        BaseStream.Seek(4, SeekOrigin.Current);
      }
    }

    public void Append(StringType type, int elemNummer, string stringParam)
    {
      BaseStream.Seek(0, SeekOrigin.End);
      int iPos = (int)BaseStream.Position;

      StringParamIndex overwriteIndex = new StringParamIndex
      {
        Type = type,
        ElemNummer = elemNummer,
        Size = stringParam.Length / 64 * 64 + 64 // for some funny reason
      };
      Append(overwriteIndex);

      Overwrite(overwriteIndex, stringParam);

      for (int i = 0; i < 64; i++) // same reason as above
      { Io.Writer.Write((byte)0); }
    }

    public void Overwrite(StringParamIndex overwriteIndex, string stringParam)
    {
      if (stringParam.Length > overwriteIndex.Size)
      { }
      BaseStream.Seek(overwriteIndex.FilePosition, SeekOrigin.Begin);
      int n = stringParam.Length;
      for (int i = 0; i < n; i++)
      { Io.Writer.Write((byte)stringParam[i]); }
    }

    public void Overwrite(StringType type, int elemNummer, string stringParam)
    {
      BaseStream.Seek(0, SeekOrigin.End);
      int iPos = (int)BaseStream.Position;

      StringParamIndex overwriteIndex = null;

      foreach (var index in Io.EnumStringParamIndices())
      {
        if (index.Type == type)
        {
          overwriteIndex = index;
          break;
        }
      }

      if (overwriteIndex == null)
      {
        overwriteIndex = new StringParamIndex
        {
          Type = type,
          ElemNummer = elemNummer,
          Size = stringParam.Length
        };
        Append(overwriteIndex);
      }
      else
      {
        BaseStream.Seek(-16, SeekOrigin.Current);
        overwriteIndex.Size = stringParam.Length;
        overwriteIndex.FilePosition = iPos;

        Io.Writer.Write((int)overwriteIndex.FilePosition);
        Io.Writer.Write((int)overwriteIndex.Size);
        Io.Writer.Write((int)overwriteIndex.Type);
        Io.Writer.Write((int)overwriteIndex.ElemNummer);
      }

      BaseStream.Seek(overwriteIndex.FilePosition, SeekOrigin.Begin);
      int n = stringParam.Length;
      for (int i = 0; i < n; i++)
      { Io.Writer.Write((byte)stringParam[i]); }
    }

    private void Append(StringParamIndex index)
    {
      int i;
      long p = Io.FileParam.FirstStrIdxBlock;
      long p0 = p;
      while (p > 0)
      {
        p0 = p;
        Io.Writer.BaseStream.Seek(p, SeekOrigin.Begin);
        p = Io.Reader.ReadInt32();
      }
      if (p0 > 0)
      {
        p = p0 + 4;
        BaseStream.Seek(p, SeekOrigin.Begin); /* index->pos */
        StringParamIndex pIndex = new StringParamIndex { FilePosition = 1 };
        i = 0;
        while (pIndex.FilePosition > 0 && i < FileParam.N_IN_STRINGPARAM_BLOCK)
        {
          pIndex.FilePosition = Io.Reader.ReadInt32();
          pIndex.Size = Io.Reader.ReadInt32();
          pIndex.Type = (StringType)Io.Reader.ReadInt32();
          pIndex.ElemNummer = Io.Reader.ReadInt32();

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
        Io.Writer.BaseStream.Seek(0, SeekOrigin.End);
        p = BaseStream.Position;
        Io.Writer.Write((int)0);
        int n = FileParam.N_IN_STRINGPARAM_BLOCK * 16;
        for (int j = 0; j < n; j++)
        { Io.Writer.Write((byte)0); }
        if (p0 > 0)
        {
          Io.Writer.BaseStream.Seek(p0, SeekOrigin.Begin);
        }
        else
        {
          Io.Writer.BaseStream.Seek(32, SeekOrigin.Begin); /* first String Index Block position */
          Io.FileParam.FirstStrIdxBlock = (int)p;
        }
        Io.Writer.Write((int)p);
        p += 4;

        index.FilePosition = (int)p + n;
      }
      else
      {
        BaseStream.Seek(0, SeekOrigin.End);
        index.FilePosition = (int)BaseStream.Position;
        p -= 16;
      }
      BaseStream.Seek(p, SeekOrigin.Begin);

      Io.Writer.Write((int)index.FilePosition);
      Io.Writer.Write((int)index.Size);
      Io.Writer.Write((int)index.Type);
      Io.Writer.Write((int)index.ElemNummer);
    }

    public static void Write(Element element, Stream stream)
    {
      OcadWriter writer = AppendTo(stream);
      writer.WriteElement(element);
    }


    public void WriteSymbolStatus(Symbol.SymbolStatus status, int symbolStartPosition)
    {
      BaseStream.Seek(symbolStartPosition + 11, SeekOrigin.Begin);
      Io.Writer.Write((byte)status);
    }

    public void SymbolsSetState(IList<int> symbols, Symbol.SymbolStatus state)
    {
      List<int> sortSymbols = null;
      if (symbols != null)
      {
        sortSymbols = new List<int>(symbols);
        sortSymbols.Sort();
      }

      List<int> symPos = new List<int>();

      int iSymbol = 0;
      for (int posIndex = Io.ReadSymbolPosition(iSymbol);
        posIndex > 0; posIndex = Io.ReadSymbolPosition(iSymbol))
      {
        symPos.Add(posIndex);

        iSymbol++;
      }

      foreach (var posIndex in symPos)
      {
        Symbol.BaseSymbol symbol = Io.ReadSymbol(posIndex);
        if (symbols == null || sortSymbols.BinarySearch(symbol.Number) >= 0)
        {
          WriteSymbolStatus(state, posIndex);
        }
      }
    }
  }
}
