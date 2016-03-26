namespace StereoReconstruction.STLWriter
{
    /// <summary>
    /// Треугольник
    /// </summary>
    public class StlTriangle
    {
        public StlNormal Normal { get; set; }

        public StlVertex Vertex1 { get; set; }

        public StlVertex Vertex2 { get; set; }

        public StlVertex Vertex3 { get; set; }

        public StlTriangle(StlNormal normal, StlVertex vertex1, StlVertex vertexv2, StlVertex vertex3)
        {
            Normal = normal;
            Vertex1 = vertex1;
            Vertex2 = vertexv2;
            Vertex3 = vertex3;
        }
    }
}
