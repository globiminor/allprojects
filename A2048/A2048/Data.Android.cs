

using Android.OS;
using System.Collections.Generic;
using System.IO;

namespace A2048
{
  public partial class Grid
  {
    public void SaveMoves()
    {
      if (Size <= 4)
      { SetMoves(_moves); }
    }

    public void LoadMoves()
    {
      if (Size > 4)
      { return; }

      string path;
      byte[] moves = GetMoves(out path, create: false);
      if (moves == null)
      { return; }

      _moves = new List<byte>(moves);
      InitMoves();
    }

    private void SetMoves(List<byte> moves)
    {
      Java.IO.File store = Environment.ExternalStorageDirectory;
      string path = store.AbsolutePath;
      string home = Path.Combine(path, ".a2048");
      if (!Directory.Exists(home))
      {
        Directory.CreateDirectory(home);
      }

      string currentPath = Path.Combine(home, "current.moves");
      using (Stream stream = new FileStream(currentPath, FileMode.Create))
      {
        int l = moves.Count;
        byte[] moveArray = moves.ToArray();
        stream.Write(moveArray, 0, l);
      }
    }

    private byte[] GetMoves(out string currentPath, bool create)
    {
      Java.IO.File store = Environment.ExternalStorageDirectory;
      string path = store.AbsolutePath;
      string home = Path.Combine(path, ".a2048");
      if (!Directory.Exists(home))
      {
        if (!create)
        {
          currentPath = null;
          return null;
        }
        Directory.CreateDirectory(home);
      }

      currentPath = Path.Combine(home, "current.moves");
      if (File.Exists(currentPath))
      {
        byte[] moves;
        using (Stream stream = new FileStream(currentPath, FileMode.Open))
        {
          long l = stream.Length;
          moves = new byte[l];
          stream.Read(moves, 0, (int)l);
        }
        return moves;
      }
      else
      { return null; }
    }

  }
}