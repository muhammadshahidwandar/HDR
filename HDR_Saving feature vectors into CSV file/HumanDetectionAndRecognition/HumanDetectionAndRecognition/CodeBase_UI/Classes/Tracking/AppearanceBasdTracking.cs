using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.Util;
using System.Windows.Forms;
using Emgu.CV.Structure;


namespace HumanDetectionAndRecognition
{
    class AppearanceBasdTracking
   {
        #region Variables
        public List<ReferenceHumanImage> _reference = new List<ReferenceHumanImage>();
        public List<ReferenceHumanImage> _referenceBlackList = new List<ReferenceHumanImage>();
        int _blackRightArea = -1000;
        int _blackleftArea = -1000;
        int _labels = -1;
        #endregion
       public AppearanceBasdTracking()
        { }
        /*******************************************************************/
        /// <summary>
        /// Give each object in Image Special Label
        /// </summary>
        /// <param name="currentImage">current Frame YUV </param>
        /// <param name="mask">Object contain Object Silhouette and it's top, lower,right and left point </param>
        /// <returns> object label</returns>
        bool findOccultation = true;
        public int Track(Image<Ycc, Byte> currentImage, MaskData mask)
        {
            Image<Ycc, Byte> blob = CopyImage(currentImage, mask);
            Image<Gray, Byte> Newblob = getColorImage(blob);
            int[] histogram = getHistogram(Newblob, mask.Mask);
            int nHistogram = getN_Histogram(histogram);
            double comparisonResult = 0;
            double min = 160;
            int objectLabel = -1;
            int refIndex = -1;
            bool foundref = false;
            
            int center = getCentered(mask.left.Y, mask.Right.Y);

            if (_reference.Count == 0)
            {
                return AddNewObject(blob, Newblob, histogram, nHistogram, center);
            }
            else
            {
                for (int i = 0; i < _reference.Count; i++)
                {
                    comparisonResult = Math.Abs(center - _reference[i].TheCenter);
                        if (comparisonResult <= 10)
                        {
                            foundref = true;
                            if (min > comparisonResult)
                            {
                                _reference[i].TheCenter = center;
                                
                                objectLabel = _reference[i].ObjectLabel;
                                refIndex = i;
                            }
                        }
                }
                if (foundref)
                {
                    foundref = false;
                    _reference[refIndex].TheCenter = center;
                    if (_reference[refIndex].Direction == 1 && _reference[refIndex].TheCenter > 159)
                    {
                        _reference.RemoveAt(refIndex);
                    }
                    else
                        if (_reference[refIndex].Direction == 2 && _reference[refIndex].TheCenter < 1)
                        {
                            _reference.RemoveAt(refIndex);
                        }
                    return objectLabel;
                }
            }
            if (center > 25 && center < 145 && findOccultation)
            {
                //MessageBox.Show(center.ToString() + " " + mask.left.Y.ToString() + " " + mask.Right.Y.ToString());
                findOccultation = false;
                foreach (ReferenceHumanImage refHmn in _reference)
                {
                    if (mask.left.Y < refHmn.TheCenter && mask.Right.Y > refHmn.TheCenter)
                    {
                        //MessageBox.Show(refHmn.ObjectLabel.ToString() + "    " + refHmn.TheCenter.ToString());
                        if (refHmn.DirectionSetted && refHmn.Direction == 1)
                            refHmn.TheCenter = mask.Right.Y + 10;
                        else
                            if (refHmn.DirectionSetted && refHmn.Direction == 2)
                                refHmn.TheCenter = mask.left.Y - 10;
                        //MessageBox.Show(refHmn.ObjectLabel.ToString() + "    " + refHmn.TheCenter.ToString());
                        
                    }
                }
                return 255;
            }
            if (center > 25 && center < 145)
            {
                //MessageBox.Show(center.ToString());
                return 255;
            }
            return AddNewObject(blob, Newblob, histogram, nHistogram, center);
        }


        /// <summary>
        /// Convert YUV Image To gray Image Base on Y*.3+U*.35+V*.35
        /// </summary>
        /// <param name="inpImage">Current Frame YUV</param>
        /// <returns>gray Image</returns>
        Image<Gray, Byte> getColorImage(Image<Ycc, Byte> inpImage)
        {
            Image<Gray, Byte> newBlob = new Image<Gray, byte>(inpImage.Width, inpImage.Height);
            int indexe = 0;
            double temp = 0;
            unsafe
            {
                fixed (byte* ptrInpImage = inpImage.Data)
                {
                    fixed (byte* ptrNewBlob = newBlob.Data)
                    {
                        for (int i = 0; i < inpImage.Data.Length; i += 3)
                        {
                            temp = (*(ptrInpImage + i)) * .3;
                            temp += (*(ptrInpImage + i + 1)) * .35;
                            temp += (*(ptrInpImage + i + 2)) * .35;
                            temp = temp % 255;
                            *(ptrNewBlob + indexe) = System.Convert.ToByte(temp);
                            indexe++;
                            temp = 0;
                        }
                    }
                }
            }
            return newBlob;
        }

        /// <summary>
        /// Get histogram for special part of gray Image base on Mask 
        /// </summary>
        /// <param name="inpImage">current Frame Gray</param>
        /// <param name="mask">part of Image Silhouette</param>
        /// <returns>Array contain the histogram </returns>
        int[] getHistogram(Image<Gray, Byte> inpImage, Image<Gray, Byte> mask)
        {
            int[] histogramArr = new int[256];
            int temp = 0;
            unsafe
            {
                fixed (byte* ptrInpImage = inpImage.Data)
                {
                    fixed (byte* ptrMask = mask.Data)
                    {
                        fixed (int* ptrHistoArr = histogramArr)
                        {
                            for (int i = 0; i < inpImage.Data.Length; i++)
                            {
                                if (*(ptrMask + i) != 0)
                                {
                                    temp = *(ptrInpImage + i);
                                    *(ptrHistoArr + temp) = *(ptrHistoArr + temp) + 1;
                                }
                            }
                        }
                    }
                }
            }
            return histogramArr;
        }

        /// <summary>
        /// Sum number of pixels in image
        /// </summary>
        /// <param name="arr">Histogram of Image</param>
        /// <returns>Sum Pixel</returns>
        int getN_Histogram(int[] arr)
        {
            int sum = 0;
            unsafe
            {
                fixed (int* ptrArr = arr)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        sum = sum + *(ptrArr + i);
                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// Compare between current & ref histogram to find out the relation between of them
        /// </summary>
        /// <param name="CurrHisro">Current object histogram</param>
        /// <param name="RefHisto">ref object histogram</param>
        /// <param name="min">min number of pixel between ref  and current object</param>
        /// <returns>comparesion result </returns>
        double getComparisonRule(int[] CurrHisro, int[] RefHisto, int min)
        {
            double HistoSum = 0;
            unsafe
            {
                fixed (int* ptrCurrHisto = CurrHisro)
                {
                    fixed (int* ptrRefHisto = RefHisto)
                    {
                        for (int i = 0; i < CurrHisro.Length; i++)
                        {
                            HistoSum = HistoSum + Math.Min(*(ptrCurrHisto + i), *(ptrRefHisto + i));
                        }
                    }
                }
            }
            HistoSum = HistoSum / min;
            return HistoSum;
        }

        /// <summary>
        /// Copy part of image based on mask
        /// </summary>
        /// <param name="currentImage">Current frame YUV</param>
        /// <param name="mask">Object contain Object Silhouette and it's top, lower,right and left point </param>
        /// <returns>copied Image YUV</returns>
        static public Image<Ycc, Byte> CopyImage(Image<Ycc, Byte> currentImage, MaskData mask)
        {
            Image<Ycc, Byte> dest = new Image<Ycc, byte>(mask.Mask.Width, mask.Mask.Height, new Ycc(0, 0, 0));
            int sourceRow = mask.Top.X;
            int sourceCol = mask.left.Y * 3;
            int sourceTemp;
            int destCol = 0;
            int destTemp = 0;
            int maskTemp = 0;
            unsafe
            {

                fixed (byte* ptrcurrentImage = currentImage.Data)
                {
                    fixed (byte* ptrMask = mask.Mask.Data)
                    {
                        fixed (byte* ptrdest = dest.Data)
                        {
                            for (int row = 0; row < mask.Mask.Height; row++)
                            {
                                maskTemp = row * (mask.Mask.Data.Length / mask.Mask.Height);
                                sourceTemp = sourceRow * ((currentImage.Data.Length / 3) / currentImage.Height) * 3;
                                destTemp = row * ((dest.Data.Length / 3) / dest.Height) * 3;
                                for (int col = 0; col < mask.Mask.Data.Length / mask.Mask.Height; col++)
                                {
                                    if (*(ptrMask + maskTemp + col) != 0)
                                    {
                                        *(ptrdest + destTemp + destCol) = *(ptrcurrentImage + sourceTemp + sourceCol);
                                        *(ptrdest + destTemp + destCol + 1) = *(ptrcurrentImage + sourceTemp + sourceCol + 1);
                                        *(ptrdest + destTemp + destCol + 2) = *(ptrcurrentImage + sourceTemp + sourceCol + 2);
                                    }
                                    sourceCol += 3;
                                    destCol += 3;
                                }
                                destCol = 0;
                                sourceCol = mask.left.Y * 3;
                                sourceRow++;
                            }
                        }
                    }
                }
            }
            return dest;
        }

        /// <summary>
        /// Add new Ref object to refs
        /// </summary>
        /// <param name="refobject_Image">ref object Image</param>
        /// <param name="refobject_Image_Gray">ref object Image Gray </param>
        /// <param name="referenceHistogram">ref histogram </param>
        /// <param name="reference_NHistogram">sum of pixels in ref </param>
        /// <param name="Centered">centered point of the ref in X direction </param>
        /// <returns>ref Label </returns>
  public int AddNewObject(Image<Ycc, Byte> refobject_Image, Image<Gray, Byte> refobject_Image_Gray, int[] referenceHistogram, int reference_NHistogram, int Centered)
        {
            //MessageBox.Show("add new");
            _labels++;
            ReferenceHumanImage refHuman = new ReferenceHumanImage(refobject_Image.Clone(), _labels);
            refHuman.ReferenceImage_Gray = refobject_Image_Gray.Clone();
            refHuman.ReferenceHistogram = referenceHistogram;
            refHuman.Reference_NHistogram = reference_NHistogram;
            refHuman.TheCenter = Centered;
            if (refHuman.TheCenter >= 0 && refHuman.TheCenter < 20)
                refHuman.Direction = 1;
            else
                if (refHuman.TheCenter <= 160 && refHuman.TheCenter > 140)
                    refHuman.Direction = 2;
            refHuman.DirectionSetted = true;
            _reference.Add(refHuman);
            return _labels;
        }
        /*******************************************************************/
        int getCentered(double left, double right)
        {
            return System.Convert.ToInt32(((right - left) / 2) + left);
        }
        int getDirection(int refCenter, int currCenter)
        {
            if (refCenter - currCenter < -1)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
