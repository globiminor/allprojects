using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Commands;
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
			string mapFile = @"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.1.ocd";
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

	}
}
