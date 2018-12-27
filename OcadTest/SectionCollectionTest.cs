using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocad;
using Grid;
using Basics.Data;

namespace OcadTest
{
  [TestClass]
  public class CourseVerify
  {
    private const string _bahnColor = "0 252 76 0 cmyk";
    public static void Main(string[] args)
    {

      // args: [{print | <fileName>} [<ocad file> [<course doc> [<output> [workdir]]]]]
      CourseVerify course = new CourseVerify();
      course.ConvertImage(new StreamReader(@"C:\daten\felix\kapreolo\karten\stadlerberg\test.prn"),
        new StreamWriter(@"C:\daten\felix\kapreolo\karten\stadlerberg\test1.prn", false));
      //course.ConvertLiptonImage();
      //course.ConvertFlugImage();
      //course.ConvertZkbImage();
      //string a = "a";
      //while (a == "a")
      //{ continue; }

      string workDir;
      TextReader reader;
      if (args.Length <= 0)
      {
        reader = new StreamReader(@"C:\daten\felix\kapreolo\karten\stadlerberg\SOM07_v5.0.HE_.prn");
        workDir = @"C:\daten\felix\kapreolo\karten\stadlerberg\";
      }
      else
      {
        workDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
          Path.DirectorySeparatorChar;
        if (args[0] == "print")
        { reader = Console.In; }
        else
        { reader = new StreamReader(args[0]); }
      }
      string ocad = null;
      if (args.Length > 1) { ocad = args[1]; }
      string courseDoc = null;
      if (args.Length > 2) { courseDoc = args[2]; }
      string output = null;
      if (args.Length > 3) { output = args[3]; }
      if (args.Length > 4) { workDir = args[4]; }

      course.ReducePrintFile(reader, ocad, courseDoc, output, workDir);

      // test.IsIOFEqualOcad();

      //test.ReadCourseMap(Console.In);

      if (reader != Console.In)
      { reader.Close(); }

      //test.GetGlyphShape(new Font(FontFamily.Families[11], 100),'8');
    }

    private void ConvertImage(TextReader reader, TextWriter writer)
    {
      do
      {
        int xMin = 0;
        int xMax = -1;
        int yMin = 0;
        int yMax = -1;
        StringBuilder all = new StringBuilder();
        int iPixel = 0;
        int maxPixel = 12;
        string line;

        line = GetToNext(reader, NextPixel, writer, null);
        if (line == null)
        { break; }

        while (line != null)
        {
          string[] parts = line.Split();
          int ix = int.Parse(parts[0]);
          int iy = int.Parse(parts[1]);
          int nx = int.Parse(parts[2]);
          int c = int.Parse(parts[3]);
          int m = int.Parse(parts[4]);
          int y = int.Parse(parts[5]);
          int k = int.Parse(parts[6]);

          if (xMax <= xMin)
          {
            writer.WriteLine("gsave");
            writer.WriteLine(string.Format("{0} {1} translate", ix, iy));

            xMin = ix;
            yMin = iy;
            xMax = ix + nx;
            yMax = iy + 1;
          }
          if (xMax < ix + nx)
          {
            xMax = ix + nx;
          }
          if (yMax <= iy)
          {
            yMax = iy + 1;
          }

          Cmyk2Rgb(c, y, m, k, out int r, out int g, out int b);
          string hexColor = string.Format("{0:x2}{1:x2}{2:x2}", r, g, b);

          int i = nx;
          while (i > 256)
          {
            all.Append(hexColor);
            all.Append(string.Format("{0:x2}", 255));
            i -= 256;

            iPixel = NewLine(all, iPixel, maxPixel);
          }
          all.Append(hexColor);
          all.Append(string.Format("{0:x2}", i - 1));

          iPixel = NewLine(all, iPixel, maxPixel);

          line = reader.ReadLine();

          if (NextPixel(line) == false)
          { break; }
        }

        writer.WriteLine("{0} {1} 8", xMax - xMin, yMax - yMin);
        writer.WriteLine("[1 0 0 1 0 0]");
        //% red green blue magenta
        writer.WriteLine("{ count 0 eq { ");
        writer.WriteLine("  currentfile 3 string readhexstring pop ");
        writer.WriteLine("  currentfile 1 string readhexstring pop 0 get { dup } repeat ");
        writer.WriteLine(" } if } ");
        writer.WriteLine(" false 3 colorimage ");
        writer.WriteLine(all);

        writer.WriteLine("grestore");
      }
      while (true);
      reader.Close();
      writer.Close();
    }

    public void ConvertImages()
    {
      ConvertLiptonImage();
      ConvertZkbImage();
      ConvertFlugImage();
      Print();
      To.CreateXPath();
    }
    private void ConvertLiptonImage()
    {
      TextReader reader =
        new StreamReader(@"C:\daten\felix\kapreolo\karten\stadlerberg\logos\lipton300.prn");
      TextWriter writer =
        new StreamWriter(@"C:\daten\felix\kapreolo\karten\stadlerberg\logos\lipton302.prn", false);

      int xMin = 0;
      int xMax = -1;
      int yMin = 0;
      int yMax = -1;
      StringBuilder all = new StringBuilder();
      int iPixel = 0;
      int maxPixel = 12;
      string line;
      line = reader.ReadLine();
      while (line != null)
      {
        string[] parts = line.Split();
        int ix = int.Parse(parts[0]);
        int iy = int.Parse(parts[1]);
        int nx = int.Parse(parts[2]);
        int c = int.Parse(parts[3]);
        int m = int.Parse(parts[4]);
        int y = int.Parse(parts[5]);
        int k = int.Parse(parts[6]);

        if (xMax <= xMin)
        {
          xMin = ix;
          yMin = iy;
          xMax = ix + nx;
          yMax = iy + 1;
        }
        if (xMax < ix + nx)
        {
          xMax = ix + nx;
        }
        if (yMax <= iy)
        {
          yMax = iy + 1;
        }

        Cmyk2Rgb(c, y, m, k, out int r, out int g, out int b);
        string hexColor = string.Format("{0:x2}{1:x2}{2:x2}", r, g, b);

        int i = nx;
        while (i > 256)
        {
          all.Append(hexColor);
          all.Append(string.Format("{0:x2}", 255));
          i -= 256;

          iPixel = NewLine(all, iPixel, maxPixel);
        }
        all.Append(hexColor);
        all.Append(string.Format("{0:x2}", i - 1));

        iPixel = NewLine(all, iPixel, maxPixel);

        line = reader.ReadLine();
      }
      reader.Close();

      writer.WriteLine("{0} {1} 8", xMax - xMin, yMax - yMin);
      writer.WriteLine("[1 0 0 1 0 0]");
      //% red green blue magenta
      writer.WriteLine("{ count 0 eq { ");
      writer.WriteLine("  currentfile 3 string readhexstring pop ");
      writer.WriteLine("  currentfile 1 string readhexstring pop 0 get { dup } repeat ");
      writer.WriteLine(" } if } ");
      writer.WriteLine(" false 3 colorimage ");

      writer.WriteLine(all);
      writer.Close();
    }

    private void ConvertFlugImage()
    {
      GetImage(@"C:\daten\felix\kapreolo\karten\stadlerberg\logos\flughafen300.prn",
        @"C:\daten\felix\kapreolo\karten\stadlerberg\logos\flughafen302.prn", 1);
    }

    private void ConvertZkbImage()
    {
      GetImage(@"C:\daten\felix\kapreolo\karten\stadlerberg\logos\zkb300.prn",
        @"C:\daten\felix\kapreolo\karten\stadlerberg\logos\zkb302.prn", 2);
    }

    private void GetImage(string from, string to, int bits)
    {
      TextReader reader = new StreamReader(from);
      TextWriter writer = new StreamWriter(to, false);

      int iColor = 0;
      IList<string> colors = new string[1 << bits];
      int xMin = 0;
      int xMax = -1;
      int yMin = 0;
      int yMax = -1;
      StringBuilder all = new StringBuilder();
      int iPixel = 0;
      int maxPixel = 36;
      string line;
      line = reader.ReadLine();
      while (line != null)
      {
        string[] parts = line.Split();
        int ix = int.Parse(parts[0]);
        int iy = int.Parse(parts[1]);
        int nx = int.Parse(parts[2]);
        int c = int.Parse(parts[3]);
        int m = int.Parse(parts[4]);
        int y = int.Parse(parts[5]);
        int k = int.Parse(parts[6]);

        if (xMax <= xMin)
        {
          xMin = ix;
          yMin = iy;
          xMax = ix + nx;
          yMax = iy + 1;
        }
        if (xMax < ix + nx)
        {
          xMax = ix + nx;
        }
        if (yMax <= iy)
        {
          yMax = iy + 1;
        }

        Cmyk2Rgb(c, y, m, k, out int r, out int g, out int b);
        string color = string.Format("{0} {1} {2}", r, g, b);
        int ci = colors.IndexOf(color);
        if (ci < 0)
        {
          colors[iColor] = color;
          ci = iColor;
          iColor++;
        }

        int max = 256 >> bits;
        int i = nx;
        while (i > max)
        {
          all.Append(string.Format("{0:x2}", ((max - 1) << bits) + ci));
          i -= max;

          iPixel = NewLine(all, iPixel, maxPixel);
        }
        all.Append(string.Format("{0:x2}", ((i - 1) << bits) + ci));

        iPixel = NewLine(all, iPixel, maxPixel);

        line = reader.ReadLine();
      }
      reader.Close();

      writer.WriteLine("{0} {1} 8", xMax - xMin, yMax - yMin);
      writer.WriteLine("[1 0 0 1 0 0]");
      //% red green blue magenta
      writer.WriteLine("{ count 0 eq { ");
      writer.WriteLine("  currentfile 1 string readhexstring pop 0 get");
      writer.Write("  dup {0} mod [", 1 << bits);
      for (int ic = 0; ic < iColor; ic++)
      {
        writer.Write(" [{0}] ", colors[ic]);
      }
      writer.WriteLine("] exch get aload pop ");
      writer.Write("  /tColor (000) def tColor exch 2 exch put ");
      writer.WriteLine(" tColor exch 1 exch put tColor exch 0 exch put tColor");
      writer.Write(" exch {0} idiv ", 1 << bits);
      writer.WriteLine("{ dup } repeat ");
      writer.WriteLine(" } if } ");
      writer.WriteLine(" false 3 colorimage ");

      writer.WriteLine(all);
      writer.Close();
    }

    private int NewLine(StringBuilder all, int iPixel, int maxPixel)
    {
      iPixel++;
      if (iPixel >= maxPixel)
      {
        all.Append(Environment.NewLine);
        iPixel = 0;
      }
      return iPixel;
    }

    private void Cmyk2Rgb(int c, int y, int m, int k,out int r,out int g,out int b)
    {
      r = Math.Max(255 - (c + k), 0);
      g = Math.Max(255 - (m + k), 0);
      b = Math.Max(255 - (y + k), 0);
    }

    private void Print()
    {
      string line = Console.ReadLine();
      TextWriter w = new StreamWriter("C:\\temp\\xxx.txt", false);
      w.WriteLine("hallo");
      while (line != null)
      {
        w.WriteLine(line);
        line = Console.ReadLine();
      }
      w.WriteLine("tschau");
      w.Close();
      //System.Diagnostics.Process.GetCurrentProcess().WaitForInputIdle(5000);
      //      System.IO.TextReader reader = new System.IO.StreamReader(System.Diagnostics.Process.GetCurrentProcess().StandardInput.BaseStream);
    }

    [TestMethod]
    public void ReducePrintFile(TextReader reader,
      string ocadName, string combinationName, string reducedName, string workDir)
    {
      string line1 = reader.ReadLine();
      string line2 = reader.ReadLine();
      string title = "%%Title: ";
      if (line2.StartsWith(title) == false)
      { return; }
      string fileName = line2.Substring(title.Length);

      int iEnd = fileName.LastIndexOf('.');
      int iCat = fileName.Substring(0, iEnd).LastIndexOf('.');
      string cat = fileName.Substring(iCat + 1, iEnd - iCat - 1);

      if (ocadName == null || ocadName.Trim() == "")
      { ocadName = workDir + fileName.Substring(0, iCat) + ".ocd"; }
      if (combinationName == null || combinationName.Trim() == "")
      { combinationName = workDir + fileName.Substring(0, iCat) + ".Courses.xml"; }
      int minStartNr = 0;
      IList<string> validNr = null;
      if (File.Exists(ocadName))
      {
        validNr = GetValidStartNumbers(ocadName, combinationName, cat, out minStartNr);
      }

      TextWriter writer;
      if (reader == Console.In)
      { writer = Console.Out; }
      else
      {
        if (reducedName == null || reducedName.Trim() == "")
        { reducedName = workDir + fileName; }

        writer = new StreamWriter(reducedName);
      }

      writer.WriteLine(line1);
      writer.WriteLine(line2);

      string page;
      string color;
      StringBuilder store = null;

      try
      {
        int iPage = 1;
        color = GetToNext(reader, NextImageOrInfoColor, writer, null);

        do
        {
          if (NextImageColor(color))
          {
            color = HandleImages(color, reader, writer);
            writer.WriteLine(color);

            color = GetToNext(reader, NextInfoColor, writer, null);
          }

          string bahn = null;
          string code = null;
          int bahnX0 = 0, bahnY0 = 0;

          StringBuilder bahnString = new StringBuilder();
          bool infoColor = true;
          while (infoColor)
          {
            bahnString.Append(color + Environment.NewLine);
            if (color.StartsWith("0 26 0 0 "))
            {
              if (validNr != null)
              {
                code = GetString(reader, bahnString, out int x0, out int y0);
              }
            }
            else if (color.StartsWith(_bahnColor))
            {
              if (validNr != null)
              {
                bahn = GetString(reader, bahnString, out bahnX0, out bahnY0);
              }
            }
            else if (color.StartsWith("26 0 0 0 "))
            {
              _pattern = Calibrate(reader, "2345678901.ADEHK");
            }
            else
            { infoColor = false; }

            if (infoColor)
            { color = GetToNext(reader, NextColor, null, bahnString); }
          }


          int idx = -1;
          if (bahn != null && code != null)
          {
            store = new StringBuilder();

            string snr = code.Substring(0, code.IndexOf('.')).Trim();
            idx = validNr.IndexOf(snr);
          }
          if (idx >= 0 || validNr == null)
          {
            writer.WriteLine(bahnString);
            if (validNr != null)
            {
              if (code == null) throw new InvalidProgramException("code == null");
              if (bahn == null) throw new InvalidProgramException("bahn == null");
              string snr = string.Format("{0}.{1}", idx + minStartNr,
                code.Substring(code.IndexOf('.') + 1).Trim());
              if (bahn[0] == 'D')
              {
                bahnX0 -= (int)_d.X0;
                bahnY0 -= (int)_d.Y0;
              }
              writer.WriteLine(StartNrString(snr, bahnX0, bahnY0 + (int)(_pattern[0].Height() * 1.5)));
            }

            GetToNext(reader, NextPageEnd, writer, null);
            writer.WriteLine("%%[Page: {0}]%%) =", iPage);
            iPage++;
            page = GetToNext(reader, NextPage, null, store);
            if (page != null && page.StartsWith("%%Page:"))
            {
              writer.WriteLine(store);
              page = string.Format("%%Page: {0} {0}", iPage);
              writer.WriteLine(page);
              color = GetToNext(reader, NextImageOrInfoColor, writer, null);
            }
          }
          else
          {
            GetToNext(reader, NextPageEnd, null, null);
            page = GetToNext(reader, NextPage, null, store);
            if (page != null && page.StartsWith("%%Page:"))
            {
              if (bahn != null && code != null)
              {
                color = GetToNext(reader, NextInfoColor, null, null);
              }
              else
              {
                color = GetToNext(reader, NextImageOrInfoColor, null, null);
              }
            }
            else
            {
              writer.WriteLine("grestore");
              writer.WriteLine("%%Trailer");
              writer.WriteLine("%%EOF");
              writer.WriteLine("");
              writer.WriteLine("%%EndDocument");
              writer.WriteLine("");
              writer.WriteLine("Pscript_WinNT_Compat dup /suspend get exec");
              writer.WriteLine("Pscript_WinNT_Incr dup /resume get exec");
              writer.WriteLine("LH");

              writer.WriteLine("%%[Page: {0}]%%) =", iPage);
            }
          }
        }
        while (page != null && page.StartsWith("%%Page:"));

        /*
        */
        writer.WriteLine(store);
        GetToNext(reader, NextPageEnd, writer, null);
      }
      finally
      {
        if (writer != Console.Out)
        { writer.Close(); }
      }
    }

    private string HandleImages(string color, TextReader reader, TextWriter writer)
    {
      bool imageColor;
      StringBuilder imageBuilder = new StringBuilder();
      do
      {
        imageColor = true;
        if (color.StartsWith("0 122 122 0 "))
        {
          PutImage(reader, imageBuilder, "lipton302.prn");
        }
        else if (color.StartsWith("122 0 122 0 "))
        {
          PutImage(reader, imageBuilder, "flughafen302.prn");
        }
        else if (color.StartsWith("122 122 0 0 "))
        {
          PutImage(reader, imageBuilder, "zkb302.prn");
        }
        else
        {
          imageColor = false;
          writer.WriteLine(imageBuilder);
        }

        if (imageColor)
        {
          color = GetToNext(reader, NextColor, null, null);
        }
      } while (imageColor);
      return color;
    }

    private void PutImage(TextReader reader, StringBuilder builder, string image)
    {
      string path = Path.GetDirectoryName(
        System.Reflection.Assembly.GetExecutingAssembly().Location);
      image = path + Path.DirectorySeparatorChar + image;
      TextReader imageReader = new StreamReader(image);

      string line;
      string[] split;

      line = reader.ReadLine();
      split = line.Split();
      double x0 = double.Parse(split[1]);
      double y0 = double.Parse(split[2]);

      line = reader.ReadLine();
      split = line.Split();
      double dx = double.Parse(split[0]);

      line = reader.ReadLine();
      split = line.Split();
      double dy = double.Parse(split[1]);

      line = imageReader.ReadLine();
      split = line.Split();
      int nx = int.Parse(split[0]);
      int ny = int.Parse(split[1]);

      double fx = dx / nx;
      double fy = dy / ny;
      double scale = Math.Round((fx + fy) / 2.0, 1);

      builder.AppendLine("gsave");
      builder.AppendLine(string.Format("{0} {1} translate", x0, y0));
      builder.AppendLine(string.Format("{0} {0} scale", scale));
      builder.AppendLine(line);
      line = imageReader.ReadLine();
      while (line != null)
      {
        builder.AppendLine(line);
        line = imageReader.ReadLine();
      }
      imageReader.Close();
      builder.AppendLine("grestore");
    }

    private List<Letter> Calibrate(TextReader reader, string charList)
    {
      bool first = true;
      int iChar = 0;

      double w0 = 0;
      List<Letter> list = new List<Letter>();
      List<Pattern> ppp = new List<Pattern>();
      double xl0 = 0;
      double xl1 = 0;
      double yl0 = 0;
      double yl1 = 0;

      double x1l0 = 0;

      string orig = reader.ReadLine();
      while (orig.StartsWith("0 O") == false)
      {
        string sPattern = GetPattern(reader);
        Pattern p = new Pattern(sPattern);
        string[] o = orig.Split();
        double xs = double.Parse(o[1]);
        double ys = double.Parse(o[2]);

        if (first)
        {
          Letter l0 = new Letter(charList[iChar], p);
          list.Add(l0);
          x1l0 = xs - p.X0 + p.Width();
          first = false;

          iChar++;
        }
        else
        {
          double x0 = xs - p.X0;
          double y1 = ys - p.Y0;
          double x1 = x0 + p.Width();
          double y0 = y1 - p.Height();

          if (ppp.Count == 0 || x0 > xl1 + w0 / 2)
          {
            if (ppp.Count > 0)
            {
              list.Add(new Letter(charList[iChar], ppp));
              ppp = new List<Pattern>();
              iChar++;
            }
            else
            {
              w0 = x0 - x1l0;
            }

            xl0 = x0;
            xl1 = x1;
            yl0 = y0;
            yl1 = y1;
            ppp.Add(p);
          }
          else
          {
            if (x0 < xl0)
            {
              foreach (var op in ppp)
              {
                op.X0 += xl0 - x0;
              }
            }
            else
            {
              p.X0 += x0 - xl0;
            }

            if (y1 > yl1)
            {
              foreach (var op in ppp)
              {
                op.Y0 -= y1 - yl1;
              }
            }
            else
            {
              p.Y0 -= yl1 - y1;
            }
            xl0 = Math.Min(xl0, x0);
            xl1 = Math.Max(xl1, x1);
            yl0 = Math.Min(yl0, y0);
            yl1 = Math.Max(yl1, y1);
            ppp.Add(p);
          }
        }
        orig = reader.ReadLine();
      }

      list.Add(new Letter(charList[iChar], ppp));
      return list;
    }

    private string StartNrString(string leg, int x0, int y0)
    {
      StringBuilder all = new StringBuilder();
      LetterGraphics(all, leg, x0, y0);
      all.Append("0 O" + Environment.NewLine);
      return all.ToString();
    }

    private double LetterGraphics(StringBuilder all, string word, int x0, int y0)
    {
      double x = x0;
      foreach (var c in word)
      {
        foreach (var letter in _pattern)
        {
          if (letter.Char == c)
          {
            letter.AppendTo(all, (int)x, y0);
            x = x + letter.Width() + _pattern[0].Width() * 0.2;
            break;
          }
        }
      }
      return x;
    }

    #region glyph
    [DllImport("gdi32.dll")]
    static extern uint GetGlyphOutline(IntPtr hdc, uint uChar, uint uFormat,
       out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(
        IntPtr hdc, IntPtr hgdiobj);

    public struct TTPOLYGONHEADER
    {
      public int cb;
      public int dwType;
      [MarshalAs(UnmanagedType.Struct)]
      public POINTFX pfxStart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TTPOLYCURVEHEADER
    {
      public short wType;
      public short cpfx;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FIXED
    {
      public short fract;
      public short value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MAT2
    {
      [MarshalAs(UnmanagedType.Struct)]
      public FIXED eM11;
      [MarshalAs(UnmanagedType.Struct)]
      public FIXED eM12;
      [MarshalAs(UnmanagedType.Struct)]
      public FIXED eM21;
      [MarshalAs(UnmanagedType.Struct)]
      public FIXED eM22;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTFX
    {
      [MarshalAs(UnmanagedType.Struct)]
      public FIXED x;
      [MarshalAs(UnmanagedType.Struct)]
      public FIXED y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GLYPHMETRICS
    {
      public int gmBlackBoxX;
      public int gmBlackBoxY;
      [MarshalAs(UnmanagedType.Struct)]
      public POINTFX gmptGlyphOrigin;
      public short gmCellIncX;
      public short gmCellIncY;
    }



    // Parse a glyph outline in native format
    [TestMethod]
    public void GetGlyphShape(Font font, char c)
    {
      MAT2 matrix = new MAT2();
      matrix.eM11.value = 1;
      matrix.eM12.value = 0;
      matrix.eM21.value = 0;
      matrix.eM22.value = 1;

      using (Bitmap b = new Bitmap(1, 1))
      {
        using (Graphics g = Graphics.FromImage(b))
        {
          IntPtr hdc = g.GetHdc();
          IntPtr prev = SelectObject(hdc, font.ToHfont());
          int bufferSize = (int)GetGlyphOutline(hdc, c, 2, out GLYPHMETRICS metrics, 0, IntPtr.Zero, ref matrix);
          //bufferSize = 1000000;
          IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
          try
          {
            uint ret;
            if ((ret = GetGlyphOutline(hdc, c, 2, out metrics, (uint)bufferSize, buffer, ref matrix)) > 0)
            {
              int polygonHeaderSize = Marshal.SizeOf(typeof(TTPOLYGONHEADER));
              int curveHeaderSize = Marshal.SizeOf(typeof(TTPOLYCURVEHEADER));
              int pointFxSize = Marshal.SizeOf(typeof(POINTFX));

              int index = 0;
              while (index < bufferSize)
              {
                TTPOLYGONHEADER header = (TTPOLYGONHEADER)Marshal.PtrToStructure(
                  new IntPtr(buffer.ToInt32() + index), typeof(TTPOLYGONHEADER));

                // ...do something with start coords...
                int endCurvesIndex = index + header.cb;
                index += polygonHeaderSize;

                while (index < endCurvesIndex)
                {
                  TTPOLYCURVEHEADER curveHeader = (TTPOLYCURVEHEADER)Marshal.PtrToStructure(
                    new IntPtr(buffer.ToInt32() + index), typeof(TTPOLYCURVEHEADER));
                  index += curveHeaderSize;

                  POINTFX[] curvePoints = new POINTFX[curveHeader.cpfx];

                  for (int i = 0; i < curveHeader.cpfx; i++)
                  {
                    curvePoints[i] = (POINTFX)Marshal.PtrToStructure(new IntPtr(buffer.ToInt32() + index), typeof(POINTFX));
                    index += pointFxSize;
                  }

                  if (curveHeader.wType == 1)
                  {
                    // POLYLINE
                    for (int i = 0; i < curveHeader.cpfx; i++)
                    {
                      // ...do something with line points...

                    }
                  }
                  else
                  {
                    // CURVE
                    for (int i = 0; i < curveHeader.cpfx - 1; i++)
                    {
                      //POINTFX pfxB = curvePoints[i];
                      //POINTFX pfxC = curvePoints[i + 1];

                      if (i < curveHeader.cpfx - 2)
                      {
                        //int x = (short)((pfxB.x.value + pfxC.x.value) / 2);
                        //x = (short)-((pfxB.y.value + pfxC.y.value) / 2);
                      }
                      else
                      {
                        //int x = (short)-pfxC.y.value;
                      } // ...do something with curve points...



                    }
                  }
                }
              }
            }
            else
            {
              throw new Exception("Could not retrieve glyph (GDI Error: 0x" + ret.ToString("X") + ")");
            }

            g.ReleaseHdc(hdc);
          }
          finally
          {
            Marshal.FreeHGlobal(buffer);
          }

        }
      }
    }


    #endregion
    private string GetString(TextReader reader, StringBuilder store,
      out int x0, out int y0)
    {
      string code = "";
      string line;
      line = reader.ReadLine();
      string[] parts = line.Split();
      x0 = int.Parse(parts[1]);
      y0 = int.Parse(parts[2]);
      while (line.StartsWith("0 O") == false)
      {
        if (store != null)
        { store.Append(line + Environment.NewLine); }

        code = code + GetLetter(reader, store);
        line = reader.ReadLine();
      }
      if (store != null)
      { store.Append(line + Environment.NewLine); }

      return code;
    }

    private class Pattern
    {
      double _x0;
      double _y0;
      readonly List<To> _path;

      public Pattern(string path)
      {
        _path = To.CreatePath(path);
        _x0 = -To.MinX(_path);
        _y0 = -To.MaxY(_path);
      }
      public Pattern(double x0, double y0, string path)
      {
        _x0 = x0;
        _y0 = y0;
        _path = To.CreatePath(path);
      }

      public double X0
      {
        get { return _x0; }
        set { _x0 = value; }
      }
      public double Y0
      {
        get { return _y0; }
        set { _y0 = value; }
      }
      public List<To> Path
      {
        get { return _path; }
      }

      public void AppendGraphics(StringBuilder all, int x0, int y0)
      {
        all.Append(string.Format("n {0} {1} m", (int)(x0 + _x0), (int)(y0 + _y0)));
        all.Append(Environment.NewLine);
        foreach (var to in _path)
        {
          to.AppendTo(all);
          all.Append(Environment.NewLine);
        }
        all.Append("f" + Environment.NewLine);
      }

      internal double Width()
      {
        return To.MaxX(_path) - To.MinX(_path);
      }

      internal double Height()
      {
        return To.MaxY(_path) - To.MinY(_path);
      }

      internal double MinX()
      {
        return To.MinX(_path);
      }

      internal double MaxX()
      {
        return To.MaxX(_path);
      }

      internal double MinY()
      {
        return To.MinY(_path);
      }

      internal double MaxY()
      {
        return To.MaxY(_path);
      }

    }

    private class Letter
    {
      private readonly char _letter;
      private List<Pattern> _patternList;

      public Letter(char letter, double x0, double y0, string path)
      {
        _letter = letter;
        _patternList = new List<Pattern>();
        _patternList.Add(new Pattern(x0, y0, path));
      }

      public Letter(char letter, IEnumerable<Pattern> pattern)
      {
        _letter = letter;
        _patternList = new List<Pattern>(pattern);
      }

      public Letter(char letter, params Pattern[] pattern)
      {
        _letter = letter;
        _patternList = new List<Pattern>(pattern);
      }

      public double X0
      {
        get { return _patternList[0].X0; }
      }
      public double Y0
      {
        get { return _patternList[0].Y0; }
      }
      public List<Pattern> PatternList
      {
        get { return _patternList; }
      }
      public char Char
      {
        get { return _letter; }
      }

      public void AppendTo(StringBuilder builder, int x0, int y0)
      {
        foreach (var p in _patternList)
        {
          p.AppendGraphics(builder, x0, y0);
        }
      }

      public double Height()
      {
        double hMin = double.MaxValue;
        double hMax = double.MinValue;
        foreach (var p in _patternList)
        {
          hMax = Math.Max(hMax, p.Y0 + p.MaxY());
          hMin = Math.Min(hMin, p.Y0 + p.MinY());
        }
        return hMax - hMin;
      }

      public double Width()
      {
        double wMin = double.MaxValue;
        double wMax = double.MinValue;
        foreach (var p in _patternList)
        {
          wMax = Math.Max(wMax, p.X0 + p.MaxX());
          wMin = Math.Min(wMin, p.X0 + p.MinX());
        }
        return wMax - wMin;
      }
    }
    #region letter patterns
    #region .
 private readonly Letter _point = new Letter('.', 0, 2079 - 2079,
      "0 -16 l" + Environment.NewLine +
   "17 0 l" + Environment.NewLine +
   "0 16 l" + Environment.NewLine);
 #endregion

    /* 
         * . : 7970 2079
         * 0 : 8039 2020, 7961 2020
         * 1 : 7924 2079, (8017 2079, 8063 2079)
         * 2 : 8038 2065,
         * 3 : 7869 2048
         * 4 : 7930 2019, 7876 2019
         * 5 : 7869 2048
         * 6 : 7942 2020, 7868 2020
         * 7 : 7870 1976
         * 8 : 7908 2081, 7908 2020, 7908 2020, 7908 1959
         * 9 : 7947 2020, 7873 2020
         * E : 7995 1859
         * H : 7875 1859
         */

    #region 0
    private readonly Letter _0 = new Letter('0',
      new Pattern(8039 - 7961, 2020 - 2079,
    "0 15 -2 26 -4 35 c" + Environment.NewLine +
"-3 8 -8 15 -13 19 c" + Environment.NewLine +
"-6 5 -13 7 -22 7 c" + Environment.NewLine +
"-12 0 -20 -4 -27 -12 c" + Environment.NewLine +
"-8 -10 -12 -26 -12 -49 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine +
"15 0 l" + Environment.NewLine +
"0 20 3 33 7 40 c" + Environment.NewLine +
"5 6 10 9 17 9 c" + Environment.NewLine +
"7 0 12 -3 17 -9 c" + Environment.NewLine +
"5 -7 7 -20 7 -40 c" + Environment.NewLine +
"0 0 0 0 0 0 c" + Environment.NewLine),

      new Pattern(7961 - 7961, 2020 - 2079,
  "0 -13 1 -25 5 -34 c" + Environment.NewLine +
"2 -8 7 -15 12 -19 c" + Environment.NewLine +
"6 -5 13 -7 22 -7 c" + Environment.NewLine +
"6 0 12 1 17 4 c" + Environment.NewLine +
"5 2 9 6 12 11 c" + Environment.NewLine +
"3 4 6 11 7 17 c" + Environment.NewLine +
"2 7 3 17 3 28 c" + Environment.NewLine +
"0 0 0 0 0 0 c" + Environment.NewLine +
"-15 0 l" + Environment.NewLine +
"0 -19 -2 -32 -7 -38 c" + Environment.NewLine +
"-5 -7 -10 -10 -17 -10 c" + Environment.NewLine +
"-7 0 -12 3 -16 8 c" + Environment.NewLine +
"-5 8 -8 21 -8 40 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine));

    #endregion

    #region 1
    private readonly Letter _1 = new Letter ('1', 7924 - 7869, 2079 - 2079,
      "-15 0 l" + Environment.NewLine +
      "0 -93 l" + Environment.NewLine +
      "-4 4 -8 7 -14 10 c" + Environment.NewLine +
      "-6 3 -11 6 -15 8 c" + Environment.NewLine +
      "0 -14 l" + Environment.NewLine +
      "8 -4 15 -9 21 -14 c" + Environment.NewLine +
      "6 -6 11 -11 13 -16 c" + Environment.NewLine +
      "10 0 l" + Environment.NewLine);
    #endregion

    #region 2
 private readonly Letter _2 = new Letter('2', 78, 2065 - 2079,
      "0 14 l" + Environment.NewLine +
   "-78 0 l" + Environment.NewLine +
   "-1 -3 0 -7 1 -10 c" + Environment.NewLine +
   "2 -5 5 -10 10 -16 c" + Environment.NewLine +
   "4 -5 11 -11 19 -18 c" + Environment.NewLine +
   "13 -10 22 -19 26 -25 c" + Environment.NewLine +
   "5 -6 7 -12 7 -18 c" + Environment.NewLine +
   "0 -5 -2 -10 -6 -14 c" + Environment.NewLine +
   "-4 -4 -10 -6 -16 -6 c" + Environment.NewLine +
   "-7 0 -13 2 -17 6 c" + Environment.NewLine +
   "-4 4 -6 10 -7 18 c" + Environment.NewLine +
   "-15 -2 l" + Environment.NewLine +
   "1 -11 5 -20 12 -25 c" + Environment.NewLine +
   "7 -6 16 -9 27 -9 c" + Environment.NewLine +
   "12 0 20 3 27 9 c" + Environment.NewLine +
   "7 7 10 14 10 24 c" + Environment.NewLine +
   "0 5 -1 9 -3 14 c" + Environment.NewLine +
   "-2 5 -5 9 -10 14 c" + Environment.NewLine +
   "-4 5 -12 12 -22 21 c" + Environment.NewLine +
   "-9 8 -14 13 -17 15 c" + Environment.NewLine +
   "-2 3 -4 6 -6 8 c" + Environment.NewLine);
    #endregion 2

    #region 3
    private readonly Letter _3 = new Letter ('3', 7869 - 7869, 2048 - 2079,
      "15 -2 l" + Environment.NewLine +
      "2 8 5 14 9 18 c" + Environment.NewLine +
      "4 3 9 5 15 5 c" + Environment.NewLine +
      "6 0 12 -2 17 -7 c" + Environment.NewLine +
      "5 -4 7 -10 7 -17 c" + Environment.NewLine +
      "0 -7 -2 -13 -7 -17 c" + Environment.NewLine +
      "-4 -4 -9 -7 -16 -7 c" + Environment.NewLine +
      "-3 0 -6 1 -11 2 c" + Environment.NewLine +
      "2 -13 l" + Environment.NewLine +
      "1 0 2 0 2 0 c" + Environment.NewLine +
      "7 0 12 -1 17 -5 c" + Environment.NewLine +
      "5 -3 8 -8 8 -15 c" + Environment.NewLine +
      "0 -5 -2 -9 -6 -13 c" + Environment.NewLine +
      "-3 -4 -8 -5 -14 -5 c" + Environment.NewLine +
      "-5 0 -10 1 -14 5 c" + Environment.NewLine +
      "-4 4 -6 9 -7 16 c" + Environment.NewLine +
      "-15 -3 l" + Environment.NewLine +
      "2 -9 6 -17 12 -22 c" + Environment.NewLine +
      "7 -6 14 -8 24 -8 c" + Environment.NewLine +
      "6 0 12 1 18 4 c" + Environment.NewLine +
      "5 3 9 6 13 11 c" + Environment.NewLine +
      "2 5 4 10 4 16 c" + Environment.NewLine +
      "0 5 -2 9 -4 14 c" + Environment.NewLine +
      "-3 4 -7 7 -13 10 c" + Environment.NewLine +
      "7 1 13 5 17 10 c" + Environment.NewLine +
      "4 5 6 12 6 19 c" + Environment.NewLine +
      "0 11 -4 19 -12 27 c" + Environment.NewLine +
      "-7 7 -17 10 -28 10 c" + Environment.NewLine +
      "-11 0 -20 -3 -27 -9 c" + Environment.NewLine +
      "-7 -6 -11 -14 -12 -24 c" + Environment.NewLine);
    #endregion

    #region 4
    private readonly Letter _4 = new Letter('4',
      new Pattern(7930 - 7869, 2019 - 2079,
      "0 17 l" + Environment.NewLine +
      "16 0 l" + Environment.NewLine +
      "0 14 l" + Environment.NewLine +
      "-16 0 l" + Environment.NewLine +
      "0 28 l" + Environment.NewLine +
      "-15 0 l" + Environment.NewLine +
      "0 -28 l" + Environment.NewLine +
      "-51 0 l" + Environment.NewLine +
      "0 -14 l" + Environment.NewLine +
      "12 -17 l" + Environment.NewLine +
      "14 0 l" + Environment.NewLine +
      "-12 17 l" + Environment.NewLine +
      "37 0 l" + Environment.NewLine +
      "0 -17 l" + Environment.NewLine),

      new Pattern(7876 - 7869, 2019 - 2079,
      "42 -60 l" + Environment.NewLine +
      "12 0 l" + Environment.NewLine +
      "0 60 l" + Environment.NewLine +
      "-15 0 l" + Environment.NewLine +
      "0 -36 l" + Environment.NewLine +
      "-25 36 l" + Environment.NewLine));
    #endregion

    #region 5
    private readonly Letter _5 = new Letter ('5', 7869 - 7869, 2048 - 2079,
    "16 -1 l" + Environment.NewLine +
    "1 7 4 13 8 17 c" + Environment.NewLine +
    "4 3 9 5 15 5 c" + Environment.NewLine +
    "7 0 13 -2 18 -8 c" + Environment.NewLine +
    "5 -5 7 -12 7 -21 c" + Environment.NewLine +
    "0 -8 -2 -15 -7 -20 c" + Environment.NewLine +
    "-5 -4 -11 -7 -18 -7 c" + Environment.NewLine +
    "-5 0 -9 1 -13 3 c" + Environment.NewLine +
    "-4 3 -7 5 -9 9 c" + Environment.NewLine +
    "-14 -2 l" + Environment.NewLine +
    "12 -61 l" + Environment.NewLine +
    "59 0 l" + Environment.NewLine +
    "0 14 l" + Environment.NewLine +
    "-48 0 l" + Environment.NewLine +
    "-6 32 l" + Environment.NewLine +
    "7 -5 14 -8 23 -8 c" + Environment.NewLine +
    "10 0 19 4 26 11 c" + Environment.NewLine +
    "7 7 11 17 11 28 c" + Environment.NewLine +
    "0 11 -4 20 -10 28 c" + Environment.NewLine +
    "-8 10 -18 14 -31 14 c" + Environment.NewLine +
    "-11 0 -20 -3 -27 -9 c" + Environment.NewLine +
    "-7 -6 -11 -14 -12 -24 c" + Environment.NewLine);
    #endregion

    #region 6
    private readonly Letter _6 = new Letter('6',
      new Pattern(7942 - 7869, 2020 - 2079,
      "3 6 5 13 5 21 c" + Environment.NewLine +
      "0 7 -2 14 -5 21 c" + Environment.NewLine +
      "-4 6 -8 11 -14 14 c" + Environment.NewLine +
      "-5 4 -12 5 -19 5 c" + Environment.NewLine +
      "-12 0 -22 -4 -30 -13 c" + Environment.NewLine +
      "-7 -9 -11 -24 -11 -44 c" + Environment.NewLine +
      "0 -1 0 -3 0 -4 c" + Environment.NewLine +
      "26 0 l" + Environment.NewLine +
      "-1 1 -2 2 -2 2 c" + Environment.NewLine +
      "-5 5 -7 11 -7 19 c" + Environment.NewLine +
      "0 0 l" + Environment.NewLine +
      "0 5 1 10 3 14 c" + Environment.NewLine +
      "2 5 5 8 9 11 c" + Environment.NewLine +
      "4 2 8 3 12 3 c" + Environment.NewLine +
      "6 0 11 -2 16 -7 c" + Environment.NewLine +
      "4 -5 7 -12 7 -20 c" + Environment.NewLine +
      "0 -9 -3 -15 -7 -20 c" + Environment.NewLine +
      "-1 0 -1 -1 -2 -2 c" + Environment.NewLine),

    new Pattern(7868 - 7869, 2020 - 2079,
      "0 -21 4 -37 13 -47 c" + Environment.NewLine +
      "7 -9 17 -13 30 -13 c" + Environment.NewLine +
      "9 0 17 2 23 8 c" + Environment.NewLine +
      "6 5 9 12 11 21 c" + Environment.NewLine +
      "0 0 l" + Environment.NewLine +
      "-15 1 l" + Environment.NewLine +
      "-2 -5 -3 -9 -6 -12 c" + Environment.NewLine +
      "-4 -4 -9 -6 -14 -6 c" + Environment.NewLine +
      "-5 0 -9 1 -13 4 c" + Environment.NewLine +
      "-4 3 -8 8 -11 14 c" + Environment.NewLine +
      "-2 7 -4 16 -4 28 c" + Environment.NewLine +
      "4 -6 8 -10 13 -12 c" + Environment.NewLine +
      "5 -3 11 -4 16 -4 c" + Environment.NewLine +
      "10 0 18 4 25 11 c" + Environment.NewLine +
      "2 2 4 5 6 7 c" + Environment.NewLine +
      "-19 0 l" + Environment.NewLine +
      "-4 -3 -9 -5 -15 -5 c" + Environment.NewLine +
      "-6 0 -11 2 -15 5 c" + Environment.NewLine));
    #endregion

    #region 7
    private readonly Letter _7 = new Letter ('7', 7870 - 7869, 1976 - 2079,
      "0 -14 l" + Environment.NewLine +
      "78 0 l" + Environment.NewLine +
      "0 11 l" + Environment.NewLine +
      "-8 8 -15 19 -23 32 c" + Environment.NewLine +
      "-7 14 -13 28 -17 42 c" + Environment.NewLine +
      "-3 10 -5 20 -6 32 c" + Environment.NewLine +
      "-15 0 l" + Environment.NewLine +
      "0 -9 2 -20 6 -34 c" + Environment.NewLine +
      "3 -13 8 -26 15 -38 c" + Environment.NewLine +
      "6 -12 13 -23 21 -31 c" + Environment.NewLine);
    #endregion

    #region 8
    private readonly Letter _8 = new Letter('8',
      new Pattern(7908 - 7869, 2081 - 2079,
    "-11 0 -21 -3 -28 -10 c" + Environment.NewLine +
"-7 -7 -11 -16 -11 -26 c" + Environment.NewLine +
"0 -8 2 -14 6 -20 c" + Environment.NewLine +
"2 -1 3 -3 5 -5 c" + Environment.NewLine +
"28 0 l" + Environment.NewLine +
"0 1 l" + Environment.NewLine +
"0 0 0 0 0 0 c" + Environment.NewLine +
"-7 0 -12 2 -17 7 c" + Environment.NewLine +
"-5 5 -7 10 -7 17 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine +
"0 4 1 8 3 12 c" + Environment.NewLine +
"2 4 5 7 9 9 c" + Environment.NewLine +
"4 2 8 3 12 3 c" + Environment.NewLine),

      new Pattern(7908 - 7869, 2020 - 2079,
  "29 0 l" + Environment.NewLine +
"2 2 4 4 5 6 c" + Environment.NewLine +
"4 6 6 12 6 19 c" + Environment.NewLine +
"0 10 -4 19 -11 26 c" + Environment.NewLine +
"-7 7 -17 10 -29 10 c" + Environment.NewLine +
"0 0 0 0 0 0 c" + Environment.NewLine +
"0 -12 l" + Environment.NewLine +
"0 0 1 0 1 0 c" + Environment.NewLine +
"7 0 12 -2 17 -6 c" + Environment.NewLine +
"5 -5 7 -11 7 -17 c" + Environment.NewLine +
"0 -8 -2 -13 -7 -18 c" + Environment.NewLine +
"-5 -5 -10 -7 -18 -7 c" + Environment.NewLine),

      new Pattern(7908 - 7869, 2020 - 2079,
"-28 0 l" + Environment.NewLine +
"3 -2 7 -4 12 -5 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine +
"-6 -3 -11 -6 -14 -10 c" + Environment.NewLine +
"-2 -4 -4 -9 -4 -15 c" + Environment.NewLine +
"0 -8 3 -16 9 -21 c" + Environment.NewLine +
"7 -6 15 -9 25 -9 c" + Environment.NewLine +
"0 0 0 0 0 0 c" + Environment.NewLine +
"0 12 l" + Environment.NewLine +
"-5 0 -10 1 -13 5 c" + Environment.NewLine +
"-4 4 -6 8 -6 13 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine +
"0 5 2 10 6 14 c" + Environment.NewLine +
"3 3 8 5 13 5 c" + Environment.NewLine),

    new Pattern(7908 - 7869, 1959 - 2079,
"11 0 19 3 25 9 c" + Environment.NewLine +
"7 6 10 13 10 22 c" + Environment.NewLine +
"0 5 -1 10 -4 14 c" + Environment.NewLine +
"-3 4 -8 7 -14 10 c" + Environment.NewLine +
"5 1 8 3 12 5 c" + Environment.NewLine +
"-29 0 l" + Environment.NewLine +
"0 -11 l" + Environment.NewLine +
"1 0 1 0 1 0 c" + Environment.NewLine +
"5 0 10 -2 14 -5 c" + Environment.NewLine +
"3 -4 5 -8 5 -13 c" + Environment.NewLine +
"0 -6 -2 -10 -6 -14 c" + Environment.NewLine +
"-3 -4 -8 -5 -14 -5 c" + Environment.NewLine +
"0 0 0 0 0 0 c" + Environment.NewLine));

    #endregion

    #region 9
    private readonly Letter _9 = new Letter('9',
      new Pattern(7947 - 7869, 2020 - 2079,
    "0 14 -2 26 -5 34 c" + Environment.NewLine +
"-3 9 -8 16 -15 20 c" + Environment.NewLine +
"-6 5 -14 7 -23 7 c" + Environment.NewLine +
"-9 0 -17 -2 -23 -7 c" + Environment.NewLine +
"-6 -6 -9 -13 -10 -22 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine +
"14 -2 l" + Environment.NewLine +
"1 7 3 12 7 15 c" + Environment.NewLine +
"3 3 7 4 12 4 c" + Environment.NewLine +
"5 0 9 -1 12 -3 c" + Environment.NewLine +
"4 -2 7 -4 9 -8 c" + Environment.NewLine +
"2 -3 4 -8 5 -14 c" + Environment.NewLine +
"2 -6 2 -12 2 -18 c" + Environment.NewLine +
"0 -1 0 -1 0 -3 c" + Environment.NewLine +
"-3 5 -7 9 -12 12 c" + Environment.NewLine +
"-5 2 -10 4 -16 4 c" + Environment.NewLine +
"-10 0 -19 -4 -25 -11 c" + Environment.NewLine +
"-2 -2 -4 -5 -6 -8 c" + Environment.NewLine +
"19 0 l" + Environment.NewLine +
"4 4 10 6 15 6 c" + Environment.NewLine +
"6 0 12 -2 16 -6 c" + Environment.NewLine),

    new Pattern(7873 - 7869, 2020 - 2079,
  "-3 -5 -5 -12 -5 -20 c" + Environment.NewLine +
"0 -12 4 -22 11 -29 c" + Environment.NewLine +
"7 -8 16 -11 27 -11 c" + Environment.NewLine +
"8 0 15 2 21 6 c" + Environment.NewLine +
"7 4 12 10 15 18 c" + Environment.NewLine +
"3 7 5 19 5 33 c" + Environment.NewLine +
"0 1 0 2 0 3 c" + Environment.NewLine +
"-25 0 l" + Environment.NewLine +
"1 0 1 -1 2 -1 c" + Environment.NewLine +
"4 -5 7 -12 7 -20 c" + Environment.NewLine +
"0 0 l" + Environment.NewLine +
"0 -8 -3 -15 -7 -20 c" + Environment.NewLine +
"-4 -5 -10 -7 -16 -7 c" + Environment.NewLine +
"-7 0 -12 2 -17 8 c" + Environment.NewLine +
"-5 5 -7 12 -7 20 c" + Environment.NewLine +
"0 8 2 14 6 19 c" + Environment.NewLine +
"1 0 1 1 2 1 c" + Environment.NewLine));

    #endregion

    #region A

    private readonly Letter _a = new Letter ('A',
      new Pattern(74, -59,
    "25 59 l" + Environment.NewLine +
"-18 0 l" + Environment.NewLine +
"-14 -36 l" + Environment.NewLine +
"-50 0 l" + Environment.NewLine +
"-13 36 l" + Environment.NewLine +
"-17 0 l" + Environment.NewLine +
"23 -59 l" + Environment.NewLine +
"15 0 l" + Environment.NewLine +
"-4 10 l" + Environment.NewLine +
"41 0 l" + Environment.NewLine +
"-4 -10 l" + Environment. NewLine),
      new Pattern(9, -59,
"23 -60 l" + Environment.NewLine +
"17 0 l" + Environment.NewLine +
"24 60 l" + Environment.NewLine +
"-16 0 l" + Environment.NewLine +
"-9 -22 l" + Environment.NewLine +
"-3 -10 -6 -19 -8 -25 c" + Environment.NewLine +
"-1 8 -4 15 -6 23 c" + Environment.NewLine +
"-10 24 l" + Environment.NewLine
      ));
    #endregion

    #region D
    private Letter _d = new Letter ('D',
      new Pattern(98, -59,
"0 9 -1 17 -3 23 c" + Environment.NewLine +
"-2 7 -4 13 -8 18 c" + Environment.NewLine +
"-3 4 -7 8 -11 11 c" + Environment.NewLine +
"-4 2 -8 4 -14 6 c" + Environment.NewLine +
"-5 1 -12 1 -19 1 c" + Environment.NewLine +
"-43 0 l" + Environment.NewLine +
"0 -59 l" + Environment.NewLine +
"16 0 l" + Environment.NewLine +
"0 46 l" + Environment.NewLine +
"25 0 l" + Environment.NewLine +
"8 0 14 -1 19 -3 c" + Environment.NewLine +
"4 -1 8 -3 10 -6 c" + Environment.NewLine +
"4 -3 7 -9 9 -15 c" + Environment.NewLine +
"2 -6 3 -13 3 -22 c" + Environment. NewLine),

      new Pattern(0, -59,
"0 -60 l" + Environment.NewLine +
"41 0 l" + Environment.NewLine +
"9 0 16 1 21 2 c" + Environment.NewLine +
"7 2 13 4 18 9 c" + Environment.NewLine +
"6 5 11 12 14 20 c" + Environment.NewLine +
"3 8 4 18 4 28 c" + Environment.NewLine +
"0 0 0 1 0 1 c" + Environment.NewLine +
"-16 0 l" + Environment.NewLine +
"0 -1 0 -1 0 -1 c" + Environment.NewLine +
"0 -12 -2 -22 -6 -29 c" + Environment.NewLine +
"-4 -6 -9 -11 -15 -13 c" + Environment.NewLine +
"-4 -2 -11 -3 -20 -3 c" + Environment.NewLine +
"-25 0 l" + Environment.NewLine +
"0 46 l" + Environment.NewLine));

    #endregion
    #region E
 private readonly Letter _e = new Letter('E', 0, 0,
      "0 -119 l" + Environment.NewLine +
"86 0 l" + Environment.NewLine +
"0 14 l" + Environment.NewLine +
"-70 0 l" + Environment.NewLine +
"0 36 l" + Environment.NewLine +
"66 0 l" + Environment.NewLine +
"0 14 l" + Environment.NewLine +
"-66 0 l" + Environment.NewLine +
"0 41 l" + Environment.NewLine +
"73 0 l" + Environment.NewLine +
"0 14 l" + Environment.NewLine);
 #endregion

    #region H
 private readonly Letter _h = new Letter('H', 0, 0,
      "0 -119 l" + Environment.NewLine +
"16 0 l" + Environment.NewLine +
"0 49 l" + Environment.NewLine +
"62 0 l" + Environment.NewLine +
"0 -49 l" + Environment.NewLine +
"15 0 l" + Environment.NewLine +
"0 119 l" + Environment.NewLine +
"-15 0 l" + Environment.NewLine +
"0 -56 l" + Environment.NewLine +
"-62 0 l" + Environment.NewLine +
"0 56 l" + Environment.NewLine);
    #endregion

 private readonly Letter _k = new Letter('K',
      0, 0,
"0 -119 l" + Environment.NewLine +
"16 0 l" + Environment.NewLine +
"0 59 l" + Environment.NewLine +
"59 -59 l" + Environment.NewLine +
"21 0 l" + Environment.NewLine +
"-50 49 l" + Environment.NewLine +
"52 70 l" + Environment.NewLine +
"-20 0 l" + Environment.NewLine +
"-43 -60 l" + Environment.NewLine +
"-19 19 l" + Environment.NewLine +
"0 41 l" + Environment.NewLine);

    #endregion

    private abstract class To
    {
      public static To Create(string s)
      {
        if (s.EndsWith("l"))
        {
          string[] list = s.Split();
          return new LineTo(double.Parse(list[0]), double.Parse(list[1]));
        }
        else if (s.EndsWith("c"))
        {
          string[] list = s.Split();
          return new CurveTo(
            double.Parse(list[0]), double.Parse(list[1]),
            double.Parse(list[2]), double.Parse(list[3]),
            double.Parse(list[4]), double.Parse(list[5]));
        }
        else
        { throw new ArgumentException("Unhandled line " + s); }
      }
      public abstract bool IsNull(double resol);

      public static List<List<To>> CreateXPath(params string[] paths)
      {
        List<List<To>> list = new List<List<To>>();
        foreach (var path in paths)
        {
          list.Add(CreatePath(path));
        }
        return list;
      }

      public static List<To> CreatePath(string path)
      {
        List<To> list = new List<To>();
        int pos = path.IndexOf(Environment.NewLine);
        while (pos > 0)
        {
          string line = path.Substring(0, pos);
          To to = Create(line);

          list.Add(to);

          path = path.Substring(pos + Environment.NewLine.Length);
          pos = path.IndexOf(Environment.NewLine);
        }
        return list;
      }

      public static bool Equals(List<To> l0, List<To> l1, double resol)
      {
        int n0 = l0.Count;
        int n1 = l1.Count;

        int i0 = 0;
        int i1 = 0;

        while (i0 < n0 || i1 < n1)
        {
          if (i0 < n0 && i1 < n1 && l0[i0].Equals(l1[i1], resol))
          {
            i0++;
            i1++;
          }
          else if (i0 < n0 && l0[i0].IsNull(resol))
          {
            i0++;
          }
          else if (i1 < n1 && l1[i1].IsNull(resol))
          {
            i1++;
          }
          else
          {
            return false;
          }
        }
        return true;
      }
      public abstract bool Equals(To other, double resol);

      public abstract void AppendTo(StringBuilder all);

      public abstract double MinX();
      public abstract double MinY();
      public abstract double MaxX();
      public abstract double MaxY();

      public abstract double EndX();
      public abstract double EndY();

      internal static double MinX(List<To> path)
      {
        double minX = 0;

        double currentX = 0;
        foreach (var part in path)
        {
          double m = part.MinX();
          if (currentX + m < minX)
          { minX = currentX + m; }

          currentX += part.EndX();
        }

        return minX;
      }

      internal static double MaxX(List<To> path)
      {
        double maxX = 0;

        double currentX = 0;
        foreach (var part in path)
        {
          double m = part.MinX();
          if (currentX + m > maxX)
          { maxX = currentX + m; }

          currentX += part.EndX();
        }

        return maxX;
      }

      internal static double MinY(List<To> path)
      {
        double minY = 0;

        double currentY = 0;
        foreach (var part in path)
        {
          double m = part.MinY();
          if (currentY + m < minY)
          { minY = currentY + m; }

          currentY += part.EndY();
        }

        return minY;
      }

      internal static double MaxY(List<To> path)
      {
        double maxY = 0;

        double currentY = 0;
        foreach (var part in path)
        {
          double m = part.MaxY();
          if (currentY + m > maxY)
          { maxY = currentY + m; }

          currentY += part.EndY();
        }

        return maxY;
      }
    }

    private class LineTo : To
    {
      public double X;
      public double Y;

      public LineTo(double x, double y)
      {
        X = x;
        Y = y;
      }

      public override double MinX()
      {
        return Math.Min(0, X);
      }
      public override double MaxX()
      {
        return Math.Max(0, X);
      }
      public override double MinY()
      {
        return Math.Min(0, Y);
      }
      public override double MaxY()
      {
        return Math.Max(0, Y);
      }
      public override double EndX()
      {
        return X;
      }
      public override double EndY()
      {
        return Y;
      }
      public override bool IsNull(double resol)
      {
        return (Math.Abs(X) < resol && Math.Abs(Y) < resol);
      }
      public override bool Equals(To other, double resol)
      {
        if (other is LineTo == false)
        { return false; }

        LineTo o = (LineTo)other;
        if (Math.Abs(X - o.X) > resol)
        { return false; }
        if (Math.Abs(Y - o.Y) > resol)
        { return false; }

        return true;
      }
      public override void AppendTo(StringBuilder all)
      {
        all.Append(string.Format("{0} {1} l", (int)X, (int)Y));
      }

    }
    private class CurveTo : To
    {
      public double X0;
      public double Y0;
      public double X1;
      public double Y1;
      public double X2;
      public double Y2;

      public CurveTo(double x0, double y0, double x1, double y1, double x2, double y2)
      {
        X0 = x0;
        Y0 = y0;

        X1 = x1;
        Y1 = y1;

        X2 = x2;
        Y2 = y2;
      }

      public override double EndX()
      {
        return X2;
      }
      public override double EndY()
      {
        return Y2;
      }
      public override double MinX()
      {
        return Math.Min(0, Math.Min(X0, Math.Min(X1, X2)));
      }
      public override double MaxX()
      {
        return Math.Max(0, Math.Max(X0, Math.Max(X1, X2)));
      }
      public override double MinY()
      {
        return Math.Min(0, Math.Min(Y0, Math.Min(Y1, Y2)));
      }
      public override double MaxY()
      {
        return Math.Max(0, Math.Max(Y0, Math.Max(Y1, Y2)));
      }
      public override bool IsNull(double resol)
      {
        return (Math.Abs(X0) < resol && Math.Abs(Y0) < resol &&
          Math.Abs(X1) < resol && Math.Abs(Y1) < resol &&
          Math.Abs(X2) < resol && Math.Abs(Y2) < resol);
      }

      public override bool Equals(To other, double resol)
      {
        if (other is CurveTo == false)
        { return false; }

        CurveTo o = (CurveTo)other;
        if (Math.Abs(X0 - o.X0) > resol)
        { return false; }
        if (Math.Abs(Y0 - o.Y0) > resol)
        { return false; }

        if (Math.Abs(X1 - o.X1) > resol)
        { return false; }
        if (Math.Abs(Y1 - o.Y1) > resol)
        { return false; }

        if (Math.Abs(X2 - o.X2) > resol)
        { return false; }
        if (Math.Abs(Y2 - o.Y2) > resol)
        { return false; }

        return true;
      }

      public override void AppendTo(StringBuilder all)
      {
        all.Append(string.Format("{0} {1} {2} {3} {4} {5} c",
          (int)X0, (int)Y0, (int)X1, (int)Y1, (int)X2, (int)Y2));
      }

    }
    private List<Letter> _pattern;

    private void CreatePatternList()
    {
      if (_pattern != null)
      { return; }

      _pattern = new List<Letter>();

      _pattern.Add(_point);
      _pattern.Add(_0);
      _pattern.Add(_1);
      _pattern.Add(_2);
      _pattern.Add(_3);
      _pattern.Add(_4);
      _pattern.Add(_5);
      _pattern.Add(_6);
      _pattern.Add(_7);
      _pattern.Add(_8);
      _pattern.Add(_9);
      _pattern.Add(_a);
      _pattern.Add(_d);
      _pattern.Add(_e);
      _pattern.Add(_h);
      _pattern.Add(_k);
    }

    private string GetLetter(TextReader reader, StringBuilder store)
    {
      double resol = 2.1;
      CreatePatternList();
      string pattern = GetPattern(reader);

      string l = null;

      List<To> path = To.CreatePath(pattern);

      foreach (var letter in _pattern)
      {
        if (To.Equals(path, letter.PatternList[0].Path, resol))
        {
          int n = letter.PatternList.Count;
          pattern = pattern + "f" + Environment.NewLine;
          if (n > 1)
          {
            pattern += VerifyPattern(reader, letter.PatternList.GetRange(1, n - 1), resol);
          }

          l = letter.Char.ToString();
          break;
        }
      }

      if (l == null)
      {
        throw new ArgumentException("unhandled pattern " + Environment.NewLine + pattern);
      }

      if (store != null)
      { store.Append(pattern); }

      return l;
    }

    private StringBuilder
      VerifyPattern(TextReader reader, List<Pattern> patternList, double resol)
    {
      StringBuilder pattern = new StringBuilder();

      foreach (var path in patternList)
      {
        pattern.Append(reader.ReadLine() + Environment.NewLine);
        string inner = GetPattern(reader);
        if (To.Equals(To.CreatePath(inner), path.Path, resol) == false)
        { throw new ArgumentException(string.Format("Unhandled {0}", inner)); }
        pattern.Append(inner + "f" + Environment.NewLine);
      }

      return pattern;
    }

    private static string GetPattern(TextReader reader)
    {
      string pattern = "";
      string line;
      line = reader.ReadLine();
      while (line != "f")
      {
        Trace.Assert(line.StartsWith("n ") == false);
        pattern = pattern + line + Environment.NewLine;
        line = reader.ReadLine();
      }
      return pattern;
    }

    private delegate bool Check(string check);

    private bool NextPageEnd(string line)
    {
      return line.StartsWith("(%%[Page:");
    }

    private bool NextPage(string line)
    {
      return line.StartsWith("%%Page:") || line.StartsWith("%%Trailer");
    }

    private bool NextImageColor(string line)
    {
      return (line.Contains("0 122 122 0 cmyk") || line.Contains("122 122 0 0 cmyk")
        || line.Contains("122 0 122 0 cmyk"));
    }

    private bool NextInfoColor(string line)
    {
      return (line.Contains("0 26 0 0 cmyk") || line.Contains(_bahnColor)
        || line.Contains("26 0 0 0 cmyk"));
    }

    private bool NextImageOrInfoColor(string line)
    {
      return NextImageColor(line) || NextInfoColor(line);
    }

    private bool NextPixel(string line)
    {
      line = line.Trim();
      if (line.EndsWith(" r") == false)
      { return false; }

      string[] parts = line.Split();
      if (parts.Length != 8)
      { return false; }
      for (int i = 0; i < 7; i++)
      {
        if (int.TryParse(parts[i], out int j) == false)
        { return false; }
      }

      return true;
    }

    private bool NextColor(string line)
    {
      return line.Contains(" cmyk");
    }

    private string GetToNext(TextReader reader, Check checkFct,
      TextWriter writer, StringBuilder store)
    {
      string line;

      line = reader.ReadLine();
      while (line != null && checkFct(line) == false)
      {
        if (writer != null)
        { writer.WriteLine(line); }
        if (store != null)
        { store.Append(line + Environment.NewLine); }
        line = reader.ReadLine();
      }
      return line;
    }

    [TestMethod]
    public void IsIOFEqualOcad()
    {
      CourseXmlDocument doc = new CourseXmlDocument();
      doc.Load(@"C:\daten\felix\kapreolo\karten\stadlerberg\SOM07_v4.1.1.Courses.xml");

      OcadReader reader = OcadReader.Open(@"C:\daten\felix\kapreolo\karten\stadlerberg\SOM07_v4.1.1.ocd");
      List<Course> courseList = new List<Course>();
      foreach (var course in reader.ReadCourses())
      {
        courseList.Add(course);
      }
      reader.Close();

      foreach (var course in courseList)
      {
        XmlNode nodeCourse = doc.CourseNode(course.Name);

        VerifyCombinations(doc, nodeCourse, course);
        string courseId = doc.CourseId(nodeCourse);

        doc.GetValidCombinations(courseId, course);
      }
    }

    private List<string> GetValidStartNumbers(string ocadName, string combinationName,
      string courseName,out int minStartNr)
    {
      OcadReader reader = OcadReader.Open(ocadName);
      Course course = reader.ReadCourse(courseName);
      reader.Close();

      if (course == null)
      {
        minStartNr = -1;
        return null;
      }

      CourseXmlDocument combinationDoc = new CourseXmlDocument();
      combinationDoc.Load(combinationName);

      minStartNr = combinationDoc.GetMinimumStartNr(course);
      return combinationDoc.GetValidStartNumbers(course);
    }

    private static void VerifyCombinations(CourseXmlDocument doc, XmlNode nodeCourse, Course course)
    {
      XmlNodeList listVari = doc.CourseVariationList(nodeCourse);
      foreach (var nodeVari in listVari.Enum())
      {
        string varName = nodeVari.SelectSingleNode("Name").InnerText.Trim();
        List<string> sCourse = doc.CourseVariation(nodeVari);

        string t = varName;
        List<Control> oCourse = course.GetCombination(t);

        Assert.AreEqual("", t);
        Assert.AreEqual(oCourse.Count, sCourse.Count);
        int n = oCourse.Count;
        for (int i = 0; i < n; i++)
        {
          Assert.AreEqual(oCourse[i].Name, sCourse[i]);
        }
      }
    }

    [TestMethod]
    public void CheckRun()
    {
      Fork fork;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();

      fork.Branches.Add(new Fork.Branch(new Control[] { new Control("31") }));
      fork.Branches.Add(new Fork.Branch(new Control[] { new Control("31") }));
      fork.Branches.Add(new Fork.Branch(new Control[] { new Control("32") }));

      course.AddLast(fork);

      course.AddLast(new Control("finish"));

      List<List<Control>> combinations = course.GetExplicitCombinations();

      Assert.AreEqual(3, combinations.Count);

      Assert.AreEqual(3, combinations[0].Count);
      Assert.AreEqual("start", combinations[0][0].Name);
      Assert.AreEqual("31", combinations[0][1].Name);
      Assert.AreEqual("finish", combinations[0][2].Name);

      Assert.AreEqual(3, combinations[1].Count);
      Assert.AreEqual("start", combinations[1][0].Name);
      Assert.AreEqual("31", combinations[1][1].Name);
      Assert.AreEqual("finish", combinations[1][2].Name);

      Assert.AreEqual(3, combinations[2].Count);
      Assert.AreEqual("start", combinations[2][0].Name);
      Assert.AreEqual("32", combinations[2][1].Name);
      Assert.AreEqual("finish", combinations[2][2].Name);
    }

    [TestMethod]
    public void CheckRun1()
    {
      SectionCollection course = CreateSections(
        "start," +
        "{{31},{31},{32}}," +
        "ziel");
      List<string> combs = course.GetValidCombinationStrings();
      Assert.AreEqual(3, combs.Count);
      Assert.AreEqual("A", combs[0]);
      Assert.AreEqual("B", combs[1]);
      Assert.AreEqual("C", combs[2]);
    }
    [TestMethod]
    public void CheckRun2()
    {
      SectionCollection course = CreateSections(
        "start," +
        "((1,3){31,((1){40,41,42},(3){43,44,45}),{{33,34},{35,36}}},(2){32})," +
        "ziel");
      Assert.IsNotNull(course);
    }

    [TestMethod]
    public void CheckRun3()
    {
      Fork fork;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,33")));
      course.AddLast(fork);

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("41,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      course.AddLast(fork);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("AA") >= 0);
      Assert.IsTrue(combs.IndexOf("BB") >= 0);
      Assert.IsTrue(combs.IndexOf("BC") >= 0);
      Assert.IsTrue(combs.IndexOf("CC") >= 0);
      Assert.IsTrue(combs.IndexOf("CB") >= 0);
      Assert.AreEqual(5, combs.Count);
    }


    [TestMethod]
    public void CheckRun4()
    {
      Fork fork;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      course.AddLast(fork);

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("41,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("43,45")));
      course.AddLast(fork);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("AA") >= 0);
      Assert.IsTrue(combs.IndexOf("AC") >= 0);
      Assert.IsTrue(combs.IndexOf("BB") >= 0);
      Assert.IsTrue(combs.IndexOf("CA") >= 0);
      Assert.IsTrue(combs.IndexOf("CC") >= 0);
      Assert.AreEqual(5, combs.Count);
    }

    [TestMethod]
    public void CheckRun5()
    {
      Fork fork;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      course.AddLast(fork);

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("41,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      course.AddLast(fork);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("AA") >= 0);
      Assert.IsTrue(combs.IndexOf("AB") >= 0);
      Assert.IsTrue(combs.IndexOf("AC") >= 0);
      Assert.IsTrue(combs.IndexOf("BB") >= 0);
      Assert.IsTrue(combs.IndexOf("BC") >= 0);
      Assert.IsTrue(combs.IndexOf("CA") >= 0);
      Assert.IsTrue(combs.IndexOf("CB") >= 0);
      Assert.IsTrue(combs.IndexOf("CC") >= 0);
      Assert.AreEqual(8, combs.Count);
    }

    [TestMethod]
    public void CheckRun6()
    {
      Fork fork;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("")));
      course.AddLast(fork);

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("41,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      course.AddLast(fork);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("AA") >= 0);
      Assert.IsTrue(combs.IndexOf("BB") >= 0);
      Assert.IsTrue(combs.IndexOf("BC") >= 0);
      Assert.IsTrue(combs.IndexOf("CC") >= 0);
      Assert.AreEqual(4, combs.Count);
    }

    [TestMethod]
    public void CheckRun7()
    {
      Fork fork;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("30,31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("30,32")));
      course.AddLast(fork);

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("41,45")));
      fork.Branches.Add(new Fork.Branch(CreateSections("")));
      fork.Branches.Add(new Fork.Branch(CreateSections("42,45")));
      course.AddLast(fork);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("AA") >= 0);
      Assert.IsTrue(combs.IndexOf("BB") >= 0);
      Assert.IsTrue(combs.IndexOf("BC") >= 0);
      Assert.IsTrue(combs.IndexOf("CB") >= 0);
      Assert.IsTrue(combs.IndexOf("CC") >= 0);
      Assert.AreEqual(5, combs.Count);
    }

    [TestMethod]
    public void CheckRun8()
    {
      Variation var;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      var = new Variation();
      var.Branches.Add(new Variation.Branch(new int[] { 1 }, CreateSections("30,31")));
      var.Branches.Add(new Variation.Branch(new int[] { 2 }, CreateSections("30,32")));
      var.Branches.Add(new Variation.Branch(new int[] { 3 }, CreateSections("30,32")));
      course.AddLast(var);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("1") >= 0);
      Assert.IsTrue(combs.IndexOf("2") >= 0);
      Assert.IsTrue(combs.IndexOf("3") >= 0);
      Assert.AreEqual(3, combs.Count);
    }

    [TestMethod]
    public void CheckRun9()
    {
      Variation var;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      var = new Variation();
      var.Branches.Add(new Variation.Branch(new int[] { 1, 3 }, CreateSections("{{30},{31}}")));
      var.Branches.Add(new Variation.Branch(new int[] { 2 }, CreateSections("32")));
      course.AddLast(var);

      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("1A") >= 0);
      Assert.IsTrue(combs.IndexOf("1B") >= 0);
      Assert.IsTrue(combs.IndexOf("2") >= 0);
      Assert.IsTrue(combs.IndexOf("3A") >= 0);
      Assert.IsTrue(combs.IndexOf("3B") >= 0);
      Assert.AreEqual(5, combs.Count);
    }

    [TestMethod]
    public void CheckRun10()
    {
      Fork fork;
      Variation var;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      course.AddLast(fork);

      var = new Variation();
      var.Branches.Add(new Variation.Branch(new int[] { 3 }, CreateSections("")));
      var.Branches.Add(new Variation.Branch(new int[] { 1, 2 }, CreateSections("40")));
      course.AddLast(var);

      course.AddLast(new Control("50"));
      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("1A") >= 0);
      Assert.IsTrue(combs.IndexOf("1B") >= 0);
      Assert.IsTrue(combs.IndexOf("1C") >= 0);
      Assert.IsTrue(combs.IndexOf("2A") >= 0);
      Assert.IsTrue(combs.IndexOf("2B") >= 0);
      Assert.IsTrue(combs.IndexOf("2C") >= 0);
      Assert.IsTrue(combs.IndexOf("3A") >= 0);
      Assert.IsTrue(combs.IndexOf("3C") >= 0);
      Assert.AreEqual(8, combs.Count);
    }

    [TestMethod]
    public void CheckRun11()
    {
      Fork fork;
      Variation var;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      course.AddLast(fork);

      var = new Variation();
      var.Branches.Add(new Variation.Branch(new int[] { 3 }, CreateSections("")));
      var.Branches.Add(new Variation.Branch(new int[] { 1, 2 }, CreateSections("40,{{41},{42}}")));
      course.AddLast(var);

      course.AddLast(new Control("50"));
      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("1AA") >= 0);
      Assert.IsTrue(combs.IndexOf("1AB") >= 0);
      Assert.IsTrue(combs.IndexOf("1BA") >= 0);
      Assert.IsTrue(combs.IndexOf("1BB") >= 0);
      Assert.IsTrue(combs.IndexOf("1CA") >= 0);
      Assert.IsTrue(combs.IndexOf("1CB") >= 0);
      Assert.IsTrue(combs.IndexOf("2AA") >= 0);
      Assert.IsTrue(combs.IndexOf("2AB") >= 0);
      Assert.IsTrue(combs.IndexOf("2BA") >= 0);
      Assert.IsTrue(combs.IndexOf("2BB") >= 0);
      Assert.IsTrue(combs.IndexOf("2CA") >= 0);
      Assert.IsTrue(combs.IndexOf("2CB") >= 0);
      Assert.IsTrue(combs.IndexOf("3A") >= 0);
      Assert.IsTrue(combs.IndexOf("3C") >= 0);
      Assert.AreEqual(14, combs.Count);
    }

    [TestMethod]
    public void CheckRun12()
    {
      Fork fork;
      Variation var;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      course.AddLast(fork);

      var = new Variation();
      var.Branches.Add(new Variation.Branch(new int[] { 3 }, CreateSections("")));
      var.Branches.Add(new Variation.Branch(new int[] { 1, 2 }, CreateSections("{{41},{42}}")));
      course.AddLast(var);

      course.AddLast(new Control("50"));
      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("1AA") >= 0);
      Assert.IsTrue(combs.IndexOf("1BB") >= 0);
      Assert.IsTrue(combs.IndexOf("1CB") >= 0);
      Assert.IsTrue(combs.IndexOf("2AA") >= 0);
      Assert.IsTrue(combs.IndexOf("2BB") >= 0);
      Assert.IsTrue(combs.IndexOf("2CB") >= 0);
      Assert.IsTrue(combs.IndexOf("3A") >= 0);
      Assert.IsTrue(combs.IndexOf("3C") >= 0);
      Assert.AreEqual(8, combs.Count);
    }

    [TestMethod]
    public void CheckRun13()
    {
      Fork fork;
      Variation var;

      Course course = new Course("Test");
      course.AddFirst(new Control("start"));

      var = new Variation();
      var.Branches.Add(new Variation.Branch(new int[] { 3 }, CreateSections("")));
      var.Branches.Add(new Variation.Branch(new int[] { 1, 2 }, CreateSections("{{41},{42}}")));
      course.AddLast(var);

      fork = new Fork();
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      fork.Branches.Add(new Fork.Branch(CreateSections("31")));
      fork.Branches.Add(new Fork.Branch(CreateSections("32")));
      course.AddLast(fork);

      course.AddLast(new Control("50"));
      course.AddLast(new Control("finish"));

      List<string> combs = course.GetValidCombinationStrings();
      Assert.IsTrue(combs.IndexOf("1AB") >= 0);
      Assert.IsTrue(combs.IndexOf("1BA") >= 0);
      Assert.IsTrue(combs.IndexOf("1BC") >= 0);
      Assert.IsTrue(combs.IndexOf("2AB") >= 0);
      Assert.IsTrue(combs.IndexOf("2BA") >= 0);
      Assert.IsTrue(combs.IndexOf("2BC") >= 0);
      Assert.IsTrue(combs.IndexOf("3A") >= 0);
      Assert.IsTrue(combs.IndexOf("3C") >= 0);
      Assert.AreEqual(8, combs.Count);
    }

    private SectionCollection CreateSections(string list)
    {
      SectionCollection sections = new SectionCollection();

      while (list.IndexOf(',') >= 0)
      {
        sections.AddLast(CreateSection(list, out list));
      }
      if (list != "")
      { sections.AddLast(CreateSection(list, out list)); }
      return sections;
    }
    private ISection CreateSection(string list, out string rest)
    {
      ISection ret;
      list = list.TrimStart();
      if (list[0] == '{')
      {
        Fork f = new Fork();
        list = list.Substring(1).TrimStart();
        while (list[0] == '{')
        {
          f.Branches.Add(CreateForkBranch(list, out list));
          list = list.TrimStart();
          if (list[0] == ',')
          { list = list.Substring(1).TrimStart(); }
        }
        Debug.Assert(list[0] == '}');
        rest = list.Substring(1).TrimStart();
        ret = f;
      }
      else if (list[0] == '(')
      {
        Variation v = new Variation();
        list = list.Substring(1).TrimStart();
        while (list[0] == '(')
        {
          v.Branches.Add(CreateVariationBranch(list, out list));
          list = list.TrimStart();
          if (list[0] == ',')
          { list = list.Substring(1).TrimStart(); }
        }
        Debug.Assert(list[0] == ')');
        rest = list.Substring(1).TrimStart();
        ret = v;
      }
      else
      {
        int i = list.IndexOf(',');
        string name;
        if (i > 0)
        {
          name = list.Substring(0, i);
          rest = list.Substring(i + 1).TrimStart();
        }
        else
        {
          name = list;
          rest = "";
        }
        ret = new Control(name);
      }
      if (rest.Length > 0 && rest[0] == ',')
      {
        rest = rest.Substring(1).TrimStart();
      }
      return ret;
    }
    private Fork.Branch CreateForkBranch(string list, out string rest)
    {
      int i = list.IndexOf('}');
      string part = list.Substring(1, i - 1);
      rest = list.Substring(i + 1);
      return new Fork.Branch(CreateSections(part));
    }

    private Variation.Branch CreateVariationBranch(string list, out string rest)
    {
      int i = list.IndexOf(')');
      string[] legs = list.Substring(1, i - 1).Split(',');

      list = list.Substring(i + 1).Trim();
      int nOpenBrackets = 0;
      i = 0;
      do
      {
        if (list[i] == '{')
        { nOpenBrackets++; }
        if (list[i] == '}')
        { nOpenBrackets--; }
        i++;
      } while (nOpenBrackets > 0);

      List<int> legList = new List<int>();
      foreach (var leg in legs)
      {
        legList.Add(Convert.ToInt32(leg));
      }
      Variation.Branch b = new Variation.Branch(legList, CreateSections(list.Substring(1, i - 2)));
      rest = list.Substring(i);
      return b;
    }

  }
}
