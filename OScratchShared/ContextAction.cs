using System.Collections.Generic;

namespace OMapScratch
{
  public class ContextActions
  {
    public ContextActions(string name, Elem elem, Pnt position, List<ContextAction> actions)
    {
      Name = name;
      Elem = elem;
      Position = position;
      Actions = actions;
    }
    public string Name { get; set; }
    public Elem Elem { get; }
    public Pnt Position { get; }
    public List<ContextAction> Actions { get; }
  }
  public class ContextAction
  {
    public ContextAction(Pnt position, IAction action)
    {
      Position = position;
      Action = action;
    }
    public Pnt Position { get; }
    public string Name { get; set; }
    public IAction Action { get; set; }

    public void Execute()
    { Action?.Action(); }
  }
}