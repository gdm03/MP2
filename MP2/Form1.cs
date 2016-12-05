using MP2.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP2
{
    public partial class MP2Form : Form
    {
        public MP2Form()
        {
            InitializeComponent();
        }

        private void openJPEGImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                String dir = fbd.SelectedPath;
                ImageHandler imgHandler = new ImageHandler(dir);
            }

            // Change directory here
            /*
            String dir = @"D:\DLSU-M\Term 1 AY 2016-2017\CSC741M\MP2 Specs\JPEG\uni";
            String[] imagePaths = System.IO.Directory.GetFiles(dir, "*.jpg", SearchOption.AllDirectories);
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                String imgPath = ofd.FileName;
                currImgPath = imgPath;
                selectedImageBox.Image = new Bitmap(imgPath);
                Bitmap img = new Bitmap(imgPath);
            }
            */
        }
    }
}
