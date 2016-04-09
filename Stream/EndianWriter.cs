using System;
using System.IO;
using System.Text;

namespace System.IO
{
	/// <summary>
	/// Binary Writer with endiness
	/// </summary>
  public class EndianWriter:BinaryWriter
  {
    private bool m_bIsLittleEndian = true;

    #region constructors
    public EndianWriter() {}
    public EndianWriter(Stream s):base(s) {}
    public EndianWriter(Stream s,Text.Encoding e):base(s,e) {}

    public EndianWriter(bool isLittleEndian)
    {
      m_bIsLittleEndian = isLittleEndian;
    }
    public EndianWriter(Stream s,bool isLittleEndian):base(s)
    {
      m_bIsLittleEndian = isLittleEndian;
    }
    public EndianWriter(Stream s,Text.Encoding e,bool isLittleEndian):base(s,e)
    {
      m_bIsLittleEndian = isLittleEndian;
    }
    #endregion

    public bool IsLittleEndian
    {
      get { return m_bIsLittleEndian; }
      set { m_bIsLittleEndian = value; }
    }

    #region override
    public override void Write(char ch)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) {base.Write (ch);} 
      else {base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(ch)));}
    }
    public override void Write(char[] chars,int index,int count)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) {base.Write (chars,index,count);} 
      else 
      {
        for (int i = index; i < index + count; i++) {
          base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(chars[i])));
        }
      };
    }
    public override void Write(char[] chars)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (chars);
      else Write(chars,0,chars.GetLength(0));
    }
    public override void Write(double value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(float value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(int value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(long value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(short value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(uint value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(ulong value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    public override void Write(ushort value)
    {
      if (m_bIsLittleEndian == BitConverter.IsLittleEndian) base.Write (value);
      else base.Write(EndianReader.InvertBytes(BitConverter.GetBytes(value)));
    }
    #endregion

    public void WriteUnicodeString(string text)
    {
      UnicodeEncoding unicode = new UnicodeEncoding();
      byte[] pBytes = unicode.GetBytes(text);

      base.Write(pBytes);
      base.Write((byte) 0);
      base.Write((byte) 0);
    }

  }
}
