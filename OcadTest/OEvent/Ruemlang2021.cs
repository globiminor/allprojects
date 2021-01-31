using Basics;
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Cmd.Commands;
using OCourse.Commands;
using System.Collections.Generic;
using System.IO;

namespace OcadTest.OEvent
{
	[TestClass]
	public class Ruemlang2021
	{
		[TestMethod]
		public void TestLayout()
		{
			CourseMap cm = new CourseMap();
			cm.InitFromFile(@"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.1.ocd");
			CmdCourseVerifyLayout cmd = new CmdCourseVerifyLayout(cm);
			cmd.Execute();
		}

		[TestMethod]
		public void TestPlaceNrs()
		{
			//string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.2.ocd";
			string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_10k_A4_V.Bahn2.86.2.ocd";
			CourseMap cm = new CourseMap();
			cm.InitFromFile(mapFile);
			string tmp = @"C:\daten\temp\bahn.ocd";
			File.Copy(mapFile, tmp, overwrite: true);

			using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(tmp))
			{
				CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
				cmd.Execute();
			}
		}

		[TestMethod]
		public void PrepareCourses()
		{
			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";

			Tmpl a4_15k = new Tmpl("tmpl_15k_A4_V.ocd", "tmpl_15k_A4_H.ocd");
			Tmpl a3_10k = new Tmpl("tmpl_10k_A3_V.ocd", "tmpl_10k_A3_H.ocd");
			Tmpl a4_10k = new Tmpl("tmpl_10k_A4_V.ocd", "tmpl_10k_A4_H.ocd");

			List<Conf> confs = new List<Conf>
			{
				new Conf("04_HE", a4_15k),
				new Conf("01_HAL", a3_10k),
				new Conf("05_HAM", a3_10k),
				new Conf("08_HAK", a4_10k),
				new Conf("10_HB", a3_10k),
				new Conf("05_H35", a3_10k),
				new Conf("05_H40", a3_10k),
				new Conf("05_H45", a3_10k),
				new Conf("05_H50", a3_10k),
				new Conf("05_H55", a3_10k),
				new Conf("11_H60", a3_10k),
				new Conf("06_H65", a3_10k),
				new Conf("14_H70", a3_10k),
				new Conf("08_H75", a4_10k),
				new Conf("10_H80", a4_10k),
				new Conf("13_H20", a4_15k),
				new Conf("12_H18", a3_10k),
				new Conf("05_H16", a3_10k),
				new Conf("14_H14", a3_10k),
				new Conf("H12", a4_10k){ CustomSplitWeight = SplitHD12 },
				new Conf("13_DE", a4_15k),
				new Conf("11_DAL", a3_10k),
				new Conf("07_DAM", a4_10k),
				new Conf("10_DAK", a4_10k),
				new Conf("10_DB", a4_10k),
				new Conf("06_D35", a3_10k),
				new Conf("06_D40", a3_10k),
				new Conf("06_D45", a3_10k),
				new Conf("14_D50", a3_10k),
				new Conf("14_D55", a3_10k),
				new Conf("07_D60", a4_10k),
				new Conf("08_D65", a4_10k),
				new Conf("10_D70", a4_10k),
				new Conf("10_D75", a4_10k),
				new Conf("05_D20", a4_15k),
				new Conf("11_D18", a3_10k),
				new Conf("14_D16", a3_10k),
				new Conf("07_D14", a4_10k),
				new Conf("D12", a4_10k){ CustomSplitWeight = SplitHD12 },
				new Conf("D10/H10", a4_10k),
			};
			Dictionary<Tmpl, List<Conf>> tmplDict = new Dictionary<Tmpl, List<Conf>>();
			foreach (var conf in confs)
			{
				tmplDict.GetOrCreateValue(conf.Tmpl).Add(conf);
			}

			BuildParameters pars = new BuildParameters
			{
				ConfigPath = Path.Combine(root, "bahnen_1.xml"),
				SplitCourses = true
			};
			BuildCommand cmd = new BuildCommand(pars);

			foreach (var pair in tmplDict)
			{
				Tmpl t = pair.Key;
				foreach (var s in new[] { t.Front, t.Back })
				{
					string tmpl = Path.Combine(root, "Karten", s.Replace("tmpl", "Bahnen"));
					File.Copy(Path.Combine(root, s), tmpl, overwrite: true);
					pars.TemplatePath = tmpl;
					pars.OutputPath = tmpl;

					bool first = true;
					foreach (var c in pair.Value)
					{
						pars.Course = c.Course;
						pars.BeginStartNr = "101";
						pars.EndStartNr = "101";
						pars.CustomSplitWeight = c.CustomSplitWeight;
						pars.Validate();
						pars.EndStartNr = (pars.OCourseVm.PermutEstimate + 100).ToString();
						pars.Validate();

						if (first)
						{
							CopyElems(pars);
							first = false;
						}

						cmd.Execute(out string error);
					}
				}
			}
		}

		private int SplitHD12(int nDouble, Dictionary<string, List<Ocad.Control>> dict, Dictionary<string, List<Ocad.Control>> dictPre)
		{
			if (!dictPre.ContainsKey("89"))
			{ return nDouble + 2; }
			return nDouble;
		}
		private IPoint GetCustomPositionDH12(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (cntrText != "8-63")
			{ return null; }

			return new Point2D(10986, -4069);
		}

		private IPoint GetCustomPositionHE(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-93"))
			{ return new Point2D(9099 - textWidth, -7628 ); }

			return null;
		}
		private IPoint GetCustomPositionHAL(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-87"))
			{ return new Point2D(-5598 - textWidth, 197); }

			return null;
		}

		private IPoint GetCustomPositionH45(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-85"))
			{ return new Point2D(3675 - textWidth, -2885); }

			return null;
		}


		private IPoint GetCustomPositionD20(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-85"))
			{ return new Point2D(3675 - textWidth, -2885); }

			return null;
		}


		private class Tmpl
		{
			public Tmpl(string front, string back)
			{
				Front = front;
				Back = back;
			}

			public string Front { get; }
			public string Back { get; }
		}
		private class Conf
		{
			public Conf(string course, Tmpl tmpl)
			{
				Course = course;
				Tmpl = tmpl;
			}

			public string Course { get; }
			public Tmpl Tmpl { get; }
			public System.Func<int, Dictionary<string, List<Ocad.Control>>, Dictionary<string, List<Ocad.Control>>, int> CustomSplitWeight { get; set; }
		}

		private void CopyElems(BuildParameters pars)
		{
			List<int> copySymbols = new List<int> { 708000, 709000, 709001, 709002, 709003, 709004 };
			using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(pars.TemplatePath))
			using (Ocad.OcadReader r = Ocad.OcadReader.Open(pars.OCourseVm.CourseFile))
			{
				foreach (var elem in r.EnumGeoElements())
				{
					if (copySymbols.Contains(elem.Symbol))
					{
						w.Append(elem);
					}
				}
			}
		}

		[TestMethod]
		public void PlaceControls()
		{
			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";
			//string tmpl = Path.Combine(root, "Karten", "*.ocd");
			string tmpl = Path.Combine(root, "Karten", "*.07_DAM.113.1.ocd");

			PlaceParameters pars = new PlaceParameters()
			{
				CoursePath = tmpl,
				ControlNrOverprintSymbol = 704005
			};

			PlaceCommand cmd = new PlaceCommand(pars);

			cmd.Execute(out string error);
		}

		[TestMethod]
		public void AdaptCourses()
		{
			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\Karten";
			foreach (var mapFile in Directory.GetFiles(root))
			{
				IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
				if (parts.Count != 5)
				{ continue; }

				//if (!parts[1].EndsWith("HE") || parts[2] != "110" || parts[3] != "2")
				//{ continue; }

				CourseMap cm = new CourseMap();
				cm.InitFromFile(mapFile);
				cm.ControlNrOverprintSymbol = 704005;
				cm.ControlNrSymbols.Add(cm.ControlNrOverprintSymbol);

				using (System.Drawing.Bitmap bmpFont = new System.Drawing.Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
				using (System.Drawing.Graphics grpFont = System.Drawing.Graphics.FromImage(bmpFont))
				using (System.Drawing.Font font = new System.Drawing.Font(cm.CourseNameFont, 12))
				using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(mapFile))
				{
					CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
					if (parts[1].EndsWith("HE"))
					{ cmd.GetCustomPosition = GetCustomPositionHE; }
					if (parts[1].EndsWith("HAL"))
					{ cmd.GetCustomPosition = GetCustomPositionHAL; }
					if (parts[1].EndsWith("H45"))
					{ cmd.GetCustomPosition = GetCustomPositionH45; }
					if (parts[1].EndsWith("D20"))
					{ cmd.GetCustomPosition = GetCustomPositionD20; }
					if (parts[1].EndsWith("12"))
					{ cmd.GetCustomPosition = GetCustomPositionDH12; }

					cmd.Execute();

					if (parts[3] == "1")
					{
						AdaptCourseName(w, cm, grpFont, font);
					}
				}
			}
		}

		private void AdaptCourseName(Ocad.OcadWriter w, CourseMap cm, System.Drawing.Graphics grpFont, System.Drawing.Font font)
		{
			if (string.IsNullOrWhiteSpace(cm.CourseNameFont))
			{ return; }
			w.AdaptElements((elem) =>
			{
				if (!cm.CourseNameSymbols.Contains(elem.Symbol))
				{ return null; }

				string fullCn = elem.Text;
				string coreCn = fullCn;
				int idx = coreCn.IndexOf("(");
				if (idx > 0)
				{ coreCn = coreCn.Substring(0, idx - 1); }
				idx = coreCn.IndexOf(".");
				if (idx > 0)
				{ coreCn = coreCn.Substring(idx + 1); }

				if (coreCn == fullCn)
				{ return null; }

				float fullW = grpFont.MeasureString(fullCn, font).Width;
				float coreW = grpFont.MeasureString(coreCn, font).Width;

				Ocad.GeoElement.Geom geom = elem.GetMapGeometry();
				Basics.Geom.PointCollection pts = (Basics.Geom.PointCollection)geom.GetGeometry();
				double f = coreW / fullW;
				double fullWidth = pts[2].X - pts[0].X;
				((Basics.Geom.Point)pts[2]).X = pts[0].X + f * fullWidth;
				((Basics.Geom.Point)pts[3]).X = pts[2].X;
				elem.SetMapGeometry(Ocad.GeoElement.Geom.Create(pts));
				elem.Text = coreCn;

				return elem;
			});
		}
	}
}
