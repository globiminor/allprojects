using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OCourse.Iof3
{
	[XmlRoot(Namespace = "http://www.orienteering.org/datastandard/3.0")]
	public class CourseData
	{
		public Event Event { get; set; }
		public RaceCourseData RaceCourseData { get; set; }
		[XmlAttribute] public string iofVersion { get; set; } = "3.0";
		[XmlAttribute] public DateTime createTime { get; set; } = DateTime.Now;
	}
	public class Event
	{
		public string Name { get; set; }
	}
	public class RaceCourseData
	{
		public Map Map { get; set; }
		[XmlElement("Control")] public List<Control> Controls { get; set; }
		[XmlElement("Course")] public List<Course> Courses { get; set; }
		[XmlElement("ClassCourseAssignment")] public List<ClassCourseAssignment> ClassCourseAssignments { get; set; }
		[XmlElement("PersonCourseAssignment")] public List<PersonCourseAssignment> PersonCourseAssignments { get; set; }
	}
	public class Map
	{
		public int Scale { get; set; }
		public MapPosition MapPositionTopLeft { get; set; }
		public MapPosition MapPositionBottomRight { get; set; }
	}
	public class Control
	{
		public string Id { get; set; }
		public WgsPosition Position { get; set; }
		public MapPosition MapPosition { get; set; }
	}
	public class WgsPosition
	{
		[XmlAttribute] public double lng { get; set; }
		[XmlAttribute] public double lat { get; set; }
	}
	public class MapPosition
	{
		[XmlAttribute] public double x { get; set; }
		[XmlAttribute] public double y { get; set; }
		[XmlAttribute] public string unit { get; set; }
	}

	public class Course
	{
		public string Name { get; set; }
		public string CourseFamily { get; set; }
		public int Length { get; set; }
		public int Climb { get; set; }
		[XmlElement("CourseControl")]
		public List<CourseControl> CourseControls { get; set; }
	}
	public class CourseControl
	{
		[XmlAttribute] public string type { get; set; }
		public string Control { get; set; }
		public string LegLength { get; set; }
	}

	public class ClassCourseAssignment
	{
		public string ClassName { get; set; }
		public string CourseName { get; set; }
	}
	public class PersonCourseAssignment
	{
		public int BibNumber { get; set; }
		public string CourseName { get; set; }
		public string CourseFamily { get; set; }
	}
}
