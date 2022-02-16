
using Basics;
using Basics.Geom;
using Grid;
using Ocad;
using Ocad.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCourse.Commands
{
	public class CmdCourseVerifyLayout
	{
		private readonly CourseMap _cm;
		public CmdCourseVerifyLayout(CourseMap courseMap)
		{
			_cm = courseMap;
		}


		public void Execute()
		{
			Assert.NotNull(_cm.AllControls);
			Assert.NotNull(_cm.ConnectionElems);

			foreach (var msg in VerfiyConnections(1.5 * _cm.ControlDistance))
			{

			};
			foreach (var msg in VerifyControlNrs())
			{

			}
		}

		private IEnumerable<string> VerfiyConnections(double distance, double near = -1)
		{
			BoxTree<CourseMap.ControlElement> controlsTree = Assert.NotNull(_cm.ControlsTree);
			IList<MapElement> connections = Assert.NotNull(_cm.ConnectionElems);

			if (near < 0)
			{
				near = 0.1 * distance;
			}

			double dist2 = distance * distance;
			double near2 = near * near;

			foreach (var connection in connections)
			{
				if (!(connection.GetMapGeometry().GetGeometry() is Polyline line))
				{
					yield return "Invalid Geometrie";
					continue;
				}
				Box box = new Box(line.Extent);
				box.Min.X -= distance;
				box.Min.Y -= distance;
				box.Max.X += distance;
				box.Max.Y += distance;

				ISegment sSeg = line.GetSegment(0);
				IPoint p0 = sSeg.PointAt(0);
				IPoint t0 = sSeg.TangentAt(0);
				Line sLine = new Line(p0, PointOp.Add(p0, PointOp.Scale(-distance, t0)));

				ISegment eSeg = line.GetSegment(-1);
				IPoint p1 = eSeg.PointAt(1);
				IPoint t1 = eSeg.TangentAt(1);
				Line eLine = new Line(p1, PointOp.Add(p1, PointOp.Scale(distance, t1)));

				List<CourseMap.ControlElement> starts = new List<CourseMap.ControlElement>();
				List<CourseMap.ControlElement> ends = new List<CourseMap.ControlElement>();
				foreach (var entry in controlsTree.Search(box))
				{
					if (sLine.Distance2(entry.Value.P) < near2)
					{
						starts.Add(entry.Value);
						continue;
					}
					if (eLine.Distance2(entry.Value.P) < near2)
					{
						ends.Add(entry.Value);
						continue;
					}

					foreach (var seg in line.EnumSegments())
					{
						if (!(seg is Line part))
						{
							yield return $"unexpected geometry type of connection segment in {connection.ObjectString}";
							continue;
						}

						double d2 = part.Distance2(entry.Value.P);
						if (d2 < dist2)
						{
							yield return $"control {entry.Value.Elem.ObjectString} to close to connection {connection.ObjectString}";
						}
					}
				}

				if (starts.Count <= 0)
				{
					yield return $"No control found at start of connection {connection.ObjectString}";
				}
				if (starts.Count > 1)
				{
					yield return $"{starts.Count} controls found at start of connection {connection.ObjectString}";
				}

				if (ends.Count <= 0)
				{
					yield return $"No control found at end of connection {connection.ObjectString}";
				}
				if (ends.Count > 1)
				{
					yield return $"{ends.Count} controls found at end of connection {connection.ObjectString}";
				}

			}
		}
		private IEnumerable<string> VerifyControlNrs()
		{
			BoxTree<CourseMap.ControlElement> controlsTree = Assert.NotNull(_cm.ControlsTree);
			BoxTree<CourseMap.ControlNrElement> controlNrsTree = Assert.NotNull(_cm.ControlNrsTree);
			BoxTree<CourseMap.ConnectionElement> connectionTree = Assert.NotNull(_cm.ConnectionTree);
			BoxTree<CourseMap.AreaElement> layoutsTree = Assert.NotNull(_cm.LayoutsTree);

			IList<MapElement> controlNrs = Assert.NotNull(_cm.ControlNrElems);

			foreach (var controlNrEntry in controlNrsTree.Search(null))
			{
				CourseMap.ControlNrElement elem = controlNrEntry.Value;

				Box search = elem.E;
				foreach (var entry in controlsTree.Search(search))
				{

				}
				foreach (var entry in connectionTree.Search(search))
				{
					if (GeometryOperator.Intersects(entry.Value.L, search))
					{
						yield return $"{elem.Elem.Text} intersects {entry.Value.Elem.ObjectString}";
					}
				}
				foreach (var entry in controlNrsTree.Search(search))
				{
					if (GeometryOperator.Intersects(entry.Value.E, search))
					{
						yield return $"{elem.Elem.Text} intersects {entry.Value.Elem.ObjectString}";
					}
				}
				foreach (var entry in layoutsTree.Search(search))
				{
					if (GeometryOperator.Intersects(entry.Value.A, search))
					{
						yield return $"Layout element intersects {entry.Value.Elem.ObjectString}";
					}
				}

			}
		}
	}

}
