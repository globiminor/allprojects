using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCourse.Commands;

namespace OcadTest.OEvent
{
	[TestClass]
	public class Ruemlang2021
	{
		[TestMethod]
		public void TestLayout()
		{
			var cmd = new CmdCourseVerifyLayout();
			cmd.InitFromFile(@"C:\daten\felix\kapreolo\karten\ruemlangerwald\2021\test\Bahnen_1_1.Bahn1.5.1.ocd");
			cmd.Execute();
		}
	}
}
