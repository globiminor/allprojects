using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Basics.Geom;
using DBase;
// ReSharper disable RedundantCast
// ReSharper disable UnusedVariable

namespace Shape
{
  public enum ShapeType
  {
    Point = 1,
    Line = 3,
    Area = 5,

    PointZ = 11,
    LineZ = 13,
    AreaZ = 15,
  }

  internal class Common
  {
    public const int DataStart = 100;
    public const int ShapeTypePos = 32;
    public const int IndexRecLength = 8;
    public const int PointRecLength = 10;
    public const int PointZRecLength = 18;

    public static int PolyRecLength(int nParts, int nPoints)
    {
      return (52 + nParts * 4 + nPoints * 16) / 2 - 4;
    }

    public static int PolyZRecLength(int nParts, int nPoints)
    {
      return (68 + nParts * 4 + nPoints * 24) / 2 - 4;
    }

    public static int PolyZMRecLength(int nParts, int nPoints)
    {
      return (84 + nParts * 4 + nPoints * 32) / 2 - 4;
    }
  }
  /// <summary>
  /// Summary description for Shape.
  /// </summary>
  public class ShpReader : IEnumerable<IGeometry>, IDisposable
  {
    #region nested classes
    private class Enumerator : IEnumerator<IGeometry>
    {
      private IGeometry _geom;
      private readonly ShpReader _shpReader;
      private int _pos;
      private readonly int _nObj;

      public Enumerator(ShpReader reader)
      {
        _shpReader = reader;
        _nObj = _shpReader.GetNObjects();
        ResetMe();
      }

      public void Dispose()
      { }

      object System.Collections.IEnumerator.Current
      { get { return _geom; } }
      public IGeometry Current
      {
        get { return _geom; }
      }

      public void Reset()
      {
        ResetMe();
      }

      private void ResetMe()
      {
        _pos = 0;
        _shpReader.Reset();
      }

      public bool MoveNext()
      {
        if (_pos >= _nObj)
        { return false; }
        _shpReader.SeekNextStartPos();
        _geom = _shpReader.ReadNextShape();
        _pos++;
        return true;
      }
    }

    #endregion

    private readonly EndianReader _indexReader;
    private readonly EndianReader _geomReader;
    private readonly ShapeType _shapeType;
    private readonly int _dimension;

    public static bool Exists(string name)
    {
      string ext = Path.GetExtension(name);
      if (ext != "" && ext != ".shp" && ext != ".shx")
      { return false; }
      string shpName =
        Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".shp";
      if (File.Exists(shpName) == false)
      { return false; }
      string shxName = shpName.Substring(0, shpName.Length - 1) + "x";
      if (File.Exists(shxName) == false)
      { return false; }
      return true;
    }
    public ShpReader(string name)
    {
      string shpName = Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".shp";
      _geomReader = new EndianReader(
        new FileStream(shpName, FileMode.Open, FileAccess.Read));

      string shxName = shpName.Substring(0, shpName.Length - 1) + "x";
      _indexReader = new EndianReader(
        new FileStream(shxName, FileMode.Open, FileAccess.Read));

      _shapeType = ReadShapeType();

      if (_shapeType == ShapeType.Point)
      {
        _dimension = 2;
        Topology = 0;
      }
      else if (_shapeType == ShapeType.Line)
      {
        _dimension = 2;
        Topology = 1;
      }
      else if (_shapeType == ShapeType.Area)
      {
        _dimension = 2;
        Topology = 2;
      }
      else if (_shapeType == ShapeType.PointZ)
      {
        _dimension = 3;
        Topology = 0;
      }
      else if (_shapeType == ShapeType.LineZ)
      {
        _dimension = 3;
        Topology = 1;
      }
      else if (_shapeType == ShapeType.AreaZ)
      {
        _dimension = 3;
        Topology = 2;
      }
      else
      { throw new InvalidOperationException("unhandled shape type " + _shapeType); }
    }

    public void Dispose()
    {
      Close();
    }

    public ShapeType ShapeType { get { return _shapeType; } }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    { return GetEnumerator(); }
    public IEnumerator<IGeometry> GetEnumerator()
    {
      return new Enumerator(this);
    }
    private int SeekStartPos(int rec)
    {
      _indexReader.BaseStream.Seek(Common.IndexRecLength * rec + Common.DataStart,
        SeekOrigin.Begin);
      return SeekNextStartPos();
    }

    private int SeekNextStartPos()
    {
      _indexReader.IsLittleEndian = false;
      int pos = 2 * _indexReader.ReadInt32();
      int recLen = _indexReader.ReadInt32();
      _geomReader.BaseStream.Seek(pos, SeekOrigin.Begin);
      return pos;
    }

    public void Reset()
    {
      _indexReader.BaseStream.Seek(Common.DataStart, SeekOrigin.Begin);
    }

    private void ReadNextLineNPartsPnts(out int nParts, out int nPoints)
    {
      long pos = _geomReader.BaseStream.Position;
      //_geomReader.IsLittleEndian = false;
      //_geomReader.BaseStream.Seek(pos, SeekOrigin.Begin);
      //int recNo = _geomReader.ReadInt32();
      //int recLen = _geomReader.ReadInt32();
      //_geomReader.IsLittleEndian = true;
      //int shpType = _geomReader.ReadInt32();
      //double xMin = _geomReader.ReadDouble();
      //double yMin = _geomReader.ReadDouble();
      //double xMax = _geomReader.ReadDouble();
      //double yMax = _geomReader.ReadDouble();

      _geomReader.BaseStream.Seek(pos + 44, SeekOrigin.Begin);
      _geomReader.IsLittleEndian = true;

      nParts = _geomReader.ReadInt32();
      nPoints = _geomReader.ReadInt32();
    }

    private void ReadNCoord(int n, Point2D[] p)
    {
      int i;

      _geomReader.IsLittleEndian = true;
      for (i = 0; i < n; i++)
      {
        p[i].X = _geomReader.ReadDouble();
        p[i].Y = _geomReader.ReadDouble();
      }
    }
    private Point ReadCoord()
    {
      double x = _geomReader.ReadDouble();
      double y = _geomReader.ReadDouble();
      if (_dimension == 2)
      { return new Point2D(x, y); }
      else if (_dimension == 3)
      { return new Point3D(x, y, 0); }
      else
      { throw new InvalidOperationException("Unhandled Dimension " + _dimension); }
    }

    private Point ReadNextPoint()
    {
      _geomReader.ReadInt32(); // read rec
      _geomReader.ReadInt32(); // read rec_length
      _geomReader.IsLittleEndian = true;
      _geomReader.ReadInt32(); // read Shape Type
      double x = _geomReader.ReadDouble();
      double y = _geomReader.ReadDouble();
      if (_dimension == 2)
      { return new Point2D(x, y); }
      else if (_dimension == 3)
      { return new Point3D(x, y, 0); }
      else
      { throw new InvalidOperationException("Unhandled Dimension " + _dimension); }
    }

    private List<Point>[] ReadNextLine()
    {
      ReadNextLineNPartsPnts(out int nParts, out int nPoints);
      int[] nPartPoints = new int[nParts];
      List<Point>[] l = new List<Point>[nParts];
      for (int i = 0; i < nParts; i++)
      { nPartPoints[i] = _geomReader.ReadInt32(); } // read start index

      // calculate part lengths
      for (int i = 0; i < nParts - 1; i++)
      { nPartPoints[i] = nPartPoints[i + 1] - nPartPoints[i]; }
      nPartPoints[nParts - 1] = nPoints - nPartPoints[nParts - 1];


      for (int i = 0; i < nParts; i++)
      {
        int n = nPartPoints[i];
        List<Point> pntList = new List<Point>();

        for (int j = 0; j < n; j++)
        { pntList[j] = ReadCoord(); }

        l[i] = pntList;
      }
      return l;
    }

    private List<Point>[] ReadLine(int rec)
    {
      SeekStartPos(rec);
      return ReadNextLine();
    }

    private Point3D ReadNextPointZ()
    {
      Point3D p = (Point3D)ReadNextPoint();
      p.Z = _geomReader.ReadDouble();
      return p;
    }

    private List<Point>[] ReadNextLineZ()
    {
      List<Point>[] lines = ReadNextLine();

      double zMin = _geomReader.ReadDouble(); // zmin
      double zMax = _geomReader.ReadDouble(); // zmax
      foreach (var l in lines)
      {
        foreach (var p in l)
        { p.Z = _geomReader.ReadDouble(); }
      }
      return lines;
    }

    private List<Point>[] ReadLineZ(int rec)
    {
      SeekStartPos(rec);
      return ReadNextLineZ();
    }

    private Point3D ReadPointZ(int rec)
    {
      SeekStartPos(rec);
      return ReadNextPointZ();
    }

    private IGeometry ReadNextShape()
    {
      IGeometry geom;
      if (_shapeType == ShapeType.Point)
      { geom = ReadNextPoint(); }
      else if (_shapeType == ShapeType.Line)
      { geom = Polyline.Create(ReadNextLine()[0]); }
      else if (_shapeType == ShapeType.Area)
      { geom = new Area(ReadNextLine().Select(x => Polyline.Create(x))); }
      else if (_shapeType == ShapeType.PointZ)
      { geom = ReadNextPointZ(); }
      else if (_shapeType == ShapeType.LineZ)
      { geom = Polyline.Create(ReadNextLineZ()[0]); }
      else if (_shapeType == ShapeType.AreaZ)
      { geom = new Area(ReadNextLineZ().Select(x => Polyline.Create(x))); }
      else
      { throw new InvalidOperationException("unhandled shape type" + _shapeType); }
      return geom;
    }

    public int GetNObjects()
    {
      _indexReader.BaseStream.Seek(24, SeekOrigin.Begin);
      _indexReader.IsLittleEndian = false;

      return (_indexReader.ReadInt32() - 50) / 4;
    }

    public IGeometry this[int index]
    {
      get
      {
        SeekStartPos(index);
        return ReadNextShape();
      }
    }

    private ShapeType ReadShapeType()
    {
      _geomReader.BaseStream.Seek(Common.ShapeTypePos, SeekOrigin.Begin);
      _geomReader.IsLittleEndian = true;

      return (ShapeType)_geomReader.ReadInt32();
    }
    #region IGeometry Members

    public int Topology { get; }

    public int Dimension
    {
      get
      { return _dimension; }
    }

    public bool Intersects(IBox box)
    {
      return BoxOp.Intersects(Extent, box);
    }
    public bool Intersects(IGeometry geometry)
    {
      return Extent.Intersection(geometry) != null;
    }
    public Box Extent
    {
      get
      {
        _geomReader.IsLittleEndian = true;
        _geomReader.BaseStream.Seek(36, SeekOrigin.Begin);
        return new Box(
          new Point2D(_geomReader.ReadDouble(), _geomReader.ReadDouble()),
          new Point2D(_geomReader.ReadDouble(), _geomReader.ReadDouble()));
      }
    }

    #endregion

    internal void Close()
    {
      _geomReader.Close();
      _indexReader.Close();
    }

    public void ReadHeader()
    {
      _geomReader.BaseStream.Seek(0, SeekOrigin.Begin);
      _geomReader.IsLittleEndian = false;
      int fileCode = _geomReader.ReadInt32();

      _geomReader.BaseStream.Seek(24, SeekOrigin.Begin);
      int fileLength = _geomReader.ReadInt32();

      _geomReader.IsLittleEndian = true;
      int version = _geomReader.ReadInt32();
      int shpType = _geomReader.ReadInt32();
      double xMin = _geomReader.ReadDouble();
      double yMin = _geomReader.ReadDouble();
      double xMax = _geomReader.ReadDouble();
      double yMax = _geomReader.ReadDouble();
      double zMin = _geomReader.ReadDouble();
      double zMax = _geomReader.ReadDouble();
      double mMin = _geomReader.ReadDouble();
      double mMax = _geomReader.ReadDouble();
    }
  }


  public class ShapeReader : IEnumerable<DataRow>, IDisposable
  {
    private class ShapeEnumerator : IEnumerator<DataRow>
    {
      private readonly ShapeReader _shape;
      private readonly IEnumerator<IGeometry> _shpEnum;
      private readonly IEnumerator<DataRow> _dbfEnum;
      private DataRow _currentRow;

      public ShapeEnumerator(ShapeReader shape)
      {
        _shape = shape;
        _shpEnum = _shape._shpReader.GetEnumerator();
        _dbfEnum = _shape._dbfReader.GetEnumerator();
      }

      public void Dispose()
      { }
      #region IEnumerator Members

      public void Reset()
      {
        _shpEnum.Reset();
        _dbfEnum.Reset();
      }

      object System.Collections.IEnumerator.Current
      { get { return Current; } }

      public DataRow Current
      {
        get { return _currentRow; }
      }

      public bool MoveNext()
      {
        bool bShp = _shpEnum.MoveNext();
        bool bDbf = _dbfEnum.MoveNext();

        if (bShp == false)
        { return false; }

        _currentRow = _shape._schemaTable.NewRow();
        DataRow dbfRow = _dbfEnum.Current;
        _currentRow[0] = _shpEnum.Current;
        if (bDbf)
        {
          for (int i = 0; i < dbfRow.ItemArray.Length; i++)
          { _currentRow[i + 1] = dbfRow.ItemArray[i]; }
        }

        return true;
      }

      #endregion
    }
    private readonly ShpReader _shpReader;
    private readonly DBaseReader _dbfReader;
    private readonly DataTable _schemaTable;

    public static bool Exists(string name)
    {
      string ext = Path.GetExtension(name);
      if (ext != "" && ext != ".shp" && ext != ".shx")
      { return false; }
      string shpName =
        Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".shp";
      if (File.Exists(shpName) == false)
      { return false; }
      string shxName = shpName.Substring(0, shpName.Length - 1) + "x";
      if (File.Exists(shxName) == false)
      { return false; }
      return true;
    }

    public ShapeReader(string name)
    {
      _shpReader = new ShpReader(name);
      _dbfReader = new DBaseReader(name);

      _schemaTable = new DataTable();
      _schemaTable.Columns.Add("Shape", typeof(IGeometry));
      foreach (var col in _dbfReader.Schema.Columns)
      {
        _schemaTable.Columns.Add(col); // TODO : create new column
      }
    }

    public void Dispose()
    {
      Close();
    }
    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    { return GetEnumerator(); }
    public IEnumerator<DataRow> GetEnumerator()
    {
      return new ShapeEnumerator(this);
    }

    #endregion

    public Box Extent
    {
      get
      { return _shpReader.Extent; }
    }

    public int Topology
    {
      get
      { return _shpReader.Topology; }
    }

    public void Close()
    {
      _shpReader.Close();
      _dbfReader.Close();
    }

    public void ReadShpHeader()
    {
      _shpReader.ReadHeader();
    }
  }


  public class ShpWriter : IDisposable
  {
    private Box _extent;
    private EndianWriter _indexWriter;
    private EndianWriter _geomWriter;
    private readonly ShapeType _shapeType;
    private int _shpPos;
    private int _nRec;

    public ShapeType Type => _shapeType;

    public int Count => _nRec;

    public void Dispose()
    {
      try
      {
        Close();
      }
      catch { }

      _geomWriter?.Dispose();
      _geomWriter = null;

      _indexWriter?.Dispose();
      _indexWriter = null;
    }
    public ShpWriter(string name, ShapeType shapeType)
    {
      _shapeType = shapeType;
      string shpName = Path.GetDirectoryName(name) + Path.DirectorySeparatorChar +
        Path.GetFileNameWithoutExtension(name) + ".shp";
      _geomWriter = new EndianWriter(new FileStream(shpName, FileMode.Create));
      string shxName = shpName.Substring(0, shpName.Length - 1) + "x";
      _indexWriter = new EndianWriter(new FileStream(shxName, FileMode.Create));

      for (int i = 0; i < 100; i++)
      {
        _geomWriter.BaseStream.WriteByte(0);
        _indexWriter.BaseStream.WriteByte(0);
      }
      WriteShapeType(_shapeType);

      _geomWriter.Seek(Common.DataStart, SeekOrigin.Begin);
      _indexWriter.Seek(Common.DataStart, SeekOrigin.Begin);
      _shpPos = 50;
      _nRec = 0;
    }

    private void WriteShapeType(ShapeType shapeType)
    {
      _geomWriter.BaseStream.Seek(Common.ShapeTypePos, SeekOrigin.Begin);
      _geomWriter.IsLittleEndian = true;
      _indexWriter.BaseStream.Seek(Common.ShapeTypePos, SeekOrigin.Begin);
      _indexWriter.IsLittleEndian = true;

      _geomWriter.Write((int)shapeType);
      _indexWriter.Write((int)shapeType);

      _geomWriter.IsLittleEndian = false;
      _indexWriter.IsLittleEndian = false;
    }

    private void WriteObjectHead(int rec, int rl, int pos, ShapeType type)
    {
      _indexWriter.IsLittleEndian = false;
      _indexWriter.Write(pos);
      _indexWriter.Write(rl);

      _geomWriter.IsLittleEndian = false;
      _geomWriter.Write(rec);
      _geomWriter.Write(rl);

      _geomWriter.IsLittleEndian = true;
      _geomWriter.Write((int)type);
    }

    private void WriteObjectBox(IBox extent)
    {
      _geomWriter.IsLittleEndian = true;

      _geomWriter.Write(extent.Min.X);
      _geomWriter.Write(extent.Min.Y);
      _geomWriter.Write(extent.Max.X);
      _geomWriter.Write(extent.Max.Y);
    }

    private void WriteObjectGeom(Polyline line)
    {
      _geomWriter.IsLittleEndian = true;

      foreach (var p in line.Points)
      {
        _geomWriter.Write(p.X);
        _geomWriter.Write(p.Y);
      }
    }

    private void WriteObjectZ(Polyline line)
    {
      WriteObjectDim(line, 2);
    }

    private void WriteObjectDim(Polyline line, int dim)
    {
      _geomWriter.IsLittleEndian = true;

      foreach (var p in line.Points)
      { _geomWriter.Write(p[dim]); }
    }

    private int Write(int rec, int pos, Point2D p)
    {
      int rl = Common.PointRecLength;

      WriteObjectHead(rec, rl, pos, ShapeType.Point);
      _geomWriter.IsLittleEndian = true;

      _geomWriter.Write(p.X);
      _geomWriter.Write(p.Y);

      return rl + 4;
    }

    public void Write(Point2D p)
    {
      if (_extent == null)
      { _extent = new Box(p.Clone(), p.Clone()); }
      else
      { _extent.Include(p.Extent); }

      _shpPos += Write(_nRec + 1, _shpPos, p);
      _nRec++;
    }

    private int Write(int rec, int pos, Point3D p)
    {
      int rl = Common.PointZRecLength;
      WriteObjectHead(rec, rl, pos, ShapeType.PointZ);

      _geomWriter.IsLittleEndian = true;
      _geomWriter.Write(p.X);
      _geomWriter.Write(p.Y);
      _geomWriter.Write(p.Z);
      _geomWriter.Write((double)0); // measure

      return rl + 4;
    }

    public void Write(Point3D p)
    {
      if (_extent == null)
      { _extent = new Box(p.Clone(), p.Clone()); }
      else
      { _extent.Include(p.Extent); }

      _shpPos += Write(_nRec + 1, _shpPos, p);
      _nRec++;
    }

    private int Write(int rec, int pos, Polyline line)
    {
      int rl = Common.PolyRecLength(1, line.Points.Count);
      WriteObjectHead(rec, rl, pos, ShapeType.Line);
      WriteObjectBox(line.Extent);

      _geomWriter.IsLittleEndian = true;

      _geomWriter.Write((int)1);
      _geomWriter.Write(line.Points.Count);
      _geomWriter.Write((int)0);

      WriteObjectGeom(line);
      return rl + 4;
    }

    public void Write(Polyline line)
    {
      if (_extent == null)
      {
        IBox box = line.Extent;
        Point min = Point.Create(box.Min);
        Point max = Point.Create(box.Max);
        _extent = new Box(min, max);
      }
      else
      { _extent.Include(line.Extent); }

      if (_shapeType == ShapeType.Line)
      { _shpPos += Write(_nRec + 1, _shpPos, line); }
      else if (_shapeType == ShapeType.LineZ)
      { _shpPos += WriteZ(_nRec + 1, _shpPos, line); }
      else
      { throw new ArgumentException("Unhandled Shape type " + _shapeType); }

      _nRec++;
    }

    public void Write(IGeometry shape)
    {
      if (_shapeType == ShapeType.Point)
      { Write((Point2D)shape); }
      else if (_shapeType == ShapeType.PointZ)
      { Write((Point3D)shape); }
      else if (_shapeType == ShapeType.Line ||
        _shapeType == ShapeType.LineZ)
      { Write((Polyline)shape); }
      else
      { throw new ArgumentException("Unhandled Shape type " + _shapeType); }
    }

    private int WriteZ(int rec, int pos, Polyline line)
    {
      IBox extent = line.Extent;
      int rl;
      if (extent.Dimension < 4)
      {
        rl = Common.PolyZRecLength(1, line.Points.Count);
      }
      else
      {
        rl = Common.PolyZMRecLength(1, line.Points.Count);
      }

      WriteObjectHead(rec, rl, pos, ShapeType.LineZ);
      WriteObjectBox(extent);

      _geomWriter.IsLittleEndian = true;

      _geomWriter.Write((int)1);
      _geomWriter.Write(line.Points.Count);
      _geomWriter.Write((int)0);

      WriteObjectGeom(line);

      _geomWriter.Write(extent.Min[2]);
      _geomWriter.Write(extent.Max[2]);

      WriteObjectZ(line);

      if (extent.Dimension >= 4)
      {
        _geomWriter.Write(extent.Min[3]);
        _geomWriter.Write(extent.Max[3]);

        WriteObjectDim(line, 3);
      }
      return rl + 4;
    }

    public void WriteZ(Polyline line)
    {
      if (_extent == null)
      {
        IBox box = line.Extent;
        Point min = Point.Create(box.Min);
        Point max = Point.Create(box.Max);
        _extent = new Box(min, max);
      }
      else
      { _extent.Include(line.Extent); }
      _shpPos += WriteZ(_nRec + 1, _shpPos, line);
      _nRec++;
    }

    private int Write(int rec, int pos, Area area)
    /*
       the total number of points is in np[0],
       the starting point of the i. path is in np[i] (first part : i = 0)
    */
    {
      int nTotPoints = area.Border.PointCount;
      int rl = Common.PolyRecLength(area.Border.Count, nTotPoints);
      WriteObjectHead(rec, rl, pos, ShapeType.Area);
      WriteObjectBox(area.Extent);

      _geomWriter.IsLittleEndian = true;

      _geomWriter.Write(area.Border.Count);
      _geomWriter.Write(nTotPoints);
      _geomWriter.Write((int)0);

      int nPoints = 0;
      for (int i = 1; i < area.Border.Count; i++)
      {
        nPoints += area.Border[i - 1].Points.Count;
        _geomWriter.Write(nPoints);
      }

      foreach (var line in area.Border)
      { WriteObjectGeom(line); }

      return nTotPoints + 4;
    }

    //    int SHFPutObject(SHData *shData,int n,double **cop)
    //    {
    //      double x0,x1,y0,y1,z0,z1;
    //
    //      if (shData->geomDim == 2) 
    //      {
    //        G2GetLineBox(n,cop[0],cop[1],&x0,&y0,&x1,&y1);
    //      } 
    //      else if (shData->geomDim == 3) 
    //      {
    //        G3GetLineBox(n,cop[0],cop[1],cop[2],&x0,&y0,&z0,&x1,&y1,&z1);
    //      }
    //      if (shData->x0 > shData->x1) 
    //      {
    //        switch (shData->geomDim) 
    //        {
    //          case 3:
    //            shData->z0 = z0;
    //            shData->z1 = z1;
    //          case 2:
    //            shData->y0 = y0;
    //            shData->y1 = y1;
    //          case 1:
    //            shData->x0 = x0;
    //            shData->x1 = x1;
    //            break;
    //        }    
    //      } 
    //      else 
    //      {
    //        switch (shData->geomDim) 
    //        {
    //          case 3:
    //            if (shData->z0 > z0) shData->z0 = z0;
    //            if (shData->z1 < z1) shData->z1 = z1;
    //          case 2:
    //            if (shData->y0 > y0) shData->y0 = y0;
    //            if (shData->y1 < y1) shData->y1 = y1;
    //          case 1:
    //            if (shData->x0 > x0) shData->x0 = x0;
    //            if (shData->x1 < x1) shData->x1 = x1;
    //            break;
    //        }
    //      }
    //      if (shData->geomDim == 3 && shData->topoDim == 0) 
    //      {
    //        shData->shpPos += SHFPutPointZ0(shData->shp,shData->shx,
    //          shData->nRec + 1,shData->shpPos,cop[0][0],cop[1][0],cop[2][0]);
    //        shData->nRec++;
    //      }
    //      else if (shData->geomDim == 3 && shData->topoDim == 1) 
    //      {
    //        shData->shpPos += SHFPutLineZ0(shData->shp,shData->shx,
    //          shData->nRec + 1,shData->shpPos,n,cop[0],cop[1],cop[2],
    //          x0,y0,x1,y1,z0,z1);
    //        shData->nRec++;
    //      }
    //      return 0;
    //    }

    private void WriteHeader()
    {
      _geomWriter.BaseStream.Seek(0, SeekOrigin.Begin);
      _indexWriter.BaseStream.Seek(0, SeekOrigin.Begin);

      _geomWriter.IsLittleEndian = false;
      _indexWriter.IsLittleEndian = false;

      _geomWriter.Write((int)9994);
      _indexWriter.Write((int)9994);

      for (int i = 4; i < 24; i++)
      {
        _geomWriter.BaseStream.WriteByte(0);
        _indexWriter.BaseStream.WriteByte(0);
      }

      _geomWriter.Write((int)(_geomWriter.BaseStream.Length / 2));
      _indexWriter.Write((int)(_indexWriter.BaseStream.Length / 2));

      _geomWriter.IsLittleEndian = true;
      _indexWriter.IsLittleEndian = true;

      _geomWriter.Write((int)1000);
      _indexWriter.Write((int)1000);
      _geomWriter.Write((int)_shapeType);
      _indexWriter.Write((int)_shapeType);

      _geomWriter.Write(_extent.Min.X);
      _indexWriter.Write(_extent.Min.X);
      _geomWriter.Write(_extent.Min.Y);
      _indexWriter.Write(_extent.Min.Y);
      _geomWriter.Write(_extent.Max.X);
      _indexWriter.Write(_extent.Max.X);
      _geomWriter.Write(_extent.Max.Y);
      _indexWriter.Write(_extent.Max.Y);

      for (int i = 68; i < 100; i++)
      {
        _geomWriter.BaseStream.WriteByte(0);
        _indexWriter.BaseStream.WriteByte(0);
      }

      _geomWriter.IsLittleEndian = false;
      _indexWriter.IsLittleEndian = false;
    }

    private void WriteHeaderZ()
    {
      WriteHeader();

      _geomWriter.BaseStream.Seek(68, SeekOrigin.Begin);
      _indexWriter.BaseStream.Seek(68, SeekOrigin.Begin);

      _geomWriter.IsLittleEndian = true;
      _indexWriter.IsLittleEndian = true;

      _geomWriter.Write(_extent.Min.Z);
      _indexWriter.Write(_extent.Min.Z);
      _geomWriter.Write(_extent.Max.Z);
      _indexWriter.Write(_extent.Max.Z);

      if (_extent.Dimension > 3)
      {
        _geomWriter.Write(_extent.Min[3]);
        _indexWriter.Write(_extent.Min[3]);
        _geomWriter.Write(_extent.Max[3]);
        _indexWriter.Write(_extent.Max[3]);
      }

      _geomWriter.IsLittleEndian = false;
      _indexWriter.IsLittleEndian = false;
    }

    public void Close()
    {
      if (_geomWriter != null)
      {
        if (_shapeType == ShapeType.Point || _shapeType == ShapeType.Line ||
          _shapeType == ShapeType.Area)
        { WriteHeader(); }
        else if (_shapeType == ShapeType.PointZ || _shapeType == ShapeType.LineZ ||
          _shapeType == ShapeType.AreaZ)
        { WriteHeaderZ(); }

        _geomWriter.Close();
        _geomWriter = null;
      }
      if (_indexWriter != null)
      {
        _indexWriter.Close();
        _indexWriter = null;
      }
    }
  }

  public class ShapeWriter : IDisposable
  {
    private ShpWriter _shpWriter;
    private DBaseWriter _dbfWriter;

    public ShapeWriter(string name, ShapeType shapeType)
    {
      _shpWriter = new ShpWriter(name, shapeType);
    }
    public ShapeWriter(string name, ShapeType shapeType, DataTable schema)
    {
      _shpWriter = new ShpWriter(name, shapeType);
      DBaseSchema pDbSchema = new DBaseSchema();

      // 0 = Shape Col
      for (int iCol = 1; iCol < schema.Columns.Count; iCol++)
      {
        pDbSchema.Add((DBaseColumn)schema.Columns[iCol]);
      }
      _dbfWriter = new DBaseWriter(name, pDbSchema);
    }
    public void Dispose()
    {
      Close();
    }
    public void Write(IGeometry shape, object[] attr)
    {
      _shpWriter.Write(shape);
      _dbfWriter.WriteRec(_shpWriter.Count - 1, attr);
    }
    public void Close()
    {
      if (_shpWriter != null)
      {
        _shpWriter.Close();
        _shpWriter = null;
      }
      if (_dbfWriter != null)
      {
        _dbfWriter.Close();
        _dbfWriter = null;
      }
    }

  }
}
