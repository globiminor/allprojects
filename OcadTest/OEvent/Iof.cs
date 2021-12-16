using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Iof3;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OcadTest.OEvent
{
	[TestClass]
	public class Iof
	{
		[TestMethod]
		public void TestExportIOF3()
		{
			CourseData cd = new CourseData
			{
				Event = new Event { Name = "NOM2022" },
				RaceCourseData = new RaceCourseData
				{
					Map = new Map { Scale = 15000, MapPositionTopLeft = new MapPosition { x = 1, y = 1, unit = "mm" }, MapPositionBottomRight = new MapPosition { x = 300, y = 200, unit = "mm" } },
					Controls = new List<Control>
					{
						new Control {Id = "S1", Position = new WgsPosition{ lng = 8.30000, lat = 47.5000} , MapPosition = new MapPosition{ x = 10, y = 10, unit = "mm"}},
						new Control {Id = "30", Position = new WgsPosition{ lng = 8.30010, lat = 47.5001} , MapPosition = new MapPosition{ x = 20, y = 20, unit = "mm"}}
					},
					Courses = new List<Course>
					{
						new Course
						{
							Name = "R2", Length = 10000, Climb = 100,
							CourseControls = new List<CourseControl>
							{
								new CourseControl { type = "Start", Control = "S1"},
								new CourseControl { type = "Control", Control = "30", LegLength = 100.ToString()},
								new CourseControl { type = "Finish", Control = "F1", LegLength = 100.ToString()}
							}
						},

						new Course
						{
							Name = "R3_V1", CourseFamily = "R3", Length = 10000, Climb = 100,
							CourseControls = new List<CourseControl>
							{
								new CourseControl { type = "Start", Control = "S1"},
								new CourseControl { type = "Control", Control = "30", LegLength = 100.ToString()},
								new CourseControl { type = "Finish", Control = "F1", LegLength = 100.ToString()}
							}
						},
						new Course
						{
							Name = "R3_V2", CourseFamily = "R3", Length = 10000, Climb = 100,
							CourseControls = new List<CourseControl>
							{
								new CourseControl { type = "Start", Control = "S1"},
								new CourseControl { type = "Control", Control = "30", LegLength = 100.ToString()},
								new CourseControl { type = "Finish", Control = "F1", LegLength = 100.ToString()}
							}
						}
					},
					ClassCourseAssignments = new List<ClassCourseAssignment>
					{
						new ClassCourseAssignment{ ClassName="HE", CourseName="HE"},
						new ClassCourseAssignment{ ClassName="DE", CourseName="DE"}
					},
					PersonCourseAssigments = new List<PersonCourseAssigment>
					{
						new PersonCourseAssigment { BibNumber = 100, CourseName = "R3_V1", CourseFamily = "R3"},
						new PersonCourseAssigment { BibNumber = 100, CourseName = "R3_V2", CourseFamily = "R3"},
						new PersonCourseAssigment { BibNumber = 100, CourseName = "R2"}
					}
				}
			};

			StringBuilder sb = new StringBuilder();
			using (TextWriter t = new StringWriter(sb))
			{
				Basics.Serializer.Serialize(cd, t,
					namespaces: new Dictionary<string, string> {
					{ "", "http://www.orienteering.org/datastandard/3.0" },
					{ "xsi", "http://www.w3.org/2001/XMLSchema-instance"} });
			}
			string xml = sb.ToString();
		}
	}
}
