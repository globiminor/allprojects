
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Basics
{
  public static class Serializer
  {
    public static void Serialize<T>(T obj, TextWriter writer,
      bool? indent = null, bool? omitXmlDeclaration = null)
    {
      XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      ns.Add(string.Empty, string.Empty);
      XmlSerializer ser = new XmlSerializer(typeof(T));

      XmlWriterSettings settings = new XmlWriterSettings();
      if (indent.HasValue)
      { settings.Indent = indent.Value; }
      if (omitXmlDeclaration.HasValue)
      { settings.OmitXmlDeclaration = omitXmlDeclaration.Value; }

      using (XmlWriter xw = XmlWriter.Create(writer, settings))
      {
        ser.Serialize(xw, obj, ns);
      }
    }

    public static bool TryDeserialize<T>(out T obj, TextReader reader)
    {
      try
      {
        Deserialize(out obj, reader);
        return true;
      }
      catch (System.Exception e)
      {
        obj = default(T);
        return false;
      }
    }
    public static void Deserialize<T>(out T obj, TextReader reader)
    {
      XmlSerializer ser = new XmlSerializer(typeof(T));

      object o = ser.Deserialize(reader);
      obj = (T)o;
    }
  }
}
