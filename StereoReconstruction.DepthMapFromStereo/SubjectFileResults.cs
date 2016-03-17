using System.Collections.Generic;

namespace StereoReconstruction.DepthMapFromStereo
{
    /// <summary>
    /// Класс для записи информации о результатах построения карты глубины субъекта
    /// </summary>
    public class SubjectFileResults
    {
        public string SubjectName; // Имя субъекта
        public List<DepthMapResult> DepthMapResults; // Результаты построения карт глубин для каждой пары

        public SubjectFileResults() { }

        public SubjectFileResults(string subjectName, List<DepthMapResult> depthMapResults)
        {
            SubjectName = subjectName;
            DepthMapResults = depthMapResults;
        }

        /// <summary>
        /// Класс результатов построения карты глубины
        /// </summary>
        public class DepthMapResult
        {
            public long Time; // Время, затраченное на одну итерацию (создание карты глубины для одной пары)
            public string OutputDepthMapPath; // Путь к карте глубины
            public string OutputDepthMapImagePath; // Путь к изображению карты глубины
            public Subject.StereoPair.Settings Properties; // Параметры камеры для пары изображений
            public Subject.StereoPair.Coordinates Coordinates; // Координаты камеры для пары изображений

            public DepthMapResult() { }
            
            public DepthMapResult(long time, string outputDepthMapPath, string outputDepthMapImagePath, Subject.StereoPair.Settings properties, Subject.StereoPair.Coordinates coordinates)
            {
                Time = time;
                OutputDepthMapPath = outputDepthMapPath;
                OutputDepthMapImagePath = outputDepthMapImagePath;
                Properties = properties;
                Coordinates = coordinates;
            }
        }
    }
}
