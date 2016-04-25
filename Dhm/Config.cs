using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhm
{
  public class Config
  {
    public Info Info { get; set; }
    public List<ContourSetting> Contour {get; set; }
    public List<FallDirSetting> FallDir { get; set; }
  }

  public class Info
  {
    public int Aequidistance { get; set; }
    public int Intervall { get; set; }
    public int HMin { get; set; }
    public int HMax { get; set; }
  }
  public class ContourSetting
  {
    public int Symbol { get; set; }
    public int Intervall { get; set; }
    public int Offset { get; set; }
  }

  public class FallDirSetting
  {
    public int Symbol { get; set; }
  }
}
