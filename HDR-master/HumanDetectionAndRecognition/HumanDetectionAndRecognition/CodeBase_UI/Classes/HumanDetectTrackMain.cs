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
//using HumanDetectionAndRecognition;
using System.Drawing;
namespace HumanDetectionAndRecognition
{
    class HumanDetectTrackMain
    {
        /// <summary>
        /// Background Subtraction, Object tracking :)
        /// </summary>
        BackgroundSubtractorMOG _bgSubrctObj;
        ConnectedComponent _connectedCompObj;
        AppearanceBasdTracking _tracking;
        Classification _classification;
        public HumanDetectTrackMain()
        {
            _bgSubrctObj = new BackgroundSubtractorMOG();
            _connectedCompObj = new ConnectedComponent();
            _tracking = new AppearanceBasdTracking();
            _classification = new Classification();
        }
        public Image<Gray, Byte> DetectAndTrack(Mat currentImage)
        {
            Image<Gray, Byte> tmp2;
            Image<Gray, Byte> imgReturn;
            Image<Ycc, Byte> imgYcc;
            List<ComponentData> connectedComp;
            List<ComponentData> humans = new List<ComponentData>();
            MaskData maskData = new MaskData();
            imgYcc = currentImage.ToImage<Ycc, Byte>();
            Mat rect_12 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(5, 5), new Point(3, 3));

            //_bgSubrctObj.Apply(currentImage, currentImage);
            imgReturn = currentImage.ToImage<Gray, Byte>();
            tmp2 = imgReturn.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, rect_12, new Point(3, 3), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
            connectedComp = _connectedCompObj.Find_ConnectedComponent(tmp2);
            foreach (ComponentData comp in connectedComp)
            {
                maskData.Mask = comp.Silhouette.Clone();
                maskData.left = comp.LeftPoint;
                maskData.Right = comp.RightPoint;
                maskData.Top = comp.UpperPoint;
                maskData.Bottom = comp.LowerPoint;
                //comp.Label = 0;
                comp.Label = (byte)_tracking.Track(imgYcc, maskData);// _tracking.Track(currentImage, maskData);
                if (comp.Label != 255)
                {
                    humans.Add(comp);
                }
            }
            Load();
            // humans = classify(connectedComp);
           // trainImage(connectedComp[0].Silhouette);
           // Save();
             imgReturn = connectedComp[0].Silhouette;
            return imgReturn;

            
        }
        /// <summary>
        /// Get Feature Vector of current image only.
        /// </summary>
        /// <param name="_image">the image to be trained and saved</param>
        public void trainImage(Image<Gray, Byte> _image)
        {
            _classification.Training_Image(_image);
        }
        public void Save()
        {
            _classification.SaveFeatureVectors();
        }
        /// <summary>
        /// Load stored Feature Vectors.
        /// </summary>
        public void Load()
        {
            _classification.LoadFeatureVectors();
        }
    }
}
