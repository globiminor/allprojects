
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Basics.Geom;

namespace Ocad.Data
{
  public class OcadLayouter
  {
    private readonly string _outFile;
    public const string NewConstr = "_c.ocd";
    public const string PreConstr = "_p.ocd";
    public const string Layout = "_l.ocd";

    public OcadLayouter(string outFile)
    {
      _outFile = outFile;
    }

    private class Match
    {
      public Element Element { get; set; }
      public ElementIndex Index { get; set; }

      public List<Offset> Targets { get; } = new List<Offset>();
    }

    private class MatchBoxTree : BoxTree<Match>
    {
      public MatchBoxTree()
        : base(2)
      { }

      public IEnumerable<Match> GetAllElements()
      {
        foreach (TileEntry tileEntry in Search(null))
        {
          yield return tileEntry.Value;
        }
      }
    }

    public void UpdateUserValues()
    {
      string outFile = _outFile;
      string dir = Path.GetDirectoryName(outFile);
      string name = Path.GetFileNameWithoutExtension(outFile);

      MatchBoxTree newTree = CreateBoxTree(Path.Combine(dir, name + NewConstr));
      MatchBoxTree preTree = CreateBoxTree(Path.Combine(dir, name + PreConstr));
      MatchBoxTree manTree = CreateBoxTree(Path.Combine(dir, name + Layout));

      BuildMatches(newTree, preTree.GetAllElements(), 0);
      BuildMatches(preTree, GetElements(Path.Combine(dir, name + Layout)), 200);

      foreach (Match match in newTree.GetAllElements())
      {
        if (match.Targets.Count != 1)
        { continue; }

        Offset off = match.Targets[0];
        if (off.Dist2 != 0)
        { continue; }

        if (off.Element.Targets == null)
        { continue; }

        //  Match zMatch = new Match();
        //  zMatch.Targets[0].Delete();
        //  foreach (Offset offset in layouts)
        //  {
        //    zw.Append(offset.Element);
        //  }
      }
    }

    private IEnumerable<Match> GetElements(string fileName)
    {
      using (OcadReader reader = OcadReader.Open(fileName))
      {
        IList<ElementIndex> idxs = reader.GetIndices();
        foreach (ElementIndex idx in idxs)
        {
          if (idx.Status == ElementIndex.StatusDeleted)
          { continue; }
          Element elem = reader.ReadElement(idx);

          if (elem == null)
          { continue; }

          Match m = new Match { Element = elem, Index = idx };
          yield return m;
        }
      }
    }

    private class Offset
    {
      public void Delete() { }
      public Match Element { get; set; }
      public double Dist2 { get; set; }
    }

    private void BuildMatches(BoxTree<Match> tree, IEnumerable<Match> neighbors, double maxDist)
    {
      Point2D off = new Point2D(maxDist, maxDist);
      double maxD2 = maxDist * maxDist;

      Dictionary<Match, List<Offset>> gagas = new Dictionary<Match, List<Offset>>();
      Dictionary<Match, List<Offset>> revers = new Dictionary<Match, List<Offset>>();
      foreach (Match nb in neighbors)
      {
        IPoint nbGeom = nb.Element.Geometry as IPoint;
        if (nbGeom == null)
        {
          if (nb.Element.Geometry is PointCollection txt && txt.Count > 0)
          { nbGeom = txt[0]; }
        }

        if (nbGeom == null)
        { throw new NotImplementedException(); }
        Point2D nbP = new Point2D(nbGeom);

        IBox extent = nbGeom.Extent;
        Point min = extent.Min + off;
        Point max = extent.Max - off;
        Box search = new Box(min, max);
        foreach (BoxTree<Match>.TileEntry tileEntry in tree.Search(search))
        {
          Element elem = tileEntry.Value.Element;
          if (elem.Symbol != nb.Element.Symbol)
          { continue; }

          IPoint p = elem.Geometry as IPoint;
          if (p == null)
          {
            if (elem.Geometry is PointCollection txt && txt.Count > 0)
            { p = txt[0]; }
          }

          if (p == null)
          { throw new NotImplementedException(); }

          double d2 = nbP.Dist2(p);
          if (d2 > maxD2)
          { continue; }

          if (elem.Text != nb.Element.Text)
          { continue; }

          if (!gagas.TryGetValue(nb, out List<Offset> elems))
          {
            elems = new List<Offset>();
            gagas.Add(nb, elems);
          }
          elems.Add(new Offset { Element = tileEntry.Value, Dist2 = d2 });
        }
      }
      while (gagas.Count > 0)
      {
        List<Match> unhandleds = new List<Match>(gagas.Keys);
        foreach (var pair in gagas)
        { pair.Value.Sort((x, y) => x.Dist2.CompareTo(y.Dist2)); }

        unhandleds.Sort((x, y) => gagas[x][0].Dist2.CompareTo(gagas[y][0].Dist2));

        Match handled = unhandleds[0];
        List<Offset> offs = gagas[handled];
        offs[0].Element.Targets.Add(new Offset { Element = handled, Dist2 = offs[0].Dist2 });
        gagas.Remove(handled);
      }
    }

    private MatchBoxTree CreateBoxTree(string fileName)
    {
      List<Match> matches = new List<Match>();
      IBox fullBox = null;
      foreach (Match match in GetElements(fileName))
      {
        matches.Add(match);
        IBox box = match.Element.Geometry.Extent;
        if (fullBox == null)
        { fullBox = box.Clone(); }
        else
        { fullBox.Include(box); }
      }

      MatchBoxTree tree = new MatchBoxTree();
      if (fullBox == null)
      { return tree; }
      tree.Init(new Box(fullBox.Min, fullBox.Max), 16);
      foreach (Match match in matches)
      { tree.Add(match.Element.Geometry.Extent, match); }
      return tree;
    }
  }
}
