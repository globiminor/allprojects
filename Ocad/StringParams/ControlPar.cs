namespace Ocad.StringParams
{
  public class ControlPar : MultiParam
  {
    public const char TypeKey = 'Y';
    public const char TypeStart = 's';
    public const char TypeControl = 'c';
    public const char TypeMarked = 'm';
    public const char TypeFinish = 'f';
    public const char TypeText = 't';
    // (s = start, c = control, m = marked route, f = finish, d = control description,
    //n = course name, u = start number, v = variation code, t = text block)
    public const char SymCKey = 'c';
    public const char SymDKey = 'd';
    public const char SymEKey = 'e';
    public const char SymFKey = 'f';
    public const char SymGKey = 'g';
    public const char SymHKey = 'h';
    public const string FunnelTapesKey = "mf";
    public const string DescriptionKey = "ot";
    public const char SizeInfoKey = 's';
    public const char TextDescKey = 't';

    private ControlPar() { }
    public static ControlPar Create(Control control)
    {
      ControlPar par = new ControlPar();

      par.ParaList.Add(control.Name);
      par.Add(TypeKey, control.Code);

      return par;
    }

    public ControlPar(string para)
      : base(para)
    { }

    public char Type
    {
      get { return GetString(TypeKey)[0]; }
      set { SetParam(TypeKey, value); }
    }

    public string TextDesc
    {
      get { return GetString(TextDescKey); }
      set { SetParam(TextDescKey, value); }
    }

  }
}
