using System;
using System.Collections.Generic;

namespace StereoReconstruction.ConvexHullCreater
{
    public static class ConvexHull
    {
        /// <summary>
        /// Создает выпуклую оболочку из входных данных
        /// </summary>
        public static ConvexHull<TVertex, TFace> Create<TVertex, TFace>(IList<TVertex> data, ConvexHullComputationConfig config = null)
            where TVertex : IVertex
            where TFace : ConvexFace<TVertex, TFace>, new()
        {
            return ConvexHull<TVertex, TFace>.Create(data, config);
        }
    }

    /// <summary>
    /// Представление выпуклой оболочки
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TFace"></typeparam>
    public class ConvexHull<TVertex, TFace>
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>, new()
    {
        /// <summary>
        /// Точки выпуклой оболочки
        /// </summary>
        public IEnumerable<TVertex> Points { get; internal set; }

        /// <summary>
        /// Лица выпуклой оболочки
        /// </summary>
        public IEnumerable<TFace> Faces { get; internal set; }

        /// <summary>
        /// Создает выпуклую оболочку
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">Если null, то используется стандартный ConvexHullComputationConfig</param>
        /// <returns></returns>
        public static ConvexHull<TVertex, TFace> Create(IList<TVertex> data, ConvexHullComputationConfig config)
        {
            if (data == null) throw new ArgumentNullException("data");
            return ConvexHullInternal.GetConvexHull<TVertex, TFace>((IList<TVertex>)data, config);
        }

        /// <summary>
        /// Может быть создан только с использованием метода фабрики
        /// </summary>
        internal ConvexHull()
        {

        }
    }
}
