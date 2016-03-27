namespace StereoReconstruction.ConvexHullCreater
{
    public abstract class ConvexFace<TVertex, TFace>
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>
    {
        /// <summary>
        /// Смежности. Массив, длинной "dimension"
        /// Если F = Adjacency[i] тогда вершины, общие с F являются j-ми вершинами где j != i
        /// В контексте триангуляции может быть нулевой (указывает, что ячейка находится на границе)
        /// </summary>
        public TFace[] Adjacency { get; set; }

        /// <summary>
        /// Вершины хранятся по часовой стрелке для размерности 2 - 4, в более высоких размерностях порядок произвольный
        /// 3D Normal = (V[1] - V[0]) x (V[2] - V[1])
        /// </summary>
        public TVertex[] Vertices { get; set; }

        /// <summary>
        /// Нормальный вектор. Пустое значение, если используется в триангуляции
        /// </summary>
        public double[] Normal { get; set; }
    }

    /// <summary>
    /// Стандартное представление выпуклой грани
    /// </summary>
    public class DefaultConvexFace<TVertex> : ConvexFace<TVertex, DefaultConvexFace<TVertex>>
        where TVertex : IVertex
    {
    }
}
