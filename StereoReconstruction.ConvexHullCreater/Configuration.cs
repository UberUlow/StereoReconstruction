using System;

namespace StereoReconstruction.ConvexHullCreater
{
    /// <summary>
    /// Определяет тип точки перевода для использования
    /// Это полезно для работы с «дегенеративным» данными
    /// </summary>
    public enum PointTranslationType
    {
        /// <summary>
        /// Ничего не произошло
        /// </summary>
        None,

        /// <summary>
        /// Точки переводятся только внутри, в результате вершины сохраняют свои первоначальные координаты
        /// </summary>
        TranslateInternal
    }

    /// <summary>
    /// Конфигурация вычисления выпуклой оболочки
    /// </summary>
    public class ConvexHullComputationConfig
    {
        /// <summary>
        /// Это значение используется для определения того, какие вершины имеют право быть частью выпуклой оболочки
        /// 
        /// Default = 0.00001
        /// </summary>
        public double PlaneDistanceTolerance { get; set; }

        /// <summary>
        /// Определяет какой метод использовать для точечного перевода
        /// Это помогает с обработкой «вырожденных» данных, таких как равномерных сетках
        /// 
        /// Default = None
        /// </summary>
        public PointTranslationType PointTranslationType { get; set; }

        /// <summary>
        /// Функция используемая для генерации направления перевода
        /// 
        /// Default = null
        /// </summary>
        public Func<double> PointTranslationGenerator { get; set; }

        /// <summary>
        /// Создание конфигурации со значениями по умолчанию
        /// </summary>
        public ConvexHullComputationConfig()
        {
            PlaneDistanceTolerance = 0.00001;
            PointTranslationType = PointTranslationType.None;
            PointTranslationGenerator = null;
        }
    }
}
