namespace StereoReconstruction.DepthMapFromStereo
{
    /// <summary>
    /// Результаты построения карт глубины для алгоритма наращивания регионов
    /// </summary>
    public class ResultsFromRegionGrowing
    {
        public double[,] DepthMapValue; // Карта глубины
        public int TemplateSize; // Размер шаблона

        public ResultsFromRegionGrowing() { }

        public ResultsFromRegionGrowing(double[,] depthMapValue, int templateSize)
        {
            DepthMapValue = depthMapValue;
            TemplateSize = templateSize;
        }
    }
}
