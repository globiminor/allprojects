using System;

namespace Ocad.StringParams
{
  public class ScalePar : MultiParam
  {
    public const char ScaleKey = 'm';
    public const char XKey = 'x';
    public const char YKey = 'y';
    public const char RotationKey = 'a';
    public const char RealWorldKey = 'r';
    public const char KeyGridZone = 'i';

    public ScalePar()
    { }
    public ScalePar(string stringParam)
      : base(stringParam)
    { }

    public double Scale
    {
      get { return GetDouble(ScaleKey); }
      set { SetParam(ScaleKey, value); }
    }

    public int? GridAndZone
    {
      get { return GetInt_(KeyGridZone); }
      set { SetParam(KeyGridZone, value); }
    }
    public double X
    {
      get { return GetDouble(XKey); }
      set { SetParam(XKey, value); }
    }
    public double Y
    {
      get { return GetDouble(YKey); }
      set { SetParam(YKey, value); }
    }
    public double Rotation
    {
      get { return GetDouble(RotationKey); }
      set { SetParam(RotationKey, value); }
    }

    public bool RealWorld
    {
      get { return GetInt(RealWorldKey) != 0; }
      set { SetParam(RealWorldKey, value ? "1" : "0"); }
    }
  }
}
