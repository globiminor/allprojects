using System;
using System.Collections;
using System.Text;

namespace System.IO
{
  /// <summary>
  /// Binary Reader with endian
  /// </summary>
  public class EndianReader : BinaryReader
  {
    private bool _isLittleEndian = true;

    #region constructors
    public EndianReader(Stream s) : base(s) { }
    public EndianReader(Stream s, Encoding e) : base(s, e) { }

    public EndianReader(Stream s, bool isLittleEndian)
      : base(s)
    {
      _isLittleEndian = isLittleEndian;
    }
    public EndianReader(Stream s, Encoding e, bool isLittleEndian)
      : base(s, e)
    {
      _isLittleEndian = isLittleEndian;
    }
    #endregion

    public bool IsLittleEndian
    {
      get
      { return _isLittleEndian; }
      set
      { _isLittleEndian = value; }
    }

    internal static byte[] InvertBytes(byte[] b)
    {
      int n = b.GetLength(0);
      int n2 = n / 2;
      n--;
      for (int i = 0; i < n2; i++)
      {
        Byte replace = b[i];
        b[i] = b[n - i];
        b[n - i] = replace;
      }
      return b;
    }

    #region override
    public override char ReadChar()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian)
      {
        char c = base.ReadChar();
        return c;
        //byte[] bytes = base.ReadBytes(2);
        //char c = BitConverter.ToChar(bytes, 0);
        //return c; // return base.ReadChar();
      }
      else return BitConverter.ToChar(InvertBytes(base.ReadBytes(2)), 0);
    }
    /*    public override char[] ReadChars(int count)
        {
          if (m_bIsLittleEndian == BitConverter.IsLittleEndian) return base.ReadChars(count);
          else 
          {
            char[] c = new char[count];
            base.ReadBytes(
            for (int i = 0; i < count; i++) {
              base.Write(InvertBytes(BitConverter.GetBytes(chars[i])));
            }
            return c;
          }
        } */
    public override double ReadDouble()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadDouble();
      else return BitConverter.ToDouble(InvertBytes(base.ReadBytes(8)), 0);
    }
    public override float ReadSingle()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadSingle();
      else return BitConverter.ToSingle(InvertBytes(base.ReadBytes(4)), 0);
    }
    public override short ReadInt16()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadInt16();
      else return BitConverter.ToInt16(InvertBytes(base.ReadBytes(2)), 0);
    }
    public override int ReadInt32()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadInt32();
      else return BitConverter.ToInt32(InvertBytes(base.ReadBytes(4)), 0);
    }
    public override long ReadInt64()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadInt64();
      else return BitConverter.ToInt64(InvertBytes(base.ReadBytes(8)), 0);
    }
    public override ushort ReadUInt16()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadUInt16();
      else return BitConverter.ToUInt16(InvertBytes(base.ReadBytes(2)), 0);
    }
    public override uint ReadUInt32()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadUInt32();
      else return BitConverter.ToUInt32(InvertBytes(base.ReadBytes(4)), 0);
    }
    public override ulong ReadUInt64()
    {
      if (_isLittleEndian == BitConverter.IsLittleEndian) return base.ReadUInt64();
      else return BitConverter.ToUInt64(InvertBytes(base.ReadBytes(8)), 0);
    }
    #endregion

    /// <summary>
    /// Reads a 0 terminated Unicode string from the stream
    /// </summary>
    /// <returns></returns>
    public string ReadUnicodeString()
    {
      byte b0, b1;
      ArrayList pList = new ArrayList();
      do
      {
        b0 = ReadByte();
        b1 = ReadByte();
        if (b0 != 0 || b1 != 0)
        {
          pList.Add(b0);
          pList.Add(b1);
        }
      } while (b0 != 0 || b1 != 0);

      int iNBytes = pList.Count;
      byte[] pBytes = new byte[iNBytes];
      for (int iByte = 0; iByte < iNBytes; iByte++)
      { pBytes[iByte] = (byte)pList[iByte]; }

      UnicodeEncoding unicode = new UnicodeEncoding();
      return unicode.GetString(pBytes);
    }
  }
}
