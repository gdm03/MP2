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
                imgHandler.returnShotBoundaries();
            }


        }
    }
}
