using System.Drawing;

namespace StereoReconstruction.RegionGrowing
{
    public class RegionUniqueness
    {
        public int RegionValue;
        public Color ColorValue;

        public RegionUniqueness(int regionValues, Color colorValue)
        {
            RegionValue = regionValues;
            ColorValue = colorValue;
        }
    }
}
