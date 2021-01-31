using OCourse.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OCourse.Cmd.Commands
{
	public class PlaceCommand : Basics.Cmd.ICommand
  {
    private readonly PlaceParameters _pars;
    public PlaceCommand(PlaceParameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {
        foreach (var mapFile in Directory.EnumerateFiles(Path.GetDirectoryName(_pars.CoursePath), Path.GetFileName(_pars.CoursePath)))
				{
          CourseMap cm = new CourseMap();
          cm.InitFromFile(mapFile);
          cm.ControlNrOverprintSymbol = _pars.ControlNrOverprintSymbol;

          string ext = Path.GetExtension(mapFile);
          string dir = Path.GetDirectoryName(mapFile);
          string name = Path.GetFileNameWithoutExtension(mapFile);
          string result = Path.Combine(dir, $"{name}.p{ext}");

          File.Copy(mapFile, result, overwrite: true);

          using (Ocad.OcadWriter w = Ocad.OcadWriter.AppendTo(result))
          {
            CmdCoursePlaceControlNrs cmd = new CmdCoursePlaceControlNrs(w, cm);
            cmd.Execute();
          }
        }
      }
      finally
      {
        _pars.Dispose();
      }
      error = null;
      return true;
    }
  }
}
