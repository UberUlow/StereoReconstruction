namespace StereoReconstruction.СoordinateСonverter
{
    /// <summary>
    /// Класс, описывающих координаты точки в пространстве
    /// </summary>
    public class Point3D
    {
        public double X; // Координата X (по ширине)
        public double Y; // Координата Y (глубина)
        public double Z; // Координата Z (по высоте)

        public Point3D() { }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
