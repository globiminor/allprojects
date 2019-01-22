using System.Collections.Generic;

namespace Basics.Geom
{
  public interface IDimension
  {
    int Dimension { get; }
  }
  public interface IGeometry : IDimension
  {
    int Topology { get; }
    bool IsWithin(IPoint point);
    IBox Extent { get; }
    IGeometry Border { get; }
    IGeometry Project(IProjection projection);

    bool EqualGeometry(IGeometry other);
  }

  public interface IBox : IGeometry
  {
    IPoint Min { get; }
    IPoint Max { get; }
    IBox Clone();
    Relation RelationTo(IBox box);
    bool Contains(IBox box);
    bool Contains(IBox box, IEnumerable<int> dimensionList);
    //bool IsWithin(IBox box);
    //bool IsWithin(IBox box, IList<int> calcDimensions);
    //bool Intersects(IBox box, IList<int> calcDimensions);
    bool Intersects(IGeometry geometry);
    double GetMaxExtent();
  }

  public interface IVolume
  {
    IReadOnlyList<ISignedArea> Planes { get; }
  }
  public interface ISignedArea
  {
    int Sign { get; }
    ISimpleArea Area { get; }
  }
  public interface ISimpleArea
  {
    /// <summary>
    /// Last point == first point
    /// </summary>
    IReadOnlyList<IPoint> Border { get; }
  }

  public interface IMultipartGeometry : IGeometry
  {
    bool IsContinuous { get; }
    bool HasSubparts { get; }
    IEnumerable<IGeometry> Subparts();
  }

  public interface IIndexGeometry
  {

  }

  public interface IPoint : IGeometry
  {
    new IPoint Project(IProjection prj);
    double X { get; }
    double Y { get; }
    double Z { get; }
    double this[int index] { get; }
  }

  public enum ParamRelate { Unknown, Intersect, Near, Disjoint }

  public interface IParamGeometry : IGeometry
  {
    IPoint PointAt(IPoint parameters);
    IBox ParameterRange { get; }
  }

  public interface IRelationGeometry
  {
    ParamRelate Relation(IBox paramBox);

    IEnumerable<ParamGeometryRelation> CreateRelations(IParamGeometry other, TrackOperatorProgress trackProgress);

    bool IsLinear { get; }

    /// <summary>
    /// Normed maximum offset of the (linear) line[P0,P1] from the curve[Parameters[t]], 
    /// where:<para/>
    ///   curve[Parameters[0]] = P0 <para/>
    ///   curve[Parameters[1]] = P1 <para/>
    ///   Parameters[t] = Parameters[0] + t * (Parameters[1] - Parameters[0]) <para/>
    /// <para/>
    /// Normed means that MaxOffset is valid if Distance(P0-P1)==1 <para/>
    /// if distance from P0-P1 == f, then the maximum offset from P0-P1 = (f * NormedMaxOffset)^2 <para/>
    /// <para/>
    /// This relation must be valid in the entire 'ParameterRange'.
    /// Otherwise, the implementation can lead to errors
    /// </summary>
    double NormedMaxOffset { get; }
  }

  public interface ITangentGeometry
  {
    IList<IPoint> TangentAt(IPoint parameters);
  }

  public interface IRelParamGeometry : IParamGeometry, IRelationGeometry
  { }

  public interface ITanParamGeometry : IParamGeometry, ITangentGeometry
  { }

  public interface IRelTanParamGeometry : IParamGeometry, ITangentGeometry, IRelationGeometry, IRelParamGeometry, ITanParamGeometry
  { }

  public interface IProjection
  {
    IPoint Project(IPoint point);
  }
}
