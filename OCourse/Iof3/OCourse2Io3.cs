using Basics;
using Basics.Geom;
using OCourse.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OCourse.Iof3
{
  public class OCourse2Io3
  {

    public static OCourse2Io3 Init(string name, string ocadFile)
    {
      CourseData data = new CourseData
      {
        Event = new Event { Name = name },
        RaceCourseData = new RaceCourseData { Map = new Map() },
        createTime = DateTime.Now
      };
      return Init(data, ocadFile);
    }
    public static OCourse2Io3 Init(CourseData data, string ocadFile)
    {
      using (Ocad.OcadReader r = Ocad.OcadReader.Open(ocadFile))
      {
        Ocad.Setup setup = r.ReadSetup();
        data.RaceCourseData.Map.Scale = (int)setup.Scale;

        IProjection prjLv95_Wgs84 = new Basics.Geom.Projection.TransferProjection(new Basics.Geom.Projection.Ch1903_LV95(), Basics.Geom.Projection.Geographic.Wgs84());

        return new OCourse2Io3(data, setup.Prj2Map, prjLv95_Wgs84);
      }
    }


    private readonly CourseData _courseData;
    private readonly RaceCourseData _raceData;
    private readonly IProjection _prj2Map;
    private readonly IProjection _prj2Wgs;

    private readonly Dictionary<string, Course> _courseDict;
    private readonly Dictionary<string, Control> _controlDict;

    private OCourse2Io3(CourseData data, IProjection prj2Map, IProjection prj2Wgs)
    {
      _courseData = data;
      _courseData.RaceCourseData = _courseData.RaceCourseData ?? new RaceCourseData();
      _prj2Map = prj2Map;
      _prj2Wgs = prj2Wgs;

      _raceData = _courseData.RaceCourseData;

      _raceData.Controls = _raceData.Controls ?? new List<Control>();
      _raceData.Courses = _raceData.Courses ?? new List<Course>();
      _raceData.PersonCourseAssignments = _raceData.PersonCourseAssignments ?? new List<PersonCourseAssignment>();

      _courseDict = _raceData.Courses.ToDictionary(x => x.Name);
      _controlDict = _raceData.Controls.ToDictionary(x => x.Id);
    }

    public CourseData CourseData => _courseData;
    public void AddCurrentPermutations(OCourseVm vm)
    {
      // add courses
      foreach (var permutation in vm.Permutations)
      {
        foreach (var part in permutation.Parts)
        {
          string name = part.GetName();
          string fullName = $"{vm.Course.Name}_{part.PreName}{name}";

          _raceData.PersonCourseAssignments.Add(new PersonCourseAssignment
          {
            BibNumber = permutation.StartNr,
            CourseName = fullName,
            CourseFamily = vm.Course.Name
          });

          if (_courseDict.ContainsKey(fullName))
          {
            continue;
          }

          Course course = Create(fullName, vm.Course.Name, part, vm);
          _raceData.Courses.Add(course);
          _courseDict.Add(fullName, course);

          foreach (var control in part.Controls)
          {
            if (_controlDict.ContainsKey(control.Name))
            { continue; }

            Control cntr = Create(control);
            _raceData.Controls.Add(cntr);
            _controlDict.Add(cntr.Id, cntr);
          }
        }
      }
    }

    public void InitMapInfo()
    {
      if ((_controlDict?.Count) <= 0)
      {
        return;
      }

      Box box = null;
      foreach (var ctr in _controlDict.Values)
      {
        Point p = new Point2D(ctr.MapPosition.x, ctr.MapPosition.y);
        box = box ?? new Box(p.Clone(), p.Clone());
        IPoint ip = p;
        box.Include(ip);
      }

      _raceData.Map.MapPositionTopLeft = new MapPosition { x = box.Min.X - 10, y = box.Max.Y + 10, unit = "mm" };
      _raceData.Map.MapPositionBottomRight = new MapPosition { x = box.Max.X + 10, y = box.Min.Y - 10, unit = "mm" };
    }

    public void Export(string file)
    {
      using (TextWriter w = new StreamWriter(file))
      {
        Serializer.Serialize(CourseData, w, namespaces: new Dictionary<string, string> {
          { "", "http://www.orienteering.org/datastandard/3.0" },
          { "xsi", "http://www.w3.org/2001/XMLSchema-instance"} });
      }
    }
    private Control Create(Ocad.Control control)
    {
      IGeometry p0 = (IGeometry)control.Element.Geometry.EnumPoints().First();
      IPoint p = (IPoint)p0.Project(_prj2Wgs);
      IPoint pMap = (IPoint)p0.Project(_prj2Map);
      Control created = new Control
      {
        Id = control.Name,
        Position = new WgsPosition
        {
          lat = p.Y,
          lng = p.X
        },
        MapPosition = new MapPosition
        {
          x = Math.Round(pMap.X * Ocad.FileParam.OCAD_UNIT * 1000, 2),
          y = Math.Round(pMap.Y * Ocad.FileParam.OCAD_UNIT * 1000, 2),
          unit = "mm"
        }
      };
      return created;
    }

    private static Course Create(string name, string courseFamily, Ext.SectionList sections, OCourseVm vm)
    {
      Course course = new Course
      {
        Name = name,
        CourseFamily = courseFamily,
        CourseControls = new List<CourseControl>()
      };
      double sumLength = 0;
      double sumClimb = 0;
      double preLength = 0;
      Ocad.Control pre = null;
      foreach (var control in sections.Controls)
      {
        double legLength = -1;
        if (pre != null)
        {
          Route.CostFromTo cost = vm.RouteCalculator.GetCost(pre.Name, control.Name);

          legLength = cost.Direct + preLength;
          preLength = legLength;

          sumLength += cost.Direct;
          sumClimb += cost.Climb;
        }

        if (control.Code != Ocad.ControlCode.MarkedRoute)
        {
          CourseControl c = new CourseControl { Control = control.Name };
          c.type =
            control.Code == Ocad.ControlCode.Control ? "Control" :
            control.Code == Ocad.ControlCode.Start ? "Start" :
            control.Code == Ocad.ControlCode.Finish ? "Finish" : "unknown";
          course.CourseControls.Add(c);

          if (legLength > 0)
          { c.LegLength = $"{(int)legLength}"; }

          preLength = 0;
        }
        pre = control;
      }
      course.Length = (int)sumLength;
      course.Climb = (int)Basics.Utils.Round(sumClimb, 5);
      return course;
    }
  }
}
