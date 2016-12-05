using System;
using System.Collections.Generic;
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
        private string path;
        private List<String> imgPaths;
        private List<String> shotBoundaries;
        private List<String> keyframes;

        public ImageHandler(String path)
        {
            this.path = path;
            imgPaths = Directory.GetFiles(path).Where(x => x.EndsWith("*.jpg")).ToList();
        }

        public Dictionary<int, int> quantizeImage(String path)
        {
            Bitmap img = new Bitmap(path);
            Dictionary<int, int> histogram = new Dictionary<int, int>();

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; i < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i); // x, y
                    int code = ((pixel.R & 192) >> 2) + ((pixel.G & 192) >> 4) + ((pixel.B & 192) >> 6) & 63; // 64 bins

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
    }
}
