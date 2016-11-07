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
using HumanDetectionAndRecognition.CodeBase_UI.Classes;


namespace HumanDetectionAndRecognition
{
    public partial class Form1 : Form
    {
        Capture capt = null;
        HumanDetectTrackMain detecObj;

        public Form1()
        {
            InitializeComponent();
            detecObj = new HumanDetectTrackMain();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            Mat img;
            Image<Gray, Byte> ret_image;
            img = capt.QueryFrame();
            if (img != null)
            {
                ret_image = detecObj.DetectAndTrack(img);
                imageBox1.Image = ret_image;
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
