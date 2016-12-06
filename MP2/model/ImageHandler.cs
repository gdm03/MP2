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
        
        List<String> startShots = new List<String>();
        List<String> endShots = new List<String>();
        List<String> transitionShots = new List<String>();

        public static int width = 176;
        public static int height = 144;

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

        public double computeHistogramDifference(Dictionary<int, double> hist1, Dictionary<int, double> hist2)
        {
            double[] histBins1 = new double[64];
            double[] histBins2 = new double[64];
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

            List<Dictionary<int, int>> allHistograms = new List<Dictionary<int, int>>();

            // histogram difference 
            hist1 = quantizeImage(imgPaths[0]);
            allHistograms.Add(quantizeImage(imgPaths[0]));
            for (int i = 1; i < imgPaths.Count; i++)
            {
                hist2 = quantizeImage(imgPaths[i]);
                allHistograms.Add(quantizeImage(imgPaths[i]));
                differenceHistogram[i - 1] = computeHistogramDifference(hist1, hist2);
                hist1 = hist2;
            }

            //Debug.WriteLine("i: " + differenceHistogram.ToList().IndexOf(differenceHistogram.Max()) + " value: " + differenceHistogram.Max());
            //Debug.WriteLine("done");

            double mean = differenceHistogram.Average();
            double ssd = differenceHistogram.Sum(x => (x - mean) * (x - mean)); // sum of squared differences
            double stdev = Math.Sqrt(ssd / differenceHistogram.Length);

            //default alpha
            int alpha = 5;
            // threshold
            double thresholdBreak = mean + alpha * stdev;
            double thresholdTransition = mean * 1.5;
            //Debug.WriteLine(mean + " " + stdev);
            int transitionFrameTolerance = 3;
            int transitionCounter = 0;
            bool transitioning = false;
            double accumulatedDifference = 0; // AC
            int startIndex = -1;

            shotBoundaries.Add(imgPaths[0]); //Fs

            for (int i = 0; i < differenceHistogram.Length; i++)
            {
                if (differenceHistogram[i] > thresholdBreak)
                {
                    shotBoundaries.Add(imgPaths[i]);
                    endShots.Add(imgPaths[i]);
                    shotBoundaries.Add(imgPaths[i + 1]);
                    startShots.Add(imgPaths[i + 1]);
                }

                else if (differenceHistogram[i] > thresholdTransition)
                {
                    if (!transitioning)
                    {
                        //shotBoundaries.Add(imgPaths[i]); // mark potential GT
                        transitionShots.Add(imgPaths[i]);
                        startIndex = i;
                        transitioning = true;
                    }
                }

                else // x < Ts < Tb
                {
                    //if (transitioning && startIndex > -1)
                    if (transitioning)
                    {
                        //if (transitionCounter < transitionFrameTolerance && computeHistogramDifference(allHistograms[startIndex], allHistograms[i]) > thresholdBreak)
                        if (transitionCounter < transitionFrameTolerance)
                        {
                            accumulatedDifference += computeHistogramDifference(allHistograms[i], allHistograms[i + 1]);
                            transitionCounter++;
                        }
                        else
                        {
                            //Debug.WriteLine(frameDifference + " " + thresholdTransition + " " + accumulatedDifference + " " + thresholdBreak);
                            if (accumulatedDifference > thresholdBreak)
                            {
                                for (int j = startIndex; j < i; j++)
                                {
                                    //shotBoundaries.Add(imgPaths[j + 1]);
                                    transitionShots.Add(imgPaths[j + 1]);
                                }

                                accumulatedDifference = 0;
                            }
                            startIndex = -1;
                            transitioning = false;
                            transitionCounter = 0;
                        }
                    }
                }
            }
            
            if (!shotBoundaries.Contains(imgPaths[imgPaths.Count - 1]))
                shotBoundaries.Add(imgPaths[imgPaths.Count - 1]);

            //foreach (String s in shotBoundaries)
            /*
            Debug.WriteLine("Start Shots");
            foreach (String s in startShots)
                Debug.WriteLine(s);

            Debug.WriteLine("End Shots");
            foreach (String s in endShots)
                Debug.WriteLine(s);

            Debug.WriteLine("Transition Shots");
            foreach (String s in transitionShots)
                Debug.WriteLine(s);

            */
            return shotBoundaries;
        }

        public List<String> returnTransitionShots()
        {
            return transitionShots;
        }

        public List<string> returnKeyframes()
        {
            keyframes = new List<string>();

            List<List<string>> shots = new List<List<string>>();
            List<string> currShot = new List<string>();
            int boundaryIndex = 0;
            for (int i = 0; i< imgPaths.Count; i++)
            {
                string s = imgPaths[i];
                currShot.Add(s);
                if (s.Equals(endShots[boundaryIndex]))
                {
                    shots.Add(currShot);
                    currShot = new List<string>();
                    if(boundaryIndex < endShots.Count - 1)
                        boundaryIndex++;
                }
                if (i == imgPaths.Count - 1)
                {
                    shots.Add(currShot);
                }
            }

            foreach (List<string> shot in shots)
            {
                //Console.WriteLine("New shot");
                Dictionary<string, Dictionary<int, int>> histograms = new Dictionary<string, Dictionary<int, int>>();
                Dictionary<string, Dictionary<int, double>> normalizedHistograms = new Dictionary<string, Dictionary<int, double>>();
                Dictionary<string, double> distances = new Dictionary<string, double>();

                foreach (string s in shot)
                {
                    //Console.WriteLine("Img: " + s);
                    histograms.Add(s, quantizeImage(s));
                    normalizedHistograms.Add(s, getNormalizedHistogram(quantizeImage(s)));
                }

                Dictionary<int, double> averageHistogram = getNormalizedHistogram(getAverageHistogram(histograms));
                foreach(KeyValuePair<String, Dictionary<int, double>> entry in normalizedHistograms)
                {
                    double difference = computeHistogramDifference(entry.Value, averageHistogram);
                    distances.Add(entry.Key, difference);
                }

                foreach (KeyValuePair<string,double> entry in distances.OrderBy(pair => pair.Value).Take(1)) //take lowest difference
                {
                    keyframes.Add(entry.Key);
                    Console.WriteLine("difference: " + entry.Value);
                }

            }
            return keyframes;
        }

        /** for key frame detection, pads 0's to every bin */
        private Dictionary<int, double> getNormalizedHistogram(Dictionary<int, int> hist)
        {
            Dictionary<int, double> normHist = new Dictionary<int, double>();
            double divisor = width * height;
            for (int i = 0; i < 64; i++)
            {
                int val;
                hist.TryGetValue(i, out val);
                normHist.Add(i, val / divisor);
            }
            return normHist;
        }

        private Dictionary<int, double> getNormalizedHistogram(Dictionary<int, double> hist)
        {
            Dictionary<int, double> normHist = new Dictionary<int, double>();
            double divisor = width * height;
            for (int i = 0; i < 64; i++)
            {
                double val;
                hist.TryGetValue(i, out val);
                normHist.Add(i, val / divisor);
            }
            return normHist;
        }

        private Dictionary<int,double> getAverageHistogram(Dictionary<string, Dictionary<int, int>> histograms)
        {
            Dictionary<int, double> avghist = new Dictionary<int, double>();
            for (int i = 0; i < 64; i++)
            {
                double ave = 0;
                foreach(Dictionary<int,int> hist in histograms.Values)
                {
                    int val;
                    hist.TryGetValue(i, out val);
                    ave += val;
                }

                avghist.Add(i, ave/histograms.Count);
            }
            return avghist;
        }
    }
}
