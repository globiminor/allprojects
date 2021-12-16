using Basics;
using Basics.Geom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Cmd.Commands;
using OCourse.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace OcadTest.OEvent
{
	[TestClass]
	public class SchlaufenLayout
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

		// private string _root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";
		private string _root = @"C:\daten\felix\kapreolo\karten\wangenerwald\2020\NOM\";
		private string _exportFolder = "Karten";


		[TestMethod]
		public void T01_PrepareCourses()
		{
			//string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\";
			string root = _root;
			string configPath = Path.Combine(root, "NOM.xml");
			string eventName = "NOM 2022";

			string exportFolder = _exportFolder;
			Tmpl a4_15k = new Tmpl("tmpl_15k_A4_V.ocd", "tmpl_15k_A4_H.ocd");
			Tmpl a3_10k = new Tmpl("tmpl_10k_A3_V.ocd", "tmpl_10k_A3_H.ocd");
			Tmpl a4_10k = new Tmpl("tmpl_10k_A4_V.ocd", "tmpl_10k_A4_H.ocd");
			Func<string, string> exportCourseFileFct = (tmpl) => tmpl.Replace("tmpl", "Bahnen");
			string io3File = Path.Combine(root, exportFolder, "NOM.Courses.xml");

			List<Conf> confs = new List<Conf>
			{
				new Conf("HE", a3_10k),
				new Conf("D14", a4_10k),
				new Conf("D75", a4_10k),
				//new Conf("04_HE", a4_15k),
				//new Conf("01_HAL", a4_15k),
				//new Conf("05_HAM", a3_10k),
				//new Conf("08_HAK", a4_10k),
				//new Conf("10_HB", a3_10k),
				//new Conf("05_H35", a3_10k),
				//new Conf("05_H40", a3_10k),
				//new Conf("05_H45", a3_10k){ CustomSplitWeight = SplitH45 },
				//new Conf("05_H50", a3_10k),
				//new Conf("05_H55", a3_10k),
				//new Conf("11_H60", a3_10k),
				//new Conf("06_H65", a3_10k),
				//new Conf("14_H70", a3_10k),
				//new Conf("08_H75", a4_10k),
				//new Conf("10_H80", a4_10k),
				//new Conf("13_H20", a4_15k),
				//new Conf("12_H18", a3_10k),
				//new Conf("05_H16", a3_10k),
				//new Conf("14_H14", a3_10k),
				//new Conf("H12", a4_10k){ CustomSplitWeight = SplitHD12 },
				//new Conf("13_DE", a4_15k),
				//new Conf("11_DAL", a4_15k),
				//new Conf("07_DAM", a4_10k),
				//new Conf("10_DAK", a4_10k),
				//new Conf("10_DB", a4_10k),
				//new Conf("06_D35", a3_10k),
				//new Conf("06_D40", a3_10k),
				//new Conf("06_D45", a3_10k),
				//new Conf("14_D50", a3_10k),
				//new Conf("14_D55", a3_10k),
				//new Conf("07_D60", a4_10k),
				//new Conf("08_D65", a4_10k),
				//new Conf("10_D70", a4_10k),
				//new Conf("10_D75", a4_10k),
				//new Conf("05_D20", a4_15k),
				//new Conf("11_D18", a3_10k),
				//new Conf("14_D16", a3_10k),
				//new Conf("07_D14", a4_10k),
				//new Conf("D12", a4_10k){ CustomSplitWeight = SplitHD12 },
				//new Conf("D10/H10", a4_10k),
			};
			int iStart = 101;
			Dictionary<Tmpl, List<Conf>> tmplDict = new Dictionary<Tmpl, List<Conf>>();
			foreach (var conf in confs)
			{
				conf.StartNr = iStart;
				conf.EndNr = iStart + 7;
				iStart += 40;
				tmplDict.GetOrCreateValue(conf.Tmpl).Add(conf);
			}

			BuildParameters pars = new BuildParameters
			{
				ConfigPath = configPath,
				SplitCourses = true
			};
			BuildCommand cmd = new BuildCommand(pars);


			OCourse.Iof3.OCourse2Io3 io3Exporter = null;

			foreach (var pair in tmplDict)
			{
				Tmpl t = pair.Key;

				Dictionary<string, List<Ocad.Course>> coursesDict
					= new Dictionary<string, List<Ocad.Course>>();

				foreach (var c in pair.Value)
				{
					pars.Course = c.Course;

					//iStart = 101;
					iStart = c.StartNr;
					pars.BeginStartNr = iStart.ToString();
					pars.EndStartNr = iStart.ToString();
					pars.CustomSplitWeight = c.CustomSplitWeight;
					pars.OutputPath = null;
					string valdError = pars.Validate();
					Assert.IsTrue(string.IsNullOrEmpty(valdError), valdError);

					//pars.EndStartNr = (pars.OCourseVm.PermutEstimate + 100).ToString();
					pars.EndStartNr = c.EndNr.ToString();
					pars.Validate();

					cmd.Execute(out string error);

					IEnumerable<OCourse.Route.CostSectionlist> selectedCombs =
						OCourse.Route.CostSectionlist.GetCostSectionLists(pars.OCourseVm.Permutations,
						pars.OCourseVm.RouteCalculator, pars.OCourseVm.LcpConfig.Resolution);

					List<Ocad.Course> selectedCourses = selectedCombs.Select(
						comb => OCourse.Ext.PermutationUtils.GetCourse(c.Course, comb, pars.SplitCourses,
						c.CustomSplitWeight)).ToList();
					coursesDict.Add(c.Course, selectedCourses);

					io3Exporter = io3Exporter ?? OCourse.Iof3.OCourse2Io3.Init(eventName, pars.OCourseVm.CourseFile);
					io3Exporter.AddCurrentPermutations(pars.OCourseVm);
				}


				foreach (var s in new[] { t.Front, t.Back })
				{
					string tmpl = Path.Combine(root, exportFolder, exportCourseFileFct(s));
					File.Copy(Path.Combine(root, s), tmpl, overwrite: true);
					pars.TemplatePath = tmpl;
					pars.OutputPath = tmpl;

					bool first = true;
					foreach (var c in pair.Value)
					{
						pars.Course = c.Course;

						List<Ocad.Course> selectedCourses = coursesDict[c.Course];

						if (first)
						{
							CopyElems(pars.OCourseVm.CourseFile, pars.TemplatePath);
							first = false;
						}

						string courseName = c.Course;

						using (CmdCourseTransfer cmdTrans = new CmdCourseTransfer(pars.OutputPath, pars.TemplatePath ?? pars.OCourseVm.CourseFile, pars.OCourseVm.CourseFile))
						{
							cmdTrans.Export(selectedCourses, courseName);
						}

						//cmd.Execute(out string error);
					}
				}
			}

			io3Exporter.InitMapInfo();
			io3Exporter.Export(io3File);
		}

		private int SplitHD12(int nDouble, Ocad.Control split, Dictionary<string, List<Ocad.Control>> dict, Dictionary<string, List<Ocad.Control>> dictPre)
		{
			if (!dictPre.ContainsKey("89"))
			{ return nDouble + 2; }
			return nDouble;
		}
		private int SplitH45(int nDouble, Ocad.Control split, Dictionary<string, List<Ocad.Control>> dict, Dictionary<string, List<Ocad.Control>> dictPre)
		{
			if (split.Name == "70")
			{ return nDouble + 2; }
			return nDouble;
		}

		private IPoint GetCustomPositionDH12(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (cntrText != "8-63")
			{ return null; }

			return new Point2D(10986, -4069);
		}
		private IPoint GetCustomPositionDAM(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (cntrText.EndsWith("-38"))
			{ return new Point2D(6927 - textWidth, -3968); }

			if (cntrText.EndsWith("-86"))
			{ return new Point2D(7578 - textWidth, -5157); }

			return null;
		}

		private IPoint GetCustomPositionHE(CourseMap cm, string cntrText, double textWidth, IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-93"))
			{ return new Point2D(9099 - textWidth, -7628); }

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
			public int StartNr { get; set; }
			public int EndNr { get; set; }
			public System.Func<int, Ocad.Control, Dictionary<string, List<Ocad.Control>>, Dictionary<string, List<Ocad.Control>>, int> CustomSplitWeight { get; set; }

			public override string ToString()
			{
				return $"{Course}; {StartNr} - {EndNr}";
			}
		}

		private void CopyElems(string source, string target)
		{
			List<int> copySymbols = new List<int> { 708000, 709000, 709001, 709002, 709003, 709004 };
			using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(target))
			using (Ocad.OcadReader r = Ocad.OcadReader.Open(source))
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
		public void T02_AdaptCourses()
		{
			//			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\Karten";
			string root = Path.Combine(_root, _exportFolder);
			foreach (var mapFile in Directory.GetFiles(root))
			{
				IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
				if (parts.Count != 5 || parts[4] != "ocd")
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
					//if (parts[1].EndsWith("HE"))
					//{ cmd.GetCustomPosition = GetCustomPositionHE; }
					//if (parts[1].EndsWith("HAL"))
					//{ cmd.GetCustomPosition = GetCustomPositionHAL; }
					//if (parts[1].EndsWith("H45"))
					//{ cmd.GetCustomPosition = GetCustomPositionH45; }
					//if (parts[1].EndsWith("D20"))
					//{ cmd.GetCustomPosition = GetCustomPositionD20; }
					//if (parts[1].EndsWith("12"))
					//{ cmd.GetCustomPosition = GetCustomPositionDH12; }
					//if (parts[1].EndsWith("D14") || parts[1].EndsWith("DAM") || parts[1].EndsWith("D60"))
					//{ cmd.GetCustomPosition = GetCustomPositionDAM; }

					cmd.Execute();

					if (parts[3] == "1")
					{
						AdaptCourseName(w, cm, grpFont, font);
					}
				}
			}
		}

		[TestMethod]
		public void T03_ExportCourses()
		{
			List<string> exports = new List<string>();
			//			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\Karten";
			string root = Path.Combine(_root, _exportFolder);
			foreach (var mapFile in Directory.GetFiles(root))
			{
				IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
				if (parts.Count != 5)
				{ continue; }
				if (parts[4] != "ocd")
				{ continue; }

				if (!parts[0].EndsWith("_V") || parts[3] != "1")
				{ continue; }

				string cat = parts[1];
				string nr = parts[2];

				string back = parts[0].Replace("_V", "_H");
				string backFile = Path.Combine(root, $"{back}.{parts[1]}.{parts[2]}.2.ocd");
				if (!File.Exists(backFile))
				{ throw new InvalidOperationException($"{backFile} not found"); }
				exports.Add(mapFile);
				exports.Add(backFile);
			}

			Ocad.Scripting.Utils.Optimize(exports, Path.Combine($"{root}", "optimize.xml"));

			string scriptFile = Path.Combine($"{root}", "createPdfs.xml");
			string defaultExe;
			using (Ocad.Scripting.Script s = new Ocad.Scripting.Script(scriptFile))
			{
				ExportBackgrounds(s, exports);

				defaultExe = CreatePdfScript(exports, s);
			}
			new Ocad.Scripting.Utils { Exe = defaultExe }.RunScript(scriptFile, defaultExe);
		}

		private void ExportBackgrounds(Ocad.Scripting.Script script, List<string> exports)
		{
			Dictionary<string, string> bgs = new Dictionary<string, string>();
			foreach (var exp in exports)
			{
				string bg = Path.GetFileName(exp).Split('.')[0];
				bgs[bg] = exp;
			}
			bool first = true;
			foreach (var pair in bgs)
			{
				string bg = pair.Value;
				string bgFileName = Path.Combine(Path.GetDirectoryName(bg), $"{pair.Key}_bg.ocd");
				File.Copy(bg, bgFileName, overwrite: true);
				using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(bgFileName))
				{
					w.DeleteElements((i) => true);
				}

				string pdfPath = PrepPdf(bgFileName);
				using (Ocad.Scripting.Node node = script.FileOpen())
				{ node.File(bgFileName); }
				using (Ocad.Scripting.Node node = script.MapOptimize())
				{ node.Enabled(true); }
				using (Ocad.Scripting.Node node = script.FileExport())
				{
					node.File(pdfPath);
					node.Format(Ocad.Scripting.Format.Pdf);
				}
				using (Ocad.Scripting.Node node = script.FileSave())
				{ node.Enabled(true); }

				if (!first)
				{
					using (Ocad.Scripting.Node node = script.FileClose())
					{ node.Enabled(true); }
				}
				first = false;
			}
		}

		[TestMethod]
		public void T04_JoinPdfs()
		{
			List<PdfId> exports = new List<PdfId>();
			string root = Path.Combine(_root, _exportFolder);
			foreach (var mapFile in Directory.GetFiles(root))
			{
				IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
				if (parts.Count != 5)
				{ continue; }
				if (parts[4] != "pdf")
				{ continue; }

				PdfId p = new PdfId { Bg = parts[0], Cat = parts[1], Nr = int.Parse(parts[2]), Side = parts[3], File = mapFile };
				exports.Add(p);
			}
			exports.Sort((x, y) =>
			{
				int d = x.Cat.CompareTo(y.Cat);
				if (d != 0) return d;
				d = x.Nr.CompareTo(y.Nr);
				if (d != 0) return d;
				d = x.Side.CompareTo(y.Side);
				return d;
			});

			Dictionary<string, List<PdfId>> catFiles = new Dictionary<string, List<PdfId>>();
			foreach (var export in exports)
			{
				catFiles.GetOrCreateValue(export.Cat).Add(export);
			}

			foreach (var pair in catFiles)
			{
				string cat = pair.Key;

				List<string> files = pair.Value.Select(x => x.File).ToList();

				string front = Path.Combine(root, $"{pair.Value[0].Bg}_bg.pdf");
				string rueck = Path.Combine(root, $"{pair.Value[1].Bg}_bg.pdf");

				OTextSharp.Models.PdfText.OverprintFrontBack(
					Path.Combine(root, $"Comb_{cat}.pdf"), files, front, rueck);
			}
		}
		private class PdfId
		{
			public string Bg { get; set; }
			public string Cat { get; set; }
			public int Nr { get; set; }
			public string Side { get; set; }
			public string File { get; set; }
		}
		private static string PrepPdf(string ocdFile)
		{
			string pdfFile = Path.GetFileNameWithoutExtension(ocdFile) + ".pdf";
			string dir = Path.GetDirectoryName(ocdFile);
			string pdfPath = Path.Combine(dir, pdfFile);
			if (File.Exists(pdfPath))
			{ File.Delete(pdfPath); }

			return pdfPath;
		}

		private static string CreatePdfScript(IEnumerable<string> files, Ocad.Scripting.Script expPdf)
		{
			string exe = null;
			foreach (var ocdFile in files)
			{
				if (exe == null)
				{
					exe = Utils.FindExecutable(ocdFile);
				}
				string ocdTmpl = ocdFile.Replace(".1.ocd", ".Front.ocd").Replace(".2.ocd", ".Rueck.ocd");
				string pdfPath = PrepPdf(ocdTmpl);

				using (Ocad.Scripting.Node node = expPdf.FileOpen())
				{ node.File(ocdFile); }
				using (Ocad.Scripting.Node node = expPdf.BackgroundMapRemove())
				{ }
				using (Ocad.Scripting.Node node = expPdf.FileExport())
				{
					node.File(pdfPath);
					node.Format(Ocad.Scripting.Format.Pdf);
					//node.Child("ExportScale","10000");
					//node.Child("Colors", "normal");
					//using (Ocad.Scripting.Node ext = node.Child("PartOfMap"))
					//{
					//	ext.Enabled(true);
					//	ext.Child("Coordinates", "mm");
					//	ext.Child("L", "0");
					//	ext.Child("R", "100");
					//	ext.Child("B", "0");
					//	ext.Child("T", "100");
					//}
				}
				//using (Ocad.Scripting.Node node = expPdf.FileSave())
				//{ node.Enabled(true); }
				using (Ocad.Scripting.Node node = expPdf.FileClose())
				{ node.Enabled(true); }
			}
			if (exe != null)
			{
				using (Ocad.Scripting.Node node = expPdf.FileExit())
				{
					node.Enabled(true);
				}
			}
			return exe;
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
