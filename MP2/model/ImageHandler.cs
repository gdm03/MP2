using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP2.model
{
    class ImageHandler
    {
        private String path;
        private List<String> imgPaths;
        private List<String> shotBoundaries;
        private List<String> keyframes;

        public ImageHandler(String path)
        {
            this.path = path;
            imgPaths = Directory.GetFiles(path).Where(x => x.EndsWith(".jpg")).ToList(); // LINQ query
        }

        public Dictionary<int, int> quantizeImage(String path)
        {
            Bitmap img = new Bitmap(path);
            Dictionary<int, int> histogram = new Dictionary<int, int>();

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i);
                    int code = ((pixel.R & 192) >> 2) + ((pixel.G & 192) >> 4) + ((pixel.B & 192) >> 6);
                    
                    if (histogram.ContainsKey(code))
                    {
                        histogram[code] = histogram[code] + 1;
                    }
                    else
                    {
                        histogram.Add(code, 1);
                    }
                }
            }

            return histogram;
        }

        public double computeHistogramDifference(Dictionary<int, int> hist1, Dictionary<int, int> hist2)
        {
            int[] histBins1 = new int[64];
            int[] histBins2 = new int[64];
            double difference = 0;

            for (int i = 0; i < 64; i++)
            {
                hist1.TryGetValue(i, out histBins1[i]);
                hist2.TryGetValue(i, out histBins2[i]);
                //differenceHistogram[i] = Math.Abs(histBins1[i] - histBins2[i]);
                difference += Math.Abs(histBins1[i] - histBins2[i]);
                //Debug.WriteLine("[" + i + "]: " + histBins1[i] + " - " + histBins2[i] + " = " + Math.Abs(histBins1[i] - histBins2[i]));
            }
            return difference;
        }

        public List<String> returnShotBoundaries()
        {
            shotBoundaries = new List<String>();
            Dictionary<int, int> hist1 = new Dictionary<int, int>();
            Dictionary<int, int> hist2 = new Dictionary<int, int>();
            double[] differenceHistogram = new double[imgPaths.Count - 1];

            // histogram difference 
            hist1 = quantizeImage(imgPaths[0]);

            //foreach (String s in imgPaths) Debug.WriteLine(s);

            for (int i = 1; i < imgPaths.Count; i++)
            {
                hist2 = quantizeImage(imgPaths[i]);
                differenceHistogram[i - 1] = computeHistogramDifference(hist1, hist2);
                hist1 = hist2;
                Debug.WriteLine("["+ (i-1) + "] " + differenceHistogram[i - 1]);
            }

            //Debug.WriteLine("i: " + differenceHistogram.ToList().IndexOf(differenceHistogram.Max()) + " value: " + differenceHistogram.Max());
            Debug.WriteLine("done");

            double mean = differenceHistogram.Average();
            double ssd = differenceHistogram.Sum(x => (x - mean) * (x - mean)); // sum of squared differences
            double stdev = Math.Sqrt(ssd / differenceHistogram.Length);

            //default alpha
            int alpha = 5;
            // threshold
            double thresholdBreak = mean + alpha * stdev;
            double thresholdTransition;
            Debug.WriteLine(thresholdBreak);


            return shotBoundaries;
        }

        public List<String> returnKeyframes()
        {
            return keyframes;
        }
    }
}
