using System.Text;

namespace Ocad
{
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
