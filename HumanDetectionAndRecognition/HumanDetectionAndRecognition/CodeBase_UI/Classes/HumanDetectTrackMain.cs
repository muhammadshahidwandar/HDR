using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.BgSegm;

namespace HumanDetectionAndRecognition.CodeBase_UI.Classes
{
    class HumanDetectTrackMain
    {
        /// <summary>
        /// Background Subtraction, Object tracking :)
        /// </summary>
        BackgroundSubtractorMOG bgSubrctObj;
        public HumanDetectTrackMain()
        {
            bgSubrctObj = new BackgroundSubtractorMOG();
        }
        public Image <Gray, Byte> DetectAndTrack(Mat currentImage)
        {
              Image<Gray, Byte> ImgGray;
            //ImgGray = cur
              Image<Gray, Byte> imgReturn;
             bgSubrctObj.Apply(currentImage, currentImage);
              imgReturn = currentImage.ToImage<Gray, Byte>();
            return imgReturn;
        }



    }
}
