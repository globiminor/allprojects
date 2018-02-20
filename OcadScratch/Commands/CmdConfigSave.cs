
using Basics;
using OcadScratch.ViewModels;
using OMapScratch;
using System;
using System.Collections.Generic;
using System.IO;

namespace OcadScratch.Commands
{
  public class CmdConfigSave : IDisposable
  {
    private readonly ConfigVm _config;
    private readonly string _path;

    public CmdConfigSave(ConfigVm config, string path)
    {
      _config = config;
      _path = path;
    }

    public bool Execute()
    {
      if (_config.BaseConfig == null || string.IsNullOrEmpty(_path))
      { return false; }

      string dir = Path.GetDirectoryName(_path);
      string scratchFile = Path.Combine(dir, Path.GetFileName(_config.Scratch));
      if (!File.Exists(scratchFile))
      {
        XmlElems elems = new XmlElems();
        elems.Elems = new List<XmlElem>();
        using (TextWriter w = new StreamWriter(scratchFile))
        {
          Serializer.Serialize(elems, w);
        }
      }
      string symbolsFile = Path.Combine(dir, Path.GetFileName(_config.SymbolPath));
      if (!File.Exists(symbolsFile))
      {
        XmlSymbols symbols = new XmlSymbols();
        symbols.Colors = new List<XmlColor>();
        foreach (ColorRef color in _config.Colors)
        { symbols.Colors.Add(XmlColor.Create(color)); }

        symbols.Symbols = new List<XmlSymbol>();
        foreach (Symbol sym in _config.Symbols)
        { symbols.Symbols.Add(XmlSymbol.Create(sym)); }

        using (TextWriter w = new StreamWriter(symbolsFile))
        {
          Serializer.Serialize(symbols, w);
        }
      }

      foreach (ImageVm img in _config.Images)
      {
        if (string.IsNullOrEmpty(img.CopyFromPath))
        { continue; }

        string target = Path.Combine(dir, img.Path);
        if (!img.CopyFromPath.Equals(target, StringComparison.InvariantCultureIgnoreCase))
        {
          File.Copy(img.CopyFromPath, target);
          File.Copy(Path.ChangeExtension(img.CopyFromPath, ".jgw"),
            Path.ChangeExtension(Path.Combine(dir, img.Path), ".jgw"));
        }
        img.CopyFromPath = null;
      }

      using (TextWriter w = new StreamWriter(_path))
      {
        Serializer.Serialize(_config.BaseConfig, w);
      }
      return true;
    }


    public void Dispose()
    { }
  }
}
