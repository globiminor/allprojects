
using Basics.Geom;
using Ocad;
using Ocad.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCourse.Commands
{
	public class CourseMap
	{
		public class ControlElement
		{
			public ControlElement(GeoElement elem)
			{
				P = elem.Geometry.GetGeometry() as Point;
				Elem = elem;
			}
			public Point P { get; }
			public GeoElement Elem { get; }
		}
		public class ConnectionElement
		{
			public ConnectionElement(GeoElement elem)
			{
				L = elem.Geometry.GetGeometry() as Polyline;
				Elem = elem;
			}

			public Polyline L { get; }
			public GeoElement Elem { get; }
		}

		public class ControlNrElement
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

		public BoxTree<ControlElement> ControlsTree => _controlsTree ??
			(_controlsTree = BoxTree.Create(AllControls.Select(e => new ControlElement(e)), (c) => c.P.Extent, 4));
		private BoxTree<ControlElement> _controlsTree;
		public BoxTree<ConnectionElement> ConnectionTree => _connectionTree ??
			(_connectionTree = BoxTree.Create(ConnectionElems.Select(e => new ConnectionElement(e)), (c) => c.L.Extent, 4));
		private BoxTree<ConnectionElement> _connectionTree;

		public BoxTree<ControlNrElement> ControlNrsTree => _controlNrsTree ??
			(_controlNrsTree = BoxTree.Create(ControlNrElems.Select(e => new ControlNrElement(e, ControlNrHeight)), (c) => c.E, 4));
		private BoxTree<ControlNrElement> _controlNrsTree;

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

	}
}
