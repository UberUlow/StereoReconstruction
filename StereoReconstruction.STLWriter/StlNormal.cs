namespace StereoReconstruction.STLWriter
{
    /// <summary>
    /// Нормальный вектор
    /// </summary>
    public class StlNormal
    {
        public double X;
        public double Y;
        public double Z;

        public StlNormal(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
