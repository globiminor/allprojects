using Basics.Geom;
using Ocad;
using OCourse.Ext;
using OCourse.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OCourse.Commands
{
  public class CmdExportCourseV8
  {
    private readonly Course _course;
    private readonly TextWriter _writer;
    private readonly IReadOnlyList<PermutationVm> _permutations;

    public CmdExportCourseV8(Course course, IReadOnlyList<PermutationVm> permutations, TextWriter writer)
    {
      _course = course;
      _permutations = permutations;
      _writer = writer;
    }

    public void Execute()
    {
      foreach (var permutation in _permutations)
      {
        List<List<SectionList>> legs = GetLegs(permutation.Parts);

        foreach (var leg in legs)
        {
          string controls = GetControls(leg, out double full);

          string startNr = legs.Count > 1
            ? $"{permutation.StartNr}.{legs.IndexOf(leg) + 1}"
            : $"{permutation.StartNr}";

          string line = $";{_course.Name};{startNr};{Math.Round(full / 1000.0, 1):N3};{controls}";
          _writer.WriteLine(line);
        }
      }
    }

    private List<List<SectionList>> GetLegs(IReadOnlyList<SectionList> parts)
    {
      List<List<SectionList>> legs = new List<List<SectionList>>();
      List<SectionList> leg = null;

      foreach (var part in parts)
      {
        if (leg == null)
        {
          leg = new List<SectionList>();
          legs.Add(leg);
        }

        leg.Add(part);

        if (part.NextControls.Last().Control.Code == ControlCode.Finish)
          leg = null;
      }

      return legs;
    }

    private string GetControls(IList<SectionList> leg, out double fullDistance)
    {
      fullDistance = 0;
      Control pre = null;
      double part = 0;

      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      foreach (var section in leg)
      {
        foreach (var c in section.Controls)
        {
          IPoint p0 = null;
          if (pre != null)
          {
            p0 = Utils.GetBorderPoint(pre.Element.Geometry, atEnd:true);
          }
          if (c.Element != null)
          {
            GeoElement.Geom geom = c.Element.Geometry;
            IPoint p1 = Utils.GetBorderPoint(c.Element.Geometry, atEnd: false);
            {
              double d = (geom as GeoElement.Line)?.BaseGeometry.Length() ?? 0;
              fullDistance += d;
              part += d;
            }
            if (p1 == null)
            { }

            if (p0 != null)
            {
              double d = Math.Sqrt(PointOp.Dist2(p0, p1));
              fullDistance += d;
              part += d;
            }
            if (geom is GeoElement.Point || geom is GeoElement.Points)
            {
              if (part > 0)
              { sb.Append($"{part / 1000.0:N3};"); }
              sb.Append($"{c.Name};");
              part = 0;
            }

            pre = c;
          }
        }
      }

      return sb.ToString();
    }
  }
}
