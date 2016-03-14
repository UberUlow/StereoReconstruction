namespace StereoReconstruction.RegionGrowing
{
    /// <summary>
    /// Класс дескрипторов
    /// </summary>
    public class Descriptors
    {
        public double ER; // Значение мат.ожидания по R- компоненте
        public double EG; // Значение мат.ожидания по G- компоненте
        public double EB; // Значение мат.ожидания по B- компоненте

        public double DR; // Значение дисперсии по R- компоненте
        public double DG; // Значение дисперсии по G- компоненте
        public double DB; // Значение дисперсии по B- компоненте

        public Descriptors(double eR, double eG, double eB, double dR, double dG, double dB)
        {
            ER = eR;
            EG = eG;
            EB = eB;
            DR = dR;
            DG = dG;
            DB = dB;
        }
    }
}
