
using System;
using System.Xml;
namespace Ocad.Scripting
{
  public class Script : IDisposable
  {
    private readonly XmlDocument _doc;
    private readonly XmlElement _scriptNode;
    private readonly string _path;
    public Script(string path)
    {
      _path = path;
      _doc = new XmlDocument();

      _scriptNode = _doc.CreateElement("OcadScript");
      _doc.AppendChild(_scriptNode);
    }
    public XmlElement ScriptNode => _scriptNode;
    public Node FileOpen()
    {
      XmlElement childElem = _doc.CreateElement("File.Open");
      _scriptNode.AppendChild(childElem);

      return new Node(childElem);
    }

    public Node FileExport()
    {
      XmlElement childElem = _doc.CreateElement("File.Export");
      _scriptNode.AppendChild(childElem);

      return new Node(childElem);
    }
    public Node FileClose()
    {
      XmlElement childElem = _doc.CreateElement("File.Close");
      _scriptNode.AppendChild(childElem);

      return new Node(childElem);
    }
    public Node FileSave()
    {
      XmlElement childElem = _doc.CreateElement("File.Save");
      _scriptNode.AppendChild(childElem);

      return new Node(childElem);
    }
    public Node FileExit()
    {
      XmlElement childElem = _doc.CreateElement("File.Exit");
      _scriptNode.AppendChild(childElem);

      return new Node(childElem);
    }
    public Node MapOptimize()
    {
      XmlElement childElem = _doc.CreateElement("Map.OptimizeRepair");
      _scriptNode.AppendChild(childElem);

      return new Node(childElem);
    }
    public Node BackgroundMapRemove(string filename = "all")
    {
      XmlElement childElem = _doc.CreateElement("BackgroundMap.Remove");
      childElem.InnerText = filename;
      _scriptNode.AppendChild(childElem);

      Node node = new Node(childElem);
      return node;
    }

    public void Dispose()
    {
      _doc.Save(_path);
    }
  }
}
