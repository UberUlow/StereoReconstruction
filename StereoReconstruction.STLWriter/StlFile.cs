using System.Collections.Generic;
using System.IO;

namespace StereoReconstruction.STLWriter
{
    /// <summary>
    /// Класс STL файла
    /// </summary>
    public class StlFile
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string SolidName { get; set; }

        /// <summary>
        /// Список треугольников
        /// </summary>
        public List<StlTriangle> Triangles { get; private set; }

        /// <summary>
        /// Инициализация класса
        /// </summary>
        public StlFile()
        {
            Triangles = new List<StlTriangle>();
        }

        /// <summary>
        /// Запись STL модели в файл
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            var writer = new StlWriter();
            writer.WriteAscii(this, stream);
        }
    }
}
