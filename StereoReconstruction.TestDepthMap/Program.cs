using System;
using StereoReconstruction.DepthMapFromStereo;

using Emgu.CV;
using Emgu.CV.Structure;

namespace StereoReconstruction.TestDepthMap
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.GetLength(0) == 1)
            {
                DepthMap.Create(args[0]);
            }
            else if (args.GetLength(0) == 2)
            {
                int size = Convert.ToInt32(args[1]);
                Image<Gray, byte> image1 = new Image<Gray, byte>("1.jpg");
                Image<Gray, byte> image2 = new Image<Gray, byte>("2.jpg");

                Image<Gray, byte> mediansmooth1 = image1.SmoothMedian(size);
                Image<Gray, byte> mediansmooth2 = image2.SmoothMedian(size);
                mediansmooth1.Save(@"D:\Data\!Test\Img\_median1.jpg");
                mediansmooth2.Save(@"D:\Data\!Test\Img\_median2.jpg");
            }
            else
            {
                DepthMap.Create("test.xml");
            }

            Console.WriteLine("Готово");
            Console.ReadKey();
        }
    }
}
