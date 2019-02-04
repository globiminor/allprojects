using System.Collections.Generic;

namespace Basics.Geom
{
  public interface IDimension
  {
    int Dimension { get; }
  }
  public interface ITopology
  {
    int Topology { get; }
  }
  public interface IGeometry : IDimension, ITopology
  {
    bool IsWithin(IPoint point);
    IBox Extent { get; }
    IGeometry Border { get; }
    IGeometry Project(IProjection projection);

    bool EqualGeometry(IGeometry other);
  }

  public interface IBox : IDimension, ITopology
  {
    IPoint Min { get; }
    IPoint Max { get; }
  }

  public interface ILine : IDimension
  {
    IPoint Start { get; }
    IPoint End { get; }
  }
  public interface IArc
  {
    IPoint Center { get; }
    double Radius { get; }
    double DirStart { get; }
    double Angle { get; }
  }
  public interface IBezier : IDimension
  {
    IPoint Start { get; }
    IPoint P1 { get; }
    IPoint P2 { get; }
    IPoint End { get; }
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

  public interface ISimplePolyline
  {
    IReadOnlyList<IPoint> Points { get; }
  }
  public interface ISimpleArea
  {
    /// <summary>
    /// Foreach border: Last point == first point
    /// </summary>
    IReadOnlyList<ISimplePolyline> Border { get; }
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

  public interface IPoint : IDimension
  {
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
