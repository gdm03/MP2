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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Debug.WriteLine(fbd.SelectedPath);
                ImageHandler imgHandler = new ImageHandler(fbd.SelectedPath);
                displayShotBoundaries(imgHandler.returnShotBoundaries());
            }
        }

        private void displayShotBoundaries(List<String> paths)
        {
            List<int> bottomlist = new List<int>();
            int c = 0;
            panel1.Controls.Clear();
            foreach (String s in paths)
            {
                //Debug.WriteLine(s);
                PictureBox pc = new PictureBox();
                Image imgTest = new Bitmap(s);
                pc.Image = imgTest;
                pc.Size = imgTest.Size;
                if (c == 0)
                {
                    bottomlist.Add(pc.Bottom + 8);
                    pc.Top = 8;
                    pc.Left = 8;
                }

                else
                {
                    bottomlist.Add(pc.Bottom + bottomlist[c - 1] + 8);
                    pc.Top = bottomlist[c - 1] + 8;
                    pc.Left = 8;
                }
                c++;
                panel1.Controls.Add(pc);
            }
        }

        private void displayKeyframes(List<String> paths)
        {
            List<int> bottomlist = new List<int>();
            int c = 0;
            panel2.Controls.Clear();
            foreach (String s in paths)
            {
                //Debug.WriteLine(s);
                PictureBox pc = new PictureBox();
                Image imgTest = new Bitmap(s);
                pc.Image = imgTest;
                pc.Size = imgTest.Size;
                if (c == 0)
                {
                    bottomlist.Add(pc.Bottom + 8);
                    pc.Top = 8;
                    pc.Left = 8;
                }

                else
                {
                    bottomlist.Add(pc.Bottom + bottomlist[c - 1] + 8);
                    pc.Top = bottomlist[c - 1] + 8;
                    pc.Left = 8;
                }
                c++;
                panel2.Controls.Add(pc);
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
