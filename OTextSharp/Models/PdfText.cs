using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace OTextSharp.Models
{
  public class PdfText
  {
    private class FilePdfReader { public string File; public PdfReader Reader; }

    public static void OverprintFrontBack(string exportFile, IList<string> fileNames,
      string frontBack, string rueckBack)
    {
      OverprintFrontBack(exportFile, fileNames, (f) => frontBack, rueckBack);
    }

    public static void OverprintFrontBack(string exportFile, IList<string> fileNames,
      Func<string, string> getFrontBack, string rueckBack)
    {
      List<FilePdfReader> readers = new List<FilePdfReader>();
      PdfReader template = null;

      foreach (var fileName in fileNames)
      {
        PdfReader reader = new PdfReader(fileName);
        readers.Add(new FilePdfReader { File = fileName, Reader = reader });

        template = template ?? reader;
      }
      Dictionary<string, PdfReader> frontDict = new Dictionary<string, PdfReader>();

      PdfReader rueck = null;
      if (!string.IsNullOrEmpty(rueckBack))
      { rueck = new PdfReader(rueckBack); }

      if (template == null)
      { return; }
      {
        GetDocument(exportFile, template.GetPageSize(1), out Document document, out PdfWriter writer);

        // step 4: we add content
        PdfContentByte cb = writer.DirectContent;

        foreach (var fileReader in readers)
        {
          document.NewPage();

          PdfReader reader = fileReader.Reader;
          PdfReader front = GetBackReader(getFrontBack, fileReader.File, frontDict);
          if (fileReader.File.Contains("Front") && front != null)
          {
            PdfImportedPage p = writer.GetImportedPage(front, 1);
            cb.AddTemplate(p, 1, 0, 0, 1, 0, 0);
          }
          if (fileReader.File.Contains("Rueck") && rueck != null)
          {
            PdfImportedPage p = writer.GetImportedPage(rueck, 1);
            cb.AddTemplate(p, 1, 0, 0, 1, 0, 0);
          }

          PdfImportedPage page1 = writer.GetImportedPage(reader, 1);
          cb.AddTemplate(page1, 1, 0, 0, 1, 0, 0);
        }
        document.Close();
      }
    }

    private static PdfReader GetBackReader(Func<string, string> getBackName, string file, Dictionary<string, PdfReader> nameReaderDict)
    {
      string key = getBackName(file);
      if (key == null)
      { return null; }

      if (!nameReaderDict.TryGetValue(key, out PdfReader reader))
      {
        reader = null;
        if (File.Exists(key))
        { reader = new PdfReader(key); }
        nameReaderDict.Add(key, reader);
      }

      return reader;
    }

    public static void Overprint(string templateName, IList<string> fileNames, string exportFile)
    {
      IList<PdfReader> readers = new List<PdfReader>();
      PdfReader template = null;
      foreach (var fileName in fileNames)
      {
        PdfReader reader = new PdfReader(fileName);
        if (fileName == templateName)
        {
          template = reader;
        }
        readers.Add(reader);
      }

      if (template == null)
      {
        if (string.IsNullOrEmpty(templateName) == false)
        {
          template = new PdfReader(templateName);
        }
        else
        {
          template = readers[0];
        }
      }
      {
        GetDocument(exportFile, template.GetPageSize(1), out Document document, out PdfWriter writer);

        // step 4: we add content
        PdfContentByte cb = writer.DirectContent;

        document.NewPage();

        foreach (var reader in readers)
        {
          PdfImportedPage page1 = writer.GetImportedPage(reader, 1);
          cb.AddTemplate(page1, 1, 0, 0, 1, 0, 0);
        }
        document.Close();
      }
    }

    private static void GetDocument(string exportFile, Rectangle size, out Document document, out PdfWriter writer)
    {
      // we retrieve the size of the first page
      // Rectangle psize = partReader.GetPageSize(1);

      // step 1: creation of a document-object
      Document.Compress = true;
      document = new Document(size);

      Stream stream = new FileStream(exportFile, FileMode.Create);
      writer = PdfWriter.GetInstance(document, stream);
      document.Open();
    }
  }
}