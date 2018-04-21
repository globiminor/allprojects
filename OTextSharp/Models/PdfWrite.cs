
using Basics.Geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace OTextSharp.Models
{
  public class PdfWrite : IDisposable
  {
    private Document _doc;
    private Stream _stream;
    private PdfWriter _writer;
    private PdfContentByte _pcb;
    private BaseFont _font;

    public const float DefaultWidth = 595;
    public const float DefaultHeight = 842;

    public PdfWrite(string fileName)
    {
      Document.Compress = true;
      _doc = new Document(new Rectangle(DefaultWidth, DefaultHeight));

      _stream = new FileStream(fileName, FileMode.Create);
      _writer = PdfWriter.GetInstance(_doc, _stream);
      _doc.Open();

      _pcb = _writer.DirectContent;
    }

    public BaseFont Font
    {
      get { return _font ?? (_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false)); }
      set { _font = value; }
    }

    public void SetFontSize(float size)
    {
      _pcb.SetFontAndSize(Font, size);
    }

    protected PdfContentByte Pcb { get { return _pcb; } }

    public void NewPage()
    {
      _doc.NewPage();
    }

    public void ShowText(int align, string text, float x, float y, float rot)
    {
      _pcb.BeginText();
      _pcb.ShowTextAligned(align, text, x, y, rot);
      _pcb.EndText();
    }

    public void DrawCurve(Polyline curve)
    {
      bool first = true;
      foreach (Curve seg in curve.Segments)
      {
        if (first)
        {
          Pcb.MoveTo((float)seg.Start.X, (float)seg.Start.Y);
          first = false;
        }
        if (seg is Line l)
        {
          Pcb.LineTo((float)l.End.X, (float)l.End.Y);
        }
        else if (seg is Bezier b)
        {
          Pcb.CurveTo((float)b.P1.X, (float)b.P1.Y, (float)b.P2.X, (float)b.P2.Y, (float)b.End.X, (float)b.End.Y);
        }
        else if (seg is Arc arc)
        {
          Pcb.Circle((float)arc.Center.X, (float)arc.Center.Y, (float)arc.Radius);
        }
        else
        {
          throw new NotImplementedException();
        }
      }
    }
    public void Dispose()
    {
      if (_writer != null)
      {
        _writer.Flush();
        _writer = null;
      }

      if (_doc != null)
      {
        _doc.Close();
        _doc = null;
      }

      if (_stream != null)
      {
        _stream.Close();
        _stream = null;
      }
    }
  }
}