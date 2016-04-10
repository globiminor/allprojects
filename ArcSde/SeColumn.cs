using System;

namespace ArcSde
{
  public class SeColumn
  {
    private string _name;
    private int _type;

    private int _nDigits;
    private string _description;

    internal SeColumn(IntPtr se_columnInfo)
    {
      char[] nm = new char[SdeType.SE_MAX_COLUMN_LEN];
      ErrorHandling.checkRC(
        IntPtr.Zero, IntPtr.Zero, CApi.SE_columninfo_get_name(se_columnInfo, nm));
      _name = Api.String(nm);

      ErrorHandling.checkRC(
        IntPtr.Zero, IntPtr.Zero, CApi.SE_columninfo_get_type(se_columnInfo, out _type));

      char[] desc = new char[SdeType.SE_MAX_DESCRIPTION_LEN];
      ErrorHandling.checkRC(
        IntPtr.Zero, IntPtr.Zero, CApi.SE_columninfo_get_description(se_columnInfo, desc));
      _description = Api.String(desc);

      ErrorHandling.checkRC(
        IntPtr.Zero, IntPtr.Zero, CApi.SE_columninfo_get_decimal_digits(se_columnInfo, out _nDigits));
    }

    public string Name
    {
      get { return _name; }
    }

    public int Type
    {
      get { return _type; }
    }

    public int NrDigits
    {
      get { return _nDigits; }
    }

    public string Description
    {
      get { return _description; }
    }

    public override string ToString()
    {
      return _name;
    }
  }
}