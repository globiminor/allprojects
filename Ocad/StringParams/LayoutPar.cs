using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocad.StringParams
{
  public class LayoutObjectPar : MultiParam
  {
    public const char TypeKey = 'r';
    public const char VisibleKey = 's';
    public const char AngleOmegaKey = 'a';
    public const char AnglePhiKey = 'b';
    public const char DimKey = 'd';
    public const char TransparentKey = 't';
    public const char OffsetXKey = 'x';
    public const char OffsetYKey = 'y';

    public const char PixelSizeXKey = 'u';
    public const char PixelSizeYKey = 'v';
    public const char InfraRedKey = 'i';
    public const char ActObjectIndexKey = 'n';

    public LayoutObjectPar(string para)
      : base(para)
    {
      if (!Type.HasValue)
      { Type = 0; }
    }

    private LayoutObjectPar()
    { }


    public bool? Visible
    {
      get
      {
        int? v = GetInt_(VisibleKey);
        if (!v.HasValue) return null;
        return v.Value != 0;
      }
      set { SetParam(VisibleKey, value); }
    }

    public int? Type
    {
      get { return GetInt_(TypeKey); }
      set { SetParam(TypeKey, value); }
    }

    public int? ActObjectIndex
    {
      get { return GetInt_(ActObjectIndexKey); }
      set { SetParam(ActObjectIndexKey, value); }
    }
  }
}
