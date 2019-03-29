using System;
using System.Collections.Generic;

namespace ToDos.Shared
{
  public class ToDo
  {
    public string Name { get; set; }
    public TimeSpan Repeat { get; set; }

    public List<DateTime> CompleteTimes { get; set; }
  }
}