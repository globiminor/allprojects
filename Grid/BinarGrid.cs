using System.IO;

namespace Grid
{
	/// <summary>
	/// Summary description for BinarGrid.
	/// </summary>
	internal class BinarGrid
	{
    public enum EGridType {eInt = 0, eDouble = 1};
    public const int START_DATA = 52;

    public static void PutHeader(System.IO.Stream fi,
      int nx,int ny,EGridType type,short length,
      double x0,double y0,double dx,double z0,double dz)
    {
      fi.Seek(0,SeekOrigin.Begin);
      BinaryWriter writer = new BinaryWriter(fi);
      writer.Write(nx);
      writer.Write(ny);
      writer.Write((short) type);
      writer.Write((short) length);
      writer.Write(x0);
      writer.Write(y0);
      writer.Write(dx);
      writer.Write(z0);
      writer.Write(dz);
    }

    public static void GetHeader(FileStream fi,
      out int nx,out int ny,out EGridType type,out short length,
      out double x0,out double y0,out double dx,out double z0,out double dz)
    {
      fi.Seek(0,SeekOrigin.Begin);
      BinaryReader pFile = new BinaryReader(fi);
      nx = pFile.ReadInt32();
      ny = pFile.ReadInt32();
      type = (EGridType) pFile.ReadInt16();
      length = pFile.ReadInt16();
      x0 = pFile.ReadDouble();
      y0 = pFile.ReadDouble();
      dx = pFile.ReadDouble();
      z0 = pFile.ReadDouble();
      dz = pFile.ReadDouble();
    }
	}
}
