using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using ZedGraph;
using Emgu.CV.CvEnum;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace HumanDetectionAndRecognition
{
    public class Classification
    {
        #region Public Variables
        public List<List<int>> _vectorsFeature = new List<List<int>>();
        public List<List<int>> _trainVF = new List<List<int>>();
        public string vectorFeatureStore = "D:\\vectorFeatureStore.bin";
        //int Threshold = 300;
        #endregion
        /// <summary>
        /// This function take Connected Component and it only call another function Called ==> GetFeatureVector
        /// to calculate VectorFeature for this Connected Component
        /// and will Assign the result in public variable (_vectorsFeature)
        /// </summary>
        /// <param name="image">Connected Components of training object</param>
        public void Training_Image(Image<Gray, byte> image)
        {
            List<int> _vectorFeature;
            _vectorFeature = GetFeatureVector(image);
            #region Add Vector To Vectors
            _vectorsFeature.Add(_vectorFeature);
            #endregion
        }
        /// <summary>
        /// This function take Connected Component for unknown object and
        /// Threshold ToCompare with min disstortion of it and the trained image
        /// </summary>
        /// <param name="image">Connected Componets of object</param>
        /// <param name="Threshold">Threshold if it > min diss therefore it's human</param>
        /// <returns>true, if it is Human.
        /// false, if else.</returns>
        public bool Classify_Image(Image<Gray, byte> image, int Threshold)
        {
            #region Private Variables
            List<int> _dist = new List<int>();
            List<int> _vectorFeature = new List<int>();
            int _minDis = 10000;
            int _vectorDiss = 0;
            #endregion

            //Retrun in Public Variable _vectorsFeature
            _vectorFeature = GetFeatureVector(image);
            if (_vectorFeature.Count > 0)
            {
                for (int i = 0; i < _trainVF.Count; i++)
                {
                    for (int j = 0; j < 40; j++)
                    {
                        _vectorDiss += Math.Abs(_vectorFeature[j] - _trainVF[i][j]);
                    }
                    _dist.Add(_vectorDiss);
                    _vectorDiss = 0;
                }
                _dist.Sort();
                if (_dist.Count == 0)
                    return false;
                _minDis = _dist[0];

                if (_minDis < Threshold)
                {
                    //MessageBox.Show("It's Human");
                    // MessageBox.Show("It's Human");
                    return true;
                }
            }

            //MessageBox.Show("No it Isn't Human");
            return false;
        }
        /// <summary>
        /// This function calculate the VectorFeature for the given Connected Components
        /// </summary>
        /// <param name="_image">Connected Componets of object</param>
        private List<int> GetFeatureVector(Image<Gray, byte> _image)
        {
            #region Private Variables
            bool _next = true;
            int _left_len = 0, _right_len = 0;
            int CntLeft = 1, CntRight = 1;
            Image<Gray, byte> image2;
            //VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint(); //VectorOfPoint 
            VectorOfPoint contours;
            List<int> _vectorFeature;
            List<Point> _vectorFeature_left;
            List<Point> _vectorFeature_right;
            #endregion
            #region Get Contour Of Image
            image2 = _image.Resize(40, 80, 0);  //20 ,40;
            //   CvInvoke.Canny(image2,image2, 100, 200); 
            //     Contour ctr = _image.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP);
            contours = FindLargestContour(image2, image2);
            #endregion
            #region Initialization
            _vectorFeature = new List<int>();
            _vectorFeature_left = new List<Point>();
            _vectorFeature_right = new List<Point>();

            #endregion

            if (contours == null)
                return _vectorFeature;
            //      while(_next)
            //           for (int k = 0; k < total; k++)
            //           {
            if (contours.Size >= 40)
            {
                #region GetPixels
                for (int k = 0; k < contours.Size; k++)
                {

                    //Left Hand Side
                    if (contours[k].X <= 20)
                        _vectorFeature_left.Add(contours[k]);
                    //Right Hand Side
                    else if (contours[k].X > 20)
                        _vectorFeature_right.Add(contours[k]);
                }
                if (_vectorFeature_left.Count < 20 || _vectorFeature_right.Count < 20)
                { }
                #endregion
                else
                {
                    #region Select 20 Points From Left and Right
                    _left_len = _vectorFeature_left.Count;
                    _right_len = _vectorFeature_right.Count;
                    if (_left_len >= 40)
                        CntLeft = _left_len / 20;
                    if (_right_len >= 40)
                        CntRight = _right_len / 20;
                    //20 From Left
                    for (int i = 0; i < _left_len; )
                    {
                        if (_vectorFeature.Count >= 20)
                            break;
                        _vectorFeature.Add(_vectorFeature_left[i].X);
                        i += CntLeft;
                    }
                    //20 From Right
                    for (int i = 0; i < _right_len; )
                    {
                        if (_vectorFeature.Count >= 40)
                            break;
                        _vectorFeature.Add(_vectorFeature_right[i].Y);
                        i += CntRight;
                    }
                    #endregion
                  //  #region Add Vector To Vectors
                 //   _vectorsFeature.Add(_vectorFeature);
                 //   #endregion
                    return _vectorFeature;
                }
            }
            //#region Check For Next Contour
            //if (contours.HNext != null)
            //    contours = contours.HNext;
            //else
            //    _next = false;
            //#endregion
            return _vectorFeature;
        }


        /// <summary>
        /// Human Classification Code :)
        /// Determine if the Connected component is Human or not.
        /// </summary>
        /// <param name="list">list of connected components to be classified</param>
        /// <returns>list of Humans</returns>
        public List<ComponentData> classify(List<ComponentData> list,int Threshold)
        {
            List<ComponentData> humans = new List<ComponentData>();
            for (int i = 0; i < list.Count; i++)
                if (Classify_Image(list[i].Silhouette, Threshold))
                    humans.Add(list[i]);
            return humans;
        }

        /// <summary>
        /// Loading the FeatureVectors of the trained Connected Components
        /// in public variable ( _trainVF )
        /// </summary>
        public void LoadFeatureVectors()
        {
            try
            {
                if (File.Exists(vectorFeatureStore))
                {
                    IFormatter formatter1 = new BinaryFormatter();
                    Stream stream1 = new FileStream(vectorFeatureStore, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _trainVF = (List<List<int>>)formatter1.Deserialize(stream1);
                    stream1.Close();
                }
                else
                {
                    MessageBox.Show("Feature vector data does not exist!");
                    Application.Exit();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error");
            }
        }
        /// <summary>
        /// Saving FeatureVectors of Training Connected Components
        /// </summary>
        public void SaveFeatureVectors()
        {
            Stream stream1;
            try
            {
                IFormatter formatter1 = new BinaryFormatter();
                if (File.Exists(vectorFeatureStore))
                stream1 = new FileStream(vectorFeatureStore, FileMode.Open, FileAccess.Write, FileShare.None);
                else
                    stream1 = new FileStream(vectorFeatureStore, FileMode.Create, FileAccess.Write, FileShare.None);

                formatter1.Serialize(stream1, _vectorsFeature);
                stream1.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Error In Saving File ...");
            }
        }

        /// <summary>
        /// Find Largest Contour 
        /// </summary>

        public VectorOfPoint FindLargestContour(IInputOutputArray cannyEdges, IInputOutputArray result)
        {
            int largest_contour_index = 0;
            double largest_area = 0;
            VectorOfPoint largestContour;

            using (Mat hierachy = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                IOutputArray hirarchy;

                CvInvoke.FindContours(cannyEdges, contours, hierachy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);// RetrType.Tree, ChainApproxMethod.ChainApproxNone);

                if (contours.Size > 0)
                {
                    for (int i = 0; i < contours.Size; i++)
                    {
                        MCvScalar color = new MCvScalar(0, 0, 255);

                        double a = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                        if (a > largest_area)
                        {
                            largest_area = a;
                            largest_contour_index = i;                //Store the index of largest contour
                        }

                        CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(255, 0, 0), 5);
                    }

                    CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(0, 0, 255), 1);//,LineType.EightConnected , hierachy);
                    largestContour = new VectorOfPoint(contours[largest_contour_index].ToArray());
                    return largestContour;
                }
            }
                return null;
           
           }

    }
}


