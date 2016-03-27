using StereoReconstruction.ConvexHullCreater;
using Petzold.Media3D;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace StereoReconstruction.Triangulation
{
    /// <summary>
    /// Вершина представляет собой простой класс, который сохраняет положение точки, узла или вершины
    /// </summary>
    public class Vertex : ModelVisual3D, IVertex
    {
        static readonly Material material = new DiffuseMaterial(Brushes.Black);
        static readonly SphereMesh mesh = new SphereMesh { Slices = 6, Stacks = 4, Radius = 0.5 };

        static readonly Material hullMaterial = new DiffuseMaterial(Brushes.Yellow);
        static readonly SphereMesh hullMesh = new SphereMesh { Slices = 6, Stacks = 4, Radius = 1.0 };

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Vertex"/>
        /// </summary>
        /// <param name="x">Позиция X</param>
        /// <param name="y">Позиция Y</param>
        /// <param name="z">Позиция Z</param>
        public Vertex(double x, double y, double z, bool isHull = false)
        {
            Content = new GeometryModel3D
            {
                Geometry = isHull ? hullMesh.Geometry : mesh.Geometry,
                Material = isHull ? hullMaterial : material,
                Transform = new TranslateTransform3D(x, y, z)
            };
            Position = new double[] { x, y, z };
        }

        public Vertex AsHullVertex()
        {
            return new Vertex(Position[0], Position[1], Position[2], true);
        }

        public Point3D Center { get { return new Point3D(Position[0], Position[1], Position[2]); } }

        /// <summary>
        /// Получает или задает координаты
        /// </summary>
        /// <value>Значения координат</value>
        public double[] Position
        {
            get;
            set;
        }
    }
}
