using Basics;
using Basics.Cmd;
using Basics.Geom;
using Ocad;
using OCourse.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OCourse.Cmd.Commands
{
  public class PlaceCommand : ICommand
  {
    public class Parameters : IDisposable
    {
      private static readonly List<Command<Parameters>> _cmds = new List<Command<Parameters>>
        {
            new Command<Parameters>
            {
                Key = "-c",
                Parameters = "<course path>",
                Info = "File part can contain template characters (*)",
                Read = (p, args, i) =>
                {
                    p.CoursePath = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-o",
                Parameters = "<output path>",
                Info = "output path. Use [<idx>] to take <idx>th * in path template. " +
                 " If not spezified, the character '.p' will be place before .ocd of the course input path",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.OutputPath = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-l",
                Parameters = "<custom layouts>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.CustomLayouts = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-p",
                Parameters = "<overprint symbol>",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.ControlNrOverprintSymbol = int.Parse(args[i + 1]);
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-lc",
                Parameters = "<class>",
                Info = "class for custom layout. Use [<idx>] to take <idx>th * in path template",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.Class = args[i + 1];
                    return 1;
                }
            },
            new Command<Parameters>
            {
                Key = "-r",
                Parameters = "<search> <replace",
                Info = "Replace ocad element texts = <search> by <replace>. Use [<idx>] to take <idx>th * in path template",
                Optional = true,
                Read = (p, args, i) =>
                {
                    p.SearchText = args[i + 1];
                    p.ReplaceText = args[i + 2];
                    return 2;
                }
            },

        };

      public string CoursePath { get; set; }
      public string OutputPath { get; set; }
      public string CustomLayouts { get; set; }
      public string Class { get; set; }
      public int ControlNrOverprintSymbol { get; set; }
      public string SearchText { get; set; }
      public string ReplaceText { get; set; }

      public void Dispose()
      { }
      public string Validate()
      {
        StringBuilder sb = new StringBuilder();
        string error = sb.ToString();

        return sb.ToString();
      }
      /// <summary>
      ///     interpret and verify command line arguments
      /// </summary>
      /// <param name="args"></param>
      /// <returns>null, if not successfull, otherwise interpreted parameters</returns>
      public static ICommand ReadArgs(IList<string> args)
      {
        Parameters result = new Parameters();

        if (Basics.Cmd.Utils.ReadArgs(result, args, _cmds, out string error))
        {
          if (string.IsNullOrWhiteSpace(error))
          {
            error = result.Validate();
          }
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
          Console.WriteLine(error);
          return null;
        }

        return new PlaceCommand(result);
      }
    }

    private readonly Parameters _pars;
    public PlaceCommand(Parameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {
        CustomLayoutImpl customLayoutImpl = new CustomLayoutImpl(_pars.CustomLayouts);
        int classIdx = GetClassIndex();

        Dictionary<string, Func<CourseMap, string, double, IReadOnlyList<Polyline>, IPoint>> classLayoutDict
          = new Dictionary<string, Func<CourseMap, string, double, IReadOnlyList<Polyline>, IPoint>>();
        TemplateUtils tmpl = new TemplateUtils(_pars.CoursePath);
        foreach (var mapFile in Directory.EnumerateFiles(Path.GetDirectoryName(_pars.CoursePath), tmpl.Template))
        {
          string mapFileName = Path.GetFileName(mapFile);
          string className = GetClassName(mapFile, classIdx, tmpl);

          CourseMap cm = new CourseMap();
          cm.InitFromFile(mapFile);
          cm.ControlNrOverprintSymbol = _pars.ControlNrOverprintSymbol;

          string result = GetResultName(mapFile, tmpl);

          File.Copy(mapFile, result, overwrite: true);

          using (OcadWriter w = OcadWriter.AppendTo(result))
          {
            CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
            cmd.GetCustomPosition = (courseMap, controlName, textWidth, polylines) =>
              customLayoutImpl.GetCustomPosition(className, mapFile, courseMap, controlName, textWidth, polylines);
            cmd.Execute();
          }
          if (!string.IsNullOrEmpty(_pars.SearchText))
          {
            string search = tmpl.GetReplaced(mapFile, _pars.SearchText);
            string replace = tmpl.GetReplaced(mapFile, _pars.ReplaceText);
            using (OcadWriter w = OcadWriter.AppendTo(result))
            {
              w.AdaptElements((e) =>
              {
                if (e.Text != search)
                { return null; }
                using (System.Drawing.Font font = new System.Drawing.Font(cm.CourseNameFont, 12))
                { e.SetText(replace, font: font); }
                return e;
              });
            }
          }
        }
      }
      finally
      {
        _pars.Dispose();
      }
      error = null;
      return true;
    }

    private int GetClassIndex()
    {
      if (_pars.Class != null && _pars.Class.StartsWith("[") == true && _pars.Class.EndsWith("]"))
      {
        return int.Parse(_pars.Class.Substring(1, _pars.Class.Length - 2));
      }
      return -1;
    }

    private string GetClassName(string mapFile, int classIdx, TemplateUtils tmpl)
    {
      if (classIdx < 0)
      { return _pars.Class; }

      string className = tmpl.GetPart(mapFile, classIdx);
      return className;
    }

    private string GetResultName(string mapFile, TemplateUtils tmpl)
    {
      if (string.IsNullOrWhiteSpace(_pars.OutputPath))
      {
        string ext = Path.GetExtension(mapFile);
        string dir = Path.GetDirectoryName(mapFile);
        string name = Path.GetFileNameWithoutExtension(mapFile);
        string result = Path.Combine(dir, $"{name}.p{ext}");
        return result;
      }
      else
      {
        string result = tmpl.GetReplaced(mapFile, _pars.OutputPath);
        return result;
      }
    }

    private class CustomLayoutImpl
    {
      private readonly CustomLayout _customLayout;
      private readonly DataTable _courseTable;
      private readonly DataRow _courseRow;
      private readonly List<CustomHelper> _customHelpers;
      private readonly List<CustomHelper> _freepartHelpers;

      private readonly HashSet<string> _messages;

      private const string CourseCol = "Course";
      private const string ControlCol = "Control";

      private const string TextWidthCol = "TextWidth";
      private const string XCol = "X";
      private const string YCol = "Y";

      private const string FromCol = "FromCtrl";
      private const string ToCol = "ToCtrl";

      private const double _mm = 100; // 


      public CustomLayoutImpl(string customLayoutFile)
      {
        _customLayout = null;
        if (string.IsNullOrWhiteSpace(customLayoutFile))
        { return; }

        using (TextReader r = new StreamReader(customLayoutFile))
        {
          Serializer.Deserialize(out _customLayout, r);
        }

        _messages = new HashSet<string>();

        _courseTable = new DataTable();
        _courseTable.Columns.Add(CourseCol, typeof(string));
        _courseTable.Columns.Add(ControlCol, typeof(string));

        _courseRow = _courseTable.NewRow();
        _courseTable.Rows.Add(_courseRow);

        _customHelpers = new List<CustomHelper>();
        foreach (var customGeometries in _customLayout.CustomGeometries.Where(x => !x.hasFreeParts))
        {
          foreach (var customGeometry in customGeometries.CustomGeometry)
          {
            CustomHelper customHelper = Create(customGeometry, createPositionTable: true);
            _customHelpers.Add(customHelper);
          }
        }

        _freepartHelpers = new List<CustomHelper>();
        foreach (var customGeometries in _customLayout.CustomGeometries.Where(x => x.hasFreeParts))
        {
          foreach (var customGeometry in customGeometries.CustomGeometry)
          {
            CustomHelper customHelper = Create(customGeometry, createPositionTable: false);
            _freepartHelpers.Add(customHelper);
          }
        }
      }

      private CustomHelper Create(CustomGeometry customGeometry, bool createPositionTable)
      {
        CustomHelper customHelper = new CustomHelper();
        customHelper.CustomGeometry = customGeometry;

        DataView view = new DataView(_courseTable);
        view.RowFilter = customGeometry.where;
        customHelper.CourseView = view;

        if (createPositionTable)
        {
          DataTable positionTbl = new DataTable();
          positionTbl.Columns.Add(TextWidthCol, typeof(double));
          positionTbl.Columns.Add(XCol, typeof(double), customGeometry.Point.x);
          positionTbl.Columns.Add(YCol, typeof(double), customGeometry.Point.y);
          positionTbl.Rows.Add(positionTbl.NewRow());
          customHelper.PositionTable = positionTbl;
        }

        Dictionary<DataView, MapLine> customLines = new Dictionary<DataView, MapLine>();
        if (customGeometry.Line != null)
        {
          foreach (var customLine in customGeometry.Line)
          {
            DataTable connectionTable = new DataTable();
            connectionTable.Columns.Add(FromCol, typeof(string));
            connectionTable.Columns.Add(ToCol, typeof(string));
            connectionTable.Rows.Add(connectionTable.NewRow());

            DataView connectionView = new DataView(connectionTable);
            connectionView.RowFilter = customLine.where;
            customLines.Add(connectionView, customLine);
          }
        }
        customHelper.CustomLines = customLines;

        return customHelper;
      }

      public IPoint GetCustomPosition(string course, string courseFile, CourseMap courseMap, string controlName, double textWidth, IReadOnlyList<Polyline> freePlaces)
      {
        if (_customLayout == null)
        { return null; }

        IList<string> controlParts = controlName.Split('-');
        string ctrl = controlParts[1].Trim();
        _courseRow[CourseCol] = course;
        _courseRow[ControlCol] = ctrl;

        foreach (var withFreeParts in _freepartHelpers)
        {
          if (withFreeParts.CourseView.Count != 1)
          { continue; }

          Dictionary<MapElement, Polyline> replaceDict = GetReplaceDict(courseMap, withFreeParts.CustomLines);
          if (replaceDict == null)
          { continue; }

          foreach (var pair in replaceDict)
          {
            // add only if not existing
            if (courseMap.CustomGeometries.ContainsKey(pair.Key))
            { continue; }
            courseMap.CustomGeometries.Add(pair.Key, new GeoElement.Line(pair.Value));
          }
        }
        if (freePlaces?.Count > 0)
        { return null; }

        foreach (var customHelper in _customHelpers)
        {
          if (customHelper.CourseView.Count != 1)
          { continue; }

          Dictionary<MapElement, Polyline> replaceDict = GetReplaceDict(courseMap, customHelper.CustomLines);
          if (replaceDict == null)
          { continue; }

          foreach (var pair in replaceDict)
          {
            // overwrite existing entry if necessairy
            courseMap.CustomGeometries[pair.Key] = new GeoElement.Line(pair.Value);
          }

          DataRow positionRow = customHelper.PositionTable.Rows[0];
          positionRow[TextWidthCol] = textWidth / _mm;
          if (customHelper.CustomGeometry.TextReplace is TextReplace tr)
          {
            TemplateUtils tmpl = new TemplateUtils(tr.template, fullPath: true);
            string replaced = tmpl.GetReplaced(controlName, tr.replaceBy, fullSource: true);
            courseMap.CustomTexts = courseMap.CustomTexts ?? new Dictionary<string, string>();
            courseMap.CustomTexts.Add(controlName, replaced);
          }
          return new Point2D(_mm * (double)positionRow[XCol], _mm * (double)positionRow[YCol]);
        }

        string msg = $"Unable to place Control '{ctrl}' in Course '{course}'";
        if (_messages.Add(msg))
        {
          Console.WriteLine($"{msg} (first in {courseFile})");
        }
        return null;
      }

      private Dictionary<MapElement, Polyline> GetReplaceDict(CourseMap courseMap, Dictionary<DataView, MapLine> customLines)
      {
        bool success = true;
        Dictionary<MapElement, Polyline> replaceDict = new Dictionary<MapElement, Polyline>();
        foreach (var customLine in customLines)
        {
          success = false;
          DataView lineView = customLine.Key;
          DataRow connRow = customLine.Key.Table.Rows[0];
          foreach (var connectionElem in courseMap.ConnectionElems)
          {
            if (string.IsNullOrWhiteSpace(connectionElem.ObjectString))
            { continue; }
            Ocad.StringParams.NamedParam strPar = new Ocad.StringParams.NamedParam(connectionElem.ObjectString);
            string fromCtrl = strPar.GetString('f');
            if (fromCtrl.StartsWith("S") && fromCtrl.Length > 2)
            { fromCtrl = fromCtrl.Substring(1); }
            string toCtrl = strPar.GetString('t');

            connRow[FromCol] = fromCtrl;
            connRow[ToCol] = toCtrl;
            if (lineView.Count == 1)
            {
              Polyline replaceLine = GetReplaceLine(connectionElem, customLine.Value, _mm);

              if (replaceLine != null)
              { replaceDict.Add(connectionElem, replaceLine); }

              success = true;
              break;
            }
          }
          if (!success)
          { break; }
        }

        return success ? replaceDict : null;
      }

      private Polyline GetReplaceLine(MapElement connectionElem, MapLine customLine, double mm)
      {
        if (!(customLine.Geometry?.Count > 0)) // if has no geometry
        { return null; }

        GeoElement.Line line = (GeoElement.Line)connectionElem.GetMapGeometry();
        Polyline b = new Polyline();
        foreach (var geom in customLine.Geometry)
        {
          if (geom is MapPointIndex pi)
          {
            b.Add(line.BaseGeometry.GetPoint(pi.index));
          }
          else if (geom is MapPoint p)
          {
            b.Add(new Point2D(mm * double.Parse(p.x), mm * double.Parse(p.y)));
          }
          else if (geom is MapSegment seg)
          {
            if (seg.noLine == "true")
            {
              b.Add(new Coord.CodePoint(mm * seg.x0, mm * seg.y0) { Flags = Coord.Flags.noLine | Coord.Flags.noLeftLine | Coord.Flags.noRightLine });
            }
            else
            {
              b.Add(new Point2D(mm * seg.x0, mm * seg.y0));
            }
            b.Add(new Point2D(mm * seg.x1, mm * seg.y1));
          }
          else
          { throw new NotImplementedException($"unhandled geometry type {geom.GetType()}"); }
        }
        return b;
      }
    }

    private class CustomHelper
    {
      public CustomGeometry CustomGeometry { get; set; }
      public DataView CourseView { get; set; }
      public DataTable PositionTable { get; set; }
      public Dictionary<DataView, MapLine> LineViews { get; set; }
      public Dictionary<DataView, MapLine> CustomLines { get; set; }
    }

    [XmlRoot]
    public class CustomLayout
    {
      [XmlElement]
      public List<CustomGeometries> CustomGeometries { get; set; }
    }
    public class CustomGeometries
    {
      [XmlAttribute]
      public bool hasFreeParts { get; set; }
      [XmlElement]
      public List<CustomGeometry> CustomGeometry { get; set; }
    }

    public class CustomGeometry
    {
      [XmlAttribute]
      public string where { get; set; }

      public MapPoint Point { get; set; }
      public TextReplace TextReplace { get; set; }

      [XmlElement]
      public List<MapLine> Line { get; set; }
    }

    public class TextReplace
    {
      [XmlAttribute]
      public string template { get; set; }
      [XmlAttribute]
      public string replaceBy { get; set; }
    }
    public class MapPoint : MapGeom
    {
      [XmlAttribute]
      public string x { get; set; }
      [XmlAttribute]
      public string y { get; set; }
    }
    public class MapPointIndex : MapGeom
    {
      [XmlAttribute]
      public int index { get; set; }
    }
    public class MapSegment : MapGeom
    {
      [XmlAttribute]
      public string noLine { get; set; }
      [XmlAttribute]
      public double x0 { get; set; }
      [XmlAttribute]
      public double y0 { get; set; }
      [XmlAttribute]
      public double x1 { get; set; }
      [XmlAttribute]
      public double y1 { get; set; }
    }

    [XmlInclude(typeof(MapSegment))]
    [XmlInclude(typeof(MapPoint))]
    [XmlInclude(typeof(MapPointIndex))]
    public class MapGeom { }

    public class MapLine
    {
      [XmlAttribute]
      public string where { get; set; }

      [XmlArrayItem(typeof(MapPoint), ElementName = "Point")]
      [XmlArrayItem(typeof(MapPointIndex), ElementName = "PointIndex")]
      [XmlArrayItem(typeof(MapSegment), ElementName = "Segment")]
      public List<MapGeom> Geometry { get; set; }
    }
  }
}
