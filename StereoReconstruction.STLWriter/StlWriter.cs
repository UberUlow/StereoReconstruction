using System.IO;

namespace StereoReconstruction.STLWriter
{
    /// <summary>
    /// Класс для записи Stl модели файла
    /// </summary>
    public class StlWriter
    {
        /// <summary>
        /// Запись STL модели формата Ascii в файл
        /// </summary>
        /// <param name="file">Stl модель</param>
        /// <param name="stream">Байтовый поток</param>
        public void WriteAscii(StlFile file, Stream stream)
        {
            var writer = new StreamWriter(stream);
            writer.WriteLine(string.Format("solid {0}", file.SolidName));
            foreach (var triangle in file.Triangles)
            {
                writer.WriteLine(string.Format("\tfacet normal {0}", NormalToString(triangle.Normal)));
                writer.WriteLine("\t\touter loop");
                writer.WriteLine(string.Format("\t\t\tvertex {0}", VertexToString(triangle.Vertex1)));
                writer.WriteLine(string.Format("\t\t\tvertex {0}", VertexToString(triangle.Vertex2)));
                writer.WriteLine(string.Format("\t\t\tvertex {0}", VertexToString(triangle.Vertex3)));
                writer.WriteLine("\t\tendloop");
                writer.WriteLine("\tendfacet");
            }

            writer.WriteLine(string.Format("endsolid {0}", file.SolidName));
            writer.Flush();
        }

        /// <summary>
        /// Перевод нормального вектора в строку
        /// </summary>
        /// <param name="normal">Нормальный вектор</param>
        /// <returns>Строка нормального вектора</returns>
        private static string NormalToString(StlNormal normal)
        {
            return string.Format("{0} {1} {2}", normal.X.ToString(), normal.Y.ToString(), normal.Z.ToString());
        }

        /// <summary>
        /// Перевод вершин в строку
        /// </summary>
        /// <param name="vertex">Вершины</param>
        /// <returns>Строка вершин</returns>
        private static string VertexToString(StlVertex vertex)
        {
            return string.Format("{0} {1} {2}", vertex.X.ToString(), vertex.Y.ToString(), vertex.Z.ToString());
        }
    }
}
