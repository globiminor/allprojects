
using OCourse.Commands;
using OCourse.Route;
using System.Collections.Generic;
using System.Linq;

namespace OCourse.Cmd.Commands
{
  public class BuildCommand : Basics.Cmd.ICommand
  {
    private readonly BuildParameters _pars;
    public BuildCommand(BuildParameters pars)
    {
      _pars = pars;
    }

    public bool Execute(out string error)
    {
      try
      {
        var vm = _pars.OCourseVm;
        while (vm.Working)
        { System.Threading.Thread.Sleep(100); }

        vm.PermutationsInit();
        while (vm.Working)
        { System.Threading.Thread.Sleep(100); }
        //        _pars.OCourseVm.

        IEnumerable<CostSectionlist> selectedCombs =
          CostSectionlist.GetCostSectionLists(vm.Permutations, vm.RouteCalculator, vm.LcpConfig.Resolution);

        string courseName = Ext.PermutationUtils.GetCoreCourseName(vm.Course.Name);

        using (CmdCourseTransfer cmd = new CmdCourseTransfer(_pars.OutputPath, _pars.TemplatePath ?? vm.CourseFile, vm.CourseFile))
        {
          cmd.Export(selectedCombs.Select(comb => Ext.PermutationUtils.GetCourse(courseName, comb, _pars.SplitCourses)), courseName);
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
