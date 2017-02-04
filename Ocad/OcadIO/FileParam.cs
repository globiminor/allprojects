namespace Ocad
{
  public abstract class FileParam
  {
    public const int OCAD_MARK = 3245;
    public abstract int HEADER_LENGTH { get; }
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

    public int SectionMark { get; set; }
    public int Version { get; set; }
    public int SubVersion { get; set; }
    public int FirstSymbolBlock { get; set; }
    public int FirstIndexBlock { get; set; }
    public int SetupPosition { get; set; }
    public int SetupSize { get; set; }
    public int InfoPosition { get; set; }
    public int InfoSize { get; set; }
    public int FirstStrIdxBlock { get; set; }
    public int FileNamePosition { get; set; }
    public int FileNameSize { get; set; }

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
        "Start String Index : {10,9}", OCAD_MARK, SectionMark, Version,
        SubVersion, FirstSymbolBlock, FirstIndexBlock, SetupPosition,
        SetupSize, InfoPosition, InfoSize, FirstStrIdxBlock);
    }
  }


  public class FileParamV8 : FileParam
  {
    public override int HEADER_LENGTH { get { return 48; } }
    public override int INDEX_LENGTH { get { return 24; } }
  }

  public class FileParamV9 : FileParam
  {
    public override int HEADER_LENGTH { get { return 48; } }
    public override int INDEX_LENGTH { get { return 40; } }
  }
  public class FileParamV12 : FileParam
  {
    public override int HEADER_LENGTH { get { return 60; } }
    public override int INDEX_LENGTH { get { return 40; } }

    public int OfflineSyncSerial { get; set; }
    public int CurrentFileVersion { get; set; }

    public int MrStartBlockPosition { get; set; }
  }
}
