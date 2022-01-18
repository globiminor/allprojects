using Basics;
using Basics.Geom;
using Macro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Cmd.Commands;
using OCourse.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;
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
		private string _allVarsName = @"allVars\allVars.ocd";

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
				new Conf("D14", a4_10k),
				new Conf("D16", a3_10k),
				new Conf("D18", a3_10k),
				new Conf("D20", a3_10k),
				new Conf("DE",  a3_10k),
				new Conf("DAL", a3_10k),
				new Conf("DAM", a3_10k),
				new Conf("DAK", a3_10k),
				new Conf("DB",  a3_10k),
				new Conf("D35", a3_10k),
				new Conf("D40", a3_10k),
				new Conf("D45", a3_10k),
				new Conf("D50", a3_10k),
				new Conf("D55", a3_10k),
				new Conf("D60", a3_10k),
				new Conf("D65", a3_10k),
				new Conf("D70", a4_10k),
				new Conf("D75", a4_10k),

				new Conf("H14", a3_10k),
				new Conf("H16", a3_10k),
				new Conf("H18", a3_10k),
				new Conf("H20", a3_10k),
				new Conf("HE",  a3_10k),
				new Conf("HAL", a3_10k),
				new Conf("HAM", a3_10k),
				new Conf("HAK", a3_10k),
				new Conf("HB",  a3_10k),
				new Conf("H35", a3_10k),
				new Conf("H40", a3_10k),
				new Conf("H45", a3_10k),
				new Conf("H50", a3_10k),
				new Conf("H55", a3_10k),
				new Conf("H60", a3_10k),
				new Conf("H65", a3_10k),
				new Conf("H70", a3_10k),
				new Conf("H75", a3_10k),
				new Conf("H80", a3_10k),
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

			string allVarsPath = Path.Combine(root, exportFolder, _allVarsName);
			File.Copy(Path.Combine(root, a3_10k.Front), allVarsPath, overwrite: true);

			pars.EnsureOCourseVm();
			using (var r = Ocad.OcadReader.Open(pars.OCourseVm.CourseFile))
			{
				foreach (var pStrIdx in r.ReadStringParamIndices())
				{
					if (pStrIdx.Type == Ocad.StringParams.StringType.Template)
					{
						string sTemp = r.ReadStringParam(pStrIdx);
						Ocad.StringParams.TemplatePar tplPar = new Ocad.StringParams.TemplatePar(sTemp);
						if (!tplPar.Visible)
						{ continue; }
						string tpl = tplPar.Name;
						string courseDir = Path.GetDirectoryName(pars.OCourseVm.CourseFile);
						if (!Path.IsPathRooted(tpl) ||
							Path.GetDirectoryName(tpl).Equals(courseDir, StringComparison.OrdinalIgnoreCase))
						{
							string sourcePath = Path.IsPathRooted(tpl) ? tpl : Path.Combine(courseDir, tpl);
							string sourceName = Path.GetFileName(sourcePath);

							File.Copy(sourcePath, Path.Combine(root, exportFolder, sourceName), overwrite: true);
							File.Copy(sourcePath, Path.Combine(Path.GetDirectoryName(allVarsPath), sourceName), overwrite: true);
						}
					}
				}
			}
			CopyElems(pars.OCourseVm.CourseFile, allVarsPath);

			foreach (var pair in tmplDict)
			{
				Tmpl t = pair.Key;

				Dictionary<string, List<Ocad.Course>> coursesDict
					= new Dictionary<string, List<Ocad.Course>>();
				Dictionary<string, List<Ocad.Course>> variationsDict
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


					IEnumerable<OCourse.Route.CostSectionlist> allCombs =
						OCourse.Route.CostSectionlist.GetCostSectionLists(GetAllPermutations(pars.OCourseVm.Info),
						pars.OCourseVm.RouteCalculator, pars.OCourseVm.LcpConfig.Resolution);
					List<Ocad.Course> allVariations = allCombs.Select(
						comb => OCourse.Ext.PermutationUtils.GetCourse(c.Course, comb, pars.SplitCourses,
						c.CustomSplitWeight)).ToList();
					variationsDict.Add(c.Course, allVariations);

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

				foreach (var c in pair.Value)
				{
					List<Ocad.Course> selectedCourses = variationsDict[c.Course];
					string courseName = c.Course;
					using (CmdCourseTransfer cmdTrans = new CmdCourseTransfer(allVarsPath, allVarsPath ?? pars.OCourseVm.CourseFile, pars.OCourseVm.CourseFile))
					{
						cmdTrans.Export(selectedCourses, courseName);
					}
				}
			}

			io3Exporter.InitMapInfo();
			io3Exporter.Export(io3File);
		}

		private List<OCourse.ViewModels.PermutationVm> GetAllPermutations(IEnumerable<OCourse.Route.ICost> costs)
		{
			List<OCourse.ViewModels.PermutationVm> permuts = new List<OCourse.ViewModels.PermutationVm>();
			int idx = 0;
			int start = 1;
			foreach (var cost in costs)
			{
				if (!(cost is OCourse.Route.CostSectionlist costSections)) continue;

				OCourse.ViewModels.PermutationVm permut =
					new OCourse.ViewModels.PermutationVm(costSections.Sections) { Index = idx, StartNr = start };

				permuts.Add(permut);

				idx++;
				start++;
			}
			return permuts;
		}

		[TestMethod]
		public void T02_ExportCourseMaps()
		{
			string rootFolder = Path.Combine(_root, _exportFolder);
			foreach (var mapFile in Directory.GetFiles(rootFolder))
			{
				IList<string> parts = Path.GetFileName(mapFile).Split(new[] { '.' });
				if (parts.Count != 2)
				{ continue; }
				if (parts[1] != "ocd")
				{ continue; }

				if (!(parts[0].EndsWith("_V") || parts[0].EndsWith("_H")))
				{ continue; }

				ExportCourseMaps(mapFile);
			}
		}
		[TestMethod]
		public void T02_Z_ExportAllVars()
		{
			string allVarsPath = Path.Combine(_root, _exportFolder, _allVarsName);
			ExportCourseMaps(allVarsPath);
		}

		private void ExportCourseMaps(string mapFile)
		{
			string rootFolder = Path.GetDirectoryName(mapFile);

			string mapName = Path.GetFileNameWithoutExtension(mapFile);
			string lastCourse = null;
			using (Ocad.OcadReader r = Ocad.OcadReader.Open(mapFile))
			{
				foreach (var course in r.ReadCourses())
				{
					lastCourse = course.Name;
					foreach (var filePath in Directory.GetFiles(rootFolder, $"{mapName}.{lastCourse}.*"))
					{
						if (filePath.EndsWith(".ocd"))
						{ File.Delete(filePath); }
					}
				}
			}
			string exe = Utils.FindExecutable(mapFile);
			string script = Path.Combine(rootFolder, "exportCourse.xml");
			using (Ocad.Scripting.Script expPdf = new Ocad.Scripting.Script(script))
			{
				using (Ocad.Scripting.Node node = expPdf.FileOpen())
				{ node.File(mapFile); }
			}
			string pName = new Ocad.Scripting.Utils().RunScript(script, exe, wait: 3000);
			IList<Process> processes = Process.GetProcessesByName(pName);

			if (processes.Count < 1)
			{ return; }
			Processor m = new Processor();
			m.SetForegroundProcess(processes[0]);
			m.SetForegroundWindow("OCAD", out string fullText);
			System.Threading.Thread.Sleep(2000);

			// Export Course maps...
			m.SendCommand('c', new List<byte> { Ui.VK_ALT });
			m.SendCommand('x');
			m.SendCommand('x');

			System.Threading.Thread.Sleep(1000);

			//IntPtr p = Ui.GetFocus();
			//var root = AutomationElement.FromHandle(processes[0].MainWindowHandle);

			// Click "Select All"
			SetFocusByName("Select all", m);
			m.SendCode(new[] { Ui.VK_RETURN });
			System.Threading.Thread.Sleep(500);

			// Click "Select OK"
			SetFocusByName("OK", m);
			m.SendCode(new[] { Ui.VK_RETURN });
			System.Threading.Thread.Sleep(500);

			// "Save" exported course maps
			m.SendCommand('s', new[] { Ui.VK_ALT });
			System.Threading.Thread.Sleep(500);

			while (!(Directory.GetFiles(rootFolder, $"{mapName}.{lastCourse}.*")?.Length > 0))
			{
				System.Threading.Thread.Sleep(500);
			}
			System.Threading.Thread.Sleep(1000);
			m.SetForegroundWindow("OCAD", out fullText);
			System.Threading.Thread.Sleep(1000);


			// Close map
			m.SendCommand('w', new[] { Ui.VK_CONTROL });
			System.Threading.Thread.Sleep(500);
			// Check for "Save File ...",
			if (AutomationElement.FocusedElement.Current.ControlType.ProgrammaticName == "ControlType.Button")
			{
				// Do not save file
				m.SendCommand('n', new[] { Ui.VK_ALT });
				System.Threading.Thread.Sleep(500);
			}
			var c = AutomationElement.FocusedElement.Current;
		}

		private void SetFocusByName(string name, Processor m)
		{
			var parent = TreeWalker.ContentViewWalker.GetParent(AutomationElement.FocusedElement);
			var toFocus = parent.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, name));
			toFocus?.SetFocus();
			System.Threading.Thread.Sleep(500);
			// Click "Select All"
			while (!AutomationElement.FocusedElement.Current.Name.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				m.SendCommand('\t');
				System.Threading.Thread.Sleep(500);
			}

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

		private IPoint GetCustomPositionD20(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-73"))
			{ return new Point2D(14032 - textWidth, 4642); }

			return null;
		}

		private IPoint GetCustomPositionD35(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-60"))
			{ return new Point2D(15014, 5213); }

			return null;
		}

		private IPoint GetCustomPositionD40(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-60"))
			{
				foreach (var elem in cm.ConnectionElems)
				{
					if (string.IsNullOrWhiteSpace(elem.ObjectString))
					{ continue; }
					Ocad.StringParams.ControlPar par = new Ocad.StringParams.ControlPar(elem.ObjectString);
					string f = par.GetString('f');
					if (f != "69")
					{ continue; }
					string t = par.GetString('t');
					if (t != "52")
					{ continue; }

					Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)elem.GetMapGeometry();
					Polyline b = new Polyline();
					b.Add(line.BaseGeometry.Points[0]);
					b.Add(new Ocad.Coord.CodePoint(17148, 5076) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
					b.Add(new Point2D(16794, 4378));
					b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

					Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
					cm.CustomGeometries.Add(elem, l1);
				}
				return new Point2D(16166, 4539);
			}

			return null;
		}

		private IPoint GetCustomPositionD70(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-66"))
			{ return new Point2D(6942, 7819); }

			return null;
		}

		private IPoint GetCustomPositionDAL(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-73"))
			{
				if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt73") == true) != null
					&& cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null)
				{
					return new Point2D(13099, 5462);
				}
			}

			if (cntrText.EndsWith("-60"))
			{
				if (//int.Parse(cntrText.Split('-')[0]) < 10 
					cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null
					&& cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("75\tt46") == true) is Ocad.MapElement eTo46)
				{
					//					return new Point2D(15222, 5090);

					Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)eTo46.GetMapGeometry();
					Polyline b = new Polyline();
					b.Add(line.BaseGeometry.Points[0]);
					b.Add(new Ocad.Coord.CodePoint(16388, 4401) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
					b.Add(new Point2D(15604, 3937));
					b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

					Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
					cm.CustomGeometries.Add(eTo46, l1);

					return new Point2D(15547, 3938);
				}
			}


			return null;
		}

		private IPoint GetCustomPositionH16(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			// as DAL
			if (cntrText.EndsWith("-73"))
			{
				if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt73") == true) != null
					&& cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null)
				{
					return new Point2D(13099, 5462);
				}
			}

			return null;
		}

		private IPoint GetCustomPositionH20(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-71"))
			{
				if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("47\tt39") == true) != null)
				{
					return new Point2D(13848, 4068);
				}
				else
				{
					return new Point2D(13427, 4087);
				}
			}

			return null;
		}

		private IPoint GetCustomPositionH70(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-80"))
			{
				if (//int.Parse(cntrText.Split('-')[0]) < 10 
					cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt80") == true) != null
					&& cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("80\tt31") == true) != null
					&& cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("31\tt38") == true) is Ocad.MapElement eTo38)
				{
					//					return new Point2D(15222, 5090);

					Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)eTo38.GetMapGeometry();
					Polyline b = new Polyline();
					b.Add(line.BaseGeometry.Points[0]);
					b.Add(new Ocad.Coord.CodePoint(13207, 4920) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
					b.Add(new Point2D(13195,  4284));
					b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

					Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
					cm.CustomGeometries.Add(eTo38, l1);

					return new Point2D(11782, 4377);
				}
			}
			return null;
		}


		private IPoint GetCustomPositionH80(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-66"))
			{ return new Point2D(7370 - textWidth, 6565); }

			return null;
		}

		private IPoint GetCustomPositionHAM(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			// as DAL
			if (cntrText.EndsWith("-73"))
			{
				if (cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("77\tt73") == true) != null
					&& cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("73\tt32") == true) != null)
				{
					return new Point2D(13099, 5462);
				}
			}

			return null;
		}

		private IPoint GetCustomPositionHAK(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			// see H80
			if (cntrText.EndsWith("-66"))
			{ return new Point2D(7370 - textWidth, 6565); }

			return null;
		}

		private IPoint GetCustomPositionHE(CourseMap cm, string cntrText, double textWidth,
			IReadOnlyList<Polyline> freeParts)
		{
			if (freeParts.Count > 0)
			{ return null; }

			if (cntrText.EndsWith("-82"))
			{
				if (//int.Parse(cntrText.Split('-')[0]) < 10 
					cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("50\tt43") == true) is Ocad.MapElement e50To43)
				{
					//					return new Point2D(15222, 5090);

					Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)e50To43.GetMapGeometry();
					Polyline b = new Polyline();
					b.Add(line.BaseGeometry.Points[0]);
					b.Add(new Ocad.Coord.CodePoint(20865, -1920) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
					b.Add(new Point2D(21024, -2545));
					b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

					Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
					cm.CustomGeometries.Add(e50To43, l1);

					return new Point2D(20289, -2454);
				}
				if (//int.Parse(cntrText.Split('-')[0]) < 10 
					cm.ConnectionElems.FirstOrDefault(x => x.ObjectString?.EndsWith("34\tt43") == true) is Ocad.MapElement e34To43)
				{
					//					return new Point2D(15222, 5090);

					Ocad.GeoElement.Line line = (Ocad.GeoElement.Line)e34To43.GetMapGeometry();
					Polyline b = new Polyline();
					b.Add(line.BaseGeometry.Points[0]);
					b.Add(new Ocad.Coord.CodePoint(21455, -1988) { Flags = Ocad.Coord.Flags.noLine | Ocad.Coord.Flags.noLeftLine | Ocad.Coord.Flags.noRightLine });
					b.Add(new Point2D(21343, -2514));
					b.Add(line.BaseGeometry.Points[line.BaseGeometry.Points.Count - 1]);

					Ocad.GeoElement.Line l1 = new Ocad.GeoElement.Line(b);
					cm.CustomGeometries.Add(e34To43, l1);

					return new Point2D(20289, -2454);
				}
			}
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
		public void T03_AdaptCourses()
		{
			AdaptCourses(Path.Combine(_root, _exportFolder));
		}

		[TestMethod]
		public void T03_Z_AdaptAllVars()
		{
			AdaptCourses(Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName)));
		}

		private void AdaptCourses(string root)
		{
			foreach (var mapFile in Directory.GetFiles(root))
			{
				AdaptCourse(mapFile);
			}
		}
		private void AdaptCourse(string mapFile)
		{
			string mapName = Path.GetFileName(mapFile);
			IList<string> parts = mapName.Split(new[] { '.' });
			if (parts.Count != 5 || parts[4] != "ocd")
			{ return; }

			CourseMap cm = new CourseMap();
			cm.ControlNrOverprintSymbol = 704005;
			cm.ControlNrSymbols.Add(cm.ControlNrOverprintSymbol);
			cm.InitFromFile(mapFile);

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
				if (parts[1].EndsWith("D20"))
				{ cmd.GetCustomPosition = GetCustomPositionD20; }
				if (parts[1].EndsWith("D35"))
				{ cmd.GetCustomPosition = GetCustomPositionD35; }
				if (parts[1].EndsWith("D40"))
				{ cmd.GetCustomPosition = GetCustomPositionD40; }
				if (parts[1].EndsWith("D70"))
				{ cmd.GetCustomPosition = GetCustomPositionD70; }
				if (parts[1].EndsWith("DAL"))
				{ cmd.GetCustomPosition = GetCustomPositionDAL; }
				if (parts[1].EndsWith("H16"))
				{ cmd.GetCustomPosition = GetCustomPositionH16; }
				if (parts[1].EndsWith("H20"))
				{ cmd.GetCustomPosition = GetCustomPositionH20; }
				if (parts[1].EndsWith("H70"))
				{ cmd.GetCustomPosition = GetCustomPositionH70; }
				if (parts[1].EndsWith("H80"))
				{ cmd.GetCustomPosition = GetCustomPositionH80; }
				if (parts[1].EndsWith("HAK"))
				{ cmd.GetCustomPosition = GetCustomPositionHAK; }
				if (parts[1].EndsWith("HAM"))
				{ cmd.GetCustomPosition = GetCustomPositionHAM; }
				if (parts[1].EndsWith("HE"))
				{ cmd.GetCustomPosition = GetCustomPositionHE; }

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

			if (cm.UnplacedControls.Count > 0)
			{ }

		}

		[TestMethod]
		public void T04_ExportCourses()
		{
			//			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\Karten";
			string root = Path.Combine(_root, _exportFolder);
			ExportCourses(root);
		}

		[TestMethod]
		public void T04_Z_ExportCourses()
		{
			//			string root = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\Karten";
			string root = Path.GetDirectoryName(Path.Combine(_root, _exportFolder, _allVarsName));
			ExportCourses(root);
		}

		private static void ExportCourses(string root)
		{ 
			List<string> exports = new List<string>();
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

		private static void ExportBackgrounds(Ocad.Scripting.Script script, List<string> exports)
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
		public void T05_JoinPdfs()
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
