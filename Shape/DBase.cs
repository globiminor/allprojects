using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;

namespace DBase
{
  public enum ColumnType
  {
    Int = 'N',
    Double = 'F',
    String = 'C'
  }

  public class DBaseColumn : DataColumn
  {
    private const string _dbStart = "start";
    private readonly string _dBaseName;
    private readonly ColumnType _dBaseType;
    private readonly int _length;
    private readonly int _precision;

    private int _start;

    public static byte Byte(ColumnType type)
    {
      return (byte)type;
    }

    [Obsolete("Used only for Table.Clone()")]
    public DBaseColumn()
    { }

    public DBaseColumn(string name, ColumnType type, int length, int precision)
      : base(name,
      type == ColumnType.Double ? typeof(double) :
      type == ColumnType.Int ? typeof(int) : typeof(string))
    {
      string nameDb;

      if (name.Length > 10)
      { nameDb = name.Substring(0, 10); }
      else
      { nameDb = name; }

      _dBaseName = nameDb;
      _dBaseType = type;
      _length = length;
      _precision = precision;
    }

    public int Start
    {
      get
      { return _start; }
    }
    internal void SetStart(int start)
    {
      _start = start;
    }

    public string DBaseName
    {
      get
      { return _dBaseName; }
    }

    public ColumnType DBaseType
    {
      get
      { return _dBaseType; }
    }

    public int Length
    {
      get
      { return _length; }
    }

    public int Precision
    {
      get
      { return _precision; }
    }
  }

  public class DBaseColumnCollection : List<DBaseColumn>
  {
    public int IndexOf(string name)
    {
      int i = 0;
      foreach (var column in this)
      {
        if (column.ColumnName == name)
        { return i; }
        i++;
      }
      return -1;
    }
  }

  public class DBaseSchema
  {
    private DBaseColumnCollection _columnList;
    private DataTable _schema;
    protected int _recLength;

    protected System.Type SystemType(ColumnType type)
    {
      // TODO
      return typeof(int);
    }

    public DBaseSchema()
    {
      _columnList = new DBaseColumnCollection();
    }
    public DataRow NewRow()
    {
      if (_schema == null)
      {
        DBaseSchema dbschema = new DBaseSchema();
        foreach (var col in _columnList)
        {
          dbschema.Add(new DBaseColumn(col.DBaseName,
          col.DBaseType, col.Length, col.Precision));
        }
        _schema = new DataTable();
        foreach (var col in dbschema._columnList)
        { _schema.Columns.Add(col); }
      }
      return _schema.NewRow();
    }

    public DBaseColumnCollection Columns
    {
      get { return _columnList; }
    }

    public int AttrStartPos(int rec, int attr)
    {
      return RecStartPos(rec) + _columnList[attr].Start;
    }

    public int RecStartPos(int rec)
    {
      return (_columnList.Count + 1) * 32 + rec * _recLength + 2;
    }

    public int Add(DBaseColumn col)
    {
      _columnList.Add(col);
      int index = _columnList.IndexOf(col);

      if (index == 0)
      {
        col.SetStart(0);
        _recLength = 1;
      }
      else
      {
        int s0 = _columnList[index - 1].Start;
        col.SetStart(s0 + _columnList[index - 1].Length);
      }
      _recLength += col.Length;

      return index;
    }

    public int Add(string name, ColumnType type, int length, int precision)
    {
      return Add(new DBaseColumn(name, type, length, precision));
    }

    public int RecLength
    {
      get
      { return _recLength; }
    }

    internal void SetRecLength(int length)
    {
      _recLength = length;
    }
  }

  public class DBaseReader : IEnumerable<DataRow>
  {
    private class Enumerator : IEnumerator<DataRow>
    {
      private DBaseReader _reader;
      private int _pos;
      private readonly int _nObj;
      private DataRow _currentRow;

      public Enumerator(DBaseReader reader)
      {
        _reader = reader;
        _nObj = reader._nRec;
        ResetMe();
      }
      public void Dispose()
      { }

      private void ResetMe()
      {
        _pos = 0;
      }

      #region IEnumerator Members

      public void Reset()
      {
        ResetMe();
      }

      object System.Collections.IEnumerator.Current
      { get { return this.Current; } }
      public DataRow Current
      {
        get { return _currentRow; }
      }

      public bool MoveNext()
      {
        if (_pos >= _nObj)
        { return false; }

        _currentRow = _reader.Read(_pos);

        _pos++;
        return true;
      }

      #endregion
    }

    private EndianReader _reader;
    private DBaseSchema _schema;
    private int _nRec;

    internal DBaseSchema Schema
    {
      get
      { return _schema; }
    }
    public DBaseReader(string name)
    {
      string dbfName =
        Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".dbf";

      System.Text.Encoding e = System.Text.Encoding.GetEncoding("iso-8859-1");

      _reader = new EndianReader(
        new FileStream(dbfName, FileMode.Open, FileAccess.Read), e);

      _schema = new DBaseSchema(); // TODO : create schema

      if (_reader.ReadByte() != 3)
      { throw new InvalidDataException("missing DBase Header = 3"); }
      // year  = 
      _reader.ReadByte() /* + 1900 */ ;
      // month =
      _reader.ReadByte();
      // day   =
      _reader.ReadByte();

      int nRec = _reader.ReadInt32();

      int s = _reader.ReadInt16();
      int recordLength = _reader.ReadInt16();

      int nItem = s / 32 - 1;

      for (int i = 0; i < nItem; i++)
      {
        _schema.Add(ReadAttrDef(i));
      }
      Trace.Assert(_schema.RecLength == recordLength,
        string.Format("Record length field ({0}) does not correspond to Attr-Definition ({1}). " +
        Environment.NewLine +
        "Using record length from Attribute definition", recordLength, _schema.RecLength));
      _nRec = (int)((_reader.BaseStream.Length - (_schema.RecStartPos(0) - 1)) / _schema.RecLength);
      Trace.Assert(_nRec == nRec,
        string.Format("# records field ({0}) does not correspond records fount from file size ({1})." +
        Environment.NewLine +
        "Using # records from file size", nRec, _nRec));
      Rewind();
    }
    private DBaseColumn ReadAttrDef(int attrIndex)
    {
      int i = (attrIndex + 1) * 32;
      _reader.BaseStream.Seek(i, SeekOrigin.Begin);

      byte[] bName = _reader.ReadBytes(11);
      string name = "";
      int pos = 0;
      while (bName[pos] != 0)
      {
        name += (char)bName[pos];
        pos++;
      }

      ColumnType type = (ColumnType)_reader.ReadByte();

      _reader.BaseStream.Seek(i + 16, SeekOrigin.Begin);

      int length = _reader.ReadByte();
      int precision = _reader.ReadByte();

      return new DBaseColumn(name, type, length, precision);
    }

    private string GetNextRec()
    {
      char[] chars = _reader.ReadChars(_schema.RecLength);
      string s = new string(chars);

      return s;
    }

    public void SeekAttr(int rec, int attr)
    {
      _reader.BaseStream.Seek(_schema.RecStartPos(rec) +
        _schema.Columns[attr].Start,
        SeekOrigin.Begin);
    }
    public void SeekRec(int rec)
    {
      _reader.BaseStream.Seek(_schema.RecStartPos(rec), SeekOrigin.Begin);
    }

    public string GetRec(int rec)
    {
      SeekRec(rec);
      return GetNextRec();
    }
    public DataRow Read(int rec)
    {
      string sRec = GetRec(rec);
      DataRow row = _schema.NewRow();
      for (int iCol = 0; iCol < _schema.Columns.Count; iCol++)
      {
        DBaseColumn pCol = _schema.Columns[iCol];
        if (pCol.DBaseType == ColumnType.Int)
        { row[iCol] = ReadInt32(sRec, pCol.Start, pCol.Length); }
        else if (pCol.DBaseType == ColumnType.Double)
        { row[iCol] = ReadDouble(sRec, pCol.Start, pCol.Length); }
        else if (pCol.DBaseType == ColumnType.String)
        { row[iCol] = sRec.Substring(pCol.Start, pCol.Length); }
      }
      return row;
    }

    private int ReadInt32(string rec, int length)
    {
      return Convert.ToInt32(rec.Substring(0, length));
    }
    private int ReadInt32(string rec, int start, int length)
    {
      return Convert.ToInt32(rec.Substring(start, length));
    }

    private double ReadDouble(string rec, int length)
    {
      return Convert.ToDouble(rec.Substring(0, length));
    }
    private double ReadDouble(string rec, int start, int length)
    {
      return Convert.ToDouble(rec.Substring(start, length));
    }

    private string ReadAttr(int rec, int attr, int length)
    {
      SeekAttr(rec, attr);
      return new string(_reader.ReadChars(length));
    }

    public void Rewind()
    {
      SeekRec(0);
    }

    public void Open(string name)
    {
      int s, nItem, k;
      ColumnType type;
      int length, precision;
      char[] word = new char[12];

      if (Path.GetExtension(name).ToLower() != ".dbf")
      { name = name + ".dbf"; }

      _reader = new EndianReader(new FileStream(name, FileMode.Open), true);

      // check code
      if (_reader.BaseStream.ReadByte() != 3)
      { throw new System.Exception(string.Format("*** is {0} a DBase-File ?", name)); }
      /* year  = */
      _reader.BaseStream.ReadByte() /* + 1900 */ ;
      /* month = */
      _reader.BaseStream.ReadByte();
      /* day   = */
      _reader.BaseStream.ReadByte();

      _nRec = _reader.ReadInt32();

      s = _reader.ReadInt16();
      _schema.SetRecLength(_reader.ReadInt16());

      nItem = s / 32 - 1;

      int i = 64;
      _reader.BaseStream.Seek(i - 32, SeekOrigin.Begin);
      word[0] = (char)_reader.BaseStream.ReadByte();

      while (word[0] != 0 && i < s)
      {
        k = 0;
        for (int j = 1; j < 11; j++)
        {
          word[j] = (char)_reader.BaseStream.ReadByte();
          if (k == 0 && word[j] == 0)
          { k = j; }
        }
        string attr = new string(word, 0, k);

        _reader.BaseStream.Seek(i - 32 + 11, SeekOrigin.Begin);
        type = (ColumnType)_reader.BaseStream.ReadByte();

        _reader.BaseStream.Seek(i - 32 + 16, SeekOrigin.Begin);
        length = _reader.BaseStream.ReadByte();
        precision = _reader.BaseStream.ReadByte();

        _schema.Add(name, type, length, precision);

        i += 32;
        _reader.BaseStream.Seek(i - 32, SeekOrigin.Begin);
        word[0] = (char)_reader.BaseStream.ReadByte();
      }
      Rewind();
    }

    public int Count
    {
      get
      { return _nRec; }
    }

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    { return GetEnumerator(); }
    public IEnumerator<DataRow> GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    internal void Close()
    {
      _reader.Close();
    }
  }


  public class DBaseWriter
  {
    private EndianWriter _writer;
    private DBaseSchema _schema;
    private int _nRec;

    public DBaseWriter(string name)
    {
      _writer = new EndianWriter(new FileStream(name, FileMode.Create), true);
    }

    public DBaseWriter(string name, DBaseSchema schema)
    {
      name = Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".dbf";
      FileStream stream = new FileStream(name, FileMode.Create);
      _writer = new EndianWriter(stream, true);
      _schema = schema;

      _writer.Write((byte)3);
      _writer.Write((byte)99);
      _writer.Write((byte)11);
      _writer.Write((byte)11);

      _writer.Write((int)0); // # records
      _writer.Write((short)((_schema.Columns.Count + 1) * 32 + 1));
      _writer.Write((short)(_schema.RecLength));
      for (int i = 12; i < 32; i++)
      {
        _writer.Write((byte)0);
      }
      foreach (var col in _schema.Columns)
      {
        Append(col);
      }
      _writer.Write((byte)13);
    }

    private void Append(DBaseColumn col)
    {
      int lName = Math.Min(11, col.DBaseName.Length);
      for (int i = 0; i < lName; i++)
      { _writer.Write((byte)col.DBaseName[i]); }
      for (int i = lName; i < 11; i++)
      { _writer.Write((byte)0); }
      _writer.Write((byte)col.DBaseType);
      for (int i = 12; i < 16; i++)
      { _writer.Write((byte)0); }
      _writer.Write((byte)col.Length);
      _writer.Write((byte)col.Precision);

      for (int i = 18; i < 32; i++)
      { _writer.Write((byte)0); }
    }

    public void Close()
    {
      if (_writer != null)
      {
        _writer.Seek(4, SeekOrigin.Begin);
        _writer.Write((int)_nRec);
        _writer.Close();
        _writer = null;
      }
    }

    public static void WriteShapeDefault(string name, int nRec)
    {
      name = Path.GetPathRoot(name) + Path.GetFileNameWithoutExtension(name) + ".dbf";
      DBaseWriter writer = new DBaseWriter(name);
      Stream stream = writer._writer.BaseStream;

      stream.WriteByte(3);
      stream.WriteByte(99);
      stream.WriteByte(8);
      stream.WriteByte(17);

      writer._writer.Write(nRec);
      writer._writer.Write((short)65);
      writer._writer.Write((short)8);
      for (int i = 12; i < 32; i++)
      { stream.WriteByte(0); }
      writer._writer.Write("ID");
      for (int i = 2; i < 11; i++)
      { stream.WriteByte(0); }
      stream.WriteByte((byte)'N');
      for (int i = 12; i < 16; i++)
      { stream.WriteByte(0); }
      stream.WriteByte(7);
      stream.WriteByte(0);
      for (int i = 18; i < 32; i++)
      { stream.WriteByte(0); }
      stream.WriteByte(13);

      for (int i = 1; i <= nRec; i++)
      {
        string id = string.Format("{0:7i}", i);
        stream.WriteByte(32);
        writer._writer.Write(id);
      }
      writer.Close();
    }

    private void Write(string sVal)
    {
      int length = sVal.Length;
      for (int i = 0; i < length; i++)
      { _writer.Write((byte)sVal[i]); }
    }
    private void Write(int value, int length)
    {
      string fmt = "{0,-" + length + "}";
      string sVal = string.Format(fmt, value);
      Write(sVal);
    }
    private void Write(double value, int length, int precision)
    {
      string fmt = "{0,-" + length + ":f" + precision + "}";
      string sVal = string.Format(fmt, value);
      Write(sVal);
    }
    private void Write(string value, int length)
    {
      string fmt = "{0,-" + length + "}";
      string sVal = string.Format(fmt, value);
      Write(sVal);
    }
    private void WriteRecStart()
    {
      _writer.BaseStream.WriteByte(32);
    }
    private void WriteRecEnd()
    {
    }
    private void VerifySize(int rec)
    {
      int i, j;
      if (rec >= _nRec)
      {
        _writer.Seek(0, SeekOrigin.End);
        for (i = _nRec; i < rec + 1; i++)
        {
          for (j = 0; j < _schema.RecLength; j++)
          {
            _writer.Write((byte)32);
          }
        }
        _nRec = rec + 1;
      }
    }
    public void Write(int rec, int attr, int value)
    {
      VerifySize(rec);
      _writer.Seek(_schema.AttrStartPos(rec, attr), SeekOrigin.Begin);
      Write(value, _schema.Columns[attr].Length);
    }
    public void Write(int rec, int attr, double value)
    {
      VerifySize(rec);
      _writer.Seek(_schema.AttrStartPos(rec, attr), SeekOrigin.Begin);
      Write(value, _schema.Columns[attr].Length, _schema.Columns[attr].Precision);
    }
    public void Write(int rec, int attr, string value)
    {
      VerifySize(rec);
      _writer.Seek(_schema.AttrStartPos(rec, attr), SeekOrigin.Begin);
      Write(value, _schema.Columns[attr].Length);
    }
    public void WriteRec(int rec, object[] value)
    {
      VerifySize(rec);
      for (int iAttr = 0; iAttr < _schema.Columns.Count; iAttr++)
      {
        DBaseColumn pCol = _schema.Columns[iAttr];
        _writer.Seek(_schema.AttrStartPos(rec, iAttr), SeekOrigin.Begin);

        if (pCol.DBaseType == ColumnType.Int)
        { Write((int)value[iAttr], pCol.Length); }
        else if (pCol.DBaseType == ColumnType.Double)
        { Write((double)value[iAttr], pCol.Length, pCol.Precision); }
        else if (pCol.DBaseType == ColumnType.String)
        { Write((string)value[iAttr], pCol.Length); }
      }
    }
    public void WriteRec(int rec, string value)
    {
      VerifySize(rec);
      _writer.Seek(_schema.RecStartPos(rec), SeekOrigin.Begin);
      Write(value, _schema.RecLength);
    }

    private void WriteNRec(int nRec)
    {
      _writer.BaseStream.Seek(4, SeekOrigin.Begin);
      _writer.Write((short)nRec);
    }

    private void WriteItemHead(string name, ColumnType type, int length, int precision)
    {
      if (name.Length > 10)
      { name = name.Substring(0, 10); }

      _writer.Write(name);

      for (int i = name.Length; i < 10; i++)
      { _writer.BaseStream.WriteByte(0); }
      _writer.BaseStream.WriteByte(0);

      _writer.BaseStream.WriteByte(DBaseColumn.Byte(type));
      for (int i = 12; i < 16; i++)
      { _writer.BaseStream.WriteByte(0); }
      _writer.BaseStream.WriteByte((byte)length);
      _writer.BaseStream.WriteByte((byte)precision);
      for (int i = 18; i < 32; i++)
      { _writer.BaseStream.WriteByte(0); }
    }

    private void WriteHeader(int nItem, int recLength, int nRec)
    {
      _writer.BaseStream.WriteByte(3);
      _writer.BaseStream.WriteByte(100); // year (+ 1900)
      _writer.BaseStream.WriteByte(11); // month
      _writer.BaseStream.WriteByte(28); // day

      _writer.Write(nRec);
      _writer.Write((short)(32 + nItem * 32 + 1));
      _writer.Write((short)recLength);
      for (int i = 12; i < 32; i++)
      { _writer.BaseStream.WriteByte(0); }
    }

    private void WriteHeader()
    {
      WriteHeader(_schema.Columns.Count, _schema.RecLength, _nRec);

      foreach (var column in _schema.Columns)
      {
        WriteItemHead(column.DBaseName, column.DBaseType, column.Length, column.Precision);
      }
      _writer.BaseStream.WriteByte(13);
    }

    public int AddAttr(string name, ColumnType type, int length, int precision)
    {
      if (_nRec > 0)
      { throw new SystemException("Can only add attributes to empty tables"); }
      int n = _schema.Add(name, type, length, precision);

      WriteHeader();
      return n;
    }

    public void Rewind()
    {
      _writer.BaseStream.Seek((_schema.Columns.Count + 1) * 32 + 1,
        SeekOrigin.Begin);
    }
  }
}

