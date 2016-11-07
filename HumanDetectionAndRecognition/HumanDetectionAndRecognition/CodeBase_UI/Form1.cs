using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.Util;
//using HumanDetectionAndTracking;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.IO;


namespace HumanDetectionAndRecognition
{
    public partial class Form1 : Form
    {
        Capture capt = null;
      //  DetectionAndTracking detecObj;

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            Mat img;
            Image<Gray, Byte> ret_image;
            img = capt.QueryFrame();
            if (img != null)
            {
                imageBox1.Image = img;
            }


        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            capt = new Capture(openFileDialog1.FileName);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            timer1.Enabled = true;

        }
    }
}
