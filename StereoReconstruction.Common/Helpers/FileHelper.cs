using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StereoReconstruction.Common.Helpers
{
    public static class FileHelper
    {
        /// Запись матрицы в файл
        /// </summary>
        /// <typeparam name="T">Тип матрицы</typeparam>
        /// <param name="path">Путь</param>
        /// <param name="matrix">Матрица</param>
        /// <param name="separator">Разделитель</param>
        public static void WriteMatrix<T>(string path, T[,] matrix, string separator)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        sw.Write(matrix[i, j] + separator);
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}
