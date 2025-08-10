using DongUtility;

namespace Geometry.Geometry3D
{
    /// <summary>
    /// Polyhedron base class
    /// </summary>
    public abstract class Polyhedron : Shape3D
    {
        public abstract IEnumerable<Plane> Faces { get; }
        public abstract IEnumerable<LineSegment> Edges { get; }
        public abstract IEnumerable<Point> Vertices { get; }

        public override double SurfaceArea => throw new NotImplementedException();

        public override RangePair Range => throw new NotImplementedException();

        public override double MaxRadius => throw new NotImplementedException();

        public override Point Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Point ClosestPoint(Point point)
        {
            throw new NotImplementedException();
        }

        public override bool Inside(Point point)
        {
            throw new NotImplementedException();
        }

        public override LineSegment Overlap(LineSegment lineSegment)
        {
            throw new NotImplementedException();
        }

        public override Shape3D Translate(Vector vector)
        {
            throw new NotImplementedException();
        }
    }
}
