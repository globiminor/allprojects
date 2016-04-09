using System;

namespace Basics.Geom
{
  public delegate void ParamRelationHandler(object sender, ParamRelationCancelArgs args);
  public class ParamRelationCancelArgs : EventArgs
  {
    private readonly ParamGeometryRelation _relation;
    private bool _cancel;

    public ParamRelationCancelArgs(ParamGeometryRelation relation)
    {
      _relation = relation;
    }
    public ParamGeometryRelation Relation
    {
      get { return _relation; }
    }
    public bool Cancel
    {
      get { return _cancel; }
      set { _cancel = value; }
    }
  }

  public class TrackOperatorProgress
  {
    public event ParamRelationHandler RelationFound;

    private bool _cancel;
    public void OnRelationFound(object sender, ParamGeometryRelation relation)
    {
      if (RelationFound != null)
      {
        ParamRelationCancelArgs args = new ParamRelationCancelArgs(relation);
        RelationFound(sender, args);

        _cancel = _cancel || args.Cancel;
      }
    }

    public bool Cancel
    {
      get { return _cancel; }
    }
  }
}
