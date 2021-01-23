using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Basics.Geom
{
	public class PointComparer : IComparer<IPoint>, IEqualityComparer<IPoint>
	{
		public bool Equals(IPoint x, IPoint y)
		{
			return Compare(x, y) == 0;
		}
		public int GetHashCode(IPoint o)
		{
			return o.X.GetHashCode() ^ o.Y.GetHashCode();
		}
		public int Compare(IPoint x, IPoint y)
		{
			if (x == y)
			{ return 0; }
			int nDim = x.Dimension;
			for (int iDim = 0; iDim < nDim; iDim++)
			{
				int i = x[iDim].CompareTo(y[iDim]);
				if (i != 0)
				{ return i; }
			}

			return 0;
		}
	}

	public abstract class PointCore : Geometry, IPoint, IBox
	{
		public double X { get => GetX(); }
		public double Y { get => GetY(); }
		public double Z { get => GetZ(); }
		public double this[int index] { get => GetCoord(index); }

		protected abstract double GetX();
		protected abstract double GetY();
		protected abstract double GetZ();
		protected abstract double GetCoord(int index);

		IPoint IBox.Min
		{
			get { return this; }
		}
		IPoint IBox.Max
		{
			get { return this; }
		}

		public IGeometry Border
		{ get { return null; } }
		protected override IGeometry BorderGeom
		{ get { return null; } }

		/// <summary>
		/// Vector product
		/// </summary>
		public double VectorProduct(IPoint p1) => PointOp.VectorProduct(this, p1);

		public Point3D VectorProduct3D(IPoint p1) => VectorProduct3D(this, p1);
		public static Point3D VectorProduct3D(IPoint p0, IPoint p1)
		{
			if (p1 == null)
			{ return null; }

			Point3D p = new Point3D
			{
				X = p0.Y * p1.Z - p1.Z * p1.Y,
				Y = p0.Z * p1.X - p1.X * p1.Z,
				Z = p0.X * p1.Y - p1.Y * p1.X
			};
			return p;
		}

		protected sealed override IBox GetExtent() => this;

		protected override IGeometry ProjectCore(IProjection projection) => Project(projection);
		public Point Project(IProjection projection)
		{
			IPoint p = PointOp.Project(this, projection);
			return Point.CastOrCreate(p);
		}

		public Point Clone()
		{
			Point p = new Point(Dimension);
			for (int i = 0; i < Dimension; i++)
			{ p[i] = GetCoord(i); }
			return p;
		}
		public override int Topology
		{ get { return 0; } }

		public double Dist2(IPoint point, IEnumerable<int> dimensions = null)
		{
			return PointOp.Dist2(this, point, dimensions);
		}

		public double OrigDist2(IEnumerable<int> dimensions = null)
		{
			return PointOp.OrigDist2(this, dimensions);
		}

		public override bool IsWithin(IPoint point)
		{
			return PointOp.EqualGeometry(this, point);
		}
		public override bool EqualGeometry(IGeometry other)
		{
			if (this == other)
			{ return true; }

			if (!(other is IPoint o))
			{ return false; }

			if (Dimension != o.Dimension)
			{ return false; }

			return PointOp.EqualGeometry(this, o);
		}

		public override int GetHashCode()
		{
			if (Dimension == 1)
			{ return X.GetHashCode(); }
			else
			{
				return X.GetHashCode() ^ Y.GetHashCode();
			}
		}

		public override string ToString()
		{
			if (Dimension > 2)
			{ return string.Format("{0};{1};{2}", X, Y, Z); }
			else if (Dimension > 1)
			{ return string.Format("{0};{1}", X, Y); }
			else if (Dimension > 0)
			{ return X.ToString(); }
			else
			{ return "null"; }
		}

		public static Point operator +(PointCore p0, PointCore p1) => PointOp.Add(p0, p1);
		public static Point operator +(IPoint p0, PointCore p1) => PointOp.Add(p0, p1);
		public static Point operator +(PointCore p0, IPoint p1) => PointOp.Add(p0, p1);

		public static Point operator -(PointCore p0, IPoint p1) => PointOp.Sub(p0, p1);
		public static Point operator -(IPoint p0, PointCore p1) => PointOp.Sub(p0, p1);
		public static Point operator -(PointCore p0, PointCore p1) => PointOp.Sub(p0, p1);

		public static Point operator -(PointCore p0) => PointOp.Neg(p0);


		/// <summary>
		/// scalar product
		/// </summary>
		public static Point operator *(double f, PointCore p) => PointOp.Scale(f, p);

		public double SkalarProduct(IPoint p1) => PointOp.SkalarProduct(this, p1);

		/// <summary>
		/// get factor f of skalar product p0 = f * p1
		/// </summary>
		/// <param name="p0">result of scalar product</param>
		/// <returns>factor</returns>
		public double GetFactor(IPoint p0) => PointOp.GetFactor(this, p0);
	}

	public class PointWrap : PointCore
	{
		private readonly IPoint _point;
		public PointWrap(IPoint point)
		{
			_point = point;
		}
		protected override double GetX() => _point.X;
		protected override double GetY() => _point.Y;
		protected override double GetZ() => _point.Z;
		protected override double GetCoord(int index) => _point[index];
		public override int Dimension => _point.Dimension;
	}
	public class Point : PointCore
	{
		private readonly int _dimension;
		public Point(int dimension)
		{
			Coords = new double[dimension];
			_dimension = dimension;
		}

		public Point(double[] coordinates)
		{
			Coords = coordinates;
			_dimension = coordinates.Length;
		}
		public override int Dimension => _dimension;

		protected override double GetX() => X;
		protected override double GetY() => Y;
		protected override double GetZ() => Z;
		protected override double GetCoord(int index) => Coords[index];

		public new double X { get => Coords[0]; set => Coords[0] = value; }
		public new double Y { get => Coords[1]; set => Coords[1] = value; }
		public new double Z { get => Coords[2]; set => Coords[2] = value; }
		public new double this[int index] { get => Coords[index]; set => Coords[index] = value; }

		protected double[] Coords { get; }

		public static Point Create_0(int dimension)
		{
			Point created = CreateCore(new double[dimension]);
			return created;
		}
		public static Point Create(double[] coords)
		{
			Point created = CreateCore(coords);
			return created;
		}

		internal static Point CreateCore(double[] coords)
		{
			int dim = coords.Length;
			if (dim == 2)
			{ return new Point2D(coords[0], coords[1]); }
			else if (dim == 3)
			{ return new Point3D(coords[0], coords[1], coords[2]); }
			else
			{ return new Point(coords); }
		}

		public static Point Create(IPoint point)
		{
			int iDim = point.Dimension;
			double[] coords = new double[iDim];
			Point p = CreateCore(coords);
			for (int i = 0; i < iDim; i++)
			{ p[i] = point[i]; }
			return p;
		}

		public static Point CastOrCreate(IPoint point)
		{
			Point p = point as Point ?? Create(point);
			return p;
		}

		public static PointCore CastOrWrap(IPoint point)
		{
			PointCore p = point as PointCore ?? new PointWrap(point);
			return p;
		}
	}
	public class Point2D : Point
	{
		[DebuggerStepThrough]
		public Point2D()
			: base(2)
		{ }
		public Point2D(double x, double y)
			: base(2)
		{
			X = x;
			Y = y;
		}
		public Point2D(IPoint p)
			: this(p.X, p.Y)
		{ }
		protected Point2D(int dimension) : base(dimension) { }
	}

	public class Point3D : Point
	{
		public Point3D()
			: base(3)
		{ }
		public Point3D(double x, double y, double z)
			: base(3)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public new Point3D Clone()
		{
			return new Point3D(X, Y, Z);
		}

		public Point3D Transform(Point3D trans, double[][] rotMatrix)
		{

			Point3D p = new Point3D();

			double[][] r = rotMatrix;

			p.X = r[0][0] * X + r[0][1] * Y + r[0][2] * Z;
			p.Y = r[1][0] * X + r[1][1] * Y + r[1][2] * Z;
			p.Z = r[2][0] * X + r[2][1] * Y + r[2][2] * Z;

			if (trans != null)
			{
				p.X += trans.X;
				p.Y += trans.Y;
				p.Z += trans.Z;
			}

			return p;
		}

		public override string ToString()
		{
			return string.Format("{0};{1};{2}", X, Y, Z);
		}
	}

	public static class PointOp
	{
		public static double GetFactor(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
			double dMin = -1;
			double f = 0;
			foreach (int dim in dimensions)
			{
				if (Math.Abs(p0[dim]) > dMin)
				{
					f = p1[dim] / p0[dim];
					dMin = Math.Abs(p0[dim]);
				}
			}

			return f;
		}

		public static bool EqualGeometry(IPoint x, IPoint y, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(x, y);

			foreach (int dim in dimensions)
			{
				if (x[dim] != y[dim])
				{ return false; }
			}
			return true;
		}

		public static IPoint Project(IPoint p, IProjection projection)
		{
			IPoint projected = projection.Project(p);
			return projected;
		}

		public static double VectorProduct(IPoint p0, IPoint p1)
		{
			double v = p0.X * p1.Y - p0.Y * p1.X;
			return v;
		}

		public static double OrigDist2(IPoint p0, IEnumerable<int> dimensions = null)
		{
			IEnumerable<int> dims = dimensions ?? GeometryOperator.GetDimensions(p0);
			double dist2 = 0;
			foreach (int i in dims)
			{
				double d = p0[i];
				dist2 += d * d;
			}
			return dist2;
		}
		public static double Dist2(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
		{
			IEnumerable<int> dims = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
			double dist2 = 0;
			foreach (int i in dims)
			{
				double d = p0[i] - (p1?[i] ?? 0);
				dist2 += d * d;
			}
			return dist2;
		}

		public static Point Add(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
			List<double> sum = new List<double>();
			foreach (var dim in dimensions)
			{ sum.Add(p0[dim] + p1[dim]); }

			return Point.Create(sum.ToArray());
		}
		public static Point Sub(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
			List<double> diff = new List<double>();
			foreach (var dim in dimensions)
			{ diff.Add(p0[dim] - p1[dim]); }

			return Point.Create(diff.ToArray());
		}
		public static Point Neg(IPoint p0, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(p0);
			List<double> neg = new List<double>();
			foreach (var dim in dimensions)
			{ neg.Add(-p0[dim]); }

			return Point.Create(neg.ToArray());
		}
		public static Point Ortho(IPoint p0)
		{
			return Point.Create(new[] { -p0.Y, p0.X });
		}

		public static Point Scale(double f, IPoint p, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(p);
			List<double> scaled = new List<double>();
			foreach (var dim in dimensions)
			{ scaled.Add(f * p[dim]); }

			return Point.Create(scaled.ToArray());
		}

		public static double SkalarProduct(IPoint p0, IPoint p1, IEnumerable<int> dimensions = null)
		{
			dimensions = dimensions ?? GeometryOperator.GetDimensions(p0, p1);
			double prod = 0;
			foreach (var dim in dimensions)
			{ prod += p0[dim] * p1[dim]; }
			return prod;
		}
	}
}
