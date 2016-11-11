using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetectionAndRecognition
{
    class ReferenceHumanImage
    {
        #region Variables
        Emgu.CV.Image<Ycc, Byte> _referenceImage;
        Image<Gray, Byte> _referenceImage_Gray;
        int _objectLabel = 0;
        int[] _referenceHistogram = new int[256];
        int _reference_NHistogram = 0;
        int _center = 0;
        int _direction = -1;
        public bool Deleted = false;
        public bool DirectionSetted = false;
        #endregion
        public ReferenceHumanImage()
        { }
        public ReferenceHumanImage(Image<Ycc, Byte> referenceImage, int lable)
        {
            _referenceImage = referenceImage;
            _objectLabel = lable;
        }
        public Image<Ycc, Byte> ReferenceImage
        {
            get { return _referenceImage; }
            set { _referenceImage = value; }
        }
        public Image<Gray, Byte> ReferenceImage_Gray
        {
            get { return _referenceImage_Gray; }
            set { _referenceImage_Gray = value; }
        }
        public int ObjectLabel
        {
            get { return _objectLabel; }
            set { _objectLabel = value; }
        }
        public int[] ReferenceHistogram
        {
            get { return _referenceHistogram; }
            set { _referenceHistogram = value; }
        }
        public int Reference_NHistogram
        {
            get { return _reference_NHistogram; }
            set { _reference_NHistogram = value; }
        }
        public int TheCenter
        {
            get { return _center; }
            set { _center = value; }
        }
        public int Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

    }
}
