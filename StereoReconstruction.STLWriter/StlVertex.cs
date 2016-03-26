namespace StereoReconstruction.STLWriter
{
    /// <summary>
    /// Вершины
    /// </summary>
    public class StlVertex
    {
        public double X;
        public double Y;
        public double Z;

        public StlVertex(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
