using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using StereoReconstruction.ConvexHullCreater;
using StereoReconstruction.СoordinateСonverter;
using StereoReconstruction.Common.Logging;
using StereoReconstruction.STLWriter;

namespace StereoReconstruction.Triangulation
{
    /// <summary>
    /// Класс для построения 3D модели
    /// </summary>
    public static class Builder3DModel
    {
        /// <summary>
        /// Создание 3Д модели
        /// </summary>
        /// <param name="points3D">Набор точек в трехмерном пространстве</param>
        /// <param name="writeToFile">Флаг записи в файл</param>
        /// <returns>3D модель</returns>
        public static void Create3DModel(List<Point3D> points3D, string modelName = "standartModel", string path = "")
        {
            Tracer.Info("\nПостроение 3D модели:");
            Stopwatch timer = Stopwatch.StartNew(); // Старт секундомера для диагностики работы алгоритма
            Tracer.Info("\nСоздание вершин, на основе точек в трехмерном пространстве...");
            List<Vertex> vertices = new List<Vertex>();
            foreach (var point3D in points3D)
            {
                vertices.Add(new Vertex(point3D.X, point3D.Y, point3D.Z));
            }
            Tracer.Info("Построение оболочки 3D модели...");
            ConvexHull<Vertex, Face> convexHull = ConvexHull.Create<Vertex, Face>(vertices);

            WriteModelToFile(convexHull.Faces.ToList(), modelName, path);

            timer.Stop(); // Остановка секундомера
            Tracer.Info($"\nПостроение 3D модели завершено. Это заняло {timer.ElapsedMilliseconds} мс.\n");
        }

        private static void WriteModelToFile(List<Face> faces, string modelName, string path)
        {
            Tracer.Info("Запись 3D модели в STL файл...");

            StlFile stlF = new StlFile();
            stlF.SolidName = modelName;
            foreach (var face in faces)
            {
                stlF.Triangles.Add(new StlTriangle(new StlNormal(face.Normal[0], face.Normal[1], face.Normal[2]),
                    new StlVertex(face.Vertices[0].Center.X, face.Vertices[0].Center.Y, face.Vertices[0].Center.Z),
                    new StlVertex(face.Vertices[1].Center.X, face.Vertices[1].Center.Y, face.Vertices[1].Center.Z),
                    new StlVertex(face.Vertices[2].Center.X, face.Vertices[2].Center.Y, face.Vertices[2].Center.Z)));
            }
            using (FileStream fs = new FileStream(string.Concat(path, @"\", modelName, ".stl"), FileMode.Create))
            {
                stlF.Save(fs);
            }
        }
    }
}
