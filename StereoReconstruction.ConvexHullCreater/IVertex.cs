namespace StereoReconstruction.ConvexHullCreater
{
    public interface IVertex
    {
        /// <summary>
        /// Позиция вершины
        /// </summary>
        double[] Position { get; }
    }

    public class DefaultVertex : IVertex
    {
        /// <summary>
        /// Позиция вершины
        /// </summary>
        public double[] Position { get; set; }
    }
}
