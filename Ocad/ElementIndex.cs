using Basics.Geom;

namespace Ocad
{
  public class ElementIndex
  {
    public const int StatusDeleted = 3;

    private int _index;
    private Box _box;
    private int _symbol;
    // Version 9
    private short _objType;
    private short _status;
    private short _viewType;
    private short _color;
    private short _importLayer;

    public ElementIndex(int index)
    {
      _index = index;
      _box = new Box(new Point2D(), new Point2D());
    }

    public ElementIndex(IBox box)
    {
      _index = -1;
      SetBox(box);
    }

    public int Index
    {
      get { return _index; }
    }

    internal void SetBox(IBox box)
    {
      if (box is Box)
      { _box = (Box)box; }
      else
      { _box = new Box(Point.Create(box.Min), Point.Create(box.Max)); }
    }

    public Box Box
    {
      get { return _box; }
      internal set { _box = value; }
    }

    internal int Position { get; set; }
    internal int Length { get; private set; }
    internal int ReadElementLength(OcadReader reader)
    {
      Length = reader.ReadElementLength();
      return Length;
    }
    internal int CalcElementLength(OcadReader reader, Element elem)
    {
      Length = reader.CalcElementLength(elem);
      return Length;
    }

    public int Symbol
    {
      get { return _symbol; }
      set { _symbol = value; }
    }

    public short ObjectType
    {
      get { return _objType; }
      set { _objType = value; }
    }
    public short Status
    {
      get { return _status; }
      set { _status = value; }
    }
    public short ViewType
    {
      get { return _viewType; }
      set { _viewType = value; }
    }
    public short Color
    {
      get { return _color; }
      set { _color = value; }
    }
    public short ImportLayer
    {
      get { return _importLayer; }
      set { _importLayer = value; }
    }


    public string ToString(bool full)
    {
      if (full == false)
      { return ToString(); }

      return string.Format(
        "Lower left x  : {0,9}\n" +
        "Lower left y  : {1,9}\n" +
        "Upper right x : {2,9}\n" +
        "Upper right y : {3,9}\n" +
        "File position : {4,9}\n" +
        "File length   : {5,9}\n" +
        "Symbol        : {6,9}",
        _box.Min.X, _box.Min.Y, _box.Max.X, _box.Max.Y,
        Position, Length, _symbol);
    }
  }
}
