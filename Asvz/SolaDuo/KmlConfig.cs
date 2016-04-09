namespace Asvz
{
  public class KmlConfig
  {
    private bool _includeLookAt;
    private bool _includeMarks;

    public bool IncludeLookAt
    {
      get { return _includeLookAt; }
      set { _includeLookAt = value; }
    }

    public bool IncludeMarks
    {
      get { return _includeMarks; }
      set { _includeMarks = value; }
    }
  }
}
