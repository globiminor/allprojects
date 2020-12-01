
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
		public CmdCourseVerifyLayout()
		{
		}

		public List<int> StartSymbols { get; set; } = new List<int> { 701000 };
		public List<int> ControlSymbols { get; set; } = new List<int> { 703000 };
		public List<int> ControlNrSymbols { get; set; } = new List<int> { 704000 };
		public List<int> ConnectionSymbols { get; set; } = new List<int> { 705000 };
		public List<int> FinishSymbols { get; set; } = new List<int> { 706000 };

		public double ControlDistance { get; set; }
		public List<GeoElement> AllControls { get; set; }
		public List<GeoElement> ConnectionElems { get; set; }
		public List<GeoElement> ControlNrElems { get; set; }
		public double ControlNrHeight { get; set; }

		public void InitFromFile(string courseFile)
		{
			List<GeoElement> startElems = new List<GeoElement>();
			List<GeoElement> controlElems = new List<GeoElement>();
			List<GeoElement> controlNrElems = new List<GeoElement>();
			List<GeoElement> connectionElems = new List<GeoElement>();
			List<GeoElement> finishElems = new List<GeoElement>();
			double controlDistance = 0;
			double textHeight = 0;

			using (OcadReader r = OcadReader.Open(courseFile))
			{
				foreach (var elem in r.EnumGeoElements())
				{
					if (StartSymbols.Contains(elem.Symbol))
					{ startElems.Add(elem); }
					else if (ControlSymbols.Contains(elem.Symbol))
					{ controlElems.Add(elem); }
					else if (ControlNrSymbols.Contains(elem.Symbol))
					{ controlNrElems.Add(elem); }
					else if (ConnectionSymbols.Contains(elem.Symbol))
					{ connectionElems.Add(elem); }
					else if (FinishSymbols.Contains(elem.Symbol))
					{ finishElems.Add(elem); }
				}

				double mapDistance = 0;
				foreach (var symbol in r.ReadSymbols())
				{
					if (ControlSymbols.Contains(symbol.Number) && symbol is PointSymbol p)
					{
						foreach (var g in p.Graphics)
						{
							IBox ext = g.MapGeometry.Extent;
							mapDistance = Math.Max(mapDistance, Math.Abs(ext.Min.X));
							mapDistance = Math.Max(mapDistance, Math.Abs(ext.Min.Y));
							mapDistance = Math.Max(mapDistance, Math.Abs(ext.Max.X));
							mapDistance = Math.Max(mapDistance, Math.Abs(ext.Max.Y));
						}
					}
					if (ControlNrSymbols.Contains(symbol.Number) && symbol is TextSymbol t)
					{
						double pts = t.Size / 10.0;
						textHeight = Math.Max(textHeight, pts / 4.05); // mm
					}
				}

				controlDistance = mapDistance * FileParam.OCAD_UNIT * r.Setup.Scale;
				textHeight = textHeight * 0.001 * r.Setup.Scale; // m
			}

			List<GeoElement> allControls = new List<GeoElement>(controlElems);
			allControls.AddRange(startElems);
			allControls.AddRange(finishElems);

			ControlDistance = controlDistance;
			AllControls = allControls;
			ConnectionElems = connectionElems;
			ControlNrElems = controlNrElems;
			ControlNrHeight = textHeight;
		}

		private BoxTree<ControlElement> ControlsTree => _controlsTree ??
			(_controlsTree = BoxTree.Create(AllControls.Select(e => new ControlElement(e)), (c) => c.P.Extent, 4));
		private BoxTree<ControlElement> _controlsTree;
		private BoxTree<ConnectionElement> ConnectionTree => _connectionTree ??
			(_connectionTree = BoxTree.Create(ConnectionElems.Select(e => new ConnectionElement(e)), (c) => c.L.Extent, 4));
		private BoxTree<ConnectionElement> _connectionTree;

		private BoxTree<ControlNrElement> ControlNrsTree => _controlNrsTree ??
			(_controlNrsTree = BoxTree.Create(ControlNrElems.Select(e => new ControlNrElement(e, ControlNrHeight)), (c) => c.E, 4));
		private BoxTree<ControlNrElement> _controlNrsTree;

		public void Execute()
		{
			Assert.NotNull(AllControls);
			Assert.NotNull(ConnectionElems);

			foreach (var msg in VerfiyConnections(1.5 * ControlDistance))
			{

			};
			foreach (var msg in VerifyControlNrs())
			{

			}
		}

		private class ControlElement
		{
			public ControlElement(GeoElement elem)
			{
				P = elem.Geometry.GetGeometry() as Point;
				Elem = elem;
			}
			public Point P { get; }
			public GeoElement Elem { get; }
		}
		private class ConnectionElement
		{
			public ConnectionElement(GeoElement elem)
			{
				L = elem.Geometry.GetGeometry() as Polyline;
				Elem = elem;
			}

			public Polyline L { get; }
			public GeoElement Elem { get; }
		}

		private class ControlNrElement
		{
			public ControlNrElement(GeoElement elem, double textHeight)
			{
				PointCollection pts = (PointCollection)elem.Geometry.GetGeometry();
				E = new Box(new Point2D(pts[0].X, pts[0].Y), new Point2D(pts[2].X, pts[0].Y + textHeight));
				Elem = elem;
			}
			public Box E { get; }
			public GeoElement Elem { get; }
		}


		private IEnumerable<string> VerfiyConnections(double distance, double near = -1)
		{
			BoxTree<ControlElement> controlsTree = Assert.NotNull(ControlsTree);
			IList<GeoElement> connections = Assert.NotNull(ConnectionElems);

			if (near < 0)
			{
				near = 0.1 * distance;
			}

			double dist2 = distance * distance;
			double near2 = near * near;

			foreach (var connection in connections)
			{
				if (!(connection.Geometry.GetGeometry() is Polyline line))
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

				List<ControlElement> starts = new List<ControlElement>();
				List<ControlElement> ends = new List<ControlElement>();
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
			BoxTree<ControlElement> controlsTree = Assert.NotNull(ControlsTree);
			BoxTree<ControlNrElement> controlNrsTree = Assert.NotNull(ControlNrsTree);
			BoxTree<ConnectionElement> connectionTree = Assert.NotNull(ConnectionTree);

			IList<GeoElement> controlNrs = Assert.NotNull(ControlNrElems);

			foreach (var controlNrEntry in controlNrsTree.Search(null))
			{
				ControlNrElement elem = controlNrEntry.Value;

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

			}
		}
	}

}
