using System;
using System.IO;
using System.Linq;

namespace StereoReconstruction.Common.Helpers
{
    /// <summary>
    /// Класс, для чтения или записи из/в файл
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Запись матрицы в файл
        /// </summary>
        /// <typeparam name="T">Тип матрицы</typeparam>
        /// <param name="path">Путь</param>
        /// <param name="matrix">Матрица</param>
        /// <param name="separator">Разделитель</param>
        public static void WriteMatrix<T>(string path, T[,] matrix, char separator)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (j != matrix.GetLength(1) - 1)
                        {
                            writer.Write(string.Concat(matrix[i, j], separator));
                        }
                        else
                        {
                            writer.Write(string.Concat(matrix[i, j]));
                        }
                    }
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Чтение матрицы из файла
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Матрица типа T</returns>
        public static T[,] ReadMatrix<T>(string path, char separator)
        {
            return JaggedToMultidimensional(File.ReadAllLines(path).Select(l => l.Split(separator).Select(i => (T)Convert.ChangeType(i, typeof(T))).ToArray()).ToArray());
        }

        /// <summary>
        /// Перевод массива массивов в двумерный массив
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="jaggedArray">Массив массивов</param>
        /// <returns>Матрица типа T</returns>
        private static T[,] JaggedToMultidimensional<T>(T[][] jaggedArray)
        {
            int rows = jaggedArray.Length;
            int columns = jaggedArray.Max(subArray => subArray.Length);
            T[,] array = new T[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    array[i, j] = jaggedArray[i][j];
                }
            }
            return array;
        }
    }
}
