

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace GeomProcessor
{
  public enum TileStatus
  {
    Unhandled,
    Processing,
    Done,
  }
  public class TileCoordinator
  {
    public static int NumThreads = 4;

    public static void Main(string[] args)
    {
      if (args.Length == 1 && args[0] == TileProcessor.PipeInit)
      {
        NamedPipeClientStream pipeClient =
          new NamedPipeClientStream(".", TileProcessor.PipeName, PipeDirection.InOut);
        try
        {
          Random r = new Random(Process.GetCurrentProcess().Id);
          Console.WriteLine("Connecting to server");
          pipeClient.Connect();
          StreamString ss = new StreamString(pipeClient);
          if (ss.ReadString() != TileProcessor.PipeId)
          {
            Console.WriteLine("Server could not be verified");
          }
          else
          {
            string sTile = ss.ReadString();
            while (int.TryParse(sTile, out int iTile) && iTile >= 0)
            {
              Console.WriteLine($"Processing tile {iTile}");
              Thread.Sleep((int)(r.NextDouble() * 5000));
              ss.WriteString($"Processed {iTile}");
              sTile = ss.ReadString();
            }
            ss.WriteString("Hallo");
          }
        }
        finally
        {
          pipeClient.Close();
//          Console.ReadLine();
        }
      }
      else
      {
        TileProcessor.StartProcessors();
        StartTiles(98);
      }
    }
    public static TileStatus[] TileStatusList { get; private set; }
    private static void StartTiles(int nTiles)
    {
      string procName = System.Reflection.Assembly.GetExecutingAssembly().Location;

      TileStatusList = new TileStatus[nTiles];

      Process[] procs = new Process[NumThreads];
      for (int iThread = 0; iThread < NumThreads; iThread++)
      {
        procs[iThread] = Process.Start(procName, TileProcessor.PipeInit);
      }
      bool anyRunning = true;
      while (anyRunning)
      {
        anyRunning = false;
        for (int iClient = 0; iClient < NumThreads; iClient++)
        {
          if (procs[iClient] != null)
          {
            anyRunning = true;
            if (procs[iClient].HasExited == true)
            {
              Console.WriteLine($"Client process[{procs[iClient].Id}] has exited");
              procs[iClient] = null;
            }
            else
            {
              Thread.Sleep(250);
            }
          }
        }
      }
      Console.WriteLine("All finished");
      Console.ReadLine();
    }
  }

  public class TileProcessor
  {
    public const string PipeName = "testpipe";
    public const string PipeId = "Id";
    public const string PipeInit = "Init";
    public static void StartProcessors()
    {
      for (int iThread = 0; iThread < TileCoordinator.NumThreads; iThread++)
      {
        Thread procThread = new Thread(ServerThread);
        procThread.Start();
      }

    }

    private static void ServerThread()
    {
      NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, TileCoordinator.NumThreads);
      int threadId = Thread.CurrentThread.ManagedThreadId;

      pipeServer.WaitForConnection();
      Console.WriteLine($"Client connected to thread[{threadId}]");
      try
      {
        StreamString ss = new StreamString(pipeServer);
        ss.WriteString(TileProcessor.PipeId);

        int iTile = GetNextTile();
        while (iTile >= 0)
        {
          ss.WriteString($"{iTile}");
          string status = ss.ReadString();
          Console.WriteLine($"[{threadId}]: {status}");
          iTile = GetNextTile();
        }
        ss.WriteString($"{-1}");
        string text = ss.ReadString();
        Console.WriteLine($"[{threadId}]: return: {text}");
      }
      catch (Exception e)
      {
        Console.WriteLine($"ERROR: {Basics.Utils.GetMsg(e)}");
      }

      pipeServer.Close();
    }

    private static object _lock = new object();
    private static int GetNextTile()
    {
      return DoLocked(() =>
      {
        for (int i = 0; i < TileCoordinator.TileStatusList.Length; i++)
        {
          if (TileCoordinator.TileStatusList[i] == TileStatus.Unhandled)
          {
            TileCoordinator.TileStatusList[i] = TileStatus.Processing;
            return i;
          }
        }
        return -1;
      });
    }
    private static T DoLocked<T>(Func<T> func)
    {
      lock (_lock)
      {
        return func();
      };
    }
  }

  public class StreamString
  {
    private readonly Stream _ioStream;
    private readonly UnicodeEncoding _encoding;

    public StreamString(Stream ioStream)
    {
      _ioStream = ioStream;
      _encoding = new UnicodeEncoding();
    }

    public string ReadString()
    {
      int len = _ioStream.ReadByte() * 256 + _ioStream.ReadByte();
      byte[] buffer = new byte[len];
      _ioStream.Read(buffer, 0, len);

      string text = _encoding.GetString(buffer);
      return text;
    }

    public int WriteString(string text)
    {
      byte[] buffer = _encoding.GetBytes(text);
      int len = Math.Min(buffer.Length, ushort.MaxValue);

      _ioStream.WriteByte((byte)(len / 256));
      _ioStream.WriteByte((byte)(len % 256));
      _ioStream.Write(buffer, 0, len);
      _ioStream.Flush();

      return len + 2;
    }
  }
}
