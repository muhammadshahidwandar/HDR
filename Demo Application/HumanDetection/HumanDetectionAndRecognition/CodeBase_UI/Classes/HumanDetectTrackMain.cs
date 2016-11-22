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
using System.Drawing;
using Emgu.CV.CvEnum;
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
        int Threshold = 230;
        public HumanDetectTrackMain()
        {
            _bgSubrctObj = new BackgroundSubtractorMOG();
            _connectedCompObj = new ConnectedComponent();
            _tracking = new AppearanceBasdTracking();
            _classification = new Classification();
            Load();
        }
        public Image<Bgr, Byte> DetectAndTrack(Mat currentImage)
        {
            Mat resizeImg = new Mat();
            Image<Gray, Byte> tmp2;
            CvInvoke.Resize(currentImage, resizeImg, new Size(320, 240));
            Image<Gray, Byte> imgReturn;
            Image<Ycc, Byte> imgYcc;
            Image<Bgr, Byte> imgBgr;
            List<ComponentData> connectedComp;
            List<ComponentData> humans = new List<ComponentData>();
            VectorOfPoint contours = new VectorOfPoint();
            MaskData maskData = new MaskData();
            imgYcc = currentImage.ToImage<Ycc, Byte>();
            imgBgr = currentImage.ToImage<Bgr, Byte>();
            Mat rect_12 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(5, 5), new Point(3, 3));

            _bgSubrctObj.Apply(currentImage, currentImage);
            imgReturn = currentImage.ToImage<Gray, Byte>();
            tmp2 = imgReturn.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, rect_12, new Point(3, 3), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
            connectedComp = _connectedCompObj.Find_ConnectedComponent(tmp2);
          
            
     //       trainImage(connectedComp[0].Silhouette);
     //       Save();
           // imgReturn = tmp2;
            humans = classify(connectedComp);
            //imgReturn = objectLabel(humans, tmp2);
            if (connectedComp.Count > 0)
            {
                contours = _classification.FindLargestContour(tmp2, tmp2);
                if (contours != null)
                {
                    // CvInvoke.FindContours(tmp2, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                    imgBgr.Draw(CvInvoke.BoundingRectangle(contours), new Bgr(0, 255, 0), 1);
                    Point a = contours[0];
                    a.Y = a.Y - 2;
                    a.X = a.X - 5;
                    // Save();
                    if (humans.Count > 0)
                    {


                        //Draw the label for each detected person
                        //ImgGray.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                        CvInvoke.PutText(
                                            imgBgr,
                                            "  Human ",
                                            a,
                                            FontFace.HersheyComplex,
                                            0.3,
                                            new MCvScalar(255, 0, 0));

                    }
                    if (humans.Count < 1)
                    {


                        //Draw the label for each detected person
                        //ImgGray.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                        CvInvoke.PutText(
                                            imgBgr,
                                            "Not a Human ",
                                            a,
                                            FontFace.HersheyComplex,
                                            0.3,
                                            new MCvScalar(255, 0, 0));

                    }
                }
            }
            //imgReturn = connectedComp[0].Silhouette;
            return imgBgr;

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
        /// <summary>
        /// Human Classification Code :)
        /// Determine if the Connected component is Human or not.
        /// </summary>
        /// <param name="list">list of connected components to be classified</param>
        /// <returns>list of Humans</returns>
        public List<ComponentData> classify(List<ComponentData> list)
        {
            List<ComponentData> humans = new List<ComponentData>();
            for (int i = 0; i < list.Count; i++)
                if (_classification.Classify_Image(list[i].Silhouette, Threshold))
                    humans.Add(list[i]);
            return humans;
        }
 /*       /// <summary>
        /// Moving Object Lablization Code
        /// </summary>
        /// <param name="List">human</param>
        /// <param name="Image">imgReturn gray image of segmented Object</param>
        /// <returns>Returned Image</returns>
        public Image<Gray, Byte> objectLabel(List<ComponentData> humans, Image<Gray, Byte> imgReturn)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Image<Gray, Byte> reslt;// = imgReturn;
            reslt = imgReturn;
            CvInvoke.FindContours(reslt, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            imgReturn.Draw(CvInvoke.BoundingRectangle(contours[0]), new Gray(255), 1);
            Point a = contours[0][0];
            a.Y = a.Y - 9;
            a.X = a.X - 40;
            // Save();
            if (humans.Count > 0)
            {


                //Draw the label for each detected person
                //ImgGray.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                CvInvoke.PutText(
                                     imgReturn,
                                    " Human ",
                                    a,
                                    FontFace.HersheyComplex,
                                    0.3,
                                    new MCvScalar(255, 0, 0));

            }
            if (humans.Count < 1)
            {


                //Draw the label for each detected person
                //ImgGray.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                CvInvoke.PutText(
                                     imgReturn,
                                    "  Not a Human ",
                                    a,
                                    FontFace.HersheyComplex,
                                    0.3,
                                    new MCvScalar(255, 0, 0));

            }
            return imgReturn;
        }*/
    }
}
