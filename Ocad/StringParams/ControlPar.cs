namespace Ocad.StringParams
{
  public class ControlPar : MultiParam
  {
    public const char TypeKey = 'Y';
    public const char IdKey = 'a';

    public const char SymCKey = 'c';
    public const char SymDKey = 'd';
    public const char SymEKey = 'e';
    public const char SymFKey = 'f';
    public const char SymGKey = 'g';
    public const char SymHKey = 'h';
    public const string FunnelTapesKey = "mf";
    public const string DescriptionKey = "ot";
    public const char SizeInfoKey = 's';

    private ControlPar() { }
    public static ControlPar Create(Control control)
    {
      ControlPar par = new ControlPar();

      par.ParaList.Add(control.Name);
      par.Add(TypeKey, control.Code);

      return par;
    }

    public void ClearSymbols()
    {
      SetParam(SymCKey, null);
      SetParam(SymDKey, null);
      SetParam(SymEKey, null);
      SetParam(SymFKey, null);
      SetParam(SymGKey, null);
      SetParam(SymHKey, null);
      SetParam(SizeInfoKey, null);
    }
    public ControlPar(string para)
      : base(para)
    { }

    public char Type
    {
      get { return GetString(TypeKey)[0]; }
      set { SetParam(TypeKey, value); }
    }

    public string Id
    {
      get { return GetString(IdKey); }
      set { SetParam(IdKey, value); }
    }


    public string TextDesc
    {
      get { return GetString((char)ControlCode.TextBlock); }
      set { SetParam((char)ControlCode.TextBlock, value); }
    }

  }

  public class TextBlockPar : MultiParam
  {
    public const char CodeKey = 'a';
    public const char TextKey = 't';

    public TextBlockPar(string para)
      : base(para)
    { }

    public string Code
    {
      get { return GetString(CodeKey); }
      set { SetParam(CodeKey, value); }
    }

    public string TextDesc
    {
      get { return GetString(TextKey); }
      set { SetParam(TextKey, value); }
    }

  }
}
