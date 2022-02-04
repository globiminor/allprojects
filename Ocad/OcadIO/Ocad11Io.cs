using System.Collections.Generic;
using System.IO;
using System.Text;
using Ocad.Symbol;

namespace Ocad
{
  internal class Ocad11Io : OcadIo //Ocad9Reader
  {
    public Ocad11Io(Stream stream)
      : base(stream)
    { }

    protected override FileParam Init(int sectionMark, int version) => Ocad9Io.Init(this, sectionMark, version);
    public override Setup ReadSetup() => Ocad9Io.ReadSetup(this);
    public override ElementIndex ReadIndex(int i) => Ocad9Io.ReadIndex(this, i);
    public override int CalcElementLength(Element element) => Ocad9Io.CalcElementLength(this, element);
    public override int ReadElementLength() => Ocad9Io.ReadElementLength(this);
    public override T ReadElement<T>(out T element) => Ocad9Io.ReadElement(this, out element);
    public override void ReadAreaSymbol(AreaSymbol symbol) => Ocad9Io.ReadAreaSymbol(this, symbol);
    public override IEnumerable<ColorInfo> ReadColorInfos() => Ocad9Io.ReadColorInfos(this);
    public override ElementIndex GetIndex(Element element) => Ocad9Io.GetIndex(this, element);
    public override void Write(ElementIndex index) => Ocad9Io.Write(this, index);
    public override void WriteElementHeader(Element element) => Ocad9Io.WriteElementHeader(this, element);
    public override void WriteElementSymbol(int symbol) => Ocad9Io.WriteElementSymbol(this, symbol);
    public override void WriteElementContent(Element element) => Ocad9Io.WriteElementContent(this, element);
    public override int GetElementTextCount(Element element, string text) => Ocad9Io.TextCountV9(text);
    public override IList<Control> ReadControls(IList<StringParams.StringParamIndex> indexList = null) => Ocad9Io.ReadControls(this, indexList);

    public override BaseSymbol ReadSymbol() => ReadSymbol(this);
    public static BaseSymbol ReadSymbol(OcadIo io)
    {
      BaseSymbol symbol = Ocad9Io.ReadSymbolRoot(io);

      //SymbolTreeGroup
      for (int i = 0; i < 64; i++)
      {
        io.Reader.ReadInt16();
      }

      Ocad9Io.ReadSymbolSpez(io, symbol);

      return symbol;
    }

    public override string ReadDescription() => ReadDescription(this);
    public static string ReadDescription(OcadIo io)
    {
      byte[] bytes = io.Reader.ReadBytes(128);

      // OCAD 11
      string read = Encoding.Unicode.GetString(bytes).Trim('\0');
      return read;
    }

    public override void AppendControlPar(OcadWriter writer, Control control, int elementPosition)
    {
      Ocad9Io.AppendControlParam(writer, control, elementPosition);
    }
  }
}
