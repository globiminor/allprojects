using System;
using System.Xml;

namespace Ocad.Scripting
{
  public class Node : IDisposable
  {
    private readonly XmlElement _node;
    internal Node(XmlElement node)
    {
      _node = node;
    }
    public void Dispose()
    {
    }
    public Node File(string fileName)
    {
      XmlElement childElem = _node.OwnerDocument.CreateElement("File");
      _node.AppendChild(childElem);

      childElem.InnerText = fileName;

      return new Node(childElem);
    }

    public Node Enabled(bool enabled)
    {
      XmlElement childElem = _node.OwnerDocument.CreateElement("Enabled");
      _node.AppendChild(childElem);

      childElem.InnerText = enabled.ToString().ToLower();

      return new Node(childElem);
    }

    public Node Format(Format format)
    {
      XmlElement childNode = _node.OwnerDocument.CreateElement("Format");
      _node.AppendChild(childNode);

      string val;
      if (format == Scripting.Format.Pdf)
      { val = "PDF"; }
      else
      { throw new NotImplementedException("Unhandled Format " + format); }
      childNode.InnerText = val;

      return new Node(childNode);
    }
  }
}
