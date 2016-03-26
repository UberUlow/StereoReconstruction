using System.Collections.Generic;

namespace StereoReconstruction.СoordinateСonverter
{
    /// <summary>
    /// Класс для сравнения точек в трехмерном пространстве
    /// </summary>
    public class Point3DComparer : IEqualityComparer<Point3D>
    {
        public bool Equals(Point3D a, Point3D b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public int GetHashCode(Point3D obj)
        {
            return obj.X.GetHashCode() + obj.Y.GetHashCode() + obj.Z.GetHashCode();
        }
    }
}
